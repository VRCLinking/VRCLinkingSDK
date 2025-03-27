using UdonSharp;
using UnityEngine;

namespace VRCLinking.Modules
{
    public class VrcLinkingModuleBase : UdonSharpBehaviour
    {
        [HideInInspector] public VrcLinkingDownloader downloader;
        
        /// <summary>
        /// The name of the module.
        /// </summary>
        private protected virtual string ModuleName => "VrcLinkingModuleBase";
        
        /// <summary>
        /// Called when the data is loaded.
        /// </summary>
        public virtual void OnDataLoaded() {}

        /// <summary>
        /// Module based Logging.
        /// </summary>
        /// <param name="message"></param>
        private protected void Log(object message)
        {
            UnityEngine.Debug.Log($"[{ModuleName}-{gameObject.name}] {message}");
        }
        
        /// <summary>
        /// Module based Error Logging.
        /// </summary>
        /// <param name="message"></param>
        private protected void LogError(object message)
        {
            UnityEngine.Debug.LogError($"[{ModuleName}-{gameObject.name}] {message}");
        }
    }
}
