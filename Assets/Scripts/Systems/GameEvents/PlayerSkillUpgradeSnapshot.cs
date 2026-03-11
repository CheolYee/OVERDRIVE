using System;
using Agents.Players.Skills;
using Systems.Database;
using UnityEngine;

namespace Systems.GameEvents
{
    [Serializable]
    public struct PlayerSkillUpgradeSnapshot
    {
        public PlayerSkill skillId;
        public string skillName;
        public Sprite skillIcon;
        public int currentLevel;
        public int maxLevel;
        public int shardCount;
        public int requiredShards;
        public int requiredGold;
        public bool canUpgrade;
        public bool isMaxLevel;
    }
}