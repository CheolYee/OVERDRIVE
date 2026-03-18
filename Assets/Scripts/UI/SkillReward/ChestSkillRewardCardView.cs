using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.SkillReward
{
    public class ChestSkillRewardCardView : MonoBehaviour, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform rectTrm;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subTitleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private ChestSkillRewardSelectButtonView selectButtonView;

        [Header("Motion")]
        [SerializeField] private float hiddenYOffset = -140f;
        [SerializeField] private float showDuration = 0.12f;
        [SerializeField] private float hideDuration = 0.08f;

        [Header("Select Motion")]
        [SerializeField] private float selectedLiftY = 28f;
        [SerializeField] private float selectMoveDuration = 0.12f;

        private Vector2 _originAnchoredPos;
        private RewardCardViewData _currentData;
        private bool _isOriginCaptured;
        private bool _isSelected;

        public RewardCardViewData CurrentData => _currentData;
        public bool IsSelected => _isSelected;

        public event Action<ChestSkillRewardCardView> OnCardClicked;
        public event Action<RewardCardViewData> OnAcquireRequested;

        private void Awake()
        {
            Debug.Assert(rectTrm != null, $"[{nameof(ChestSkillRewardCardView)}] : rectTrm is null.");
            Debug.Assert(canvasGroup != null, $"[{nameof(ChestSkillRewardCardView)}] : canvasGroup is null.");
            Debug.Assert(selectButtonView != null, $"[{nameof(ChestSkillRewardCardView)}] : selectButtonView is null.");

            selectButtonView.OnClicked += HandleSelectButtonClicked;
        }

        private void OnDestroy()
        {
            if (selectButtonView != null)
                selectButtonView.OnClicked -= HandleSelectButtonClicked;
        }

        public void CaptureOrigin()
        {
            if (_isOriginCaptured)
                return;

            Canvas.ForceUpdateCanvases();
            _originAnchoredPos = rectTrm.anchoredPosition;
            _isOriginCaptured = true;
        }

        public void Bind(RewardCardViewData data)
        {
            _currentData = data;
            _isSelected = false;

            if (_currentData == null)
            {
                ClearVisual();
                selectButtonView.SetHiddenImmediate();
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

            selectButtonView.SetLabel(_currentData.CardType == RewardCardType.Skill ? "획득하기" : "선택하기");
            selectButtonView.SetHiddenImmediate();
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
            if (!_isOriginCaptured)
                CaptureOrigin();

            _isSelected = false;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            rectTrm.anchoredPosition = _originAnchoredPos + new Vector2(0f, hiddenYOffset);
            rectTrm.localScale = Vector3.one;

            selectButtonView.SetHiddenImmediate();
        }

        public Tween PlayShow()
        {
            if (!_isOriginCaptured)
                CaptureOrigin();

            _isSelected = false;

            canvasGroup.DOKill();
            rectTrm.DOKill();

            selectButtonView.SetHiddenImmediate();

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            sequence.Join(canvasGroup.DOFade(1f, showDuration * 0.8f));
            sequence.Join(rectTrm.DOAnchorPos(_originAnchoredPos, showDuration).SetEase(Ease.OutCubic));

            return sequence;
        }

        public Tween PlayHide()
        {
            if (!_isOriginCaptured)
                CaptureOrigin();

            _isSelected = false;

            canvasGroup.DOKill();
            rectTrm.DOKill();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            selectButtonView.PlayHide();

            Sequence sequence = DOTween.Sequence();
            sequence.SetUpdate(true);

            sequence.Join(canvasGroup.DOFade(0f, hideDuration));
            sequence.Join(
                rectTrm.DOAnchorPos(_originAnchoredPos + new Vector2(0f, hiddenYOffset), hideDuration)
                    .SetEase(Ease.InCubic));

            return sequence;
        }

        public void SetSelected(bool isSelected, bool immediate = false)
        {
            if (!_isOriginCaptured)
                CaptureOrigin();

            _isSelected = isSelected;

            rectTrm.DOKill();

            Vector2 targetPos = isSelected
                ? _originAnchoredPos + new Vector2(0f, selectedLiftY)
                : _originAnchoredPos;

            if (immediate)
            {
                rectTrm.anchoredPosition = targetPos;

                if (isSelected)
                    selectButtonView.SetHiddenImmediate();
                else
                    selectButtonView.SetHiddenImmediate();

                if (isSelected)
                    selectButtonView.PlayShow();
                else
                    selectButtonView.SetHiddenImmediate();

                return;
            }

            rectTrm.DOAnchorPos(targetPos, selectMoveDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);

            if (isSelected)
                selectButtonView.PlayShow();
            else
                selectButtonView.PlayHide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_currentData == null || canvasGroup.alpha <= 0f)
                return;

            OnCardClicked?.Invoke(this);
        }

        private void HandleSelectButtonClicked()
        {
            if (_currentData == null)
                return;

            OnAcquireRequested?.Invoke(_currentData);
        }
    }
}