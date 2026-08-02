using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal sealed class WorldTrackingAreasResponse
    {
        [JsonProperty("worldId")] public Guid WorldId { get; set; }
        [JsonProperty("trackingEnabled")] public bool TrackingEnabled { get; set; }
        [JsonProperty("trackingConfiguredAtUtc")] public DateTime? TrackingConfiguredAtUtc { get; set; }
        [JsonProperty("areas")] public List<WorldTrackingAreaResponse> Areas { get; set; } = new List<WorldTrackingAreaResponse>();
    }

    internal sealed class WorldTrackingAreaResponse
    {
        [JsonProperty("id")] public Guid Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("displayOrder")] public int DisplayOrder { get; set; }
        [JsonProperty("isActive")] public bool IsActive { get; set; }
        [JsonProperty("isOutside")] public bool IsOutside { get; set; }
    }

    internal sealed class SyncWorldTrackingAreasRequest
    {
        [JsonProperty("trackingEnabled")] public bool TrackingEnabled { get; set; }
        [JsonProperty("areas")] public List<SyncWorldTrackingAreaRequest> Areas { get; set; } = new List<SyncWorldTrackingAreaRequest>();
    }

    internal sealed class SyncWorldTrackingAreaRequest
    {
        [JsonProperty("id")] public Guid Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("displayOrder")] public int DisplayOrder { get; set; }
    }
}
