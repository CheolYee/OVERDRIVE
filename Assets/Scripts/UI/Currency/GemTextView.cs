using Agents.Players;

namespace UI.Currency
{
    public class GemTextView : PlayerCurrencyTextViewBase
    {
        protected override void Bind(PlayerData playerData)
        {
            if (playerData == null) return;
            playerData.OnGemChanged += HandleGemChanged;
        }

        protected override void Unbind(PlayerData playerData)
        {
            if (playerData == null) return;
            playerData.OnGemChanged -= HandleGemChanged;
        }

        protected override void Refresh()
        {
            if (PlayerData == null) return;
            SetAmountText(PlayerData.Gem);
        }

        private void HandleGemChanged(int amount)
        {
            SetAmountText(amount);
        }
    }
}