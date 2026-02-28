using System;
using Systems.AnimationSystems;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "PlayClip", story: "[Enemy] play [Clip]", category: "Action/Animation", id: "8b8ef9f4cefefd1319e8e2fc39c65b29")]
    public partial class PlayClipAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<AnimParamSO> Clip;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Clip.Value == null || Enemy.Value.Renderer == null)
                return Status.Failure;
            
            Enemy.Value.Renderer.PlayClip(Clip.Value.ParamHash);
            
            return Status.Success;
        }
    }
}

