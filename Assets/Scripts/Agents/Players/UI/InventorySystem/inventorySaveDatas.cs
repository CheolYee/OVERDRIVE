using System;
using System.Collections.Generic;

namespace Agents.Players.UI.InventorySystem
{
    [Serializable]
    public struct PlayerSkillInventoryEntrySaveData
    {
        public int skillAssetIndex;
        public int acquiredOrder;
    }

    [Serializable]
    public struct PlayerSkillInventorySaveData
    {
        public List<PlayerSkillInventoryEntrySaveData> entries;
        public int nextAcquireOrder;
    }
}