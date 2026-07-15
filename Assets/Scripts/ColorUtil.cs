using UnityEngine;

namespace NeonShooter
{
    public static class ColorUtil
    {
        public static Color HSVToColor(float h, float s, float v)
        {
            Color color = Color.HSVToRGB(h / 6f, s, v);
            return color;
        }
    }
}