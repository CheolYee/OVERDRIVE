using System;
using System.Collections.Generic;
using System.Linq;
using Agents.Players.Skills;
using Modules;
using Systems.CoreSystem;
using Systems.Database;
using UnityEngine;

namespace Agents.Players.UI.InventorySystem
{
    public class PlayerSkillInventoryModule : MonoBehaviour, IModule, ISaveable, IPlayerSkillInventoryModule
    {
        [field: SerializeField] public SaveIdData SaveId { get; private set; } //세이브 아이디를 통해 저장할 데이터를 구분한다.
        [SerializeField] private PlayerSkillDataTableSo playerSkillTable; //스킬들

        public event Action OnInventoryChanged;

        public IReadOnlyList<PlayerSkillInventoryEntry> Entries => _entries;

        private readonly List<PlayerSkillInventoryEntry> _entries = new(); //인벤토리에 담긴 스킬들
        private readonly Dictionary<PlayerSkill, int> _entryIndexBySkillId = new(); //스킬 이넘으로 인벤토리 엔트리 인덱스 찾기
        private readonly Dictionary<int, PlayerSkillDataSo> _skillDataByAssetIndex = new(); //에셋 인덱스로 스킬 찾기

        private int _nextAcquireOrder; //스킬 획득 순서를 기록하기 위한 카운터. 인벤토리에 담긴 스킬들은 이 획득 순서에 따라 정렬된다.

        public void Initialize(ModuleOwner owner)
        {
            BuildSkillLookup();
            ClearRuntimeState();
        }

        public int GetSkillCount() //인벤토리에 담긴 스킬 수 반환
        {
            return _entries.Count;
        }

        public bool Contains(PlayerSkill skillId) //인벤토리에 해당 스킬이 있는지
        {
            return _entryIndexBySkillId.ContainsKey(skillId);
        }

        //인벤토리에 해당 스킬이 있는지, 있다면 스킬 데이터 반환
        public bool TryGetSkill(PlayerSkill skillId, out PlayerSkillDataSo skillData)
        {
            skillData = null;

            if (!_entryIndexBySkillId.TryGetValue(skillId, out int entryIndex))
                return false;

            PlayerSkillInventoryEntry entry = _entries[entryIndex];
            return TryResolveSkillData(entry.currentSkillAssetIndex, out skillData);
        }

        //인벤토리에서 해당 인덱스의 스킬이 있는지, 있다면 스킬 데이터 반환
        public bool TryGetSkillAt(int index, out PlayerSkillDataSo skillData)
        {
            skillData = null;

            if (!IsValidIndex(index))
                return false;

            PlayerSkillInventoryEntry entry = _entries[index];
            return TryResolveSkillData(entry.currentSkillAssetIndex, out skillData);
        }

        //인벤토리에서 해당 인덱스의 엔트리가 있는지, 있다면 엔트리 반환
        public bool TryGetEntryAt(int index, out PlayerSkillInventoryEntry entry)
        {
            entry = default;

            if (!IsValidIndex(index))
                return false;

            entry = _entries[index];
            return true;
        }

        //인벤토리에 스킬 추가 시도. 이미 같은 스킬이 있거나, skillData가 null 이면 false 반환. 성공적으로 추가하면 true 반환
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
                acquiredOrder = _nextAcquireOrder++
            };

            _entries.Add(entry);
            _entryIndexBySkillId.Add(entry.skillId, _entries.Count - 1);

            NotifyInventoryChanged();
            return true;
        }

        //인벤토리에서 해당 스킬 제거 시도. 인벤토리에 해당 스킬이 없거나, skillId가 null 이면 false 반환. 성공적으로 제거하면 true 반환
        public string GetSaveData()
        {
            PlayerSkillInventorySaveData saveData = new PlayerSkillInventorySaveData
            {
                nextAcquireOrder = _nextAcquireOrder,
                entries = new List<PlayerSkillInventoryEntrySaveData>(_entries.Count)
            };

            foreach (PlayerSkillInventoryEntry entry in _entries)
            {
                saveData.entries.Add(new PlayerSkillInventoryEntrySaveData
                {
                    skillAssetIndex = entry.currentSkillAssetIndex,
                    acquiredOrder = entry.acquiredOrder
                });
            }

            return JsonUtility.ToJson(saveData);
        }

        //인벤토리에 해당 스킬 제거 시도. 인벤토리에 해당 스킬이 없거나, skillId가 null 이면 false 반환. 성공적으로 제거하면 true 반환
        public void RestoreData(string data)
        {
            ClearRuntimeState();
            BuildSkillLookup();

            if (string.IsNullOrEmpty(data))
            {
                NotifyInventoryChanged();
                return;
            }

            PlayerSkillInventorySaveData saveData = JsonUtility.FromJson<PlayerSkillInventorySaveData>(data);

            if (saveData.entries != null)
            {
                foreach (PlayerSkillInventoryEntrySaveData entrySaveData in saveData.entries)
                {
                    if (!TryResolveSkillData(entrySaveData.skillAssetIndex, out PlayerSkillDataSo skillData))
                    {
                        Debug.LogWarning(
                            $"{nameof(PlayerSkillInventoryModule)} : AssetIndex {entrySaveData.skillAssetIndex} 에 해당하는 스킬 데이터를 찾을 수 없습니다.");
                        continue;
                    }

                    if (_entryIndexBySkillId.ContainsKey(skillData.skillId))
                    {
                        Debug.LogWarning(
                            $"{nameof(PlayerSkillInventoryModule)} : 중복 skillId({skillData.skillId}) 가 저장 데이터에 있습니다. 뒤 항목은 무시합니다.");
                        continue;
                    }

                    PlayerSkillInventoryEntry entry = new PlayerSkillInventoryEntry
                    {
                        skillId = skillData.skillId,
                        currentSkillAssetIndex = skillData.AssetIndex,
                        acquiredOrder = entrySaveData.acquiredOrder
                    };

                    _entries.Add(entry);
                }
            }

            _entries.Sort((left, right) => left.acquiredOrder.CompareTo(right.acquiredOrder));
            RebuildEntryIndexLookup();

            int nextOrderFromEntries = _entries.Count == 0 ? 0 : _entries.Max(entry => entry.acquiredOrder) + 1;
            _nextAcquireOrder = Mathf.Max(saveData.nextAcquireOrder, nextOrderFromEntries);

            NotifyInventoryChanged();
        }
        
        private void ClearRuntimeState() //런타임에만 존재하는 상태 초기화. 스킬 테이블이 변경된 경우나, 세이브 데이터를 복원하기 전에 이 메서드를 호출해서 기존 상태를 초기화해야 한다.
        {
            _entries.Clear();
            _entryIndexBySkillId.Clear();
            _nextAcquireOrder = 0;
        }

        private void BuildSkillLookup() //스킬 테이블의 데이터를 기반으로 에셋 인덱스로 스킬 데이터를 찾는 룩업 테이블 구축. 런타임에 스킬 데이터를 빠르게 찾을 수 있게 해준다.
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

        private bool IsValidIndex(int index) //인벤토리 엔트리 인덱스가 유효한지 검사
        {
            return index >= 0 && index < _entries.Count;
        }

        private void NotifyInventoryChanged() //인벤토리에 변경이 생겼음을 구독자들에게 알린다.
        {
            OnInventoryChanged?.Invoke();
        }
    }
}