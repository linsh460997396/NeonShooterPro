using UnityEngine;

namespace NeonShooter
{
    public abstract class Entity
    {
        protected Texture2D image;
        protected Color color = Color.white;

        public Vector2 Position;
        public Vector2 Velocity;
        public float Orientation;
        public float Radius = 20;
        public bool IsExpired;

        public Vector2 Size
        {
            get
            {
                return image == null ? Vector2.zero : new Vector2(image.width, image.height);
            }
        }

        public abstract void Update();

        public virtual void Draw()
        {
            if (image != null && BatchRenderer.Instance != null)
            {
                BatchRenderer.Instance.Draw(image, Position, Size * 0.5f, Orientation, Vector2.one, color);
            }
        }
    }
}