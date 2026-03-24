using System;
using DG.Tweening;
using Gamelib.EventSystem;
using Gamelib.SoundSystem;
using Systems;
using Systems.GameEvents;
using Systems.Managers;
using UnityEngine;

namespace UI.SkillReward
{
    public class RewardCanvasUI : MonoBehaviour
    {
        [field: SerializeField] public UIInputSo UIInput { get; private set; }

        [SerializeField] private EventChannelSO uiChannel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float transitionTime = 0.1f;

        private Tween _fadeTween;

        public UIWindowStatus WindowStatus { get; private set; } = UIWindowStatus.CLOSED;

        private void Awake()
        {
            SetOverlayImmediate(false);
        }

        private void OnDestroy()
        {
            _fadeTween?.Kill();
        }

        public void OpenOverlay(Action endCallback = null)
        {
            if (WindowStatus == UIWindowStatus.OPENED || WindowStatus == UIWindowStatus.OPENING)
                return;

            WindowStatus = UIWindowStatus.OPENING;

            UIInput?.SetEnable(false);
            UIInput?.SetPlayerInputEnable(false);

            SoundPlayManager.Instance.PlaySfx(SfxSounds.CARD_PANEL_OPEN, transform.position);

            SetOverlay(true, true);

            uiChannel.RaiseEvent(UIEvents.BlurPanel.Init(true, () =>
            {
                WindowStatus = UIWindowStatus.OPENED;
                endCallback?.Invoke();
            }));
        }

        public void CloseOverlay(Action endCallback = null)
        {
            if (WindowStatus != UIWindowStatus.OPENED)
                return;

            WindowStatus = UIWindowStatus.CLOSING;

            uiChannel.RaiseEvent(UIEvents.BlurPanel.Init(false, () =>
            {
                SetOverlay(false, true, () =>
                {
                    WindowStatus = UIWindowStatus.CLOSED;
                    UIInput?.SetEnable(true);
                    UIInput?.SetPlayerInputEnable(true);
                    endCallback?.Invoke();
                });
            }));
        }

        private void SetOverlay(bool isOpen, bool isTween, Action endCallback = null)
        {
            float alpha = isOpen ? 1f : 0f;

            _fadeTween?.Kill();

            canvasGroup.interactable = isOpen;
            canvasGroup.blocksRaycasts = isOpen;

            if (isTween)
            {
                _fadeTween = canvasGroup.DOFade(alpha, transitionTime)
                    .SetUpdate(true)
                    .OnComplete(() => endCallback?.Invoke());
            }
            else
            {
                canvasGroup.alpha = alpha;
                endCallback?.Invoke();
            }
        }

        private void SetOverlayImmediate(bool isOpen)
        {
            _fadeTween?.Kill();
            canvasGroup.alpha = isOpen ? 1f : 0f;
            canvasGroup.interactable = isOpen;
            canvasGroup.blocksRaycasts = isOpen;
        }
    }
}