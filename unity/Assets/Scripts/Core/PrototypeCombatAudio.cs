using System.Collections.Generic;
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

        private readonly HashSet<Health> trackedHealth = new HashSet<Health>();

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private AudioSource musicSource;
        private Health ownerHealth;
        private SimpleCameraFollow cameraFeedback;
        private Transform purpleVisual;
        private Transform domainVisual;
        private float lastOwnerHealth;
        private float nextHealthRefreshAt;
        private bool purpleWasActive;
        private bool domainWasActive;
        private bool resultSoundPlayed;
        private bool ownerHealthBound;

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
            RefreshHealthBindings();
            LocateTechniqueVisuals();
            LocateCameraFeedback();
        }

        private void Update()
        {
            if (Time.time >= nextHealthRefreshAt)
            {
                RefreshHealthBindings();
            }

            DetectTechniqueVisualActivations();
        }

        private void OnDestroy()
        {
            if (ownerHealth != null && ownerHealthBound)
            {
                ownerHealth.HealthChanged -= HandleOwnerHealthChanged;
            }

            foreach (Health health in trackedHealth)
            {
                if (health != null)
                {
                    health.Died -= HandleAnyDeath;
                }
            }
        }

        public void PlayBlueCast()
        {
            PlayVoice(blueVoice);
            PlaySfx(blueCastFallback, 0.75f);
        }

        public void PlayBlueImpact()
        {
            PlaySfx(blueImpactFallback, 1f);
            ShakeAndFlash(0.18f, 0.18f, new Color(0.12f, 0.62f, 1f), 0.08f, 0.16f);
        }

        public void PlayRedCast()
        {
            PlayVoice(redVoice);
            PlaySfx(redCastFallback, 0.82f);
        }

        public void PlayRedImpact()
        {
            PlaySfx(redImpactFallback, 1f);
            ShakeAndFlash(0.30f, 0.24f, new Color(1f, 0.12f, 0.08f), 0.13f, 0.20f);
        }

        public void PlayPurple()
        {
            PlayVoice(purpleVoice);
            PlaySfx(purpleFallback, 1f);
            ShakeAndFlash(0.58f, 0.42f, new Color(0.66f, 0.12f, 1f), 0.23f, 0.34f);
        }

        public void PlayDomain()
        {
            PlayVoice(domainVoice);
            PlaySfx(domainFallback, 1f);
            ShakeAndFlash(0.36f, 0.48f, new Color(0.34f, 0.64f, 1f), 0.20f, 0.42f);
        }

        public void PlayBasicSwing(int chainStep)
        {
            float volume = chainStep >= 3 ? 0.94f : 0.68f + chainStep * 0.08f;
            PlaySfx(basicSwingSound != null ? basicSwingSound : basicSwingFallback, volume);
        }

        public void PlayBasicHit(int chainStep)
        {
            AudioClip regularHit = basicHitSound != null ? basicHitSound : basicHitFallback;
            if (chainStep >= 3)
            {
                PlaySfx(regularHit, 0.78f);
                PlaySfx(
                    basicFinisherSound != null ? basicFinisherSound : basicFinisherFallback,
                    1f
                );
                ShakeAndFlash(
                    0.40f,
                    0.22f,
                    new Color(0.86f, 0.72f, 1f),
                    0.19f,
                    0.18f
                );
                return;
            }

            PlaySfx(regularHit, 0.72f + chainStep * 0.08f);
            float amplitude = 0.09f + chainStep * 0.025f;
            ShakeAndFlash(amplitude, 0.12f, Color.white, 0.045f, 0.10f);
        }

        public void PlayDodge()
        {
            PlaySfx(dodgeSound != null ? dodgeSound : dodgeFallback, 0.90f);
            GetCameraFeedback()?.AddShake(0.05f, 0.10f);
        }

        public void PlayPlayerHit()
        {
            PlaySfx(playerHitSound != null ? playerHitSound : playerHitFallback, 0.92f);
            ShakeAndFlash(0.22f, 0.20f, new Color(1f, 0.04f, 0.04f), 0.16f, 0.22f);
        }

        public void PlayVictory()
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

        public void PlayDefeat()
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

        private void RefreshHealthBindings()
        {
            nextHealthRefreshAt = Time.time + 0.5f;
            Health[] healthObjects = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health health in healthObjects)
            {
                if (health == null || !trackedHealth.Add(health))
                {
                    continue;
                }

                health.Died += HandleAnyDeath;
            }

            if (ownerHealth != null && !ownerHealthBound)
            {
                ownerHealthBound = true;
                lastOwnerHealth = ownerHealth.CurrentHealth;
                ownerHealth.HealthChanged += HandleOwnerHealthChanged;
            }
        }

        private void HandleOwnerHealthChanged(Health _, float currentHealth)
        {
            if (currentHealth < lastOwnerHealth)
            {
                PlayPlayerHit();
            }

            lastOwnerHealth = currentHealth;
        }

        private void HandleAnyDeath(Health deadHealth)
        {
            if (deadHealth == ownerHealth)
            {
                PlayDefeat();
                return;
            }

            bool foundLivingOpponent = false;
            foreach (Health health in trackedHealth)
            {
                if (health != null && health != ownerHealth && !health.IsDead)
                {
                    foundLivingOpponent = true;
                    break;
                }
            }

            if (!foundLivingOpponent)
            {
                PlayVictory();
            }
        }

        private void LocateTechniqueVisuals()
        {
            purpleVisual ??= transform.Find("HollowPurplePrototypeVisual");
            domainVisual ??= transform.Find("UnlimitedVoidPrototypeVisual");
        }

        private void DetectTechniqueVisualActivations()
        {
            LocateTechniqueVisuals();

            bool purpleActive = purpleVisual != null && purpleVisual.gameObject.activeInHierarchy;
            if (purpleActive && !purpleWasActive)
            {
                PlayPurple();
            }
            purpleWasActive = purpleActive;

            bool domainActive = domainVisual != null && domainVisual.gameObject.activeInHierarchy;
            if (domainActive && !domainWasActive)
            {
                PlayDomain();
            }
            domainWasActive = domainActive;
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
