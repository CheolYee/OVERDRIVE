using Gamelib.EventSystem;

namespace Systems.GameEvents
{
    public class StageEvents : GameEvent
    {
        public static readonly EnemyDeadEvent EnemyDead = new();
        public static readonly RoomClearedEvent RoomCleared = new();
        public static readonly NextRoomRequestedEvent NextRoomRequested = new();
    }

    public class EnemyDeadEvent : GameEvent
    {
        public int RunId { get; private set; }
        public int RoomId { get; private set; }
        public int EnemyRuntimeId { get; private set; }

        public EnemyDeadEvent Init(int runId, int roomId, int enemyRuntimeId)
        {
            RunId = runId;
            RoomId = roomId;
            EnemyRuntimeId = enemyRuntimeId;
            return this;
        }
    }

    public class RoomClearedEvent : GameEvent
    {
        public int RunId { get; private set; }
        public int RoomId { get; private set; }

        public RoomClearedEvent Init(int runId, int roomId)
        {
            RunId = runId;
            RoomId = roomId;
            return this;
        }
    }

    public class NextRoomRequestedEvent : GameEvent
    {
        public int RunId { get; private set; }
        public int RoomId { get; private set; }

        public NextRoomRequestedEvent Init(int runId, int roomId)
        {
            RunId = runId;
            RoomId = roomId;
            return this;
        }
    }
}