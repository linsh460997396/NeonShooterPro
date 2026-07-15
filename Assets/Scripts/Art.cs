using UnityEngine;

namespace NeonShooter
{
    public static class Art
    {
        public static Texture2D Player { get; private set; }
        public static Texture2D Seeker { get; private set; }
        public static Texture2D Wanderer { get; private set; }
        public static Texture2D Bullet { get; private set; }
        public static Texture2D Pointer { get; private set; }
        public static Texture2D BlackHole { get; private set; }

        public static Texture2D Laser { get; private set; }
        public static Texture2D Glow { get; private set; }
        public static Texture2D Pixel { get; private set; }

        public static Font Font { get; private set; }

        public static void Load()
        {
            Player = Resources.Load<Texture2D>("Art/Player");
            Seeker = Resources.Load<Texture2D>("Art/Seeker");
            Wanderer = Resources.Load<Texture2D>("Art/Wanderer");
            Bullet = Resources.Load<Texture2D>("Art/Bullet");
            Pointer = Resources.Load<Texture2D>("Art/Pointer");
            BlackHole = Resources.Load<Texture2D>("Art/Black Hole");

            Laser = Resources.Load<Texture2D>("Art/Laser");
            Glow = Resources.Load<Texture2D>("Art/Glow");

            Pixel = new Texture2D(1, 1);
            Pixel.SetPixels(new[] { Color.white });
            Pixel.Apply();

            Font = Resources.Load<Font>("Fonts/Font");
            if (Font == null)
            {
                Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (Font == null)
                    Font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
        }
    }
}