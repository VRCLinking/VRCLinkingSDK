using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using VRCLinkingAPI.Client;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal sealed class VrcLinkingWorldMapApi
    {
        readonly Configuration _configuration;
        readonly ApiClient _client;

        internal VrcLinkingWorldMapApi(Configuration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = new ApiClient(_configuration.BasePath);
        }

        internal async Task<WorldMapCapabilities> GetCapabilitiesAsync(
            string guildId, Guid worldId, CancellationToken cancellationToken = default)
        {
            RequestOptions options = CreateOptions(guildId, worldId);
            ApiResponse<WorldMapCapabilities> response = await _client.GetAsync<WorldMapCapabilities>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/maps/capabilities", options, _configuration,
                cancellationToken);
            ThrowIfFailed("GetWorldMapCapabilities", response);
            return response.Data;
        }

        internal async Task<List<WorldMapSummary>> GetMapsAsync(
            string guildId, Guid worldId, CancellationToken cancellationToken = default)
        {
            RequestOptions options = CreateOptions(guildId, worldId);
            ApiResponse<List<WorldMapSummary>> response = await _client.GetAsync<List<WorldMapSummary>>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/maps", options, _configuration,
                cancellationToken);
            ThrowIfFailed("GetWorldMaps", response);
            return response.Data ?? new List<WorldMapSummary>();
        }

        internal async Task<WorldMapUploadSession> CreateUploadAsync(string guildId, Guid worldId,
            CreateWorldMapSnapshotUploadRequest request, CancellationToken cancellationToken = default)
        {
            RequestOptions options = CreateOptions(guildId, worldId);
            AddJsonHeaders(options);
            options.Data = request;
            ApiResponse<WorldMapUploadSession> response = await _client.PostAsync<WorldMapUploadSession>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/map-uploads", options, _configuration,
                cancellationToken);
            ThrowIfFailed("CreateWorldMapUpload", response);
            return response.Data;
        }

        internal async Task UploadPartAsync(string guildId, Guid worldId, Guid sessionId, string partId,
            byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            string url = string.Format("{0}/guilds/{1}/worlds/{2:D}/tracking/map-uploads/{3:D}/parts/{4}",
                _configuration.BasePath.TrimEnd('/'), UnityWebRequest.EscapeURL(guildId), worldId, sessionId,
                UnityWebRequest.EscapeURL(partId));
            using (UnityWebRequest request = UnityWebRequest.Put(url, data))
            {
                request.SetRequestHeader("Content-Type", "application/octet-stream");
                request.SetRequestHeader("Accept", "application/json");
                await SendAsync(request, cancellationToken);
                if (request.result != UnityWebRequest.Result.Success)
                    throw CreateRequestException("UploadWorldMapPart", request);
            }
        }

        internal async Task<WorldMapUploadSession> FinalizeAsync(string guildId, Guid worldId,
            Guid sessionId, string manifestSha256, CancellationToken cancellationToken = default)
        {
            RequestOptions options = CreateUploadOptions(guildId, worldId, sessionId);
            AddJsonHeaders(options);
            options.Data = new FinalizeWorldMapUploadRequest { ManifestSha256 = manifestSha256 };
            ApiResponse<WorldMapUploadSession> response = await _client.PostAsync<WorldMapUploadSession>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/map-uploads/{sessionId}/finalize", options,
                _configuration, cancellationToken);
            ThrowIfFailed("FinalizeWorldMapUpload", response);
            return response.Data;
        }

        internal async Task<WorldMapUploadSession> GetUploadAsync(string guildId, Guid worldId,
            Guid sessionId, CancellationToken cancellationToken = default)
        {
            RequestOptions options = CreateUploadOptions(guildId, worldId, sessionId);
            ApiResponse<WorldMapUploadSession> response = await _client.GetAsync<WorldMapUploadSession>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/map-uploads/{sessionId}", options,
                _configuration, cancellationToken);
            ThrowIfFailed("GetWorldMapUpload", response);
            return response.Data;
        }

        internal async Task CancelAsync(string guildId, Guid worldId, Guid sessionId,
            CancellationToken cancellationToken = default)
        {
            RequestOptions options = CreateUploadOptions(guildId, worldId, sessionId);
            ApiResponse<WorldMapUploadSession> response = await _client.DeleteAsync<WorldMapUploadSession>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/map-uploads/{sessionId}", options, _configuration,
                cancellationToken);
            ThrowIfFailed("CancelWorldMapUpload", response);
        }

        static async Task SendAsync(UnityWebRequest request, CancellationToken cancellationToken)
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    request.Abort();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }
        }

        static Exception CreateRequestException(string operation, UnityWebRequest request)
        {
            string response = request.downloadHandler == null ? null : request.downloadHandler.text;
            string details = string.IsNullOrWhiteSpace(response) ? request.error : response;
            return new InvalidOperationException(
                string.Format("{0} failed ({1}): {2}", operation, request.responseCode, details));
        }

        RequestOptions CreateOptions(string guildId, Guid worldId)
        {
            if (string.IsNullOrWhiteSpace(guildId))
                throw new ArgumentException("Guild ID is required", nameof(guildId));
            if (worldId == Guid.Empty)
                throw new ArgumentException("World ID is required", nameof(worldId));

            RequestOptions options = new RequestOptions();
            options.PathParameters.Add("guildId", ClientUtils.ParameterToString(guildId));
            options.PathParameters.Add("worldId", ClientUtils.ParameterToString(worldId));
            return options;
        }

        RequestOptions CreateUploadOptions(string guildId, Guid worldId, Guid sessionId)
        {
            RequestOptions options = CreateOptions(guildId, worldId);
            if (sessionId == Guid.Empty) throw new ArgumentException("Upload session ID is required", nameof(sessionId));
            options.PathParameters.Add("sessionId", ClientUtils.ParameterToString(sessionId));
            return options;
        }

        static void AddJsonHeaders(RequestOptions options)
        {
            options.HeaderParameters.Add("Content-Type", "application/json");
            options.HeaderParameters.Add("Accept", "application/json");
        }

        static void ThrowIfFailed<T>(string operation, ApiResponse<T> response)
        {
            Exception exception = Configuration.DefaultExceptionFactory(operation, response);
            if (exception != null) throw exception;
        }
    }
}
