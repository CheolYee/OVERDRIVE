using Agents.FSM;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerWallSlideState : AbstractPlayerState
    {
        public PlayerWallSlideState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _mover.StopImmediately(true, true);
            _mover.CanManualMovement = false;
            _mover.AddForceToAgent(new Vector2(_renderer.FacingDirection * 10f, 0));
            _mover.SetGravityScale(0.25f);
        }

        public override void Update()
        {
            base.Update();
            //여기에 일정이상의 속도제어로직도 만들어야 한다.
            
            float xInput = _player.PlayerInput.InputDirection.x;
            //슬라이드 중에 벽이 아닌 다른방향으로 키보드를 누르면 슬라이드를 끝내고 탈출
            if (Mathf.Abs(xInput + _renderer.FacingDirection) < 0.5f)
            {
                _player.ChangeState(PlayerStateEnum.FALL);
                return;
            }

            if (_mover.IsGrounded)
            {
                _player.ChangeState(PlayerStateEnum.IDLE);
                _player.ResetJumpCount();
                return;
            }
            
            Vector2 direction = new Vector2(_renderer.FacingDirection * 0.7f, 0);

            if (!_player.Sensor.IsObstaclePresent(direction, out Collider2D hitCollider))
            {
                _player.ChangeState(PlayerStateEnum.FALL);
                return;
            }
        }

        public override void Exit()
        {
            _mover.CanManualMovement = true; 
            _mover.SetGravityScale(1f);
            base.Exit();
        }
    }
}