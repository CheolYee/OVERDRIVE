using System;
using Agents.Players.Skills;
using Modules;
using System.Linq;
using Systems.Database;
using UnityEngine;

namespace Agents.Players
{
    public class PlayerDashLoadoutModule : MonoBehaviour, IModule, IPlayerDashLoadoutModule
    {
        [SerializeField] private DashSkillSlot[] dashSlots;

        public event Action OnDashLoadoutChanged;
        public event Action OnDashPreviewChanged;

        private int _nextDashSlotIndex;
        private bool _isCurrentSkillStartedByDash;

        public void Initialize(ModuleOwner owner)
        {
            _nextDashSlotIndex = 0;
            _isCurrentSkillStartedByDash = false;
        }

        public bool TryPeekNextDashSkill(out PlayerSkillDataSo skillData)
        {
            ClampNextDashSlotIndex();
            return TryFindDashSkill(_nextDashSlotIndex, 0, out _, out skillData);
        }

        public bool TryGetDashPreview(out PlayerSkillDataSo currentSkill, out PlayerSkillDataSo nextSkill)
        {
            currentSkill = null;
            nextSkill = null;

            ClampNextDashSlotIndex();

            if (!TryFindDashSkill(_nextDashSlotIndex, 0, out _, out currentSkill))
                return false;

            TryFindDashSkill(_nextDashSlotIndex, 1, out _, out nextSkill);
            return true;
        }

        public bool AdvanceToNextDashSkill()
        {
            ClampNextDashSlotIndex();

            if (!TryFindDashSkill(_nextDashSlotIndex, 0, out int foundIndex, out _))
                return false;

            _nextDashSlotIndex = (foundIndex + 1) % dashSlots.Length;
            NotifyDashPreviewChanged();
            return true;
        }

        public void MarkDashSkillStarted()
        {
            _isCurrentSkillStartedByDash = true;
            NotifyDashPreviewChanged();
        }

        public void ClearDashSkillStarted()
        {
            _isCurrentSkillStartedByDash = false;
        }

        public bool CanReleaseDashCharge()
        {
            return _isCurrentSkillStartedByDash;
        }

        public bool ReplaceEquippedSkill(PlayerSkill skillId, PlayerSkillDataSo newSkillData)
        {
            if (newSkillData == null)
                return false;

            bool changed = false;

            for (int i = 0; i < dashSlots.Length; i++)
            {
                DashSkillSlot slot = dashSlots[i];

                if (!slot.isUnlocked || slot.equippedSkill == null)
                    continue;

                if (slot.equippedSkill.skillId != skillId)
                    continue;

                if (slot.equippedSkill.AssetIndex == newSkillData.AssetIndex)
                    continue;

                slot.equippedSkill = newSkillData;
                dashSlots[i] = slot;
                changed = true;
            }

            if (!changed)
                return false;

            NotifyDashLoadoutChanged();
            NotifyDashPreviewChanged();
            return true;
        }

        public int GetDashSlotCount() => dashSlots?.Length ?? 0;

        public int GetUnlockedDashSlotCount()
        {
            if (dashSlots == null)
                return 0;

            return dashSlots.Count(slot => slot.isUnlocked);
        }

        public bool IsDashSlotUnlocked(int slotIndex)
        {
            return IsValidDashSlotIndex(slotIndex) && dashSlots[slotIndex].isUnlocked;
        }

        public PlayerSkillDataSo GetEquippedDashSkill(int slotIndex)
        {
            if (!IsValidDashSlotIndex(slotIndex))
                return null;

            return dashSlots[slotIndex].equippedSkill;
        }

        public bool UnlockDashSlot(int slotIndex)
        {
            if (!IsValidDashSlotIndex(slotIndex))
                return false;

            DashSkillSlot slot = dashSlots[slotIndex];
            if (slot.isUnlocked)
                return false;

            slot.isUnlocked = true;
            dashSlots[slotIndex] = slot;

            NotifyDashLoadoutChanged();
            NotifyDashPreviewChanged();
            return true;
        }

        public bool LockDashSlot(int slotIndex)
        {
            if (!IsValidDashSlotIndex(slotIndex))
                return false;

            DashSkillSlot slot = dashSlots[slotIndex];
            if (!slot.isUnlocked && slot.equippedSkill == null)
                return false;

            slot.isUnlocked = false;
            slot.equippedSkill = null;
            dashSlots[slotIndex] = slot;

            ClampNextDashSlotIndex();
            NotifyDashLoadoutChanged();
            NotifyDashPreviewChanged();
            return true;
        }

        public bool EquipDashSkill(int slotIndex, PlayerSkillDataSo skillData)
        {
            if (!CanModifyDashSlot(slotIndex) || skillData == null)
                return false;

            DashSkillSlot slot = dashSlots[slotIndex];
            if (slot.equippedSkill == skillData)
                return false;

            slot.equippedSkill = skillData;
            dashSlots[slotIndex] = slot;

            NotifyDashLoadoutChanged();
            NotifyDashPreviewChanged();
            return true;
        }

        public bool UnequipDashSkill(int slotIndex)
        {
            if (!CanModifyDashSlot(slotIndex))
                return false;

            DashSkillSlot slot = dashSlots[slotIndex];
            if (slot.equippedSkill == null)
                return false;

            slot.equippedSkill = null;
            dashSlots[slotIndex] = slot;

            NotifyDashLoadoutChanged();
            NotifyDashPreviewChanged();
            return true;
        }

        public bool SwapDashSlots(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return false;

            if (!CanModifyDashSlot(fromIndex) || !CanModifyDashSlot(toIndex))
                return false;

            (dashSlots[fromIndex], dashSlots[toIndex]) = (dashSlots[toIndex], dashSlots[fromIndex]);

            NotifyDashLoadoutChanged();
            NotifyDashPreviewChanged();
            return true;
        }

        private bool IsValidDashSlotIndex(int slotIndex)
        {
            return dashSlots != null && slotIndex >= 0 && slotIndex < dashSlots.Length;
        }

        private bool CanModifyDashSlot(int slotIndex)
        {
            return IsValidDashSlotIndex(slotIndex) && dashSlots[slotIndex].isUnlocked;
        }

        private void ClampNextDashSlotIndex()
        {
            if (dashSlots == null || dashSlots.Length == 0)
            {
                _nextDashSlotIndex = 0;
                return;
            }

            _nextDashSlotIndex = Mathf.Clamp(_nextDashSlotIndex, 0, dashSlots.Length - 1);
        }

        private bool TryFindDashSkill(int startIndex, int skipCount, out int foundIndex, out PlayerSkillDataSo skillData)
        {
            foundIndex = -1;
            skillData = null;

            if (dashSlots == null || dashSlots.Length == 0 || skipCount < 0)
                return false;

            int slotCount = dashSlots.Length;
            int foundOrder = 0;
            int maxSearchCount = slotCount * (skipCount + 1);

            for (int i = 0; i < maxSearchCount; i++)
            {
                int currentIndex = (startIndex + i) % slotCount;
                DashSkillSlot slot = dashSlots[currentIndex];

                if (!slot.isUnlocked || slot.equippedSkill == null)
                    continue;

                if (foundOrder == skipCount)
                {
                    foundIndex = currentIndex;
                    skillData = slot.equippedSkill;
                    return true;
                }

                foundOrder++;
            }

            return false;
        }

        private void NotifyDashLoadoutChanged() => OnDashLoadoutChanged?.Invoke();
        private void NotifyDashPreviewChanged() => OnDashPreviewChanged?.Invoke();
    }
}