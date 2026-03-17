using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SkillReward
{
    public class ChestSkillRewardCardView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform rectTrm;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subTitleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Motion")]
        [SerializeField] private float hiddenYOffset = -140f;
        [SerializeField] private float showDuration = 0.12f;
        [SerializeField] private float hideDuration = 0.08f;

        private Vector2 _originAnchoredPos;
        private RewardCardViewData _currentData;

        public RewardCardViewData CurrentData => _currentData;

        private void Awake()
        {
            Debug.Assert(rectTrm != null, $"[{nameof(ChestSkillRewardCardView)}] : rectTrm is null.");
            Debug.Assert(canvasGroup != null, $"[{nameof(ChestSkillRewardCardView)}] : canvasGroup is null.");

            _originAnchoredPos = rectTrm.anchoredPosition;
            SetHiddenImmediate();
        }

        public void Bind(RewardCardViewData data)
        {
            _currentData = data;

            if (_currentData == null)
            {
                ClearVisual();
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = _currentData.Icon;
                iconImage.enabled = _currentData.Icon != null;
                iconImage.color = _currentData.Icon != null ? Color.white : Color.clear;
            }

            if (titleText != null)
                titleText.text = _currentData.Title;

            if (subTitleText != null)
                subTitleText.text = _currentData.SubTitle;

            if (descriptionText != null)
                descriptionText.text = _currentData.Description;
        }

        private void ClearVisual()
        {
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                iconImage.color = Color.clear;
            }

            if (titleText != null)
                titleText.text = string.Empty;

            if (subTitleText != null)
                subTitleText.text = string.Empty;

            if (descriptionText != null)
                descriptionText.text = string.Empty;
        }

        public void SetHiddenImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            rectTrm.anchoredPosition = _originAnchoredPos + new Vector2(0f, hiddenYOffset);
            rectTrm.localScale = Vector3.one;
        }

        public Tween PlayShow()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            sequence.Join(canvasGroup.DOFade(1f, showDuration * 0.8f));
            sequence.Join(
                rectTrm.DOAnchorPos(_originAnchoredPos, showDuration)
                    .SetEase(Ease.OutCubic));

            return sequence;
        }

        public Tween PlayHide()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            sequence.Join(canvasGroup.DOFade(0f, hideDuration));
            sequence.Join(
                rectTrm.DOAnchorPos(_originAnchoredPos + new Vector2(0f, hiddenYOffset), hideDuration)
                    .SetEase(Ease.InCubic));

            return sequence;
        }
    }
}