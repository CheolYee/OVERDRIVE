using System;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.CoreSystem
{
    [DefaultExecutionOrder(-20)]
    public class FirstLoader : MonoBehaviour
    {
        [field: SerializeField] public EventChannelSO UIChannel { get; private set; }
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }
        [SerializeField] public float effectDuration = 0.3f;

        private void Start()
        {
            PlayerChannel.RaiseEvent(PlayerEvents.ActivePlayerEvent.Init(false));
            SystemChannel.RaiseEvent(SystemEvents.LoadPref);
            
            UIChannel.RaiseEvent(UIEvents.Fade.Init(true, effectDuration, () =>
            {
                PlayerChannel.RaiseEvent(PlayerEvents.ActivePlayerEvent.Init(true));
            }));
        }
    }
}