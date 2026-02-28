using System.Collections;
using Agents.FSM;
using Agents.Players.States;
using CombatSystem;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class GroundSlamSkill : AbstractPlayerSkill
    {
        [SerializeField] private AnimParamSO slamAttackParam;
        [SerializeField] private AnimParamSO yVelocityParam;
        [SerializeField] private Vector2 jumpForce;
        [SerializeField] private float hitDuration = 0.1f;
        [SerializeField] private float afterDelayDuration = 0.4f;
        [SerializeField] private float changeAnimationThreshold = 1.5f;
        [SerializeField] private Vector3 impulseForce;
        
        private IMover _mover;
        private IRenderer _renderer;
        
        [SerializeField] private AbstractDamageCaster airDamageCaster;
        [SerializeField] private AbstractDamageCaster slamDamageCaster;
        [SerializeField] private Vector2 slamKnockbackForce;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            _mover = skillModule.Owner.GetModule<IMover>();
            _renderer = skillModule.Owner.GetModule<IRenderer>();
            Debug.Assert(_mover != null, $"{gameObject.name} is not attached to mover");
            Debug.Assert(_renderer != null, $"{gameObject.name} is not attached to renderer");
            
            Debug.Assert(airDamageCaster != null, $"{gameObject.name} is not attached to damage caster");
            Debug.Assert(slamDamageCaster != null, $"{gameObject.name} is not attached to damage caster");
            
            airDamageCaster.InitCaster(_player);
            slamDamageCaster.InitCaster(_player);
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
            _mover.StopImmediately(true, true);
            jumpForce.x = Mathf.Sign(_player.transform.right.x) * Mathf.Abs(jumpForce.x);
            _mover.AddForceToAgent(jumpForce);
            StartCoroutine(SlamAttackCoroutine());
        }

        private IEnumerator SlamAttackCoroutine()
        {
            while (_mover.Rigidbody2D.linearVelocityY > changeAnimationThreshold)
            {
                _renderer.SetFloat(yVelocityParam, _mover.Rigidbody2D.linearVelocityY);
                yield return null;
            }
            
            _renderer.PlayClip(slamAttackParam.ParamHash);
            float hitTimer = 0f;
            while (!_mover.IsGrounded)
            {
                hitTimer += Time.deltaTime;
                if (hitTimer >= hitDuration)
                {
                    hitTimer = 0f;
                    float damage = _skillModule.GetBaseDamage(SkillData);
                    airDamageCaster.CastDamage(damage, SkillData.knockBackForce);
                }

                yield return null;
            }
            
            _mover.StopImmediately(true, false);
            slamDamageCaster.CastDamage(_skillModule.GetBaseDamage(SkillData), slamKnockbackForce);
            _skillModule.GenerateImpulse(impulseForce);
            yield return new WaitForSeconds(afterDelayDuration);
            StopSkill();
        }

        public override void StopSkill()
        {
            base.StopSkill();
            StopAllCoroutines();
            _lastUseTime = Time.time;
            _skillModule.InvokeAttackEnd();
        }
    }
}