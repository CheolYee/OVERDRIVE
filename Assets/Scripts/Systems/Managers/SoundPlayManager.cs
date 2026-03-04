using Gamelib.EventSystem;
using Gamelib.SoundSystem;
using UnityEngine;

namespace Systems.Managers
{
    public class SoundPlayManager : MonoSingleton<SoundPlayManager>
    {
        [field: SerializeField] public EventChannelSO SoundChannel { get; private set; }
        [SerializeField] private SoundListSo sfxSoundList;
        [SerializeField] private SoundListSo bgmSoundList;
        
        public void PlaySfx(SfxSounds sfxSound, Vector3 position)
        {
            SoundChannel.RaiseEvent(SoundEvents.PlaySoundEvent.Init(position, sfxSoundList.sounds[(int)sfxSound]));
        }
        
        public void PlayBgm(BgmSounds bgmSound, Vector3 position)
        {
            SoundChannel.RaiseEvent(SoundEvents.PlaySoundEvent.Init(position, bgmSoundList.sounds[(int)bgmSound]));
        }
    }
}