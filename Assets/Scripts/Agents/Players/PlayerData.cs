using System;
using System.Collections.Generic;
using System.Linq;
using Agents.StatSystem;
using ItemSystem;
using Modules;
using Systems.CoreSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Agents.Players
{
    public class PlayerData : MonoBehaviour, IModule, ISaveable, IPlayerCurrencyWallet
    {
        [SerializeField] private LevelDataSo levelData;
        [field: SerializeField] public int CurrentExp { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public int SkillPoint { get; private set; }
        [field: SerializeField] public int StatPoint { get; private set; }
        [field: SerializeField] public int Gold { get; private set; }
        [field: SerializeField] public PlayerInventory Inventory { get; private set; }
        
        private Player _player;
        private IStatModule _statModule;
        public void Initialize(ModuleOwner owner)
        {
            _player = owner as Player;
            _statModule = owner.GetModule<IStatModule>();
            Debug.Assert(_player != null, $"{gameObject.name} is not attached to player");
            Debug.Assert(_statModule != null, $"{gameObject.name} is not attached to stat module");
            Debug.Assert(Inventory != null, $"{gameObject.name} is not attached to PlayerInventory");
            
            _player.PlayerEventChannel.AddListener<AddExpEvent>(HandleAddExpEvent);
            _player.PlayerEventChannel.AddListener<PickUpItemEvent>(HandlePickUpItemEvent);
        }

        private void HandlePickUpItemEvent(PickUpItemEvent evt)
        {
            if (Inventory == null) return;
            ItemObject target = evt.PickableItem;

            if (Inventory.CanAddItem(target.ItemData, target.Amount))
            {
                Inventory.AddItem(target.ItemData, target.Amount);
                target.PickUpComplete(true);
            }
            else
            {
                target.PickUpComplete(false);
            }
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.PlayerEventChannel.RemoveListener<AddExpEvent>(HandleAddExpEvent);
                _player.PlayerEventChannel.RemoveListener<PickUpItemEvent>(HandlePickUpItemEvent);
            }
        }   

        private void Start()
        {
            _player.PlayerEventChannel.RaiseEvent(PlayerEvents.PlayerDataSetUpComplete.Init(this));
        }

        private void HandleAddExpEvent(AddExpEvent obj)
        {
            CurrentExp += obj.Amount;

            while (TryLevelUp())
            {
                _player.PlayerEventChannel.RaiseEvent(PlayerEvents.LevelUpEvent.Init(Level));
            }
        }

        private bool TryLevelUp()
        {
            if (levelData.IsMaxLevel(Level)) return false;

            int requiredExp = levelData.GetRequiredExp(Level);
            if (CurrentExp >= requiredExp)
            {
                CurrentExp -= requiredExp;
                Level++;
                SkillPoint += levelData.skillPerLevelUp;
                StatPoint += levelData.statPerLevelUp;
                return true;
            }
            
            return false;
        }

        #region 세이브 로직

        [Header("Save Data Settings")]
        
        [field: SerializeField] public SaveIdData SaveId { get; private set; }
        
        [Serializable]
        public struct StatSaveData
        {
            public int assetIndex;
            public float baseValue;
        }
        
        [Serializable]
        public struct PlayerSaveData
        {
            public int currentExp;
            public int level;
            public int skillPoint;
            public int gold;
            public int statPoint;
            public List<StatSaveData> stats;
        }
        public string GetSaveData()
        {
            List<StatSaveData> saveStatData = _statModule.GetAllStats()
                .Select(stat => new StatSaveData
                {
                    assetIndex = stat.AssetIndex,
                    baseValue = stat.BaseValue
                }).ToList();
            
            PlayerSaveData saveData = new PlayerSaveData
            {
                currentExp = CurrentExp,
                level = Level,
                skillPoint = SkillPoint,
                gold = Gold,
                statPoint = StatPoint,
                stats = saveStatData
            };
            return JsonUtility.ToJson(saveData);
        }

        public void RestoreData(string data)
        {
            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(data);
            CurrentExp = saveData.currentExp;
            Level = saveData.level;
            SkillPoint = saveData.skillPoint;
            StatPoint = saveData.statPoint;
            Gold = saveData.gold;
            
            foreach (StatSaveData statData in saveData.stats)
            {
                if (_statModule.TryGetStat(statData.assetIndex, out StatSO stat))
                {
                    stat.BaseValue = statData.baseValue;
                }
            }
        }
        #endregion

        public int CurrentGold => Gold;
        public bool CanSpendGold(int amount)
        {
            if (amount <= 0)
                return true;
            
            return Gold >= amount;
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
                return true;
            
            if (Gold < amount)
                return false;
            
            Gold -= amount;
            return true;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;
            
            Gold += amount;
        }
    }
}