using System.Reflection;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace VRCLinking.Modules.Toggles
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VrcLinkingToggleModule : VrcLinkingModuleBase
    {
        public override string ModuleName => "VrcLinkingToggleModule";
        public string supporterRoleId;
        public GameObject[] toggleOnObjects;
        public GameObject[] toggleOffObjects;

        public override void OnDataLoaded()
        {
            if (!downloader.TryGetGuildMembersByRoleId(supporterRoleId, out DataList members)) return;
            
            if (members.Contains(Networking.LocalPlayer.displayName))
            {
                ToggleObjects();
            }
        }

        void ToggleObjects()
        {
            foreach (var obj in toggleOnObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            
            foreach (var obj in toggleOffObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}