﻿using Agents.FSM;
using CombatSystem;
using Systems.AnimationSystems;
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
        
        private IAnimatorTrigger _trigger;
        private IRenderer _renderer;
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _trigger = skillModule.Owner.GetModule<IAnimatorTrigger>();
            _renderer = skillModule.Owner.GetModule<IRenderer>();
            _mover = skillModule.Owner.GetModule<IMover>();
            Debug.Assert(_trigger != null, $"{gameObject.name} is not attached to trigger");
            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");
            _damageCaster.InitCaster(_player);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            return _player.GetCurrentState() is ICanAttackState;
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            if (comboCounter > 2 || Time.time >= _lastUseTime + comboWindow)
            {
                comboCounter = 0; //콤보가 2를 넘어섰거나 마지막으로 공격한 시간으로부터 콤보 윈도우 시간만큼 지나갔다면
            }

            //무브먼트 적용 부분
            _mover.CanManualMovement = false;
            Vector2 movement = comboCounter < comboMovements.Length ? comboMovements[comboCounter] : Vector2.zero;
            _mover.StopImmediately(true, false);
            movement.x *= _renderer.FacingDirection;
            _mover.AddForceToAgent(movement);

            //캐스터 위치 조절 부분
            Vector2 offset = comboCounter < casterOffsets.Length ? casterOffsets[comboCounter] : Vector2.zero;
            _damageCaster.transform.localPosition = offset; //로컬포지션
            
            _renderer.SetFloat(attackIndexParam, comboCounter); //인덱스 지정

            _trigger.OnAttackTrigger += HandleAttackTrigger;
            _trigger.OnAnimationEnd += AnimationEndTrigger;
        }

        private void HandleAttackTrigger()
        {
            float damage = _skillModule.GetBaseDamage(SkillData);
            Vector2 knockBackPower = comboCounter < overrideKnockbackForce.Length ? overrideKnockbackForce[comboCounter] : Vector2.zero;
            
            bool isSuccess = _damageCaster.CastDamage(damage, knockBackPower); //타이밍에 맞게 나가야하는데 아직 그거 안맞췄어.
            if (isSuccess)
            {
                _mover.StopImmediately(true, false);
            }
        }

        private void AnimationEndTrigger() => ClearSkillParams();

        protected override void ClearSkillParams()
        {
            base.ClearSkillParams();
            _trigger.OnAnimationEnd -= AnimationEndTrigger;
            _trigger.OnAttackTrigger -= HandleAttackTrigger;
            ++comboCounter;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
            _mover.CanManualMovement = true;
        }
    }
}