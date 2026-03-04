using System;
using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using DG.Tweening;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class DashSkill : AbstractPlayerSkill
    {
        [SerializeField] private float dashDuration;
        [SerializeField] private float dashDistance;
        
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;
        private IRenderer _renderer;
        
        private bool _isDashing;
        
        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _renderer = skillModule.Owner.GetModule<IRenderer>();
            _mover = skillModule.Owner.GetModule<IMover>();
            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to _renderer");
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");
            
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");
            _damageCaster.InitCaster(_player);
        }
        
        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f && !IsAttacking
                                            && (current is ICanAttackState || current is PlayerAttackState || current is AbstractPlayerAirState);
        }
        
        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _mover.StopImmediately(true, true);
            StartDashSkill();
        }

        private void StartDashSkill()
        {
            float xInput = _player.PlayerInput.InputDirection.x;
            float yInput = _player.PlayerInput.InputDirection.y;
            
            float moveX = Mathf.Abs(xInput) > 0.05f ? Mathf.Sign(xInput) : _renderer.FacingDirection;
            float moveY = Mathf.Abs(yInput) > 0.05f ? Mathf.Sign(yInput) : 0;
            
            if (moveY != 0 && xInput == 0)
                moveX = 0; //수직 입력이 있으면 수평 대시 안함
            
            Vector2 dashDirection = new Vector2(moveX, moveY);
            _renderer.FlipController(moveX);
            _mover.CanManualMovement = false;
            _mover.SetGravityScale(0);
            _mover.StopImmediately(true, true);
            
            float realDistance = _player.Sensor.BoxCastObstacle(dashDirection, dashDistance, out RaycastHit2D hit);
            Vector3 destination = _player.transform.position + (Vector3)dashDirection * realDistance;
            float realDashDuration = realDistance * dashDuration / dashDistance; //비례식을 이용하여 실제 이동거리만큼의 시간 구하기

            DealDashDamage();
            
            _player.transform.DOMove(destination, realDashDuration).SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Fixed)
                .OnComplete(
                StopSkill);
            _isDashing = true;
        }

        public override void StopSkill()
        {
            base.StopSkill();
            _player.transform.DOKill(); //나갈때 강제로 이 트랜스폼에 걸린 모든 트윈을 제거하는거야.
            
            _mover.StopImmediately(true, false);
            _mover.CanManualMovement = true;
            _mover.SetGravityScale(1f);
            
            _player.IsSuperArmor = false;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
            _isDashing = false;
            Debug.Log("대쉬 끝");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isDashing) return;
            DealDashDamage();
        }
        
        private void DealDashDamage()
        {
            float damage = _skillModule.GetBaseDamage(SkillData);
            Vector2 knockBackPower = SkillData.knockBackForce;
            _damageCaster.CastDamage(damage, knockBackPower);
        }
    }
}