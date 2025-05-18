using System.Collections.Generic;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRCLinking.Modules;
using VRCLinking.Utilities;

namespace VRCLinking.Editor
{
    public class OnBuildHandler
    {
        [PostProcessScene(-10)]
        public static void OnBuild()
        {
            var objects = GetAllObjectsInScene();
            var downloader = objects.Select(o => o.GetComponent<VrcLinkingDownloader>()).FirstOrDefault(o => o != null);
            if (downloader == null)
            {
                return;
            }

            var lzwCompressor = downloader.GetComponent<Compressor>();
            
            if (lzwCompressor == null)
            {
                lzwCompressor = downloader.gameObject.AddComponent<Compressor>();
            }
            
            downloader.compressor = lzwCompressor;
            
            downloader.mainUrl = new VRCUrl("https://data.vrclinking.com/v2/" + downloader.worldId);
            downloader.fallbackUrl = new VRCUrl("https://linkingbotvrchat.github.io/v2data/worlds/" + downloader.worldId);
            
            var modules = objects.Select(o => o.GetComponent<VrcLinkingModuleBase>()).Where(o => o != null).ToArray();

            downloader.modules = modules;

            foreach (var module in modules)
            {
                module.downloader = downloader;
            }


        }
        
        static List<GameObject> GetAllObjectsInScene()
        {
            List<GameObject> objectsInScene = new List<GameObject>();
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var gameObject in go.GetComponentsInChildren<Transform>(true))
                {
                    objectsInScene.Add(gameObject.gameObject);
                }
            }
            return objectsInScene;
        }
    }
    
    
}