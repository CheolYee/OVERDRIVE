using System;
using Gamelib.EventSystem;

namespace Systems.GameEvents
{
    public static class UIEvents
    {
        public static readonly FadeEvent Fade = new FadeEvent();
        public static readonly DeadPanelEvent DeadPanel = new DeadPanelEvent();
    }

    public class FadeEvent : GameEvent
    {
        public bool IsFadeIn { get; private set; }
        public float Duration { get; private set; }
        public Action OnFadeEnd { get; private set; }
        
        public FadeEvent Init(bool isFadeIn, float duration, Action onFadeEnd = null)
        {
            IsFadeIn = isFadeIn;
            Duration = duration;
            OnFadeEnd = onFadeEnd;
            return this;
        }
    }

    public class DeadPanelEvent : GameEvent { }
}