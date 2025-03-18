using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRCLinking.Modules.SupporterBoard
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(VrcLinkingSupporterModule))]
    public class VrcLinkingSupporterModuleHelper : MonoBehaviour
    {
        public List<SupporterRole> roleList = new List<SupporterRole>();

        void OnValidate()
        {
            this.hideFlags = HideFlags.HideInInspector;
        }
    }
}