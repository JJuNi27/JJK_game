using JJKGame.CameraSystem;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    public sealed class SukunaCombatAudio : MonoBehaviour
    {
        [Header("Optional Local Sukuna Audio")]
        [SerializeField] private AudioClip domainVoice;
        [SerializeField] private AudioClip domainSound;
        [SerializeField] private AudioClip domainFugaSound;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.92f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.98f;

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private SimpleCameraFollow cameraFeedback;
        private AudioClip domainFallback;
        private AudioClip domainFugaFallback;

        public static SukunaCombatAudio GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            SukunaCombatAudio audio = owner.GetComponent<SukunaCombatAudio>();
            return audio != null ? audio : owner.AddComponent<SukunaCombatAudio>();
        }

        private void Awake()
        {
            LoadLocalOverrides();
            BuildSources();
            BuildFallbackClips();
        }

        private void OnDestroy()
        {
            if (domainFallback != null)
            {
                Destroy(domainFallback);
            }
            if (domainFugaFallback != null)
            {
                Destroy(domainFugaFallback);
            }
        }

        public void PlayDomain()
        {
            PlayVoice(domainVoice);
            PlaySfx(domainSound != null ? domainSound : domainFallback, 1f);
            ShakeAndFlash(0.44f, 0.52f, new Color(0.82f, 0.035f, 0.02f), 0.24f, 0.46f);
        }

        public void PlayDomainFuga()
        {
            PlaySfx(domainFugaSound != null ? domainFugaSound : domainFugaFallback, 1f);
            ShakeAndFlash(0.72f, 0.48f, new Color(1f, 0.16f, 0.015f), 0.30f, 0.38f);
        }

        private void LoadLocalOverrides()
        {
            domainVoice ??= Resources.Load<AudioClip>("LocalAudio/Sukuna_Domain");
            domainSound ??= Resources.Load<AudioClip>("LocalAudio/Sukuna_DomainSFX");
            domainFugaSound ??= Resources.Load<AudioClip>("LocalAudio/Sukuna_DomainFuga");
        }

        private void BuildSources()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.spatialBlend = 0f;
        }

        private void BuildFallbackClips()
        {
            domainFallback = CreateSweepClip("SukunaDomainFallback", 1.10f, 118f, 42f, 0.34f, 0.20f);
            domainFugaFallback = CreateSweepClip("SukunaDomainFugaFallback", 0.82f, 92f, 26f, 0.48f, 0.56f);
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

        private void ShakeAndFlash(
            float shakeAmplitude,
            float shakeDuration,
            Color color,
            float flashAlpha,
            float flashDuration
        )
        {
            cameraFeedback ??= FindFirstObjectByType<SimpleCameraFollow>();
            if (cameraFeedback == null)
            {
                return;
            }

            cameraFeedback.AddShake(shakeAmplitude, shakeDuration);
            cameraFeedback.Flash(color, flashAlpha, flashDuration);
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

                float attack = Mathf.Clamp01(normalized / 0.06f);
                float release = Mathf.Clamp01((1f - normalized) / 0.26f);
                float envelope = attack * release;
                float tone = Mathf.Sin(phase) * amplitude;
                float noise = (Random.value * 2f - 1f) * noiseAmount * amplitude;
                samples[index] = Mathf.Clamp((tone + noise) * envelope, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
