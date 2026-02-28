using Agents.FSM;
using CombatSystem;
using Systems.AnimationSystems;

namespace Agents.Players.States
{
    public class PlayerAttackState : AbstractPlayerState
    {
        private readonly PlayerSkillModule _skillModule;
        
        public PlayerAttackState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
            _skillModule = owner.GetModule<PlayerSkillModule>();
        }

        public override void Enter()
        {
            _isTriggerCall = false;
            _mover.CanManualMovement = false;
            _skillModule.OnAttackEnd += AnimationEndTrigger;
        }

        public override void Update()
        {
            base.Update();
            if (_isTriggerCall)
            {
                PlayerStateEnum nextState = _skillModule.CurrentUsingSkill.PlayerSkillData.nextState;
                _player.ChangeState(nextState);
            }
        }

        public override void Exit()
        {
            _mover.CanManualMovement = true;
            _skillModule.OnAttackEnd -= AnimationEndTrigger;
            base.Exit();
        }
    }
}