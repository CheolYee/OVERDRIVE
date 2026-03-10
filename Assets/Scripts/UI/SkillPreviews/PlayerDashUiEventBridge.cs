using Agents.Players;
using Agents.Players.Skills;
using Systems.GameEvents;
using UnityEngine;

namespace UI.SkillPreviews
{
    public class PlayerDashUiEventBridge : MonoBehaviour
    {
        [SerializeField] private Player player;

        private IPlayerDashLoadoutModule _dashLoadoutModule;
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
                Debug.LogError($"{nameof(PlayerDashUiEventBridge)} : Player reference is null.");
                return;
            }

            _dashLoadoutModule = player.GetModule<IPlayerDashLoadoutModule>();
            Debug.Assert(_dashLoadoutModule != null, $"{gameObject.name} is not attached to player dash loadout module.");

            if (_dashLoadoutModule == null)
                return;

            Subscribe();

            PublishLoadoutChanged();
            PublishPreviewChanged();
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
            if (_dashLoadoutModule == null || _isSubscribed)
                return;

            _dashLoadoutModule.OnDashLoadoutChanged += HandleDashLoadoutChanged;
            _dashLoadoutModule.OnDashPreviewChanged += HandleDashPreviewChanged;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_dashLoadoutModule == null || !_isSubscribed)
                return;

            _dashLoadoutModule.OnDashLoadoutChanged -= HandleDashLoadoutChanged;
            _dashLoadoutModule.OnDashPreviewChanged -= HandleDashPreviewChanged;
            _isSubscribed = false;
        }

        private void HandleDashLoadoutChanged()
        {
            PublishLoadoutChanged();
        }

        private void HandleDashPreviewChanged()
        {
            PublishPreviewChanged();
        }

        private void PublishLoadoutChanged()
        {
            if (player == null || player.PlayerEventChannel == null || _dashLoadoutModule == null)
                return;

            int slotCount = _dashLoadoutModule.GetDashSlotCount();
            DashSkillSlot[] slotsSnapshot = new DashSkillSlot[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                slotsSnapshot[i] = new DashSkillSlot
                {
                    isUnlocked = _dashLoadoutModule.IsDashSlotUnlocked(i),
                    equippedSkill = _dashLoadoutModule.GetEquippedDashSkill(i)
                };
            }

            player.PlayerEventChannel.RaiseEvent(
                DashEvents.PlayerDashLoadoutChanged.Init(slotsSnapshot));
        }

        private void PublishPreviewChanged()
        {
            if (player == null || player.PlayerEventChannel == null || _dashLoadoutModule == null)
                return;

            _dashLoadoutModule.TryGetDashPreview(out PlayerSkillDataSo currentSkill, out PlayerSkillDataSo nextSkill);

            player.PlayerEventChannel.RaiseEvent(
                DashEvents.PlayerDashPreviewChanged.Init(currentSkill, nextSkill));
        }
    }
}