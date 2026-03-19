using System;
using System.Collections.Generic;
using Agents;
using Agents.Enemies;
using Gamelib.EventSystem;
using Gamelib.ObjectPool.Runtime;
using Systems.GameEvents;
using Systems.StageSystem;
using UnityEngine;

namespace Systems.Stages
{
    public class InfiniteStageManager : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private InfiniteStageConfigSO config;
        [SerializeField] private PoolManagerSo poolManager;
        [SerializeField] private EventChannelSO stageEventChannel;
        [SerializeField] private EventChannelSO uiEventChannel;
        [SerializeField] private EventChannelSO playerChannel;

        [Header("Camera")]
        [SerializeField] private MonoBehaviour cameraBoundaryApplierProvider; // IStageCameraBoundaryApplier 구현체

        [Header("Options")]
        [SerializeField] private bool startOnStart = true;

        private Transform _playerTransform;
        private IMover _playerMover;
        private IStageCameraBoundaryApplier _cameraBoundaryApplier;

        private readonly List<AbstractEnemy> _spawnedEnemies = new();
        private readonly List<StageEnemySpawnPoint> _spawnPointBuffer = new();
        private readonly HashSet<int> _countedDeadEnemyIds = new();

        private StageRoom _currentRoom;
        private StageMapDataSO _lastMapData;

        private int _currentRunId;
        private int _currentRoomId;
        private int _currentStageIndex;
        private int _nextEnemyRuntimeId = 1;
        private int _aliveEnemyCount;

        private bool _isRunActive;
        private bool _isTransitioning;

        private void Awake()
        {
            playerChannel.AddListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
            stageEventChannel.AddListener<EnemyDeadEvent>(HandleEnemyDead);
            stageEventChannel.AddListener<NextRoomRequestedEvent>(HandleNextRoomRequested);
        }

        private void HandlePlayerDataSetUp(PlayerDataSetUpEvent obj)
        {
            _playerTransform = obj.PlayerData.Player.transform;
            _playerMover = obj.PlayerData.Player.GetModule<IMover>();
            _cameraBoundaryApplier = cameraBoundaryApplierProvider as IStageCameraBoundaryApplier;

            Debug.Assert(_playerTransform != null, $"{name} : playerTransform이 없습니다.");
            Debug.Assert(_playerMover != null, $"{name} : playerMoverProvider는 IMover를 구현해야 합니다.");
            Debug.Assert(_cameraBoundaryApplier != null,
                    $"{name} : cameraBoundaryApplierProvider는 IStageCameraBoundaryApplier를 구현해야 합니다.");
        }

        private void Start()
        {
            if (startOnStart)
                StartRun();
        }

        private void OnDestroy()
        {
            if (stageEventChannel != null)
            {
                playerChannel.RemoveListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
                stageEventChannel.RemoveListener<EnemyDeadEvent>(HandleEnemyDead);
                stageEventChannel.RemoveListener<NextRoomRequestedEvent>(HandleNextRoomRequested);
            }
        }

        public void StartRun()
        {
            StopRun();

            _isRunActive = true;
            _isTransitioning = false;

            _currentRunId = UnityEngine.Random.Range(1, 1_000_000_000);
            _currentRoomId = 0;
            _currentStageIndex = 0;
            _nextEnemyRuntimeId = 1;
            _lastMapData = null;

            SpawnNextRoomImmediate();
        }

        public void StopRun()
        {
            _isRunActive = false;
            _isTransitioning = false;

            if (_playerMover != null)
                _playerMover.CanManualMovement = true;

            ClearCurrentEnemies();
            ClearCurrentRoom();

            _countedDeadEnemyIds.Clear();
            _aliveEnemyCount = 0;
        }

        public void HandlePlayerDead()
        {
            StopRun();
        }

        private void HandleEnemyDead(EnemyDeadEvent evt)
        {
            if (!_isRunActive || _isTransitioning)
                return;

            if (evt.RunId != _currentRunId || evt.RoomId != _currentRoomId)
                return;

            if (_countedDeadEnemyIds.Add(evt.EnemyRuntimeId) == false)
                return;

            _aliveEnemyCount = Mathf.Max(0, _aliveEnemyCount - 1);

            if (_aliveEnemyCount == 0)
            {
                stageEventChannel.RaiseEvent(
                    StageEvents.RoomCleared.Init(_currentRunId, _currentRoomId));
            }
        }

        private void HandleNextRoomRequested(NextRoomRequestedEvent evt)
        {
            if (!_isRunActive || _isTransitioning)
                return;

            if (evt.RunId != _currentRunId || evt.RoomId != _currentRoomId)
                return;

            BeginNextRoomTransition();
        }

        private void BeginNextRoomTransition()
        {
            _isTransitioning = true;

            if (_playerMover != null)
                _playerMover.CanManualMovement = false;

            RequestFade(true, HandleFadeCovered);
        }

        private void HandleFadeCovered()
        {
            ClearCurrentEnemies();
            ClearCurrentRoom();

            SpawnNextRoomImmediate();

            RequestFade(false, FinishTransition);
        }

        private void FinishTransition()
        {
            if (_playerMover != null)
                _playerMover.CanManualMovement = true;

            _isTransitioning = false;
        }

        private void SpawnNextRoomImmediate()
        {
            StageMapDataSO nextMapData = SelectNextMapData();
            if (nextMapData == null)
            {
                Debug.LogError($"{name} : 사용할 StageMapDataSO가 없습니다.");
                StopRun();
                return;
            }

            StageRoom nextRoom = poolManager.Pop<StageRoom>(nextMapData.RoomPoolItem);
            if (nextRoom == null)
            {
                Debug.LogError($"{name} : {nextMapData.name} 맵을 풀에서 가져오지 못했습니다.");
                StopRun();
                return;
            }

            _currentStageIndex++;
            _currentRoomId++;
            _countedDeadEnemyIds.Clear();
            _aliveEnemyCount = 0;

            _currentRoom = nextRoom;
            _currentRoom.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _currentRoom.Bind(stageEventChannel, _currentRunId, _currentRoomId);

            if (_cameraBoundaryApplier != null && _currentRoom.CameraBoundary != null)
                _cameraBoundaryApplier.ApplyBoundary(_currentRoom.CameraBoundary);

            WarpPlayerToRoomStart(_currentRoom);

            _aliveEnemyCount = SpawnEnemies(nextMapData, _currentRoom);

            if (_aliveEnemyCount == 0)
            {
                stageEventChannel.RaiseEvent(
                    StageEvents.RoomCleared.Init(_currentRunId, _currentRoomId));
            }

            _lastMapData = nextMapData;
        }

        private StageMapDataSO SelectNextMapData()
        {
            StageMapDataSO[] mapList = config.MapList;
            if (mapList == null || mapList.Length == 0)
                return null;

            if (!config.AvoidImmediateRepeat || mapList.Length == 1 || _lastMapData == null)
                return mapList[UnityEngine.Random.Range(0, mapList.Length)];

            StageMapDataSO selected;
            int safety = 16;

            do
            {
                selected = mapList[UnityEngine.Random.Range(0, mapList.Length)];
                safety--;
            }
            while (selected == _lastMapData && safety > 0);

            return selected;
        }

        private int SpawnEnemies(StageMapDataSO mapData, StageRoom room)
        {
            StageEnemySpawnPoint[] spawnPoints = room.EnemySpawnPoints;
            if (spawnPoints == null || spawnPoints.Length == 0)
                return 0;

            int spawnCount = mapData.GetSpawnCount(spawnPoints.Length);
            if (spawnCount <= 0)
                return 0;

            _spawnPointBuffer.Clear();
            _spawnPointBuffer.AddRange(spawnPoints);
            Shuffle(_spawnPointBuffer);

            StageEnemyModifierData modifierData = config.CreateModifierData(_currentStageIndex);

            int actualSpawnCount = 0;

            for (int i = 0; i < spawnCount; i++)
            {
                if (!mapData.TryGetRandomEnemyPoolItem(out PoolItemSo enemyPoolItem) || enemyPoolItem == null)
                    continue;

                AbstractEnemy enemy = poolManager.Pop<AbstractEnemy>(enemyPoolItem);
                if (enemy == null)
                    continue;

                StageEnemySpawnPoint spawnPoint = _spawnPointBuffer[i];
                enemy.transform.SetPositionAndRotation(spawnPoint.Position, spawnPoint.Rotation);

                IStageSpawnedEnemy stageEnemy = enemy.GetComponent<IStageSpawnedEnemy>();
                Debug.Assert(stageEnemy != null,
                    $"{enemy.name} : StageSpawnedEnemy 또는 IStageSpawnedEnemy 구현체가 필요합니다.");

                stageEnemy.Bind(
                    stageEventChannel,
                    _currentRunId,
                    _currentRoomId,
                    CreateEnemyRuntimeId(),
                    modifierData);

                _spawnedEnemies.Add(enemy);
                actualSpawnCount++;
            }

            return actualSpawnCount;
        }

        private void WarpPlayerToRoomStart(StageRoom room)
        {
            if (_playerTransform == null || room.PlayerSpawnPoint == null)
                return;

            Vector3 spawnPosition = room.PlayerSpawnPoint.position;

            if (_playerMover != null && _playerMover.Rigidbody2D != null)
            {
                _playerMover.StopImmediately(true, true);
                _playerMover.Rigidbody2D.position = spawnPosition;
                _playerMover.Rigidbody2D.linearVelocity = Vector2.zero;
            }
            else
            {
                _playerTransform.position = spawnPosition;
            }
        }

        private void ClearCurrentEnemies()
        {
            foreach (var enemy in _spawnedEnemies)
            {
                if (enemy == null || enemy.gameObject == null || !enemy.gameObject.activeSelf)
                    continue;

                poolManager.Push(enemy);
            }

            _spawnedEnemies.Clear();
            _countedDeadEnemyIds.Clear();
            _aliveEnemyCount = 0;
        }

        private void ClearCurrentRoom()
        {
            if (_currentRoom == null || _currentRoom.gameObject == null || !_currentRoom.gameObject.activeSelf)
            {
                _currentRoom = null;
                return;
            }

            poolManager.Push(_currentRoom);
            _currentRoom = null;
        }

        private int CreateEnemyRuntimeId() => _nextEnemyRuntimeId++;

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        private void RequestFade(bool isFadeIn, Action onFadeEnd)
        {
            if (uiEventChannel == null)
            {
                onFadeEnd?.Invoke();
                return;
            }

            // 네 프로젝트의 FadeEvent가 이전에 보여준 Init 패턴이라는 전제
            uiEventChannel.RaiseEvent(
                new FadeEvent().Init(isFadeIn, config.TransitionDuration, onFadeEnd));
        }
    }
}