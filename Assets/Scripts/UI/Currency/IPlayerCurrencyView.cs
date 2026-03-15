using Agents.Players;

namespace UI.Currency
{
    public interface IPlayerCurrencyView
    {
        void SetPlayerData(PlayerData playerData);
        void ClearPlayerData();
    }
}