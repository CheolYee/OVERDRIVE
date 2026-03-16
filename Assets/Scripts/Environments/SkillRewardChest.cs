using CombatSystem;
using Gamelib.EventSystem;
using Gamelib.ObjectPool.Runtime;
using Modules;
using Systems.AnimationSystems;
using Systems.GameEvents;
using UnityEngine;

namespace Environments
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SkillRewardChest : ModuleOwner, IDamageable, IPoolable
    {
        [SerializeField] private EventChannelSO uiEventChannel;
        [SerializeField] private AnimParamSO openAnimParam;
        [SerializeField] private AnimParamSO idleAnimParam;
        [SerializeField] private bool isFlip;
        public PoolItemSo PoolItem { get; set; }
        public GameObject GameObject => gameObject;
        
        private IRenderer _renderer;
        private IAnimatorTrigger _trigger;
        
        private bool _isOpening;
        private bool _isOpened;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _renderer = GetModule<IRenderer>();
            _trigger = GetModule<IAnimatorTrigger>();
            
            Debug.Assert(_renderer != null, "[SkillRewardChest] : Renderer module is missing.");
            Debug.Assert(_trigger != null, "[SkillRewardChest] : Trigger module is missing.");
            Debug.Assert(uiEventChannel != null, "[SkillRewardChest] : UI Event Channel is not assigned.");
        }

        protected override void AfterInitComponents()
        {
            base.AfterInitComponents();
            if (isFlip)
                _renderer.Flip();
        }

        private void OnEnable()
        {
            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        private void OnDisable()
        {
            _trigger.OnAnimationEnd -= HandleAnimationEnd;
        }

        public void ApplyDamage(DamageData damageData, Vector2 hitPoint, Vector2 hitDirection, Vector2 hitNormal)
        {
            if (_isOpened || _isOpening)
                return;
            
            BeginOpen();
        }
        
        private void BeginOpen()
        {
            _isOpening = true;
            gameObject.layer = LayerMask.NameToLayer("DeadBody");
            _renderer.PlayClip(openAnimParam.ParamHash);
        }
        private void HandleAnimationEnd()
        {
            if (!_isOpening || _isOpened)
                return;

            _isOpened = true;
            _isOpening = false;
            
            uiEventChannel.RaiseEvent(
                EnvironmentEvents.OpenChestSkillRewardEvent
                    .Init(gameObject.GetInstanceID(), transform.position));
        }

        public void ResetItem()
        {
            _isOpened = false;
            _isOpening = false;
            gameObject.layer = LayerMask.NameToLayer("Environments");
            _renderer.PlayClip(idleAnimParam.ParamHash);
        }
    }
}