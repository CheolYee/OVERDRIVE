using System;
using DG.Tweening;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Managers
{
    public class ScreenEffectManager : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        
        private readonly int _circleSizeHash = Shader.PropertyToID("_CircleSize");
        
        [field: SerializeField] public EventChannelSO UIEventChannel { get; private set; }

        private void Awake()
        {
            fadeImage.material = new Material(fadeImage.material);
            UIEventChannel.AddListener<FadeEvent>(HandleFadeEvent);
        }

        private void OnDestroy()
        {
            UIEventChannel.RemoveListener<FadeEvent>(HandleFadeEvent);
        }

        private void HandleFadeEvent(FadeEvent evt)
        {
            float fadeValue = evt.IsFadeIn ? 1.5f : 0f;
            float startValue = evt.IsFadeIn ? 0f : 1.5f;
            
            fadeImage.material.SetFloat(_circleSizeHash, startValue);
            fadeImage.material.DOFloat(fadeValue, _circleSizeHash, evt.Duration).OnComplete(() =>
            {
                evt.OnFadeEnd?.Invoke();
            });
        }
    }
}