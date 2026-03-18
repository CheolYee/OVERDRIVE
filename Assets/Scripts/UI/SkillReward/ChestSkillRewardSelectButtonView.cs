using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SkillReward
{
    public class ChestSkillRewardSelectButtonView : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTrm;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI labelText;

        [Header("Motion")]
        [SerializeField] private float showDuration = 0.12f;
        [SerializeField] private float hideDuration = 0.08f;
        [SerializeField] private float hiddenScale = 0.86f;

        public event Action OnClicked;

        private void Awake()
        {
            Debug.Assert(rectTrm != null, $"[{nameof(ChestSkillRewardSelectButtonView)}] : rectTrm is null.");
            Debug.Assert(canvasGroup != null, $"[{nameof(ChestSkillRewardSelectButtonView)}] : canvasGroup is null.");
            Debug.Assert(button != null, $"[{nameof(ChestSkillRewardSelectButtonView)}] : button is null.");

            button.onClick.AddListener(HandleClicked);
            SetHiddenImmediate();
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleClicked);
        }

        public void SetLabel(string text)
        {
            if (labelText != null)
                labelText.text = text;
        }

        public void SetHiddenImmediate()
        {
            rectTrm.DOKill();
            canvasGroup.DOKill();

            rectTrm.localScale = Vector3.one * hiddenScale;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public Tween PlayShow()
        {
            rectTrm.DOKill();
            canvasGroup.DOKill();

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            rectTrm.localScale = Vector3.one * hiddenScale;
            canvasGroup.alpha = 0f;

            sequence.Join(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutCubic));
            sequence.Join(rectTrm.DOScale(1f, showDuration).SetEase(Ease.OutBack));

            return sequence;
        }

        public Tween PlayHide()
        {
            rectTrm.DOKill();
            canvasGroup.DOKill();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            sequence.Join(canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.OutCubic));
            sequence.Join(rectTrm.DOScale(hiddenScale, hideDuration).SetEase(Ease.OutCubic));

            return sequence;
        }

        private void HandleClicked()
        {
            OnClicked?.Invoke();
        }
    }
}