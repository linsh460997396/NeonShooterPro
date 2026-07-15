using UnityEngine;
using System;
using System.Collections.Generic;

namespace NeonShooter
{
    public class Enemy : Entity
    {
        public static System.Random rand = new System.Random();

        private List<IEnumerator<int>> behaviours = new List<IEnumerator<int>>();
        private int timeUntilStart = 60;
        public bool IsActive { get { return timeUntilStart <= 0; } }
        public int PointValue { get; private set; }

        public Enemy(Texture2D image, Vector2 position)
        {
            this.image = image;
            Position = position;
            Radius = image.width / 2f;
            color = Color.clear;
            PointValue = 1;
        }

        public static Enemy CreateSeeker(Vector2 position)
        {
            var enemy = new Enemy(Art.Seeker, position);
            enemy.AddBehaviour(enemy.FollowPlayer(0.9f));
            enemy.PointValue = 2;

            return enemy;
        }

        public static Enemy CreateWanderer(Vector2 position)
        {
            var enemy = new Enemy(Art.Wanderer, position);
            enemy.AddBehaviour(enemy.MoveRandomly());

            return enemy;
        }

        public override void Update()
        {
            if (timeUntilStart <= 0)
                ApplyBehaviours();
            else
            {
                timeUntilStart--;
                color = Color.white * (1 - timeUntilStart / 60f);
            }

            Position += Velocity;
            Position = MathUtil.Clamp(Position, Size / 2, GameManager.ScreenSize - Size / 2);

            Velocity *= 0.8f;
        }

        public override void Draw()
        {
            if (BatchRenderer.Instance == null || image == null) return;

            if (timeUntilStart > 0)
            {
                float factor = timeUntilStart / 60f;
                float scale = 2 - factor;
                // 原点为未缩放的纹理中心,BatchRenderer内部会乘以scale
                Vector2 origin = new Vector2(image.width * 0.5f, image.height * 0.5f);
                BatchRenderer.Instance.Draw(image, Position, origin, Orientation, new Vector2(scale, scale), Color.white * factor);
            }
            else
            {
                base.Draw();
            }
        }

        private void AddBehaviour(IEnumerable<int> behaviour)
        {
            behaviours.Add(behaviour.GetEnumerator());
        }

        private void ApplyBehaviours()
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (!behaviours[i].MoveNext())
                    behaviours.RemoveAt(i--);
            }
        }

        public void HandleCollision(Enemy other)
        {
            var d = Position - other.Position;
            Velocity += 10 * d / (d.sqrMagnitude + 1);
        }

        public void WasShot()
        {
            IsExpired = true;
            PlayerStatus.AddPoints(PointValue);
            PlayerStatus.IncreaseMultiplier();

            float hue1 = rand.NextFloat(0, 6);
            float hue2 = (hue1 + rand.NextFloat(0, 2)) % 6f;
            Color color1 = ColorUtil.HSVToColor(hue1, 0.5f, 1);
            Color color2 = ColorUtil.HSVToColor(hue2, 0.5f, 1);

            for (int i = 0; i < 120; i++)
            {
                float speed = 18f * (1f - 1 / rand.NextFloat(1, 10));
                var state = new ParticleState()
                {
                    Velocity = rand.NextVector2(speed, speed),
                    Type = ParticleType.Enemy,
                    LengthMultiplier = 1
                };

                Color color = Color.Lerp(color1, color2, rand.NextFloat(0, 1));
                GameManager.ParticleManager.CreateParticle(Art.Laser, Position, color, 190, 1.5f, state);
            }

            Sound.PlayClip(Sound.Explosion, 0.5f);
        }

        private IEnumerable<int> FollowPlayer(float acceleration)
        {
            while (true)
            {
                if (!PlayerShip.Instance.IsDead)
                    Velocity += (PlayerShip.Instance.Position - Position).ScaleTo(acceleration);

                if (Velocity != Vector2.zero)
                    Orientation = Velocity.ToAngle();

                yield return 0;
            }
        }

        private IEnumerable<int> MoveRandomly()
        {
            float direction = rand.NextFloat(0, Mathf.PI * 2);

            while (true)
            {
                direction += rand.NextFloat(-0.1f, 0.1f);
                direction = Mathf.Repeat(direction, Mathf.PI * 2);

                for (int i = 0; i < 6; i++)
                {
                    Velocity += MathUtil.FromPolar(direction, 0.4f);
                    Orientation -= 0.05f;

                    var bounds = new Rect(0, 0, GameManager.ScreenSize.x, GameManager.ScreenSize.y);
                    bounds.xMin += image.width / 2 + 1;
                    bounds.xMax -= image.width / 2 + 1;
                    bounds.yMin += image.height / 2 + 1;
                    bounds.yMax -= image.height / 2 + 1;

                    if (!bounds.Contains(Position))
                        direction = (GameManager.ScreenSize / 2 - Position).ToAngle() + rand.NextFloat(-Mathf.PI / 2, Mathf.PI / 2);

                    yield return 0;
                }
            }
        }
    }
}