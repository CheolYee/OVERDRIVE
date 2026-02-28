using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerSlideState : AbstractPlayerState
    {
        private const float SlideForce = 5f;
        
        private IAnimatorTrigger _trigger;
        public PlayerSlideState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
            _trigger = owner.GetModule<IAnimatorTrigger>();
        }

        public override void Enter()
        {
            base.Enter();
            _mover.CanManualMovement = false;
            _mover.AddForceToAgent(new Vector2(SlideForce * _renderer.FacingDirection, 0));
            
            _trigger.OnAnimationEnd += AnimationEndTrigger;
        }

        public override void Update()
        {
            base.Update();
            if (_isTriggerCall)
            {
                _player.ChangeState(PlayerStateEnum.IDLE);
                return;
            }
            
            if(!_mover.IsGrounded)
                _player.ChangeState(PlayerStateEnum.FALL);
        }

        public override void Exit()
        {
            _trigger.OnAnimationEnd -= AnimationEndTrigger;
            _mover.CanManualMovement = true;
            base.Exit();
        }
    }
}