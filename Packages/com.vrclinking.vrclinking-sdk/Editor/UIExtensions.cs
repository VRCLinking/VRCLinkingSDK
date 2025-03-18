using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VRCLinking.Editor
{
    public static class UIExtensions
    {
        public static void SetDisplay(this VisualElement element, bool value)
        {
            element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public static void SetVisibility(this VisualElement element, bool value)
        {
            element.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
        }
    }
}