using UnityEngine;
using static System.Globalization.NumberStyles;

namespace VRCLinking.Utilitites
{
    public static class VrcLinkingUtilities
    {
        public static string ToHex(this Color color)
        {
            return $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}";
        }
        
        public static string ToHex(this Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }
        
        public static Color ToColor(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }
            if (hex.Length != 6)
            {
                return Color.white;
            }
            byte r = byte.Parse(hex.Substring(0, 2), HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), HexNumber);
            return new Color32(r, g, b, 255);
        }
    }
}