using System;
using Gamelib.ObjectPool.Runtime;
using Systems.Database;
using UnityEngine;

namespace Systems.Stages
{
    [CreateAssetMenu(fileName = "StageMapData", menuName = "Stage/Stage Map Data", order = 0)]
    public class StageMapDataSO : ScriptableObject
    {
        [Serializable]
        public struct EnemySpawnOption
        {
            public EnemyDataSo enemyData;
            [Min(1)] public int weight;
        }
        [Serializable]
        public struct ChestSpawnOption
        {
            public PoolItemSo chestPoolItem;
            [Min(1)] public int weight;
        }
        

        [field: Header("Room")]
        [field: SerializeField] public PoolItemSo RoomPoolItem { get; private set; }

        [Header("Enemy Spawn")]
        [SerializeField, Min(0)] private int minSpawnCount = 2;
        [SerializeField, Min(0)] private int maxSpawnCount = 4;
        [SerializeField] private EnemySpawnOption[] enemyOptions;

        public int GetSpawnCount(int availableSpawnPointCount)
        {
            if (availableSpawnPointCount <= 0)
                return 0;

            int clampedMin = Mathf.Clamp(minSpawnCount, 0, availableSpawnPointCount);
            int clampedMax = Mathf.Clamp(maxSpawnCount, clampedMin, availableSpawnPointCount);

            return UnityEngine.Random.Range(clampedMin, clampedMax + 1);
        }

        public bool TryGetRandomEnemyData(out EnemyDataSo enemyData)
        {
            enemyData = null;

            if (enemyOptions == null || enemyOptions.Length == 0)
                return false;

            int totalWeight = 0;
            foreach (var option in enemyOptions)
            {
                if (option.enemyData == null)
                    continue;

                totalWeight += option.weight;
            }

            if (totalWeight <= 0)
                return false;

            int random = UnityEngine.Random.Range(0, totalWeight);

            foreach (var option in enemyOptions)
            {
                if (option.enemyData == null)
                    continue;

                random -= option.weight;
                if (random < 0)
                {
                    enemyData = option.enemyData;
                    return true;
                }
            }

            return false;
        }

        private void OnValidate()
        {
            if (maxSpawnCount < minSpawnCount)
                maxSpawnCount = minSpawnCount;
        }

        [Header("Chest Spawn")]
        [SerializeField, Min(0)] private int minChestCount;
        [SerializeField, Min(0)] private int maxChestCount = 1;
        [SerializeField] private ChestSpawnOption[] chestOptions;

        public int GetChestSpawnCount(int availableSpawnPointCount)
        {
            if (availableSpawnPointCount <= 0)
                return 0;

            int clampedMin = Mathf.Clamp(minChestCount, 0, availableSpawnPointCount);
            int clampedMax = Mathf.Clamp(maxChestCount, clampedMin, availableSpawnPointCount);

            return UnityEngine.Random.Range(clampedMin, clampedMax + 1);
        }

        public bool TryGetRandomChestPoolItem(out PoolItemSo chestPoolItem)
        {
            chestPoolItem = null;

            if (chestOptions == null || chestOptions.Length == 0)
                return false;

            int totalWeight = 0;
            foreach (var option in chestOptions)
            {
                if (option.chestPoolItem == null)
                    continue;

                totalWeight += option.weight;
            }

            if (totalWeight <= 0)
                return false;

            int random = UnityEngine.Random.Range(0, totalWeight);

            foreach (var option in chestOptions)
            {
                if (option.chestPoolItem == null)
                    continue;

                random -= option.weight;
                if (random < 0)
                {
                    chestPoolItem = option.chestPoolItem;
                    return true;
                }
            }

            return false;
        }
    }
}