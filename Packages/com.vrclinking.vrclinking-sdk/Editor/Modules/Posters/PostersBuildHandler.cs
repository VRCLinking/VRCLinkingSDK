using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRCLinking.Modules.Posters;
using Object = UnityEngine.Object;

namespace VRCLinking.Editor.Modules.Posters
{
    public static class PostersBuildHandler
    {
        [PostProcessScene(-10)]
        public static void OnBuild()
        {
            var modules = Object.FindObjectsOfType<VrcLinkingPostersModule>(true);
            var posters = FindPosters();
            if (modules.Length == 0 && posters.Length == 0) return;
            if (modules.Length != 1) throw new BuildFailedException("Posters require exactly one Posters Module in the scene.");
            var downloader = Object.FindObjectOfType<VrcLinkingDownloader>(true);
            if (downloader == null) throw new BuildFailedException("Posters require a VRCLinking Downloader.");
            Bake(modules[0], downloader, posters);
            foreach (var poster in posters) Object.DestroyImmediate(poster);
            foreach (var group in Object.FindObjectsOfType<VrcLinkingPosterGroup>(true)) Object.DestroyImmediate(group);
        }

        public static VrcLinkingPoster[] FindPosters() => Object.FindObjectsOfType<VrcLinkingPoster>(true)
            .OrderBy(p => HierarchyKey(p.transform), StringComparer.Ordinal).ToArray();

        private static string HierarchyKey(Transform transform)
        {
            string path = "";
            while (transform != null)
            {
                path = "/" + transform.GetSiblingIndex().ToString("D6") + ":" + transform.name + path;
                transform = transform.parent;
            }
            return path;
        }

        private static int StableKey(string value)
        {
            uint hash = 2166136261;
            foreach (char c in value) hash = unchecked((hash ^ c) * 16777619);
            return (int)(hash & 0x7fffffff);
        }

        public static void Bake(VrcLinkingPostersModule module, VrcLinkingDownloader downloader, VrcLinkingPoster[] posters)
        {
            if (module.posterMaterial == null || !module.posterMaterial.HasProperty("_PosterState"))
                throw new BuildFailedException("Assign a VRCLinking poster material with playlist shader support.");
            if (!module.gameObject.activeInHierarchy || !module.enabled)
                throw new BuildFailedException("The Posters Module must be active and enabled.");
            if (module.maxAtlasCount < 1 || module.maxAtlasCount > 256)
                throw new BuildFailedException("Poster maxAtlasCount must be between 1 and 256.");
            module.downloader = downloader;
            module.atlasUrlsVariation0 = new VRCUrl[module.maxAtlasCount];
            module.atlasUrlsVariation1 = new VRCUrl[module.maxAtlasCount];
            for (int i = 0; i < module.maxAtlasCount; i++)
            {
                module.atlasUrlsVariation0[i] = new VRCUrl($"https://data.vrclinking.com/atlas/{downloader.worldId}/0/{i}");
                module.atlasUrlsVariation1[i] = new VRCUrl($"https://data.vrclinking.com/atlas/{downloader.worldId}/1/{i}");
            }

            int count = posters.Length;
            module.playlistOffsets = new int[count]; module.playlistCounts = new int[count];
            module.sequenceKeys = new int[count]; module.memberIndices = new int[count];
            module.uniqueGroups = new bool[count]; module.playbackModes = new int[count];
            module.orders = new int[count]; module.synchronizedPlayback = new bool[count];
            module.randomStarts = new bool[count]; module.transitions = new int[count];
            module.directions = new int[count]; module.timings = new Vector4[count];
            module.groupChangeDurations = new float[count];
            module.sizeModes = new int[count]; module.fitModes = new int[count];
            module.backgroundColors = new Color[count]; module.fadeColors = new Color[count];
            module.posterRoots = new GameObject[count]; module.posterSlotsRenderers = new MeshRenderer[count];
            module.posterSlotsImages = new RawImage[count]; module.posterSlotsWidth = new SizeControl[count];
            module.posterSlotsHeight = new SizeControl[count]; module.imageMaterials = new Material[count];
            var ids = new List<int>();
            var usedTargets = new HashSet<Component>();
            var groupMembers = new Dictionary<VrcLinkingPosterGroup, int>();
            var uiShader = Shader.Find("VRCLinking/Poster UI");
            for (int i = 0; i < count; i++)
            {
                var poster = posters[i];
                var playlist = poster.GetSlotIds();
                var group = poster.group;
                var settings = group != null ? group.playback : poster.playback;
                if (settings == null || playlist.Length == 0 || playlist.Any(id => id < 0))
                    throw new BuildFailedException($"Poster {poster.name} needs a non-empty playlist of non-negative IDs.");
                if (playlist.Length != playlist.Distinct().Count())
                    throw new BuildFailedException($"Poster {poster.name} repeats an ID within its playlist. Use each ID once.");
                if (playlist.Length > 1024) throw new BuildFailedException($"Poster {poster.name} exceeds the 1024-entry playlist limit.");
                if (!ValidDuration(settings.displayDuration) || !ValidDuration(settings.transitionDuration) ||
                    !ValidDuration(settings.startDelay) || !ValidDuration(settings.staggerSeconds) || !ValidDuration(poster.resizeDuration))
                    throw new BuildFailedException($"Poster {poster.name} has invalid timing values.");
                var renderer = poster.targetRenderer;
                var image = poster.targetImage;
                if (renderer == null && image == null)
                {
                    renderer = poster.GetComponentInChildren<MeshRenderer>(true);
                    if (renderer == null) image = poster.GetComponentInChildren<RawImage>(true);
                }
                if ((renderer == null) == (image == null))
                    throw new BuildFailedException($"Poster {poster.name} must target exactly one MeshRenderer or RawImage.");
                Component target = renderer != null ? (Component)renderer : image;
                if (!usedTargets.Add(target)) throw new BuildFailedException($"Multiple posters target {target.name}.");
                if (target.transform != poster.transform && !target.transform.IsChildOf(poster.transform))
                    throw new BuildFailedException($"Poster target {target.name} must belong to its poster's hierarchy.");
                if (poster.transform == module.transform || module.transform.IsChildOf(poster.transform))
                    throw new BuildFailedException("The Posters Module cannot be inside a poster that it controls.");
                var parentPoster = poster.transform.parent != null ? poster.transform.parent.GetComponentInParent<VrcLinkingPoster>(true) : null;
                if (parentPoster != null) throw new BuildFailedException("Poster roots must not be nested inside other posters.");

                int rank = 0;
                bool unique = group != null && group.distribution == PosterGroupMode.UniqueDistribution;
                if (group != null)
                {
                    groupMembers.TryGetValue(group, out rank);
                    groupMembers[group] = rank + 1;
                    if (unique && posters.Count(p => p.group == group) > playlist.Length)
                        throw new BuildFailedException($"Group {group.name} needs at least one distinct slot ID per display.");
                    if (unique && settings.staggerSeconds * (posters.Count(p => p.group == group) - 1) >= Mathf.Max(0.1f, settings.displayDuration))
                        throw new BuildFailedException($"Group {group.name}: total stagger must be shorter than Display Duration so each unique distribution can finish before the next change.");
                }
                var sizeMode = poster.sizeMode;
                bool legacy = sizeMode == PosterSizeMode.Automatic && group == null &&
                              (poster.slotIds == null || poster.slotIds.Length == 0);
                if (sizeMode == PosterSizeMode.Automatic)
                    sizeMode = playlist.Length == 1 && poster.enableAutoSize ? PosterSizeMode.Instant : PosterSizeMode.Fixed;
                // Existing RawImages never auto-resized; preserve their original stretch behavior.
                if (legacy && image != null) sizeMode = PosterSizeMode.Fixed;
                if (renderer != null)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null || poster.widthSizeControl == poster.heightSizeControl)
                        throw new BuildFailedException($"Poster {poster.name} needs a mesh and two distinct size axes.");
                    var bounds = filter.sharedMesh.bounds.size;
                    var scale = renderer.transform.lossyScale;
                    if (Mathf.Abs(bounds[(int)poster.widthSizeControl] * scale[(int)poster.widthSizeControl]) < 0.0001f ||
                        Mathf.Abs(bounds[(int)poster.heightSizeControl] * scale[(int)poster.heightSizeControl]) < 0.0001f)
                        throw new BuildFailedException($"Poster {poster.name} has zero size along a selected axis.");
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,
                        GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) & ~StaticEditorFlags.BatchingStatic);
                }
                else
                {
                    if (image.rectTransform.rect.width * Mathf.Abs(image.transform.lossyScale.x) < 0.0001f || image.rectTransform.rect.height * Mathf.Abs(image.transform.lossyScale.y) < 0.0001f)
                        throw new BuildFailedException($"Poster {poster.name} needs a positive UI rect size.");
                    if (uiShader == null) throw new BuildFailedException("Poster UI shader is missing.");
                    module.imageMaterials[i] = new Material(uiShader) { name = "Poster UI " + i };
                }
                module.playlistOffsets[i] = ids.Count; module.playlistCounts[i] = playlist.Length;
                ids.AddRange(playlist);
                module.sequenceKeys[i] = StableKey(HierarchyKey(unique ? group.transform : poster.transform));
                module.memberIndices[i] = rank; module.uniqueGroups[i] = unique;
                module.playbackModes[i] = (int)settings.mode; module.orders[i] = (int)settings.order;
                module.synchronizedPlayback[i] = settings.synchronize; module.randomStarts[i] = settings.randomStart;
                module.transitions[i] = (int)settings.transition; module.directions[i] = (int)settings.direction;
                float stagger = settings.staggerSeconds * (group != null ? rank : i);
                module.timings[i] = new Vector4(Mathf.Max(0.1f, settings.displayDuration),
                    settings.transition == PosterTransition.Instant ? 0 : settings.transitionDuration,
                    settings.startDelay + stagger, poster.resizeDuration);
                module.sizeModes[i] = (int)sizeMode;
                module.fitModes[i] = legacy ? (image != null || module.posterMaterial.GetFloat("_AspectCorrection") == 0 ? (int)PosterFitMode.Stretch : (int)PosterFitMode.Fit) : (int)poster.fitMode;
                module.backgroundColors[i] = legacy && renderer != null ? module.posterMaterial.GetColor("_BoxingColor") : poster.backgroundColor;
                module.fadeColors[i] = settings.fadeColor;
                module.posterRoots[i] = poster.gameObject; module.posterSlotsRenderers[i] = renderer;
                module.posterSlotsImages[i] = image; module.posterSlotsWidth[i] = poster.widthSizeControl;
                module.posterSlotsHeight[i] = poster.heightSizeControl;
            }
            module.playlistIds = ids.ToArray();
            // Group periods must agree even when member resize durations differ.
            for (int i = 0; i < count; i++)
            {
                if (posters[i].group == null) continue;
                float longestResize = 0;
                for (int j = 0; j < count; j++)
                    if (posters[j].group == posters[i].group && module.sizeModes[j] == (int)PosterSizeMode.Animated)
                        longestResize = Mathf.Max(longestResize, module.timings[j].w);
                // Separate common cycle length from each member's own animation duration.
                module.groupChangeDurations[i] = Mathf.Max(module.timings[i].y, longestResize);
            }
            var backing = UdonSharpEditorUtility.GetBackingUdonBehaviour(module);
            if (backing != null) backing.SyncMethod = Networking.SyncType.Manual;
            foreach (var poster in posters)
                if (module.disablePostersOnBuild) poster.gameObject.SetActive(false);
        }

        private static bool ValidDuration(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0 && value <= 86400;
    }
}
