using CombatSystem;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    public class CommonMeleeSkill : AbstractEnemySkill, ICounterable
    {
        protected IAnimatorTrigger Trigger;
        
        private AbstractDamageCaster _damageCaster;

        public override void InitializeSkill(ISkillModule skillModule)
        {
            base.InitializeSkill(skillModule);
            Trigger = Enemy.GetModule<IAnimatorTrigger>();
            Debug.Assert(Trigger != null, $"{gameObject.name} 소유자가 애니메이션 트리거를 가지고 있지 않습니다.");

            _damageCaster = GetComponentInChildren<AbstractDamageCaster>();
            Debug.Assert(_damageCaster != null, $"{gameObject.name} 에 데미지 캐스터가 없습니다.");
            _damageCaster.InitCaster(Enemy);
            
            Collider = Enemy.GetComponent<Collider2D>();
            Debug.Assert(Collider != null, $"{gameObject.name} 에 콜라이더가 없습니다.");
        }

        public override bool CanUseSkill(GameObject target = null)
        {
            if (target == null) return false;
            
            float distanceToAgent = Vector2.Distance(target.transform.position, Enemy.transform.position);
            return distanceToAgent < SkillData.minRange && NormalizedCooldown >= 1f;
        }

        public override void UseSkill(GameObject target = null)
        {
            CanCounter = false;
            Trigger.OnAnimationEnd += HandleAttackEnd;
            Trigger.OnAttackTrigger += HandleAttackTrigger;
            Trigger.OnCounterStateChange += HandleCounterStateChange;
        }

        private void HandleAttackTrigger()
        {
            float damage = SkillModule.GetBaseDamage(SkillData);
            Vector2 knockBackPower = SkillData.knockBackForce;
            _damageCaster.CastDamage(damage, knockBackPower);
            Debug.Log(damage);
        }

        private void HandleAttackEnd()
        {
            Trigger.OnAnimationEnd -= HandleAttackEnd;
            Trigger.OnAttackTrigger -= HandleAttackTrigger;
            Trigger.OnCounterStateChange -= HandleCounterStateChange;
            LastUseTime = Time.time;
            CanCounter = false;
            SkillModule.InvokeAttackEnd();
        }

        public override void StopSkill()
        {
            HandleAttackEnd();
        }

        #region 카운터 처리

        public Collider2D Collider { get; private set; }
        [field: SerializeField] public bool CanCounter { get; private set; }
        
        public void ApplyCounter(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
            StopSkill();
            Enemy.ApplyDamage(damageData, hitPoint, hitDirection, hitNormal);
        }
        
        private void HandleCounterStateChange(bool counter) => CanCounter = counter;

        #endregion

    }
}