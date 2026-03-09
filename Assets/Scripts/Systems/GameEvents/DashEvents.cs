using Agents.Players.Skills;
using Gamelib.EventSystem;

namespace Systems.GameEvents
{
    public class DashEvents : GameEvent
    {
        public static readonly PlayerDashLoadoutChangedEvent PlayerDashLoadoutChanged = new();
        public static readonly PlayerDashPreviewChangedEvent PlayerDashPreviewChanged = new();
    }
    
    public class PlayerDashLoadoutChangedEvent : GameEvent
    {
        public DashSkillSlot[] Slots { get; private set; }

        public PlayerDashLoadoutChangedEvent Init(DashSkillSlot[] slots)
        {
            Slots = slots;
            return this;
        }
    }

    public class PlayerDashPreviewChangedEvent : GameEvent
    {
        public PlayerSkillDataSo CurrentSkill { get; private set; }
        public PlayerSkillDataSo NextSkill { get; private set; }

        public PlayerDashPreviewChangedEvent Init(PlayerSkillDataSo currentSkill, PlayerSkillDataSo nextSkill)
        {
            CurrentSkill = currentSkill;
            NextSkill = nextSkill;
            return this;
        }
    }
}