using Agents.Players;
using Agents.Players.Skills;
using Systems.GameEvents;
using UnityEngine;

namespace UI.InventorySystem
{
    public class PlayerSkillInventoryUiEventBridge : MonoBehaviour
    {
        [SerializeField] private Player player;

        private IPlayerSkillInventoryModule _inventoryModule;
        private bool _isSubscribed;

        private void Awake()
        {
            if (player == null)
                player = GetComponent<Player>();

            if (player == null)
                player = GetComponentInParent<Player>();
        }

        private void Start()
        {
            if (player == null)
            {
                Debug.LogError($"{nameof(PlayerSkillInventoryUiEventBridge)} : Player reference is null.");
                return;
            }

            _inventoryModule = player.GetModule<IPlayerSkillInventoryModule>();
            Debug.Assert(_inventoryModule != null, $"{gameObject.name} is not attached to player skill inventory module.");

            if (_inventoryModule == null)
                return;

            Subscribe();
            PublishInventoryChanged();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_inventoryModule == null || _isSubscribed)
                return;

            _inventoryModule.OnInventoryChanged += HandleInventoryChanged;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_inventoryModule == null || !_isSubscribed)
                return;

            _inventoryModule.OnInventoryChanged -= HandleInventoryChanged;
            _isSubscribed = false;
        }

        private void HandleInventoryChanged()
        {
            PublishInventoryChanged();
        }

        private void PublishInventoryChanged()
        {
            if (player == null || player.PlayerEventChannel == null || _inventoryModule == null)
                return;

            int skillCount = _inventoryModule.GetSkillCount();
            PlayerSkillDataSo[] skillsSnapshot = new PlayerSkillDataSo[skillCount];

            for (int i = 0; i < skillCount; i++)
            {
                if (_inventoryModule.TryGetSkillAt(i, out PlayerSkillDataSo skillData))
                    skillsSnapshot[i] = skillData;
            }

            player.PlayerEventChannel.RaiseEvent(
                InventoryEvents.PlayerSkillInventoryChanged.Init(skillsSnapshot));
        }
    }
}