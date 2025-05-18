using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using VRCLinking.Modules.Posters;

// ReSharper disable once CheckNamespace
namespace VRCLinking.Editor.Modules.Posters
{
    [CustomEditor(typeof(VrcLinkingPostersModule))]
    public class VrcLinkingPostersModuleEditor : UnityEditor.Editor
    {
        VrcLinkingApiHelper _apiHelper;
        bool _isLoggedIn;

        void OnEnable()
        {
            _apiHelper = new VrcLinkingApiHelper();
            _ = OnEnableAsync();
        }

        async Task OnEnableAsync()
        {
            _isLoggedIn = await _apiHelper.IsUserLoggedIn();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var module = (VrcLinkingPostersModule)target;


            if (GUILayout.Button("Fix All SlotIds in scene"))
            {
                var allPosters = FindObjectsOfType<VrcLinkingPoster>(true);
                foreach (var poster in allPosters)
                {
                    var slotId = poster.slotId;
                    if (slotId == -1)
                    {
                        Debug.LogWarning($"Poster {poster.name} has no SlotId set. Skipping.");
                        continue;
                    }

                    var idCount = module.posterSlotIds.Count(x => x == slotId);
                    if (idCount > 1)
                    {
                        if (GUILayout.Button("Fix Poster IDs"))
                        {
                            poster.slotId = module.posterSlotIds.Max() + 1;

                            poster.gameObject.name = "Poster " + poster.slotId;
                        }
                    }
                }


                module.posterSlotIds = allPosters.Select(x => x.slotId).ToArray();
                var meshRenderers = allPosters.Select(x => x.transform.GetComponentInChildren<MeshRenderer>());
                var renderers = meshRenderers.ToList();

                module.posterSlotsRenderers = renderers.ToArray();
                module.posterSlotsImages =
                    allPosters.Select(x => x.transform.GetComponentInChildren<RawImage>()).ToArray();
                module.posterAutoSize = allPosters.Select(x => x.enableAutoSize).ToArray();
                module.posterSlotsWidth = allPosters.Select(x => x.widthSizeControl).ToArray();
                module.posterSlotsHeight = allPosters.Select(x => x.heightSizeControl).ToArray();
            }

            _ = SyncPosters();
        }

        async Task SyncPosters()
        {
            if (_isLoggedIn)
            {
                var downloader = FindObjectOfType<VrcLinkingDownloader>(true);
                if (downloader == null)
                {
                    EditorGUILayout.HelpBox("No Downloader SDK found in the scene", MessageType.Error);
                    return;
                }

                if (downloader.worldId == Guid.Empty)
                {
                    EditorGUILayout.HelpBox("World ID is not set. Please set the World in the Downloader.",
                        MessageType.Error);
                    return;
                }

                if (string.IsNullOrEmpty(downloader.serverId))
                {
                    EditorGUILayout.HelpBox("Guild ID is not set. Please set the Guild in the Downloader.",
                        MessageType.Error);
                    return;
                }

                if (GUILayout.Button("Sync Posters"))
                {
                    var worldId = downloader.worldId;
                    var guildId = downloader.serverId;

                    try
                    {
                        var posters = FindObjectsOfType<VrcLinkingPoster>().ToList();
                        await _apiHelper.SyncPosters(guildId, worldId, posters);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to find posters: {e.Message}");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "You are not logged in to the Downloader SDK. Please log in via Downloader to sync posters.",
                    MessageType.Error);
            }
        }
    }
}