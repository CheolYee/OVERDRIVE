using Agents.Players;
using Agents.Players.Skills;
using Gamelib.EventSystem;
using ItemSystem;
using Systems.Database;

namespace Systems.GameEvents
{
    public static class PlayerEvents
    {
        public static readonly ActivePlayerEvent ActivePlayerEvent = new ActivePlayerEvent();
        public static readonly AddExpEvent AddExpEvent = new AddExpEvent();
        public static readonly LevelUpEvent LevelUpEvent = new LevelUpEvent();
        public static readonly PlayerDataSetUpCompleteEvent PlayerDataSetUpComplete = new PlayerDataSetUpCompleteEvent();
        public static readonly PickUpItemEvent PickUpItem = new PickUpItemEvent();

        public static readonly RequestPlayerSkillUpgradeInfoEvent RequestPlayerSkillUpgradeInfo = new RequestPlayerSkillUpgradeInfoEvent();
        public static readonly PlayerSkillUpgradeInfoChangedEvent PlayerSkillUpgradeInfoChanged = new PlayerSkillUpgradeInfoChangedEvent();
        public static readonly RequestUpgradeOwnedSkillEvent RequestUpgradeOwnedSkill = new RequestUpgradeOwnedSkillEvent();
        public static readonly PlayerSkillUpgradeResultEvent PlayerSkillUpgradeResult = new PlayerSkillUpgradeResultEvent();
    }

    public class ActivePlayerEvent : GameEvent
    {
        public bool IsActive { get; private set; }

        public ActivePlayerEvent Init(bool isActive)
        {
            IsActive = isActive;
            return this;
        }
    }

    public class AddExpEvent : GameEvent
    {
        public int Amount { get; private set; }

        public AddExpEvent Init(int amount)
        {
            Amount = amount;
            return this;
        }
    }

    public class LevelUpEvent : GameEvent
    {
        public int NewLevel { get; private set; }

        public LevelUpEvent Init(int newLevel)
        {
            NewLevel = newLevel;
            return this;
        }
    }

    public class PlayerDataSetUpCompleteEvent : GameEvent
    {
        public PlayerData PlayerData { get; private set; }

        public PlayerDataSetUpCompleteEvent Init(PlayerData playerData)
        {
            PlayerData = playerData;
            return this;
        }
    }

    public class PickUpItemEvent : GameEvent
    {
        public ItemObject PickableItem { get; private set; }

        public PickUpItemEvent Init(ItemObject pickableItem)
        {
            PickableItem = pickableItem;
            return this;
        }
    }

    public class RequestPlayerSkillUpgradeInfoEvent : GameEvent
    {
        public PlayerSkill SkillId { get; private set; }

        public RequestPlayerSkillUpgradeInfoEvent Init(PlayerSkill skillId)
        {
            SkillId = skillId;
            return this;
        }
    }

    public class PlayerSkillUpgradeInfoChangedEvent : GameEvent
    {
        public PlayerSkillUpgradeSnapshot Snapshot { get; private set; }

        public PlayerSkillUpgradeInfoChangedEvent Init(PlayerSkillUpgradeSnapshot snapshot)
        {
            Snapshot = snapshot;
            return this;
        }
    }

    public class RequestUpgradeOwnedSkillEvent : GameEvent
    {
        public PlayerSkill SkillId { get; private set; }

        public RequestUpgradeOwnedSkillEvent Init(PlayerSkill skillId)
        {
            SkillId = skillId;
            return this;
        }
    }

    public class PlayerSkillUpgradeResultEvent : GameEvent
    {
        public PlayerSkill SkillId { get; private set; }
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public PlayerSkillUpgradeResultEvent Init(PlayerSkill skillId, bool success, string message)
        {
            SkillId = skillId;
            Success = success;
            Message = message;
            return this;
        }
    }
}