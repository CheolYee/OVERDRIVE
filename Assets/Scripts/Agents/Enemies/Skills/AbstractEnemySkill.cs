using CombatSystem;
using UnityEngine;

namespace Agents.Enemies.Skills
{
    public abstract class AbstractEnemySkill : MonoBehaviour, ISkill
    {
        [field: SerializeField] public SkillDataSO SkillData { get; private set; }

        protected float LastUseTime;
        protected ISkillModule SkillModule;
        protected AbstractEnemy Enemy;

        public float NormalizedCooldown => Mathf.Clamp01((Time.time - LastUseTime) / SkillData.cooldown);
        
        public virtual void InitializeSkill(ISkillModule skillModule)
        {
            SkillModule = skillModule;
            Enemy = SkillModule.Owner as AbstractEnemy;
            Debug.Assert(Enemy != null, $"{gameObject.name} 은 에너미가 아닙니다.");
        }

        public abstract bool CanUseSkill(GameObject target = null);
        public abstract void UseSkill(GameObject target = null);
        public abstract void StopSkill();
    }
}