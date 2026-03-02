using UnityEngine;

namespace Gamelib.SoundSystem
{
    public enum AudioTypes
    { SFX, MUSIC }

    [CreateAssetMenu(fileName = "Sound clip data", menuName = "Sound/ClipData", order = 10)]
    public class SoundClipSo : ScriptableObject
    {
        public AudioTypes audioType;
        public AudioClip clip;
        public bool loop;
        public bool randomizePitch;

        [Range(0, 1f)]
        public float randomPitchModifier = 0.1f;
        [Range(0.1f, 2f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;

    }

}