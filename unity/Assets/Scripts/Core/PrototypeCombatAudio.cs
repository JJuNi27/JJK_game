using JJKGame.CameraSystem;
using UnityEngine;

namespace JJKGame.Core
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCombatAudio : MonoBehaviour
    {
        [Header("Optional Local Voice / Music Overrides")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip blueVoice;
        [SerializeField] private AudioClip redVoice;
        [SerializeField] private AudioClip purpleVoice;
        [SerializeField] private AudioClip domainVoice;

        [Header("Optional Local Combat SFX Overrides")]
        [SerializeField] private AudioClip basicSwingSound;
        [SerializeField] private AudioClip basicHitSound;
        [SerializeField] private AudioClip basicFinisherSound;
        [SerializeField] private AudioClip playerHitSound;
        [SerializeField] private AudioClip dodgeSound;
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip defeatSound;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float voiceVolume = 0.95f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.32f;

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private AudioSource musicSource;
        private Health ownerHealth;
        private SimpleCameraFollow cameraFeedback;
        private bool resultSoundPlayed;

        private AudioClip blueCastFallback;
        private AudioClip blueImpactFallback;
        private AudioClip redCastFallback;
        private AudioClip redImpactFallback;
        private AudioClip purpleFallback;
        private AudioClip domainFallback;
        private AudioClip basicSwingFallback;
        private AudioClip basicHitFallback;
        private AudioClip basicFinisherFallback;
        private AudioClip playerHitFallback;
        private AudioClip dodgeFallback;
        private AudioClip victoryFallback;
        private AudioClip defeatFallback;

        public static PrototypeCombatAudio GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            PrototypeCombatAudio audio = owner.GetComponent<PrototypeCombatAudio>();
            return audio != null ? audio : owner.AddComponent<PrototypeCombatAudio>();
        }

        private void Awake()
        {
            ownerHealth = GetComponent<Health>();
            LoadLocalOverrides();
            BuildSources();
            BuildFallbackClips();
            StartBackgroundMusic();
        }

        private void Start()
        {
            LocateCameraFeedback();
        }

        // Gate 4F compatibility shims. Existing prototype callers may keep these names,
        // but they no longer play clips directly. New production-facing code should raise
        // CombatAudioEvents explicitly instead of depending on this prototype component.
        public void PlayBlueCast()
        {
            Raise(CombatAudioEventId.GojoBlueCast);
        }

        public void PlayBlueImpact()
        {
            Raise(CombatAudioEventId.GojoBlueImpact);
        }

        public void PlayRedCast()
        {
            Raise(CombatAudioEventId.GojoRedCast);
        }

        public void PlayRedImpact()
        {
            Raise(CombatAudioEventId.TechniqueImpact);
        }

        public void PlayPurple()
        {
            Raise(CombatAudioEventId.HollowPurple);
        }

        public void PlayDomain()
        {
            Raise(CombatAudioEventId.UnlimitedVoid);
        }

        public void PlayBasicSwing(int chainStep)
        {
            Raise(CombatAudioEventId.BasicSwing, chainStep);
        }

        public void PlayBasicHit(int chainStep)
        {
            Raise(CombatAudioEventId.BasicHit, chainStep);
        }

        public void PlayDodge()
        {
            Raise(CombatAudioEventId.Dodge);
        }

        public void PlayPlayerHit()
        {
            Raise(CombatAudioEventId.PlayerHit);
        }

        public void PlayVictory()
        {
            Raise(CombatAudioEventId.Victory);
        }

        public void PlayDefeat()
        {
            Raise(CombatAudioEventId.Defeat);
        }

        // Runtime-only playback entry points consumed by PrototypeCombatAudioEventBridge.
        public void PlayBlueCastRuntime()
        {
            PlayVoice(blueVoice);
            PlaySfx(blueCastFallback, 0.75f);
        }

        public void PlayBlueImpactRuntime()
        {
            PlaySfx(blueImpactFallback, 1f);
            ShakeAndFlash(0.18f, 0.18f, new Color(0.12f, 0.62f, 1f), 0.08f, 0.16f);
        }

        public void PlayRedCastRuntime()
        {
            PlayVoice(redVoice);
            PlaySfx(redCastFallback, 0.82f);
        }

        public void PlayRedImpactRuntime()
        {
            PlaySfx(redImpactFallback, 1f);
            ShakeAndFlash(0.30f, 0.24f, new Color(1f, 0.12f, 0.08f), 0.13f, 0.20f);
        }

        // Audio-only path for events whose camera/flash feedback is already owned by
        // ProductionCombatFeedbackDirector (for example Fuga impact).
        public void PlayRedImpactAudioOnlyRuntime()
        {
            PlaySfx(redImpactFallback, 1f);
        }

        public void PlayPurpleRuntime()
        {
            PlayVoice(purpleVoice);
            PlaySfx(purpleFallback, 1f);
        }

        public void PlayDomainRuntime()
        {
            PlayVoice(domainVoice);
            PlaySfx(domainFallback, 1f);
        }

        public void PlayBasicSwingRuntime(int chainStep)
        {
            float volume = chainStep >= 3 ? 0.94f : 0.68f + chainStep * 0.08f;
            PlaySfx(basicSwingSound != null ? basicSwingSound : basicSwingFallback, volume);
        }

        public void PlayBasicHitRuntime(int chainStep)
        {
            AudioClip regularHit = basicHitSound != null ? basicHitSound : basicHitFallback;
            if (chainStep >= 3)
            {
                PlaySfx(regularHit, 0.78f);
                PlaySfx(
                    basicFinisherSound != null ? basicFinisherSound : basicFinisherFallback,
                    1f
                );
                return;
            }

            PlaySfx(regularHit, 0.72f + chainStep * 0.08f);
        }

        public void PlayDodgeRuntime()
        {
            PlaySfx(dodgeSound != null ? dodgeSound : dodgeFallback, 0.90f);
            GetCameraFeedback()?.AddShake(0.05f, 0.10f);
        }

        public void PlayPlayerHitRuntime()
        {
            PlaySfx(playerHitSound != null ? playerHitSound : playerHitFallback, 0.92f);
            ShakeAndFlash(0.22f, 0.20f, new Color(1f, 0.04f, 0.04f), 0.16f, 0.22f);
        }

        public void PlayVictoryRuntime()
        {
            if (resultSoundPlayed)
            {
                return;
            }

            resultSoundPlayed = true;
            FadeMusicForResult();
            PlaySfx(victorySound != null ? victorySound : victoryFallback, 1f);
            ShakeAndFlash(0.12f, 0.28f, new Color(0.16f, 0.72f, 1f), 0.12f, 0.34f);
        }

        public void PlayDefeatRuntime()
        {
            if (resultSoundPlayed)
            {
                return;
            }

            resultSoundPlayed = true;
            FadeMusicForResult();
            PlaySfx(defeatSound != null ? defeatSound : defeatFallback, 1f);
            ShakeAndFlash(0.26f, 0.36f, new Color(0.70f, 0.01f, 0.02f), 0.20f, 0.40f);
        }

        private void Raise(CombatAudioEventId eventId, int variant = 0, bool amplified = false)
        {
            ownerHealth ??= GetComponent<Health>();
            if (ownerHealth == null)
            {
                return;
            }

            CombatAudioEvents.Raise(
                CombatAudioEvent.ForOwner(ownerHealth, eventId, variant, amplified)
            );
        }

        private void LoadLocalOverrides()
        {
            backgroundMusic ??= Resources.Load<AudioClip>("LocalAudio/BGM");
            blueVoice ??= Resources.Load<AudioClip>("LocalAudio/Gojo_Blue");
            redVoice ??= Resources.Load<AudioClip>("LocalAudio/Gojo_Red");
            purpleVoice ??= Resources.Load<AudioClip>("LocalAudio/Gojo_Purple");
            domainVoice ??= Resources.Load<AudioClip>("LocalAudio/Gojo_Domain");
            basicSwingSound ??= Resources.Load<AudioClip>("LocalAudio/BasicSwing");
            basicHitSound ??= Resources.Load<AudioClip>("LocalAudio/BasicHit");
            basicFinisherSound ??= Resources.Load<AudioClip>("LocalAudio/BasicFinisher");
            playerHitSound ??= Resources.Load<AudioClip>("LocalAudio/PlayerHit");
            dodgeSound ??= Resources.Load<AudioClip>("LocalAudio/Dodge");
            victorySound ??= Resources.Load<AudioClip>("LocalAudio/Victory");
            defeatSound ??= Resources.Load<AudioClip>("LocalAudio/Defeat");
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
            blueCastFallback = CreateSweepClip("BlueCastFallback", 0.24f, 190f, 520f, 0.20f, 0.04f);
            blueImpactFallback = CreateSweepClip("BlueImpactFallback", 0.34f, 150f, 62f, 0.27f, 0.16f);
            redCastFallback = CreateSweepClip("RedCastFallback", 0.20f, 360f, 760f, 0.22f, 0.06f);
            redImpactFallback = CreateSweepClip("RedImpactFallback", 0.42f, 125f, 48f, 0.34f, 0.38f);
            purpleFallback = CreateSweepClip("PurpleFallback", 0.75f, 105f, 430f, 0.32f, 0.19f);
            domainFallback = CreateSweepClip("DomainFallback", 1.05f, 85f, 310f, 0.27f, 0.10f);
            basicSwingFallback = CreateSweepClip("BasicSwingFallback", 0.14f, 520f, 190f, 0.12f, 0.22f);
            basicHitFallback = CreateSweepClip("BasicHitFallback", 0.16f, 115f, 55f, 0.26f, 0.42f);
            basicFinisherFallback = CreateSweepClip("BasicFinisherFallback", 0.32f, 82f, 28f, 0.42f, 0.58f);
            playerHitFallback = CreateSweepClip("PlayerHitFallback", 0.24f, 92f, 42f, 0.30f, 0.52f);
            dodgeFallback = CreateSweepClip("DodgeFallback", 0.20f, 260f, 720f, 0.15f, 0.20f);
            victoryFallback = CreateSweepClip("VictoryFallback", 0.85f, 260f, 690f, 0.20f, 0.03f);
            defeatFallback = CreateSweepClip("DefeatFallback", 0.90f, 180f, 48f, 0.24f, 0.10f);
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

        private SimpleCameraFollow GetCameraFeedback()
        {
            if (cameraFeedback == null)
            {
                LocateCameraFeedback();
            }

            return cameraFeedback;
        }

        private void LocateCameraFeedback()
        {
            cameraFeedback = FindFirstObjectByType<SimpleCameraFollow>();
        }

        private void ShakeAndFlash(
            float shakeAmplitude,
            float shakeDuration,
            Color color,
            float flashAlpha,
            float flashDuration
        )
        {
            SimpleCameraFollow feedback = GetCameraFeedback();
            if (feedback == null)
            {
                return;
            }

            feedback.AddShake(shakeAmplitude, shakeDuration);
            feedback.Flash(color, flashAlpha, flashDuration);
        }

        private void FadeMusicForResult()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.volume = musicVolume * 0.35f;
            }
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
