using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using VRCLinking.Editor.Models;
using VRCLinkingAPI.Model;

namespace VRCLinking.Editor
{
    [CustomEditor(typeof(VrcLinkingDownloader))]
    public class VrcLinkingDownloaderEditor : UnityEditor.Editor
    {
        private enum LoginState
        {
            None,
            Waiting,
            Success,
            Failed,
        }

        private Label LoginStatus;
        private Button LoginButton;
        private Button CancelLoginButton;
        private Button LogoutButton;

        private DropdownField ServersField;
        private DropdownField WorldsField;

        private VisualElement ServerIconField;
        private Label ServerNameLabel;
        private Label WorldNameLabel;

        private VisualElement _root;

        private LoginState _loginState;
        private double _loginStartTime;
        private float _loginTimeout = 60;

        private const string LOGIN_NONE = "Not logged in.";
        private const string LOGIN_WAITING = "Logging in...";
        private const string LOGIN_SUCCESS = "Logged in.";
        private const string LOGIN_FAILED = "Failed to log in.";

        private List<string> _serverNames = new List<string>();
        private List<string> _serverIds = new List<string>();
        private List<string> _worldNames = new List<string>();
        private List<Guid> _worldIds = new List<Guid>();

        private VrcLinkingApiHelper _apiHelper;
        private VrcLinkingDownloader _script;


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

            QueryComponents();

            BindComponents();

            _ = CheckLogin();

            return _root;
        }

        private void QueryComponents()
        {
            LoginStatus = _root.Q<Label>(nameof(LoginStatus));
            LoginButton = _root.Q<Button>(nameof(LoginButton));
            CancelLoginButton = _root.Q<Button>(nameof(CancelLoginButton));
            LogoutButton = _root.Q<Button>(nameof(LogoutButton));

            ServersField = _root.Q<DropdownField>(nameof(ServersField));
            WorldsField = _root.Q<DropdownField>(nameof(WorldsField));

            ServerIconField = _root.Q<VisualElement>(nameof(ServerIconField));
            ServerNameLabel = _root.Q<Label>(nameof(ServerNameLabel));
            WorldNameLabel = _root.Q<Label>(nameof(WorldNameLabel));
        }

        private void BindComponents()
        {
            LoginButton.clicked += () => SetLoginState(LoginState.Waiting);
            CancelLoginButton.clicked += () => SetLoginState(LoginState.Failed);
            LogoutButton.clicked += () =>
            {
                _ = _apiHelper.Logout();
                SetLoginState(LoginState.None);
            };

            ServersField.choices = _serverNames;
            ServersField.RegisterValueChangedCallback(evt =>
            {
                ClearWorldInfo();

                WorldsField.SetValueWithoutNotify(null);
                Undo.RecordObject(_script, "Cleared WorldId");
                _script.serverId = string.Empty;
                _script.worldId = Guid.Empty;
                PrefabUtility.RecordPrefabInstancePropertyModifications(_script);
                
                _ = PopulateWorlds();
            });

            WorldsField.choices = _worldNames;
            WorldsField.SetEnabled(false);
            WorldsField.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(_script, "Updated WorldId");

                _script.serverId = _serverIds[ServersField.index];
                _script.serverName = _serverNames[ServersField.index];
                _script.worldId = _worldIds[WorldsField.index];
                _script.worldName = _worldNames[WorldsField.index];
                
                PrefabUtility.RecordPrefabInstancePropertyModifications(_script);

                _ = FillServerInfo();
            });
        }

        private async Task CheckLogin()
        {
            bool result = await _apiHelper.IsUserLoggedIn();

            if (result)
            {
                SetLoginState(LoginState.Success);
            }
            else
                SetLoginState(LoginState.None);
            
            await PopulateFromSavedData();
        }

        private async Task PopulateFromSavedData()
        {
            await PopulateServers();

            if (!string.IsNullOrEmpty(_script.serverId))
            {
                int serverIndex = _serverIds.IndexOf(_script.serverId);
                
                ServersField.SetValueWithoutNotify(serverIndex == -1 ?
                    _script.serverName : _serverNames[serverIndex]);

                await PopulateWorlds();

                int worldIndex = _worldIds.IndexOf(_script.worldId);

                WorldsField.SetValueWithoutNotify(worldIndex == -1 ?
                    _script.worldName : _worldNames[worldIndex]);
            }
        }

        private async Task AttemptSignIn()
        {
            _loginStartTime = EditorApplication.timeSinceStartup;

            string token = await _apiHelper.GetAuthToken();

            Application.OpenURL(_apiHelper.GetAuthTokenUrl(token));

            while (_loginState == LoginState.Waiting)
            {
                double totalDuration = EditorApplication.timeSinceStartup - _loginStartTime;

                int remainingSeconds = (int)(_loginTimeout - totalDuration);
                LoginStatus.text = string.Concat(LOGIN_WAITING, "  ", remainingSeconds);

                if (totalDuration >= _loginTimeout)
                {
                    SetLoginState(LoginState.Failed);
                    return;
                }

                (AuthStatus, SdkLoginResponse) response = await _apiHelper.TryLogin(token);
                switch (response.Item1)
                {
                    case AuthStatus.Ok:
                        SetLoginState(LoginState.Success);
                        _apiHelper.SetToken(response.Item2.Token);
                        break;
                    case AuthStatus.Retry:
                        break;
                    case AuthStatus.Failed:
                        SetLoginState(LoginState.Failed);
                        return;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await Task.Delay(1000);
            }
        }

        private void SetLoginState(LoginState state)
        {
            _loginState = state;
            switch (_loginState)
            {
                default:
                case LoginState.None:
                    LoginStatus.text = LOGIN_NONE;
                    LoginButton.SetDisplay(true);
                    CancelLoginButton.SetDisplay(false);
                    LogoutButton.SetDisplay(false);
                    break;
                case LoginState.Waiting:
                    LoginStatus.text = LOGIN_WAITING;
                    LoginButton.SetDisplay(false);
                    CancelLoginButton.SetDisplay(true);
                    LogoutButton.SetDisplay(false);
                    _ = AttemptSignIn();
                    break;
                case LoginState.Success:
                    LoginStatus.text = LOGIN_SUCCESS;
                    LoginButton.SetDisplay(false);
                    CancelLoginButton.SetDisplay(false);
                    LogoutButton.SetDisplay(true);
                    _ = PopulateServers();
                    _ = FillServerInfo();
                    break;
                case LoginState.Failed:
                    LoginStatus.text = LOGIN_FAILED;
                    LoginButton.SetDisplay(true);
                    CancelLoginButton.SetDisplay(false);
                    LogoutButton.SetDisplay(false);
                    break;
            }
        }

        private async Task PopulateServers()
        {
            List<LimitedGuild> guilds = await _apiHelper.GetAvailableGuilds();
            _serverNames.Clear();
            _serverIds.Clear();
            foreach (LimitedGuild guild in guilds)
            {
                _serverNames.Add(guild.Name);
                _serverIds.Add(guild.Id);
            }
        }

        private async Task PopulateWorlds()
        {
            ServersField.SetEnabled(false);
            WorldsField.SetEnabled(false);

            string id = _serverIds[ServersField.index];
            List<WorldSettingsDto> worlds = await _apiHelper.GetWorldSettingsList(id);

            _worldNames.Clear();
            _worldIds.Clear();
            foreach (WorldSettingsDto world in worlds)
            {
                _worldNames.Add(world.Name);
                _worldIds.Add(world.Id);
            }

            ServersField.SetEnabled(true);
            WorldsField.SetEnabled(true);
        }

        private void ClearWorldInfo()
        {
            ServerIconField.style.backgroundImage = null;
            ServerNameLabel.text = string.Empty;
            WorldNameLabel.text = string.Empty;
        }

        private async Task FillServerInfo()
        {
            if (string.IsNullOrEmpty(_script.serverId)) return;

            Guild guild = await _apiHelper.GetGuild(_script.serverId);

            using (HttpClient client = new HttpClient())
            {
                byte[] imageByteData = await client.GetByteArrayAsync(guild.Icon);
                Texture2D icon = new Texture2D(0, 0);
                icon.LoadImage(imageByteData);

                ServerIconField.style.backgroundImage = icon;
            }

            ServerNameLabel.text = guild.Name;

            WorldSettingsDto worldSettings = await _apiHelper.GetWorldSettings(_script.serverId, _script.worldId);
            WorldNameLabel.text = worldSettings.Name;
        }
    }
}