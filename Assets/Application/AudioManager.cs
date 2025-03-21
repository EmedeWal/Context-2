namespace Context
{
    using UnityEngine;

    public class AudioManager
    {
        private readonly AudioSource _musicSource;
        private readonly AudioData[] _trackArray;

        private int _trackIndex;

        public AudioManager(AudioSource source, AudioData[] array)
        {
            _musicSource = source;

            // If the incoming array is empty, return
            if (array == null || array.Length == 0) return;

            // Initialize the track array or if it has been initialized, check if the incoming array is the same. If so, don't update.
            if (_trackArray != null && Collections.AreScriptableObjectArraysEqual(_trackArray, array)) return;

            _musicSource.loop = true;
            _trackArray = array;
            _trackIndex = 0;

            Play(_trackArray[_trackIndex], _musicSource);
        }

        public void Tick()
        {
            if (!_musicSource.isPlaying && _trackArray != null) // Cycle to next track if this one is finished
            {
                _trackIndex = Collections.SafeIncrement(_trackIndex, _trackArray.Length);
                Play(_trackArray[_trackIndex], _musicSource);
            }
        }

        public void Play(AudioData data, AudioSource source, float multiplier = 1)
        {
            if (data.Clip != null && data.Group != null)
            {
                if (source.isPlaying && !data.Overwrite)
                    return;

                source.Stop();
                SetupSource(data, source, multiplier);
            }
            else
                Debug.LogWarning("Audio data had a missing reference!");
        }

        private void SetupSource(AudioData data, AudioSource source, float multiplier)
        {
            multiplier = Mathf.Min(multiplier, 1);

            var pitch = data.PitchRanges.Length > 0 ? data.PitchRanges[Random.Range(0, data.PitchRanges.Length)] : 1;
            var volume = data.Volume * multiplier;
            var offset = data.Offset;

            source.outputAudioMixerGroup = data.Group;
            source.clip = data.Clip;

            source.volume = volume;
            source.pitch = pitch;
            source.time = offset;

            source.Play();
        }
    }
}