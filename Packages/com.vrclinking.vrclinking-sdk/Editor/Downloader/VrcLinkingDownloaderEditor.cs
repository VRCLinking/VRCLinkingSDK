using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;

namespace VRCLinking.Editor
{
    [Serializable]
    public struct ServerWorldData
    {
        public string ServerName;
        public string ServerId;
        public string WorldName;
        public Guid WorldId;

        public static ServerWorldData FillFromScript(VrcLinkingDownloader script)
        {
            ServerWorldData data = new ServerWorldData
            {
                ServerName = script.serverName,
                ServerId = script.serverId,
                WorldName = script.worldName,
                WorldId = script.worldId
            };
            return data;
        }

        /// <summary>
        /// If any values aren't populated...
        /// </summary>
        public bool AnyEmpty()
        {
            return string.IsNullOrEmpty(ServerName) ||
                   string.IsNullOrEmpty(ServerId) ||
                   string.IsNullOrEmpty(ServerName) ||
                   string.IsNullOrEmpty(ServerId);
        }
    }
    
    [CustomEditor(typeof(VrcLinkingDownloader))]
    public class VrcLinkingDownloaderEditor : UnityEditor.Editor
    {
        private VrcLinkingDownloader _script;

        private VisualElement _root;
        private LoginField _loginField;
        private ServerWorldSelection _serverWorldSelection;
        private SelectedWorldField _selectedWorldField;

        private VrcLinkingApiHelper _apiHelper;


        private void OnEnable()
        {
            _script = target as VrcLinkingDownloader;
            _apiHelper = new VrcLinkingApiHelper();
        }

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("VrcLinkingDownloaderEditor");
            uxml.CloneTree(_root);

            QueryElements();
            BindElements();

            return _root;
        }

        private void QueryElements()
        {
            _loginField = _root.Q<LoginField>();
            _serverWorldSelection = _root.Q<ServerWorldSelection>();
            _selectedWorldField = _root.Q<SelectedWorldField>();
        }

        private void BindElements()
        {
            _loginField.OnLogin += OnLogin;
            _loginField.OnLogout += OnLogout;

            _serverWorldSelection.OnValuesChanged += OnValuesChanged;
            _serverWorldSelection.BindApi(_apiHelper);
            _serverWorldSelection.SetSelectedValues(
                _script.serverName,
                _script.worldName);

            _selectedWorldField.BindApi(_apiHelper);

            PreviewSelectedWorldField(ServerWorldData.FillFromScript(_script));
            
            // Done last to run callbacks.
            _loginField.BindApi(_apiHelper);
        }

        private void OnValuesChanged()
        {
            // Preview in the SelectedWorldField.
            ServerWorldData worldData = _serverWorldSelection.GetWorldData();

            PreviewSelectedWorldField(worldData);

            // Write data to the script.
            Undo.RecordObject(_script, "Updated WorldId");

            _script.serverName = worldData.ServerName;
            _script.serverId = worldData.ServerId;
            _script.worldName = worldData.WorldName; 
            _script.worldId = worldData.WorldId;
            _script.mainUrl = new VRCUrl("https://data.vrclinking.com/v2/" + worldData.WorldId);
            _script.fallbackUrl = new VRCUrl("https://linkingbotvrchat.github.io/v2data/worlds/" + worldData.WorldId);

            PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
        }

        private void OnLogin()
        {
            _serverWorldSelection.SetEnabled(true);
            _ = _serverWorldSelection.PopulateServers();
            
            PreviewSelectedWorldField(ServerWorldData.FillFromScript(_script));
        }

        private void OnLogout()
        {
            _serverWorldSelection.SetEnabled(false);
        }

        /// <summary>
        /// Send world data to the SelectedWorldField.
        /// </summary>
        private void PreviewSelectedWorldField(ServerWorldData worldData)
        {
            _ = _selectedWorldField.FillData(worldData);
        }
    }
}