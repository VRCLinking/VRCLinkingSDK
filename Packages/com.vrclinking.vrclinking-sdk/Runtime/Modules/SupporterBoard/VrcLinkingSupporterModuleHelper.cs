using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Udon.Serialization.OdinSerializer;

namespace VRCLinking.Modules.SupporterBoard
{
    [AddComponentMenu("")]
    public class VrcLinkingSupporterModuleHelper : MonoBehaviour
    {
        [Header("Ignore this component, it's used internally by the VrcLinkingSupporterModule")]
        public List<SupporterRole> roleList = new List<SupporterRole>();
    }
}