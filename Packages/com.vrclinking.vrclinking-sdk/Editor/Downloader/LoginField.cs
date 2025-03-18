using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VRCLinking.Editor.Models;
using VRCLinkingAPI.Model;

namespace VRCLinking.Editor
{
    public class LoginField : VisualElement
    {
        // Makes the LoginField show up as an option within UI Builder.
        public new class UxmlFactory : UxmlFactory<LoginField, UxmlTraits> {}
        
        private enum LoginState
        {
            None,
            Waiting,
            Success,
            Failed,
        }

        public event Action OnLogin;
        public event Action OnLogout;
        
        private Label LoginStatus;
        private Button LoginButton;
        private Button CancelLoginButton;
        private Button LogoutButton;

        private const string LOGIN_NONE = "Not logged in.";
        private const string LOGIN_WAITING = "Logging in...";
        private const string LOGIN_SUCCESS = "Logged in.";
        private const string LOGIN_FAILED = "Failed to log in.";

        private VrcLinkingApiHelper _apiHelper;
        private LoginState _loginState;
        private double _loginStartTime;
        private readonly double _loginTimeout = 60;


        public LoginField()
        {
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("LoginField");
            uxml.CloneTree(this);
            
            // Query Elements.
            LoginStatus = this.Q<Label>(nameof(LoginStatus));
            LoginButton = this.Q<Button>(nameof(LoginButton));
            CancelLoginButton = this.Q<Button>(nameof(CancelLoginButton));
            LogoutButton = this.Q<Button>(nameof(LogoutButton));
            
            // Bind actions.
            LoginButton.clicked += OnLoginPressed;
            CancelLoginButton.clicked += OnCancelLoginPressed;
            LogoutButton.clicked += OnLogoutPressed;

            UpdateLoginState(LoginState.None);
        }

        public void BindApi(VrcLinkingApiHelper apiHelper)
        {
            _apiHelper = apiHelper;
            OnLogout?.Invoke();
            _ = CheckLogin();
        }
        
        private async Task CheckLogin()
        {
            SetEnabled(false);
            bool result = await _apiHelper.IsUserLoggedIn();

            if (result)
            {
                UpdateLoginState(LoginState.Success);
                OnLogin?.Invoke();
            }
            else
                UpdateLoginState(LoginState.None);
            SetEnabled(true);
        }
        
        private void OnLoginPressed()
        {
            UpdateLoginState(LoginState.Waiting);
            _ = AttemptLogin();
        }

        private void OnCancelLoginPressed()
        {            
            UpdateLoginState(LoginState.None);
        }

        private void OnLogoutPressed()
        {
            UpdateLoginState(LoginState.None);
            _ = _apiHelper.Logout();
            OnLogout?.Invoke();
        }
        
        /// <summary>
        /// This should only handle visible UI changes, not api interactions.
        /// </summary>
        private void UpdateLoginState(LoginState state)
        {
            _loginState = state;
            switch (_loginState)
            {
                case LoginState.None:
                    SetLoginStatusText(LOGIN_NONE);
                    LoginButton.SetDisplay(true);
                    CancelLoginButton.SetDisplay(false);
                    LogoutButton.SetDisplay(false);
                    break;
                case LoginState.Waiting:
                    SetLoginStatusText(LOGIN_WAITING);
                    LoginButton.SetDisplay(false);
                    CancelLoginButton.SetDisplay(true);
                    LogoutButton.SetDisplay(false);
                    break;
                case LoginState.Success:
                    SetLoginStatusText(LOGIN_SUCCESS);
                    LoginButton.SetDisplay(false);
                    CancelLoginButton.SetDisplay(false);
                    LogoutButton.SetDisplay(true);
                    break;
                case LoginState.Failed:
                    SetLoginStatusText(LOGIN_FAILED);
                    LoginButton.SetDisplay(true);
                    CancelLoginButton.SetDisplay(false);
                    LogoutButton.SetDisplay(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task AttemptLogin()
        {
            // Start time for the login wait timer.
            _loginStartTime = EditorApplication.timeSinceStartup;

            // Open the login screen in your browser.
            string token = await _apiHelper.GetAuthToken();
            string url = _apiHelper.GetAuthTokenUrl(token);
            Application.OpenURL(url);

            while (_loginState == LoginState.Waiting)
            {
                double waitDuration = EditorApplication.timeSinceStartup - _loginStartTime;
                int remainingSeconds = (int)(_loginTimeout - waitDuration);

                SetLoginStatusText(remainingSeconds);
                
                // If the total wait time is over the timeout duration...
                if (waitDuration >= _loginTimeout)
                {
                    UpdateLoginState(LoginState.Failed);
                    return;
                }

                // Attempt login.
                Stopwatch timer = new Stopwatch();
                timer.Start();
                (AuthStatus, SdkLoginResponse) response = await _apiHelper.TryLogin(token);
                timer.Stop();
                
                switch (response.Item1)
                {
                    case AuthStatus.Ok:
                        UpdateLoginState(LoginState.Success);
                        _apiHelper.SetToken(response.Item2.Token);
                        OnLogin?.Invoke();
                        break;
                    case AuthStatus.Retry:
                        break;
                    case AuthStatus.Failed:
                        UpdateLoginState(LoginState.Failed);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                // Delay by one second, accounting for TryLogin delay.
                await Task.Delay(1000 - timer.Elapsed.Milliseconds);
            }
        }

        /// <summary>
        /// Write to the current status.
        /// </summary>
        private void SetLoginStatusText(string text)
        {
            LoginStatus.text = text;
        }

        /// <summary>
        /// Report the time remaining for login waiting.
        /// </summary>
        private void SetLoginStatusText(int timeRemaining)
        {
            LoginStatus.text = string.Concat(LOGIN_WAITING, "  ", timeRemaining);
        }
        
    }
}