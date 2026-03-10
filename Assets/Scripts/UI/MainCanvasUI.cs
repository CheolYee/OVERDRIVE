using System;
using Gamelib.EventSystem;
using Systems;
using Systems.GameEvents;
using UnityEngine;

namespace UI
{
    public class MainCanvasUI : MonoBehaviour
    {
        [field: SerializeField] public UIInputSo UIInput { get; private set; }
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }
        
        [SerializeField] private MenuCanvasUI menuCanvasUI;

        private void Awake()
        {
            SystemChannel.AddListener<OpenMenuEvent>(HandleOpenMenuEvent);
        }

        private void OnDestroy()
        {
            SystemChannel.RemoveListener<OpenMenuEvent>(HandleOpenMenuEvent);
        }

        private void HandleOpenMenuEvent(OpenMenuEvent evt)
        {
            switch (menuCanvasUI.WindowStatus)
            {
                case UIWindowStatus.CLOSING:
                case UIWindowStatus.OPENING:
                    break;
                case UIWindowStatus.OPENED:
                    menuCanvasUI.CloseWindow();
                    UIInput.SetPlayerInputEnable(true);
                    break;
                case UIWindowStatus.CLOSED:
                    menuCanvasUI.OpenWindow(evt.TargetUIHash);
                    UIInput.SetPlayerInputEnable(false);
                    break;
            }
        }
    }
}