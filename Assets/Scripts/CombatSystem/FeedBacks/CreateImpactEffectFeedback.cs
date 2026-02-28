using Agents;
using Gamelib.EventSystem;
using Systems;
using Systems.AnimationSystems;
using Systems.GameEvents;
using UnityEngine;

namespace CombatSystem.FeedBacks
{
    public class CreateImpactEffectFeedback : Feedback
    {
        [field: SerializeField] public EventChannelSO CreateChannel {get; private set;}
        
        [SerializeField] private AnimParamSO effectParam;
        [SerializeField] private float effectDuration = 1f;
        [SerializeField] private bool isPoolingEffect = true;
        [SerializeField] private ActionDataModule actionData;
        [SerializeField] private Vector2 randomOffset;
        public override void PlayFeedback()
        {
            float xRandom = Random.Range(-randomOffset.x, randomOffset.x);
            float yRandom = Random.Range(-randomOffset.y, randomOffset.y);
            Vector3 spawnPosition = actionData.LastHitPoint + new Vector2(xRandom, yRandom);

            CreateEffectEvent evt = CreateEvents.CreateEffect.Init(
                spawnPosition,
                Quaternion.Euler(0, 0, Random.Range(0, 360)),
                effectParam.ParamHash,
                effectDuration, isPoolingEffect);
            
            CreateChannel.RaiseEvent(evt);
        }
    }
}