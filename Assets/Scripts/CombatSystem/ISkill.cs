using UnityEngine;

namespace CombatSystem
{
    public interface ISkill
    {
        SkillDataSO SkillData { get; }
        float NormalizedCooldown { get; }

        void InitializeSkill(ISkillModule skillModule);
        bool CanUseSkill(GameObject target = null);
        void UseSkill(GameObject target = null);
        void StopSkill();
    }
}