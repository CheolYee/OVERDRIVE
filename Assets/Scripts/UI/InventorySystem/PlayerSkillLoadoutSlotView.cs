using Agents.Players.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.InventorySystem
{
    public class PlayerSkillLoadoutSlotView : MonoBehaviour,
        IDropHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI slotIndexText;
        [SerializeField] private Image skillIconImage;
        [SerializeField] private Image lockImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private int _slotIndex;
        private DashSkillSlot _slotData;
        private PlayerSkillDragMediator _dragMediator;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            _dragMediator = GetComponentInParent<PlayerSkillDragMediator>();
        }

        public void Bind(int slotIndex, DashSkillSlot slotData)
        {
            _slotIndex = slotIndex;
            _slotData = slotData;

            if (slotIndexText != null)
                slotIndexText.text = $"{slotIndex + 1}";

            if (lockImage != null)
                lockImage.enabled = !slotData.isUnlocked;

            bool hasSkill = slotData.equippedSkill != null;

            if (skillIconImage != null)
            {
                skillIconImage.sprite = hasSkill ? slotData.equippedSkill.skillIcon : null;
                skillIconImage.color = hasSkill ? Color.white : Color.clear;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!_slotData.isUnlocked)
                return;

            _dragMediator?.TryHandleDropOnLoadout(_slotIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_slotData.isUnlocked || _slotData.equippedSkill == null || _dragMediator == null)
                return;

            if (_dragMediator.BeginLoadoutDrag(_slotIndex, _slotData.equippedSkill) && canvasGroup != null)
                canvasGroup.alpha = 0.6f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _dragMediator?.UpdateDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            _dragMediator?.EndDrag();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_slotData.isUnlocked || _slotData.equippedSkill == null)
                return;

            _dragMediator?.TryUnequipByClick(_slotIndex);
        }
    }
}