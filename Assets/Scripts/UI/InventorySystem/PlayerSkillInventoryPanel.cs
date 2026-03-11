using System.Collections.Generic;
using Agents.Players;
using Agents.Players.Skills;
using Gamelib.EventSystem;
using Systems.GameEvents;
using TMPro;
using UnityEngine;

namespace UI.InventorySystem
{
    public class PlayerSkillInventoryPanel : AbstractPanelUI
    {
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }

        [SerializeField] private GameObject skillSlotPrefab;
        [SerializeField] private Transform skillSlotParent;
        [SerializeField] private TextMeshProUGUI emptyText;

        private readonly List<PlayerSkillInventorySlotView> _slotViews = new();

        private PlayerData _playerData;
        private IPlayerSkillInventoryModule _skillInventory;

        protected virtual void Awake()
        {
            if (PlayerChannel != null)
                PlayerChannel.AddListener<PlayerDataSetUpCompleteEvent>(HandlePlayerDataSetUp);
        }

        private void OnDestroy()
        {
            if (PlayerChannel != null)
                PlayerChannel.RemoveListener<PlayerDataSetUpCompleteEvent>(HandlePlayerDataSetUp);

            UnsubscribeInventory();
        }

        private void HandlePlayerDataSetUp(PlayerDataSetUpCompleteEvent evt)
        {
            if (evt == null || evt.PlayerData == null)
                return;

            if (_playerData == evt.PlayerData)
                return;

            UnsubscribeInventory();

            _playerData = evt.PlayerData;
            _skillInventory = _playerData.SkillInventory;

            if (_skillInventory == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillInventoryPanel)} : SkillInventory is null.");
                Refresh();
                return;
            }

            _skillInventory.OnInventoryChanged += HandleInventoryChanged;
            Refresh();
        }

        private void HandleInventoryChanged()
        {
            Refresh();
        }

        private void Refresh()
        {
            int skillCount = _skillInventory?.GetSkillCount() ?? 0;

            if (emptyText != null)
                emptyText.gameObject.SetActive(skillCount == 0);

            EnsureSlotCount(skillCount);

            for (int i = 0; i < _slotViews.Count; i++)
            {
                bool hasSkill = _skillInventory != null && i < skillCount;
                _slotViews[i].gameObject.SetActive(hasSkill);

                if (!hasSkill)
                    continue;

                if (_skillInventory.TryGetSkillAt(i, out PlayerSkillDataSo skillData))
                {
                    _slotViews[i].Bind(skillData);
                }
            }
        }

        private void EnsureSlotCount(int targetCount)
        {
            if (skillSlotPrefab == null || skillSlotParent == null)
                return;

            while (_slotViews.Count < targetCount)
            {
                GameObject slotObject = Instantiate(skillSlotPrefab, skillSlotParent);
                PlayerSkillInventorySlotView slotView = slotObject.GetComponent<PlayerSkillInventorySlotView>();
                Debug.Assert(slotView != null, $"{slotObject.name} 에 {nameof(PlayerSkillInventorySlotView)} 가 없습니다.");
                _slotViews.Add(slotView);
            }
        }

        private void UnsubscribeInventory()
        {
            if (_skillInventory != null)
                _skillInventory.OnInventoryChanged -= HandleInventoryChanged;

            _skillInventory = null;
            _playerData = null;
        }
    }
}