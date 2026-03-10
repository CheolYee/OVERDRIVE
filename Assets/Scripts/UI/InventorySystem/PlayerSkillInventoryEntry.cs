using System;
using Systems.Database;

namespace UI.InventorySystem
{
    [Serializable]
    public struct PlayerSkillInventoryEntry
    {
        public PlayerSkill skillId; //스킬 식별자
        public int currentSkillAssetIndex; //현재 보유 중인 스킬 SO
        public int acquiredOrder; //표시 순서
        public int shardCount; //스킬 조각 수
    }
}