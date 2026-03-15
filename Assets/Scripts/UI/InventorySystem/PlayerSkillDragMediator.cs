using Agents.Players.Skills;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.InventorySystem
{
    public enum PlayerSkillDragSourceType
    {
        None = 0,
        Inventory = 1,
        Loadout = 2
    }

    public class PlayerSkillDragMediator : MonoBehaviour
    {
        [SerializeField] private Canvas panelCanvas;
        [SerializeField] private RectTransform dragGhostRoot;
        [SerializeField] private Image dragGhostIcon;
        [SerializeField] private CanvasGroup dragGhostCanvasGroup;
        [SerializeField] private EventChannelSO playerEventChannel;

        private bool _isDragging;
        private bool _dropHandled;

        private PlayerSkillDataSo _dragSkill;
        private PlayerSkillDragSourceType _sourceType;
        private int _sourceLoadoutSlotIndex = -1;

        private void Awake()
        {
            if (panelCanvas == null)
                panelCanvas = GetComponentInParent<Canvas>();

            HideGhost();
        }

        public bool BeginInventoryDrag(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            if (playerEventChannel == null)
            {
                Debug.LogError($"{nameof(PlayerSkillDragMediator)} : PlayerEventChannel is null.");
                return false;
            }

            _isDragging = true;
            _dropHandled = false;
            _dragSkill = skillData;
            _sourceType = PlayerSkillDragSourceType.Inventory;
            _sourceLoadoutSlotIndex = -1;

            ShowGhost(skillData.skillIcon);
            return true;
        }

        public bool BeginLoadoutDrag(int slotIndex, PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            if (playerEventChannel == null)
            {
                Debug.LogError($"{nameof(PlayerSkillDragMediator)} : PlayerEventChannel is null.");
                return false;
            }

            _isDragging = true;
            _dropHandled = false;
            _dragSkill = skillData;
            _sourceType = PlayerSkillDragSourceType.Loadout;
            _sourceLoadoutSlotIndex = slotIndex;

            ShowGhost(skillData.skillIcon);
            return true;
        }

        public void UpdateDrag(PointerEventData eventData)
        {
            if (!_isDragging || panelCanvas == null || dragGhostRoot == null)
                return;

            RectTransform canvasRect = panelCanvas.transform as RectTransform;
            if (canvasRect == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                dragGhostRoot.anchoredPosition = localPoint;
            }
        }

        public bool TryHandleDropOnLoadout(int targetSlotIndex)
        {
            if (!_isDragging || playerEventChannel == null)
                return false;

            bool result = false;

            switch (_sourceType)
            {
                case PlayerSkillDragSourceType.Inventory:
                {
                    EquipSkillRequestEvent requestEvent =
                        PlayerEvents.EquipSkillRequest.Init(targetSlotIndex, _dragSkill);

                    playerEventChannel.RaiseEvent(requestEvent);
                    result = requestEvent.Result;
                    break;
                }

                case PlayerSkillDragSourceType.Loadout:
                {
                    if (_sourceLoadoutSlotIndex == targetSlotIndex)
                    {
                        result = true;
                    }
                    else
                    {
                        SwapSkillSlotsRequestEvent requestEvent =
                            PlayerEvents.SwapSkillSlotsRequest.Init(_sourceLoadoutSlotIndex, targetSlotIndex);

                        playerEventChannel.RaiseEvent(requestEvent);
                        result = requestEvent.Result;
                    }

                    break;
                }
            }

            if (result)
                _dropHandled = true;

            return result;
        }

        public void TryUnequipByClick(int slotIndex)
        {
            if (playerEventChannel == null)
                return;

            UnequipSkillRequestEvent requestEvent =
                PlayerEvents.UnequipSkillRequest.Init(slotIndex);

            playerEventChannel.RaiseEvent(requestEvent);
        }

        public void EndDrag()
        {
            if (!_isDragging)
                return;

            if (!_dropHandled && _sourceType == PlayerSkillDragSourceType.Loadout)
            {
                if (_sourceLoadoutSlotIndex >= 0 && playerEventChannel != null)
                {
                    UnequipSkillRequestEvent requestEvent =
                        PlayerEvents.UnequipSkillRequest.Init(_sourceLoadoutSlotIndex);

                    playerEventChannel.RaiseEvent(requestEvent);
                }
            }

            ClearDragState();
        }

        private void ClearDragState()
        {
            _isDragging = false;
            _dropHandled = false;
            _dragSkill = null;
            _sourceType = PlayerSkillDragSourceType.None;
            _sourceLoadoutSlotIndex = -1;

            HideGhost();
        }

        private void ShowGhost(Sprite icon)
        {
            if (dragGhostIcon != null)
            {
                dragGhostIcon.sprite = icon;
                dragGhostIcon.enabled = icon != null;
            }

            if (dragGhostCanvasGroup != null)
            {
                dragGhostCanvasGroup.alpha = 1f;
                dragGhostCanvasGroup.blocksRaycasts = false;
                dragGhostCanvasGroup.interactable = false;
            }
        }

        private void HideGhost()
        {
            if (dragGhostIcon != null)
            {
                dragGhostIcon.sprite = null;
                dragGhostIcon.enabled = false;
            }

            if (dragGhostCanvasGroup != null)
            {
                dragGhostCanvasGroup.alpha = 0f;
                dragGhostCanvasGroup.blocksRaycasts = false;
                dragGhostCanvasGroup.interactable = false;
            }
        }
    }
}