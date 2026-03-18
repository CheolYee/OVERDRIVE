using Agents.Players.Skills;
using Systems.Database;

namespace CombatSystem
{
    public interface IPlayerSkillModule : ISkillModule
    {
        bool TryUseBasicAttack();
        bool EnsureSkillRegistered(PlayerSkillDataSo skillData, SkillKey bindKey = SkillKey.NONE);

        bool ReplaceOwnedSkill(PlayerSkillDataSo oldSkillData, PlayerSkillDataSo newSkillData);
    }
}