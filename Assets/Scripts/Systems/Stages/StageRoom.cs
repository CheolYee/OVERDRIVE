using Gamelib.EventSystem;
using Gamelib.ObjectPool.Runtime;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Stages
{
    public class StageEnemySpawnPoint : MonoBehaviour
    {
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
    }

    public class StageRoom : PoolableMono
    {
        [Header("Room References")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private StageExitDoor exitDoor;
        [SerializeField] private Collider2D cameraBoundary;
        [SerializeField] private StageEnemySpawnPoint[] enemySpawnPoints;

        private EventChannelSO _stageEventChannel;
        private int _runId = -1;
        private int _roomId = -1;

        public Transform PlayerSpawnPoint => playerSpawnPoint;
        public Collider2D CameraBoundary => cameraBoundary;
        public StageEnemySpawnPoint[] EnemySpawnPoints => enemySpawnPoints;

        public void Bind(EventChannelSO stageEventChannel, int runId, int roomId)
        {
            Unsubscribe();

            _stageEventChannel = stageEventChannel;
            _runId = runId;
            _roomId = roomId;

            _stageEventChannel?.AddListener<RoomClearedEvent>(HandleRoomCleared);

            exitDoor?.Bind(stageEventChannel, runId, roomId);
            CloseDoor();
        }

        public void OpenDoor() => exitDoor?.SetOpen(true);
        public void CloseDoor() => exitDoor?.SetOpen(false);

        private void HandleRoomCleared(RoomClearedEvent evt)
        {
            if (evt.RunId != _runId || evt.RoomId != _roomId)
                return;

            OpenDoor();
        }

        public override void ResetItem()
        {
            base.ResetItem();

            Unsubscribe();

            _runId = -1;
            _roomId = -1;

            if (exitDoor != null)
            {
                exitDoor.ClearBinding();
                exitDoor.SetOpen(false);
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_stageEventChannel != null)
            {
                _stageEventChannel.RemoveListener<RoomClearedEvent>(HandleRoomCleared);
                _stageEventChannel = null;
            }
        }

        private void OnValidate()
        {
            if (exitDoor == null)
                exitDoor = GetComponentInChildren<StageExitDoor>(true);

            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
                enemySpawnPoints = GetComponentsInChildren<StageEnemySpawnPoint>(true);
        }
    }
}