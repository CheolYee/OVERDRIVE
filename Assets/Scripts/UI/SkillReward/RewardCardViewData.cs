using UnityEngine;
using Agents.Players.Skills;
using Agents.StatSystem;

namespace UI.SkillReward
{
    public sealed class RewardCardViewData
    {
        public RewardCardType CardType { get; }
        public Sprite Icon { get; }
        public string Title { get; }
        public string SubTitle { get; }
        public string Description { get; }

        public PlayerSkillDataSo SkillData { get; }
        public StatSO StatData { get; }
        public float StatAmount { get; }

        private RewardCardViewData(
            RewardCardType cardType,
            Sprite icon,
            string title,
            string subTitle,
            string description,
            PlayerSkillDataSo skillData,
            StatSO statData,
            float statAmount)
        {
            CardType = cardType;
            Icon = icon;
            Title = title;
            SubTitle = subTitle;
            Description = description;
            SkillData = skillData;
            StatData = statData;
            StatAmount = statAmount;
        }

        public static RewardCardViewData CreateSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return null;

            string subTitle = $"레벨 {skillData.level}";
            string description = skillData.description;

            return new RewardCardViewData(
                RewardCardType.Skill,
                skillData.skillIcon,
                skillData.attackName,
                subTitle,
                description,
                skillData,
                null,
                0f);
        }

        public static RewardCardViewData CreateStat(StatSO statData, float amount, Sprite icon = null)
        {
            if (statData == null)
                return null;

            string title = statData.StatName;
            string subTitle = $"+{amount:0.##}";
            string description = $"{statData.StatName}이(가) 증가합니다.";

            return new RewardCardViewData(
                RewardCardType.Stat,
                icon,
                title,
                subTitle,
                description,
                null,
                statData,
                amount);
        }
    }
}