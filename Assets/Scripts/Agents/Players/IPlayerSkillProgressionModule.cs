using Agents.Players.Skills;
using Systems.Database;

namespace Agents.Players
{
    public interface IPlayerSkillProgressionModule
    {
        bool TryAcquireSkill(PlayerSkillDataSo acquiredSkillData);
        bool TryGetUpgradePath(PlayerSkill skillId, out PlayerSkillUpgradePathSo upgradePath);
    }
}