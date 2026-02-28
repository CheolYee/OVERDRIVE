using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemSystem;
using Modules;
using UnityEngine;

namespace Agents.Players
{
    public class PlayerInventory : AbstractInventory, IModule
    {
        [field: SerializeField] public int MaxInventorySize {get; private set;}
        private Player _player;
        
        public void Initialize(ModuleOwner owner)
        {
            _player = owner as Player;
            Debug.Assert(_player != null, $"{gameObject.name}은 플레이어가 아닙니다.");
            ItemDict = new Dictionary<int, InventoryItem>();
        }

        public override void AddItem(AbstractItemDataSo itemData, int count = 1)
        {
            IEnumerable<InventoryItem> items = GetItems(itemData);

            InventoryItem canAddItem = items.FirstOrDefault(item => item.IsFullStack == false);

            if (canAddItem.ItemData == null)
            {
                CreateNewInventoryItem(itemData, count);
            }
            else
            {
                while (canAddItem.ItemData != null && count > 0)
                {
                    int remain = canAddItem.AddStack(count);
                    count = remain;
                    ItemDict[canAddItem.SlotNumber] = canAddItem;
                    if (count > 0)
                        canAddItem = items.FirstOrDefault(item => item.IsFullStack == false);
                }
                if (count > 0)
                    CreateNewInventoryItem(itemData, count);
            }
            
            base.AddItem(itemData, count);
        }

        private void CreateNewInventoryItem(AbstractItemDataSo itemData, int count)
        {
            while (count > 0)
            {
                int slotNumber = FindEmptySlotNumber();
                int stackSize = count > itemData.maxStack ? itemData.maxStack : count;
                InventoryItem newItem = new InventoryItem(itemData, slotNumber, stackSize);
                count -= itemData.maxStack;
                ItemDict.Add(newItem.SlotNumber, newItem);
            }
        }

        private int FindEmptySlotNumber()
        {
            for (int i = 0; i < MaxInventorySize; i++)
            {
                if (!ItemDict.ContainsKey(i)) return i;
            }
            return -1;
        }

        public override void RemoveAt(int index, int count)
        {
            if (ItemDict.TryGetValue(index, out InventoryItem item))
            {
                int targetCount = Mathf.Min(item.StackSize, count);
                item.RemoveStack(targetCount);
                if (item.StackSize == 0)
                    ItemDict.Remove(index);
                else
                    ItemDict[index] = item;
            }
            base.RemoveAt(index, count);
        }

        public override bool CanAddItem(AbstractItemDataSo itemData, int count = 1)
        {
            int emptySlotCount = MaxInventorySize - ItemDict.Count;
            int emptyCapacity = itemData.maxStack * emptySlotCount;

            if (emptyCapacity > count)
                return true;

            int emptySize = GetItems(itemData)
                .Where(item => !item.IsFullStack)
                .Select(item => itemData.maxStack - item.StackSize).Sum();
            
            return emptySize + emptyCapacity >= count;
        }

    }
}