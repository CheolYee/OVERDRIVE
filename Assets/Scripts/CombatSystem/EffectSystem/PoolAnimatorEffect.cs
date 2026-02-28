using Gamelib.ObjectPool.Runtime;
using UnityEngine;

namespace CombatSystem.EffectSystem
{
    public class PoolAnimatorEffect : PoolableMono
    {
        [SerializeField] private AnimatorEffect animatorEffect;

        public void PlayClipEffect(Vector3 position, Quaternion rotation, int clipHash)
        {
            transform.SetPositionAndRotation(position, rotation);
            animatorEffect.PlayEffectClip(clipHash, 0);
        }
    }
}