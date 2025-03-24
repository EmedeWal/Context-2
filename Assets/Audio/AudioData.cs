namespace Context
{
    using UnityEngine.Audio;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Audio Data", menuName = "Scriptable Objects/Audio")]
    public class AudioData : ScriptableObject
    {
        [Header("SETTINGS")]
        public AudioMixerGroup Group;
        public AudioClip Clip;
        [Range(0, 1)] public float Volume = 0.5f;
        [Min(0)] public float Offset = 0;
        public float[] PitchRanges;
        [Range(0, 1)] public float Overwrite = 0f;
    }
}