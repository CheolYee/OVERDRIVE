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
        public static readonly PlayerDataSetUpEvent PlayerDataSetUpEvent = new PlayerDataSetUpEvent();
        
        public static readonly PickUpItemEvent PickUpItem = new PickUpItemEvent();

        public static readonly EquipSkillRequestEvent EquipSkillRequest = new EquipSkillRequestEvent();
        public static readonly SwapSkillSlotsRequestEvent SwapSkillSlotsRequest = new SwapSkillSlotsRequestEvent();
        public static readonly UnequipSkillRequestEvent UnequipSkillRequest = new UnequipSkillRequestEvent();
    }
    
    public class PlayerDataSetUpEvent : GameEvent
    {
        public PlayerData PlayerData;

        public PlayerDataSetUpEvent Init(PlayerData playerData)
        {
            PlayerData = playerData;
            return this;
        }
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
    
    public class EquipSkillRequestEvent : GameEvent
    {
        public int TargetSlotIndex { get; private set; }
        public PlayerSkillDataSo SkillData { get; private set; }
        public bool Result { get; private set; }

        public EquipSkillRequestEvent Init(int targetSlotIndex, PlayerSkillDataSo skillData)
        {
            TargetSlotIndex = targetSlotIndex;
            SkillData = skillData;
            Result = false;
            return this;
        }

        public void SetResult(bool result)
        {
            Result = result;
        }
    }

    public class SwapSkillSlotsRequestEvent : GameEvent
    {
        public int FromSlotIndex { get; private set; }
        public int ToSlotIndex { get; private set; }
        public bool Result { get; private set; }

        public SwapSkillSlotsRequestEvent Init(int fromSlotIndex, int toSlotIndex)
        {
            FromSlotIndex = fromSlotIndex;
            ToSlotIndex = toSlotIndex;
            Result = false;
            return this;
        }

        public void SetResult(bool result)
        {
            Result = result;
        }
    }

    public class UnequipSkillRequestEvent : GameEvent
    {
        public int TargetSlotIndex { get; private set; }
        public bool Result { get; private set; }

        public UnequipSkillRequestEvent Init(int targetSlotIndex)
        {
            TargetSlotIndex = targetSlotIndex;
            Result = false;
            return this;
        }

        public void SetResult(bool result)
        {
            Result = result;
        }
    }
}