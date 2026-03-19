using Agents.Players;
using Agents.Players.Skills;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace UI.SkillPreviews
{
    public class PlayerDashUiEventBridge : MonoBehaviour, IHandlePlayerDataSetUp
    {
        [SerializeField] private EventChannelSO playerEventChannel;

        private IPlayerDashLoadoutModule _dashLoadoutModule;
        private Player _player;
        private bool _isSubscribed;

        private void Awake()
        {
            playerEventChannel.AddListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
        }

        public void HandlePlayerDataSetUp(PlayerDataSetUpEvent evt)
        {
            _player = evt.PlayerData.Player;
            _dashLoadoutModule = _player.GetModule<IPlayerDashLoadoutModule>();
            Debug.Assert(_player != null, $"[PlayerDashUiEventBridge] : 플레이어가 없습니다.");
            Debug.Assert(_dashLoadoutModule != null, $"[PlayerDashUiEventBridge] : 로드아웃 대쉬 모듈이 없습니다.");
            
            Subscribe();
            PublishLoadoutChanged();
            PublishPreviewChanged();
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
            if (_player == null || _player.PlayerEventChannel == null || _dashLoadoutModule == null)
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

            _player.PlayerEventChannel.RaiseEvent(
                DashEvents.PlayerDashLoadoutChanged.Init(slotsSnapshot));
        }

        private void PublishPreviewChanged()
        {
            if (_player == null || _player.PlayerEventChannel == null || _dashLoadoutModule == null)
                return;

            _dashLoadoutModule.TryGetDashPreview(out PlayerSkillDataSo currentSkill, out PlayerSkillDataSo nextSkill);

            _player.PlayerEventChannel.RaiseEvent(
                DashEvents.PlayerDashPreviewChanged.Init(currentSkill, nextSkill));
        }
    }
}