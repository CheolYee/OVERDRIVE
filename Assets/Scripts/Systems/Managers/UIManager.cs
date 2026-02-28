using System;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Managers
{
    public class UIManager : MonoBehaviour
    {
        [field: SerializeField] public UIInputSo UIInput {get; private set;}
        [field: SerializeField] public EventChannelSO SystemChannel {get; private set;}

        private void Awake()
        {
            UIInput.OnOpenMenuKeyPressed += HandleOpenMenuKeyPressed;
        }

        private void OnDestroy()
        {
            UIInput.OnOpenMenuKeyPressed -= HandleOpenMenuKeyPressed;
        }

        private void HandleOpenMenuKeyPressed()
        {
            SystemChannel.RaiseEvent(SystemEvents.OpenMenu.Init(0));
        }
    }
}