using JJKGame.CameraSystem;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    [DisallowMultipleComponent]
    public sealed class PrototypeCombatAudio : MonoBehaviour
    {
        [Header("캐릭터 공통 오디오 프로필")]
        [SerializeField, InspectorName("전투 오디오 프로필")]
        [Tooltip("기술 1/2/필살기/영역 슬롯 기반 프로필입니다. 비워 두면 아래 기존 연결을 그대로 사용합니다.")]
        private CombatAudioProfile presentationProfile;

        [Header("기존 로컬 음성 / 음악 연결")]
        [SerializeField, InspectorName("전투 배경음악"), Tooltip("전투 배경음악입니다.")] private AudioClip backgroundMusic;
        [SerializeField, InspectorName("아오 음성"), Tooltip("아오 시전 시 재생할 음성입니다.")] private AudioClip blueVoice;
        [SerializeField, InspectorName("아카 음성"), Tooltip("아카 시전 시 재생할 음성입니다.")] private AudioClip redVoice;
        [SerializeField, InspectorName("무라사키 음성"), Tooltip("무라사키 시전 시 재생할 음성입니다.")] private AudioClip purpleVoice;
        [SerializeField, InspectorName("무량공처 음성"), Tooltip("무량공처 시전 시 재생할 음성입니다.")] private AudioClip domainVoice;

        [Header("기존 로컬 전투 효과음 연결")]
        [SerializeField, InspectorName("평타 휘두르기 효과음")] private AudioClip basicSwingSound;
        [SerializeField, InspectorName("평타 적중 효과음")] private AudioClip basicHitSound;
        [SerializeField, InspectorName("평타 마무리 효과음")] private AudioClip basicFinisherSound;
        [SerializeField, InspectorName("피격 효과음")] private AudioClip playerHitSound;
        [SerializeField, InspectorName("회피 효과음")] private AudioClip dodgeSound;
        [SerializeField, InspectorName("승리 효과음")] private AudioClip victorySound;
        [SerializeField, InspectorName("패배 효과음")] private AudioClip defeatSound;

        [Header("볼륨")]
        [SerializeField, InspectorName("효과음 볼륨"), Range(0f, 1f), Tooltip("모든 전투 효과음의 기준 볼륨입니다.")] private float sfxVolume = 0.85f;
        [SerializeField, InspectorName("음성 볼륨"), Range(0f, 1f), Tooltip("모든 음성의 기준 볼륨입니다.")] private float voiceVolume = 0.95f;
        [SerializeField, InspectorName("음악 볼륨"), Range(0f, 1f), Tooltip("배경음악의 기준 볼륨입니다.")] private float musicVolume = 0.32f;

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

        public void SetPresentationProfile(CombatAudioProfile profile)
        {
            presentationProfile = profile;
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
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Skill1);
            PlayVoice(cue != null && cue.voice != null ? cue.voice : blueVoice);
            PlaySfx(cue != null && cue.castSfx != null ? cue.castSfx : blueCastFallback, 0.75f);
        }

        public void PlayBlueImpactRuntime()
        {
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Skill1);
            PlaySfx(cue != null && cue.impactSfx != null ? cue.impactSfx : blueImpactFallback, 1f);
            ShakeAndFlash(0.18f, 0.18f, new Color(0.12f, 0.62f, 1f), 0.08f, 0.16f);
        }

        public void PlayRedCastRuntime()
        {
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Skill2);
            PlayVoice(cue != null && cue.voice != null ? cue.voice : redVoice);
            PlaySfx(cue != null && cue.castSfx != null ? cue.castSfx : redCastFallback, 0.82f);
        }

        public void PlayRedImpactRuntime(bool repulsivePressure = false)
        {
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Skill2);
            PlaySfx(cue != null && cue.impactSfx != null ? cue.impactSfx : redImpactFallback, 1f);
            if (repulsivePressure)
                ShakeAndFlash(0.58f, 0.24f, new Color(1f, 0.88f, 0.84f),
                    JJKGame.Player.GojoPolishSettings.Current.redFlashIntensity, 0.14f);
            else ShakeAndFlash(0.30f, 0.24f, new Color(1f, 0.12f, 0.08f), 0.13f, 0.20f);
        }

        // Audio-only path for events whose camera/flash feedback is already owned by
        // ProductionCombatFeedbackDirector (for example Fuga impact).
        public void PlayRedImpactAudioOnlyRuntime()
        {
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Skill2);
            PlaySfx(cue != null && cue.impactSfx != null ? cue.impactSfx : redImpactFallback, 1f);
        }

        public void PlayPurpleRuntime()
        {
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Ultimate);
            PlayVoice(cue != null && cue.voice != null ? cue.voice : purpleVoice);
            PlaySfx(ResolveReleaseOrCast(cue, purpleFallback), 1f);
        }

        public void PlayDomainRuntime()
        {
            TechniqueAudioProfile cue = ResolveTechnique(CharacterPresentationSkillSlot.Domain);
            PlayVoice(cue != null && cue.voice != null ? cue.voice : domainVoice);
            PlaySfx(ResolveReleaseOrCast(cue, domainFallback), 1f);
        }

        public void PlayBasicSwingRuntime(int chainStep)
        {
            float volume = chainStep >= 3 ? 0.94f : 0.68f + chainStep * 0.08f;
            AudioClip clip = presentationProfile != null && presentationProfile.basicSwing != null
                ? presentationProfile.basicSwing
                : basicSwingSound != null ? basicSwingSound : basicSwingFallback;
            PlaySfx(clip, volume);
        }

        public void PlayBasicHitRuntime(int chainStep)
        {
            AudioClip regularHit = presentationProfile != null && presentationProfile.basicHit != null
                ? presentationProfile.basicHit
                : basicHitSound != null ? basicHitSound : basicHitFallback;
            if (chainStep >= 3)
            {
                PlaySfx(regularHit, 0.78f);
                PlaySfx(
                    presentationProfile != null && presentationProfile.basicFinisher != null
                        ? presentationProfile.basicFinisher
                        : basicFinisherSound != null ? basicFinisherSound : basicFinisherFallback,
                    1f
                );
                return;
            }

            PlaySfx(regularHit, 0.72f + chainStep * 0.08f);
        }

        public void PlayDodgeRuntime()
        {
            AudioClip clip = presentationProfile != null && presentationProfile.dodge != null
                ? presentationProfile.dodge
                : dodgeSound != null ? dodgeSound : dodgeFallback;
            PlaySfx(clip, 0.90f);
            GetCameraFeedback()?.AddShake(0.05f, 0.10f);
        }

        /// <summary>
        /// Stops transient runtime playback without touching background music.
        /// Developer preview hosts use this to prevent one-shot/voice overlap when
        /// a preview is replayed, looped, or cancelled.
        /// </summary>
        public void StopTransientRuntimePlayback()
        {
            sfxSource?.Stop();
            voiceSource?.Stop();
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

        private TechniqueAudioProfile ResolveTechnique(CharacterPresentationSkillSlot slot)
        {
            return presentationProfile != null ? presentationProfile.GetTechnique(slot) : null;
        }

        private static AudioClip ResolveReleaseOrCast(TechniqueAudioProfile cue, AudioClip fallback)
        {
            if (cue == null)
            {
                return fallback;
            }

            if (cue.releaseSfx != null)
            {
                return cue.releaseSfx;
            }

            return cue.castSfx != null ? cue.castSfx : fallback;
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
