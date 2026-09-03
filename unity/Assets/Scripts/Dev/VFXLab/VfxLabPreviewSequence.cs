using System.Collections.Generic;
using JJKGame.Core;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Dev.VFXLab
{
    public enum VfxLabPreviewAction
    {
        None,
        BasicAttack,
        Dodge,
        Blue,
        Red,
        HollowPurple,
        UnlimitedVoid,
        BlueFieldDebug,
        BlueImpactDebug,
    }

    [DisallowMultipleComponent]
    public sealed class VfxLabPreviewSequence : MonoBehaviour
    {
        private enum DomainGestureStep
        {
            None,
            AwaitRightPress,
            AwaitLeftClick,
            AwaitRightRelease,
            Active,
        }

        private const float BlueRadius = 4.5f;
        private const float BlueFieldDuration = 0.95f;
        private const float PreviewAnchorForwardDistance = 4.2f;
        private const float PreviewAnchorHeight = 1f;
        private const float LoopDelay = 0.34f;
        private const float DomainPreviewDuration = 3.2f;

        private readonly List<PresentationVfxHandle> activeHandles =
            new List<PresentationVfxHandle>(8);

        private Transform previewPoint;
        private VfxLabPreviewCharacter previewCharacter;
        private VfxLabPreviewAction selectedAction = VfxLabPreviewAction.Blue;
        private VfxLabPreviewAction activeAction;
        private DomainGestureStep domainGestureStep;
        private Transform travelAnchor;
        private Vector3 travelStart;
        private Vector3 travelEnd;
        private GameObject domainPreviewRoot;
        private UnlimitedVoidProductionVisual domainVisual;
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
        private int sequenceStep;
        private string phaseBeforePause = "IDLE";

        public string SelectedActionLabel => selectedAction switch
        {
            VfxLabPreviewAction.BasicAttack => "BASIC ATTACK COMBO",
            VfxLabPreviewAction.Dodge => "DODGE",
            VfxLabPreviewAction.Red => "CURSED TECHNIQUE REVERSAL: RED",
            VfxLabPreviewAction.HollowPurple => "HOLLOW PURPLE",
            VfxLabPreviewAction.UnlimitedVoid => "UNLIMITED VOID",
            VfxLabPreviewAction.BlueFieldDebug => "BLUE · FIELD ONLY [DEBUG]",
            VfxLabPreviewAction.BlueImpactDebug => "BLUE · IMPACT ONLY [DEBUG]",
            _ => "CURSED TECHNIQUE LAPSE: BLUE",
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

            if (IsWaitingForDomainGesture())
            {
                if (domainGestureStep == DomainGestureStep.AwaitRightRelease)
                {
                    CurrentPhaseLabel = $"DOMAIN SEAL · RELEASE RMB · {sequenceElapsed:0.00}s";
                    sequenceElapsed += Time.deltaTime;
                }
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
                    Begin(selectedAction, selectedAction == VfxLabPreviewAction.UnlimitedVoid);
                }
                return;
            }

            if (!running)
            {
                return;
            }

            sequenceElapsed += Time.deltaTime;
            switch (activeAction)
            {
                case VfxLabPreviewAction.BasicAttack:
                    TickBasicAttack();
                    break;
                case VfxLabPreviewAction.Dodge:
                    TickDodge();
                    break;
                case VfxLabPreviewAction.Blue:
                    TickBlue();
                    break;
                case VfxLabPreviewAction.Red:
                    TickRed();
                    break;
                case VfxLabPreviewAction.HollowPurple:
                    TickHollowPurple();
                    break;
                case VfxLabPreviewAction.UnlimitedVoid:
                    TickUnlimitedVoid();
                    break;
                case VfxLabPreviewAction.BlueFieldDebug:
                    if (sequenceElapsed >= BlueFieldDuration + 0.12f)
                    {
                        CompletePreview();
                    }
                    break;
                case VfxLabPreviewAction.BlueImpactDebug:
                    if (sequenceElapsed >= 0.40f)
                    {
                        CompletePreview();
                    }
                    break;
            }
        }

        private void HandleInput()
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (ProductionCombatInput.CancelPressed)
            {
                CancelPreview("CANCELLED");
                return;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                CancelPreview("HARD CLEARED");
                return;
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                loopEnabled = !loopEnabled;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                SetPaused(!paused);
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

            if (shift && Input.GetKeyDown(CombatInputBindings.Ultimate))
            {
                Begin(selectedAction, selectedAction == VfxLabPreviewAction.UnlimitedVoid);
                return;
            }
            if (shift && Input.GetKeyDown(KeyCode.Alpha2))
            {
                Begin(VfxLabPreviewAction.BlueFieldDebug);
                return;
            }
            if (shift && Input.GetKeyDown(KeyCode.Alpha3))
            {
                Begin(VfxLabPreviewAction.BlueImpactDebug);
                return;
            }
            if (shift && ProductionCombatInput.DomainPressed)
            {
                Begin(VfxLabPreviewAction.UnlimitedVoid, true);
                return;
            }

            if (IsWaitingForDomainGesture())
            {
                HandleDomainGestureInput();
                return;
            }

            if (ProductionCombatInput.BasicAttackPressed)
            {
                Begin(VfxLabPreviewAction.BasicAttack);
            }
            else if (ProductionCombatInput.DodgePressed)
            {
                Begin(VfxLabPreviewAction.Dodge);
            }
            else if (ProductionCombatInput.Skill1Pressed)
            {
                Begin(VfxLabPreviewAction.Blue);
            }
            else if (ProductionCombatInput.Skill2Pressed)
            {
                Begin(VfxLabPreviewAction.Red);
            }
            else if (ProductionCombatInput.UltimatePressed)
            {
                Begin(VfxLabPreviewAction.HollowPurple);
            }
            else if (ProductionCombatInput.DomainPressed)
            {
                Begin(VfxLabPreviewAction.UnlimitedVoid);
            }
        }

        private void Begin(VfxLabPreviewAction action, bool directDomain = false)
        {
            StopPreviewContent();
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
            selectedAction = action;
            activeAction = action;
            sequenceElapsed = 0f;
            loopWaitElapsed = 0f;
            sequenceStep = 0;
            running = true;
            loopWaiting = false;
            domainGestureStep = DomainGestureStep.None;

            switch (action)
            {
                case VfxLabPreviewAction.BasicAttack:
                    CurrentPhaseLabel = "BASIC ATTACK 1";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.BasicAttack1);
                    break;
                case VfxLabPreviewAction.Dodge:
                    CurrentPhaseLabel = "DODGE";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Dodge);
                    break;
                case VfxLabPreviewAction.BlueFieldDebug:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    CurrentPhaseLabel = "FIELD · DEBUG";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                    SpawnBlueField();
                    break;
                case VfxLabPreviewAction.BlueImpactDebug:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    CurrentPhaseLabel = "IMPACT · DEBUG";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                    SpawnBlueImpact();
                    break;
                case VfxLabPreviewAction.Red:
                    PositionPreviewAnchorFromCharacter(6.5f);
                    CaptureTravelPath(0.9f);
                    CurrentPhaseLabel = "ANTICIPATION";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueAnticipation);
                    break;
                case VfxLabPreviewAction.HollowPurple:
                    PositionPreviewAnchorFromCharacter(7f);
                    CaptureTravelPath(1.05f);
                    CurrentPhaseLabel = "ANTICIPATION";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueAnticipation);
                    SpawnPurpleFormation(travelStart);
                    break;
                case VfxLabPreviewAction.UnlimitedVoid:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    BeginUnlimitedVoid(directDomain);
                    break;
                default:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    CurrentPhaseLabel = "ANTICIPATION";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueAnticipation);
                    break;
            }

            phaseBeforePause = CurrentPhaseLabel;
            if (paused)
            {
                CurrentPhaseLabel = "PAUSED";
            }
        }

        private void TickBasicAttack()
        {
            if (sequenceStep == 0 && sequenceElapsed >= 0.10f)
            {
                SpawnBasicHit(1);
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= 0.38f)
            {
                CurrentPhaseLabel = "BASIC ATTACK 2";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.BasicAttack2);
                sequenceStep = 2;
            }
            if (sequenceStep == 2 && sequenceElapsed >= 0.48f)
            {
                SpawnBasicHit(2);
                sequenceStep = 3;
            }
            if (sequenceStep == 3 && sequenceElapsed >= 0.78f)
            {
                CurrentPhaseLabel = "BASIC ATTACK FINISHER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.BasicAttackFinisher);
                sequenceStep = 4;
            }
            if (sequenceStep == 4 && sequenceElapsed >= 0.90f)
            {
                SpawnBasicHit(3);
                sequenceStep = 5;
            }
            if (sequenceStep == 5 && sequenceElapsed >= 1.16f)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
                sequenceStep = 6;
            }
            if (sequenceElapsed >= 1.42f)
            {
                CompletePreview();
            }
        }

        private void TickDodge()
        {
            if (sequenceElapsed >= 0.36f && sequenceStep == 0)
            {
                CurrentPhaseLabel = "DODGE RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
                sequenceStep = 1;
            }
            if (sequenceElapsed >= 0.58f)
            {
                CompletePreview();
            }
        }

        private void TickBlue()
        {
            if (sequenceStep == 0 && sequenceElapsed >= 0.34f)
            {
                CurrentPhaseLabel = "CAST";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueCast);
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= 0.58f)
            {
                CurrentPhaseLabel = "FIELD · RELEASE";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                SpawnBlueField();
                sequenceStep = 2;
            }
            if (sequenceStep == 2 && sequenceElapsed >= 1.30f)
            {
                CurrentPhaseLabel = "IMPACT COLLAPSE";
                SpawnBlueImpact();
                sequenceStep = 3;
            }
            if (sequenceStep == 3 && sequenceElapsed >= 1.58f)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRecover);
                sequenceStep = 4;
            }
            if (sequenceElapsed >= 1.92f)
            {
                CompletePreview();
            }
        }

        private void TickRed()
        {
            if (sequenceStep == 0 && sequenceElapsed >= 0.26f)
            {
                CurrentPhaseLabel = "CAST";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueCast);
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= 0.44f)
            {
                CurrentPhaseLabel = "RED RELEASE";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                SpawnRedRelease();
                sequenceStep = 2;
            }
            if (sequenceStep >= 2 && sequenceStep < 4)
            {
                UpdateTravelAnchor(0.44f, 0.42f);
            }
            if (sequenceStep == 2 && sequenceElapsed >= 0.86f)
            {
                CurrentPhaseLabel = "RED IMPACT";
                SpawnRedImpact();
                sequenceStep = 3;
            }
            if (sequenceStep == 3 && sequenceElapsed >= 1.05f)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRecover);
                sequenceStep = 4;
            }
            if (sequenceElapsed >= 1.36f)
            {
                CompletePreview();
            }
        }

        private void TickHollowPurple()
        {
            if (sequenceStep == 0 && sequenceElapsed >= 0.38f)
            {
                CurrentPhaseLabel = "CAST · FORMATION";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueCast);
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= 0.62f)
            {
                CurrentPhaseLabel = "HOLLOW PURPLE · RELEASE";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                SpawnPurpleRelease();
                sequenceStep = 2;
            }
            if (sequenceStep >= 2 && sequenceStep < 4)
            {
                UpdateTravelAnchor(0.62f, 0.68f);
            }
            if (sequenceStep == 2 && sequenceElapsed >= 1.30f)
            {
                CurrentPhaseLabel = "CULMINATION";
                SpawnPurpleFormation(travelEnd);
                sequenceStep = 3;
            }
            if (sequenceStep == 3 && sequenceElapsed >= 1.48f)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRecover);
                sequenceStep = 4;
            }
            if (sequenceElapsed >= 1.80f)
            {
                CompletePreview();
            }
        }

        private void TickUnlimitedVoid()
        {
            if (domainGestureStep == DomainGestureStep.Active
                && sequenceElapsed >= DomainPreviewDuration)
            {
                CompletePreview();
            }
        }

        private void BeginUnlimitedVoid(bool direct)
        {
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.DomainAnticipation);
            SpawnDomainAnticipation();
            if (direct)
            {
                ActivateUnlimitedVoid();
                return;
            }

            running = false;
            domainGestureStep = DomainGestureStep.AwaitRightPress;
            CurrentPhaseLabel = "DOMAIN READY · PRESS / HOLD RMB";
        }

        private void HandleDomainGestureInput()
        {
            if (domainGestureStep == DomainGestureStep.AwaitRightPress
                && ProductionCombatInput.DomainModifierPressed)
            {
                domainGestureStep = DomainGestureStep.AwaitLeftClick;
                CurrentPhaseLabel = "DOMAIN SEAL · HOLD RMB + CLICK LMB";
                return;
            }

            if (domainGestureStep == DomainGestureStep.AwaitLeftClick)
            {
                if (ProductionCombatInput.DomainModifierReleased)
                {
                    domainGestureStep = DomainGestureStep.AwaitRightPress;
                    CurrentPhaseLabel = "DOMAIN INPUT RESET · HOLD RMB";
                    return;
                }
                if (ProductionCombatInput.BasicAttackPressed
                    && ProductionCombatInput.DomainModifierHeld)
                {
                    domainGestureStep = DomainGestureStep.AwaitRightRelease;
                    sequenceElapsed = 0f;
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.DomainRelease);
                    CurrentPhaseLabel = "DOMAIN SEAL · RELEASE RMB";
                }
                return;
            }

            if (domainGestureStep == DomainGestureStep.AwaitRightRelease
                && ProductionCombatInput.DomainModifierReleased)
            {
                ActivateUnlimitedVoid();
            }
        }

        private bool IsWaitingForDomainGesture()
        {
            return activeAction == VfxLabPreviewAction.UnlimitedVoid
                && domainGestureStep != DomainGestureStep.None
                && domainGestureStep != DomainGestureStep.Active;
        }

        private void ActivateUnlimitedVoid()
        {
            StopHandles();
            sequenceElapsed = 0f;
            running = true;
            domainGestureStep = DomainGestureStep.Active;
            CurrentPhaseLabel = "DOMAIN ACTIVE · UNLIMITED VOID";
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.DomainRelease);

            domainPreviewRoot = new GameObject("VFXLab_UnlimitedVoidPreview");
            domainPreviewRoot.SetActive(false);
            domainPreviewRoot.transform.SetParent(transform, true);
            if (previewCharacter != null)
            {
                domainPreviewRoot.transform.SetPositionAndRotation(
                    previewCharacter.transform.position,
                    previewCharacter.transform.rotation
                );
            }
            domainVisual = domainPreviewRoot.AddComponent<UnlimitedVoidProductionVisual>();
            domainVisual.Configure(30f);
            domainPreviewRoot.SetActive(true);
            domainVisual.enabled = !paused;
        }

        private void PositionPreviewAnchorFromCharacter(float forwardDistance)
        {
            if (previewPoint == null || previewCharacter == null)
            {
                return;
            }

            Transform characterTransform = previewCharacter.transform;
            Vector3 forward = ResolveCharacterForward();
            previewPoint.position = characterTransform.position
                + forward * forwardDistance
                + Vector3.up * PreviewAnchorHeight;
            previewPoint.rotation = Quaternion.LookRotation(forward, Vector3.up);
            previewCharacter.SetTechniqueAnchor(previewPoint.position);
        }

        private void CaptureTravelPath(float startForwardDistance)
        {
            if (previewCharacter == null)
            {
                travelStart = Vector3.up;
                travelEnd = previewPoint != null ? previewPoint.position : Vector3.forward * 6f;
                return;
            }
            Vector3 forward = ResolveCharacterForward();
            travelStart = previewCharacter.transform.position
                + forward * startForwardDistance
                + Vector3.up;
            travelEnd = previewPoint != null
                ? previewPoint.position
                : travelStart + forward * 5f;
        }

        private Vector3 ResolveCharacterForward()
        {
            Vector3 forward = previewCharacter != null
                ? previewCharacter.transform.forward
                : Vector3.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        private Transform CreateTravelAnchor()
        {
            if (travelAnchor != null)
            {
                travelAnchor.gameObject.SetActive(false);
                Destroy(travelAnchor.gameObject);
            }
            travelAnchor = new GameObject("VFXLabTravelAnchor").transform;
            travelAnchor.SetParent(transform, true);
            travelAnchor.SetPositionAndRotation(
                travelStart,
                Quaternion.LookRotation(ResolveCharacterForward(), Vector3.up)
            );
            return travelAnchor;
        }

        private void UpdateTravelAnchor(float travelBeginsAt, float travelDuration)
        {
            if (travelAnchor == null)
            {
                return;
            }
            float progress = Mathf.Clamp01(
                (sequenceElapsed - travelBeginsAt) / Mathf.Max(0.01f, travelDuration)
            );
            travelAnchor.position = Vector3.Lerp(travelStart, travelEnd, progress);
        }

        private void SpawnBasicHit(int step)
        {
            Vector3 contactPoint = previewCharacter != null
                ? previewCharacter.transform.position
                    + ResolveCharacterForward() * (1.05f + step * 0.12f)
                    + Vector3.up * 0.88f
                : Vector3.up;
            PresentationVfxStyleId style = step switch
            {
                1 => PresentationVfxStyleId.BasicHit1,
                2 => PresentationVfxStyleId.BasicHit2,
                _ => PresentationVfxStyleId.BasicHitFinisher,
            };
            Track(
                PresentationVfxRuntime.Spawn(
                    PresentationVfxSpawnRequest.AtWorld(
                        contactPoint,
                        step >= 3
                            ? new Color(1f, 0.72f, 0.24f, 0.92f)
                            : new Color(0.72f, 0.90f, 1f, 0.84f),
                        Color.white,
                        0.08f,
                        step >= 3 ? 1.15f : 0.72f,
                        step >= 3 ? 0.20f : 0.13f,
                        0f,
                        PresentationVfxTimePolicy.Scaled,
                        style,
                        ResolveCharacterForward()
                    )
                )
            );
        }

        private void SpawnBlueField()
        {
            if (previewPoint == null)
            {
                CurrentPhaseLabel = "MISSING PREVIEW POINT";
                return;
            }
            Track(PresentationVfxRuntime.Spawn(
                GojoBluePresentationPreset.CreateFieldRequest(
                    previewPoint,
                    BlueRadius,
                    BlueFieldDuration,
                    PresentationVfxTimePolicy.Scaled
                )
            ));
        }

        private void SpawnBlueImpact()
        {
            if (previewPoint == null)
            {
                CurrentPhaseLabel = "MISSING PREVIEW POINT";
                return;
            }
            Track(PresentationVfxRuntime.Spawn(
                GojoBluePresentationPreset.CreateImpactRequest(
                    previewPoint.position + Vector3.up * 0.35f,
                    BlueRadius,
                    PresentationVfxTimePolicy.Scaled
                )
            ));
        }

        private void SpawnRedRelease()
        {
            Transform anchor = CreateTravelAnchor();
            Track(PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.Follow(
                    anchor, Vector3.zero,
                    new Color(0.84f, 0.015f, 0.025f, 0.94f),
                    new Color(1f, 0.26f, 0.04f, 0.76f),
                    0.48f, 3.06f, 0.58f, 0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.GojoRed,
                    ResolveCharacterForward()
                )
            ));
        }

        private void SpawnRedImpact()
        {
            Track(PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.AtWorld(
                    travelEnd,
                    new Color(0.88f, 0.015f, 0.025f, 0.92f),
                    new Color(1f, 0.30f, 0.04f, 0.74f),
                    0.34f, 4.08f, 0.26f, 0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.GojoRed,
                    ResolveCharacterForward()
                )
            ));
        }

        private void SpawnPurpleFormation(Vector3 position)
        {
            Track(PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.AtWorld(
                    position,
                    new Color(0.58f, 0.025f, 1f, 0.96f),
                    new Color(0.95f, 0.32f, 1f, 0.82f),
                    0.18f, 1.6f, 0.48f, 0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.HollowPurpleFormation,
                    ResolveCharacterForward()
                )
            ));
        }

        private void SpawnPurpleRelease()
        {
            Transform anchor = CreateTravelAnchor();
            Track(PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.Follow(
                    anchor, Vector3.zero,
                    new Color(0.72f, 0.08f, 1f, 0.92f),
                    new Color(1f, 0.18f, 0.72f, 0.82f),
                    0.40f, 2.2f, 0.86f, 0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.HollowPurpleRelease,
                    ResolveCharacterForward()
                )
            ));
        }

        private void SpawnDomainAnticipation()
        {
            if (previewCharacter == null)
            {
                return;
            }
            Track(PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.Follow(
                    previewCharacter.transform,
                    Vector3.up * 1.05f,
                    new Color(0.18f, 0.64f, 1f, 0.70f),
                    new Color(0.58f, 0.42f, 1f, 0.55f),
                    0.18f, 2.8f, 2.6f, 0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.UnlimitedVoidAnticipation,
                    ResolveCharacterForward()
                )
            ));
        }

        private void Track(PresentationVfxHandle handle)
        {
            if (handle.IsValid)
            {
                activeHandles.Add(handle);
            }
        }

        private void CompletePreview()
        {
            StopPreviewContent();
            running = false;
            activeAction = VfxLabPreviewAction.None;
            CurrentPhaseLabel = "IDLE";
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
            if (loopEnabled)
            {
                loopWaiting = true;
                loopWaitElapsed = 0f;
            }
        }

        private void CancelPreview(string phase)
        {
            StopPreviewContent();
            running = false;
            loopWaiting = false;
            activeAction = VfxLabPreviewAction.None;
            domainGestureStep = DomainGestureStep.None;
            sequenceElapsed = 0f;
            loopWaitElapsed = 0f;
            phaseBeforePause = phase;
            CurrentPhaseLabel = paused ? "PAUSED" : phase;
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
        }

        private void StopPreviewContent()
        {
            StopHandles();
            domainGestureStep = DomainGestureStep.None;
            if (travelAnchor != null)
            {
                travelAnchor.gameObject.SetActive(false);
                Destroy(travelAnchor.gameObject);
                travelAnchor = null;
            }
            if (domainPreviewRoot != null)
            {
                domainPreviewRoot.SetActive(false);
                Destroy(domainPreviewRoot);
                domainPreviewRoot = null;
                domainVisual = null;
            }
        }

        private void StopHandles()
        {
            foreach (PresentationVfxHandle handle in activeHandles)
            {
                handle.Stop(PresentationVfxStopMode.Immediate);
            }
            activeHandles.Clear();
        }

        private void SetPaused(bool value)
        {
            paused = value;
            if (paused)
            {
                phaseBeforePause = CurrentPhaseLabel;
            }
            else
            {
                CurrentPhaseLabel = phaseBeforePause;
            }
            if (domainVisual != null)
            {
                domainVisual.enabled = !paused;
            }
            ApplyTimeScale();
            CurrentPhaseLabel = paused ? "PAUSED" : CurrentPhaseLabel;
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
            StopPreviewContent();
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
            RestoreTimeScale();
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
        }
    }
}
