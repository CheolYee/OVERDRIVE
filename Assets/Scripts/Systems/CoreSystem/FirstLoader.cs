using System.Collections;
using Gamelib.EventSystem;
using Gamelib.SoundSystem;
using Systems.GameEvents;
using Systems.Managers;
using UnityEngine;

namespace Systems.CoreSystem
{
    [DefaultExecutionOrder(-20)]
    public class FirstLoader : MonoBehaviour
    {
        [SerializeField] private UIInputSo uiInputSo;
        [field: SerializeField] public EventChannelSO UIChannel { get; private set; }
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }
        [field: SerializeField] public EventChannelSO SystemChannel { get; private set; }
        [SerializeField] public float effectDuration = 0.3f;
        [SerializeField] private BgmSounds bgmSounds;

        private void Start()
        {
            StartCoroutine(StartSequence());
        }

        private IEnumerator StartSequence()
        {
            Time.timeScale = 1.0f;
            uiInputSo.SetPlayerInputEnable(true);
            uiInputSo.SetEnable(true);

            PlayerChannel.RaiseEvent(PlayerEvents.ActivePlayerEvent.Init(false));
            SystemChannel.RaiseEvent(SystemEvents.LoadPref);

            yield return null;
            yield return new WaitForEndOfFrame();

            UIChannel.RaiseEvent(UIEvents.Fade.Init(true, effectDuration, () =>
            {
                PlayerChannel.RaiseEvent(PlayerEvents.ActivePlayerEvent.Init(true));
            }));
            SoundPlayManager.Instance?.PlayBgm(bgmSounds, transform.position);
        }
    }
}