using JJKGame.Core;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Dev.VFXLab
{
    public enum VfxLabPreviewKind
    {
        BlueFull,
        BlueField,
        BlueImpact,
    }

    [DisallowMultipleComponent]
    public sealed class VfxLabPreviewSequence : MonoBehaviour
    {
        private const float BlueRadius = 4.5f;
        private const float BlueFieldDuration = 0.95f;
        private const float FullCastAt = 0.34f;
        private const float FullFieldAt = 0.58f;
        private const float FullImpactAt = 1.30f;
        private const float FullRecoverAt = 1.58f;
        private const float FullCleanupAt = 1.92f;
        private const float LoopDelay = 0.34f;

        private Transform previewPoint;
        private VfxLabPreviewCharacter previewCharacter;
        private PresentationVfxHandle fieldHandle;
        private PresentationVfxHandle impactHandle;
        private VfxLabPreviewKind selectedPreview = VfxLabPreviewKind.BlueFull;
        private float sequenceElapsed;
        private float loopWaitElapsed;
        private float playbackSpeed = 1f;
        private float originalTimeScale = 1f;
        private float originalFixedDeltaTime = 0.02f;
        private bool capturedTimeScale;
        private bool running;
        private bool paused;
        private bool loopWaiting;
        private bool loopEnabled;
        private bool castPosePlayed;
        private bool fieldSpawned;
        private bool impactSpawned;
        private string phaseBeforePause = "IDLE";

        public string SelectedPreviewLabel => selectedPreview switch
        {
            VfxLabPreviewKind.BlueField => "BLUE · FIELD ONLY",
            VfxLabPreviewKind.BlueImpact => "BLUE · IMPACT / COLLAPSE ONLY",
            _ => "BLUE · FULL SEQUENCE",
        };

        public string CurrentPhaseLabel { get; private set; } = "IDLE";
        public bool LoopEnabled => loopEnabled;
        public bool Paused => paused;
        public float PlaybackSpeed => playbackSpeed;
        public bool RuntimeReady => PresentationVfxRuntime.HasRuntime;

        public void Configure(
            Transform newPreviewPoint,
            VfxLabPreviewCharacter newPreviewCharacter
        )
        {
            previewPoint = newPreviewPoint;
            previewCharacter = newPreviewCharacter;
        }

        private void OnEnable()
        {
            CaptureTimeScale();
            ApplyTimeScale();
        }

        private void Update()
        {
            HandleInput();

            if (paused)
            {
                return;
            }

            if (loopWaiting)
            {
                if (!loopEnabled)
                {
                    loopWaiting = false;
                    CurrentPhaseLabel = "IDLE";
                    return;
                }
                loopWaitElapsed += Time.deltaTime;
                CurrentPhaseLabel = "LOOP WAIT";
                if (loopWaitElapsed >= LoopDelay)
                {
                    Begin(selectedPreview);
                }
                return;
            }

            if (!running)
            {
                return;
            }

            sequenceElapsed += Time.deltaTime;
            switch (selectedPreview)
            {
                case VfxLabPreviewKind.BlueField:
                    TickFieldOnly();
                    break;
                case VfxLabPreviewKind.BlueImpact:
                    TickImpactOnly();
                    break;
                default:
                    TickFullSequence();
                    break;
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Begin(VfxLabPreviewKind.BlueFull);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Begin(VfxLabPreviewKind.BlueField);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Begin(VfxLabPreviewKind.BlueImpact);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                Begin(selectedPreview);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                loopEnabled = !loopEnabled;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                paused = !paused;
                if (paused)
                {
                    phaseBeforePause = CurrentPhaseLabel;
                }
                else
                {
                    CurrentPhaseLabel = phaseBeforePause;
                }
                ApplyTimeScale();
                CurrentPhaseLabel = paused ? "PAUSED" : CurrentPhaseLabel;
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                playbackSpeed = Mathf.Max(0.125f, playbackSpeed * 0.5f);
                ApplyTimeScale();
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                playbackSpeed = Mathf.Min(2f, playbackSpeed * 2f);
                ApplyTimeScale();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                ClearPreview();
            }
        }

        private void Begin(VfxLabPreviewKind kind)
        {
            StopHandles();
            selectedPreview = kind;
            sequenceElapsed = 0f;
            loopWaitElapsed = 0f;
            running = true;
            loopWaiting = false;
            castPosePlayed = false;
            fieldSpawned = false;
            impactSpawned = false;

            switch (kind)
            {
                case VfxLabPreviewKind.BlueField:
                    CurrentPhaseLabel = "FIELD";
                    previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Release);
                    SpawnField();
                    break;
                case VfxLabPreviewKind.BlueImpact:
                    CurrentPhaseLabel = "IMPACT COLLAPSE";
                    previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Release);
                    SpawnImpact();
                    break;
                default:
                    CurrentPhaseLabel = "ANTICIPATION";
                    previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Anticipation);
                    break;
            }
            phaseBeforePause = CurrentPhaseLabel;
            if (paused)
            {
                CurrentPhaseLabel = "PAUSED";
            }
        }

        private void TickFullSequence()
        {
            if (!castPosePlayed && sequenceElapsed >= FullCastAt)
            {
                castPosePlayed = true;
                CurrentPhaseLabel = "CAST";
                previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Cast);
            }
            if (!fieldSpawned && sequenceElapsed >= FullFieldAt)
            {
                CurrentPhaseLabel = "FIELD / RELEASE";
                previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Release);
                SpawnField();
            }
            if (!impactSpawned && sequenceElapsed >= FullImpactAt)
            {
                CurrentPhaseLabel = "IMPACT COLLAPSE";
                SpawnImpact();
            }
            if (sequenceElapsed >= FullRecoverAt && CurrentPhaseLabel != "RECOVER")
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Recover);
            }
            if (sequenceElapsed >= FullCleanupAt)
            {
                CompletePreview();
            }
        }

        private void TickFieldOnly()
        {
            if (sequenceElapsed >= BlueFieldDuration + 0.12f)
            {
                CompletePreview();
            }
        }

        private void TickImpactOnly()
        {
            if (sequenceElapsed >= 0.40f)
            {
                CompletePreview();
            }
        }

        private void SpawnField()
        {
            fieldSpawned = true;
            if (previewPoint == null)
            {
                CurrentPhaseLabel = "MISSING PREVIEW POINT";
                return;
            }
            fieldHandle = PresentationVfxRuntime.Spawn(
                GojoBluePresentationPreset.CreateFieldRequest(
                    previewPoint,
                    BlueRadius,
                    BlueFieldDuration,
                    PresentationVfxTimePolicy.Scaled
                )
            );
        }

        private void SpawnImpact()
        {
            impactSpawned = true;
            if (previewPoint == null)
            {
                CurrentPhaseLabel = "MISSING PREVIEW POINT";
                return;
            }
            impactHandle = PresentationVfxRuntime.Spawn(
                GojoBluePresentationPreset.CreateImpactRequest(
                    previewPoint.position + Vector3.up * 0.35f,
                    BlueRadius,
                    PresentationVfxTimePolicy.Scaled
                )
            );
        }

        private void CompletePreview()
        {
            StopHandles();
            running = false;
            CurrentPhaseLabel = "IDLE";
            previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Idle);
            if (loopEnabled)
            {
                loopWaiting = true;
                loopWaitElapsed = 0f;
            }
        }

        public void ClearPreview()
        {
            StopHandles();
            running = false;
            loopWaiting = false;
            sequenceElapsed = 0f;
            loopWaitElapsed = 0f;
            phaseBeforePause = "CLEARED";
            CurrentPhaseLabel = paused ? "PAUSED" : phaseBeforePause;
            previewCharacter?.SetTechniqueMotion(VfxLabTechniqueMotion.Idle);
        }

        private void StopHandles()
        {
            fieldHandle.Stop(PresentationVfxStopMode.Immediate);
            impactHandle.Stop(PresentationVfxStopMode.Immediate);
            fieldHandle = default;
            impactHandle = default;
        }

        private void CaptureTimeScale()
        {
            if (capturedTimeScale)
            {
                return;
            }
            originalTimeScale = Time.timeScale;
            originalFixedDeltaTime = Time.fixedDeltaTime;
            capturedTimeScale = true;
        }

        private void ApplyTimeScale()
        {
            if (!capturedTimeScale)
            {
                return;
            }
            float baseScale = originalTimeScale > 0f ? originalTimeScale : 1f;
            Time.timeScale = paused ? 0f : baseScale * playbackSpeed;
            Time.fixedDeltaTime = originalFixedDeltaTime * playbackSpeed;
        }

        private void RestoreTimeScale()
        {
            if (!capturedTimeScale)
            {
                return;
            }
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }

        private void OnDisable()
        {
            StopHandles();
            RestoreTimeScale();
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
        }
    }
}
