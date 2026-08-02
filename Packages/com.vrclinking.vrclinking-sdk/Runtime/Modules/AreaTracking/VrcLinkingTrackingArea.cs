
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class VrcLinkingTrackingArea : UdonSharpBehaviour
{
    public string areaName = "Area";

    [HideInInspector] public string areaId;
    [HideInInspector] public BoxCollider trackingCollider;
    [HideInInspector] public VrcLinkingAreaTracker tracker;
    [HideInInspector] public int runtimeIndex = -1;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal || tracker == null)
            return;

        tracker.NotifyAreaEntered(runtimeIndex);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal || tracker == null)
            return;

        tracker.NotifyAreaExited(runtimeIndex);
    }

    public bool ContainsWorldPosition(Vector3 worldPosition)
    {
        if (trackingCollider == null || !trackingCollider.enabled || !gameObject.activeInHierarchy)
            return false;

        Vector3 localPosition =
            trackingCollider.transform.InverseTransformPoint(worldPosition) - trackingCollider.center;
        Vector3 halfSize = trackingCollider.size * 0.5f;
        const float epsilon = 0.0001f;

        return Mathf.Abs(localPosition.x) <= halfSize.x + epsilon &&
               Mathf.Abs(localPosition.y) <= halfSize.y + epsilon &&
               Mathf.Abs(localPosition.z) <= halfSize.z + epsilon;
    }
}
