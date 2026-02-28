using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "StopEnemy", story: "[Enemy] stop move x: [XValue] y: [YValue]", category: "Action/Navigation", id: "cd40f85d7d43ba4538ecb82e9f4ad76b")]
    public partial class StopEnemyAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<bool> XValue;
        [SerializeReference] public BlackboardVariable<bool> YValue;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.Mover == null)
                return Status.Failure;
            
            Enemy.Value.Mover.StopImmediately(XValue.Value, YValue.Value);
            return Status.Success;
        }
    }
}

