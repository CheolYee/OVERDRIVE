using Agents.Players;

namespace UI.Currency
{
    public class GoldTextView : PlayerCurrencyTextViewBase
    {
        protected override void Bind(PlayerData playerData)
        {
            if (playerData == null) return;
            playerData.OnGoldChanged += HandleGoldChanged;
        }

        protected override void Unbind(PlayerData playerData)
        {
            if (playerData == null) return;
            playerData.OnGoldChanged -= HandleGoldChanged;
        }

        protected override void Refresh()
        {
            if (PlayerData == null) return;
            SetAmountText(PlayerData.Gold);
        }

        private void HandleGoldChanged(int amount)
        {
            SetAmountText(amount);
        }
    }
}