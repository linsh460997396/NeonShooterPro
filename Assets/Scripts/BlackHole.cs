using UnityEngine;
using System;

namespace NeonShooter
{
    public class BlackHole : Entity
    {
        private static System.Random rand = new System.Random();

        private int hitpoints = 10;
        private float sprayAngle = 0;

        public BlackHole(Vector2 position)
        {
            image = Art.BlackHole;
            Position = position;
            Radius = image.width / 2f;
        }

        public override void Update()
        {
            var entities = EntityManager.GetNearbyEntities(Position, 250);

            foreach (var entity in entities)
            {
                if (entity is Enemy && !(entity as Enemy).IsActive)
                    continue;

                if (entity is Bullet)
                    entity.Velocity += (entity.Position - Position).ScaleTo(0.3f);
                else
                {
                    var dPos = Position - entity.Position;
                    var length = dPos.magnitude;

                    entity.Velocity += dPos.ScaleTo(Mathf.Lerp(2, 0, length / 250f));
                }
            }

            if ((GameManager.TotalTime * 1000 / 250) % 2 == 0)
            {
                Vector2 sprayVel = MathUtil.FromPolar(sprayAngle, rand.NextFloat(12, 15));
                Color color = ColorUtil.HSVToColor(5, 0.5f, 0.8f);
                Vector2 pos = Position + 2f * new Vector2(sprayVel.y, -sprayVel.x) + rand.NextVector2(4, 8);
                var state = new ParticleState()
                {
                    Velocity = sprayVel,
                    LengthMultiplier = 1,
                    Type = ParticleType.Enemy
                };

                GameManager.ParticleManager.CreateParticle(Art.Laser, pos, color, 190, 1.5f, state);
            }

            sprayAngle -= Mathf.PI * 2 / 50f;

            GameManager.Grid.ApplyImplosiveForce((float)Math.Sin(sprayAngle / 2) * 10 + 20, Position, 200);
        }

        public void WasShot()
        {
            hitpoints--;
            if (hitpoints <= 0)
            {
                IsExpired = true;
                PlayerStatus.AddPoints(5);
                PlayerStatus.IncreaseMultiplier();
            }

            float hue = (float)((3 * GameManager.TotalTime) % 6);
            Color color = ColorUtil.HSVToColor(hue, 0.25f, 1);
            const int numParticles = 150;
            float startOffset = rand.NextFloat(0, Mathf.PI * 2 / numParticles);

            for (int i = 0; i < numParticles; i++)
            {
                Vector2 sprayVel = MathUtil.FromPolar(Mathf.PI * 2 * i / numParticles + startOffset, rand.NextFloat(8, 16));
                Vector2 pos = Position + 2f * sprayVel;
                var state = new ParticleState()
                {
                    Velocity = sprayVel,
                    LengthMultiplier = 1,
                    Type = ParticleType.IgnoreGravity
                };

                GameManager.ParticleManager.CreateParticle(Art.Laser, pos, color, 90, 1.5f, state);
            }

            Sound.PlayClip(Sound.Explosion, 0.5f);
        }

        public void Kill()
        {
            hitpoints = 0;
            WasShot();
        }

        public override void Draw()
        {
            if (BatchRenderer.Instance == null || image == null) return;

            float scale = 1 + 0.1f * (float)Mathf.Sin(10 * GameManager.TotalTime);
            // 原点为未缩放的纹理中心,BatchRenderer内部会乘以scale
            Vector2 origin = new Vector2(image.width * 0.5f, image.height * 0.5f);
            BatchRenderer.Instance.Draw(image, Position, origin, Orientation, new Vector2(scale, scale), color);
        }
    }
}