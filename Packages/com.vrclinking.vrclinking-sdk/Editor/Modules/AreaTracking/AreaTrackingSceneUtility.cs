using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.Udon;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal sealed class AreaTrackingEntry
    {
        internal VrcLinkingTrackingArea Area { get; }
        internal BoxCollider Collider { get; }
        internal bool IsIncluded { get; }
        internal string ExclusionReason { get; }
        internal Guid ParsedId { get; }
        internal string NormalizedName { get; }

        internal AreaTrackingEntry(VrcLinkingTrackingArea area, BoxCollider collider,
            bool isIncluded, string exclusionReason, Guid parsedId, string normalizedName)
        {
            Area = area;
            Collider = collider;
            IsIncluded = isIncluded;
            ExclusionReason = exclusionReason;
            ParsedId = parsedId;
            NormalizedName = normalizedName;
        }
    }

    internal sealed class AreaTrackingSceneSnapshot
    {
        internal IReadOnlyList<VrcLinkingAreaTracker> Trackers { get; }
        internal IReadOnlyList<VrcLinkingDownloader> Downloaders { get; }
        internal IReadOnlyList<AreaTrackingEntry> Areas { get; }
        internal IReadOnlyList<AreaTrackingEntry> IncludedAreas { get; }

        internal AreaTrackingSceneSnapshot(IReadOnlyList<VrcLinkingAreaTracker> trackers,
            IReadOnlyList<VrcLinkingDownloader> downloaders,
            IReadOnlyList<AreaTrackingEntry> areas, IReadOnlyList<AreaTrackingEntry> includedAreas)
        {
            Trackers = trackers;
            Downloaders = downloaders;
            Areas = areas;
            IncludedAreas = includedAreas;
        }
    }

    internal static class AreaTrackingSceneUtility
    {
        internal static AreaTrackingSceneSnapshot Capture()
        {
            List<VrcLinkingAreaTracker> trackers = new List<VrcLinkingAreaTracker>();
            List<VrcLinkingDownloader> downloaders = new List<VrcLinkingDownloader>();
            List<AreaTrackingEntry> areas = new List<AreaTrackingEntry>();

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                return new AreaTrackingSceneSnapshot(
                    trackers, downloaders, areas, new List<AreaTrackingEntry>());

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                trackers.AddRange(root.GetComponentsInChildren<VrcLinkingAreaTracker>(true));
                downloaders.AddRange(root.GetComponentsInChildren<VrcLinkingDownloader>(true));

                foreach (VrcLinkingTrackingArea area in root.GetComponentsInChildren<VrcLinkingTrackingArea>(true))
                {
                    BoxCollider collider = area.GetComponent<BoxCollider>();
                    Guid.TryParse(area.areaId, out Guid parsedId);
                    string normalizedName = (area.areaName ?? string.Empty).Trim();
                    bool isIncluded = TryGetInclusion(area, collider, out string exclusionReason);

                    areas.Add(new AreaTrackingEntry(area, collider, isIncluded, exclusionReason,
                        parsedId, normalizedName));
                }
            }

            List<AreaTrackingEntry> included = areas.Where(entry => entry.IsIncluded).ToList();
            return new AreaTrackingSceneSnapshot(trackers, downloaders, areas, included);
        }

        internal static VrcLinkingDownloader GetSingleDownloader(AreaTrackingSceneSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            return snapshot.Downloaders.Count == 1 ? snapshot.Downloaders[0] : null;
        }

        internal static bool RepairAreaIds(AreaTrackingSceneSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            bool repairedAny = false;
            HashSet<Guid> usedIds = new HashSet<Guid>();

            foreach (AreaTrackingEntry entry in snapshot.Areas)
            {
                VrcLinkingTrackingArea area = entry.Area;
                bool isValid = Guid.TryParse(area.areaId, out Guid id) && id != Guid.Empty;
                if (isValid && usedIds.Add(id))
                    continue;

                do
                {
                    id = Guid.NewGuid();
                } while (!usedIds.Add(id));

                Undo.RecordObject(area, "Repair Area Tracking ID");
                area.areaId = id.ToString("D");
                EditorUtility.SetDirty(area);
                MarkSceneDirty(area.gameObject.scene);
                repairedAny = true;
            }

            return repairedAny;
        }

        internal static List<AreaTrackingEntry> GetIncludedAreasInDisplayOrder(
            AreaTrackingSceneSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            return snapshot.IncludedAreas
                .OrderBy(entry => entry.NormalizedName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.ParsedId)
                .ToList();
        }

        internal static List<SyncWorldTrackingAreaRequest> BuildSyncAreas(
            AreaTrackingSceneSnapshot snapshot)
        {
            return GetIncludedAreasInDisplayOrder(snapshot)
                .Select((entry, index) => new SyncWorldTrackingAreaRequest
                {
                    Id = entry.ParsedId,
                    Name = entry.NormalizedName,
                    DisplayOrder = index
                })
                .ToList();
        }

        internal static SyncWorldTrackingAreasRequest BuildSyncRequest(
            AreaTrackingSceneSnapshot snapshot, bool trackingEnabled)
        {
            return new SyncWorldTrackingAreasRequest
            {
                TrackingEnabled = trackingEnabled,
                Areas = BuildSyncAreas(snapshot)
            };
        }

        internal static string ComputeFingerprint(Guid worldId, bool trackingEnabled,
            AreaTrackingSceneSnapshot snapshot)
        {
            return ComputeFingerprint(worldId, trackingEnabled, BuildSyncAreas(snapshot));
        }

        internal static string ComputeFingerprint(Guid worldId, bool trackingEnabled,
            IReadOnlyList<SyncWorldTrackingAreaRequest> areas)
        {
            if (areas == null)
                throw new ArgumentNullException(nameof(areas));

            StringBuilder canonical = new StringBuilder();
            canonical.Append(worldId.ToString("D"));
            canonical.Append('\n');
            canonical.Append(trackingEnabled ? "true" : "false");

            foreach (SyncWorldTrackingAreaRequest area in areas)
            {
                canonical.Append('\n');
                canonical.Append(area.DisplayOrder);
                canonical.Append('|');
                canonical.Append(area.Id.ToString("D"));
                canonical.Append('|');
                canonical.Append((area.Name ?? string.Empty).Trim());
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
                StringBuilder hexadecimal = new StringBuilder(hash.Length * 2);
                foreach (byte value in hash)
                    hexadecimal.Append(value.ToString("x2"));
                return hexadecimal.ToString();
            }
        }

        internal static VrcLinkingTrackingArea CreateArea(VrcLinkingAreaTracker tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException(nameof(tracker));

            string areaName = GetUniqueAreaName("Area", numberedBaseName: true, excludedArea: null);
            GameObject gameObject = new GameObject(areaName);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Tracking Area");
            Undo.SetTransformParent(gameObject.transform, tracker.transform, "Parent Tracking Area");
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            VrcLinkingTrackingArea area = UdonSharpUndo.AddComponent<VrcLinkingTrackingArea>(gameObject);
            BoxCollider collider = Undo.AddComponent<BoxCollider>(gameObject);
            area.areaName = areaName;
            area.areaId = Guid.NewGuid().ToString("D");
            collider.isTrigger = true;

            EditorUtility.SetDirty(area);
            EditorUtility.SetDirty(collider);
            MarkSceneDirty(gameObject.scene);
            SelectAndFrame(gameObject);
            return area;
        }

        internal static VrcLinkingTrackingArea DuplicateArea(VrcLinkingTrackingArea source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            GameObject duplicateObject = UnityEngine.Object.Instantiate(
                source.gameObject, source.transform.parent, false);
            Undo.RegisterCreatedObjectUndo(duplicateObject, "Duplicate Tracking Area");

            VrcLinkingTrackingArea duplicate = duplicateObject.GetComponent<VrcLinkingTrackingArea>();
            if (duplicate == null)
            {
                Undo.DestroyObjectImmediate(duplicateObject);
                throw new InvalidOperationException("Duplicating the object did not preserve its tracking area behaviour.");
            }

            string sourceName = string.IsNullOrWhiteSpace(source.areaName) ? "Area" : source.areaName.Trim();
            string copyName = GetUniqueAreaName(sourceName + " Copy", numberedBaseName: false, source);
            duplicate.areaName = copyName;
            duplicate.areaId = Guid.NewGuid().ToString("D");
            duplicateObject.name = copyName;

            EditorUtility.SetDirty(duplicate);
            MarkSceneDirty(duplicateObject.scene);
            SelectAndFrame(duplicateObject);
            return duplicate;
        }

        internal static bool RequiresRemovalConfirmation(VrcLinkingTrackingArea area)
        {
            if (area == null)
                return false;

            if (area.transform.childCount != 0)
                return true;

            UdonBehaviour backing = UdonSharpEditorUtility.GetBackingUdonBehaviour(area);
            foreach (Component component in area.GetComponents<Component>())
            {
                if (component is Transform || component == area || component == backing ||
                    component is BoxCollider)
                    continue;

                return true;
            }

            return false;
        }

        internal static void RemoveArea(VrcLinkingTrackingArea area)
        {
            if (area == null)
                return;

            GameObject gameObject = area.gameObject;
            UdonSharpUndo.DestroyImmediate(area);
            Undo.DestroyObjectImmediate(gameObject);
        }

        internal static bool IsOtherwiseActive(VrcLinkingTrackingArea area)
        {
            if (area == null || !area.gameObject.activeInHierarchy)
                return false;

            UdonBehaviour backing = UdonSharpEditorUtility.GetBackingUdonBehaviour(area);
            return backing != null && backing.enabled;
        }

        static bool TryGetInclusion(VrcLinkingTrackingArea area, BoxCollider collider,
            out string exclusionReason)
        {
            if (!area.gameObject.activeInHierarchy)
            {
                exclusionReason = "GameObject is inactive.";
                return false;
            }

            UdonBehaviour backing = UdonSharpEditorUtility.GetBackingUdonBehaviour(area);
            if (backing == null)
            {
                exclusionReason = "Backing UdonBehaviour is missing.";
                return false;
            }

            if (!backing.enabled)
            {
                exclusionReason = "Backing UdonBehaviour is disabled.";
                return false;
            }

            if (collider == null)
            {
                exclusionReason = "BoxCollider is missing.";
                return false;
            }

            if (!collider.enabled)
            {
                exclusionReason = "BoxCollider is disabled.";
                return false;
            }

            exclusionReason = null;
            return true;
        }

        static string GetUniqueAreaName(string baseName, bool numberedBaseName,
            VrcLinkingTrackingArea excludedArea)
        {
            HashSet<string> names = new HashSet<string>(
                Capture().Areas
                    .Where(entry => entry.Area != excludedArea)
                    .Select(entry => entry.NormalizedName),
                StringComparer.OrdinalIgnoreCase);

            if (numberedBaseName)
            {
                for (int index = 1;; index++)
                {
                    string candidate = $"{baseName} {index}";
                    if (!names.Contains(candidate))
                        return candidate;
                }
            }

            if (!names.Contains(baseName))
                return baseName;

            for (int index = 2;; index++)
            {
                string candidate = $"{baseName} {index}";
                if (!names.Contains(candidate))
                    return candidate;
            }
        }

        static void SelectAndFrame(GameObject gameObject)
        {
            Selection.activeGameObject = gameObject;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }

        static void MarkSceneDirty(Scene scene)
        {
            if (scene.IsValid() && scene.isLoaded)
                EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    [InitializeOnLoad]
    internal static class AreaTrackingIdRepairWatcher
    {
        static bool _repairQueued;

        static AreaTrackingIdRepairWatcher()
        {
            EditorApplication.hierarchyChanged += QueueRepair;
            EditorSceneManager.sceneOpened += (_, _) => QueueRepair();
            EditorSceneManager.activeSceneChangedInEditMode += (_, _) => QueueRepair();
            QueueRepair();
        }

        static void QueueRepair()
        {
            if (_repairQueued || BuildPipeline.isBuildingPlayer)
                return;

            _repairQueued = true;
            EditorApplication.delayCall += RepairNow;
        }

        static void RepairNow()
        {
            _repairQueued = false;
            if (BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                QueueRepair();
                return;
            }

            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            if (snapshot.Areas.Count > 0)
                AreaTrackingSceneUtility.RepairAreaIds(snapshot);
        }
    }
}
