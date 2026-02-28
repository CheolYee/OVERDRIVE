using System;
using Modules;
using UnityEngine;

namespace CombatSystem
{
    public interface ISkillModule
    {
        ModuleOwner Owner { get; }

        event Action OnAttackEnd;
        bool CanUseSkill(int skillIndex, GameObject target = null);
        void UseSkill(int skillIndex, GameObject target = null);
        void InvokeAttackEnd();
        float GetBaseDamage(SkillDataSO skillData); // 스킬 기본 데미지 반환
    }
}