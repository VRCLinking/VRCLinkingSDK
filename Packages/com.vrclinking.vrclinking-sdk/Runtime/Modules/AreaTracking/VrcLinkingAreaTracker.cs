
using UdonSharp;
using UnityEngine;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class VrcLinkingAreaTracker : UdonSharpBehaviour
{
    public bool trackingEnabled = true;
    [Range(30f, 120f)] public float heartbeatIntervalSeconds = 60f;
    [Range(5f, 120f)] public float startupDelaySeconds = 10f;

    [HideInInspector] public string syncedWorldId;
    [HideInInspector] public string syncedConfigurationHash;
    [HideInInspector] public VrcLinkingTrackingArea[] trackingAreas;
    [HideInInspector] public VRCUrl[] areaHeartbeatUrls;
    [HideInInspector] public VRCUrl outsideHeartbeatUrl;

    int _currentAreaIndex = -1;
    UdonSharpBehaviour[] _areaChangeListeners = new UdonSharpBehaviour[0];

    // Local player's index into trackingAreas; -1 means outside or not yet known.
    public int CurrentAreaIndex { get { return _currentAreaIndex; } }

    // Listeners implement public void OnVrcLinkingAreaChanged() and read CurrentAreaIndex.
    // Registration does not replay the current state; read it after registering if needed.
    public void RegisterAreaChangeListener(UdonSharpBehaviour listener)
    {
        if (listener == null) return;
        for (int i = 0; i < _areaChangeListeners.Length; i++)
            if (_areaChangeListeners[i] == listener) return;

        var listeners = new UdonSharpBehaviour[_areaChangeListeners.Length + 1];
        for (int i = 0; i < _areaChangeListeners.Length; i++)
            listeners[i] = _areaChangeListeners[i];
        listeners[listeners.Length - 1] = listener;
        _areaChangeListeners = listeners;
    }

    public void UnregisterAreaChangeListener(UdonSharpBehaviour listener)
    {
        int remaining = 0;
        for (int i = 0; i < _areaChangeListeners.Length; i++)
            if (_areaChangeListeners[i] != null && _areaChangeListeners[i] != listener) remaining++;
        if (remaining == _areaChangeListeners.Length) return;

        var listeners = new UdonSharpBehaviour[remaining];
        int index = 0;
        for (int i = 0; i < _areaChangeListeners.Length; i++)
            if (_areaChangeListeners[i] != null && _areaChangeListeners[i] != listener)
                listeners[index++] = _areaChangeListeners[i];
        _areaChangeListeners = listeners;
    }

    void SetCurrentAreaIndex(int index)
    {
        if (_currentAreaIndex == index) return;
        _currentAreaIndex = index;

        // Copy-on-write registration keeps this notification's recipient list stable.
        var listeners = _areaChangeListeners;
        for (int i = 0; i < listeners.Length; i++)
        {
            if (listeners[i] != null)
                listeners[i].SendCustomEvent("OnVrcLinkingAreaChanged");
        }
    }

    void Start()
    {
        heartbeatIntervalSeconds = Mathf.Clamp(heartbeatIntervalSeconds, 30f, 120f);
        startupDelaySeconds = Mathf.Clamp(startupDelaySeconds, 5f, 120f);

        if (trackingEnabled)
            BeginTracking();
    }

    public void BeginTracking()
    {
        if (!trackingEnabled)
            return;

        if (Networking.LocalPlayer == null)
        {
            SendCustomEventDelayedSeconds(nameof(BeginTracking), 1f);
            return;
        }

        SendCustomEventDelayedSeconds(nameof(SendHeartbeat), startupDelaySeconds);
    }

    public void NotifyAreaEntered(int runtimeIndex)
    {
        if (runtimeIndex >= 0 && trackingAreas != null && runtimeIndex < trackingAreas.Length)
            SetCurrentAreaIndex(runtimeIndex);
    }

    public void NotifyAreaExited(int runtimeIndex)
    {
        if (_currentAreaIndex == runtimeIndex)
            SetCurrentAreaIndex(-1);
    }

    public void SendHeartbeat()
    {
        if (!trackingEnabled)
            return;

        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (localPlayer == null)
        {
            SendCustomEventDelayedSeconds(nameof(SendHeartbeat), 1f);
            return;
        }

        ReconcileCurrentArea(localPlayer.GetPosition());

        // Schedule before starting the request so a failed or delayed response cannot stop the loop.
        SendCustomEventDelayedSeconds(nameof(SendHeartbeat), heartbeatIntervalSeconds);

        VRCUrl heartbeatUrl = _currentAreaIndex >= 0 && areaHeartbeatUrls != null &&
                              _currentAreaIndex < areaHeartbeatUrls.Length
            ? areaHeartbeatUrls[_currentAreaIndex]
            : outsideHeartbeatUrl;

        VRCStringDownloader.LoadUrl(heartbeatUrl, (IUdonEventReceiver)this);
    }

    void ReconcileCurrentArea(Vector3 position)
    {
        int nextAreaIndex = -1;
        if (trackingAreas != null)
        {
            for (int i = 0; i < trackingAreas.Length; i++)
            {
                VrcLinkingTrackingArea area = trackingAreas[i];
                if (area != null && area.ContainsWorldPosition(position))
                {
                    nextAreaIndex = i;
                    break;
                }
            }
        }
        SetCurrentAreaIndex(nextAreaIndex);
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogWarning($"[VrcLinkingAreaTracker] Heartbeat failed: {result.Error}");
    }
}
