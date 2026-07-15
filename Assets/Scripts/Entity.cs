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
            if (image != null)
            {
                DrawTexture(image, Position, Size / 2, Orientation, color);
            }
        }

        protected void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, float rotation, Color tint)
        {
            if (texture == null) return;

            GUIUtility.RotateAroundPivot(rotation * Mathf.Rad2Deg, position);

            GUI.color = tint;
            GUI.DrawTexture(new Rect(position.x - origin.x, position.y - origin.y, texture.width, texture.height), texture);

            GUIUtility.RotateAroundPivot(-rotation * Mathf.Rad2Deg, position);
            GUI.color = Color.white;
        }
    }
}