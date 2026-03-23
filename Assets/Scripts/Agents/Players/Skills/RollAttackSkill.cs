using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using DG.Tweening;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class RollAttack : AbstractPlayerSkill
    {
        [Header("Roll Settings")]
        [SerializeField] private float rollDuration;

        private IMover _mover;
        private AbstractDamageCaster _damageCaster;
        private IRenderer _renderer;
        private IAnimatorTrigger _animTrigger;
        private float _rollDistance;

        protected override void OnInitialized()
        {
            _renderer = _player.GetModule<IRenderer>();
            _mover = _player.GetModule<IMover>();
            _animTrigger = _player.GetModule<IAnimatorTrigger>();

            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");

            if (_damageCaster != null)
                _damageCaster.InitCaster(_player);

            _animTrigger.OnAttackTrigger += StartAttack;
            _animTrigger.OnAnimationEnd += StopSkill;
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f &&
                   !IsAttacking &&
                   (current is ICanAttackState || current is PlayerAttackState);
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _rollDistance = PlayerSkillData.maxRange;
            _mover.StopImmediately(true, true);
            StartRollSkill();
        }

        private void StartRollSkill()
        {
            _player.IsSuperArmor = true;
            
            float xInput = _player.PlayerInput.InputDirection.x;

            float moveX = Mathf.Abs(xInput) > 0.05f ? Mathf.Sign(xInput) : _renderer.FacingDirection;

            Vector2 dashDirection = new(moveX, 0);

            _renderer.FlipController(moveX);
            _mover.CanManualMovement = false;
            _mover.StopImmediately(true, true);

            float realDistance = _player.Sensor.BoxCastObstacle(dashDirection, _rollDistance, out _);
            Vector3 destination = _player.transform.position + (Vector3)dashDirection * realDistance;
            float realDashDuration = realDistance * rollDuration / _rollDistance;

            _mover.Rigidbody2D.DOMove(destination, realDashDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(UpdateType.Fixed);
        }

        public override void StopSkill()
        {
            base.StopSkill();
            _animTrigger.OnAnimationEnd -= StopSkill;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }
        
        private void StartAttack()
        {
            _animTrigger.OnAttackTrigger -= StartAttack;
            _player.transform.DOKill();
            _player.IsSuperArmor = false;
            _mover.CanManualMovement = true;
            _mover.StopImmediately(true, false);
            
            float damage = _skillModule.GetBaseDamage(PlayerSkillData);
            Vector2 knockBackPower = PlayerSkillData.knockBackForce;
            _damageCaster.CastDamage(damage, knockBackPower);
        }
    }
}
