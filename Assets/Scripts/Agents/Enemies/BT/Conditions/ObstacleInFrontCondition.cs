using System;
using Unity.Behavior;
using UnityEngine;

namespace Agents.Enemies.BT.Conditions
{
    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "ObstacleInFront", story: "[Enemy] Check Obstacle In [Distance]", category: "Conditions", id: "dddeba40a44920d7c14820e1421c253b")]
    public partial class ObstacleInFrontCondition : Condition
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<float> Distance;

        public override bool IsTrue()
        {
            if (Enemy.Value == null || Enemy.Value.Sensor == null || Enemy.Value.AttackConfig == null)
                return false;

            Vector2 dir = Enemy.Value.transform.right;
            float realDistance = Enemy.Value.Sensor.BoxCastObstacle(dir, Distance.Value, out RaycastHit2D hit);
            return realDistance > Enemy.Value.AttackConfig.StoppingDistance;
        }
    }
}
