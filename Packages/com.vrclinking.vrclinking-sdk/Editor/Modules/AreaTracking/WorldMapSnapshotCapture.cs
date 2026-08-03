using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRCLinking.Editor.Modules.AreaTracking
{
    internal sealed class WorldMapSnapshotTile
    {
        internal string PartId { get; set; }
        internal string FilePath { get; set; }
        internal int Order { get; set; }
        internal int TileX { get; set; }
        internal int TileY { get; set; }
        internal int PixelX { get; set; }
        internal int PixelY { get; set; }
        internal int Width { get; set; }
        internal int Height { get; set; }
        internal long ByteLength { get; set; }
        internal string Sha256 { get; set; }
    }

    internal sealed class WorldMapSnapshotPlan
    {
        internal IReadOnlyList<AreaTrackingEntry> Areas { get; set; }
        internal Bounds WorldBounds { get; set; }
        internal int PixelWidth { get; set; }
        internal int PixelHeight { get; set; }
        internal int TileSize { get; set; }
        internal int Columns { get; set; }
        internal int Rows { get; set; }
        internal int LayerMask { get; set; }
        internal Color Background { get; set; }
        internal double PixelsPerWorldUnit { get; set; }
    }

    internal sealed class WorldMapSnapshotPackage : IDisposable
    {
        internal WorldMapSnapshotPlan Plan { get; set; }
        internal List<WorldMapSnapshotTile> Tiles { get; set; } = new List<WorldMapSnapshotTile>();
        internal List<WorldMapProposedOverlay> Overlays { get; set; } = new List<WorldMapProposedOverlay>();
        internal string TemporaryDirectory { get; set; }
        internal long ExpectedBytes => Tiles.Sum(tile => tile.ByteLength);

        public void Dispose()
        {
            if (string.IsNullOrWhiteSpace(TemporaryDirectory) || !Directory.Exists(TemporaryDirectory)) return;
            try { Directory.Delete(TemporaryDirectory, true); }
            catch (Exception exception) { Debug.LogWarning("Could not remove temporary VRC Linking capture: " + exception.Message); }
        }
    }

    internal static class WorldMapSnapshotCapture
    {
        const float MinimumSpan = 0.01f;

        internal static HashSet<Guid> FindRecommendedAreaIds(
            IReadOnlyList<AreaTrackingEntry> areas, double extremeImpactRatio)
        {
            HashSet<Guid> all = new HashSet<Guid>(areas.Where(IsCapturable).Select(area => area.ParsedId));
            if (all.Count < 2) return all;

            List<AreaExtent> extents = areas.Where(IsCapturable).Select(GetExtent).ToList();
            if (extents.Count == 2)
            {
                float span = Mathf.Max(MinimumSpan, extents.Max(extent => Mathf.Max(extent.Width, extent.Depth)));
                float limit = span * Mathf.Max(8f, (float)extremeImpactRatio * 4f);
                if (Vector2.Distance(extents[0].Center, extents[1].Center) <= limit) return all;
                AreaExtent primary = extents.OrderByDescending(extent => extent.Width * extent.Depth)
                    .ThenBy(extent => extent.Entry.ParsedId).First();
                return new HashSet<Guid> { primary.Entry.ParsedId };
            }
            float medianX = Median(extents.Select(extent => extent.Center.x));
            float medianZ = Median(extents.Select(extent => extent.Center.y));
            Vector2 medianCenter = new Vector2(medianX, medianZ);
            List<float> spans = extents.Select(extent => Mathf.Max(extent.Width, extent.Depth))
                .Where(value => value > MinimumSpan).OrderBy(value => value).ToList();
            float typicalSpan = spans.Count == 0 ? 1f : spans[spans.Count / 2];
            List<float> distances = extents.Select(extent => Vector2.Distance(extent.Center, medianCenter))
                .OrderBy(value => value).ToList();
            // The lower quartile represents the dense body even when several equally distant outliers exist.
            float typicalDistance = Mathf.Max(typicalSpan, distances[Math.Max(0, (distances.Count - 1) / 4)]);
            float threshold = typicalDistance * Mathf.Max(8f, (float)extremeImpactRatio * 4f);

            HashSet<Guid> recommended = new HashSet<Guid>(extents
                .Where(extent => Vector2.Distance(extent.Center, medianCenter) <= threshold)
                .Select(extent => extent.Entry.ParsedId));
            return recommended.Count == 0 ? all : recommended;
        }

        internal static bool SelectionHasExtremeSpread(IReadOnlyList<AreaTrackingEntry> areas,
            IReadOnlyCollection<Guid> selectedIds, double extremeImpactRatio)
        {
            List<AreaTrackingEntry> selected = areas
                .Where(area => IsCapturable(area) && selectedIds.Contains(area.ParsedId)).ToList();
            if (selected.Count < 2) return false;
            HashSet<Guid> recommended = FindRecommendedAreaIds(selected, extremeImpactRatio);
            return recommended.Count != selected.Count;
        }

        internal static WorldMapSnapshotPlan CreatePlan(IReadOnlyList<AreaTrackingEntry> areas,
            IReadOnlyCollection<Guid> selectedIds, WorldMapCapabilities capabilities, int requestedLongEdge,
            float paddingPercent, int layerMask, Color background)
        {
            if (capabilities == null) throw new ArgumentNullException(nameof(capabilities));
            List<AreaTrackingEntry> selected = areas
                .Where(area => IsCapturable(area) && selectedIds.Contains(area.ParsedId)).ToList();
            if (selected.Count == 0) throw new InvalidOperationException("Select at least one synchronized area.");

            Bounds bounds = GetBounds(selected);
            bounds = ExpandVerticalBounds(bounds, layerMask);
            float padding = Mathf.Max(bounds.size.x, bounds.size.z) * Mathf.Clamp(paddingPercent, 0f, 100f) / 100f;
            bounds.Expand(new Vector3(padding * 2f, 0f, padding * 2f));
            if (bounds.size.x < MinimumSpan || bounds.size.z < MinimumSpan)
                throw new InvalidOperationException("The selected areas do not define a usable XZ capture size.");

            int maximumLongEdge = Math.Max(1, capabilities.MaximumLongEdge);
            int longEdge;
            if (requestedLongEdge <= 0)
            {
                int automatic = Mathf.RoundToInt(Mathf.Max(bounds.size.x, bounds.size.z) *
                                                 Math.Max(1, capabilities.AutoPixelsPerWorldUnit));
                longEdge = Mathf.Clamp(automatic, Math.Max(1, capabilities.AutoMinimumLongEdge),
                    Math.Min(maximumLongEdge, Math.Max(1, capabilities.AutoMaximumLongEdge)));
            }
            else
            {
                longEdge = Mathf.Clamp(requestedLongEdge, 1, maximumLongEdge);
            }

            float aspect = bounds.size.x / bounds.size.z;
            int width = aspect >= 1f ? longEdge : Mathf.Max(1, Mathf.RoundToInt(longEdge * aspect));
            int height = aspect >= 1f ? Mathf.Max(1, Mathf.RoundToInt(longEdge / aspect)) : longEdge;
            long maximumPixels = Math.Max(1, capabilities.MaximumPixelCount);
            long pixelCount = (long)width * height;
            if (pixelCount > maximumPixels)
            {
                double scale = Math.Sqrt((double)maximumPixels / pixelCount);
                width = Math.Max(1, (int)Math.Floor(width * scale));
                height = Math.Max(1, (int)Math.Floor(height * scale));
            }

            int tileSize = Math.Max(1, capabilities.CaptureTileSize);
            if (tileSize > SystemInfo.maxTextureSize)
                throw new InvalidOperationException(string.Format(
                    "The server capture tile size ({0}px) exceeds this GPU's texture limit ({1}px).",
                    tileSize, SystemInfo.maxTextureSize));

            return new WorldMapSnapshotPlan
            {
                Areas = selected,
                WorldBounds = bounds,
                PixelWidth = width,
                PixelHeight = height,
                TileSize = tileSize,
                Columns = DivideRoundUp(width, tileSize),
                Rows = DivideRoundUp(height, tileSize),
                LayerMask = layerMask,
                Background = background,
                PixelsPerWorldUnit = width / (double)bounds.size.x
            };
        }

        internal static async Task<WorldMapSnapshotPackage> CaptureAsync(WorldMapSnapshotPlan plan,
            Action<string, float> reportProgress, CancellationToken cancellationToken)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            string temporaryDirectory = Path.Combine(Path.GetTempPath(), "VRCLinkingMapCapture",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            WorldMapSnapshotPackage package = new WorldMapSnapshotPackage
            {
                Plan = plan,
                TemporaryDirectory = temporaryDirectory,
                Overlays = BuildOverlays(plan)
            };

            GameObject cameraObject = null;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                cameraObject = EditorUtility.CreateGameObjectWithHideFlags(
                    "VRC Linking Map Capture", HideFlags.HideAndDontSave, typeof(Camera));
                Camera camera = cameraObject.GetComponent<Camera>();
                ConfigureCamera(camera, plan);
                int tileCount = checked(plan.Columns * plan.Rows);
                int order = 0;
                for (int tileY = 0; tileY < plan.Rows; tileY++)
                for (int tileX = 0; tileX < plan.Columns; tileX++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int pixelX = tileX * plan.TileSize;
                    int pixelY = tileY * plan.TileSize;
                    int width = Math.Min(plan.TileSize, plan.PixelWidth - pixelX);
                    int height = Math.Min(plan.TileSize, plan.PixelHeight - pixelY);
                    string partId = string.Format("tile-{0:D3}-{1:D3}", tileY, tileX);
                    string path = Path.Combine(temporaryDirectory, partId + ".png");
                    reportProgress?.Invoke(string.Format("Capturing tile {0} of {1}…", order + 1, tileCount),
                        order / (float)tileCount);

                    PositionCameraForTile(camera, plan, pixelX, pixelY, width, height);
                    byte[] png = RenderTile(camera, width, height, plan.Background);
                    await WriteAllBytesAsync(path, png, cancellationToken);
                    package.Tiles.Add(new WorldMapSnapshotTile
                    {
                        PartId = partId,
                        FilePath = path,
                        Order = order,
                        TileX = tileX,
                        TileY = tileY,
                        PixelX = pixelX,
                        PixelY = pixelY,
                        Width = width,
                        Height = height,
                        ByteLength = png.LongLength,
                        Sha256 = ComputeSha256(png)
                    });
                    order++;
                    await Task.Yield();
                }
                reportProgress?.Invoke("Capture complete.", 1f);
                return package;
            }
            catch
            {
                package.Dispose();
                throw;
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }

        internal static string ComputeAreaFingerprint(IEnumerable<Guid> areaIds)
        {
            string canonical = string.Join("\n", areaIds.OrderBy(id => id).Select(id => id.ToString("N")));
            using (SHA256 sha256 = SHA256.Create())
                return ToHex(sha256.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
        }

        internal static WorldMapCaptureMetadata BuildMetadata(WorldMapSnapshotPlan plan, string areaFingerprint)
        {
            Bounds bounds = plan.WorldBounds;
            return new WorldMapCaptureMetadata
            {
                BoundsMinX = bounds.min.x,
                BoundsMinY = bounds.min.z,
                BoundsMaxX = bounds.max.x,
                BoundsMaxY = bounds.max.z,
                VerticalMin = bounds.min.y,
                VerticalMax = bounds.max.y,
                LayerMask = plan.LayerMask,
                Background = "#" + ColorUtility.ToHtmlStringRGBA(plan.Background),
                PixelsPerWorldUnit = plan.PixelsPerWorldUnit,
                PixelWidth = plan.PixelWidth,
                PixelHeight = plan.PixelHeight,
                SceneName = SceneManager.GetActiveScene().name,
                UnityVersion = Application.unityVersion,
                Fingerprint = areaFingerprint,
                CapturedAtUtc = DateTime.UtcNow
            };
        }

        static List<WorldMapProposedOverlay> BuildOverlays(WorldMapSnapshotPlan plan)
        {
            Bounds bounds = plan.WorldBounds;
            return plan.Areas.Select(area =>
            {
                List<Vector2> hull = ConvexHull(GetWorldCorners(area.Collider)
                    .Select(point => new Vector2(point.x, point.z)).ToList());
                List<WorldMapPoint> points = hull.Select(point => Normalize(point, bounds)).ToList();
                Vector3 center = area.Collider.transform.TransformPoint(area.Collider.center);
                return new WorldMapProposedOverlay
                {
                    AreaId = area.ParsedId,
                    Polygon = new WorldMapPolygon { Points = points },
                    LabelPosition = Normalize(new Vector2(center.x, center.z), bounds)
                };
            }).ToList();
        }

        static WorldMapPoint Normalize(Vector2 point, Bounds bounds)
        {
            double x = Mathf.Clamp01((point.x - bounds.min.x) / bounds.size.x);
            double y = Mathf.Clamp01((bounds.max.z - point.y) / bounds.size.z);
            return new WorldMapPoint(x, y);
        }

        static List<Vector2> ConvexHull(List<Vector2> points)
        {
            List<Vector2> sorted = points.Distinct(new ApproximateVectorComparer())
                .OrderBy(point => point.x).ThenBy(point => point.y).ToList();
            if (sorted.Count <= 2) return sorted;
            List<Vector2> lower = new List<Vector2>();
            foreach (Vector2 point in sorted)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], point) <= 0f)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(point);
            }
            List<Vector2> upper = new List<Vector2>();
            for (int index = sorted.Count - 1; index >= 0; index--)
            {
                Vector2 point = sorted[index];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], point) <= 0f)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(point);
            }
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            return lower;
        }

        static void ConfigureCamera(Camera camera, WorldMapSnapshotPlan plan)
        {
            camera.enabled = false;
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = plan.Background;
            camera.cullingMask = plan.LayerMask;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.useOcclusionCulling = false;
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            float verticalMargin = Mathf.Max(10f, plan.WorldBounds.size.y * 0.1f);
            camera.transform.position = new Vector3(plan.WorldBounds.center.x,
                plan.WorldBounds.max.y + verticalMargin, plan.WorldBounds.center.z);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = Mathf.Max(100f, plan.WorldBounds.size.y + verticalMargin * 2f);
        }

        static void PositionCameraForTile(Camera camera, WorldMapSnapshotPlan plan,
            int pixelX, int pixelY, int width, int height)
        {
            Bounds bounds = plan.WorldBounds;
            float minX = bounds.min.x + bounds.size.x * (pixelX / (float)plan.PixelWidth);
            float maxX = bounds.min.x + bounds.size.x * ((pixelX + width) / (float)plan.PixelWidth);
            float maxZ = bounds.max.z - bounds.size.z * (pixelY / (float)plan.PixelHeight);
            float minZ = bounds.max.z - bounds.size.z * ((pixelY + height) / (float)plan.PixelHeight);
            camera.transform.position = new Vector3((minX + maxX) * 0.5f,
                camera.transform.position.y, (minZ + maxZ) * 0.5f);
            camera.orthographicSize = (maxZ - minZ) * 0.5f;
            camera.aspect = (maxX - minX) / (maxZ - minZ);
        }

        static byte[] RenderTile(Camera camera, int width, int height, Color background)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Texture2D texture = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                renderTexture.antiAliasing = 1;
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                GL.Clear(true, true, background);
                camera.Render();
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                texture.Apply(false, false);
                return texture.EncodeToPNG();
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        static Bounds GetBounds(IEnumerable<AreaTrackingEntry> areas)
        {
            bool initialized = false;
            Bounds bounds = default;
            foreach (Vector3 point in areas.SelectMany(area => GetWorldCorners(area.Collider)))
            {
                if (!initialized) { bounds = new Bounds(point, Vector3.zero); initialized = true; }
                else bounds.Encapsulate(point);
            }
            return bounds;
        }

        static Bounds ExpandVerticalBounds(Bounds captureBounds, int layerMask)
        {
            float minimumY = captureBounds.min.y;
            float maximumY = captureBounds.max.y;
            foreach (Renderer renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy ||
                    (layerMask & (1 << renderer.gameObject.layer)) == 0) continue;
                Bounds bounds = renderer.bounds;
                if (bounds.max.x < captureBounds.min.x || bounds.min.x > captureBounds.max.x ||
                    bounds.max.z < captureBounds.min.z || bounds.min.z > captureBounds.max.z) continue;
                minimumY = Mathf.Min(minimumY, bounds.min.y);
                maximumY = Mathf.Max(maximumY, bounds.max.y);
            }
            foreach (Terrain terrain in UnityEngine.Object.FindObjectsOfType<Terrain>())
            {
                if (!terrain.enabled || !terrain.gameObject.activeInHierarchy || terrain.terrainData == null ||
                    (layerMask & (1 << terrain.gameObject.layer)) == 0) continue;
                Vector3 position = terrain.transform.position;
                Vector3 size = terrain.terrainData.size;
                if (position.x + size.x < captureBounds.min.x || position.x > captureBounds.max.x ||
                    position.z + size.z < captureBounds.min.z || position.z > captureBounds.max.z) continue;
                minimumY = Mathf.Min(minimumY, position.y);
                maximumY = Mathf.Max(maximumY, position.y + size.y);
            }
            captureBounds.SetMinMax(new Vector3(captureBounds.min.x, minimumY, captureBounds.min.z),
                new Vector3(captureBounds.max.x, maximumY, captureBounds.max.z));
            return captureBounds;
        }

        static AreaExtent GetExtent(AreaTrackingEntry entry)
        {
            Vector3[] corners = GetWorldCorners(entry.Collider);
            float minX = corners.Min(point => point.x);
            float maxX = corners.Max(point => point.x);
            float minZ = corners.Min(point => point.z);
            float maxZ = corners.Max(point => point.z);
            return new AreaExtent(entry, new Vector2((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f),
                maxX - minX, maxZ - minZ);
        }

        static Vector3[] GetWorldCorners(BoxCollider collider)
        {
            Vector3 half = collider.size * 0.5f;
            Vector3 center = collider.center;
            Vector3[] result = new Vector3[8];
            int index = 0;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
                result[index++] = collider.transform.TransformPoint(center + Vector3.Scale(half,
                    new Vector3(x, y, z)));
            return result;
        }

        static bool IsCapturable(AreaTrackingEntry area) =>
            area != null && area.IsIncluded && area.Collider != null && area.ParsedId != Guid.Empty;

        static float Median(IEnumerable<float> values)
        {
            List<float> sorted = values.OrderBy(value => value).ToList();
            return sorted.Count == 0 ? 0f : sorted[sorted.Count / 2];
        }

        static float Cross(Vector2 origin, Vector2 a, Vector2 b) =>
            (a.x - origin.x) * (b.y - origin.y) - (a.y - origin.y) * (b.x - origin.x);

        static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;

        static string ComputeSha256(byte[] bytes)
        {
            using (SHA256 sha256 = SHA256.Create()) return ToHex(sha256.ComputeHash(bytes));
        }

        static string ToHex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) builder.Append(value.ToString("x2"));
            return builder.ToString();
        }

        static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       81920, true))
                await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
        }

        readonly struct AreaExtent
        {
            internal readonly AreaTrackingEntry Entry;
            internal readonly Vector2 Center;
            internal readonly float Width;
            internal readonly float Depth;
            internal AreaExtent(AreaTrackingEntry entry, Vector2 center, float width, float depth)
            { Entry = entry; Center = center; Width = width; Depth = depth; }
        }

        sealed class ApproximateVectorComparer : IEqualityComparer<Vector2>
        {
            public bool Equals(Vector2 first, Vector2 second) =>
                Mathf.Abs(first.x - second.x) < 0.00001f && Mathf.Abs(first.y - second.y) < 0.00001f;
            public int GetHashCode(Vector2 value) =>
                (Mathf.RoundToInt(value.x * 100000f) * 397) ^ Mathf.RoundToInt(value.y * 100000f);
        }
    }
}
