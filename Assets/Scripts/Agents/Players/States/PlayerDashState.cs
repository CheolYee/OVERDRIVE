using Agents.FSM;
using DG.Tweening;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.States
{
    public class PlayerDashState : AbstractPlayerState
    {
        private readonly float _dashDistance = 4.5f;
        private readonly float _dashDuration = 0.25f;
        
        public PlayerDashState(Agent owner, AnimParamSO stateParam) : base(owner, stateParam)
        {
        }

        public override void Enter()
        {
            base.Enter();
            float xInput = _player.PlayerInput.InputDirection.x;
            float moveX = Mathf.Abs(xInput) > 0.05f ? Mathf.Sign(xInput) : _renderer.FacingDirection;
            Vector2 dashDirection = new Vector2(moveX, 0);
            _renderer.FlipController(moveX);
            _mover.CanManualMovement = false;
            _mover.SetGravityScale(0);
            _mover.StopImmediately(true, true);
            
            //Vector3 destination = _player.transform.position + (Vector3)dashDirection * _dashDistance;
            
            float realDistance = _player.Sensor.BoxCastObstacle(dashDirection, _dashDistance, out RaycastHit2D hit);
            Vector3 destination = _player.transform.position + (Vector3)dashDirection * realDistance;
            float dashDuration = realDistance * _dashDuration / _dashDistance; //비례식을 이용하여 실제 이동거리만큼의 시간 구하기
            
            _player.transform.DOMove(destination, dashDuration).SetEase(Ease.OutQuad).OnComplete(
                ()=>_player.ChangeState(PlayerStateEnum.IDLE));
        }

        public override void Exit()
        {
            _player.transform.DOKill(); //나갈때 강제로 이 트랜스폼에 걸린 모든 트윈을 제거하는거야.
            
            _mover.StopImmediately(true, false);
            _mover.CanManualMovement = true;
            _mover.SetGravityScale(1f);
            base.Exit();
        }
    }
}