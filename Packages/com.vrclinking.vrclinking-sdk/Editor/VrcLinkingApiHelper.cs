using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRCLinking.Editor.Models;
using VRCLinking.Editor.Modules.AreaTracking;
using VRCLinking.Modules.Posters;
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
        UnityPosterApi _unityPosterApi;
        VrcLinkingWorldTrackingApi _worldTrackingApi;
        VrcLinkingWorldMapApi _worldMapApi;
        
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
            _unityPosterApi = new UnityPosterApi(config);
            _worldTrackingApi = new VrcLinkingWorldTrackingApi(config);
            _worldMapApi = new VrcLinkingWorldMapApi(config);
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
                
                return true;
            }
            catch (ApiException)
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

        public async Task SyncPosters(string guildId, Guid worldId, List<VrcLinkingPoster> posters)
        {
            
            var request = new SyncPostersRequest(posters.Select(p => new UnityPosterData()
            {
                SlotId = p.slotId,
                SlotName = p.slotName
            }).ToList());
            
            await _unityPosterApi.SyncPostersAsync(guildId, worldId, request);
        }

        internal Task<WorldTrackingAreasResponse> GetWorldTrackingAreas(string guildId, Guid worldId) =>
            _worldTrackingApi.GetAsync(guildId, worldId);

        internal Task<WorldTrackingAreasResponse> SyncWorldTrackingAreas(
            string guildId, Guid worldId, SyncWorldTrackingAreasRequest request) =>
            _worldTrackingApi.PutAsync(guildId, worldId, request);

        internal Task<WorldMapCapabilities> GetWorldMapCapabilities(string guildId, Guid worldId,
            CancellationToken cancellationToken = default) =>
            _worldMapApi.GetCapabilitiesAsync(guildId, worldId, cancellationToken);

        internal Task<List<WorldMapSummary>> GetWorldMaps(string guildId, Guid worldId,
            CancellationToken cancellationToken = default) =>
            _worldMapApi.GetMapsAsync(guildId, worldId, cancellationToken);

        internal Task<WorldMapUploadSession> CreateWorldMapUpload(string guildId, Guid worldId,
            CreateWorldMapSnapshotUploadRequest request, CancellationToken cancellationToken = default) =>
            _worldMapApi.CreateUploadAsync(guildId, worldId, request, cancellationToken);

        internal Task UploadWorldMapPart(string guildId, Guid worldId, Guid sessionId, string partId,
            byte[] data, CancellationToken cancellationToken = default) =>
            _worldMapApi.UploadPartAsync(guildId, worldId, sessionId, partId, data, cancellationToken);

        internal Task<WorldMapUploadSession> FinalizeWorldMapUpload(string guildId, Guid worldId,
            Guid sessionId, string manifestSha256, CancellationToken cancellationToken = default) =>
            _worldMapApi.FinalizeAsync(guildId, worldId, sessionId, manifestSha256, cancellationToken);

        internal Task<WorldMapUploadSession> GetWorldMapUpload(string guildId, Guid worldId,
            Guid sessionId, CancellationToken cancellationToken = default) =>
            _worldMapApi.GetUploadAsync(guildId, worldId, sessionId, cancellationToken);

        internal Task CancelWorldMapUpload(string guildId, Guid worldId, Guid sessionId,
            CancellationToken cancellationToken = default) =>
            _worldMapApi.CancelAsync(guildId, worldId, sessionId, cancellationToken);
        
        static Configuration GetConfiguration()
        {
            // SDK OAuth establishes a cookie-backed session shared by UnityWebRequest.
            // The token returned by that flow belongs to the legacy session system;
            // sending it as a bearer API key bypasses session loading on the backend.
            return new Configuration();
        }

        static string GetToken() => EditorPrefs.GetString(TokenEditorPrefKey, "");
    }
}
