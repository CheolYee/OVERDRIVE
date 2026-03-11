using System;
using Gamelib.EventSystem;
using Modules;
using Systems.CoreSystem;
using Systems.GameEvents;
using UI.InventorySystem;
using UnityEngine;

namespace Agents.Players
{
    public class PlayerData : MonoBehaviour, IModule, ISaveable
    {
        [field: SerializeField] public SaveIdData SaveId { get; private set; }
        [field: SerializeField] public int Gold { get; private set; }
        [field: SerializeField] public int Gem { get; private set; }

        public Player Player { get; private set; }
        public EventChannelSO PlayerChannel => Player != null ? Player.PlayerEventChannel : null;

        public IPlayerSkillInventoryModule SkillInventory { get; private set; }
        public IPlayerDashLoadoutModule DashLoadout { get; private set; }

        public event Action<int> OnGoldChanged;
        public event Action<int> OnGemChanged;

        public void Initialize(ModuleOwner owner)
        {
            Player = owner as Player;
            Debug.Assert(Player != null, $"{gameObject.name} is not attached to Player");

            SkillInventory = owner.GetModule<IPlayerSkillInventoryModule>();
            DashLoadout = owner.GetModule<IPlayerDashLoadoutModule>();
        }

        private void Start()
        {
            PlayerChannel?.RaiseEvent(PlayerEvents.PlayerDataSetUpComplete.Init(this));
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0 || Gold < amount)
                return false;

            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public void AddGem(int amount)
        {
            if (amount <= 0) return;
            Gem += amount;
            OnGemChanged?.Invoke(Gem);
        }

        public bool TrySpendGem(int amount)
        {
            if (amount <= 0 || Gem < amount)
                return false;

            Gem -= amount;
            OnGemChanged?.Invoke(Gem);
            return true;
        }

        public void SetGold(int amount)
        {
            Gold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(Gold);
        }

        public void SetGem(int amount)
        {
            Gem = Mathf.Max(0, amount);
            OnGemChanged?.Invoke(Gem);
        }

        [Serializable]
        private struct PlayerSaveData
        {
            public int gold;
            public int gem;
        }

        public string GetSaveData()
        {
            PlayerSaveData saveData = new PlayerSaveData
            {
                gold = Gold,
                gem = Gem
            };

            return JsonUtility.ToJson(saveData);
        }

        public void RestoreData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(data);
            Gold = Mathf.Max(0, saveData.gold);
            Gem = Mathf.Max(0, saveData.gem);

            OnGoldChanged?.Invoke(Gold);
            OnGemChanged?.Invoke(Gem);
        }
    }
}