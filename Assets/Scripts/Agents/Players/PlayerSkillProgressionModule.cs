using System.Collections.Generic;
using Agents.Players.Skills;
using Modules;
using Systems.Database;
using UI.InventorySystem;
using UnityEngine;

namespace Agents.Players
{
    public class PlayerSkillProgressionModule : MonoBehaviour, IModule, IPlayerSkillProgressionModule
    {
        [SerializeField] private PlayerSkillUpgradePathSo[] upgradePaths;

        private IPlayerSkillInventoryModule _inventoryModule;
        private IPlayerSkillRuntimeRegistry _runtimeRegistry;

        private readonly Dictionary<PlayerSkill, PlayerSkillUpgradePathSo> _upgradePathBySkillId = new();

        public void Initialize(ModuleOwner owner)
        {
            _inventoryModule = owner.GetModule<IPlayerSkillInventoryModule>();
            _runtimeRegistry = owner.GetModule<IPlayerSkillRuntimeRegistry>();

            Debug.Assert(_inventoryModule != null, $"{nameof(PlayerSkillProgressionModule)} : InventoryModule 이 필요합니다.");

            BuildUpgradePathLookup();
        }

        public bool TryAcquireSkill(PlayerSkillDataSo acquiredSkillData)
        {
            if (acquiredSkillData == null || _inventoryModule == null)
                return false;

            PlayerSkill skillId = acquiredSkillData.skillId;

            if (_inventoryModule.Contains(skillId))
            {
                int shardReward = ResolveDuplicateShardReward(skillId);
                return _inventoryModule.TryAddShards(skillId, shardReward);
            }

            PlayerSkillDataSo entrySkillData = ResolveEntrySkillData(acquiredSkillData);

            if (!_inventoryModule.TryAddSkill(entrySkillData))
                return false;

            _runtimeRegistry?.TryRegisterSkill(entrySkillData);
            return true;
        }

        public bool TryGetUpgradePath(PlayerSkill skillId, out PlayerSkillUpgradePathSo upgradePath)
        {
            return _upgradePathBySkillId.TryGetValue(skillId, out upgradePath);
        }

        private void BuildUpgradePathLookup()
        {
            _upgradePathBySkillId.Clear();

            if (upgradePaths == null)
                return;

            foreach (PlayerSkillUpgradePathSo upgradePath in upgradePaths)
            {
                if (upgradePath == null)
                    continue;

                if (_upgradePathBySkillId.ContainsKey(upgradePath.SkillId))
                {
                    Debug.LogWarning($"{nameof(PlayerSkillProgressionModule)} : 중복 skillId({upgradePath.SkillId}) upgrade path가 있습니다.");
                    continue;
                }

                _upgradePathBySkillId.Add(upgradePath.SkillId, upgradePath);
            }
        }

        private PlayerSkillDataSo ResolveEntrySkillData(PlayerSkillDataSo fallbackSkillData)
        {
            if (fallbackSkillData == null)
                return null;

            if (_upgradePathBySkillId.TryGetValue(fallbackSkillData.skillId, out PlayerSkillUpgradePathSo upgradePath))
            {
                PlayerSkillDataSo entrySkill = upgradePath.GetEntrySkill();
                if (entrySkill != null)
                    return entrySkill;
            }

            return fallbackSkillData;
        }

        private int ResolveDuplicateShardReward(PlayerSkill skillId)
        {
            if (_upgradePathBySkillId.TryGetValue(skillId, out PlayerSkillUpgradePathSo upgradePath))
            {
                return Mathf.Max(1, upgradePath.DuplicateShardReward);
            }

            return 1;
        }
    }
}