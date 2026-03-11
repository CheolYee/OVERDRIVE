using Agents.FSM;
using Agents.Players.Skills;
using Agents.Players.States;
using Modules;
using UnityEngine;

namespace Agents.Players
{
    public class PlayerBasicAttackModule : MonoBehaviour, IModule, IAfterInitModule
    {
        [field: SerializeField] public PlayerSkillDataSo GroundBasicAttackSkill { get; private set; }
        [field: SerializeField] public PlayerSkillDataSo AirBasicAttackSkill { get; private set; }

        private Player _player;
        private PlayerSkillModule _playerSkillModule;

        public void Initialize(ModuleOwner owner)
        {
            _player = owner as Player;
            Debug.Assert(_player != null, $"{nameof(PlayerBasicAttackModule)} : owner is not Player.");
            _playerSkillModule = _player.GetModule<PlayerSkillModule>();
            Debug.Assert(_playerSkillModule != null, $"{nameof(PlayerBasicAttackModule)} : PlayerSkillModule is null.");
        }

        public void AfterInit()
        {
            RegisterBasicAttackSkills();
        }

        private void RegisterBasicAttackSkills()
        {
            if (_playerSkillModule == null)
                return;

            if (GroundBasicAttackSkill != null)
                _playerSkillModule.EnsureSkillRegistered(GroundBasicAttackSkill);

            if (AirBasicAttackSkill != null)
                _playerSkillModule.EnsureSkillRegistered(AirBasicAttackSkill);
        }

        public bool TryUseBasicAttack()
        {
            if (_player == null || _playerSkillModule == null)
                return false;

            if (_player.GetCurrentState() is ICanAttackState)
                return TryUseConfiguredSkill(GroundBasicAttackSkill);

            if (_player.GetCurrentState() is AbstractPlayerAirState)
                return TryUseConfiguredSkill(AirBasicAttackSkill);

            return false;
        }

        private bool TryUseConfiguredSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            return _playerSkillModule.TryUseRegisteredSkill(skillData, shouldMarkDashSequence: false);
        }
    }
}