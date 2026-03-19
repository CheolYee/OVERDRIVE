namespace Systems.Stages
{
    public readonly struct StageEnemyModifierData
    {
        public readonly int HpStatIndex;
        public readonly float HpBonus;

        public readonly int AttackStatIndex;
        public readonly float AttackBonus;

        public readonly int MoveSpeedStatIndex;
        public readonly float MoveSpeedBonus;

        public StageEnemyModifierData(
            int hpStatIndex, float hpBonus,
            int attackStatIndex, float attackBonus,
            int moveSpeedStatIndex, float moveSpeedBonus)
        {
            HpStatIndex = hpStatIndex;
            HpBonus = hpBonus;
            AttackStatIndex = attackStatIndex;
            AttackBonus = attackBonus;
            MoveSpeedStatIndex = moveSpeedStatIndex;
            MoveSpeedBonus = moveSpeedBonus;
        }
    }
}