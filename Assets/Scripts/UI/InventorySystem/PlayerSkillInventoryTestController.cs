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

        private IPlayerSkillInventoryModule _inventoryModule;
        private IPlayerSkillProgressionModule _progressionModule;

        private void Start()
        {
            if (player == null)
                player = GetComponent<Player>();

            if (player == null)
                player = GetComponentInParent<Player>();

            if (player != null)
            {
                _inventoryModule = player.GetModule<IPlayerSkillInventoryModule>();
                _progressionModule = player.GetModule<IPlayerSkillProgressionModule>();
            }
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

        [ContextMenu("Acquire Skill A Again")]
        public void AcquireSkillAAgain()
        {
            _progressionModule?.TryAcquireSkill(testSkillA);
        }

        [ContextMenu("Log Skill A Info")]
        public void LogSkillAInfo()
        {
            if (testSkillA == null || _inventoryModule == null)
                return;

            if (_inventoryModule.TryGetEntry(testSkillA.skillId, out PlayerSkillInventoryEntry entry))
            {
                Debug.Log($"Skill: {entry.skillId}, AssetIndex: {entry.currentSkillAssetIndex}, Shards: {entry.shardCount}, Order: {entry.acquiredOrder}");
            }
            else
            {
                Debug.Log("Skill A is not in inventory.");
            }
        }
    }
}