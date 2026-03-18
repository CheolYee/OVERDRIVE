using Agents.Players;
using Agents.Players.Skills;
using CombatSystem;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UI.InventorySystem;
using UnityEngine;

namespace UI.SkillReward
{
    public class ChestSkillRewardPresenter : MonoBehaviour, IHandlePlayerDataSetUp
    {
        [SerializeField] private ChestSkillRewardModel model;
        [SerializeField] private ChestSkillRewardPopupView view;
        [SerializeField] private RewardCanvasUI rewardCanvasUI;
        [SerializeField] private EventChannelSO playerChannel;

        private Player _player;
        private IPlayerSkillInventoryModule _inventoryModule;
        private IPlayerDashLoadoutModule _dashLoadoutModule;
        private IPlayerSkillModule _playerSkillModule;

        private bool _isResolvingReward;

        private void Awake()
        {
            playerChannel.AddListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);

            Debug.Assert(model != null, $"[{nameof(ChestSkillRewardPresenter)}] : model is null.");
            Debug.Assert(view != null, $"[{nameof(ChestSkillRewardPresenter)}] : view is null.");
            Debug.Assert(rewardCanvasUI != null, $"[{nameof(ChestSkillRewardPresenter)}] : rewardCanvasUI is null.");
        }

        private void OnDestroy()
        {
            if (playerChannel != null)
            {
                playerChannel.RemoveListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
            }
        }

        public void HandlePlayerDataSetUp(PlayerDataSetUpEvent evt)
        {
            _player = evt.PlayerData.Player;
            Debug.Assert(_player != null, $"[{nameof(ChestSkillRewardPresenter)}] : player is null.");

            _playerSkillModule = _player.GetModule<IPlayerSkillModule>();
            _dashLoadoutModule = _player.GetModule<IPlayerDashLoadoutModule>();
            _inventoryModule = _player.GetModule<IPlayerSkillInventoryModule>();

            Debug.Assert(_inventoryModule != null, $"[{nameof(ChestSkillRewardPresenter)}] : inventoryModule is null.");
            Debug.Assert(_dashLoadoutModule != null, $"[{nameof(ChestSkillRewardPresenter)}] : dashLoadoutModule is null.");
            Debug.Assert(_playerSkillModule != null, $"[{nameof(ChestSkillRewardPresenter)}] : playerSkillModule is null.");
        }

        private void OnEnable()
        {
            model.OnPendingRequestReceived += HandlePendingRequestReceived;
            view.OnCardAcquireRequested += HandleCardAcquireRequested;
        }

        private void OnDisable()
        {
            model.OnPendingRequestReceived -= HandlePendingRequestReceived;
            view.OnCardAcquireRequested -= HandleCardAcquireRequested;
        }

        private void HandlePendingRequestReceived()
        {
            if (!model.HasPendingRequest)
                return;

            view.BindCards(model.PendingCards);

            rewardCanvasUI.OpenOverlay(() =>
            {
                view.PlayOpenSequence();
            });

            model.ClearPendingRequest();
        }

        private void HandleCardAcquireRequested(RewardCardViewData cardData)
        {
            if (_isResolvingReward || cardData == null)
                return;

            if (cardData.CardType != RewardCardType.Skill || cardData.SkillData == null)
                return;

            if (!TryApplySkillReward(cardData.SkillData))
                return;

            _isResolvingReward = true;

            view.PlayCloseSequence();
            rewardCanvasUI.CloseOverlay(() =>
            {
                _isResolvingReward = false;
            });
        }

        private bool TryApplySkillReward(PlayerSkillDataSo rewardSkillData)
        {
            if (rewardSkillData == null)
                return false;

            if (_inventoryModule == null || _playerSkillModule == null || _dashLoadoutModule == null)
                return false;

            return _inventoryModule.Contains(rewardSkillData.skillId)
                ? TryUpgradeOwnedSkill(rewardSkillData)
                : TryAcquireNewSkill(rewardSkillData);
        }

        private bool TryAcquireNewSkill(PlayerSkillDataSo rewardSkillData)
        {
            if (!_inventoryModule.TryAddSkill(rewardSkillData))
                return false;

            return _playerSkillModule.EnsureSkillRegistered(rewardSkillData, rewardSkillData.defaultKey);
        }

        private bool TryUpgradeOwnedSkill(PlayerSkillDataSo rewardSkillData)
        {
            if (!_inventoryModule.TryUpgradeSkill(rewardSkillData, out PlayerSkillDataSo previousSkillData))
                return false;

            if (!_playerSkillModule.ReplaceOwnedSkill(previousSkillData, rewardSkillData))
            {
                _inventoryModule.TrySetCurrentSkill(previousSkillData);
                return false;
            }

            _dashLoadoutModule.ReplaceEquippedSkill(rewardSkillData.skillId, rewardSkillData);
            return true;
        }
    }
}