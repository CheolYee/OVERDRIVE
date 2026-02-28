using System;
using UnityEngine;

namespace CombatSystem.EffectSystem
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorEffect : MonoBehaviour
    {
        [SerializeField] private bool destroyOnFinish = true;
        private Animator _animator;
        private float _startTime;
        private float _lifeTime;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void PlayEffectClip(int clipHash, float lifeTime)
        {
            _animator.Play(clipHash);
            _startTime = Time.time;
            _lifeTime = lifeTime;
        }

        private void Update()
        {
            if (!destroyOnFinish) return;
            
            float passedTime = Time.time - _startTime;
            if (passedTime >= _lifeTime)
                Destroy(gameObject);
        }
    }
}