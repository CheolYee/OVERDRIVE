using System.Collections.Generic;
using System.Linq;
using Agents.Players;
using Gamelib.EventSystem;
using ItemSystem;
using Systems;
using Systems.GameEvents;
using UnityEngine;

namespace UI.InventorySystem
{
    public class InventoryPanelUI : AbstractPanelUI
    {
        [field: SerializeField] public EventChannelSO PlayerChannel {get; private set;}

        [SerializeField] protected GameObject itemSlotPrefab;
        [SerializeField] protected Transform itemSlotParent;
        private ItemSlotUI[] _itemSlotUis;
        private PlayerData _playerData;
        private int _maxSlotCount;

        protected virtual void Awake()
        {
            _itemSlotUis = itemSlotParent.GetComponentsInChildren<ItemSlotUI>();
            PlayerChannel.AddListener<PlayerDataSetUpCompleteEvent>(HandlePlayerDataSetUp);
        }

        private void OnDestroy()
        {
            PlayerChannel.RemoveListener<PlayerDataSetUpCompleteEvent>(HandlePlayerDataSetUp);
            if (_playerData != null && _playerData.Inventory != null)
                _playerData.Inventory.OnInventoryChanged -= HandleInventoryChange;
        }

        private void HandlePlayerDataSetUp(PlayerDataSetUpCompleteEvent evt)
        {
            _playerData = evt.PlayerData;
            _maxSlotCount = _playerData.Inventory.MaxInventorySize;
            
            _itemSlotUis = new ItemSlotUI[_maxSlotCount];

            for (int i = 0; i < _maxSlotCount; i++)
            {
                CreateSlotUI(i);
            }

            _playerData.Inventory.OnInventoryChanged += HandleInventoryChange;

            UpdateSlotUI();
        }

        private void UpdateSlotUI()
        {
            if (_playerData == null || _playerData.Inventory == null) return;

            foreach (ItemSlotUI slot in _itemSlotUis)
            {
                slot.CleanUpSlot();
            }

            List<InventoryItem> items = _playerData.Inventory.ItemDict.Values.ToList();

            for (int i = 0; i < items.Count; i++)
            {
                _itemSlotUis[i].UpdateSlot(items[i]);
            }
        }

        private void HandleInventoryChange()
        {
            UpdateSlotUI();
        }

        private void CreateSlotUI(int i)
        {
            GameObject itemSlot = Instantiate(itemSlotPrefab, itemSlotParent);
            _itemSlotUis[i] = itemSlot.GetComponent<ItemSlotUI>();
        }
    }
}