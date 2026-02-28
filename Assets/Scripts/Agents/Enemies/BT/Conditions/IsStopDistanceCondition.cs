using System;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsStopDistance", story: "[Enemy] In Stop Distance from [Target] [isIn]", category: "Conditions", id: "7802ea9693e0c5c5a469b7bb49c57bd4")]
    public partial class IsStopDistanceCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<bool> IsIn;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Enemy.Value.AttackConfig == null || Target.Value == null)
                return false;
            
            float distance = Vector2.Distance(Enemy.Value.transform.position, Target.Value.transform.position);
            AttackConfigSO attackConfig = Enemy.Value.AttackConfig;
            
            return IsIn.Value ? distance <= attackConfig.StoppingDistance : distance > attackConfig.StoppingDistance;
        }
    }
}
