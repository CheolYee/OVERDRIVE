using System;
using Systems.AnimationSystems;
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

        IAnimatorTrigger _animatorTrigger;
        protected override Status OnStart()
        {
            if (Enemy.Value == null)
                return Status.Failure;
            
            _animatorTrigger = Enemy.Value.GetModule<IAnimatorTrigger>();


            _animatorTrigger.OnAnimationEnd += OnAnimEnd;
            return Status.Success;
        }

        private void OnAnimEnd()
        {
            if (Enemy.Value == null || _animatorTrigger == null)
                return;
                
            _animatorTrigger.OnAnimationEnd -= OnAnimEnd;
            Enemy.Value.SetDead();
        }
    }
}

