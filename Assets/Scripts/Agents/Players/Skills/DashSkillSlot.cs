using System;
using UnityEngine.Serialization;

namespace Agents.Players.Skills
{
    [Serializable]
    public struct DashSkillSlot
    {
        public bool isUnlocked;
        public PlayerSkillDataSo equippedSkill;
    }
}