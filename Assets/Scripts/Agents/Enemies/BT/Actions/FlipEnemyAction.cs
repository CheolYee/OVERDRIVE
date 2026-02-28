using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "FlipEnemy", story: "Flip [Enemy] to [Target]", category: "Action/Transform", id: "97460c3e2a27a82f8daf82df58d80246")]
    public partial class FlipEnemyAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        protected override Status OnStart()
        {
            if(Enemy.Value == null || Enemy.Value.Renderer == null)
                return Status.Failure;

            if (Target.Value == null) //타겟이 없으니까 무조건 회전
            {
                Enemy.Value.Renderer.FlipController(Enemy.Value.Renderer.FacingDirection * -1f);
            }
            else //타겟이 있다면 타겟방향으로 회전하도록 
            {
                Vector2 direction = Target.Value.transform.position - Enemy.Value.transform.position;
                Enemy.Value.Renderer.FlipController(Mathf.Sign(direction.x)); //디렉션 방향을 -1또는 1로만 리턴
            }
            
            return Status.Success;
        }
    }
}

