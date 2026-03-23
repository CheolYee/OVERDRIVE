/*using System;
using System.Collections;
using Agents.Enemies;
using Gamelib.ObjectPool.Runtime;
using Gamelib.SoundSystem;
using Systems.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Systems.Environments
{
    public class EnemyPortal : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private PoolManagerSo poolManager;
        [SerializeField] private PoolItemSo enemyPoolItem;

        [Header("Spawn")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnEnable = true;
        [SerializeField] private bool loopSpawn = true;
        [SerializeField, Min(0f)] private float firstDelay = 0f;
        [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
        [SerializeField, Min(1)] private int spawnCountPerCycle = 1;

        [Header("Random Offset")]
        [SerializeField] private Vector2 randomOffsetMin;
        [SerializeField] private Vector2 randomOffsetMax;

        private Coroutine _spawnRoutine;

        private void OnEnable()
        {
            if (spawnOnEnable)
                StartSpawn();
        }

        private void OnDisable()
        {
            StopSpawn();
        }

        public void StartSpawn()
        {
            if (_spawnRoutine != null)
                return;

            _spawnRoutine = StartCoroutine(SpawnRoutine());
        }

        public void StopSpawn()
        {
            if (_spawnRoutine == null)
                return;

            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        [ContextMenu("Spawn Once")]
        public void SpawnOnce()
        {
            for (int i = 0; i < spawnCountPerCycle; i++)
            {
                TrySpawnEnemy();
            }
        }

        private IEnumerator SpawnRoutine()
        {
            if (firstDelay > 0f)
                yield return new WaitForSeconds(firstDelay);

            do
            {
                for (int i = 0; i < spawnCountPerCycle; i++)
                {
                    TrySpawnEnemy();
                }

                if (!loopSpawn)
                    break;

                yield return new WaitForSeconds(spawnInterval);
            }
            while (true);

            _spawnRoutine = null;
        }

        private bool TrySpawnEnemy()
        {
            Debug.Assert(poolManager != null, $"{name} : PoolManagerSo가 비어 있습니다.");
            Debug.Assert(enemyPoolItem != null, $"{name} : enemyPoolItem이 비어 있습니다.");

            if (poolManager == null || enemyPoolItem == null)
                return false;

            AbstractEnemy enemy = poolManager.Pop<AbstractEnemy>(enemyPoolItem);
            if (enemy == null)
            {
                Debug.LogWarning($"{name} : {enemyPoolItem.name} 풀에서 적을 가져오지 못했습니다.");
                return false;
            }

            Transform enemyTrm = enemy.transform;

            Vector3 spawnPosition = GetSpawnPosition();
            Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            enemyTrm.SetPositionAndRotation(spawnPosition, spawnRotation);
            enemy.gameObject.SetActive(true);
            enemy.ResetItem();

            return true;
        }

        private Vector3 GetSpawnPosition()
        {
            Vector3 basePosition = spawnPoint != null ? spawnPoint.position : transform.position;

            float offsetX = Random.Range(randomOffsetMin.x, randomOffsetMax.x);
            float offsetY = Random.Range(randomOffsetMin.y, randomOffsetMax.y);

            return basePosition + new Vector3(offsetX, offsetY, 0f);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, 0.2f);

            Vector3 size = new Vector3(
                Mathf.Abs(randomOffsetMax.x - randomOffsetMin.x),
                Mathf.Abs(randomOffsetMax.y - randomOffsetMin.y),
                0.1f);

            Vector3 boxCenter = center + new Vector3(
                (randomOffsetMin.x + randomOffsetMax.x) * 0.5f,
                (randomOffsetMin.y + randomOffsetMax.y) * 0.5f,
                0f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(boxCenter, size);
        }
    }
}*/