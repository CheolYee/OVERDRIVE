using System;
using System.Collections.Generic;
using Agents.Players;
using Agents.Players.Skills;
using Alchemy.Inspector;
using Alchemy.Serialization;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.InventorySystem
{
    [AlchemySerialize]
    public partial class PlayerSkillInventoryTestController : MonoBehaviour, IHandlePlayerDataSetUp
    {
        [SerializeField] private EventChannelSO playerEventChannel;
        [SerializeField] private PlayerSkillDataSo testSkillA;
        [SerializeField] private PlayerSkillDataSo testSkillB;
        [SerializeField] private int shardAmount = 1;
        [SerializeField] private int testGold = 999;
        
        [AlchemySerializeField, NonSerialized]
        public HashSet<GameObject> hashset = new();
        
        private Player _player;
        private IPlayerSkillInventoryModule _inventoryModule;
        private PlayerData _playerData;

        private void Awake()
        {
            playerEventChannel?.AddListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
        }
        
        private void OnDestroy()
        {
            playerEventChannel?.RemoveListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
        }
        
        public void HandlePlayerDataSetUp(PlayerDataSetUpEvent evt)
        {
            _playerData = evt.PlayerData;
            _player = evt.PlayerData.Player;
            _inventoryModule = _player.GetModule<IPlayerSkillInventoryModule>();
            
            Debug.Assert(_playerData != null, "[PlayerSkillInventoryTestController] : 플레이어 데이터가 없습니다.");
            Debug.Assert(_player != null, "[PlayerSkillInventoryTestController] : 플레이어가 없습니다.");
            Debug.Assert(_inventoryModule != null, "[PlayerSkillInventoryTestController] : 플레이어 스킬 인벤토리 모듈이 없습니다.");
        }

        [Button]
        public void AcquireSkillA()
        {
            _inventoryModule?.TryAcquireSkill(testSkillA);
        }

        [Button]
        public void AcquireSkillB()
        {
            _inventoryModule?.TryAcquireSkill(testSkillB);
        }

        [ContextMenu("Add Shards To Skill A")]
        public void AddShardsToSkillA()
        {
            if (testSkillA == null || _inventoryModule == null)
                return;

            _inventoryModule.TryAddShards(testSkillA.skillId, shardAmount);
        }

        [ContextMenu("Set Test Gold")]
        public void SetTestGold()
        {
            _playerData?.SetGold(testGold);
        }

        [ContextMenu("Log Skill A Entry")]
        public void LogSkillAEntry()
        {
            if (testSkillA == null || _inventoryModule == null)
                return;

            if (_inventoryModule.TryGetEntry(testSkillA.skillId, out PlayerSkillInventoryEntry entry))
            {
                Debug.Log($"Skill={entry.skillId}, AssetIndex={entry.currentSkillAssetIndex}, Shards={entry.shardCount}, Order={entry.acquiredOrder}");
            }
            else
            {
                Debug.Log("Skill A is not owned.");
            }
        }
    }
}