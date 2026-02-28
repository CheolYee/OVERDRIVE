using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerIdleState : AbstractPlayerState, ICanJumpState, ICanDashState, ICanAttackState, ICanCounterState
    {
        public PlayerIdleState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately(true, false);
            if (_mover.IsGrounded)
            {
                _player.ResetJumpCount();
            }
        }

        public override void Update()
        {
            base.Update();
            float xInput = _player.PlayerInput.InputDirection.x;

            if (Mathf.Abs(xInput) > 0.1f)
            {
                _player.ChangeState(PlayerStateEnum.MOVE);
                return;
            }
            
            if(!_mover.IsGrounded)
                _player.ChangeState(PlayerStateEnum.FALL);
        }
    }
}