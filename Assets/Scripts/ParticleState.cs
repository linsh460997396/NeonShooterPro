using UnityEngine;

namespace NeonShooter
{
    public enum ParticleType
    {
        None,
        Enemy,
        Bullet,
        IgnoreGravity
    }

    public struct ParticleState
    {
        public Vector2 Velocity;
        public ParticleType Type;
        public float LengthMultiplier;

        public ParticleState(Vector2 velocity, ParticleType type)
        {
            Velocity = velocity;
            Type = type;
            LengthMultiplier = 1;
        }

        public ParticleState(Vector2 velocity, ParticleType type, float lengthMultiplier)
        {
            Velocity = velocity;
            Type = type;
            LengthMultiplier = lengthMultiplier;
        }

        public static void UpdateParticle(ParticleManager<ParticleState>.Particle particle)
        {
            var state = particle.State;
            particle.Position += state.Velocity;
            particle.Orientation = state.Velocity.ToAngle();
            particle.Scale = new Vector2(1, Mathf.Min(particle.Scale.y, particle.PercentLife * state.LengthMultiplier));

            if (state.Type != ParticleType.IgnoreGravity)
            {
                foreach (var blackHole in EntityManager.BlackHoles)
                {
                    Vector2 dPos = blackHole.Position - particle.Position;
                    float length = dPos.magnitude;
                    if (length < 300)
                    {
                        state.Velocity += dPos.ScaleTo(Mathf.Lerp(2, 0, length / 300f));
                    }
                }
            }

            particle.State = state;
        }
    }
}