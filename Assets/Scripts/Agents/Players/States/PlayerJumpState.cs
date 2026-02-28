using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerJumpState : AbstractPlayerAirState
    {
        public PlayerJumpState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately(false,true);
            _mover.AddForceToAgent(new Vector2(0, _player.JumpForce));

            _mover.OnVelocityChange += HandleVelocityChange;
        }

        private void HandleVelocityChange(Vector2 velocity)
        {
            if(velocity.y < 0)
                _player.ChangeState(PlayerStateEnum.FALL);
        }

        public override void Exit()
        {
            _mover.OnVelocityChange -= HandleVelocityChange;
            base.Exit();
        }
        
        
    }
}