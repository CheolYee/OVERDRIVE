using System.Collections;
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
        [field: SerializeField] public PoolItemSo PoolItem { get; set; }
        [SerializeField] private EventChannelSO uiEventChannel;
        [SerializeField] private PoolManagerSo poolManager;
        [SerializeField] private AnimParamSO openAnimParam;
        [SerializeField] private AnimParamSO idleAnimParam;
        [SerializeField, Min(0f)] private float returnDelay = 1f;

        public GameObject GameObject => gameObject;
        
        private IRenderer _renderer;
        private IAnimatorTrigger _trigger;

        private Coroutine _returnCoroutine;
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
            Debug.Assert(poolManager != null, "[SkillRewardChest] : PoolManagerSo is not assigned.");
        }

        private void OnEnable()
        {
            _trigger.OnAnimationEnd += HandleAnimationEnd;
        }

        private void OnDisable()
        {
            _trigger.OnAnimationEnd -= HandleAnimationEnd;

            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
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

            if (_returnCoroutine != null)
                StopCoroutine(_returnCoroutine);

            _returnCoroutine = StartCoroutine(ReturnToPoolAfterDelay());
        }

        private IEnumerator ReturnToPoolAfterDelay()
        {
            yield return new WaitForSeconds(returnDelay);

            _returnCoroutine = null;

            if (poolManager != null && gameObject.activeSelf)
                poolManager.Push(this);
        }

        public void ResetItem()
        {
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }

            _isOpened = false;
            _isOpening = false;
            gameObject.layer = LayerMask.NameToLayer("Environments");
            _renderer.PlayClip(idleAnimParam.ParamHash);
        }
    }
}