using Agents.Enemies;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Stages
{
    [RequireComponent(typeof(AbstractEnemy))]
    public class EnemyDeathEventRelay : MonoBehaviour, IEnemyStageEventRelay
    {
        private AbstractEnemy _enemy;
        private EventChannelSO _stageEventChannel;

        private int _runId = -1;
        private int _roomId = -1;
        private int _enemyRuntimeId = -1;
        private bool _isRaised;

        private void Awake()
        {
            _enemy = GetComponent<AbstractEnemy>();
            _enemy.onDeath.AddListener(HandleDeath);
        }

        private void OnDestroy()
        {
            if (_enemy != null)
                _enemy.onDeath.RemoveListener(HandleDeath);
        }

        private void OnEnable()
        {
            _isRaised = false;
        }

        public void Bind(EventChannelSO stageEventChannel, int runId, int roomId, int enemyRuntimeId)
        {
            _stageEventChannel = stageEventChannel;
            _runId = runId;
            _roomId = roomId;
            _enemyRuntimeId = enemyRuntimeId;
            _isRaised = false;
        }

        public void ClearBinding()
        {
            _stageEventChannel = null;
            _runId = -1;
            _roomId = -1;
            _enemyRuntimeId = -1;
            _isRaised = false;
        }

        private void HandleDeath()
        {
            if (_isRaised || _stageEventChannel == null)
                return;

            _isRaised = true;
            _stageEventChannel.RaiseEvent(
                StageEvents.EnemyDead.Init(_runId, _roomId, _enemyRuntimeId));
        }
    }
}