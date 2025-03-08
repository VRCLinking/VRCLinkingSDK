using System.Collections.Generic;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRCLinking.Modules;

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
            
            var modules = objects.Select(o => o.GetComponent<VrcLinkingModuleBase>()).Where(o => o != null).ToArray();

            downloader.GetComponent<VrcLinkingDownloader>().modules = modules;

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