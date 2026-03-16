using Gamelib.EventSystem;
using UnityEngine;

namespace Systems.GameEvents
{
    public class EnvironmentEvents : GameEvent
    {
        public static readonly OpenChestSkillRewardEvent OpenChestSkillRewardEvent = new OpenChestSkillRewardEvent();
    }
    
    public class OpenChestSkillRewardEvent : GameEvent
    {
        public int ChestInstanceId { get; private set; }
        public Vector3 ChestWorldPosition { get; private set; }

        public OpenChestSkillRewardEvent Init(int chestInstanceId, Vector3 chestWorldPosition)
        {
            ChestInstanceId = chestInstanceId;
            ChestWorldPosition = chestWorldPosition;
            return this;
        }
    }
}