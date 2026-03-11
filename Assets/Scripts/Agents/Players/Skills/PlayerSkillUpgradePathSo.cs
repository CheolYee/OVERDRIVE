using System;
using Systems.Database;
using UnityEngine;

namespace Agents.Players.Skills
{
    [Serializable]
    public struct PlayerSkillUpgradeTier
    {
        public PlayerSkillDataSo skillData;
        [Min(0)] public int requiredShardsToNext;
        [Min(0)] public int requiredGoldToNext;
    }

    [CreateAssetMenu(fileName = "Player Skill Upgrade Path", menuName = "Combat/Player Skill Upgrade Path", order = 16)]
    public class PlayerSkillUpgradePathSo : ScriptableObject
    {
        public PlayerSkill skillId;
        [Min(1)] public int duplicateShardReward = 1;
        public PlayerSkillUpgradeTier[] tiers;

        public int MaxLevel => tiers?.Length ?? 0;

        public bool TryGetTierIndex(PlayerSkillDataSo skillData, out int tierIndex)
        {
            tierIndex = -1;

            if (skillData == null || tiers == null)
                return false;

            for (int i = 0; i < tiers.Length; i++)
            {
                if (tiers[i].skillData == null)
                    continue;

                if (tiers[i].skillData.AssetIndex == skillData.AssetIndex)
                {
                    tierIndex = i;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetCurrentTier(PlayerSkillDataSo currentSkill, out PlayerSkillUpgradeTier currentTier, out int tierIndex)
        {
            currentTier = default;
            tierIndex = -1;

            if (!TryGetTierIndex(currentSkill, out tierIndex))
                return false;

            currentTier = tiers[tierIndex];
            return true;
        }

        public bool TryGetNextTier(PlayerSkillDataSo currentSkill, out PlayerSkillUpgradeTier currentTier, out PlayerSkillUpgradeTier nextTier)
        {
            currentTier = default;
            nextTier = default;

            if (!TryGetCurrentTier(currentSkill, out currentTier, out int tierIndex))
                return false;

            int nextIndex = tierIndex + 1;
            if (tiers == null || nextIndex >= tiers.Length)
                return false;

            nextTier = tiers[nextIndex];
            return nextTier.skillData != null;
        }
    }
}