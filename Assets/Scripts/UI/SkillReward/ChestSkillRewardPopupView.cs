using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using DG.Tweening;
using UnityEngine;

namespace UI.SkillReward
{
    public class ChestSkillRewardPopupView : MonoBehaviour
    {
        [Header("Cards")]
        [SerializeField] private ChestSkillRewardCardView leftCard;
        [SerializeField] private ChestSkillRewardCardView centerCard;
        [SerializeField] private ChestSkillRewardCardView rightCard;

        [Header("Sequence")]
        [SerializeField] private float cardInterval = 0.05f;

        private Sequence _openSequence;
        private ChestSkillRewardCardView _selectedCard;

        public event Action<RewardCardViewData> OnCardAcquireRequested;

        private void Awake()
        {
            Canvas.ForceUpdateCanvases();

            leftCard?.CaptureOrigin();
            centerCard?.CaptureOrigin();
            rightCard?.CaptureOrigin();

            SubscribeCardEvents();
            PrepareClosedState();
        }

        private void OnDestroy()
        {
            UnsubscribeCardEvents();
        }

        public void BindCards(IReadOnlyList<RewardCardViewData> cards)
        {
            leftCard?.Bind(GetCardOrNull(cards, 0));
            centerCard?.Bind(GetCardOrNull(cards, 1));
            rightCard?.Bind(GetCardOrNull(cards, 2));
        }

        [Button]
        public void PrepareClosedState()
        {
            _openSequence?.Kill();
            _selectedCard = null;

            leftCard?.SetHiddenImmediate();
            centerCard?.SetHiddenImmediate();
            rightCard?.SetHiddenImmediate();
        }

        public void PlayOpenSequence()
        {
            _openSequence?.Kill();
            PrepareClosedState();

            _openSequence = DOTween.Sequence();
            _openSequence.SetUpdate(true);

            if (leftCard != null)
            {
                _openSequence.AppendCallback(() => leftCard.PlayShow());
                _openSequence.AppendInterval(cardInterval);
            }

            if (centerCard != null)
            {
                _openSequence.AppendCallback(() => centerCard.PlayShow());
                _openSequence.AppendInterval(cardInterval);
            }

            if (rightCard != null)
            {
                _openSequence.AppendCallback(() => rightCard.PlayShow());
            }

            _openSequence.OnComplete(() =>
            {
                if (leftCard != null && leftCard.CurrentData != null)
                    SelectCard(leftCard);
            });
        }

        public void PlayCloseSequence()
        {
            _openSequence?.Kill();

            leftCard?.PlayHide();
            centerCard?.PlayHide();
            rightCard?.PlayHide();

            _selectedCard = null;
        }

        private void SubscribeCardEvents()
        {
            if (leftCard != null)
            {
                leftCard.OnCardClicked += HandleCardClicked;
                leftCard.OnAcquireRequested += HandleAcquireRequested;
            }

            if (centerCard != null)
            {
                centerCard.OnCardClicked += HandleCardClicked;
                centerCard.OnAcquireRequested += HandleAcquireRequested;
            }

            if (rightCard != null)
            {
                rightCard.OnCardClicked += HandleCardClicked;
                rightCard.OnAcquireRequested += HandleAcquireRequested;
            }
        }

        private void UnsubscribeCardEvents()
        {
            if (leftCard != null)
            {
                leftCard.OnCardClicked -= HandleCardClicked;
                leftCard.OnAcquireRequested -= HandleAcquireRequested;
            }

            if (centerCard != null)
            {
                centerCard.OnCardClicked -= HandleCardClicked;
                centerCard.OnAcquireRequested -= HandleAcquireRequested;
            }

            if (rightCard != null)
            {
                rightCard.OnCardClicked -= HandleCardClicked;
                rightCard.OnAcquireRequested -= HandleAcquireRequested;
            }
        }

        private void HandleCardClicked(ChestSkillRewardCardView clickedCard)
        {
            if (clickedCard == null || clickedCard.CurrentData == null)
                return;

            SelectCard(clickedCard);
        }

        private void SelectCard(ChestSkillRewardCardView targetCard)
        {
            if (targetCard == null)
                return;

            if (_selectedCard == targetCard)
                return;

            if (_selectedCard != null)
                _selectedCard.SetSelected(false);

            _selectedCard = targetCard;
            _selectedCard.SetSelected(true);
        }

        private void HandleAcquireRequested(RewardCardViewData data)
        {
            if (data == null)
                return;

            OnCardAcquireRequested?.Invoke(data);
        }

        private RewardCardViewData GetCardOrNull(IReadOnlyList<RewardCardViewData> cards, int index)
        {
            if (cards == null || index < 0 || index >= cards.Count)
                return null;

            return cards[index];
        }
    }
}