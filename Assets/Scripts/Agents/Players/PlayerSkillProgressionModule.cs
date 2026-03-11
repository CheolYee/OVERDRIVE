using System;
using System.Collections.Generic;
using Agents.Players;
using Modules;
using Systems.Database;
using UI.InventorySystem;
using UnityEngine;

namespace Agents.Players.Skills
{
    public struct PlayerSkillUpgradeInfo
    {
        public PlayerSkillDataSo currentSkill;
        public PlayerSkillDataSo nextSkill;
        public int currentLevel;
        public int maxLevel;
        public int shardCount;
        public int requiredShards;
        public int requiredGold;
        public bool isMaxLevel;
        public bool canUpgrade;
    }

    public class PlayerSkillProgressionModule : MonoBehaviour, IModule
    {
        [SerializeField] private PlayerSkillUpgradePathSo[] upgradePaths;

        public event Action<PlayerSkill> OnSkillAcquired;
        public event Action<PlayerSkill> OnSkillUpgraded;

        private readonly Dictionary<PlayerSkill, PlayerSkillUpgradePathSo> _pathBySkillId = new();

        private Player _player;
        private PlayerData _playerData;
        private IPlayerSkillInventoryModule _skillInventory;
        private IPlayerDashLoadoutModule _dashLoadout;
        private PlayerSkillModule _skillModule;

        public void Initialize(ModuleOwner owner)
        {
            _player = owner as Player;
            Debug.Assert(_player != null, $"{nameof(PlayerSkillProgressionModule)} : owner is not Player.");

            _playerData = _player.GetComponent<PlayerData>();
            _skillModule = _player.GetComponent<PlayerSkillModule>();
            _skillInventory = _player.GetModule<IPlayerSkillInventoryModule>();
            _dashLoadout = _player.GetModule<IPlayerDashLoadoutModule>();

            Debug.Assert(_playerData != null, $"{nameof(PlayerSkillProgressionModule)} : PlayerData is null.");
            Debug.Assert(_skillModule != null, $"{nameof(PlayerSkillProgressionModule)} : PlayerSkillModule is null.");
            Debug.Assert(_skillInventory != null, $"{nameof(PlayerSkillProgressionModule)} : SkillInventory is null.");
            Debug.Assert(_dashLoadout != null, $"{nameof(PlayerSkillProgressionModule)} : DashLoadout is null.");

            BuildUpgradePathLookup();
        }

        public bool TryAcquireSkill(PlayerSkillDataSo skillData)
        {
            if (skillData == null || _skillInventory == null)
                return false;

            bool alreadyOwned = _skillInventory.Contains(skillData.skillId);
            int duplicateShardReward = GetDuplicateShardReward(skillData.skillId);

            bool result = _skillInventory.TryAcquireSkill(skillData, duplicateShardReward);
            if (!result)
                return false;

            if (!alreadyOwned)
            {
                _skillModule.EnsureSkillRegistered(skillData, skillData.defaultKey);
                OnSkillAcquired?.Invoke(skillData.skillId);
            }

            return true;
        }

        public bool TryGetUpgradeInfo(PlayerSkill skillId, out PlayerSkillUpgradeInfo info)
        {
            info = default;

            if (_skillInventory == null)
                return false;

            if (!_skillInventory.TryGetSkill(skillId, out PlayerSkillDataSo currentSkill))
                return false;

            if (!_skillInventory.TryGetShardCount(skillId, out int shardCount))
                shardCount = 0;

            if (!TryGetUpgradePath(skillId, out PlayerSkillUpgradePathSo path))
                return false;

            info.currentSkill = currentSkill;
            info.currentLevel = currentSkill != null ? currentSkill.level : 0;
            info.maxLevel = path.MaxLevel;
            info.shardCount = shardCount;

            if (!path.TryGetNextTier(currentSkill, out PlayerSkillUpgradeTier currentTier, out PlayerSkillUpgradeTier nextTier))
            {
                info.nextSkill = null;
                info.requiredShards = 0;
                info.requiredGold = 0;
                info.isMaxLevel = true;
                info.canUpgrade = false;
                return true;
            }

            info.nextSkill = nextTier.skillData;
            info.requiredShards = currentTier.requiredShardsToNext;
            info.requiredGold = currentTier.requiredGoldToNext;
            info.isMaxLevel = false;
            info.canUpgrade = shardCount >= info.requiredShards && _playerData != null && _playerData.Gold >= info.requiredGold;

            return true;
        }

        public bool TryUpgradeSkill(PlayerSkill skillId)
        {
            if (_playerData == null || _skillInventory == null || _dashLoadout == null || _skillModule == null)
                return false;

            if (!_skillInventory.TryGetSkill(skillId, out PlayerSkillDataSo currentSkill))
                return false;

            if (!TryGetUpgradePath(skillId, out PlayerSkillUpgradePathSo path))
                return false;

            if (!path.TryGetNextTier(currentSkill, out PlayerSkillUpgradeTier currentTier, out PlayerSkillUpgradeTier nextTier))
                return false;

            int requiredShards = currentTier.requiredShardsToNext;
            int requiredGold = currentTier.requiredGoldToNext;

            if (!_skillInventory.TryGetShardCount(skillId, out int currentShards))
                return false;

            if (currentShards < requiredShards)
                return false;

            if (_playerData.Gold < requiredGold)
                return false;

            if (!_playerData.TrySpendGold(requiredGold))
                return false;

            if (!_skillInventory.TryConsumeShards(skillId, requiredShards))
            {
                _playerData.AddGold(requiredGold);
                return false;
            }

            if (!_skillInventory.TrySetCurrentSkill(nextTier.skillData))
            {
                _playerData.AddGold(requiredGold);
                _skillInventory.TryAddShards(skillId, requiredShards);
                return false;
            }

            _dashLoadout.ReplaceEquippedSkill(skillId, nextTier.skillData);
            _skillModule.ReplaceOwnedSkill(currentSkill, nextTier.skillData);

            OnSkillUpgraded?.Invoke(skillId);
            return true;
        }

        private void BuildUpgradePathLookup()
        {
            _pathBySkillId.Clear();

            if (upgradePaths == null)
                return;

            foreach (PlayerSkillUpgradePathSo path in upgradePaths)
            {
                if (path == null)
                    continue;

                if (_pathBySkillId.ContainsKey(path.skillId))
                {
                    Debug.LogWarning($"{nameof(PlayerSkillProgressionModule)} : duplicated upgrade path for {path.skillId}");
                    continue;
                }

                _pathBySkillId.Add(path.skillId, path);
            }
        }

        private bool TryGetUpgradePath(PlayerSkill skillId, out PlayerSkillUpgradePathSo path)
        {
            return _pathBySkillId.TryGetValue(skillId, out path);
        }

        private int GetDuplicateShardReward(PlayerSkill skillId)
        {
            if (TryGetUpgradePath(skillId, out PlayerSkillUpgradePathSo path))
                return Mathf.Max(1, path.duplicateShardReward);

            return 1;
        }
    }
}