using System;
using Gamelib.ObjectPool.Runtime;
using UnityEngine;

namespace Systems.Stages
{
    [CreateAssetMenu(fileName = "StageMapData", menuName = "Stage/Stage Map Data", order = 0)]
    public class StageMapDataSO : ScriptableObject
    {
        [Serializable]
        public struct EnemySpawnOption
        {
            public PoolItemSo enemyPoolItem;
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

        public bool TryGetRandomEnemyPoolItem(out PoolItemSo enemyPoolItem)
        {
            enemyPoolItem = null;

            if (enemyOptions == null || enemyOptions.Length == 0)
                return false;

            int totalWeight = 0;
            foreach (var option in enemyOptions)
            {
                if (option.enemyPoolItem == null)
                    continue;

                totalWeight += option.weight;
            }

            if (totalWeight <= 0)
                return false;

            int random = UnityEngine.Random.Range(0, totalWeight);

            foreach (var option in enemyOptions)
            {
                if (option.enemyPoolItem == null)
                    continue;

                random -= option.weight;
                if (random < 0)
                {
                    enemyPoolItem = option.enemyPoolItem;
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
    }
}