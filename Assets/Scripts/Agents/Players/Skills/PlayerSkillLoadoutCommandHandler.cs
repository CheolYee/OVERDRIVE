using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Agents.Players.Skills
{
    public class PlayerSkillLoadoutCommandHandler : MonoBehaviour, IHandlePlayerDataSetUp
    {
        [SerializeField] private EventChannelSO playerEventChannel;
        
        private IPlayerDashLoadoutModule _dashLoadoutModule;

        private void OnEnable()
        {
            if (playerEventChannel == null)
                return;
            
            playerEventChannel.AddListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
            playerEventChannel.AddListener<EquipSkillRequestEvent>(HandleEquipDashSkillRequest);
            playerEventChannel.AddListener<SwapSkillSlotsRequestEvent>(HandleSwapDashSkillSlotsRequest);
            playerEventChannel.AddListener<UnequipSkillRequestEvent>(HandleUnequipDashSkillRequest);
        }

        private void OnDisable()
        {
            if (playerEventChannel == null)
                return;
            
            playerEventChannel.RemoveListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
            playerEventChannel.RemoveListener<EquipSkillRequestEvent>(HandleEquipDashSkillRequest);
            playerEventChannel.RemoveListener<SwapSkillSlotsRequestEvent>(HandleSwapDashSkillSlotsRequest);
            playerEventChannel.RemoveListener<UnequipSkillRequestEvent>(HandleUnequipDashSkillRequest);
        }
        
        public void HandlePlayerDataSetUp(PlayerDataSetUpEvent evt)
        {
            _dashLoadoutModule = evt.PlayerData.DashLoadout;
            Debug.Assert(_dashLoadoutModule != null,
                $"[PlayerSkillLoadoutCommandHandler] : 로드아웃 대쉬 모듈이 없습니다.");
        }

        private void HandleEquipDashSkillRequest(EquipSkillRequestEvent evt)
        {
            if (_dashLoadoutModule == null)
            {
                evt.SetResult(false);
                return;
            }

            bool result = _dashLoadoutModule.EquipDashSkill(evt.TargetSlotIndex, evt.SkillData);
            evt.SetResult(result);
        }

        private void HandleSwapDashSkillSlotsRequest(SwapSkillSlotsRequestEvent evt)
        {
            if (_dashLoadoutModule == null)
            {
                evt.SetResult(false);
                return;
            }

            bool result = _dashLoadoutModule.SwapDashSlots(evt.FromSlotIndex, evt.ToSlotIndex);
            evt.SetResult(result);
        }

        private void HandleUnequipDashSkillRequest(UnequipSkillRequestEvent evt)
        {
            if (_dashLoadoutModule == null)
            {
                evt.SetResult(false);
                return;
            }

            bool result = _dashLoadoutModule.UnequipDashSkill(evt.TargetSlotIndex);
            evt.SetResult(result);
        }
    }
}