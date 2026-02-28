namespace Agents.Players.Skills
{
    public interface IChargeableSkill
    {
        void ChargeStart();
        void ChargeEnd();
        void ChargeCancel();
    }
}