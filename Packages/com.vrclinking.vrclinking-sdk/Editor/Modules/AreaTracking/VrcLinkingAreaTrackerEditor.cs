using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    [CustomEditor(typeof(VrcLinkingAreaTracker))]
    public sealed class VrcLinkingAreaTrackerEditor : UnityEditor.Editor
    {
        enum ServerState
        {
            Unknown,
            Synced,
            Dirty,
            Unavailable,
            Invalid
        }

        VrcLinkingApiHelper _apiHelper;
        VisualElement _root;
        Label _worldStatus;
        Label _syncStatus;
        Button _errorCount;
        Label _serverComparison;
        Label _areaSummary;
        Label _validationSummary;
        VisualElement _setupPanel;
        VisualElement _areasPanel;
        VisualElement _validationPanel;
        ScrollView _areaList;
        ScrollView _validationList;
        ToolbarToggle _setupTab;
        ToolbarToggle _areasTab;
        ToolbarToggle _validationTab;
        Button _syncButton;
        ServerState _serverState = ServerState.Unknown;
        string _serverMessage = "Server status has not been checked.";
        bool _serverRequestRunning;

        VrcLinkingAreaTracker Tracker => (VrcLinkingAreaTracker)target;

        void OnEnable()
        {
            _apiHelper = new VrcLinkingApiHelper();
            EditorApplication.hierarchyChanged += QueueRefresh;
            Undo.undoRedoPerformed += QueueRefresh;
        }

        void OnDisable()
        {
            EditorApplication.hierarchyChanged -= QueueRefresh;
            Undo.undoRedoPerformed -= QueueRefresh;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>(nameof(VrcLinkingAreaTrackerEditor));
            _root = template != null ? template.CloneTree() : new VisualElement();

            StyleSheet styles = Resources.Load<StyleSheet>("AreaTrackingStyles");
            if (styles != null)
                _root.styleSheets.Add(styles);

            CacheElements();
            BindSettings();
            BindTabs();
            BindActions();
            RefreshLocal();
            _root.schedule.Execute(() =>
            {
                if (!(_root?.panel?.focusController?.focusedElement is TextField))
                    RefreshLocal();
            }).Every(1000);
            _ = RefreshServerAsync();
            return _root;
        }

        void CacheElements()
        {
            _worldStatus = _root.Q<Label>("WorldStatus");
            _syncStatus = _root.Q<Label>("SyncStatus");
            _errorCount = _root.Q<Button>("ErrorCount");
            _serverComparison = _root.Q<Label>("ServerComparison");
            _areaSummary = _root.Q<Label>("AreaSummary");
            _validationSummary = _root.Q<Label>("ValidationSummary");
            _setupPanel = _root.Q<VisualElement>("SetupPanel");
            _areasPanel = _root.Q<VisualElement>("AreasPanel");
            _validationPanel = _root.Q<VisualElement>("ValidationPanel");
            _areaList = _root.Q<ScrollView>("AreaList");
            _validationList = _root.Q<ScrollView>("ValidationList");
            _setupTab = _root.Q<ToolbarToggle>("SetupTab");
            _areasTab = _root.Q<ToolbarToggle>("AreasTab");
            _validationTab = _root.Q<ToolbarToggle>("ValidationTab");
            _syncButton = _root.Q<Button>("SyncAreas");
        }

        void BindSettings()
        {
            Toggle enabledToggle = _root.Q<Toggle>("TrackingEnabled");
            Slider heartbeatSlider = _root.Q<Slider>("HeartbeatInterval");
            Slider startupSlider = _root.Q<Slider>("StartupDelay");

            enabledToggle.SetValueWithoutNotify(Tracker.trackingEnabled);
            heartbeatSlider.SetValueWithoutNotify(Tracker.heartbeatIntervalSeconds);
            startupSlider.SetValueWithoutNotify(Tracker.startupDelaySeconds);

            enabledToggle.RegisterValueChangedCallback(evt =>
                ChangeTracker("Change Area Tracking State", () => Tracker.trackingEnabled = evt.newValue,
                    requiresServerSync: true));
            heartbeatSlider.RegisterValueChangedCallback(evt => ChangeTracker("Change Heartbeat Interval",
                () => Tracker.heartbeatIntervalSeconds = Mathf.Clamp(evt.newValue, 30f, 120f)));
            startupSlider.RegisterValueChangedCallback(evt => ChangeTracker("Change Tracking Startup Delay",
                () => Tracker.startupDelaySeconds = Mathf.Clamp(evt.newValue, 5f, 120f)));
        }

        void BindTabs()
        {
            _setupTab.RegisterValueChangedCallback(evt => { if (evt.newValue) ShowTab(_setupTab); });
            _areasTab.RegisterValueChangedCallback(evt => { if (evt.newValue) ShowTab(_areasTab); });
            _validationTab.RegisterValueChangedCallback(evt => { if (evt.newValue) ShowTab(_validationTab); });
            ShowTab(_setupTab);
        }

        void BindActions()
        {
            _root.Q<Button>("AddArea").clicked += () =>
            {
                AreaTrackingSceneUtility.CreateArea(Tracker);
                RefreshLocal();
            };
            _root.Q<Button>("RefreshServer").clicked += () => _ = RefreshServerAsync();
            _syncButton.clicked += () => _ = SyncAsync();
            _errorCount.clicked += () => ShowTab(_validationTab);
        }

        void ShowTab(ToolbarToggle selected)
        {
            _setupTab.SetValueWithoutNotify(selected == _setupTab);
            _areasTab.SetValueWithoutNotify(selected == _areasTab);
            _validationTab.SetValueWithoutNotify(selected == _validationTab);
            _setupPanel.EnableInClassList("tracking-hidden", selected != _setupTab);
            _areasPanel.EnableInClassList("tracking-hidden", selected != _areasTab);
            _validationPanel.EnableInClassList("tracking-hidden", selected != _validationTab);
        }

        void ChangeTracker(string undoName, Action change, bool requiresServerSync = false)
        {
            Undo.RecordObject(Tracker, undoName);
            change();
            EditorUtility.SetDirty(Tracker);
            EditorSceneManager.MarkSceneDirty(Tracker.gameObject.scene);
            if (requiresServerSync)
            {
                _serverState = ServerState.Dirty;
                _serverMessage = "Local configuration has changed and must be synced.";
            }
            RefreshLocal();
        }

        void QueueRefresh()
        {
            if (_root != null)
                _root.schedule.Execute(RefreshLocal).ExecuteLater(1);
        }

        void RefreshLocal()
        {
            if (_root == null || Tracker == null)
                return;

            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            if (AreaTrackingSceneUtility.RepairAreaIds(snapshot))
            {
                snapshot = AreaTrackingSceneUtility.Capture();
                _serverState = ServerState.Dirty;
                _serverMessage = "Area IDs were repaired. Sync the updated configuration.";
            }

            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
            string fingerprint = downloader != null && downloader.worldId != Guid.Empty
                ? AreaTrackingSceneUtility.ComputeFingerprint(
                    downloader.worldId, Tracker.trackingEnabled, snapshot)
                : string.Empty;
            AreaTrackingValidationResult validation = AreaTrackingValidation.Validate(
                snapshot, downloader, fingerprint);
            AreaTrackingValidationResult syncValidation =
                AreaTrackingValidation.ValidateForSync(snapshot, downloader);

            UpdateHeader(downloader, validation);
            RenderAreas(snapshot);
            RenderValidation(validation);
            _syncButton.SetEnabled(!syncValidation.HasErrors && !_serverRequestRunning);
        }

        void UpdateHeader(VrcLinkingDownloader downloader, AreaTrackingValidationResult validation)
        {
            _worldStatus.text = downloader == null
                ? "No VRC Linking Downloader found"
                : downloader.worldId == Guid.Empty
                    ? "Downloader world is not selected"
                    : $"{downloader.serverName} / {downloader.worldName}";

            int errors = validation.Issues.Count(issue => issue.Severity == AreaTrackingIssueSeverity.Error);
            _errorCount.text = errors == 1 ? "1 blocker" : $"{errors} blockers";
            _errorCount.EnableInClassList("status-invalid", errors > 0);

            ServerState displayState = validation.HasErrors ? ServerState.Invalid : _serverState;
            _syncStatus.RemoveFromClassList("status-synced");
            _syncStatus.RemoveFromClassList("status-dirty");
            _syncStatus.RemoveFromClassList("status-unavailable");
            _syncStatus.RemoveFromClassList("status-invalid");

            switch (displayState)
            {
                case ServerState.Synced:
                    _syncStatus.text = "● Synced";
                    _syncStatus.AddToClassList("status-synced");
                    break;
                case ServerState.Dirty:
                    _syncStatus.text = "● Local changes";
                    _syncStatus.AddToClassList("status-dirty");
                    break;
                case ServerState.Invalid:
                    _syncStatus.text = "● Configuration invalid";
                    _syncStatus.AddToClassList("status-invalid");
                    break;
                default:
                    _syncStatus.text = "● Unable to check";
                    _syncStatus.AddToClassList("status-unavailable");
                    break;
            }

            _serverComparison.text = _serverMessage;
        }

        void RenderAreas(AreaTrackingSceneSnapshot snapshot)
        {
            _areaList.Clear();
            List<AreaTrackingEntry> ordered = snapshot.Areas
                .OrderBy(entry => entry.IsIncluded ? 0 : 1)
                .ThenBy(entry => entry.NormalizedName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            _areaSummary.text = $"{snapshot.IncludedAreas.Count} included · " +
                                $"{snapshot.Areas.Count - snapshot.IncludedAreas.Count} excluded · 64 maximum";

            if (ordered.Count == 0)
            {
                _areaList.Add(new HelpBox(
                    "No areas found. Outside-only tracking is valid, or use Add Area to create a volume.",
                    HelpBoxMessageType.Info));
                return;
            }

            foreach (AreaTrackingEntry entry in ordered)
                _areaList.Add(CreateAreaRow(entry));
        }

        VisualElement CreateAreaRow(AreaTrackingEntry entry)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("area-row");
            row.EnableInClassList("area-excluded", !entry.IsIncluded);

            VisualElement main = new VisualElement();
            main.AddToClassList("area-row-main");
            TextField name = new TextField();
            name.AddToClassList("area-row-name");
            name.SetValueWithoutNotify(entry.Area.areaName);
            name.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(entry.Area, "Rename Tracking Area");
                entry.Area.areaName = evt.newValue;
                EditorUtility.SetDirty(entry.Area);
                EditorSceneManager.MarkSceneDirty(entry.Area.gameObject.scene);
                _serverState = ServerState.Dirty;
                _serverMessage = "Local area names have changed and must be synced.";
                QueueRefresh();
            });
            main.Add(name);

            string colliderText = entry.Collider == null
                ? "Missing BoxCollider"
                : $"Box {FormatVector(entry.Collider.size)}";
            string status = entry.IsIncluded ? "Included" : $"Excluded — {entry.ExclusionReason}";
            Label metadata = new Label($"{status} · {colliderText}");
            metadata.AddToClassList("area-row-meta");
            main.Add(metadata);
            row.Add(main);

            VisualElement actions = new VisualElement();
            actions.AddToClassList("area-row-actions");
            actions.Add(ActionButton("◎", "Select and frame area", () => SelectAndFrame(entry.Area)));
            actions.Add(ActionButton("Copy", "Duplicate area", () =>
            {
                AreaTrackingSceneUtility.DuplicateArea(entry.Area);
                _serverState = ServerState.Dirty;
                QueueRefresh();
            }));
            actions.Add(ActionButton("×", "Remove area", () => RemoveArea(entry.Area)));
            row.Add(actions);
            return row;
        }

        void RemoveArea(VrcLinkingTrackingArea area)
        {
            if (AreaTrackingSceneUtility.RequiresRemovalConfirmation(area) &&
                !EditorUtility.DisplayDialog("Remove Tracking Area",
                    "This GameObject contains children or unrelated components. Remove the entire GameObject?",
                    "Remove", "Cancel"))
                return;

            AreaTrackingSceneUtility.RemoveArea(area);
            _serverState = ServerState.Dirty;
            _serverMessage = "An area was removed. Sync the updated configuration.";
            QueueRefresh();
        }

        void RenderValidation(AreaTrackingValidationResult validation)
        {
            _validationList.Clear();
            int errorCount = validation.Issues.Count(issue => issue.Severity == AreaTrackingIssueSeverity.Error);
            _validationSummary.text = errorCount == 0
                ? "No build blockers found."
                : $"{errorCount} build blocker{(errorCount == 1 ? string.Empty : "s")} must be fixed.";

            if (validation.Issues.Count == 0)
            {
                _validationList.Add(new HelpBox("Configuration is valid.", HelpBoxMessageType.Info));
                return;
            }

            foreach (AreaTrackingIssue issue in validation.Issues)
            {
                VisualElement row = new VisualElement();
                row.AddToClassList("validation-row");
                row.AddToClassList(issue.Severity == AreaTrackingIssueSeverity.Error
                    ? "validation-error"
                    : issue.Severity == AreaTrackingIssueSeverity.Warning
                        ? "validation-warning"
                        : "validation-info");
                Label message = new Label(issue.Message);
                message.AddToClassList("validation-message");
                row.Add(message);

                VisualElement actions = new VisualElement();
                actions.AddToClassList("validation-actions");
                if (issue.PrimaryObject != null)
                    actions.Add(ActionButton("Select", "Select relevant object",
                        () => SelectAndFrame(issue.PrimaryObject)));
                if (issue.SecondaryObject != null)
                    actions.Add(ActionButton("Select Other", "Select second relevant object",
                        () => SelectAndFrame(issue.SecondaryObject)));
                row.Add(actions);
                _validationList.Add(row);
            }
        }

        async Task RefreshServerAsync()
        {
            if (_serverRequestRunning || Tracker == null)
                return;

            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
            if (downloader == null || downloader.worldId == Guid.Empty ||
                string.IsNullOrWhiteSpace(downloader.serverId))
            {
                SetServerState(ServerState.Unavailable, "Select a server and world on the Downloader first.");
                return;
            }

            _serverRequestRunning = true;
            SetServerState(ServerState.Unknown, "Checking server configuration…");
            try
            {
                if (!await _apiHelper.IsUserLoggedIn())
                {
                    SetServerState(ServerState.Unavailable,
                        "Not logged in. A previously synced unchanged world can still build.");
                    return;
                }

                WorldTrackingAreasResponse response =
                    await _apiHelper.GetWorldTrackingAreas(downloader.serverId, downloader.worldId);
                snapshot = AreaTrackingSceneUtility.Capture();
                SyncWorldTrackingAreasRequest request =
                    AreaTrackingSceneUtility.BuildSyncRequest(snapshot, Tracker.trackingEnabled);
                bool matches = ServerMatches(response, request, downloader.worldId);
                SetServerState(matches ? ServerState.Synced : ServerState.Dirty,
                    matches ? "Server and scene configuration match." :
                        "Scene configuration differs from the server. Use Sync Areas when ready.");
            }
            catch (Exception exception)
            {
                SetServerState(ServerState.Unavailable, $"Unable to check server: {exception.Message}");
            }
            finally
            {
                _serverRequestRunning = false;
                RefreshLocal();
            }
        }

        async Task SyncAsync()
        {
            if (_serverRequestRunning || Tracker == null)
                return;

            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
            AreaTrackingValidationResult validation = AreaTrackingValidation.ValidateForSync(snapshot, downloader);
            if (validation.HasErrors)
            {
                SetServerState(ServerState.Invalid, "Fix validation errors before syncing.");
                ShowTab(_validationTab);
                RefreshLocal();
                return;
            }

            _serverRequestRunning = true;
            _syncButton.SetEnabled(false);
            SetServerState(ServerState.Unknown, "Syncing configuration…");
            try
            {
                if (!await _apiHelper.IsUserLoggedIn())
                    throw new InvalidOperationException("Log in through the VRC Linking Downloader before syncing.");

                snapshot = AreaTrackingSceneUtility.Capture();
                downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
                validation = AreaTrackingValidation.ValidateForSync(snapshot, downloader);
                if (validation.HasErrors)
                    throw new InvalidOperationException(
                        "The scene configuration changed and is no longer valid. Fix validation errors and sync again.");

                SyncWorldTrackingAreasRequest request =
                    AreaTrackingSceneUtility.BuildSyncRequest(snapshot, Tracker.trackingEnabled);
                string guildId = downloader.serverId;
                Guid worldId = downloader.worldId;
                bool trackingEnabled = Tracker.trackingEnabled;
                string submittedFingerprint = AreaTrackingSceneUtility.ComputeFingerprint(
                    worldId, trackingEnabled, snapshot);
                WorldTrackingAreasResponse response = await _apiHelper.SyncWorldTrackingAreas(
                    guildId, worldId, request);
                if (!ServerMatches(response, request, worldId))
                    throw new InvalidOperationException("The server returned a configuration different from the request.");

                AreaTrackingSceneSnapshot currentSnapshot = AreaTrackingSceneUtility.Capture();
                VrcLinkingDownloader currentDownloader =
                    AreaTrackingSceneUtility.GetSingleDownloader(currentSnapshot);
                string currentFingerprint = currentDownloader != null && currentDownloader.worldId != Guid.Empty
                    ? AreaTrackingSceneUtility.ComputeFingerprint(
                        currentDownloader.worldId, Tracker.trackingEnabled, currentSnapshot)
                    : string.Empty;
                if (currentDownloader != downloader || currentDownloader.serverId != guildId ||
                    currentDownloader.worldId != worldId || Tracker.trackingEnabled != trackingEnabled ||
                    !string.Equals(currentFingerprint, submittedFingerprint, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "The scene configuration changed while syncing. Review it and sync again.");
                }

                Undo.RecordObject(Tracker, "Sync Area Tracking Configuration");
                Tracker.syncedWorldId = worldId.ToString("D");
                Tracker.syncedConfigurationHash = submittedFingerprint;
                EditorUtility.SetDirty(Tracker);
                EditorSceneManager.MarkSceneDirty(Tracker.gameObject.scene);
                SetServerState(ServerState.Synced, "Configuration synced successfully.");
            }
            catch (Exception exception)
            {
                SetServerState(ServerState.Dirty, $"Sync failed: {exception.Message}");
            }
            finally
            {
                _serverRequestRunning = false;
                RefreshLocal();
            }
        }

        void SetServerState(ServerState state, string message)
        {
            _serverState = state;
            _serverMessage = message;
            if (_root != null)
                RefreshLocal();
        }

        static bool ServerMatches(WorldTrackingAreasResponse response,
            SyncWorldTrackingAreasRequest request, Guid expectedWorldId)
        {
            if (response == null || response.WorldId != expectedWorldId || response.Areas == null ||
                response.TrackingEnabled != request.TrackingEnabled)
                return false;

            List<WorldTrackingAreaResponse> serverAreas = response.Areas
                .Where(area => !area.IsOutside && area.IsActive)
                .OrderBy(area => area.DisplayOrder)
                .ThenBy(area => area.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (serverAreas.Count != request.Areas.Count)
                return false;

            for (int index = 0; index < serverAreas.Count; index++)
            {
                WorldTrackingAreaResponse server = serverAreas[index];
                SyncWorldTrackingAreaRequest local = request.Areas[index];
                if (server.Id != local.Id || server.DisplayOrder != local.DisplayOrder ||
                    !string.Equals(server.Name?.Trim(), local.Name?.Trim(),
                        StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        static Button ActionButton(string text, string tooltip, Action action)
        {
            Button button = new Button(action) { text = text, tooltip = tooltip };
            return button;
        }

        static string FormatVector(Vector3 value) => $"{value.x:0.##} × {value.y:0.##} × {value.z:0.##}";

        static void SelectAndFrame(UnityEngine.Object value)
        {
            Selection.activeObject = value is Component component ? component.gameObject : value;
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
        }
    }
}
