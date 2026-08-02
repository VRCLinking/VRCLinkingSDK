using System;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using VRC.SDKBase;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal static class AreaTrackingBuildHandler
    {
        const string TelemetryBaseUrl = "https://data.vrclinking.com/telemetry/v1/worlds/";

        [PostProcessScene(-20)]
        public static void OnBuild()
        {
            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            if (snapshot.Trackers.Count == 0 && snapshot.Areas.Count == 0)
                return;

            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
            VrcLinkingAreaTracker tracker = snapshot.Trackers.Count == 1 ? snapshot.Trackers[0] : null;
            string fingerprint = tracker != null && downloader != null && downloader.worldId != Guid.Empty
                ? AreaTrackingSceneUtility.ComputeFingerprint(
                    downloader.worldId, tracker.trackingEnabled, snapshot)
                : string.Empty;
            AreaTrackingValidationResult validation = AreaTrackingValidation.Validate(
                snapshot, downloader, fingerprint);

            foreach (AreaTrackingIssue issue in validation.Issues)
            {
                if (issue.Severity == AreaTrackingIssueSeverity.Error)
                    Debug.LogError($"[VRC Linking Area Tracking] {issue.Message}", issue.PrimaryObject);
                else if (issue.Severity == AreaTrackingIssueSeverity.Warning)
                    Debug.LogWarning($"[VRC Linking Area Tracking] {issue.Message}", issue.PrimaryObject);
                else
                    Debug.Log($"[VRC Linking Area Tracking] {issue.Message}", issue.PrimaryObject);
            }

            if (validation.HasErrors)
            {
                int errorCount = validation.Issues.Count(issue =>
                    issue.Severity == AreaTrackingIssueSeverity.Error);
                throw new BuildFailedException(
                    $"VRC Linking Area Tracking has {errorCount} build blocker" +
                    $"{(errorCount == 1 ? string.Empty : "s")}. Select the Area Tracker and open Validation.");
            }

            SerializeRuntimeConfiguration(tracker, downloader, snapshot);
        }

        static void SerializeRuntimeConfiguration(VrcLinkingAreaTracker tracker,
            VrcLinkingDownloader downloader, AreaTrackingSceneSnapshot snapshot)
        {
            var included = AreaTrackingSceneUtility.GetIncludedAreasInDisplayOrder(snapshot);
            string worldId = downloader.worldId.ToString("D");

            tracker.trackingAreas = included.Select(entry => entry.Area).ToArray();
            tracker.areaHeartbeatUrls = included
                .Select(entry => new VRCUrl(
                    $"{TelemetryBaseUrl}{worldId}/areas/{entry.ParsedId:D}"))
                .ToArray();
            tracker.outsideHeartbeatUrl = new VRCUrl($"{TelemetryBaseUrl}{worldId}/outside");

            for (int index = 0; index < included.Count; index++)
            {
                AreaTrackingEntry entry = included[index];
                entry.Area.tracker = tracker;
                entry.Area.runtimeIndex = index;
                entry.Area.trackingCollider = entry.Collider;
            }
        }
    }
}
