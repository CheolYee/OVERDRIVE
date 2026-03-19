using Agents.FSM;
using CombatSystem;
using Gamelib.SoundSystem;
using Systems.AnimationSystems;
using Systems.Managers;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class SwordComboSkill : AbstractPlayerSkill
    {
        [SerializeField] private AnimParamSO attackIndexParam;
        [SerializeField] private float comboWindow = 0.8f;
        [SerializeField] private int comboCounter;

        [SerializeField] private Vector2[] comboMovements;
        [SerializeField] private Vector2[] casterOffsets;
        [SerializeField] private Vector2[] overrideKnockbackForce;
        [SerializeField] private Vector2[] casterSizes;
        [SerializeField] private SfxSounds[] comboSfx;

        private IAnimatorTrigger _trigger;
        private IRenderer _renderer;
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;

        protected override void OnInitialized()
        {
            _trigger = _player.GetModule<IAnimatorTrigger>();
            _renderer = _player.GetModule<IRenderer>();
            _mover = _player.GetModule<IMover>();

            Debug.Assert(_trigger != null, $"{gameObject.name} is not attached to trigger");
            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");

            if (_damageCaster != null)
                _damageCaster.InitCaster(_player);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            return _player.GetCurrentState() is ICanAttackState;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);

            if (_mover == null || _renderer == null || _trigger == null || _damageCaster == null)
            {
                Debug.LogError($"{nameof(SwordComboSkill)} 초기화가 완료되지 않았습니다.");
                StopSkill();
                return;
            }

            if (comboCounter > 2 || Time.time >= _lastUseTime + comboWindow)
                comboCounter = 0;

            _mover.CanManualMovement = false;

            Vector2 movement = comboCounter < comboMovements.Length ? comboMovements[comboCounter] : Vector2.zero;
            _mover.StopImmediately(true, false);
            movement.x *= _renderer.FacingDirection;
            _mover.AddForceToAgent(movement);

            Vector2 offset = comboCounter < casterOffsets.Length ? casterOffsets[comboCounter] : Vector2.zero;
            _damageCaster.transform.localPosition = offset;

            Vector2 sizeOffset = comboCounter < casterSizes.Length ? casterSizes[comboCounter] : Vector2.zero;
            _damageCaster.SetBoxSize(sizeOffset);

            _renderer.SetFloat(attackIndexParam, comboCounter);

            _trigger.OnAttackTrigger -= HandleAttackTrigger;
            _trigger.OnAnimationEnd -= AnimationEndTrigger;
            _trigger.OnAttackTrigger += HandleAttackTrigger;
            _trigger.OnAnimationEnd += AnimationEndTrigger;
        }

        private void HandleAttackTrigger()
        {
            if (_damageCaster == null || _mover == null)
                return;

            if (comboCounter < comboSfx.Length)
                SoundPlayManager.Instance.PlaySfx(comboSfx[comboCounter], transform.position);

            float damage = _skillModule.GetBaseDamage(SkillData);
            Vector2 knockBackPower = comboCounter < overrideKnockbackForce.Length
                ? overrideKnockbackForce[comboCounter]
                : Vector2.zero;

            bool isSuccess = _damageCaster.CastDamage(damage, knockBackPower);
            if (isSuccess)
                _mover.StopImmediately(true, false);
        }

        private void AnimationEndTrigger()
        {
            ClearSkillParams();
        }

        protected override void ClearSkillParams()
        {
            base.ClearSkillParams();

            if (_trigger != null)
            {
                _trigger.OnAnimationEnd -= AnimationEndTrigger;
                _trigger.OnAttackTrigger -= HandleAttackTrigger;
            }

            ++comboCounter;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();

            if (_mover != null)
                _mover.CanManualMovement = true;
        }
    }
}