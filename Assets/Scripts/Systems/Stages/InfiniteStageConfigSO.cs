using Agents.StatSystem;
using Systems.Stages;
using UnityEngine;

namespace Systems.StageSystem
{
    [CreateAssetMenu(fileName = "InfiniteStageConfig", menuName = "Stage/Infinite Stage Config", order = 1)]
    public class InfiniteStageConfigSO : ScriptableObject
    {
        [Header("Room Selection")]
        [SerializeField] private StageMapDataSO[] mapList;
        [SerializeField] private bool avoidImmediateRepeat = true;

        [Header("Transition")]
        [SerializeField, Min(0.05f)] private float transitionDuration = 0.35f;

        [Header("Enemy Scaling")]
        [SerializeField] private StatSO hpStat;
        [SerializeField] private StatSO attackStat;
        [SerializeField] private StatSO moveSpeedStat;

        [SerializeField] private float hpBonusPerStage = 10f;
        [SerializeField] private float attackBonusPerStage = 2f;
        [SerializeField] private float moveSpeedBonusPerStage = 0f;

        public float TransitionDuration => transitionDuration;
        public bool AvoidImmediateRepeat => avoidImmediateRepeat;
        public StageMapDataSO[] MapList => mapList;

        public StageEnemyModifierData CreateModifierData(int stageIndex)
        {
            int stageOffset = Mathf.Max(0, stageIndex - 1);

            return new StageEnemyModifierData(
                hpStat != null ? hpStat.AssetIndex : -1,
                stageOffset * hpBonusPerStage,
                attackStat != null ? attackStat.AssetIndex : -1,
                stageOffset * attackBonusPerStage,
                moveSpeedStat != null ? moveSpeedStat.AssetIndex : -1,
                stageOffset * moveSpeedBonusPerStage
            );
        }
    }
}