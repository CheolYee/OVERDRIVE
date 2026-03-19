using Agents.Enemies;
using Agents.StatSystem;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Stages
{
    public interface IStageSpawnedEnemy
    {
        void Bind(
            EventChannelSO stageEventChannel,
            int runId,
            int roomId,
            int enemyRuntimeId,
            StageEnemyModifierData modifierData);
    }

    [RequireComponent(typeof(AbstractEnemy))]
    public class StageSpawnedEnemy : MonoBehaviour, IStageSpawnedEnemy
    {
        private AbstractEnemy _enemy;
        private IStatModule _statModule;
        private EventChannelSO _stageEventChannel;

        private int _runId = -1;
        private int _roomId = -1;
        private int _enemyRuntimeId = -1;

        private bool _isDeathRaised;

        private int _hpStatIndex = -1;
        private int _attackStatIndex = -1;
        private int _moveSpeedStatIndex = -1;

        private object _hpModifierKey;
        private object _attackModifierKey;
        private object _moveSpeedModifierKey;

        private void Awake()
        {
            _enemy = GetComponent<AbstractEnemy>();
            _statModule = GetComponent<IStatModule>();

            _enemy.onDeath.AddListener(HandleDeath);
        }

        private void OnDestroy()
        {
            if (_enemy != null)
                _enemy.onDeath.RemoveListener(HandleDeath);

            RemoveStageModifiers();
        }

        private void OnDisable()
        {
            RemoveStageModifiers();

            _stageEventChannel = null;
            _runId = -1;
            _roomId = -1;
            _enemyRuntimeId = -1;
            _isDeathRaised = false;
        }

        public void Bind(
            EventChannelSO stageEventChannel,
            int runId,
            int roomId,
            int enemyRuntimeId,
            StageEnemyModifierData modifierData)
        {
            RemoveStageModifiers();

            _stageEventChannel = stageEventChannel;
            _runId = runId;
            _roomId = roomId;
            _enemyRuntimeId = enemyRuntimeId;
            _isDeathRaised = false;

            ApplyStageModifiers(modifierData);
        }

        private void HandleDeath()
        {
            if (_isDeathRaised || _stageEventChannel == null)
                return;

            _isDeathRaised = true;

            _stageEventChannel.RaiseEvent(
                StageEvents.EnemyDead.Init(_runId, _roomId, _enemyRuntimeId));
        }

        private void ApplyStageModifiers(StageEnemyModifierData modifierData)
        {
            if (_statModule == null)
                return;

            _hpStatIndex = modifierData.HpStatIndex;
            _attackStatIndex = modifierData.AttackStatIndex;
            _moveSpeedStatIndex = modifierData.MoveSpeedStatIndex;

            if (_hpStatIndex >= 0 && !Mathf.Approximately(modifierData.HpBonus, 0f))
            {
                _hpModifierKey = new object();
                _statModule.AddModifier(_hpStatIndex, _hpModifierKey, modifierData.HpBonus);
            }

            if (_attackStatIndex >= 0 && !Mathf.Approximately(modifierData.AttackBonus, 0f))
            {
                _attackModifierKey = new object();
                _statModule.AddModifier(_attackStatIndex, _attackModifierKey, modifierData.AttackBonus);
            }

            if (_moveSpeedStatIndex >= 0 && !Mathf.Approximately(modifierData.MoveSpeedBonus, 0f))
            {
                _moveSpeedModifierKey = new object();
                _statModule.AddModifier(_moveSpeedStatIndex, _moveSpeedModifierKey, modifierData.MoveSpeedBonus);
            }
        }

        private void RemoveStageModifiers()
        {
            if (_statModule == null)
                return;

            if (_hpModifierKey != null && _hpStatIndex >= 0)
                _statModule.RemoveModifier(_hpStatIndex, _hpModifierKey);

            if (_attackModifierKey != null && _attackStatIndex >= 0)
                _statModule.RemoveModifier(_attackStatIndex, _attackModifierKey);

            if (_moveSpeedModifierKey != null && _moveSpeedStatIndex >= 0)
                _statModule.RemoveModifier(_moveSpeedStatIndex, _moveSpeedModifierKey);

            _hpModifierKey = null;
            _attackModifierKey = null;
            _moveSpeedModifierKey = null;

            _hpStatIndex = -1;
            _attackStatIndex = -1;
            _moveSpeedStatIndex = -1;
        }
    }
}