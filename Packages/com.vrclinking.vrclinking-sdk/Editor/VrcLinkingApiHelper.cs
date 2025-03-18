using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRCLinking.Editor.Models;
using VRCLinkingAPI.Api;
using VRCLinkingAPI.Client;
using VRCLinkingAPI.Model;

namespace VRCLinking.Editor
{
    public class VrcLinkingApiHelper
    {
        const string OauthBaseUrl = "https://vrclinking.com/";
        const string TokenEditorPrefKey = "VRCLinking_ApiToken";
        
        AuthApi _authApi;
        TokenAuthApi _tokenAuthApi;
        UsersApi _usersApi;
        GuildsApi _guildsApi;
        WorldsApi _worldsApi;
        
        public VrcLinkingApiHelper()
        {
            CreateNewApi();
        }
        
        void CreateNewApi()
        {
            var config = GetConfiguration();
            
            _authApi = new AuthApi(config);
            _tokenAuthApi = new TokenAuthApi(config);
            _usersApi = new UsersApi(config);
            _guildsApi = new GuildsApi(config);
            _worldsApi = new WorldsApi(config);
        }

        public void SetToken(string token)
        {
            EditorPrefs.SetString(TokenEditorPrefKey, token);
            CreateNewApi();
        }
        
        public async Task<List<LimitedGuild>> GetAvailableGuilds()
        {
            var guilds = await _guildsApi.GetGuildListAsync();
            return guilds.Guilds;
        }
        
        public async Task<Guild> GetGuild(string guildId)
        {
            return await _guildsApi.GetGuildAsync(guildId);
        }
        
        public async Task<List<WorldSettingsDto>> GetWorldSettingsList(string guildId)
        {
            var settings = await _worldsApi.GetGuildWorldsAsync(guildId);
            return settings.Worlds;
        }
        
        public async Task<WorldSettingsDto> GetWorldSettings(string guildId, Guid worldId)
        {
            var settings = await _worldsApi.GetGuildWorldAsync(guildId, worldId);
            return settings;
        }
        
        public async Task<User> GetCurrentUser()
        {
            return await _usersApi.GetUserAsync();
        }
        
        public async Task<bool> IsUserLoggedIn()
        {
            try
            {
                if (string.IsNullOrEmpty(GetToken()))
                {
                    return false;
                }

                var user = await GetCurrentUser();
                if (string.IsNullOrEmpty(user.Id) || user.Id == "0")
                {
                    return false;
                }
                
                Debug.Log(user.Id);
                return true;
            }
            catch (ApiException e)
            {
                return false;
            }
        }
        
        public async Task<string> GetAuthToken()
        {
            var token = await _tokenAuthApi.GetSdkLoginTokenAsync();
            return token.Token;
        }

        public string GetAuthTokenUrl(string token)
        {
            return $"{OauthBaseUrl}sdk-oauth?token={token}";
        }

        public async Task<(AuthStatus, SdkLoginResponse)> TryLogin(string token)
        {
            var response = await _tokenAuthApi.SdkLoginWithHttpInfoAsync(new SdkLoginRequest(token));
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var status = response.Data.Status;
                switch (status)
                {
                    case TokenAuthStatus.Ok:
                        return (AuthStatus.Ok, response.Data);
                    case TokenAuthStatus.TokenNotAuthorizedYet:
                        return (AuthStatus.Retry, null);
                }
            }

            return (AuthStatus.Failed, null);
        }
        
        public async Task Logout()
        {
            await _authApi.LogoutAsync();   
            EditorPrefs.DeleteKey(TokenEditorPrefKey);
        }

        public async Task<List<EncodeRole>> GetAllEncodeRoles(string guildId)
        {
            var guild = await _guildsApi.GetGuildAsync(guildId);
            var roles = guild.EncRoleList;
            foreach (var alwaysEncRole in guild.AlwaysEncRoles)
            {
                if (roles.All(r => r.Id != alwaysEncRole.Id))
                {
                    roles.Add(alwaysEncRole);
                }
            }
            
            return roles;
        }
        
        static Configuration GetConfiguration()
        {
            Configuration config = new Configuration();
            config.ApiKey.Add("Bearer", GetToken());
            
            return config;
        }

        static string GetToken() => EditorPrefs.GetString(TokenEditorPrefKey, "");
    }
}