using System;
using System.Collections.Generic;
using Agents.Players.Skills;
using Systems.Database;

namespace UI.InventorySystem
{
    public interface IPlayerSkillInventoryModule
    {
        event Action OnInventoryChanged;

        IReadOnlyList<PlayerSkillInventoryEntry> Entries { get; }

        int GetSkillCount();
        bool Contains(PlayerSkill skillId);

        bool TryGetSkill(PlayerSkill skillId, out PlayerSkillDataSo skillData);
        bool TryGetSkillAt(int index, out PlayerSkillDataSo skillData);

        bool TryGetEntry(PlayerSkill skillId, out PlayerSkillInventoryEntry entry);
        bool TryGetEntryAt(int index, out PlayerSkillInventoryEntry entry);
        bool TryGetShardCount(PlayerSkill skillId, out int shardCount);

        bool TryAddSkill(PlayerSkillDataSo skillData);
        bool TryAcquireSkill(PlayerSkillDataSo skillData, int duplicateShardAmount = 1);
        bool TryAddShards(PlayerSkill skillId, int amount);
        bool TryConsumeShards(PlayerSkill skillId, int amount);
        bool TrySetCurrentSkill(PlayerSkillDataSo skillData);
    }
}