namespace Context
{
    using UnityEngine;

    public class AudioManager
    {
        private readonly AudioSource[] _musicSources;
        private readonly float _overlapTime = 5f; // Overlap duration

        private int _activeSourceIndex;

        public AudioManager(AudioSource[] sources)
        {
            if (sources == null || sources.Length != 2)
            {
                Debug.LogError("AudioManager requires exactly two AudioSources.");
                return;
            }

            _musicSources = sources;
            _activeSourceIndex = 0;

            _musicSources[_activeSourceIndex].Play(); // Start first track
        }

        public void Tick()
        {
            var activeSource = _musicSources[_activeSourceIndex];
            var nextSource = _musicSources[1 - _activeSourceIndex]; // The inactive one

            if (activeSource.isPlaying && activeSource.time >= activeSource.clip.length - _overlapTime && !nextSource.isPlaying)
                nextSource.Play();

            if (!activeSource.isPlaying && nextSource.isPlaying)
                _activeSourceIndex = 1 - _activeSourceIndex;
        }

        public void Play(AudioData data, AudioSource source, float multiplier = 1)
        {
            if (data.Clip != null && data.Group != null)
            {
                var time = source.clip != null ? source.time : data.Clip.length + 1;
                var timePercentage = time / data.Clip.length;

                //Debug.Log($"Time: {timePercentage} while trying to play {data.Clip.name}");
                if (source.isPlaying && timePercentage < data.Overwrite)
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