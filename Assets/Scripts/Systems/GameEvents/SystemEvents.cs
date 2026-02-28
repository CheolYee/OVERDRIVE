using Gamelib.EventSystem;

namespace Systems.GameEvents
{
    public static class SystemEvents
    {
        public static readonly SavePrefEvent SavePref = new SavePrefEvent();
        public static readonly LoadPrefEvent LoadPref = new LoadPrefEvent();
        public static readonly OpenMenuEvent OpenMenu = new OpenMenuEvent();
    }
    
    public class SavePrefEvent : GameEvent { }
    public class LoadPrefEvent : GameEvent { }

    public class OpenMenuEvent : GameEvent
    {
        public int TargetUIHash {get; private set;}

        public OpenMenuEvent Init(int targetUIHash)
        {
            TargetUIHash = targetUIHash;
            return this;
        }
    }
}