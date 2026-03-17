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
            Debug.Assert(model != null, "[ChestSkillRewardPresenter] : model is null.");
            Debug.Assert(view != null, "[ChestSkillRewardPresenter] : view is null.");
            Debug.Assert(rewardCanvasUI != null, "[ChestSkillRewardPresenter] : rewardCanvasUI is null.");
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

            view.BindDebug(model.LastChestInstanceId, model.LastChestWorldPosition);

            rewardCanvasUI.OpenOverlay(() =>
            {
                view.PlayOpenSequence();
            });

            model.ClearPendingRequest();
        }
    }
}