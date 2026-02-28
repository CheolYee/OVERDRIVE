using System;
using System.Collections;
using CombatSystem.EffectSystem;
using Gamelib.EventSystem;
using Gamelib.ObjectPool.Runtime;
using Systems.GameEvents;
using UnityEngine;

namespace Systems.Managers
{
    public class CreateManager : MonoBehaviour
    {
        [field: SerializeField] public EventChannelSO CreateChannel { get; private set; }
        
        [SerializeField] private PoolManagerSo poolManager;
        [SerializeField] private PoolItemSo effectPoolItem;
        [SerializeField] private GameObject effectPrefab;

        private void Awake()
        {
            CreateChannel.AddListener<CreateEffectEvent>(HandleCreateEffect);
        }

        private void OnDestroy()
        {
            CreateChannel.RemoveListener<CreateEffectEvent>(HandleCreateEffect);
        }

        private void HandleCreateEffect(CreateEffectEvent evt)
        {
            if (effectPoolItem)
            {
                PoolAnimatorEffect effect = poolManager.Pop<PoolAnimatorEffect>(effectPoolItem);
                effect.PlayClipEffect(evt.Position, evt.Rotation, evt.ClipHash);
                StartCoroutine(HandleLifeTimeEnd(effect, evt.Duration));
            }
            else
            {
                GameObject effectObject = Instantiate(effectPrefab, evt.Position, evt.Rotation);
                AnimatorEffect effect = effectObject.GetComponent<AnimatorEffect>();
                Debug.Assert(effect != null, "Effect prefab does not have AnimatorEffect component");
                
                effect.PlayEffectClip(evt.ClipHash, evt.Duration);
            }
        }

        private IEnumerator HandleLifeTimeEnd(IPoolable effect, float evtDuration)
        {
            yield return new WaitForSeconds(evtDuration);
            poolManager.Push(effect);
        }
    }
}