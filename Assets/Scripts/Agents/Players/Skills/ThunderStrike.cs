using CombatSystem;
using CombatSystem.EffectSystem;
using Modules;
using Systems.AnimationSystems;
using Unity.Cinemachine;
using UnityEngine;

namespace Agents.Players.Skills
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class ThunderStrike : MonoBehaviour
    {
        [SerializeField] private AnimatorEffect effectVisual;
        [SerializeField] private CombatEffectTrigger effectTrigger;
        [SerializeField] private AnimParamSO effectParam;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private float attackRadius;
        [SerializeField] private float explosionRadius;
        [SerializeField] private float explosionMultiplier = 2f;
        
        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
        [SerializeField] private Vector3 impulseForce;
        
        private PlayerSkillDataSo _skillData;
        private float _damage;
        
        public void CastEffect(PlayerSkillDataSo skillData, Vector3 position, float damage, ModuleOwner dealer)
        {
            damageCaster.InitCaster(dealer);
            transform.position = position;
            _damage = damage;
            effectVisual.PlayEffectClip(effectParam.ParamHash, 0);
            _skillData = skillData;
            
            effectTrigger.OnAnimationEnd += HandleEffectEnd;
            effectTrigger.OnAttackTrigger += HandleAttack;
            effectTrigger.OnExplosionTrigger += HandleExplosion;
        }

        private void HandleEffectEnd()
        {
            effectTrigger.OnAnimationEnd -= HandleEffectEnd;
            effectTrigger.OnAttackTrigger -= HandleAttack;
            effectTrigger.OnExplosionTrigger -= HandleExplosion;
            Destroy(gameObject, 0.1f);
        }

        private void HandleAttack()
        {
            damageCaster.SetRadius(attackRadius);
            damageCaster.CastDamage(_damage, _skillData.knockBackForce);
        }

        private void HandleExplosion()
        {
            damageCaster.SetRadius(explosionRadius);
            damageCaster.CastDamage(_damage * explosionMultiplier, _skillData.knockBackForce * explosionMultiplier);
            cinemachineImpulseSource.GenerateImpulse(impulseForce);
        }
    }
}