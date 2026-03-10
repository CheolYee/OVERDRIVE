using Agents.Players.Skills;
using Gamelib.EventSystem;

namespace Systems.GameEvents
{
    public static class InventoryEvents
    {
        public static readonly PlayerSkillInventoryChangedEvent PlayerSkillInventoryChanged = new PlayerSkillInventoryChangedEvent();
    }

    public class PlayerSkillInventoryChangedEvent : GameEvent
    {
        public PlayerSkillDataSo[] Skills { get; private set; }

        public PlayerSkillInventoryChangedEvent Init(PlayerSkillDataSo[] skills)
        {
            Skills = skills;
            return this;
        }
    }
}