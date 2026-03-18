using System;
using System.Collections.Generic;
using System.Linq;
using Agents.Players.Skills;
using Modules;
using Systems.CoreSystem;
using Systems.Database;
using UnityEngine;

namespace UI.InventorySystem
{
    public class PlayerSkillInventoryModule : MonoBehaviour, IModule, IPlayerSkillInventoryModule
    {
        [SerializeField] private PlayerSkillDataTableSo playerSkillTable;

        public event Action OnInventoryChanged;

        public IReadOnlyList<PlayerSkillInventoryEntry> Entries => _entries;

        private readonly List<PlayerSkillInventoryEntry> _entries = new();
        private readonly Dictionary<PlayerSkill, int> _entryIndexBySkillId = new();
        private readonly Dictionary<int, PlayerSkillDataSo> _skillDataByAssetIndex = new();

        private int _nextAcquireOrder;

        public void Initialize(ModuleOwner owner)
        {
            BuildSkillLookup();
            ClearRuntimeState();
        }

        public int GetSkillCount()
        {
            return _entries.Count;
        }

        public bool Contains(PlayerSkill skillId)
        {
            return _entryIndexBySkillId.ContainsKey(skillId);
        }

        public bool TryGetSkill(PlayerSkill skillId, out PlayerSkillDataSo skillData)
        {
            skillData = null;

            if (!_entryIndexBySkillId.TryGetValue(skillId, out int entryIndex))
                return false;

            PlayerSkillInventoryEntry entry = _entries[entryIndex];
            return TryResolveSkillData(entry.currentSkillAssetIndex, out skillData);
        }

        public bool TryGetSkillAt(int index, out PlayerSkillDataSo skillData)
        {
            skillData = null;

            if (!IsValidIndex(index))
                return false;

            PlayerSkillInventoryEntry entry = _entries[index];
            return TryResolveSkillData(entry.currentSkillAssetIndex, out skillData);
        }

        public bool TryGetEntry(PlayerSkill skillId, out PlayerSkillInventoryEntry entry)
        {
            entry = default;

            if (!_entryIndexBySkillId.TryGetValue(skillId, out int index))
                return false;

            entry = _entries[index];
            return true;
        }

        public bool TryGetEntryAt(int index, out PlayerSkillInventoryEntry entry)
        {
            entry = default;

            if (!IsValidIndex(index))
                return false;

            entry = _entries[index];
            return true;
        }

        public bool TryGetShardCount(PlayerSkill skillId, out int shardCount)
        {
            shardCount = 0;

            if (!_entryIndexBySkillId.TryGetValue(skillId, out int index))
                return false;

            shardCount = _entries[index].shardCount;
            return true;
        }

        public bool TryAddSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            RegisterRuntimeSkillData(skillData);

            if (_entryIndexBySkillId.ContainsKey(skillData.skillId))
                return false;

            PlayerSkillInventoryEntry entry = new PlayerSkillInventoryEntry
            {
                skillId = skillData.skillId,
                currentSkillAssetIndex = skillData.AssetIndex,
                acquiredOrder = _nextAcquireOrder++,
                shardCount = 0
            };

            _entries.Add(entry);
            _entryIndexBySkillId.Add(entry.skillId, _entries.Count - 1);

            NotifyInventoryChanged();
            return true;
        }

        public bool TryAcquireSkill(PlayerSkillDataSo skillData, int duplicateShardAmount = 1)
        {
            if (skillData == null)
                return false;

            if (Contains(skillData.skillId))
                return TryAddShards(skillData.skillId, duplicateShardAmount);

            return TryAddSkill(skillData);
        }

        public bool TryAddShards(PlayerSkill skillId, int amount)
        {
            if (amount <= 0)
                return false;

            if (!_entryIndexBySkillId.TryGetValue(skillId, out int index))
                return false;

            PlayerSkillInventoryEntry entry = _entries[index];
            entry.shardCount += amount;
            _entries[index] = entry;

            NotifyInventoryChanged();
            return true;
        }

        public bool TryConsumeShards(PlayerSkill skillId, int amount)
        {
            if (amount <= 0)
                return false;

            if (!_entryIndexBySkillId.TryGetValue(skillId, out int index))
                return false;

            PlayerSkillInventoryEntry entry = _entries[index];

            if (entry.shardCount < amount)
                return false;

            entry.shardCount -= amount;
            _entries[index] = entry;

            NotifyInventoryChanged();
            return true;
        }

        public bool TrySetCurrentSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return false;

            RegisterRuntimeSkillData(skillData);

            if (!_entryIndexBySkillId.TryGetValue(skillData.skillId, out int index))
                return false;

            PlayerSkillInventoryEntry entry = _entries[index];

            if (entry.currentSkillAssetIndex == skillData.AssetIndex)
                return true;

            entry.currentSkillAssetIndex = skillData.AssetIndex;
            _entries[index] = entry;

            NotifyInventoryChanged();
            return true;
        }

        private void ClearRuntimeState()
        {
            _entries.Clear();
            _entryIndexBySkillId.Clear();
            _nextAcquireOrder = 0;
        }

        private void BuildSkillLookup()
        {
            _skillDataByAssetIndex.Clear();

            if (playerSkillTable == null)
            {
                Debug.LogWarning($"{nameof(PlayerSkillInventoryModule)} : PlayerSkillTable 이 비어 있습니다.");
                return;
            }

            if (playerSkillTable.AssetList == null)
                return;

            foreach (IndexedAsset indexedAsset in playerSkillTable.AssetList)
            {
                if (indexedAsset == null)
                    continue;

                if (indexedAsset is not PlayerSkillDataSo skillData)
                {
                    Debug.LogWarning(
                        $"{nameof(PlayerSkillInventoryModule)} : {indexedAsset.name} 는 PlayerSkillDataSo 가 아닙니다.");
                    continue;
                }

                if (_skillDataByAssetIndex.ContainsKey(skillData.AssetIndex))
                {
                    Debug.LogWarning(
                        $"{nameof(PlayerSkillInventoryModule)} : 중복 AssetIndex({skillData.AssetIndex}) 가 테이블에 있습니다.");
                    continue;
                }

                _skillDataByAssetIndex.Add(skillData.AssetIndex, skillData);
            }
        }

        private void RegisterRuntimeSkillData(PlayerSkillDataSo skillData)
        {
            if (skillData == null)
                return;

            if (_skillDataByAssetIndex.ContainsKey(skillData.AssetIndex))
                return;

            _skillDataByAssetIndex.Add(skillData.AssetIndex, skillData);

            Debug.LogWarning(
                $"{nameof(PlayerSkillInventoryModule)} : {skillData.name} 이(가) PlayerSkillTable 에 등록되지 않았습니다. 런타임에서만 임시 등록됩니다.");
        }

        private void RebuildEntryIndexLookup()
        {
            _entryIndexBySkillId.Clear();

            for (int i = 0; i < _entries.Count; i++)
            {
                _entryIndexBySkillId[_entries[i].skillId] = i;
            }
        }

        private bool TryResolveSkillData(int assetIndex, out PlayerSkillDataSo skillData)
        {
            return _skillDataByAssetIndex.TryGetValue(assetIndex, out skillData);
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _entries.Count;
        }

        private void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }
        
        public IReadOnlyCollection<PlayerSkillDataSo> GetAllKnownSkillData()
        {
            return _skillDataByAssetIndex.Values;
        }
    }
}