using Alchemy.Inspector;
using DG.Tweening;
using TMPro;
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

        [Header("Debug")]
        [SerializeField] private Agents.Players.Skills.PlayerSkillDataSo debugSkillData;

        private Sequence _openSequence;

        private void Awake()
        {
            PrepareClosedState();
        }

        [Button]
        public void BindDebug(int chestInstanceId, Vector3 chestWorldPosition)
        {
            RewardCardViewData debugData = RewardCardViewData.CreateSkill(debugSkillData);

            leftCard?.Bind(debugData);
            centerCard?.Bind(debugData);
            rightCard?.Bind(debugData);
        }

        public void PrepareClosedState()
        {
            _openSequence?.Kill();

            leftCard?.SetHiddenImmediate();
            centerCard?.SetHiddenImmediate();
            rightCard?.SetHiddenImmediate();
        }

        [Button]
        public void PlayOpenSequence()
        {
            _openSequence?.Kill();
            PrepareClosedState();

            _openSequence = DOTween.Sequence();
            _openSequence.SetUpdate(true);

            if (leftCard != null)
                _openSequence.Append(leftCard.PlayShow());

            if (centerCard != null)
                _openSequence.AppendInterval(cardInterval)
                    .Append(centerCard.PlayShow());

            if (rightCard != null)
                _openSequence.AppendInterval(cardInterval)
                    .Append(rightCard.PlayShow());
        }

        [Button]
        public void PlayCloseSequence()
        {
            _openSequence?.Kill();

            leftCard?.PlayHide();
            centerCard?.PlayHide();
            rightCard?.PlayHide();
        }
    }
}
