using System;
using Gamelib.EventSystem;
using Systems;
using Systems.GameEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Environments
{
    public class Portal : MonoBehaviour
    {
        public enum PortalType
        {
            POSITION_MOVE,
            SCENE_CHANGE
        }
        
        public PortalType portalType;
        [SerializeField] private int targetPositionIndex;
        [SerializeField] private string targetSceneName;
        [SerializeField] private float effectDuration = 0.3f;

        private bool _isTriggered;
        
        [field: SerializeField] public EventChannelSO UIChannel { get; private set; }
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }
        [field: SerializeField] public EventChannelSO SysyemChannel { get; private set; }

        private void Start()
        {
            _isTriggered = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isTriggered) return;
            _isTriggered = true;
            
            switch (portalType)
            {
                case PortalType.POSITION_MOVE:
                    break;
                case PortalType.SCENE_CHANGE:
                    PlayerChannel.RaiseEvent(PlayerEvents.ActivePlayerEvent.Init(false));
                    UIChannel.RaiseEvent(UIEvents.Fade.Init(false, effectDuration, () =>
                    {
                        SysyemChannel.RaiseEvent(SystemEvents.SavePref);
                        SceneManager.LoadScene(targetSceneName);
                    }));
                    break;
            }
        }
    }
}