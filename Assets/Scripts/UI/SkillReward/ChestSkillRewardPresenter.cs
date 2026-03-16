using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.SkillReward
{
    public class ChestSkillRewardPresenter : MonoBehaviour
    {
        [SerializeField] private ChestSkillRewardModel model;
        [SerializeField] private ChestSkillRewardPanelView view;
        [SerializeField] private EventChannelSO systemChannel;

        private void Awake()
        {
            Debug.Assert(model != null, "[ChestSkillRewardPresenter] : model is null.");
            Debug.Assert(view != null, "[ChestSkillRewardPresenter] : view is null.");
            Debug.Assert(systemChannel != null, "[ChestSkillRewardPresenter] : systemChannel is null.");
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

            systemChannel.RaiseEvent(
                SystemEvents.OpenMenu.Init(view.UIData.hashValue));

            model.ClearPendingRequest();
        }
    }
}