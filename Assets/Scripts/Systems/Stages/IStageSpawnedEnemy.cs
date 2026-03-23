using Gamelib.EventSystem;

namespace Systems.Stages
{
    public interface IStageSpawnedEnemy
    {
        void Bind(
            EventChannelSO stageEventChannel,
            int runId,
            int roomId,
            int enemyRuntimeId,
            StageEnemyModifierData modifierData);
    }
}