using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal sealed class WorldMapCapabilities
    {
        [JsonProperty("maximumLongEdge")] public int MaximumLongEdge { get; set; }
        [JsonProperty("maximumPixelCount")] public long MaximumPixelCount { get; set; }
        [JsonProperty("autoMinimumLongEdge")] public int AutoMinimumLongEdge { get; set; }
        [JsonProperty("autoMaximumLongEdge")] public int AutoMaximumLongEdge { get; set; }
        [JsonProperty("autoPixelsPerWorldUnit")] public int AutoPixelsPerWorldUnit { get; set; }
        [JsonProperty("captureTileSize")] public int CaptureTileSize { get; set; }
        [JsonProperty("deliveryTileSize")] public int DeliveryTileSize { get; set; }
        [JsonProperty("maximumUploadPartBytes")] public long MaximumUploadPartBytes { get; set; }
        [JsonProperty("maximumUploadBytes")] public long MaximumUploadBytes { get; set; }
        [JsonProperty("maximumPolygonPoints")] public int MaximumPolygonPoints { get; set; }
        [JsonProperty("extremeGroupImpactRatio")] public double ExtremeGroupImpactRatio { get; set; }
        [JsonProperty("maximumMapsPerWorld")] public int MaximumMapsPerWorld { get; set; }
    }

    internal sealed class WorldMapSummary
    {
        [JsonProperty("id")] public Guid Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("sourceType")] public string SourceType { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("isUsable")] public bool IsUsable { get; set; }
        [JsonProperty("isProcessingReplacement")] public bool IsProcessingReplacement { get; set; }
    }

    internal sealed class CreateWorldMapSnapshotUploadRequest
    {
        [JsonProperty("targetMapId")] public Guid? TargetMapId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("sourceType")] public string SourceType { get; set; } = "UnitySnapshot";
        [JsonProperty("width")] public int Width { get; set; }
        [JsonProperty("height")] public int Height { get; set; }
        [JsonProperty("expectedBytes")] public long ExpectedBytes { get; set; }
        [JsonProperty("areaFingerprint")] public string AreaFingerprint { get; set; }
        [JsonProperty("areaIds")] public List<Guid> AreaIds { get; set; } = new List<Guid>();
        [JsonProperty("captureMetadata")] public WorldMapCaptureMetadata CaptureMetadata { get; set; }
        [JsonProperty("manifest")] public WorldMapUploadManifestRequest Manifest { get; set; }
        [JsonProperty("proposedOverlays")] public List<WorldMapProposedOverlay> ProposedOverlays { get; set; } =
            new List<WorldMapProposedOverlay>();
    }

    internal sealed class WorldMapUploadManifestRequest
    {
        [JsonProperty("version")] public int Version { get; set; } = 1;
        [JsonProperty("originalFileName")] public string OriginalFileName { get; set; }
        [JsonProperty("contentType")] public string ContentType { get; set; } = "image/png";
        [JsonProperty("sourceSha256")] public string SourceSha256 { get; set; }
        [JsonProperty("unityTiles")] public List<WorldMapUnityTilePart> UnityTiles { get; set; } =
            new List<WorldMapUnityTilePart>();
        [JsonProperty("customChunks")] public List<object> CustomChunks { get; set; } = new List<object>();
    }

    internal sealed class WorldMapUnityTilePart
    {
        [JsonProperty("partId")] public string PartId { get; set; }
        [JsonProperty("order")] public int Order { get; set; }
        [JsonProperty("byteLength")] public long ByteLength { get; set; }
        [JsonProperty("sha256")] public string Sha256 { get; set; }
        [JsonProperty("tileX")] public int TileX { get; set; }
        [JsonProperty("tileY")] public int TileY { get; set; }
        [JsonProperty("pixelX")] public int PixelX { get; set; }
        [JsonProperty("pixelY")] public int PixelY { get; set; }
        [JsonProperty("width")] public int Width { get; set; }
        [JsonProperty("height")] public int Height { get; set; }
    }

    internal sealed class WorldMapCaptureMetadata
    {
        [JsonProperty("boundsMinX")] public double BoundsMinX { get; set; }
        [JsonProperty("boundsMinY")] public double BoundsMinY { get; set; }
        [JsonProperty("boundsMaxX")] public double BoundsMaxX { get; set; }
        [JsonProperty("boundsMaxY")] public double BoundsMaxY { get; set; }
        [JsonProperty("verticalMin")] public double? VerticalMin { get; set; }
        [JsonProperty("verticalMax")] public double? VerticalMax { get; set; }
        [JsonProperty("rotationDegrees")] public double RotationDegrees { get; set; }
        [JsonProperty("flipX")] public bool FlipX { get; set; }
        [JsonProperty("flipY")] public bool FlipY { get; set; }
        [JsonProperty("layerMask")] public int? LayerMask { get; set; }
        [JsonProperty("background")] public string Background { get; set; }
        [JsonProperty("projectionPlane")] public string ProjectionPlane { get; set; } = "XZ";
        [JsonProperty("coordinateSystem")] public string CoordinateSystem { get; set; } = "Unity world XZ; image origin top-left";
        [JsonProperty("pixelsPerWorldUnit")] public double? PixelsPerWorldUnit { get; set; }
        [JsonProperty("pixelWidth")] public int? PixelWidth { get; set; }
        [JsonProperty("pixelHeight")] public int? PixelHeight { get; set; }
        [JsonProperty("sceneName")] public string SceneName { get; set; }
        [JsonProperty("unityVersion")] public string UnityVersion { get; set; }
        [JsonProperty("captureVersion")] public string CaptureVersion { get; set; } = "VRCLinking SDK 1";
        [JsonProperty("fingerprint")] public string Fingerprint { get; set; }
        [JsonProperty("capturedAtUtc")] public DateTime? CapturedAtUtc { get; set; }
    }

    internal sealed class WorldMapProposedOverlay
    {
        [JsonProperty("areaId")] public Guid AreaId { get; set; }
        [JsonProperty("polygon")] public WorldMapPolygon Polygon { get; set; } = new WorldMapPolygon();
        [JsonProperty("labelPosition")] public WorldMapPoint LabelPosition { get; set; }
        [JsonProperty("customLabel")] public string CustomLabel { get; set; }
        [JsonProperty("polygonVisible")] public bool PolygonVisible { get; set; } = true;
        [JsonProperty("labelVisible")] public bool LabelVisible { get; set; } = true;
    }

    internal sealed class WorldMapPolygon
    {
        [JsonProperty("points")] public List<WorldMapPoint> Points { get; set; } = new List<WorldMapPoint>();
    }

    internal sealed class WorldMapPoint
    {
        internal WorldMapPoint() { }
        internal WorldMapPoint(double x, double y) { X = x; Y = y; }
        [JsonProperty("x")] public double X { get; set; }
        [JsonProperty("y")] public double Y { get; set; }
    }

    internal sealed class WorldMapUploadSession
    {
        [JsonProperty("sessionId")] public Guid SessionId { get; set; }
        [JsonProperty("mapId")] public Guid MapId { get; set; }
        [JsonProperty("generationId")] public Guid GenerationId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("expectedBytes")] public long ExpectedBytes { get; set; }
        [JsonProperty("receivedBytes")] public long ReceivedBytes { get; set; }
        [JsonProperty("manifestSha256")] public string ManifestSha256 { get; set; }
        [JsonProperty("processingError")] public string ProcessingError { get; set; }
    }

    internal sealed class FinalizeWorldMapUploadRequest
    {
        [JsonProperty("manifestSha256")] public string ManifestSha256 { get; set; }
    }
}
