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
        //여기서 키보드 입력을 받아서 Mover에 전달하도록 코드를 작성하시고, Jump와 Fall전부 이것을 상속받아서 만들어지도록 합니다. 
        // 그리고 이 상태일때는 이동속도고 80% 로 되어야 합니다. SetMoveSpeedMultiplier

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