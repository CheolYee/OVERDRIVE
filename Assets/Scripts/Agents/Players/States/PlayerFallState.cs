using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerFallState : AbstractPlayerAirState
    {
        public PlayerFallState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        // public override void Enter()
        // {
        //     base.Enter();
        //     _mover.OnGroundStatusChange += HandleGroundStatusChange;
        // }

        public override void Update()
        {
            base.Update();
            if (_mover.IsGrounded)
            {
                LandingGround();
            }
        }

        // public override void Exit()
        // {
        //     _mover.OnGroundStatusChange -= HandleGroundStatusChange;
        //     base.Exit();
        // }

        // private void HandleGroundStatusChange(bool isGrounded)
        // {
        //     if (isGrounded)
        //         LandingGround();
        // }

        private void LandingGround()
        {
            _player.ResetJumpCount();
            _player.ChangeState(PlayerStateEnum.IDLE);
        }
    }
}