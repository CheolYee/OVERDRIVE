using System;
using UnityEngine;

namespace CombatSystem.EffectSystem
{
    public class CombatEffectTrigger : MonoBehaviour
    {
        public event Action OnAnimationEnd;
        public event Action OnAttackTrigger;
        public event Action OnExplosionTrigger;
        
        private void AnimationEnd() => OnAnimationEnd?.Invoke();
        private void AttackTrigger() => OnAttackTrigger?.Invoke();
        private void ExplosionTrigger() => OnExplosionTrigger?.Invoke();
    }
}