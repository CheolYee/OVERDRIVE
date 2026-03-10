namespace Agents.Players
{
    public interface IPlayerCurrencyWallet
    {
        int CurrentGold { get; }
        bool CanSpendGold(int amount);
        bool TrySpendGold(int amount);
        void AddGold(int amount);
    }
}