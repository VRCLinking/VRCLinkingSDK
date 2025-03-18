using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

namespace VRCLinking.Modules.SupporterBoard.Editor
{
    public class SupporterBoardPostProcessScene : MonoBehaviour
    {
        [PostProcessScene(-10)]
        public static void PostProcessScene()
        {
            VrcLinkingSupporterModule[] modules =
                FindObjectsByType<VrcLinkingSupporterModule>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            foreach (VrcLinkingSupporterModule module in modules)
            {
                VrcLinkingSupporterModuleHelper helper = module.GetComponent<VrcLinkingSupporterModuleHelper>(); 
                module.roles = SupporterUtilities.ConvertToDataList(helper.roleList);
            }
        }
    }
}