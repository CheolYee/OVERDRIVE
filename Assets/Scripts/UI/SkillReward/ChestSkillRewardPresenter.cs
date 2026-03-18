using UnityEngine;

namespace UI.SkillReward
{
    public class ChestSkillRewardPresenter : MonoBehaviour
    {
        [SerializeField] private ChestSkillRewardModel model;
        [SerializeField] private ChestSkillRewardPopupView view;
        [SerializeField] private RewardCanvasUI rewardCanvasUI;

        private void Awake()
        {
            Debug.Assert(model != null, $"[{nameof(ChestSkillRewardPresenter)}] : model is null.");
            Debug.Assert(view != null, $"[{nameof(ChestSkillRewardPresenter)}] : view is null.");
            Debug.Assert(rewardCanvasUI != null, $"[{nameof(ChestSkillRewardPresenter)}] : rewardCanvasUI is null.");
        }

        private void OnEnable()
        {
            model.OnPendingRequestReceived += HandlePendingRequestReceived;
        }

        private void OnDisable()
        {
            model.OnPendingRequestReceived -= HandlePendingRequestReceived;
        }

        private void HandlePendingRequestReceived()
        {
            if (!model.HasPendingRequest)
                return;

            view.BindCards(model.PendingCards);

            rewardCanvasUI.OpenOverlay(() =>
            {
                view.PlayOpenSequence();
            });

            model.ClearPendingRequest();
        }
    }
}