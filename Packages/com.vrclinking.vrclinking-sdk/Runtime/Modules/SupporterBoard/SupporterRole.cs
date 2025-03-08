using System;
using UnityEngine;

namespace VRCLinking.Modules.SupporterBoard
{
    [Serializable]
    public class SupporterRole
    {
        public RoleType roleType = RoleType.RoleId;
        public string roleValue = "";
        public Color roleColor = Color.white;
        public string roleSeparator = ", ";
        public float roleRelativeSize = 100f;
    }
}