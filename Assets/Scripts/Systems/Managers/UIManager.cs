using System;
using Gamelib.EventSystem;
using Systems.Database;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Managers
{
    public class UIManager : MonoBehaviour
    {
        [field: SerializeField] public UIInputSo UIInput { get; private set; }
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }
        [SerializeField] private UIDataSo inventoryUIData;

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
            if (inventoryUIData == null)
                return;

            SystemChannel.RaiseEvent(SystemEvents.OpenMenu.Init(0));
        }
    }
}