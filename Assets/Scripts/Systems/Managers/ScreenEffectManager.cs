using System;
using DG.Tweening;
using Gamelib.EventSystem;
using Systems.GameEvents;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Managers
{
    [DefaultExecutionOrder(-10)]
    public class ScreenEffectManager : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;

        private readonly int _circleSizeHash = Shader.PropertyToID("_CircleSize");

        [field: SerializeField] public EventChannelSO UIEventChannel { get; private set; }

        private Tween _fadeTween;
        private Material _fadeMaterial;

        private void Awake()
        {
            Debug.Assert(fadeImage != null, $"{name} : fadeImage가 비어 있습니다.");
            Debug.Assert(UIEventChannel != null, $"{name} : UIEventChannel이 비어 있습니다.");

            _fadeMaterial = new Material(fadeImage.material);
            fadeImage.material = _fadeMaterial;

            _fadeMaterial.SetFloat(_circleSizeHash, 0f);

            UIEventChannel.AddListener<FadeEvent>(HandleFadeEvent);
        }

        private void OnDestroy()
        {
            if (UIEventChannel != null)
                UIEventChannel.RemoveListener<FadeEvent>(HandleFadeEvent);

            _fadeTween?.Kill();
        }

        private void HandleFadeEvent(FadeEvent evt)
        {
            _fadeTween?.Kill();

            float fadeValue = evt.IsFadeIn ? 1.5f : 0f;
            float startValue = evt.IsFadeIn ? 0f : 1.5f;

            _fadeMaterial.SetFloat(_circleSizeHash, startValue);

            _fadeTween = _fadeMaterial
                .DOFloat(fadeValue, _circleSizeHash, evt.Duration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    try
                    {
                        evt.OnFadeEnd?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                    }
                });
        }
    }
}