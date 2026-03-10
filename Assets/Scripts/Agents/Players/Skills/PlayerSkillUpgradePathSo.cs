using System;
using Systems.Database;
using UnityEngine;

namespace Agents.Players.Skills
{
    [CreateAssetMenu(fileName = "PlayerSkillUpgradePath", menuName = "Combat/Player Skill Upgrade Path", order = 16)]
    public class PlayerSkillUpgradePathSo : ScriptableObject
    {
        [SerializeField] private PlayerSkill skillId;
        [SerializeField] [Min(1)] private int duplicateShardReward = 1;
        [SerializeField] private PlayerSkillDataSo[] levelSkills;
        [SerializeField] private SkillUpgradeCost[] upgradeCosts;

        public PlayerSkill SkillId => skillId;
        public int DuplicateShardReward => duplicateShardReward;
        public int MaxLevel => levelSkills == null ? 0 : levelSkills.Length;

        public PlayerSkillDataSo GetEntrySkill()
        {
            if (levelSkills == null || levelSkills.Length == 0)
                return null;

            return levelSkills[0];
        }

        public bool TryGetSkillAtLevel(int level, out PlayerSkillDataSo skillData)
        {
            skillData = null;

            if (levelSkills == null)
                return false;

            int index = level - 1;
            if (index < 0 || index >= levelSkills.Length)
                return false;

            skillData = levelSkills[index];
            return skillData != null;
        }

        public bool TryGetCurrentLevel(PlayerSkillDataSo currentSkillData, out int level)
        {
            level = 0;

            if (currentSkillData == null || levelSkills == null)
                return false;

            for (int i = 0; i < levelSkills.Length; i++)
            {
                PlayerSkillDataSo levelSkill = levelSkills[i];
                if (levelSkill == null)
                    continue;

                if (levelSkill.AssetIndex == currentSkillData.AssetIndex)
                {
                    level = i + 1;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetNextSkill(PlayerSkillDataSo currentSkillData, out PlayerSkillDataSo nextSkillData)
        {
            nextSkillData = null;

            if (!TryGetCurrentLevel(currentSkillData, out int currentLevel))
                return false;

            return TryGetSkillAtLevel(currentLevel + 1, out nextSkillData);
        }

        public bool TryGetUpgradeCost(PlayerSkillDataSo currentSkillData, out SkillUpgradeCost upgradeCost)
        {
            upgradeCost = default;

            if (!TryGetCurrentLevel(currentSkillData, out int currentLevel))
                return false;

            int costIndex = currentLevel - 1;
            if (upgradeCosts == null || costIndex < 0 || costIndex >= upgradeCosts.Length)
                return false;

            upgradeCost = upgradeCosts[costIndex];
            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levelSkills == null)
                return;

            for (int i = 0; i < levelSkills.Length; i++)
            {
                PlayerSkillDataSo skillData = levelSkills[i];
                if (skillData == null)
                    continue;

                if (skillData.skillId != skillId)
                {
                    Debug.LogWarning($"{name} : levelSkills[{i}] 의 skillId가 {skillId} 와 다릅니다.");
                }
            }
        }
#endif
    }

    [Serializable]
    public struct SkillUpgradeCost
    {
        [Min(0)] public int shardCost;
        [Min(0)] public int goldCost;
    }
}