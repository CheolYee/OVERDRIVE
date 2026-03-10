using System.Collections.Generic;
using Agents.Players.Skills;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.InventorySystem
{
    public class PlayerSkillLoadoutSectionUI : MonoBehaviour
    {
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }

        [SerializeField] private GameObject loadoutSlotPrefab;
        [SerializeField] private Transform loadoutSlotParent;

        private readonly List<PlayerSkillLoadoutSlotView> _slotViews = new();

        private void Awake()
        {
            if (PlayerChannel != null)
                PlayerChannel.AddListener<PlayerDashLoadoutChangedEvent>(HandleDashLoadoutChangedEvent);
        }

        private void OnDestroy()
        {
            if (PlayerChannel != null)
                PlayerChannel.RemoveListener<PlayerDashLoadoutChangedEvent>(HandleDashLoadoutChangedEvent);
        }

        private void HandleDashLoadoutChangedEvent(PlayerDashLoadoutChangedEvent evt)
        {
            Refresh(evt.Slots);
        }

        private void Refresh(DashSkillSlot[] slots)
        {
            int slotCount = slots?.Length ?? 0;
            EnsureSlotCount(slotCount);

            for (int i = 0; i < _slotViews.Count; i++)
            {
                bool hasSlot = i < slotCount;
                _slotViews[i].gameObject.SetActive(hasSlot);

                if (hasSlot)
                    _slotViews[i].Bind(i, slots[i]);
            }
        }

        private void EnsureSlotCount(int targetCount)
        {
            if (loadoutSlotPrefab == null || loadoutSlotParent == null)
                return;

            while (_slotViews.Count < targetCount)
            {
                GameObject slotObject = Instantiate(loadoutSlotPrefab, loadoutSlotParent);
                PlayerSkillLoadoutSlotView slotView = slotObject.GetComponent<PlayerSkillLoadoutSlotView>();
                Debug.Assert(slotView != null, $"{slotObject.name} 에 {nameof(PlayerSkillLoadoutSlotView)} 가 없습니다.");
                _slotViews.Add(slotView);
            }
        }
    }
}