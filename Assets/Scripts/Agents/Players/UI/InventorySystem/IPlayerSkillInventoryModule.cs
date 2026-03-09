using System;
using System.Collections.Generic;
using Agents.Players.Skills;
using Systems.Database;

namespace Agents.Players.UI.InventorySystem
{
    public interface IPlayerSkillInventoryModule
    {
        event Action OnInventoryChanged;

        int GetSkillCount();
        bool Contains(PlayerSkill skillId);

        bool TryGetSkill(PlayerSkill skillId, out PlayerSkillDataSo skillData);
        bool TryGetSkillAt(int index, out PlayerSkillDataSo skillData);
        bool TryGetEntryAt(int index, out PlayerSkillInventoryEntry entry);

        IReadOnlyList<PlayerSkillInventoryEntry> Entries { get; }

        bool TryAddSkill(PlayerSkillDataSo skillData);
    }
}