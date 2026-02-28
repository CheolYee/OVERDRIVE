using System;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "IsOnGround", story: "[Enemy] is OnGround", category: "Conditions", id: "4e6e49d83628bc184b6c35427a9db9b9")]
    public partial class IsOnGroundCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Enemy.Value.Mover == null) return false;
            
            return Enemy.Value.Mover.IsGrounded;
        }

    }
}
