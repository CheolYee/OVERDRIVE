using Agents.Players;
using Gamelib.EventSystem;
using ItemSystem;

namespace Systems.GameEvents
{
    public static class PlayerEvents
    {
        public static readonly ActivePlayerEvent ActivePlayerEvent = new ActivePlayerEvent();
        public static readonly AddExpEvent AddExpEvent = new AddExpEvent();
        public static readonly LevelUpEvent LevelUpEvent = new LevelUpEvent();
        public static readonly PlayerDataSetUpCompleteEvent PlayerDataSetUpComplete = new PlayerDataSetUpCompleteEvent();
        public static readonly PickUpItemEvent PickUpItem = new PickUpItemEvent();
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
        public ItemObject PickableItem {get; private set;}

        public PickUpItemEvent Init(ItemObject pickableItem)
        {
            PickableItem = pickableItem;
            return this;
        }
    }
}