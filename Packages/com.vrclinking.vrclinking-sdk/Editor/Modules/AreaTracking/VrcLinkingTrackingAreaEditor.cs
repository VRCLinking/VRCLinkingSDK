using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    [CustomEditor(typeof(VrcLinkingTrackingArea))]
    public sealed class VrcLinkingTrackingAreaEditor : UnityEditor.Editor
    {
        VrcLinkingTrackingArea Area => (VrcLinkingTrackingArea)target;

        public override VisualElement CreateInspectorGUI()
        {
            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            AreaTrackingSceneUtility.RepairAreaIds(snapshot);
            snapshot = AreaTrackingSceneUtility.Capture();

            VisualElement root = new VisualElement();
            StyleSheet styles = Resources.Load<StyleSheet>("AreaTrackingStyles");
            if (styles != null)
                root.styleSheets.Add(styles);
            root.AddToClassList("tracking-panel");

            Label title = new Label("Tracking Area");
            title.AddToClassList("tracking-section-title");
            root.Add(title);

            TextField nameField = new TextField("Area Name");
            nameField.SetValueWithoutNotify(Area.areaName);
            nameField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(Area, "Rename Tracking Area");
                Area.areaName = evt.newValue;
                EditorUtility.SetDirty(Area);
                EditorSceneManager.MarkSceneDirty(Area.gameObject.scene);
            });
            root.Add(nameField);

            AreaTrackingEntry entry = snapshot.Areas.FirstOrDefault(candidate => candidate.Area == Area);
            string inclusion = entry == null
                ? "Area is not in the active scene."
                : entry.IsIncluded
                    ? "Included in tracking"
                    : $"Excluded: {entry.ExclusionReason}";
            HelpBox inclusionBox = new HelpBox(inclusion,
                entry != null && entry.IsIncluded ? HelpBoxMessageType.Info : HelpBoxMessageType.Warning);
            root.Add(inclusionBox);

            BoxCollider collider = Area.GetComponent<BoxCollider>();
            if (collider == null)
            {
                HelpBox missing = new HelpBox(
                    "A BoxCollider on this GameObject is required.", HelpBoxMessageType.Error);
                root.Add(missing);
                root.Add(new Button(() =>
                {
                    BoxCollider added = Undo.AddComponent<BoxCollider>(Area.gameObject);
                    added.isTrigger = true;
                    EditorUtility.SetDirty(added);
                    EditorSceneManager.MarkSceneDirty(Area.gameObject.scene);
                }) { text = "Add Box Collider" });
            }
            else
            {
                root.Add(new Label($"BoxCollider size: {FormatVector(collider.size)}"));
                if (!collider.isTrigger)
                {
                    root.Add(new HelpBox("The BoxCollider must be a trigger.", HelpBoxMessageType.Error));
                    root.Add(new Button(() =>
                    {
                        Undo.RecordObject(collider, "Enable Tracking Area Trigger");
                        collider.isTrigger = true;
                        EditorUtility.SetDirty(collider);
                        EditorSceneManager.MarkSceneDirty(Area.gameObject.scene);
                    }) { text = "Set Is Trigger" });
                }
            }

            VrcLinkingAreaTracker tracker = snapshot.Trackers.Count == 1 ? snapshot.Trackers[0] : null;
            if (tracker == null)
            {
                root.Add(new HelpBox(snapshot.Trackers.Count == 0
                    ? "No Area Tracker exists in the active scene."
                    : "More than one Area Tracker exists in the active scene.", HelpBoxMessageType.Error));
            }
            else
            {
                root.Add(new Button(() => SelectAndFrame(tracker)) { text = "Select Area Tracker" });
            }

            Foldout advanced = new Foldout { text = "Advanced", value = false };
            TextField idField = new TextField("Stable Area ID") { value = Area.areaId, isReadOnly = true };
            advanced.Add(idField);
            advanced.Add(new Button(() =>
            {
                AreaTrackingSceneUtility.RepairAreaIds(AreaTrackingSceneUtility.Capture());
                idField.SetValueWithoutNotify(Area.areaId);
            }) { text = "Repair Scene Area IDs" });
            root.Add(advanced);
            return root;
        }

        void OnSceneGUI()
        {
            VrcLinkingTrackingArea area = Area;
            if (area == null)
                return;

            BoxCollider collider = area.GetComponent<BoxCollider>();
            if (collider == null)
                return;

            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            AreaTrackingEntry entry = snapshot.Areas.FirstOrDefault(candidate => candidate.Area == area);
            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
            string fingerprint = snapshot.Trackers.Count == 1 && downloader != null && downloader.worldId != Guid.Empty
                ? AreaTrackingSceneUtility.ComputeFingerprint(
                    downloader.worldId, snapshot.Trackers[0].trackingEnabled, snapshot)
                : string.Empty;
            AreaTrackingValidationResult validation = AreaTrackingValidation.Validate(
                snapshot, downloader, fingerprint, validateSyncState: false);
            bool invalid = validation.Issues.Any(issue =>
                issue.Severity == AreaTrackingIssueSeverity.Error &&
                (issue.PrimaryObject == area || issue.SecondaryObject == area ||
                 issue.PrimaryObject == collider || issue.SecondaryObject == collider));

            Color color = invalid
                ? new Color(1f, 0.24f, 0.24f, 0.95f)
                : entry != null && !entry.IsIncluded
                    ? new Color(1f, 0.65f, 0.18f, 0.75f)
                    : new Color(0.27f, 0.78f, 0.9f, 0.9f);

            using (new Handles.DrawingScope(color, collider.transform.localToWorldMatrix))
                Handles.DrawWireCube(collider.center, collider.size);

            Vector3 labelLocal = collider.center + Vector3.up * collider.size.y * 0.5f;
            Vector3 labelWorld = collider.transform.TransformPoint(labelLocal);
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = color } };
            Handles.Label(labelWorld, string.IsNullOrWhiteSpace(area.areaName) ? area.name : area.areaName, style);
        }

        static string FormatVector(Vector3 value) =>
            $"{value.x:0.##} × {value.y:0.##} × {value.z:0.##}";

        static void SelectAndFrame(UnityEngine.Object value)
        {
            Selection.activeObject = value is Component component ? component.gameObject : value;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }
    }
}
