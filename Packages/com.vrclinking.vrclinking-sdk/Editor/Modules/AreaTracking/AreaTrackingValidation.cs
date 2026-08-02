using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEngine;
using VRC.Udon;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal enum AreaTrackingIssueSeverity
    {
        Info,
        Warning,
        Error
    }

    internal sealed class AreaTrackingIssue
    {
        internal AreaTrackingIssueSeverity Severity { get; }
        internal string Code { get; }
        internal string Message { get; }
        internal UnityEngine.Object PrimaryObject { get; }
        internal UnityEngine.Object SecondaryObject { get; }

        internal AreaTrackingIssue(AreaTrackingIssueSeverity severity, string code, string message,
            UnityEngine.Object primaryObject, UnityEngine.Object secondaryObject = null)
        {
            Severity = severity;
            Code = code;
            Message = message;
            PrimaryObject = primaryObject;
            SecondaryObject = secondaryObject;
        }
    }

    internal sealed class AreaTrackingValidationResult
    {
        public List<AreaTrackingIssue> Issues { get; } = new List<AreaTrackingIssue>();
        public bool HasErrors => Issues.Any(issue => issue.Severity == AreaTrackingIssueSeverity.Error);
    }

    internal static class AreaTrackingValidation
    {
        internal static AreaTrackingValidationResult Validate(AreaTrackingSceneSnapshot snapshot,
            VrcLinkingDownloader downloader, string expectedFingerprint, bool validateSyncState = true)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            AreaTrackingValidationResult result = new AreaTrackingValidationResult();
            ValidateTrackerCount(snapshot, result);
            ValidateTrackerState(snapshot, result);
            ValidateAreaCount(snapshot, result);
            ValidateColliders(snapshot, result);
            ValidateIds(snapshot, result);
            ValidateNames(snapshot, result);
            ValidateDownloaderAndSync(snapshot, downloader, expectedFingerprint, validateSyncState, result);
            ValidateOverlaps(snapshot, result);
            return result;
        }

        static void ValidateTrackerState(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            if (snapshot.Trackers.Count != 1)
                return;

            VrcLinkingAreaTracker tracker = snapshot.Trackers[0];
            if (!tracker.trackingEnabled)
                return;

            UdonBehaviour backing = UdonSharpEditorUtility.GetBackingUdonBehaviour(tracker);
            if (!tracker.gameObject.activeInHierarchy || backing == null || !backing.enabled)
            {
                AddError(result, "TRACKER_INACTIVE",
                    "Area tracking is enabled, but the area tracker GameObject or its backing UdonBehaviour is disabled.",
                    tracker);
            }
        }

        internal static AreaTrackingValidationResult ValidateForSync(
            AreaTrackingSceneSnapshot snapshot, VrcLinkingDownloader downloader)
        {
            return Validate(snapshot, downloader, null, validateSyncState: false);
        }

        static void ValidateTrackerCount(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            if (snapshot.Areas.Count > 0 && snapshot.Trackers.Count == 0)
            {
                AddError(result, "TRACKER_MISSING",
                    "Tracking areas are present, but the active scene has no area tracker.",
                    snapshot.Areas[0].Area);
            }

            if (snapshot.Trackers.Count <= 1)
                return;

            VrcLinkingAreaTracker first = snapshot.Trackers[0];
            for (int index = 1; index < snapshot.Trackers.Count; index++)
            {
                AddError(result, "TRACKER_MULTIPLE",
                    "The active scene may contain only one area tracker.",
                    first, snapshot.Trackers[index]);
            }
        }

        static void ValidateAreaCount(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            if (snapshot.IncludedAreas.Count <= 64)
                return;

            UnityEngine.Object context = snapshot.IncludedAreas[64].Area;
            AddError(result, "AREA_LIMIT_EXCEEDED",
                $"The scene includes {snapshot.IncludedAreas.Count} tracking areas; the maximum is 64.",
                context);
        }

        static void ValidateColliders(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            foreach (AreaTrackingEntry entry in snapshot.Areas)
            {
                if (!AreaTrackingSceneUtility.IsOtherwiseActive(entry.Area))
                    continue;

                Collider[] colliders = entry.Area.GetComponents<Collider>();
                if (entry.Collider == null)
                {
                    AddError(result, "AREA_COLLIDER_MISSING",
                        $"Tracking area '{DisplayName(entry)}' needs a BoxCollider on the same GameObject.",
                        entry.Area);
                }

                if (colliders.Length > 1 || colliders.Any(collider => !(collider is BoxCollider)))
                {
                    AddError(result, "AREA_COLLIDER_AMBIGUOUS",
                        $"Tracking area '{DisplayName(entry)}' must have exactly one collider, and it must be a BoxCollider.",
                        entry.Area);
                }

                if (!entry.IsIncluded)
                    continue;

                if (!entry.Collider.isTrigger)
                {
                    AddError(result, "AREA_COLLIDER_NOT_TRIGGER",
                        $"Tracking area '{DisplayName(entry)}' must use a trigger BoxCollider.",
                        entry.Collider);
                }

                Vector3 size = entry.Collider.size;
                if (size.x <= 0f || size.y <= 0f || size.z <= 0f)
                {
                    AddError(result, "AREA_COLLIDER_INVALID_SIZE",
                        $"Tracking area '{DisplayName(entry)}' has a zero or negative BoxCollider size axis.",
                        entry.Collider);
                }
            }
        }

        static void ValidateIds(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            Dictionary<Guid, AreaTrackingEntry> firstById = new Dictionary<Guid, AreaTrackingEntry>();
            foreach (AreaTrackingEntry entry in snapshot.Areas)
            {
                if (!Guid.TryParse(entry.Area.areaId, out Guid id) || id == Guid.Empty)
                {
                    AddError(result, "AREA_ID_INVALID",
                        $"Tracking area '{DisplayName(entry)}' has an empty or invalid stable ID.",
                        entry.Area);
                    continue;
                }

                if (firstById.TryGetValue(id, out AreaTrackingEntry first))
                {
                    AddError(result, "AREA_ID_DUPLICATE",
                        $"Tracking areas '{DisplayName(first)}' and '{DisplayName(entry)}' share the same stable ID.",
                        first.Area, entry.Area);
                    continue;
                }

                firstById.Add(id, entry);
            }
        }

        static void ValidateNames(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            Dictionary<string, AreaTrackingEntry> firstByName =
                new Dictionary<string, AreaTrackingEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (AreaTrackingEntry entry in snapshot.IncludedAreas)
            {
                if (entry.NormalizedName.Length == 0)
                {
                    AddError(result, "AREA_NAME_EMPTY", "An included tracking area has an empty name.",
                        entry.Area);
                    continue;
                }

                if (entry.NormalizedName.Length > 100)
                {
                    AddError(result, "AREA_NAME_TOO_LONG",
                        $"Tracking area '{DisplayName(entry)}' exceeds the 100-character name limit.",
                        entry.Area);
                }

                if (firstByName.TryGetValue(entry.NormalizedName, out AreaTrackingEntry first))
                {
                    AddError(result, "AREA_NAME_DUPLICATE",
                        $"Included tracking areas '{DisplayName(first)}' and '{DisplayName(entry)}' have the same name.",
                        first.Area, entry.Area);
                    continue;
                }

                firstByName.Add(entry.NormalizedName, entry);
            }
        }

        static void ValidateDownloaderAndSync(AreaTrackingSceneSnapshot snapshot,
            VrcLinkingDownloader downloader, string expectedFingerprint, bool validateSyncState,
            AreaTrackingValidationResult result)
        {
            if (snapshot.Trackers.Count == 0)
                return;

            VrcLinkingAreaTracker tracker = snapshot.Trackers[0];
            if (snapshot.Downloaders.Count > 1)
            {
                AddError(result, "DOWNLOADER_MULTIPLE",
                    "The active scene may contain only one VRC Linking downloader when area tracking is configured.",
                    snapshot.Downloaders[0], snapshot.Downloaders[1]);
                return;
            }

            if (downloader == null)
            {
                AddError(result, "DOWNLOADER_MISSING",
                    "Area tracking requires a VRC Linking downloader in the active scene.", tracker);
                return;
            }

            if (string.IsNullOrWhiteSpace(downloader.serverId))
            {
                AddError(result, "DOWNLOADER_SERVER_MISSING",
                    "Select a server on the VRC Linking downloader before syncing area tracking.", downloader);
            }

            if (downloader.worldId == Guid.Empty)
            {
                AddError(result, "DOWNLOADER_WORLD_MISSING",
                    "Select a world on the VRC Linking downloader before syncing area tracking.", downloader);
            }

            if (!validateSyncState)
                return;

            if (!Guid.TryParse(tracker.syncedWorldId, out Guid syncedWorldId) ||
                syncedWorldId == Guid.Empty || syncedWorldId != downloader.worldId)
            {
                AddError(result, "TRACKING_SYNC_WORLD_MISMATCH",
                    "The area configuration has not been synced for the downloader's selected world.",
                    tracker, downloader);
            }

            if (!string.Equals(tracker.syncedConfigurationHash, expectedFingerprint,
                    StringComparison.Ordinal))
            {
                AddError(result, "TRACKING_SYNC_FINGERPRINT_MISMATCH",
                    "The local area configuration differs from the last successful sync.", tracker);
            }
        }

        static void ValidateOverlaps(AreaTrackingSceneSnapshot snapshot,
            AreaTrackingValidationResult result)
        {
            Physics.SyncTransforms();

            for (int firstIndex = 0; firstIndex < snapshot.IncludedAreas.Count; firstIndex++)
            {
                AreaTrackingEntry first = snapshot.IncludedAreas[firstIndex];
                for (int secondIndex = firstIndex + 1;
                     secondIndex < snapshot.IncludedAreas.Count;
                     secondIndex++)
                {
                    AreaTrackingEntry second = snapshot.IncludedAreas[secondIndex];
                    if (!first.Collider.bounds.Intersects(second.Collider.bounds))
                        continue;

                    bool overlaps = Physics.ComputePenetration(
                        first.Collider, first.Collider.transform.position, first.Collider.transform.rotation,
                        second.Collider, second.Collider.transform.position, second.Collider.transform.rotation,
                        out _, out float distance) && distance > 0.0001f;

                    if (!overlaps)
                        continue;

                    AddError(result, "AREA_OVERLAP",
                        $"Tracking areas '{DisplayName(first)}' and '{DisplayName(second)}' overlap.",
                        first.Area, second.Area);
                }
            }
        }

        static string DisplayName(AreaTrackingEntry entry)
        {
            return entry.NormalizedName.Length == 0 ? entry.Area.gameObject.name : entry.NormalizedName;
        }

        static void AddError(AreaTrackingValidationResult result, string code, string message,
            UnityEngine.Object primaryObject, UnityEngine.Object secondaryObject = null)
        {
            result.Issues.Add(new AreaTrackingIssue(AreaTrackingIssueSeverity.Error, code, message,
                primaryObject, secondaryObject));
        }
    }
}
