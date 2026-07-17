using UnityEngine;
using System.Collections.Generic;

namespace NeonShooter
{
    public class SpatialHash
    {
        private readonly Dictionary<int, List<Entity>> buckets = new Dictionary<int, List<Entity>>();
        private readonly float cellSize;

        public SpatialHash(float cellSize)
        {
            this.cellSize = cellSize;
        }

        private int GetKey(Vector2 position)
        {
            int cellX = Mathf.FloorToInt(position.x / cellSize);
            int cellY = Mathf.FloorToInt(position.y / cellSize);
            return cellX * 100000 + cellY;
        }

        public void Insert(Entity entity)
        {
            int key = GetKey(entity.Position);
            if (!buckets.TryGetValue(key, out var bucket))
            {
                bucket = new List<Entity>();
                buckets[key] = bucket;
            }
            bucket.Add(entity);
        }

        public void Clear()
        {
            foreach (var bucket in buckets.Values)
                bucket.Clear();
        }

        public IEnumerable<Entity> QueryNearby(Entity entity)
        {
            HashSet<Entity> results = new HashSet<Entity>();
            float radius = entity.Radius;
            
            int minCellX = Mathf.FloorToInt((entity.Position.x - radius) / cellSize);
            int maxCellX = Mathf.FloorToInt((entity.Position.x + radius) / cellSize);
            int minCellY = Mathf.FloorToInt((entity.Position.y - radius) / cellSize);
            int maxCellY = Mathf.FloorToInt((entity.Position.y + radius) / cellSize);

            for (int x = minCellX; x <= maxCellX; x++)
            {
                for (int y = minCellY; y <= maxCellY; y++)
                {
                    int key = x * 100000 + y;
                    if (buckets.TryGetValue(key, out var bucket))
                    {
                        foreach (var other in bucket)
                        {
                            if (other != entity && !other.IsExpired)
                                results.Add(other);
                        }
                    }
                }
            }

            return results;
        }
    }
}