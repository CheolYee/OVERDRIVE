using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using CombatSystem.EffectSystem;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class UpperSlashSkill : AbstractPlayerSkill
    {
        [SerializeField] private AnimatorEffect effect;
        [SerializeField] private AnimParamSO effectParam;
        
        private IAnimatorTrigger _trigger;
        private IRenderer _renderer;
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _renderer = _player.GetModule<IRenderer>();
            _trigger = _player.GetModule<IAnimatorTrigger>();
            _mover = _player.GetModule<IMover>();
            
            Debug.Assert(_trigger != null, $"{gameObject.name} 소유자가 애니메이션 트리거를 가지고 있지 않습니다.");
            Debug.Assert(_renderer != null, $"{gameObject.name} 소유자가 렌더러를 가지고 있지 않습니다.");
            Debug.Assert(_mover != null, $"{gameObject.name} 소유자가 무버를 가지고 있지 않습니다.");
            
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} 에 데미지 캐스터가 없습니다.");
            _damageCaster.InitCaster(_player);
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            AgentState current = _player.GetCurrentState();
            return NormalizedCooldown >= 1f && !IsAttacking
                                            && (current is ICanAttackState || current is PlayerAttackState);
        }

        public override void UseSkill(GameObject target = null)
        {
            base.UseSkill(target);
            _mover.StopImmediately(true, false);
            _trigger.OnAnimationEnd += AnimationEndTrigger;
            _trigger.OnAttackTrigger += HandleCastDamage;

            if (effect != null)
            {
                effect.gameObject.SetActive(true);
                effect.PlayEffectClip(effectParam.ParamHash, 0);
            }
        }

        public override void StopSkill()
        {
            base.StopSkill();
            if (effect != null)
            {
                effect.gameObject.SetActive(false);
            }
            
            _trigger.OnAnimationEnd -= AnimationEndTrigger;
            _trigger.OnAttackTrigger -= HandleCastDamage;
            _lastUseTime = Time.time;
        }
        private void AnimationEndTrigger()
        {
            StopSkill();
            _skillModule.InvokeAttackEnd();
        }

        private void HandleCastDamage()
        {
            float damage = _skillModule.GetBaseDamage(SkillData);
            Vector2 knockBackPower = SkillData.knockBackForce;
            _damageCaster.CastDamage(damage, knockBackPower);
        }

    }
}