using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerColorUtility
    {
        public static Color ParseOrFallback(string htmlColor, Color fallback)
        {
            Color parsed;
            return ColorUtility.TryParseHtmlString(htmlColor, out parsed) ? parsed : fallback;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color Blend(Color from, Color to, float amount)
        {
            return Color.Lerp(from, to, Mathf.Clamp01(amount));
        }
    }
}
