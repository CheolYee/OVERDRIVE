using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public abstract class AbstractPlayerAirState : AbstractPlayerState, ICanJumpState, ICanDashState
    {
        public AbstractPlayerAirState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _mover.OnVelocityChange += HandleVelocityChange;
            _mover.SetMoveSpeedMultiplier(0.8f);
        }

        public override void Update()
        {
            base.Update();
            float xInput = _player.PlayerInput.InputDirection.x;
            _mover.SetMovementX(xInput);
        }

        public override void Exit()
        {
            _mover.OnVelocityChange -= HandleVelocityChange;
            _mover.SetMoveSpeedMultiplier(1f);
            base.Exit();
        }

        private void HandleVelocityChange(Vector2 velocity)
        {
            _renderer.SetFloat(_player.YVelocityParam, velocity.y);
        }
    }
}