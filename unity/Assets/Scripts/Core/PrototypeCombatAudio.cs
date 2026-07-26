using UnityEngine;

namespace JJKGame.Core
{
    public sealed class PrototypeCombatAudio : MonoBehaviour
    {
        [Header("Optional Local Overrides")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip blueVoice;
        [SerializeField] private AudioClip redVoice;
        [SerializeField] private AudioClip purpleVoice;
        [SerializeField] private AudioClip domainVoice;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.95f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.32f;

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private AudioSource musicSource;

        private AudioClip blueCastFallback;
        private AudioClip blueImpactFallback;
        private AudioClip redCastFallback;
        private AudioClip redImpactFallback;
        private AudioClip purpleFallback;
        private AudioClip domainFallback;

        private void Awake()
        {
            LoadLocalOverrides();
            BuildSources();
            BuildFallbackClips();
            StartBackgroundMusic();
        }

        public void PlayBlueCast()
        {
            PlayVoice(blueVoice);
            PlaySfx(blueCastFallback, 0.75f);
        }

        public void PlayBlueImpact()
        {
            PlaySfx(blueImpactFallback, 1f);
        }

        public void PlayRedCast()
        {
            PlayVoice(redVoice);
            PlaySfx(redCastFallback, 0.82f);
        }

        public void PlayRedImpact()
        {
            PlaySfx(redImpactFallback, 1f);
        }

        public void PlayPurple()
        {
            PlayVoice(purpleVoice);
            PlaySfx(purpleFallback, 1f);
        }

        public void PlayDomain()
        {
            PlayVoice(domainVoice);
            PlaySfx(domainFallback, 1f);
        }

        private void LoadLocalOverrides()
        {
            if (backgroundMusic == null)
            {
                backgroundMusic = Resources.Load<AudioClip>("LocalAudio/BGM");
            }

            if (blueVoice == null)
            {
                blueVoice = Resources.Load<AudioClip>("LocalAudio/Gojo_Blue");
            }

            if (redVoice == null)
            {
                redVoice = Resources.Load<AudioClip>("LocalAudio/Gojo_Red");
            }

            if (purpleVoice == null)
            {
                purpleVoice = Resources.Load<AudioClip>("LocalAudio/Gojo_Purple");
            }

            if (domainVoice == null)
            {
                domainVoice = Resources.Load<AudioClip>("LocalAudio/Gojo_Domain");
            }
        }

        private void BuildSources()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume;
        }

        private void BuildFallbackClips()
        {
            blueCastFallback = CreateSweepClip(
                "BlueCastFallback",
                0.24f,
                190f,
                520f,
                0.20f,
                0.04f
            );
            blueImpactFallback = CreateSweepClip(
                "BlueImpactFallback",
                0.34f,
                150f,
                62f,
                0.27f,
                0.16f
            );
            redCastFallback = CreateSweepClip(
                "RedCastFallback",
                0.20f,
                360f,
                760f,
                0.22f,
                0.06f
            );
            redImpactFallback = CreateSweepClip(
                "RedImpactFallback",
                0.42f,
                125f,
                48f,
                0.34f,
                0.38f
            );
            purpleFallback = CreateSweepClip(
                "PurpleFallback",
                0.75f,
                105f,
                430f,
                0.32f,
                0.19f
            );
            domainFallback = CreateSweepClip(
                "DomainFallback",
                1.05f,
                85f,
                310f,
                0.27f,
                0.10f
            );
        }

        private void StartBackgroundMusic()
        {
            if (backgroundMusic == null || musicSource == null)
            {
                return;
            }

            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        private void PlaySfx(AudioClip clip, float relativeVolume)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(relativeVolume));
            }
        }

        private void PlayVoice(AudioClip clip)
        {
            if (clip == null || voiceSource == null)
            {
                return;
            }

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = voiceVolume;
            voiceSource.Play();
        }

        private static AudioClip CreateSweepClip(
            string clipName,
            float duration,
            float startFrequency,
            float endFrequency,
            float amplitude,
            float noiseAmount
        )
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * sampleRate));
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int index = 0; index < sampleCount; index++)
            {
                float normalized = (float)index / Mathf.Max(1, sampleCount - 1);
                float frequency = Mathf.Lerp(startFrequency, endFrequency, normalized);
                phase += frequency / sampleRate * Mathf.PI * 2f;

                float attack = Mathf.Clamp01(normalized / 0.08f);
                float release = Mathf.Clamp01((1f - normalized) / 0.24f);
                float envelope = attack * release;
                float tone = Mathf.Sin(phase) + Mathf.Sin(phase * 0.51f) * 0.32f;
                float pseudoNoise = Mathf.Sin(index * 12.9898f) * 43758.5453f;
                pseudoNoise = (pseudoNoise - Mathf.Floor(pseudoNoise)) * 2f - 1f;

                samples[index] =
                    (tone * (1f - noiseAmount) + pseudoNoise * noiseAmount)
                    * amplitude
                    * envelope;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
