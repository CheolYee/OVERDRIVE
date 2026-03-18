using System.Collections.Generic;
using System.Linq;
using Systems.Database;
using UI.InventorySystem;
using UnityEngine;
using PlayerSkillDataSo = Agents.Players.Skills.PlayerSkillDataSo;

namespace UI.SkillReward
{
    public class ChestRewardCandidateGenerator : MonoBehaviour
    {
        [SerializeField] private PlayerSkillInventoryModule inventoryModule;
        [SerializeField] private ChestRewardStatOption[] fallbackStatOptions;

        private void Awake()
        {
            Debug.Assert(inventoryModule != null,
                $"[{nameof(ChestRewardCandidateGenerator)}] : inventoryModule is null.");
        }

        public RewardCardViewData[] GenerateCandidates(int targetCount = 3)
        {
            List<RewardCardViewData> result = new();

            if (inventoryModule == null || targetCount <= 0)
                return result.ToArray();

            List<RewardCardViewData> skillCandidates = BuildSkillCandidates();
            Shuffle(skillCandidates);

            for (int i = 0; i < skillCandidates.Count && result.Count < targetCount; i++)
            {
                result.Add(skillCandidates[i]);
            }

            if (result.Count < targetCount)
            {
                List<RewardCardViewData> statCandidates = BuildStatCandidates();
                Shuffle(statCandidates);

                for (int i = 0; i < statCandidates.Count && result.Count < targetCount; i++)
                {
                    result.Add(statCandidates[i]);
                }
            }

            return result.ToArray();
        }

        private List<RewardCardViewData> BuildSkillCandidates()
        {
            List<RewardCardViewData> candidates = new();

            IReadOnlyCollection<PlayerSkillDataSo> allSkillData = inventoryModule.GetAllKnownSkillData();
            if (allSkillData == null || allSkillData.Count == 0)
                return candidates;

            IEnumerable<IGrouping<PlayerSkill, PlayerSkillDataSo>> groups = allSkillData
                .Where(data => data != null)
                .GroupBy(data => data.skillId);

            foreach (IGrouping<PlayerSkill, PlayerSkillDataSo> group in groups)
            {
                List<PlayerSkillDataSo> ordered = group
                    .OrderBy(data => data.level)
                    .ThenBy(data => data.AssetIndex)
                    .ToList();

                if (ordered.Count == 0)
                    continue;

                PlayerSkillDataSo candidate = ResolveCandidateForGroup(group.Key, ordered);
                if (candidate == null)
                    continue;

                RewardCardViewData viewData = RewardCardViewData.CreateSkill(candidate);
                if (viewData != null)
                    candidates.Add(viewData);
            }

            return candidates;
        }

        private PlayerSkillDataSo ResolveCandidateForGroup(PlayerSkill skillId, List<PlayerSkillDataSo> orderedGroup)
        {
            if (!inventoryModule.TryGetSkill(skillId, out PlayerSkillDataSo ownedSkill))
            {
                return orderedGroup[0];
            }

            foreach (PlayerSkillDataSo t in orderedGroup)
            {
                if (t.level > ownedSkill.level)
                    return t;
            }

            return null;
        }

        private List<RewardCardViewData> BuildStatCandidates()
        {
            List<RewardCardViewData> candidates = new();

            if (fallbackStatOptions == null)
                return candidates;

            HashSet<int> usedStatAssetIndex = new();

            foreach (ChestRewardStatOption option in fallbackStatOptions)
            {
                if (option.statData == null)
                    continue;

                if (!usedStatAssetIndex.Add(option.statData.AssetIndex))
                    continue;

                RewardCardViewData viewData =
                    RewardCardViewData.CreateStat(option.statData, option.amount, option.icon);

                if (viewData != null)
                    candidates.Add(viewData);
            }

            return candidates;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
}