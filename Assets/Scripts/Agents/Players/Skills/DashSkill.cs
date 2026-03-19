using System.Collections.Generic;
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
        [Header("Dash Settings")]
        [SerializeField] private float dashDuration;

        private IMover _mover;
        private AbstractDamageCaster _damageCaster;
        private IRenderer _renderer;
        private float _dashDistance;

        private bool _isDashing;
        private readonly HashSet<int> _hitHashSet = new();

        protected override void OnInitialized()
        {
            _renderer = _player.GetModule<IRenderer>();
            _mover = _player.GetModule<IMover>();

            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");

            if (_damageCaster != null)
                _damageCaster.InitCaster(_player);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f &&
                   !IsAttacking &&
                   (current is ICanAttackState || current is PlayerAttackState || current is AbstractPlayerAirState);
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);

            _dashDistance = PlayerSkillData.maxRange;
            _mover.StopImmediately(true, true);
            StartDashSkill();
        }

        private void StartDashSkill()
        {
            _hitHashSet.Clear();
            _isDashing = true;

            float xInput = _player.PlayerInput.InputDirection.x;
            float yInput = _player.PlayerInput.InputDirection.y;

            float moveX = Mathf.Abs(xInput) > 0.05f ? Mathf.Sign(xInput) : _renderer.FacingDirection;
            float moveY = Mathf.Abs(yInput) > 0.05f ? Mathf.Sign(yInput) : 0f;

            if (moveY != 0f && xInput == 0f)
                moveX = 0f;

            Vector2 dashDirection = new(moveX, moveY);

            _renderer.FlipController(moveX);
            _mover.CanManualMovement = false;
            _mover.SetGravityScale(0f);
            _mover.StopImmediately(true, true);

            float realDistance = _player.Sensor.BoxCastObstacle(dashDirection, _dashDistance, out _);
            Vector3 destination = _player.transform.position + (Vector3)dashDirection * realDistance;
            float realDashDuration = realDistance * dashDuration / _dashDistance;

            _mover.Rigidbody2D.DOMove(destination, realDashDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Fixed)
                .OnComplete(StopSkill);
        }

        public override void StopSkill()
        {
            base.StopSkill();

            _player.transform.DOKill();

            _mover.StopImmediately(true, false);
            _mover.CanManualMovement = true;
            _mover.SetGravityScale(1f);

            _player.IsSuperArmor = false;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
            _isDashing = false;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_isDashing)
                return;

            if (!other.TryGetComponent(out IDamageable _))
                return;

            int id = other.GetInstanceID();
            if (!_hitHashSet.Add(id))
                return;

            DealDashDamage(other);
        }

        private void DealDashDamage(Collider2D col = null)
        {
            float damage = _skillModule.GetBaseDamage(PlayerSkillData);
            Vector2 knockBackPower = PlayerSkillData.knockBackForce;
            _damageCaster.CastDamage(damage, knockBackPower, col);
        }
    }
}