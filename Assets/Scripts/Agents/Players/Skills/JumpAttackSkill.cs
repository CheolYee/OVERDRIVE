using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class JumpAttackSkill : AbstractPlayerSkill
    {
        [SerializeField] private Vector2 movement;

        private IMover _mover;
        private IAnimatorTrigger _trigger;
        private IRenderer _renderer;
        private AbstractDamageCaster _damageCaster;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _trigger = _player.GetModule<IAnimatorTrigger>();
            _mover = _player.GetModule<IMover>();
            _renderer = _player.GetModule<IRenderer>();
            
            Debug.Assert(_trigger != null, $"{gameObject.name} is not attached to trigger");
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");
            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");
            _damageCaster.InitCaster(_player);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            return NormalizedCooldown >= 1 && _player.GetCurrentState() is AbstractPlayerAirState;
        }

        public override void UseSkill(GameObject target = null)
        {
            _mover.CanManualMovement = false;
            _mover.StopImmediately(false, true);
            Vector2 realMovement = new Vector2(movement.x * _renderer.FacingDirection, movement.y);
            _mover.AddForceToAgent(realMovement);
            _trigger.OnAttackTrigger += HandleAttackTrigger;
            _trigger.OnAnimationEnd += AnimationEndTrigger;
        }

        private void HandleAttackTrigger()
        {
            float damage = _skillModule.GetBaseDamage(SkillData);
            Vector2 knockBackPower = SkillData.knockBackForce;
            bool isSuccess = _damageCaster.CastDamage(damage, knockBackPower);
            if (isSuccess)
                _mover.StopImmediately(true, false);
        }

        private void AnimationEndTrigger()
        {
            _mover.CanManualMovement = true;
            _trigger.OnAnimationEnd -= AnimationEndTrigger;
            _trigger.OnAttackTrigger -= HandleAttackTrigger;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }

        public override void StopSkill()
        {
            AnimationEndTrigger();
        }
    }
}