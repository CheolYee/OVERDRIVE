using System;
using Modules;
using Systems.AnimationSystems;
using UnityEngine;

namespace Agents
{
    [RequireComponent(typeof(Animator))]
    public class AgentRenderer : MonoBehaviour, IModule, IRenderer, IAnimatorTrigger
    {
        private Agent _owner;
        private Animator _animator;

        [field: SerializeField] public float FacingDirection { get; private set; } = 1f; //외부에서도 고칠 수 있게.
        
        public void Initialize(ModuleOwner owner)
        {
            _owner = owner as Agent;
            _animator = GetComponent<Animator>();
        }

        public void PlayClip(int clipHash, int layer = -1, float normalizedTime = 0)
        => _animator.Play(clipHash, layer, normalizedTime);
        
        public void SetBool(AnimParamSO param, bool value)
            => _animator.SetBool(param.ParamHash, value);
        public void SetFloat(AnimParamSO param, float value)
            => _animator.SetFloat(param.ParamHash, value);
        public void SetInt(AnimParamSO param, int value)
            => _animator.SetInteger(param.ParamHash, value);
        public void SetTrigger(AnimParamSO param)
            => _animator.SetTrigger(param.ParamHash);

        public void FlipController(float xMoveDirection)
        {
            if(Mathf.Abs(FacingDirection + xMoveDirection) < 0.5f)
                Flip();
        }

        public void Flip()
        {
            FacingDirection *= -1;
            float targetYRotation = FacingDirection > 0 ? 0 : 180f;
            _owner.transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
        }

        #region 애니메이션 트리거 섹션

        public event Action OnAnimationEnd;
        public event Action OnAttackTrigger;
        public event Action<bool> OnCounterStateChange;

        private void EndTrigger() => OnAnimationEnd?.Invoke();
        private void AttackTrigger() => OnAttackTrigger?.Invoke();
        private void OpenCounterTrigger() => OnCounterStateChange?.Invoke(true);
        private void CloseCounterTrigger() => OnCounterStateChange?.Invoke(false);
        

        #endregion
        
    }
}