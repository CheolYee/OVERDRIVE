using System;
using Agents.Players.Skills;

namespace Agents.Players
{
    public interface IPlayerDashLoadoutModule
    {
        event Action OnDashLoadoutChanged;
        event Action OnDashPreviewChanged;

        bool TryPeekNextDashSkill(out PlayerSkillDataSo skillData);
        bool TryGetDashPreview(out PlayerSkillDataSo currentSkill, out PlayerSkillDataSo nextSkill);
        bool AdvanceToNextDashSkill();

        void MarkDashSkillStarted();
        void ClearDashSkillStarted();
        bool CanReleaseDashCharge();

        int GetDashSlotCount();
        int GetUnlockedDashSlotCount();
        bool IsDashSlotUnlocked(int slotIndex);
        PlayerSkillDataSo GetEquippedDashSkill(int slotIndex);

        bool UnlockDashSlot(int slotIndex);
        bool LockDashSlot(int slotIndex);
        bool EquipDashSkill(int slotIndex, PlayerSkillDataSo skillData);
        bool UnequipDashSkill(int slotIndex);
        bool SwapDashSlots(int fromIndex, int toIndex);
    }
}