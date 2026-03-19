using Agents.Players.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.InventorySystem
{
    public class PlayerSkillInventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image skillIconImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private PlayerSkillDataSo _skillData;
        private PlayerSkillDragMediator _dragMediator;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            _dragMediator = GetComponentInParent<PlayerSkillDragMediator>();
        }

        public void Bind(PlayerSkillDataSo skillData)
        {
            _skillData = skillData;
            bool hasSkill = skillData != null;

            if (skillIconImage != null)
            {
                skillIconImage.sprite = hasSkill ? skillData.skillIcon : null;
                skillIconImage.color = hasSkill ? Color.white : Color.clear;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_skillData == null || _dragMediator == null)
                return;

            if (_dragMediator.BeginInventoryDrag(_skillData) && canvasGroup != null)
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
    }
}