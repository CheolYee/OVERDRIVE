using CombatSystem;
using Systems.AnimationSystems;

namespace Agents.Players.States
{
    public class PlayerDeadState : AbstractPlayerState
    {
        private IAnimatorTrigger _trigger;
        private ISkillModule _skillModule;
        
        public PlayerDeadState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
            _trigger = owner.GetModule<IAnimatorTrigger>();
            _skillModule = owner.GetModule<ISkillModule>();
        }

        public override void Enter()
        {
            base.Enter();
            _isTriggerCall = false;
            _mover.CanManualMovement = false;
            _player.IsSuperArmor = true;
            _skillModule.OnAttackEnd += AnimationEndTrigger;
        }

        public override void Update()
        {
            base.Update();
            if (_isTriggerCall)
            {
                
            }
        }

        public override void Exit()
        {
            base.Exit();
            _skillModule.OnAttackEnd -= AnimationEndTrigger;
        }
    }
}