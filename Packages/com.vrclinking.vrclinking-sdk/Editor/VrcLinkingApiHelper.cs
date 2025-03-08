// using System.Collections.Generic;
// using System.Net;
// using System.Threading.Tasks;
// using UnityEditor;
// using VRCLinking.Editor.Models;
// using VRCLinkingAPI.Api;
// using VRCLinkingAPI.Client;
// using VRCLinkingAPI.Model;
//
// namespace VRCLinking.Editor
// {
//     public class VrcLinkingApiHelper
//     {
//         const string OauthBaseUrl = "http://localhost:3000/";
//         const string TokenEditorPrefKey = "VRCLinking_ApiToken";
//         
//         TokenAuthApi _tokenAuthApi;
//         readonly UsersApi _usersApi;
//         readonly GuildsApi _guildsApi;
//         readonly WorldsApi _worldsApi;
//         
//         public VrcLinkingApiHelper()
//         {
//             var config = GetConfiguration();
//             
//             _tokenAuthApi = new TokenAuthApi(config);
//             _usersApi = new UsersApi(config);
//             _guildsApi = new GuildsApi(config);
//             _worldsApi = new WorldsApi(config);
//         }
//
//         
//         public async Task<List<LimitedGuild>> GetAvailableGuilds()
//         {
//             var guilds = await _guildsApi.GetGuildListAsync();
//             return guilds.Guilds;
//         }
//         
//         public async Task<List<WorldSettingsDto>> GetWorldSettingsList(string guildId)
//         {
//             var settings = await _worldsApi.GetGuildWorldsAsync(guildId);
//             return settings.Worlds;
//         }
//         
//         public async Task<WorldSettingsDto> GetWorldSettings(string guildId, int worldId)
//         {
//             var settings = await _worldsApi.GetGuildWorldAsync(guildId, worldId);
//             return settings;
//         }
//         
//         public async Task<User> GetCurrentUser()
//         {
//             return await _usersApi.GetUserAsync();
//         }
//         
//         public async Task<string> GetTokenUrl()
//         {
//             var token = await _tokenAuthApi.GetSdkLoginTokenAsync();
//             return $"{OauthBaseUrl}sdk-oauth?token={token.Token}";
//         }
//
//         public async Task<(AuthStatus, SdkLoginResponse)> TryLogin(string token)
//         {
//             var response = await _tokenAuthApi.SdkLoginWithHttpInfoAsync(new SdkLoginRequest(token));
//             
//             if (response.StatusCode == HttpStatusCode.OK)
//             {
//                 var status = response.Data.Status;
//                 switch (status)
//                 {
//                     case TokenAuthStatus.Ok:
//                         return (AuthStatus.Ok, response.Data);
//                     case TokenAuthStatus.TokenNotAuthorizedYet:
//                         return (AuthStatus.Retry, null);
//                 }
//             }
//
//             return (AuthStatus.Failed, null);
//         }
//         
//         static Configuration GetConfiguration()
//         {
//             Configuration config = new Configuration();
//             config.ApiKey.Add("Bearer", GetToken());
//             
//             return config;
//         }
//
//         static string GetToken() => EditorPrefs.GetString(TokenEditorPrefKey, "");
//     }
// }