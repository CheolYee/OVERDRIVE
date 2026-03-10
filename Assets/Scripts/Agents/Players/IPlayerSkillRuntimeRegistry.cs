using Agents.Players.Skills;

namespace Agents.Players
{
    public interface IPlayerSkillRuntimeRegistry
    {
        bool IsSkillRegistered(PlayerSkillDataSo skillData);
        bool TryRegisterSkill(PlayerSkillDataSo skillData);
        bool TryReplaceSkill(PlayerSkillDataSo previousSkillData, PlayerSkillDataSo newSkillData);
    }
}