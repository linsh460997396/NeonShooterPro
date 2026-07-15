using UnityEngine;
using System;

namespace NeonShooter
{
    public static class EnemySpawner
    {
        static System.Random rand = new System.Random();
        static float inverseSpawnChance = 90;
        static float inverseBlackHoleChance = 600;

        public static void Update()
        {
            if (!PlayerShip.Instance.IsDead && EntityManager.Count < 200)
            {
                if (rand.Next((int)inverseSpawnChance) == 0)
                    EntityManager.Add(Enemy.CreateSeeker(GetSpawnPosition()));

                if (rand.Next((int)inverseSpawnChance) == 0)
                    EntityManager.Add(Enemy.CreateWanderer(GetSpawnPosition()));

                if (EntityManager.BlackHoleCount < 2 && rand.Next((int)inverseBlackHoleChance) == 0)
                    EntityManager.Add(new BlackHole(GetSpawnPosition()));
            }

            if (inverseSpawnChance > 30)
                inverseSpawnChance -= 0.005f;
        }

        private static Vector2 GetSpawnPosition()
        {
            Vector2 pos;
            do
            {
                pos = new Vector2(rand.Next((int)GameManager.ScreenSize.x), rand.Next((int)GameManager.ScreenSize.y));
            }
            while (Vector2.SqrMagnitude(pos - PlayerShip.Instance.Position) < 250 * 250);

            return pos;
        }

        public static void Reset()
        {
            inverseSpawnChance = 90;
        }
    }
}