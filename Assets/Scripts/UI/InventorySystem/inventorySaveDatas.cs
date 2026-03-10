using System;
using System.Collections.Generic;

namespace UI.InventorySystem
{
    [Serializable]
    public class PlayerSkillInventoryEntrySaveData
    {
        public int skillAssetIndex;
        public int acquiredOrder;
        public int shardCount;
    }

    [Serializable]
    public class PlayerSkillInventorySaveData
    {
        public int nextAcquireOrder;
        public List<PlayerSkillInventoryEntrySaveData> entries;
    }
}