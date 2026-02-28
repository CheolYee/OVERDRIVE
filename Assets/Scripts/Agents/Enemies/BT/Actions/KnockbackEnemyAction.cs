using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "KnockbackEnemy", story: "[Enemy] Knockback In [Duration]", category: "Action/Combat", id: "32487e2cf741b0fafe2c0846f12587b7")]
    public partial class KnockbackEnemyAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<float> Duration;
        
        private IMover _mover;
        private ActionDataModule _actionData;
        private float _enterTime;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.Mover == null || Enemy.Value.ActionData == null)
                return Status.Failure;

            _mover = Enemy.Value.Mover;
            _actionData = Enemy.Value.ActionData;

            _mover.CanManualMovement = false;
            _enterTime = Time.time;
            Vector2 kbForce = _actionData.LastKnockBackDirection;
            _mover.AddForceToAgent(kbForce);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            if (Time.time - _enterTime >= Duration.Value)
                return Status.Success;
            
            return Status.Running;
        }

        protected override void OnEnd()
        {
            if (_mover != null)
                _mover.CanManualMovement = true;
        }
    }
}

