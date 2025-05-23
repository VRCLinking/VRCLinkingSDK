﻿using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRCLinking.Modules.Posters;

namespace VRCLinking.Editor.Modules.Posters
{
    public class PostersBuildHandler
    {
        const string BaseUrl = "https://data.vrclinking.com/atlas/";

        [PostProcessScene(-10)]
        public static void OnBuild()
        {
            var downloader = Object.FindObjectOfType<VrcLinkingDownloader>(true);
            var module = Object.FindObjectOfType<VrcLinkingPostersModule>(true);

            if (module == null || downloader == null)
            {
                Debug.Log(
                    $"No Poster Module or Downloader found in the scene - {downloader == null} - {module == null}");
                return;
            }

            var posters = Object.FindObjectsOfType<VrcLinkingPoster>(true);

            var atlasCount = module.maxAtlasCount;
            // Create new VRCUrl array "BaseUrl/world_id/index"
            module.atlasUrlsVariation0 = new VRCUrl[atlasCount];
            module.atlasUrlsVariation1 = new VRCUrl[atlasCount];
            for (int i = 0; i < atlasCount; i++)
            {
                var worldId = downloader.worldId;
                module.atlasUrlsVariation0[i] = new VRCUrl($"{BaseUrl}{worldId}/0/{i}");
                module.atlasUrlsVariation1[i] = new VRCUrl($"{BaseUrl}{worldId}/1/{i}");
            }


            module.atlasMaterials = Enumerable.Range(0, module.maxAtlasCount)
                .Select(x => new Material(module.posterMaterial))
                .ToArray();

            var allPosters = posters;

            module.posterSlotIds = allPosters.Select(x => x.slotId).ToArray();
            var meshRenderers = allPosters.Select(x => x.transform.GetComponentInChildren<MeshRenderer>());
            var renderers = meshRenderers.ToList();

            module.posterSlotsRenderers = renderers.ToArray();
            module.posterSlotsImages =
                allPosters.Select(x => x.transform.GetComponentInChildren<RawImage>()).ToArray();
            module.posterAutoSize = allPosters.Select(x => x.enableAutoSize).ToArray();
            module.posterSlotsWidth = allPosters.Select(x => x.widthSizeControl).ToArray();
            module.posterSlotsHeight = allPosters.Select(x => x.heightSizeControl).ToArray();
            
            
            foreach (var poster in posters)
            {
                if (module.disablePostersOnBuild)
                {
                    poster.gameObject.SetActive(false);
                }

                Object.DestroyImmediate(poster);
            }
        }
    }
}