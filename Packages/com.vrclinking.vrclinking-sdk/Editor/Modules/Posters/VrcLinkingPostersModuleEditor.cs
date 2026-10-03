using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using VRCLinking.Modules.Posters;

namespace VRCLinking.Editor.Modules.Posters
{
    [CustomEditor(typeof(VrcLinkingPostersModule))]
    public class VrcLinkingPostersModuleEditor : UnityEditor.Editor
    {
        private VrcLinkingApiHelper _apiHelper;
        private bool _isLoggedIn;
        private bool _busy;

        private async void OnEnable()
        {
            _apiHelper = new VrcLinkingApiHelper();
            try { _isLoggedIn = await _apiHelper.IsUserLoggedIn(); }
            catch (Exception e) { Debug.LogWarning("Poster login check failed: " + e.Message); }
            if (this != null) Repaint();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Playback waits for the atlases used by this world's playlists on this client only. Synchronized playlists share a seed and clock; late joiners join the current timeline. Content updates are best effort. Failed downloads can be retried with RetryAtlasDownloads.", MessageType.Info);
            var downloader = FindObjectOfType<VrcLinkingDownloader>(true);
            bool configured = downloader != null && downloader.worldId != Guid.Empty && !string.IsNullOrEmpty(downloader.serverId);
            if (!_isLoggedIn) EditorGUILayout.HelpBox("Log in through the Downloader to sync poster slots.", MessageType.Info);
            if (!configured) EditorGUILayout.HelpBox("Configure the Downloader's world and guild before syncing.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(_busy || !_isLoggedIn || !configured || Application.isPlaying))
                if (GUILayout.Button(_busy ? "Syncing..." : "Sync Poster Slots")) _ = SyncPosters(downloader);
        }

        private async Task SyncPosters(VrcLinkingDownloader downloader)
        {
            _busy = true;
            try
            {
                var posters = PostersBuildHandler.FindPosters();
                if (posters.Any(p => p.GetSlotIds().Length == 0 || p.GetSlotIds().Any(id => id < 0)))
                    throw new InvalidOperationException("Every poster/group needs a non-empty playlist of non-negative IDs.");
                await _apiHelper.SyncPosters(downloader.serverId, downloader.worldId, posters.ToList());
                Debug.Log("Poster slots synchronized, including inactive posters and group playlists.");
            }
            catch (Exception e) { Debug.LogError("Failed to sync posters: " + e.Message); }
            finally { _busy = false; if (this != null) Repaint(); }
        }
    }
}
