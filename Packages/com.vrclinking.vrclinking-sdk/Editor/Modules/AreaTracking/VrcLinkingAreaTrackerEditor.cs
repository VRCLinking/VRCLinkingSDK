using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
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
        VisualElement _mapPanel;
        ScrollView _areaList;
        ScrollView _validationList;
        ScrollView _mapAreaList;
        ToolbarToggle _setupTab;
        ToolbarToggle _areasTab;
        ToolbarToggle _mapTab;
        ToolbarToggle _validationTab;
        Button _syncButton;
        Button _takeMapSnapshotButton;
        Button _cancelMapSnapshotButton;
        DropdownField _mapDestination;
        DropdownField _mapResolution;
        TextField _mapName;
        FloatField _mapPadding;
        LayerMaskField _mapLayerMask;
        ColorField _mapBackground;
        Label _mapCapabilitiesLabel;
        Label _mapAreaSummary;
        Label _mapSnapshotStatus;
        HelpBox _mapSelectionWarning;
        ProgressBar _mapSnapshotProgress;
        readonly HashSet<Guid> _selectedMapAreaIds = new HashSet<Guid>();
        HashSet<Guid> _recommendedMapAreaIds = new HashSet<Guid>();
        WorldMapCapabilities _mapCapabilities;
        List<WorldMapSummary> _worldMaps = new List<WorldMapSummary>();
        readonly List<Guid?> _mapDestinationIds = new List<Guid?>();
        int _worldMapCount;
        string _mapConfigurationScope = string.Empty;
        bool _mapConfigurationRequestRunning;
        CancellationTokenSource _snapshotCancellation;
        bool _snapshotRunning;
        bool _snapshotFinalized;
        bool _mapSelectionInitialized;
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
            _snapshotCancellation?.Cancel();
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
            _mapPanel = _root.Q<VisualElement>("MapPanel");
            _areaList = _root.Q<ScrollView>("AreaList");
            _validationList = _root.Q<ScrollView>("ValidationList");
            _mapAreaList = _root.Q<ScrollView>("MapAreaList");
            _setupTab = _root.Q<ToolbarToggle>("SetupTab");
            _areasTab = _root.Q<ToolbarToggle>("AreasTab");
            _mapTab = _root.Q<ToolbarToggle>("MapTab");
            _validationTab = _root.Q<ToolbarToggle>("ValidationTab");
            _syncButton = _root.Q<Button>("SyncAreas");
            _takeMapSnapshotButton = _root.Q<Button>("TakeMapSnapshot");
            _cancelMapSnapshotButton = _root.Q<Button>("CancelMapSnapshot");
            _mapDestination = _root.Q<DropdownField>("MapDestination");
            _mapResolution = _root.Q<DropdownField>("MapResolution");
            _mapName = _root.Q<TextField>("MapName");
            _mapPadding = _root.Q<FloatField>("MapPadding");
            _mapBackground = _root.Q<ColorField>("MapBackground");
            _mapCapabilitiesLabel = _root.Q<Label>("MapCapabilities");
            _mapAreaSummary = _root.Q<Label>("MapAreaSummary");
            _mapSnapshotStatus = _root.Q<Label>("MapSnapshotStatus");
            VisualElement outsideTrackingHelpBoxContainer =
                _root.Q<VisualElement>("OutsideTrackingHelpBoxContainer");
            outsideTrackingHelpBoxContainer?.Add(new HelpBox(
                "Players outside every included box are recorded as Outside automatically.",
                HelpBoxMessageType.Info));

            _mapSelectionWarning = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            _mapSelectionWarning.AddToClassList("tracking-hidden");
            _root.Q<VisualElement>("MapSelectionWarningContainer")?.Add(_mapSelectionWarning);
            _mapSnapshotProgress = _root.Q<ProgressBar>("MapSnapshotProgress");
            _mapLayerMask = new LayerMaskField("Layers") { value = ~0 };
            _root.Q<VisualElement>("MapLayerMaskContainer").Add(_mapLayerMask);
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
            _mapTab.RegisterValueChangedCallback(evt =>
            {
                if (!evt.newValue) return;
                ShowTab(_mapTab);
                if (_mapCapabilities == null && !_snapshotRunning) _ = RefreshMapConfigurationAsync();
            });
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
            _root.Q<Button>("MapSelectRecommended").clicked += SelectRecommendedMapAreas;
            _root.Q<Button>("MapSelectAll").clicked += SelectAllMapAreas;
            _root.Q<Button>("MapSelectNone").clicked += () =>
            {
                _selectedMapAreaIds.Clear();
                RefreshLocal();
            };
            _takeMapSnapshotButton.clicked += () => _ = TakeMapSnapshotAsync();
            _cancelMapSnapshotButton.clicked += CancelMapSnapshot;
            _mapDestination.RegisterValueChangedCallback(_ => UpdateMapDestinationName());
        }

        void ShowTab(ToolbarToggle selected)
        {
            _setupTab.SetValueWithoutNotify(selected == _setupTab);
            _areasTab.SetValueWithoutNotify(selected == _areasTab);
            _mapTab.SetValueWithoutNotify(selected == _mapTab);
            _validationTab.SetValueWithoutNotify(selected == _validationTab);
            _setupPanel.EnableInClassList("tracking-hidden", selected != _setupTab);
            _areasPanel.EnableInClassList("tracking-hidden", selected != _areasTab);
            _mapPanel.EnableInClassList("tracking-hidden", selected != _mapTab);
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
            string mapScope = GetMapConfigurationScope(downloader);
            if (!string.Equals(mapScope, _mapConfigurationScope, StringComparison.Ordinal))
            {
                ResetMapConfiguration(mapScope);
                if (_mapTab?.value == true && !_mapConfigurationRequestRunning)
                    _ = RefreshMapConfigurationAsync();
            }
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
            RenderMapAreas(snapshot);
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

        void RenderMapAreas(AreaTrackingSceneSnapshot snapshot)
        {
            if (_mapAreaList == null) return;
            List<AreaTrackingEntry> capturable = snapshot.IncludedAreas
                .Where(area => area.ParsedId != Guid.Empty && area.Collider != null)
                .OrderBy(area => area.NormalizedName, StringComparer.OrdinalIgnoreCase).ToList();
            double ratio = _mapCapabilities?.ExtremeGroupImpactRatio ?? 4d;
            _recommendedMapAreaIds = WorldMapSnapshotCapture.FindRecommendedAreaIds(capturable, ratio);
            HashSet<Guid> currentIds = new HashSet<Guid>(capturable.Select(area => area.ParsedId));
            _selectedMapAreaIds.RemoveWhere(id => !currentIds.Contains(id));
            if (!_mapSelectionInitialized && capturable.Count > 0)
            {
                _mapSelectionInitialized = true;
                foreach (Guid id in _recommendedMapAreaIds) _selectedMapAreaIds.Add(id);
            }

            _mapAreaList.Clear();
            foreach (AreaTrackingEntry entry in capturable)
            {
                bool isExtreme = !_recommendedMapAreaIds.Contains(entry.ParsedId);
                Toggle toggle = new Toggle(isExtreme
                    ? entry.NormalizedName + "  (distant outlier)"
                    : entry.NormalizedName);
                toggle.AddToClassList("map-area-row");
                toggle.EnableInClassList("map-area-extreme", isExtreme);
                toggle.SetValueWithoutNotify(_selectedMapAreaIds.Contains(entry.ParsedId));
                Guid areaId = entry.ParsedId;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue) _selectedMapAreaIds.Add(areaId);
                    else _selectedMapAreaIds.Remove(areaId);
                    RefreshMapSelectionSummary(snapshot);
                });
                _mapAreaList.Add(toggle);
            }
            RefreshMapSelectionSummary(snapshot);
        }

        void RefreshMapSelectionSummary(AreaTrackingSceneSnapshot snapshot)
        {
            int selected = snapshot.IncludedAreas.Count(area => _selectedMapAreaIds.Contains(area.ParsedId));
            int outliers = snapshot.IncludedAreas.Count(area =>
                _selectedMapAreaIds.Contains(area.ParsedId) && !_recommendedMapAreaIds.Contains(area.ParsedId));
            _mapAreaSummary.text = string.Format("{0} of {1} areas selected", selected,
                snapshot.IncludedAreas.Count);
            bool extremeSpread = WorldMapSnapshotCapture.SelectionHasExtremeSpread(snapshot.IncludedAreas,
                _selectedMapAreaIds, _mapCapabilities?.ExtremeGroupImpactRatio ?? 4d);
            _mapSelectionWarning.EnableInClassList("tracking-hidden", !extremeSpread && outliers == 0);
            if (extremeSpread)
            {
                _mapSelectionWarning.text =
                    "This selection contains extremely distant area groups. Capturing them together is blocked because it would waste almost all image resolution. Use Recommended for the main map, then select each distant group separately to create additional maps.";
            }
            else if (outliers > 0)
            {
                _mapSelectionWarning.text =
                    "A distant area is selected on its own. This is suitable for creating a separate map.";
            }
            _takeMapSnapshotButton.SetEnabled(selected > 0 && !extremeSpread && !_snapshotRunning &&
                                               _mapCapabilities != null && _mapDestinationIds.Count > 0);
        }

        void SelectRecommendedMapAreas()
        {
            _selectedMapAreaIds.Clear();
            foreach (Guid id in _recommendedMapAreaIds) _selectedMapAreaIds.Add(id);
            RefreshLocal();
        }

        void SelectAllMapAreas()
        {
            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            _selectedMapAreaIds.Clear();
            foreach (AreaTrackingEntry area in snapshot.IncludedAreas)
                if (area.ParsedId != Guid.Empty && area.Collider != null) _selectedMapAreaIds.Add(area.ParsedId);
            RefreshLocal();
        }

        async Task RefreshMapConfigurationAsync()
        {
            if (_mapConfigurationRequestRunning) return;
            AreaTrackingSceneSnapshot snapshot = AreaTrackingSceneUtility.Capture();
            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(snapshot);
            if (downloader == null || downloader.worldId == Guid.Empty ||
                string.IsNullOrWhiteSpace(downloader.serverId))
            {
                SetMapSnapshotStatus("Select a server and world on the Downloader first.");
                return;
            }

            string requestedScope = GetMapConfigurationScope(downloader);
            if (!string.Equals(requestedScope, _mapConfigurationScope, StringComparison.Ordinal))
                ResetMapConfiguration(requestedScope);
            _mapConfigurationRequestRunning = true;
            SetMapSnapshotStatus("Loading server capture limits…");
            try
            {
                if (!await _apiHelper.IsUserLoggedIn())
                    throw new InvalidOperationException("Log in through the VRC Linking Downloader first.");
                Task<WorldMapCapabilities> capabilitiesTask =
                    _apiHelper.GetWorldMapCapabilities(downloader.serverId, downloader.worldId);
                Task<List<WorldMapSummary>> mapsTask =
                    _apiHelper.GetWorldMaps(downloader.serverId, downloader.worldId);
                await Task.WhenAll(capabilitiesTask, mapsTask);
                AreaTrackingSceneSnapshot currentSnapshot = AreaTrackingSceneUtility.Capture();
                if (!string.Equals(requestedScope,
                        GetMapConfigurationScope(AreaTrackingSceneUtility.GetSingleDownloader(currentSnapshot)),
                        StringComparison.Ordinal))
                    return;
                _mapCapabilities = capabilitiesTask.Result;
                _worldMapCount = mapsTask.Result.Count(map =>
                    !string.Equals(map.Status, "Deleting", StringComparison.OrdinalIgnoreCase));
                _worldMaps = mapsTask.Result
                    .Where(map => !map.IsProcessingReplacement &&
                                  !string.Equals(map.Status, "Deleting", StringComparison.OrdinalIgnoreCase) &&
                                  !string.Equals(map.Status, "Uploading", StringComparison.OrdinalIgnoreCase) &&
                                  !string.Equals(map.Status, "Processing", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                ConfigureMapChoices();
                _mapCapabilitiesLabel.text = string.Format(
                    "Server limit: {0:N0}px long edge, {1:N0} megapixels · capture tiles: {2:N0}px · {3} maps maximum",
                    _mapCapabilities.MaximumLongEdge, _mapCapabilities.MaximumPixelCount / 1000000d,
                    _mapCapabilities.CaptureTileSize, _mapCapabilities.MaximumMapsPerWorld);
                SetMapSnapshotStatus("Ready to capture.");
            }
            catch (Exception exception)
            {
                _mapCapabilities = null;
                SetMapSnapshotStatus("Unable to load map configuration: " + exception.Message);
            }
            finally
            {
                string currentScope = GetMapConfigurationScope(
                    AreaTrackingSceneUtility.GetSingleDownloader(AreaTrackingSceneUtility.Capture()));
                bool restartForNewScope = !string.Equals(currentScope, requestedScope, StringComparison.Ordinal) &&
                                          !string.IsNullOrEmpty(currentScope) && _mapTab?.value == true;
                _mapConfigurationRequestRunning = false;
                RefreshLocal();
                if (restartForNewScope) _ = RefreshMapConfigurationAsync();
            }
        }

        void ResetMapConfiguration(string scope)
        {
            _mapConfigurationScope = scope ?? string.Empty;
            _mapCapabilities = null;
            _worldMaps.Clear();
            _worldMapCount = 0;
            _mapDestinationIds.Clear();
            _mapSelectionInitialized = false;
            _selectedMapAreaIds.Clear();
            if (_mapDestination != null)
            {
                _mapDestination.choices = new List<string> { "Loading destinations…" };
                _mapDestination.SetValueWithoutNotify("Loading destinations…");
            }
            if (_mapCapabilitiesLabel != null)
                _mapCapabilitiesLabel.text = "Open this tab to load server capture limits.";
        }

        static string GetMapConfigurationScope(VrcLinkingDownloader downloader)
        {
            if (downloader == null || downloader.worldId == Guid.Empty ||
                string.IsNullOrWhiteSpace(downloader.serverId)) return string.Empty;
            return downloader.serverId.Trim() + ":" + downloader.worldId.ToString("N");
        }

        void ConfigureMapChoices()
        {
            List<string> destinations = new List<string>();
            _mapDestinationIds.Clear();
            if (_worldMapCount < _mapCapabilities.MaximumMapsPerWorld)
            {
                destinations.Add("Create new map");
                _mapDestinationIds.Add(null);
            }
            foreach (WorldMapSummary map in _worldMaps)
            {
                destinations.Add("Replace: " + map.Name);
                _mapDestinationIds.Add(map.Id);
            }
            if (destinations.Count == 0) destinations.Add("No destination currently available");
            string previousDestination = _mapDestination.value;
            _mapDestination.choices = destinations;
            _mapDestination.SetValueWithoutNotify(destinations.Contains(previousDestination)
                ? previousDestination
                : destinations[0]);

            List<string> resolutions = new List<string> { "Auto (recommended)" };
            foreach (int resolution in new[] { 2048, 4096, 6144, 8192 })
                if (resolution <= _mapCapabilities.MaximumLongEdge)
                    resolutions.Add(string.Format("{0:N0}px long edge", resolution));
            _mapResolution.choices = resolutions;
            if (!resolutions.Contains(_mapResolution.value))
                _mapResolution.SetValueWithoutNotify(resolutions[0]);
            UpdateMapDestinationName();
        }

        void UpdateMapDestinationName()
        {
            int index = _mapDestination?.index ?? 0;
            if (index < 0 || index >= _mapDestinationIds.Count || !_mapDestinationIds[index].HasValue)
            {
                if (string.IsNullOrWhiteSpace(_mapName.value))
                    _mapName.SetValueWithoutNotify("World Map");
                return;
            }
            WorldMapSummary targetMap = _worldMaps.FirstOrDefault(map => map.Id == _mapDestinationIds[index].Value);
            if (targetMap != null) _mapName.SetValueWithoutNotify(targetMap.Name);
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

        async Task TakeMapSnapshotAsync()
        {
            if (_snapshotRunning) return;
            if (_mapCapabilities == null)
            {
                await RefreshMapConfigurationAsync();
                if (_mapCapabilities == null) return;
            }

            AreaTrackingSceneSnapshot sceneSnapshot = AreaTrackingSceneUtility.Capture();
            VrcLinkingDownloader downloader = AreaTrackingSceneUtility.GetSingleDownloader(sceneSnapshot);
            AreaTrackingValidationResult validation = AreaTrackingValidation.ValidateForSync(sceneSnapshot, downloader);
            if (validation.HasErrors)
            {
                SetMapSnapshotStatus("Fix area validation errors before taking a snapshot.");
                ShowTab(_validationTab);
                return;
            }
            if (WorldMapSnapshotCapture.SelectionHasExtremeSpread(sceneSnapshot.IncludedAreas,
                    _selectedMapAreaIds, _mapCapabilities.ExtremeGroupImpactRatio))
            {
                SetMapSnapshotStatus("The selected areas are too far apart. Capture separate maps for distant groups.");
                return;
            }
            string mapName = (_mapName.value ?? string.Empty).Trim();
            if (mapName.Length == 0)
            {
                SetMapSnapshotStatus("Enter a map name.");
                return;
            }
            int destinationIndex = _mapDestination.index;
            if (destinationIndex < 0 || destinationIndex >= _mapDestinationIds.Count)
            {
                SetMapSnapshotStatus("No map destination is currently available.");
                return;
            }

            string guildId = downloader.serverId;
            Guid worldId = downloader.worldId;
            Guid? targetMapId = _mapDestinationIds[destinationIndex];
            HashSet<Guid> selectedAreaIds = new HashSet<Guid>(_selectedMapAreaIds);
            WorldMapCapabilities capabilities = _mapCapabilities;
            int requestedLongEdge = GetRequestedLongEdge();
            float padding = Mathf.Clamp(_mapPadding.value, 0f, 100f);
            int layerMask = _mapLayerMask.value;
            Color background = _mapBackground.value;
            bool trackingEnabled = Tracker.trackingEnabled;
            string localConfigurationFingerprint = AreaTrackingSceneUtility.ComputeFingerprint(
                worldId, trackingEnabled, sceneSnapshot);

            _snapshotRunning = true;
            _snapshotFinalized = false;
            _snapshotCancellation = new CancellationTokenSource();
            CancellationToken cancellationToken = _snapshotCancellation.Token;
            WorldMapSnapshotPackage package = null;
            Guid sessionId = Guid.Empty;
            bool serverReportedTerminalFailure = false;
            SetSnapshotControlsRunning(true);
            try
            {
                if (!await _apiHelper.IsUserLoggedIn())
                    throw new InvalidOperationException("Log in through the VRC Linking Downloader before capturing.");

                SetMapSnapshotStatus("Verifying the synchronized areas…");
                WorldTrackingAreasResponse serverAreas =
                    await _apiHelper.GetWorldTrackingAreas(guildId, worldId);
                SyncWorldTrackingAreasRequest localRequest =
                    AreaTrackingSceneUtility.BuildSyncRequest(sceneSnapshot, Tracker.trackingEnabled);
                if (!ServerMatches(serverAreas, localRequest, worldId))
                    throw new InvalidOperationException(
                        "The scene areas do not match the server. Use Sync Areas before taking a snapshot.");

                HashSet<Guid> activeServerAreaIds = new HashSet<Guid>(serverAreas.Areas
                    .Where(area => area.IsActive && !area.IsOutside).Select(area => area.Id));
                HashSet<Guid> selectableServerAreaIds = new HashSet<Guid>(serverAreas.Areas
                    .Where(area => area.IsActive && !area.IsOutside).Select(area => area.Id));
                if (selectedAreaIds.Count == 0 || selectedAreaIds.Any(id => !selectableServerAreaIds.Contains(id)))
                    throw new InvalidOperationException(
                        "The map selection contains an area that is no longer active on the server.");
                string areaFingerprint = WorldMapSnapshotCapture.ComputeAreaFingerprint(activeServerAreaIds);
                WorldMapSnapshotPlan plan = WorldMapSnapshotCapture.CreatePlan(sceneSnapshot.IncludedAreas,
                    selectedAreaIds, capabilities, requestedLongEdge, padding, layerMask, background);
                string geometrySignature = ComputeAreaGeometrySignature(sceneSnapshot, selectedAreaIds);

                SetMapSnapshotStatus(string.Format("Capturing {0:N0} × {1:N0}px in {2} tile{3}…",
                    plan.PixelWidth, plan.PixelHeight, plan.Columns * plan.Rows,
                    plan.Columns * plan.Rows == 1 ? string.Empty : "s"));
                package = await WorldMapSnapshotCapture.CaptureAsync(plan, ReportSnapshotProgress,
                    cancellationToken);
                if (package.ExpectedBytes > capabilities.MaximumUploadBytes)
                    throw new InvalidOperationException(string.Format(
                        "The PNG tiles total {0}, above the server upload limit of {1}. Choose a smaller resolution.",
                        FormatBytes(package.ExpectedBytes), FormatBytes(capabilities.MaximumUploadBytes)));
                WorldMapSnapshotTile oversizedPart = package.Tiles.FirstOrDefault(tile =>
                    tile.ByteLength > capabilities.MaximumUploadPartBytes);
                if (oversizedPart != null)
                    throw new InvalidOperationException(string.Format(
                        "Capture tile {0} is {1}, above the per-part limit of {2}. Choose a smaller resolution.",
                        oversizedPart.PartId, FormatBytes(oversizedPart.ByteLength),
                        FormatBytes(capabilities.MaximumUploadPartBytes)));

                AreaTrackingSceneSnapshot currentScene = AreaTrackingSceneUtility.Capture();
                VrcLinkingDownloader currentDownloader = AreaTrackingSceneUtility.GetSingleDownloader(currentScene);
                if (!string.Equals(guildId, currentDownloader?.serverId, StringComparison.Ordinal) ||
                    currentDownloader?.worldId != worldId)
                    throw new InvalidOperationException(
                        "The selected Downloader world changed during capture. Review the Map Snapshot tab and capture again.");
                if (Tracker.trackingEnabled != trackingEnabled ||
                    !string.Equals(localConfigurationFingerprint,
                        AreaTrackingSceneUtility.ComputeFingerprint(worldId, Tracker.trackingEnabled, currentScene),
                        StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "The area configuration changed during capture. Sync the scene if needed, then capture again.");
                if (!string.Equals(geometrySignature,
                        ComputeAreaGeometrySignature(currentScene, selectedAreaIds), StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "An included area moved or changed while the image was being captured. Capture again.");

                CreateWorldMapSnapshotUploadRequest createRequest = BuildSnapshotUploadRequest(
                    package, mapName, targetMapId, areaFingerprint);
                SetMapSnapshotStatus("Creating the upload session…");
                WorldMapUploadSession session = await _apiHelper.CreateWorldMapUpload(
                    guildId, worldId, createRequest, cancellationToken);
                sessionId = session.SessionId;

                for (int index = 0; index < package.Tiles.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    WorldMapSnapshotTile tile = package.Tiles[index];
                    ReportSnapshotProgress(string.Format("Uploading tile {0} of {1}…",
                        index + 1, package.Tiles.Count), index / (float)package.Tiles.Count);
                    byte[] bytes = await ReadAllBytesAsync(tile.FilePath, cancellationToken);
                    await _apiHelper.UploadWorldMapPart(guildId, worldId,
                        sessionId, tile.PartId, bytes, cancellationToken);
                }

                SetMapSnapshotStatus("Finalizing the upload…");
                _snapshotFinalized = true;
                try
                {
                    session = await _apiHelper.FinalizeWorldMapUpload(guildId, worldId,
                        sessionId, session.ManifestSha256, cancellationToken);
                }
                catch (Exception finalizeException)
                {
                    // A connection can disappear after the server accepted finalization. Reconcile before deciding
                    // whether this is still a cancellable upload.
                    WorldMapUploadSession reconciled = await TryGetWorldMapUploadAsync(
                        guildId, worldId, sessionId, CancellationToken.None, 3);
                    if (reconciled == null)
                    {
                        throw new InvalidOperationException(
                            "The finalization response was lost and the upload state could not be confirmed. " +
                            "Check the website before taking another snapshot.", finalizeException);
                    }
                    session = reconciled;
                    if (StatusIs(session.Status, "Uploading", "0"))
                    {
                        _snapshotFinalized = false;
                        throw finalizeException;
                    }
                }
                _cancelMapSnapshotButton.text = "Stop waiting";
                while (!StatusIs(session.Status, "Complete", "3"))
                {
                    if (StatusIs(session.Status, "Failed", "4") ||
                        StatusIs(session.Status, "CleanupPending", "5"))
                    {
                        serverReportedTerminalFailure = true;
                        throw new InvalidOperationException(session.ProcessingError ?? "Server map processing failed.");
                    }
                    SetMapSnapshotStatus("Upload complete. The server is building zoom tiles…");
                    _mapSnapshotProgress.value = 100f;
                    await Task.Delay(2000, cancellationToken);
                    WorldMapUploadSession polled = await TryGetWorldMapUploadAsync(
                        guildId, worldId, sessionId, cancellationToken, 5);
                    if (polled == null)
                        throw new InvalidOperationException(
                            "The upload was finalized, but its processing status is temporarily unavailable. " +
                            "Check the website before taking another snapshot.");
                    session = polled;
                }

                _mapSnapshotProgress.value = 100f;
                await RefreshMapConfigurationAsync();
                SetMapSnapshotStatus("Map snapshot is ready on the website.");
            }
            catch (OperationCanceledException)
            {
                if (_snapshotFinalized)
                {
                    SetMapSnapshotStatus("Stopped waiting. The finalized map continues processing on the server.");
                }
                else
                {
                    await TryCancelSnapshotUploadAsync(guildId, worldId, sessionId);
                    SetMapSnapshotStatus("Map snapshot cancelled.");
                }
            }
            catch (Exception exception)
            {
                if (!_snapshotFinalized)
                {
                    await TryCancelSnapshotUploadAsync(guildId, worldId, sessionId);
                    SetMapSnapshotStatus("Map snapshot failed: " + exception.Message);
                }
                else if (serverReportedTerminalFailure)
                {
                    SetMapSnapshotStatus("Server map processing failed: " + exception.Message);
                }
                else
                {
                    SetMapSnapshotStatus("Map upload finalized; current status is unknown: " + exception.Message);
                }
            }
            finally
            {
                package?.Dispose();
                _snapshotCancellation?.Dispose();
                _snapshotCancellation = null;
                _snapshotRunning = false;
                _snapshotFinalized = false;
                SetSnapshotControlsRunning(false);
                RefreshLocal();
            }
        }

        CreateWorldMapSnapshotUploadRequest BuildSnapshotUploadRequest(WorldMapSnapshotPackage package,
            string mapName, Guid? targetMapId, string areaFingerprint)
        {
            return new CreateWorldMapSnapshotUploadRequest
            {
                TargetMapId = targetMapId,
                Name = mapName,
                Width = package.Plan.PixelWidth,
                Height = package.Plan.PixelHeight,
                ExpectedBytes = package.ExpectedBytes,
                AreaFingerprint = areaFingerprint,
                AreaIds = package.Plan.Areas.Select(area => area.ParsedId).ToList(),
                CaptureMetadata = WorldMapSnapshotCapture.BuildMetadata(package.Plan, areaFingerprint),
                ProposedOverlays = package.Overlays,
                Manifest = new WorldMapUploadManifestRequest
                {
                    OriginalFileName = SanitizeFileName(mapName) + "-unity-snapshot.png",
                    UnityTiles = package.Tiles.Select(tile => new WorldMapUnityTilePart
                    {
                        PartId = tile.PartId,
                        Order = tile.Order,
                        ByteLength = tile.ByteLength,
                        Sha256 = tile.Sha256,
                        TileX = tile.TileX,
                        TileY = tile.TileY,
                        PixelX = tile.PixelX,
                        PixelY = tile.PixelY,
                        Width = tile.Width,
                        Height = tile.Height
                    }).ToList()
                }
            };
        }

        async Task TryCancelSnapshotUploadAsync(string guildId, Guid worldId, Guid sessionId)
        {
            if (string.IsNullOrWhiteSpace(guildId) || worldId == Guid.Empty || sessionId == Guid.Empty) return;
            using (CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
            try { await _apiHelper.CancelWorldMapUpload(guildId, worldId, sessionId, timeout.Token); }
            catch (Exception exception) { Debug.LogWarning("Could not cancel map upload session: " + exception.Message); }
        }

        async Task<WorldMapUploadSession> TryGetWorldMapUploadAsync(string guildId, Guid worldId,
            Guid sessionId, CancellationToken cancellationToken, int attempts)
        {
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using (CancellationTokenSource timeout =
                           CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        timeout.CancelAfter(TimeSpan.FromSeconds(15));
                        return await _apiHelper.GetWorldMapUpload(guildId, worldId, sessionId, timeout.Token);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch when (attempt + 1 < attempts)
                {
                    await Task.Delay(Math.Min(4000, 500 * (1 << attempt)), cancellationToken);
                }
                catch { return null; }
            }
            return null;
        }

        void CancelMapSnapshot()
        {
            if (!_snapshotRunning) return;
            SetMapSnapshotStatus(_snapshotFinalized
                ? "Stopping status checks…"
                : "Cancelling snapshot and cleaning up the upload…");
            _snapshotCancellation?.Cancel();
        }

        void SetSnapshotControlsRunning(bool running)
        {
            if (_root == null) return;
            _takeMapSnapshotButton.SetEnabled(!running);
            _cancelMapSnapshotButton.EnableInClassList("tracking-hidden", !running);
            _mapSnapshotProgress.EnableInClassList("tracking-hidden", !running);
            _mapSnapshotProgress.value = running ? 0f : _mapSnapshotProgress.value;
            _cancelMapSnapshotButton.text = "Cancel";
            _mapDestination.SetEnabled(!running);
            _mapName.SetEnabled(!running);
            _mapResolution.SetEnabled(!running);
            _mapPadding.SetEnabled(!running);
            _mapLayerMask.SetEnabled(!running);
            _mapBackground.SetEnabled(!running);
            _mapAreaList.SetEnabled(!running);
            _root.Q<Button>("MapSelectRecommended").SetEnabled(!running);
            _root.Q<Button>("MapSelectAll").SetEnabled(!running);
            _root.Q<Button>("MapSelectNone").SetEnabled(!running);
        }

        void ReportSnapshotProgress(string message, float progress)
        {
            if (_root == null) return;
            SetMapSnapshotStatus(message);
            _mapSnapshotProgress.value = Mathf.Clamp01(progress) * 100f;
        }

        void SetMapSnapshotStatus(string message)
        {
            if (_mapSnapshotStatus != null) _mapSnapshotStatus.text = message;
        }

        int GetRequestedLongEdge()
        {
            if (_mapResolution == null || _mapResolution.index <= 0) return 0;
            string digits = new string(_mapResolution.value.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out int result) ? result : 0;
        }

        static string ComputeAreaGeometrySignature(AreaTrackingSceneSnapshot snapshot,
            IReadOnlyCollection<Guid> selectedIds)
        {
            StringBuilder builder = new StringBuilder();
            foreach (AreaTrackingEntry area in snapshot.IncludedAreas
                         .Where(area => selectedIds.Contains(area.ParsedId)).OrderBy(area => area.ParsedId))
            {
                if (area.Collider == null) return string.Empty;
                builder.Append(area.ParsedId.ToString("N")).Append('|');
                Matrix4x4 matrix = area.Collider.transform.localToWorldMatrix;
                for (int index = 0; index < 16; index++) builder.Append(matrix[index].ToString("R")).Append(',');
                Vector3 center = area.Collider.center;
                Vector3 size = area.Collider.size;
                builder.Append(center.x.ToString("R")).Append(',').Append(center.y.ToString("R")).Append(',')
                    .Append(center.z.ToString("R")).Append('|').Append(size.x.ToString("R")).Append(',')
                    .Append(size.y.ToString("R")).Append(',').Append(size.z.ToString("R")).Append('\n');
            }
            return builder.ToString();
        }

        static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                       81920, true))
            {
                byte[] bytes = new byte[checked((int)stream.Length)];
                int offset = 0;
                while (offset < bytes.Length)
                {
                    int read = await stream.ReadAsync(bytes, offset, bytes.Length - offset, cancellationToken);
                    if (read == 0) throw new EndOfStreamException("The temporary capture tile ended unexpectedly.");
                    offset += read;
                }
                return bytes;
            }
        }

        static bool StatusIs(string value, string name, string numeric) =>
            string.Equals(value, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, numeric, StringComparison.Ordinal);

        static string SanitizeFileName(string value)
        {
            HashSet<char> invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
            string result = new string(value.Select(character => invalid.Contains(character) ? '-' : character)
                .ToArray()).Trim();
            return string.IsNullOrWhiteSpace(result) ? "world-map" : result;
        }

        static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024L * 1024L) return (bytes / (1024d * 1024d * 1024d)).ToString("0.##") + " GB";
            if (bytes >= 1024L * 1024L) return (bytes / (1024d * 1024d)).ToString("0.##") + " MB";
            if (bytes >= 1024L) return (bytes / 1024d).ToString("0.##") + " KB";
            return bytes + " B";
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
