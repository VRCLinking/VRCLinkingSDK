using System;
using System.Threading.Tasks;
using VRCLinkingAPI.Client;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal sealed class VrcLinkingWorldTrackingApi
    {
        readonly Configuration _configuration;
        readonly ApiClient _client;

        public VrcLinkingWorldTrackingApi(Configuration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = new ApiClient(_configuration.BasePath);
        }

        public async Task<WorldTrackingAreasResponse> GetAsync(string guildId, Guid worldId)
        {
            RequestOptions options = CreateOptions(guildId, worldId);
            ApiResponse<WorldTrackingAreasResponse> response = await _client.GetAsync<WorldTrackingAreasResponse>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/areas", options, _configuration);
            ThrowIfFailed("GetWorldTrackingAreas", response);
            return response.Data;
        }

        public async Task<WorldTrackingAreasResponse> PutAsync(
            string guildId, Guid worldId, SyncWorldTrackingAreasRequest request)
        {
            RequestOptions options = CreateOptions(guildId, worldId);
            options.HeaderParameters.Add("Content-Type", "application/json");
            options.HeaderParameters.Add("Accept", "application/json");
            options.Data = request;
            ApiResponse<WorldTrackingAreasResponse> response = await _client.PutAsync<WorldTrackingAreasResponse>(
                "/guilds/{guildId}/worlds/{worldId}/tracking/areas", options, _configuration);
            ThrowIfFailed("SyncWorldTrackingAreas", response);
            return response.Data;
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

        static void ThrowIfFailed<T>(string operation, ApiResponse<T> response)
        {
            Exception exception = Configuration.DefaultExceptionFactory(operation, response);
            if (exception != null)
                throw exception;
        }
    }
}
