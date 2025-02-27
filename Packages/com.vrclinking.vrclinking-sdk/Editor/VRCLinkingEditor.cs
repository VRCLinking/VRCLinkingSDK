using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VRCLinkingAPI.Api;
using VRCLinkingAPI.Client;
using VRCLinkingAPI.Model;

namespace VRCLinking.Editor
{
    public class VRCLinkingEditor : EditorWindow
    {
        private const string ApiBaseUrl = "http://localhost:7720/";
        private const string OauthBaseUrl = "http://localhost:3000/";
        const string TokenEditorPrefKey = "VRCLinking_ApiToken";

        private string _statusMessage = "";
        private bool _isLoading = false;
        private int _timeRemaining = 300; // 5 minutes in seconds
        private string _token = "";
        private string _username = "";
        private string _avatarUrl = "";
        private Texture2D _avatarTexture;
        
        TokenAuthApi _tokenAuthApi;
        UsersApi _usersApi;

        void OnEnable()
        {
            _token = EditorPrefs.GetString(TokenEditorPrefKey, "");
            Configuration config = new Configuration();
            config.ApiKey.Add("Bearer", _token);
            _tokenAuthApi = new TokenAuthApi(config);
            _usersApi = new UsersApi(config);
        }


        [MenuItem("Window/VRCLinking")]
        public static void ShowEditor()
        {
            var window = GetWindow<VRCLinkingEditor>();
            window.titleContent = new GUIContent("VRCLinking SDK");

            window.minSize = new Vector2(300, 300);
            window.LoadUserData();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();

            if (string.IsNullOrEmpty(EditorPrefs.GetString(TokenEditorPrefKey, "")))
            {
                if (GUILayout.Button("Log In"))
                {
                    StartLoginProcess();
                }
            }
            else
            {
                GUILayout.Label("Logged in as:", EditorStyles.boldLabel);
                if (_avatarTexture != null)
                {
                    GUILayout.Label(new GUIContent(_avatarTexture), GUILayout.Height(64), GUILayout.Width(64));
                }
                GUILayout.Label($"Username: {_username}");
                GUILayout.Label("Session active.");

                if (GUILayout.Button("Log Out"))
                {
                    LogOut();
                }
            }

            if (_isLoading)
            {
                GUILayout.Label($"Time remaining: {_timeRemaining / 60}:{_timeRemaining % 60:D2}");
                GUILayout.Label("Loading... Please wait.");
                if (GUILayout.Button("Reopen Login Page"))
                {
                    Application.OpenURL($"{OauthBaseUrl}sdk-oauth?token={_token}");
                }
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                GUI.color = Color.red;
                GUILayout.Label(_statusMessage);
                GUI.color = Color.white;
            }
        }

        private async void StartLoginProcess()
        {
            _statusMessage = "";
            _isLoading = true;
            _timeRemaining = 300;

            var token = await _tokenAuthApi.GetSdkLoginTokenAsync();
            
            if (token == null)
            {
                _statusMessage = "Error getting login token.";
                _isLoading = false;
                return;
            }
            
            _token = token.Token;
            Debug.Log($"Token: {_token}");
            
            Application.OpenURL($"{OauthBaseUrl}sdk-oauth?token={_token}");
            await PollForLoginStatus();
        }

        private async Task PollForLoginStatus()
        {
            while (_timeRemaining > 0)
            {
                await Task.Delay(5000);
                _timeRemaining -= 5;
                
                var response = await _tokenAuthApi.SdkLoginWithHttpInfoAsync(new SdkLoginRequest(_token));
                
                if (response == null)
                {
                    _statusMessage = "Error logging in.";
                    _isLoading = false;
                    return;
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var user = response.Data;
                    _username = user.Username;
                    _avatarUrl = user.Avatar;
                    await LoadAvatar(_avatarUrl);
                    EditorPrefs.SetString(TokenEditorPrefKey, response.Data.Token);
                    _statusMessage = "Logged in successfully.";
                    _isLoading = false;
                    return;
                }
            }

   
        }

        private async void LoadUserData()
        {
            if (string.IsNullOrEmpty(_token)) return;

            var user = await _usersApi.GetUserAsync();
            if (user == null)
            {
                Debug.LogError("Error loading user data.");
                return;
            }

            _username = user.Username;
            _avatarUrl = user.Avatar;
            await LoadAvatar(_avatarUrl);
        }

        private async Task LoadAvatar(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            try
            {
                Debug.Log("Downloading avatar image...");
                // var data = await Client.GetByteArrayAsync(url);
                // _avatarTexture = new Texture2D(2, 2);
                // _avatarTexture.LoadImage(data);
                Debug.Log("Avatar image loaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading avatar image: " + ex.Message);
            }
        }

        private void LogOut()
        {
            Debug.Log("Logging out...");
            EditorPrefs.DeleteKey(TokenEditorPrefKey);
            _username = "";
            _avatarUrl = "";
            _avatarTexture = null;
            _statusMessage = "Logged out successfully.";
        }
        
    }
}
