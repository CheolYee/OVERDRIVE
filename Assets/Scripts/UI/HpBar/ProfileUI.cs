using Agents.Players;
using CombatSystem;
using Gamelib.EventSystem;
using Systems.GameEvents;
using TMPro;
using UnityEngine;

namespace UI.HpBar
{
    public class ProfileUI : MonoBehaviour
    {
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }
        [SerializeField] private FillBarUI healthBar;

        private PlayerData _playerData;
        private HealthModule _playerHealthModule;
        private void Awake()
        {
            PlayerChannel.AddListener<PlayerDataSetUpEvent>(HandleSetUpPlayer);
        }

        private void OnDestroy()
        {
            PlayerChannel.RemoveListener<PlayerDataSetUpEvent>(HandleSetUpPlayer);
            
            if(_playerHealthModule != null)
                _playerHealthModule.OnHealthChange -= HandlePlayerHealthChange;
        }

       
        private void HandleSetUpPlayer(PlayerDataSetUpEvent evt)
        {
            _playerData = evt.PlayerData;
            _playerHealthModule = _playerData.Player.GetModule<HealthModule>();

            _playerHealthModule.OnHealthChange += HandlePlayerHealthChange;
            
            HandlePlayerHealthChange(0, _playerHealthModule.CurrentHealth, _playerHealthModule.MaxHealth);
        }

        private void HandlePlayerHealthChange(float before, float health, float max)
        {
            healthBar.SetFill(health, max);
        }
    }
}
