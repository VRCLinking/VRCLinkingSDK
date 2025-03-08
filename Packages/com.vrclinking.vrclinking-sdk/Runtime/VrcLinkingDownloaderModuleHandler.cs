using UnityEngine;
using VRCLinking.Modules;

namespace VRCLinking
{
    public partial class VrcLinkingDownloader
    {
        public VrcLinkingModuleBase[] modules;
        
        void SendEventToModules(string @event)
        {
            Debug.Log($"Sending event to modules: {@event}");
            foreach (var module in modules)
            {
                module.SendCustomEvent(@event);
            }
        }
        
        
    }
}