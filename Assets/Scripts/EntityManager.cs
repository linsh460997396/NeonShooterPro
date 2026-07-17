using UnityEngine;
using System.Collections.Generic;

namespace NeonShooter
{
    public static class EntityManager
    {
        private const int InitialCapacity = 200;
        private const float HashCellSize = 100f;

        private static Entity[] entities = new Entity[InitialCapacity];
        private static int entityCount;

        private static Enemy[] enemies = new Enemy[InitialCapacity];
        private static int enemyCount;

        private static Bullet[] bullets = new Bullet[InitialCapacity];
        private static int bulletCount;

        private static BlackHole[] blackHoles = new BlackHole[InitialCapacity];
        private static int blackHoleCount;

        private static SpatialHash spatialHash = new SpatialHash(HashCellSize);

        private static bool isUpdating;
        private static List<Entity> addedEntities = new List<Entity>();

        public static int Count { get { return entityCount; } }
        public static int BlackHoleCount { get { return blackHoleCount; } }

        public static IEnumerable<BlackHole> BlackHoles
        {
            get
            {
                for (int i = 0; i < blackHoleCount; i++)
                    yield return blackHoles[i];
            }
        }

        public static void Add(Entity entity)
        {
            if (!isUpdating)
                AddEntity(entity);
            else
                addedEntities.Add(entity);
        }

        private static void AddEntity(Entity entity)
        {
            if (entityCount >= entities.Length)
                System.Array.Resize(ref entities, entities.Length * 2);
            entities[entityCount++] = entity;

            if (entity is Bullet)
            {
                if (bulletCount >= bullets.Length)
                    System.Array.Resize(ref bullets, bullets.Length * 2);
                bullets[bulletCount++] = (Bullet)entity;
            }
            else if (entity is Enemy)
            {
                if (enemyCount >= enemies.Length)
                    System.Array.Resize(ref enemies, enemies.Length * 2);
                enemies[enemyCount++] = (Enemy)entity;
            }
            else if (entity is BlackHole)
            {
                if (blackHoleCount >= blackHoles.Length)
                    System.Array.Resize(ref blackHoles, blackHoles.Length * 2);
                blackHoles[blackHoleCount++] = (BlackHole)entity;
            }
        }

        public static void Update()
        {
            isUpdating = true;

            spatialHash.Clear();
            for (int i = 0; i < entityCount; i++)
                spatialHash.Insert(entities[i]);

            HandleCollisions();

            for (int i = 0; i < entityCount; i++)
                entities[i].Update();

            isUpdating = false;

            for (int i = 0; i < addedEntities.Count; i++)
                AddEntity(addedEntities[i]);
            addedEntities.Clear();

            CompactArrays();
        }

        private static void CompactArrays()
        {
            int newCount = 0;
            int newEnemyCount = 0;
            int newBulletCount = 0;
            int newBlackHoleCount = 0;

            for (int i = 0; i < entityCount; i++)
            {
                if (!entities[i].IsExpired)
                {
                    entities[newCount] = entities[i];

                    if (entities[i] is Enemy)
                        enemies[newEnemyCount++] = (Enemy)entities[i];
                    else if (entities[i] is Bullet)
                        bullets[newBulletCount++] = (Bullet)entities[i];
                    else if (entities[i] is BlackHole)
                        blackHoles[newBlackHoleCount++] = (BlackHole)entities[i];

                    newCount++;
                }
            }

            entityCount = newCount;
            enemyCount = newEnemyCount;
            bulletCount = newBulletCount;
            blackHoleCount = newBlackHoleCount;
        }

        private static void HandleCollisions()
        {
            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = enemies[i];
                if (enemy.IsExpired) continue;

                foreach (var nearby in spatialHash.QueryNearby(enemy))
                {
                    if (nearby is Enemy otherEnemy)
                    {
                        if (!otherEnemy.IsExpired && IsColliding(enemy, otherEnemy))
                        {
                            enemy.HandleCollision(otherEnemy);
                            otherEnemy.HandleCollision(enemy);
                        }
                    }
                }
            }

            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = enemies[i];
                if (enemy.IsExpired || !enemy.IsActive) continue;

                foreach (var nearby in spatialHash.QueryNearby(enemy))
                {
                    if (nearby is Bullet bullet)
                    {
                        if (!bullet.IsExpired && IsColliding(enemy, bullet))
                        {
                            enemy.WasShot();
                            bullet.IsExpired = true;
                        }
                    }
                }
            }

            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = enemies[i];
                if (enemy.IsActive && !enemy.IsExpired && IsColliding(PlayerShip.Instance, enemy))
                {
                    KillPlayer();
                    break;
                }
            }

            for (int i = 0; i < blackHoleCount; i++)
            {
                var blackHole = blackHoles[i];
                if (blackHole.IsExpired) continue;

                foreach (var nearby in spatialHash.QueryNearby(blackHole))
                {
                    if (nearby is Enemy enemy)
                    {
                        if (enemy.IsActive && !enemy.IsExpired && IsColliding(blackHole, enemy))
                            enemy.WasShot();
                    }
                    else if (nearby is Bullet bullet)
                    {
                        if (!bullet.IsExpired && IsColliding(blackHole, bullet))
                        {
                            bullet.IsExpired = true;
                            blackHole.WasShot();
                        }
                    }
                }

                if (IsColliding(PlayerShip.Instance, blackHole))
                {
                    KillPlayer();
                    break;
                }
            }
        }

        private static void KillPlayer()
        {
            PlayerShip.Instance.Kill();
            for (int i = 0; i < enemyCount; i++)
                enemies[i].WasShot();
            for (int i = 0; i < blackHoleCount; i++)
                blackHoles[i].Kill();
            EnemySpawner.Reset();
        }

        private static bool IsColliding(Entity a, Entity b)
        {
            float radius = a.Radius + b.Radius;
            return !a.IsExpired && !b.IsExpired && Vector2.SqrMagnitude(a.Position - b.Position) < radius * radius;
        }

        public static IEnumerable<Entity> GetNearbyEntities(Vector2 position, float radius)
        {
            HashSet<Entity> results = new HashSet<Entity>();
            float minX = position.x - radius;
            float maxX = position.x + radius;
            float minY = position.y - radius;
            float maxY = position.y + radius;

            for (int i = 0; i < entityCount; i++)
            {
                var entity = entities[i];
                if (!entity.IsExpired &&
                    entity.Position.x >= minX && entity.Position.x <= maxX &&
                    entity.Position.y >= minY && entity.Position.y <= maxY &&
                    Vector2.SqrMagnitude(position - entity.Position) < radius * radius)
                {
                    results.Add(entity);
                }
            }

            return results;
        }

        public static void Draw()
        {
            for (int i = 0; i < entityCount; i++)
                entities[i].Draw();
        }
    }
}