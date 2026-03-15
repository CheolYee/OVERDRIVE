using Agents.Players;
using Gamelib.EventSystem;
using Systems.GameEvents;
using TMPro;
using UnityEngine;

namespace UI.Currency
{
    public abstract class PlayerCurrencyTextViewBase : MonoBehaviour, IPlayerCurrencyView
    {
        [SerializeField] private EventChannelSO playerChannel;
        [SerializeField] private TextMeshProUGUI amountText;

        protected PlayerData PlayerData { get; private set; }

        protected virtual void Awake()
        {
            if (amountText == null)
                amountText = GetComponent<TextMeshProUGUI>();
        }

        protected virtual void OnEnable()
        {
            playerChannel?.AddListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
        }

        protected virtual void OnDisable()
        {
            playerChannel?.RemoveListener<PlayerDataSetUpEvent>(HandlePlayerDataSetUp);
            ClearPlayerData();
        }

        private void HandlePlayerDataSetUp(PlayerDataSetUpEvent evt)
        {
            SetPlayerData(evt.PlayerData);
        }

        public void SetPlayerData(PlayerData playerData)
        {
            if (PlayerData == playerData)
                return;

            Unbind(PlayerData);
            PlayerData = playerData;
            Bind(PlayerData);
            Refresh();
        }

        public void ClearPlayerData()
        {
            Unbind(PlayerData);
            PlayerData = null;
        }

        protected void SetAmountText(int amount)
        {
            if (amountText != null)
                amountText.text = amount.ToString();
        }

        protected abstract void Bind(PlayerData playerData);
        protected abstract void Unbind(PlayerData playerData);
        protected abstract void Refresh();
    }
}