using Agents.Players;
using Agents.Players.Skills;
using UnityEngine;

namespace UI.InventorySystem
{
    public class PlayerSkillInventoryTestController : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private PlayerSkillDataSo testSkillA;
        [SerializeField] private PlayerSkillDataSo testSkillB;
        [SerializeField] private int shardAmount = 1;
        [SerializeField] private int testGold = 999;

        private IPlayerSkillInventoryModule _inventoryModule;
        private PlayerSkillProgressionModule _progressionModule;
        private PlayerData _playerData;

        private void Start()
        {
            if (player == null)
                player = GetComponent<Player>();

            if (player == null)
                player = GetComponentInParent<Player>();

            if (player == null)
                return;

            _inventoryModule = player.GetModule<IPlayerSkillInventoryModule>();
            _progressionModule = player.GetComponent<PlayerSkillProgressionModule>();
            _playerData = player.GetComponent<PlayerData>();
        }

        [ContextMenu("Acquire Skill A")]
        public void AcquireSkillA()
        {
            _progressionModule?.TryAcquireSkill(testSkillA);
        }

        [ContextMenu("Acquire Skill B")]
        public void AcquireSkillB()
        {
            _progressionModule?.TryAcquireSkill(testSkillB);
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

        [ContextMenu("Upgrade Skill A")]
        public void UpgradeSkillA()
        {
            if (testSkillA == null || _progressionModule == null)
                return;

            bool result = _progressionModule.TryUpgradeSkill(testSkillA.skillId);
            Debug.Log($"Upgrade Skill A Result = {result}");
        }

        [ContextMenu("Log Upgrade Info A")]
        public void LogUpgradeInfoA()
        {
            if (testSkillA == null || _progressionModule == null)
                return;

            if (_progressionModule.TryGetUpgradeInfo(testSkillA.skillId, out PlayerSkillUpgradeInfo info))
            {
                Debug.Log(
                    $"Current={info.currentSkill?.name}, Next={info.nextSkill?.name}, " +
                    $"Level={info.currentLevel}/{info.maxLevel}, " +
                    $"Shards={info.shardCount}, RequiredShards={info.requiredShards}, RequiredGold={info.requiredGold}, " +
                    $"CanUpgrade={info.canUpgrade}, IsMax={info.isMaxLevel}");
            }
            else
            {
                Debug.Log("Upgrade info unavailable.");
            }
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