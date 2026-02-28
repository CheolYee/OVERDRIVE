using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "ChaseToTarget", story: "[Enemy] Chase To [TargetGameObject]", category: "Action/Navigation", id: "b1be80bdf395828b9daf3143fe5899bd")]
    public partial class ChaseToTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;
        
        private IMover _mover;
        private Transform _targetTrm;
        private Transform _selfTrm;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || TargetGameObject.Value == null || Enemy.Value.Mover == null)
                return Status.Failure;
            
            _mover = Enemy.Value.Mover;
            _targetTrm = TargetGameObject.Value.transform;
            _selfTrm = Enemy.Value.transform;
            
            if (GetDistanceToTarget(_targetTrm) < Enemy.Value.AttackConfig.StoppingDistance)
                return Status.Success;
            
            return Status.Running;
        }

        private float GetDistanceToTarget(Transform target)
        {
            if (_selfTrm == null) return float.MaxValue;
            return Vector2.Distance(target.position, _selfTrm.position);
        }

        protected override Status OnUpdate()
        {
            if (_mover.IsGrounded == false)
                return Status.Failure;
            
            Vector2 direction = _targetTrm.position - _selfTrm.position;
            _mover.SetMovementX(Mathf.Sign(direction.x));

            float boxCastLength = Enemy.Value.AttackConfig.StoppingDistance + 3f;
            float realDistance = Enemy.Value.Sensor.BoxCastObstacle(direction, boxCastLength, out RaycastHit2D hit);

            if (realDistance < Enemy.Value.AttackConfig.StoppingDistance)
                return Status.Failure;

            if (GetDistanceToTarget(_targetTrm) < Enemy.Value.AttackConfig.StoppingDistance)
            {
                return Status.Success;
            }
            
            return Status.Running;
        }
    }
}

