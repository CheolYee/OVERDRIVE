using System;
using System.Collections;
using UnityEngine;

namespace CombatSystem.FeedBacks
{
    public class BlinkFeedback : Feedback
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float blinkDuration = 0.1f;
        [SerializeField] private int blinkValue;
        
        private readonly int _blinkShaderParam = Shader.PropertyToID("_BlinkValue");
        private Material _material;

        private void Awake()
        {
            Debug.Assert(targetRenderer != null, $"{gameObject.name} BlinkFeedback : Target Renderer is not assigned.");
            _material = targetRenderer.material;
        }

        public override void PlayFeedback()
        {
            _material.SetFloat(_blinkShaderParam, blinkValue);
            StartCoroutine(SetNormalAfterDelay());
        }

        private IEnumerator SetNormalAfterDelay()
        {
            yield return new WaitForSeconds(blinkDuration);
            StopFeedback();
        }

        public override void StopFeedback()
        {
            StopAllCoroutines();
            _material.SetFloat(_blinkShaderParam, 0);
        }
    }
}