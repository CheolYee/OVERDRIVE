using System.Collections.Generic;
using Gamelib.EventSystem;
using Gamelib.ObjectPool.Runtime;
using UnityEngine;

namespace Gamelib.SoundSystem
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private PoolManagerSo poolManager;
        [SerializeField] private PoolItemSo soundItem;
        
        [field: SerializeField] public EventChannelSO SoundChannel { get; private set; }
        
        private readonly Dictionary<int, SoundPlayer> _soundPlayerDict = new();
        
        private void Awake()
        {
            SoundChannel.AddListener<PlaySoundEvent>(HandlePlaySoundEvent);
            SoundChannel.AddListener<StopSoundEvent>(HandleStopSoundEvent);
        }

        private void OnDestroy()
        {
            SoundChannel.RemoveListener<PlaySoundEvent>(HandlePlaySoundEvent);
            SoundChannel.RemoveListener<StopSoundEvent>(HandleStopSoundEvent);
        }

        private void HandlePlaySoundEvent(PlaySoundEvent evt)
        {
            SoundPlayer player = poolManager.Pop<SoundPlayer>(soundItem);
            player.transform.position = evt.Position;
            player.PlaySound(evt.ClipData);
            player.OnSoundFinished += HandleSoundFinish;

            if (evt.ChannelNumber > 0 && evt.ClipData.loop)
            {
                if (_soundPlayerDict.TryGetValue(evt.ChannelNumber, out SoundPlayer beforePlayer))
                {
                    beforePlayer.ForceStopSound();
                    poolManager.Push(beforePlayer);
                    _soundPlayerDict.Remove(evt.ChannelNumber);
                }
                _soundPlayerDict.Add(evt.ChannelNumber, player);
            }else if(evt.ChannelNumber <= 0 && evt.ClipData.loop)
            {
                Debug.LogWarning($"사운드 데이터 루프가 활성화된 경우 채널 값은 0보다 커야 합니다. : {evt.ClipData.name}");   
            }
        }

        private void HandleSoundFinish(SoundPlayer player)
        {
            player.OnSoundFinished -= HandleSoundFinish;
            poolManager.Push(player);
        }


        private void HandleStopSoundEvent(StopSoundEvent evt)
        {
            if (_soundPlayerDict.TryGetValue(evt.ChannelNumber, out SoundPlayer beforePlayer))
            {
                beforePlayer.ForceStopSound();
                beforePlayer.OnSoundFinished -= HandleSoundFinish;
                poolManager.Push(beforePlayer);
                _soundPlayerDict.Remove(evt.ChannelNumber);
            }
        }
    }
}