
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
            _currentAreaIndex = runtimeIndex;
    }

    public void NotifyAreaExited(int runtimeIndex)
    {
        if (_currentAreaIndex == runtimeIndex)
            _currentAreaIndex = -1;
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
        _currentAreaIndex = -1;

        if (trackingAreas == null)
            return;

        for (int i = 0; i < trackingAreas.Length; i++)
        {
            VrcLinkingTrackingArea area = trackingAreas[i];
            if (area != null && area.ContainsWorldPosition(position))
            {
                _currentAreaIndex = i;
                return;
            }
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.LogWarning($"[VrcLinkingAreaTracker] Heartbeat failed: {result.Error}");
    }
}
