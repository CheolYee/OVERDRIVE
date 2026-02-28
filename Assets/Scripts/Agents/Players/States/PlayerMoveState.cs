using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerMoveState : AbstractPlayerState, ICanJumpState, ICanDashState, ICanAttackState, ICanSlideState, ICanCounterState
    {
        public PlayerMoveState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        public override void Update()
        {
            base.Update();
            float xInput = _player.PlayerInput.InputDirection.x;
            
            _mover.SetMovementX(xInput);

            if (Mathf.Approximately(xInput, 0))
            {
                _player.ChangeState(PlayerStateEnum.IDLE);
                return;
            }
            
            if(!_mover.IsGrounded)
                _player.ChangeState(PlayerStateEnum.FALL);
        }
    }
}