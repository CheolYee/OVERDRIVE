
using System;
using Systems.Database;

namespace Agents.Players.UI.InventorySystem
{
    [Serializable]
    public struct PlayerSkillInventoryEntry
    {
        public PlayerSkill skillId; //스킬 식별자
        public int currentSkillAssetIndex; //SO 복원을 위해
        public int acquiredOrder; //표시 순서
    }
}