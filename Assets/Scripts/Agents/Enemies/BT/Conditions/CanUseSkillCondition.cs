using System;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "CanUseSkill", story: "[Enemy] Can Use [SkillNumber] To [Target]", category: "Conditions", id: "0b91934343019c31eeef696a475686ad")]
    public partial class CanUseSkillCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<int> SkillNumber;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Enemy.Value.SkillModule == null)
                return false;
            
            
            return Enemy.Value.SkillModule.CanUseSkill(SkillNumber.Value, Target.Value);
        }
    }
}
