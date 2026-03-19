using System;
using DG.Tweening;
using Gamelib.SoundSystem;
using Systems;
using Systems.Managers;
using UnityEngine;

namespace UI.SkillReward
{
    public class RewardCanvasUI : MonoBehaviour
    {
        [field: SerializeField] public UIInputSo UIInput { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float transitionTime = 0.1f;

        public UIWindowStatus WindowStatus { get; private set; } = UIWindowStatus.CLOSED;

        private void Awake()
        {
            SetOverlay(false, false);
        }

        public void OpenOverlay(Action endCallback = null)
        {
            if (WindowStatus == UIWindowStatus.OPENED || WindowStatus == UIWindowStatus.OPENING)
                return;

            WindowStatus = UIWindowStatus.OPENING;
            Time.timeScale = 0f;
            UIInput?.SetEnable(false);
            UIInput?.SetPlayerInputEnable(false);
            SoundPlayManager.Instance.PlaySfx(SfxSounds.CARD_PANEL_OPEN, transform.position);

            SetOverlay(true, true, () =>
            {
                WindowStatus = UIWindowStatus.OPENED;
                endCallback?.Invoke();
            });
        }

        public void CloseOverlay(Action endCallback = null)
        {
            if (WindowStatus != UIWindowStatus.OPENED)
                return;

            WindowStatus = UIWindowStatus.CLOSING;

            SetOverlay(false, true, () =>
            {
                WindowStatus = UIWindowStatus.CLOSED;
                Time.timeScale = 1f;
                UIInput?.SetEnable(true);
                UIInput?.SetPlayerInputEnable(true);
                endCallback?.Invoke();
            });
        }

        private void SetOverlay(bool isOpen, bool isTween, Action endCallback = null)
        {
            float alpha = isOpen ? 1f : 0f;

            canvasGroup.interactable = isOpen;
            canvasGroup.blocksRaycasts = isOpen;

            if (isTween)
            {
                canvasGroup.DOFade(alpha, transitionTime)
                    .SetUpdate(true)
                    .OnComplete(() => endCallback?.Invoke());
            }
            else
            {
                canvasGroup.alpha = alpha;
                endCallback?.Invoke();
            }
        }
    }
}