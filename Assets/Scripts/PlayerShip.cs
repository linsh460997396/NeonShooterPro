using UnityEngine;
using System;

namespace NeonShooter
{
    public class PlayerShip : Entity
    {
        private static PlayerShip instance;
        public static PlayerShip Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerShip();

                return instance;
            }
        }

        const int cooldownFrames = 6;
        int cooldowmRemaining = 0;

        int framesUntilRespawn = 0;
        public bool IsDead { get { return framesUntilRespawn > 0; } }

        static System.Random rand = new System.Random();
        private bool initialized = false;

        private PlayerShip()
        {
            Position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Radius = 10;
        }

        private void Initialize()
        {
            if (initialized) return;
            image = Art.Player;
            if (GameManager.ScreenSize.sqrMagnitude > 0)
                Position = GameManager.ScreenSize / 2;
            Radius = 10;
            initialized = true;
        }

        public override void Update()
        {
            Initialize();

            if (IsDead)
            {
                if (--framesUntilRespawn == 0)
                {
                    if (PlayerStatus.Lives == 0)
                    {
                        PlayerStatus.Reset();
                        Position = GameManager.ScreenSize / 2;
                    }
                    GameManager.Grid.ApplyDirectedForce(Vector3.forward * 5000, new Vector3(Position.x, Position.y, 0), 50);
                }

                return;
            }

            var aim = Input.GetAimDirection();
            if (aim.sqrMagnitude > 0 && cooldowmRemaining <= 0)
            {
                cooldowmRemaining = cooldownFrames;
                float aimAngle = aim.ToAngle();

                float randomSpread = rand.NextFloat(-0.04f, 0.04f) + rand.NextFloat(-0.04f, 0.04f);
                Vector2 vel = MathUtil.FromPolar(aimAngle + randomSpread, 11f);

                Vector2 offset = new Vector2(35, -8).Rotate(aimAngle);
                EntityManager.Add(new Bullet(Position + offset, vel));

                offset = new Vector2(35, 8).Rotate(aimAngle);
                EntityManager.Add(new Bullet(Position + offset, vel));

                Sound.PlayClip(Sound.Shot, 0.2f);
            }

            if (cooldowmRemaining > 0)
                cooldowmRemaining--;

            const float speed = 8;
            Velocity += speed * Input.GetMovementDirection();
            Position += Velocity;
            Position = MathUtil.Clamp(Position, Size / 2, GameManager.ScreenSize - Size / 2);

            if (Velocity.sqrMagnitude > 0)
                Orientation = Velocity.ToAngle();

            MakeExhaustFire();
            Velocity = Vector2.zero;
        }

        private void MakeExhaustFire()
        {
            if (Velocity.sqrMagnitude > 0.1f)
            {
                Orientation = Velocity.ToAngle();

                double t = GameManager.TotalTime;
                Vector2 baseVel = Velocity.ScaleTo(-3);
                Vector2 perpVel = new Vector2(baseVel.y, -baseVel.x) * (0.6f * (float)Math.Sin(t * 10));
                Color sideColor = new Color(200 / 255f, 38 / 255f, 9 / 255f);
                Color midColor = new Color(255 / 255f, 187 / 255f, 30 / 255f);
                Vector2 pos = Position + new Vector2(-25, 0).Rotate(Orientation);
                const float alpha = 0.7f;

                Vector2 velMid = baseVel + rand.NextVector2(0, 1);
                GameManager.ParticleManager.CreateParticle(Art.Laser, pos, Color.white * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(velMid, ParticleType.Enemy));
                GameManager.ParticleManager.CreateParticle(Art.Glow, pos, midColor * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(velMid, ParticleType.Enemy));

                Vector2 vel1 = baseVel + perpVel + rand.NextVector2(0, 0.3f);
                Vector2 vel2 = baseVel - perpVel + rand.NextVector2(0, 0.3f);
                GameManager.ParticleManager.CreateParticle(Art.Laser, pos, Color.white * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel1, ParticleType.Enemy));
                GameManager.ParticleManager.CreateParticle(Art.Laser, pos, Color.white * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel2, ParticleType.Enemy));

                GameManager.ParticleManager.CreateParticle(Art.Glow, pos, sideColor * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel1, ParticleType.Enemy));
                GameManager.ParticleManager.CreateParticle(Art.Glow, pos, sideColor * alpha, 60f, new Vector2(0.5f, 1),
                    new ParticleState(vel2, ParticleType.Enemy));
            }
        }

        public override void Draw()
        {
            if (!IsDead)
                base.Draw();
        }

        public void Kill()
        {
            PlayerStatus.RemoveLife();
            framesUntilRespawn = PlayerStatus.IsGameOver ? 300 : 120;

            Color explosionColor = new Color(0.8f, 0.8f, 0.4f);

            for (int i = 0; i < 1200; i++)
            {
                float speed = 18f * (1f - 1 / rand.NextFloat(1f, 10f));
                Color color = Color.Lerp(Color.white, explosionColor, rand.NextFloat(0, 1));
                var state = new ParticleState()
                {
                    Velocity = rand.NextVector2(speed, speed),
                    Type = ParticleType.None,
                    LengthMultiplier = 1
                };

                GameManager.ParticleManager.CreateParticle(Art.Laser, Position, color, 190, 1.5f, state);
            }
        }
    }
}