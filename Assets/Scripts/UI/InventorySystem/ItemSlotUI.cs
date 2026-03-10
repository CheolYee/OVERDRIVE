using DG.Tweening;
using ItemSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.InventorySystem
{
    public class ItemSlotUI : MonoBehaviour
    {
        [SerializeField] protected Image itemImage;
        [SerializeField] protected TextMeshProUGUI itemAmountText;
        [SerializeField] protected Image selectionImage;
        public InventoryItem Item { get; private set; }

        public void SetSelected(bool isSelected)
        {
            selectionImage.DOComplete();
            selectionImage.DOFade(isSelected ? 1 : 0, 0.1f);
        }

        public void UpdateSlot(InventoryItem item)
        {
            Item = item;
            itemImage.color = Color.white;

            if (Item.ItemData == null) return;
            itemImage.sprite = Item.ItemData.itemIcon;
            itemAmountText.text = Item.StackSize > 1 ? Item.StackSize.ToString() : string.Empty;
        }

        public void CleanUpSlot()
        {
            itemImage.sprite = null;
            itemImage.color = Color.clear;
            itemAmountText.text = string.Empty;
        }
    }
}