using System.Reflection;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;

namespace VRCLinking.Modules.Toggles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VrcLinkingToggleModule : VrcLinkingModuleBase
    {
        private protected override string ModuleName => "VrcLinkingToggleModule";

        public DataDictionary toggleData;
        /*
         * 
         */

        public override void OnDataLoaded()
        {
            
        }
    }
}
