using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "SetDead", story: "Set [Enemy] To Dead", category: "Action/Combat", id: "7a09607cd6852be0d4d7804a1b4c5cb2")]
    public partial class SetDeadAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;

        protected override Status OnStart()
        {
            if (Enemy.Value == null)
                return Status.Failure;
            
            Enemy.Value.SetDead();
            return Status.Success;
        }
    }
}

