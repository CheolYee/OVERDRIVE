using Gamelib.EventSystem;

namespace Systems.Stages
{
    public interface IEnemyStageEventRelay
    {
        void Bind(EventChannelSO stageEventChannel, int runId, int roomId, int enemyRuntimeId);
        void ClearBinding();
    }
}