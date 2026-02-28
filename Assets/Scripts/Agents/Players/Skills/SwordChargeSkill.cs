using System.Collections;
using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using CombatSystem.EffectSystem;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class SwordChargeSkill : AbstractPlayerSkill
    {
        [SerializeField] private float chargeForce;
        [SerializeField] private float chargeDuration;
        [SerializeField] private float hitDuration;
        [SerializeField] private AnimationCurve movementCurve;

        [SerializeField] private AnimatorEffect effect;
        [SerializeField] private AnimParamSO effectParam;
        
        [SerializeField] private Vector3 impulseForce;
        
        private IMover _mover;
        private AbstractDamageCaster _damageCaster;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _mover = skillModule.Owner.GetModule<IMover>();
            
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");
            
            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} is not attached to damage caster");
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
            StartCoroutine(ChargeCoroutine());
        }

        private IEnumerator ChargeCoroutine()
        {
            float duration = 0f;
            float percent = 0f;
            float timer = 0f;
            
            _player.IsSuperArmor = true;
            float direction = _player.transform.right.x;

            if (effect != null)
            {
                effect.gameObject.SetActive(true);
                effect.PlayEffectClip(effectParam.ParamHash, 0);
            }
            
            while (percent <= 1f)
            {
                percent = duration / chargeDuration;
                float xVelocity = movementCurve.Evaluate(percent)* direction * chargeForce;
                _mover.Rigidbody2D.linearVelocityX = xVelocity;
                duration += Time.deltaTime;
                timer += Time.deltaTime;
                
                if (timer >= hitDuration)
                {
                    timer = 0f;
                    float damage = _skillModule.GetBaseDamage(SkillData);
                    Vector2 knockBackPower = SkillData.knockBackForce;
                    bool isHit = _damageCaster.CastDamage(damage, knockBackPower);
                    if (isHit) _skillModule.GenerateImpulse(impulseForce);
                }
                yield return null;
            }
            
            StopSkill();
        }

        public override void StopSkill()
        {
            base.StopSkill();
            if (effect != null)
            {
                effect.gameObject.SetActive(false);
            }
            
            StopAllCoroutines();
            _player.IsSuperArmor = false;
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }
    }
}