using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemSystem
{
    public abstract class AbstractInventory : MonoBehaviour
    {
        public Dictionary<int, InventoryItem> ItemDict;

        public event Action OnInventoryChanged;
        
        public virtual InventoryItem GetItem(AbstractItemDataSo targetItem)
            => ItemDict.Values.FirstOrDefault(invenItem => invenItem.ItemData == targetItem);

        public virtual InventoryItem GetItem(int index) => ItemDict.GetValueOrDefault(index);
        
        public virtual IEnumerable<InventoryItem> GetItems(AbstractItemDataSo targetItem)
            => ItemDict.Values.Where(invenItem => invenItem.ItemData == targetItem);

        public virtual void AddItem(AbstractItemDataSo itemData, int count = 1)
        {
            OnInventoryChanged?.Invoke();
        }

        public virtual void RemoveAt(int index, int count)
        {
            OnInventoryChanged?.Invoke();
        }
        
        public abstract bool CanAddItem(AbstractItemDataSo itemData, int count = 1);
    }
}