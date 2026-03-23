using System;
using Systems.AnimationSystems;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Agents.Enemies.BT.Actions
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "PlayClipAndWaitAction", story: "[Enemy] Play [Clip] and wait end", category: "Action/Animation", id: "33cf62d4141a814bad81d413549cf064")]
    public partial class PlayClipAndWaitAction : Action
    {
        [SerializeReference] public BlackboardVariable<AbstractEnemy> Enemy;
        [SerializeReference] public BlackboardVariable<AnimParamSO> Clip;

        private IAnimatorTrigger _animatorTrigger;
        private bool _isEnded;

        protected override Status OnStart()
        {
            if (Enemy.Value == null || Clip.Value == null || Enemy.Value.Renderer == null)
                return Status.Failure;

            _isEnded = false;
            _animatorTrigger.OnAnimationEnd += HandleAnimationEnd;

            Enemy.Value.Renderer.PlayClip(Clip.Value.ParamHash);
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            return _isEnded ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            if (_animatorTrigger != null)
                _animatorTrigger.OnAnimationEnd -= HandleAnimationEnd;
        }

        private void HandleAnimationEnd()
        {
            _isEnded = true;
        }
    }
}

