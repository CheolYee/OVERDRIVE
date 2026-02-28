using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "CheckTarget", story: "[Enemy] check [TargetGameObject] in range", category: "Action/Util", id: "faddce1b1ac53450872e62cbe4660ba8")]
    public partial class CheckTargetAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.AttackConfig == null || Enemy.Value.Sensor == null)
                return Status.Failure;

            AgentSensor sensor = Enemy.Value.Sensor;
            AttackConfigSO config = Enemy.Value.AttackConfig;
            Vector3 startPosition = Enemy.Value.transform.position + new Vector3(0, 0.5f); //바닥에서쏘면 그라운드랑 충돌해버리는 문제

            if (sensor.IsTargetInRange(config.DetectRange, out Collider2D hitCollider)
                &&sensor.IsTargetInSight(startPosition, config.DetectRange, hitCollider))
            {
                TargetGameObject.Value = hitCollider.gameObject;
                return Status.Success;
            }
            
            return Status.Failure;
        }
    }
}

