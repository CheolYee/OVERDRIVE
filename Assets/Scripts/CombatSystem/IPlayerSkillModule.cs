namespace CombatSystem
{
    public interface IPlayerSkillModule : ISkillModule
    {
        bool TryUseBasicAttack();
    }
}