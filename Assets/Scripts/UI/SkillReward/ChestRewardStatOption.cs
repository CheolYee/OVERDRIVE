using System;
using Agents.StatSystem;
using UnityEngine;

namespace UI.SkillReward
{
    [Serializable]
    public struct ChestRewardStatOption
    {
        public StatSO statData;
        public float amount;
        public Sprite icon;
    }
}