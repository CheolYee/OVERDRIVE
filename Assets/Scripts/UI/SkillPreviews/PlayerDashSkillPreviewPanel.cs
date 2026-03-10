using Agents.Players.Skills;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.SkillPreviews
{
    public class PlayerDashSkillPreviewPanel : MonoBehaviour
    {
        [SerializeField] private EventChannelSO playerEventChannel;
        [SerializeField] private PlayerDashPreviewSlotView currentSlotView;
        [SerializeField] private PlayerDashPreviewSlotView nextSlotView;

        private PlayerSkillDataSo _lastCurrentSkill;
        private PlayerSkillDataSo _lastNextSkill;

        private void Awake()
        {
            if (currentSlotView != null)
                currentSlotView.SetTitle("Current");

            if (nextSlotView != null)
                nextSlotView.SetTitle("Next");
        }

        private void OnEnable()
        {
            if (playerEventChannel != null)
                playerEventChannel.AddListener<PlayerDashPreviewChangedEvent>(HandleDashPreviewChanged);
        }

        private void OnDisable()
        {
            if (playerEventChannel != null)
                playerEventChannel.RemoveListener<PlayerDashPreviewChangedEvent>(HandleDashPreviewChanged);
        }

        private void HandleDashPreviewChanged(PlayerDashPreviewChangedEvent evt)
        {
            bool isCurrentChanged = _lastCurrentSkill != evt.CurrentSkill;
            bool isNextChanged = _lastNextSkill != evt.NextSkill;

            _lastCurrentSkill = evt.CurrentSkill;
            _lastNextSkill = evt.NextSkill;

            if (currentSlotView != null)
                currentSlotView.Bind(evt.CurrentSkill);

            if (nextSlotView != null)
                nextSlotView.Bind(evt.NextSkill);

            if (isCurrentChanged && currentSlotView != null)
                currentSlotView.PlayCurrentMotion();

            if (isNextChanged && nextSlotView != null)
                nextSlotView.PlayNextMotion();
        }
    }
}