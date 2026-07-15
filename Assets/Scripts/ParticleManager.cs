using UnityEngine;
using System;

namespace NeonShooter
{
    public class ParticleManager<T>
    {
        private Action<Particle> updateParticle;
        private CircularParticleArray particleList;

        public ParticleManager(int capacity, Action<Particle> updateParticle)
        {
            this.updateParticle = updateParticle;
            particleList = new CircularParticleArray(capacity);

            for (int i = 0; i < capacity; i++)
                particleList[i] = new Particle();
        }

        public void Update()
        {
            int removalCount = 0;
            for (int i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];

                updateParticle(particle);

                particle.PercentLife -= 1f / particle.Duration;

                Swap(particleList, i - removalCount, i);

                if (particle.PercentLife < 0)
                    removalCount++;
            }
            particleList.Count -= removalCount;
        }

        private static void Swap(CircularParticleArray list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        public void Draw()
        {
            for (int i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];

                Vector2 origin = new Vector2(particle.Texture.width / 2, particle.Texture.height / 2);

                GUIUtility.RotateAroundPivot(particle.Orientation * Mathf.Rad2Deg, particle.Position);

                GUI.color = particle.Tint;
                GUI.DrawTexture(new Rect(particle.Position.x - origin.x * particle.Scale.x, particle.Position.y - origin.y * particle.Scale.y,
                    particle.Texture.width * particle.Scale.x, particle.Texture.height * particle.Scale.y), particle.Texture);

                GUIUtility.RotateAroundPivot(-particle.Orientation * Mathf.Rad2Deg, particle.Position);
            }
            GUI.color = Color.white;
        }

        public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, float scale, T state, float theta = 0)
        {
            CreateParticle(texture, position, tint, duration, new Vector2(scale, scale), state, theta);
        }

        public void CreateParticle(Texture2D texture, Vector2 position, Color tint, float duration, Vector2 scale, T state, float theta = 0)
        {
            Particle particle;
            if (particleList.Count == particleList.Capacity)
            {
                particle = particleList[0];
                particleList.Start++;
            }
            else
            {
                particle = particleList[particleList.Count];
                particleList.Count++;
            }

            particle.Texture = texture;
            particle.Position = position;
            particle.Tint = tint;

            particle.Duration = duration;
            particle.PercentLife = 1f;
            particle.Scale = scale;
            particle.Orientation = theta;
            particle.State = state;
        }

        public void Clear()
        {
            particleList.Count = 0;
        }

        public int ParticleCount
        {
            get { return particleList.Count; }
        }

        public class Particle
        {
            public Texture2D Texture;
            public Vector2 Position;
            public float Orientation;

            public Vector2 Scale = Vector2.one;

            public Color Tint;
            public float Duration;
            public float PercentLife = 1f;
            public T State;
        }

        private class CircularParticleArray
        {
            private int start;
            public int Start
            {
                get { return start; }
                set { start = value % list.Length; }
            }

            public int Count { get; set; }
            public int Capacity { get { return list.Length; } }
            private Particle[] list;

            public CircularParticleArray(int capacity)
            {
                list = new Particle[capacity];
            }

            public Particle this[int i]
            {
                get { return list[(start + i) % list.Length]; }
                set { list[(start + i) % list.Length] = value; }
            }
        }
    }
}