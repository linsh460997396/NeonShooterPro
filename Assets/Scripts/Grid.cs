using UnityEngine;
using System;
using System.Collections.Generic;

namespace NeonShooter
{
    public class Grid
    {
        class PointMass
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float InverseMass;

            private Vector3 acceleration;
            private float damping = 0.98f;

            public PointMass(Vector3 position, float invMass)
            {
                Position = position;
                InverseMass = invMass;
            }

            public void ApplyForce(Vector3 force)
            {
                acceleration += force * InverseMass;
            }

            public void IncreaseDamping(float factor)
            {
                damping *= factor;
            }

            public void Update()
            {
                Velocity += acceleration;
                Position += Velocity;
                acceleration = Vector3.zero;
                if (Velocity.sqrMagnitude < 0.001f * 0.001f)
                    Velocity = Vector3.zero;

                Velocity *= damping;
                damping = 0.98f;
            }
        }

        struct Spring
        {
            public PointMass End1;
            public PointMass End2;
            public float TargetLength;
            public float Stiffness;
            public float Damping;

            public Spring(PointMass end1, PointMass end2, float stiffness, float damping)
            {
                End1 = end1;
                End2 = end2;
                Stiffness = stiffness;
                Damping = damping;
                TargetLength = Vector3.Distance(end1.Position, end2.Position) * 0.95f;
            }

            public void Update()
            {
                var x = End1.Position - End2.Position;

                float length = x.magnitude;
                if (length <= TargetLength)
                    return;

                x = (x / length) * (length - TargetLength);
                var dv = End2.Velocity - End1.Velocity;
                var force = Stiffness * x - dv * Damping;

                End1.ApplyForce(-force);
                End2.ApplyForce(force);
            }
        }

        Spring[] springs;
        PointMass[,] points;
        Vector2 screenSize;

        public Grid(Rect rect, Vector2 spacing)
        {
            var springList = new List<Spring>();

            int numColumns = (int)(rect.width / spacing.x) + 1;
            int numRows = (int)(rect.height / spacing.y) + 1;
            points = new PointMass[numColumns, numRows];

            PointMass[,] fixedPoints = new PointMass[numColumns, numRows];

            int column = 0, row = 0;
            for (float y = rect.y; y <= rect.y + rect.height; y += spacing.y)
            {
                for (float x = rect.x; x <= rect.x + rect.width; x += spacing.x)
                {
                    points[column, row] = new PointMass(new Vector3(x, y, 0), 1);
                    fixedPoints[column, row] = new PointMass(new Vector3(x, y, 0), 0);
                    column++;
                }
                row++;
                column = 0;
            }

            for (int y = 0; y < numRows; y++)
                for (int x = 0; x < numColumns; x++)
                {
                    if (x == 0 || y == 0 || x == numColumns - 1 || y == numRows - 1)
                        springList.Add(new Spring(fixedPoints[x, y], points[x, y], 0.1f, 0.1f));
                    else if (x % 3 == 0 && y % 3 == 0)
                        springList.Add(new Spring(fixedPoints[x, y], points[x, y], 0.002f, 0.02f));

                    const float stiffness = 0.28f;
                    const float damping = 0.06f;

                    if (x > 0)
                        springList.Add(new Spring(points[x - 1, y], points[x, y], stiffness, damping));
                    if (y > 0)
                        springList.Add(new Spring(points[x, y - 1], points[x, y], stiffness, damping));
                }

            springs = springList.ToArray();
        }

        public void ApplyDirectedForce(Vector2 force, Vector2 position, float radius)
        {
            ApplyDirectedForce(new Vector3(force.x, force.y, 0), new Vector3(position.x, position.y, 0), radius);
        }

        public void ApplyDirectedForce(Vector3 force, Vector3 position, float radius)
        {
            foreach (var mass in points)
                if (Vector3.SqrMagnitude(position - mass.Position) < radius * radius)
                    mass.ApplyForce(10 * force / (10 + Vector3.Distance(position, mass.Position)));
        }

        public void ApplyImplosiveForce(float force, Vector2 position, float radius)
        {
            ApplyImplosiveForce(force, new Vector3(position.x, position.y, 0), radius);
        }

        public void ApplyImplosiveForce(float force, Vector3 position, float radius)
        {
            foreach (var mass in points)
            {
                float dist2 = Vector3.SqrMagnitude(position - mass.Position);
                if (dist2 < radius * radius)
                {
                    mass.ApplyForce(10 * force * (position - mass.Position) / (100 + dist2));
                    mass.IncreaseDamping(0.6f);
                }
            }
        }

        public void ApplyExplosiveForce(float force, Vector2 position, float radius)
        {
            ApplyExplosiveForce(force, new Vector3(position.x, position.y, 0), radius);
        }

        public void ApplyExplosiveForce(float force, Vector3 position, float radius)
        {
            foreach (var mass in points)
            {
                float dist2 = Vector3.SqrMagnitude(position - mass.Position);
                if (dist2 < radius * radius)
                {
                    mass.ApplyForce(100 * force * (mass.Position - position) / (10000 + dist2));
                    mass.IncreaseDamping(0.6f);
                }
            }
        }

        public void Update()
        {
            foreach (var spring in springs)
                spring.Update();

            foreach (var mass in points)
                mass.Update();
        }

        public void Draw()
        {
            if (BatchRenderer.Instance == null) return;

            screenSize = GameManager.ScreenSize;

            int width = points.GetLength(0);
            int height = points.GetLength(1);
            Color color = new Color(30 / 255f, 30 / 255f, 139 / 255f, 85 / 255f);

            Texture2D pixel = GameManager.Instance != null ? GameManager.Instance.PixelTexture : null;
            if (pixel == null) return;

            var br = BatchRenderer.Instance;

            for (int y = 1; y < height; y++)
            {
                for (int x = 1; x < width; x++)
                {
                    Vector2 left = Vector2.zero, up = Vector2.zero;
                    Vector2 p = ToVec2(points[x, y].Position);
                    if (x > 1)
                    {
                        left = ToVec2(points[x - 1, y].Position);
                        float thickness = y % 3 == 1 ? 3f : 1f;

                        int clampedX = Math.Min(x + 1, width - 1);
                        Vector2 mid = MathUtil.CatmullRom(ToVec2(points[x - 2, y].Position), left, p, ToVec2(points[clampedX, y].Position), 0.5f);

                        if (Vector2.SqrMagnitude(mid - (left + p) / 2) > 1)
                        {
                            br.DrawLine(pixel, left, mid, color, thickness);
                            br.DrawLine(pixel, mid, p, color, thickness);
                        }
                        else
                            br.DrawLine(pixel, left, p, color, thickness);
                    }
                    if (y > 1)
                    {
                        up = ToVec2(points[x, y - 1].Position);
                        float thickness = x % 3 == 1 ? 3f : 1f;
                        int clampedY = Math.Min(y + 1, height - 1);
                        Vector2 mid = MathUtil.CatmullRom(ToVec2(points[x, y - 2].Position), up, p, ToVec2(points[x, clampedY].Position), 0.5f);

                        if (Vector2.SqrMagnitude(mid - (up + p) / 2) > 1)
                        {
                            br.DrawLine(pixel, up, mid, color, thickness);
                            br.DrawLine(pixel, mid, p, color, thickness);
                        }
                        else
                            br.DrawLine(pixel, up, p, color, thickness);
                    }

                    if (x > 1 && y > 1)
                    {
                        Vector2 upLeft = ToVec2(points[x - 1, y - 1].Position);
                        br.DrawLine(pixel, 0.5f * (upLeft + up), 0.5f * (left + p), color, 1f);
                        br.DrawLine(pixel, 0.5f * (upLeft + left), 0.5f * (up + p), color, 1f);
                    }
                }
            }
        }

        public Vector2 ToVec2(Vector3 v)
        {
            float factor = (v.z + 2000) / 2000;
            return (new Vector2(v.x, v.y) - screenSize / 2f) * factor + screenSize / 2;
        }
    }
}