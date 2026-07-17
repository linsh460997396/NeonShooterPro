using UnityEngine;
using System;

namespace NeonShooter
{
    public class Bullet : Entity
    {
        private static System.Random rand = new System.Random();

        public Bullet(Vector2 position, Vector2 velocity)
        {
            image = Art.Bullet;
            Position = position;
            Velocity = velocity;
            Orientation = Velocity.ToAngle();
            Radius = 8;
        }

        private static int frameCounter;

        public override void Update()
        {
            if (Velocity.sqrMagnitude > 0)
                Orientation = Velocity.ToAngle();

            Position += Velocity;

            if (frameCounter++ % 3 == 0)
                GameManager.Grid.ApplyExplosiveForce(0.5f * Velocity.magnitude, Position, 80);

            if (!IsInViewport(Position))
            {
                IsExpired = true;

                for (int i = 0; i < 30; i++)
                    GameManager.ParticleManager.CreateParticle(Art.Laser, Position, new Color(0.678f, 0.847f, 0.902f), 50, 1,
                        new ParticleState() { Velocity = rand.NextVector2(0, 9), Type = ParticleType.Bullet, LengthMultiplier = 1 });
            }
        }

        private bool IsInViewport(Vector2 position)
        {
            return position.x >= 0 && position.x <= GameManager.ScreenSize.x &&
                   position.y >= 0 && position.y <= GameManager.ScreenSize.y;
        }
    }
}