using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "EnemyUseSkill", story: "[Enemy] Use [SkillNumber] To [Target]", category: "Action/Combat", id: "36217f3bd1495bfb091f027b9506f67c")]
    public partial class EnemyUseSkillAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<int> SkillNumber;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        private bool _isSkillEnd;
        protected override Status OnStart()
        {
            if (Enemy.Value == null || Enemy.Value.SkillModule == null)
                return Status.Failure;

            Enemy.Value.SkillModule.OnAttackEnd += HandleSkillEnd;
            _isSkillEnd = false;
            Enemy.Value.SkillModule.UseSkill(SkillNumber.Value, Target.Value);
            
            return Status.Running;
        }

        private void HandleSkillEnd()
        {
            _isSkillEnd = true;
        }

        protected override Status OnUpdate()
        {
            return _isSkillEnd ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            Enemy.Value.SkillModule.OnAttackEnd -= HandleSkillEnd;
        }
    }
}

