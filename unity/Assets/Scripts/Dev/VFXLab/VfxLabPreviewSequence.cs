using System.Collections.Generic;
using JJKGame.Core;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Dev.VFXLab
{
    public enum VfxLabPreviewAction
    {
        None = 0,
        [InspectorName("평타")] BasicAttack = 1,
        [InspectorName("회피")] Dodge = 2,
        [InspectorName("기술 1")] Skill1 = 3,
        Blue = Skill1,
        [InspectorName("기술 2")] Skill2 = 4,
        Red = Skill2,
        [InspectorName("필살기")] Ultimate = 5,
        HollowPurple = Ultimate,
        [InspectorName("영역")] Domain = 6,
        UnlimitedVoid = Domain,
        BlueFieldDebug = 7,
        BlueImpactDebug = 8,
    }

    [DisallowMultipleComponent]
    public sealed class VfxLabPreviewSequence : MonoBehaviour
    {
        private const float BlueRadius = 4.5f;
        private const float BlueFieldDuration = 2.20f;
        private const float BasicComboResetDelay = 0.9f;
        private const float PreviewAnchorForwardDistance = 4.2f;
        private const float PreviewAnchorHeight = 1f;
        private const float LoopDelay = 0.34f;
        private const float DomainAnticipationDuration = 0.34f;
        [SerializeField, InspectorName("영역 유지시간"), Min(.1f), Tooltip("도착 연출 뒤 영역 프리뷰가 유지되는 시간입니다.")]
        private float domainActiveDuration = 10f;
        private float DomainPreviewDuration => domainPresentation.ArrivalDuration(false) + domainActiveDuration;

        private readonly List<PresentationVfxHandle> activeHandles =
            new List<PresentationVfxHandle>(8);

        private Transform previewPoint;
        private VfxLabPreviewCharacter previewCharacter;
        private PrototypeCombatAudio previewAudio;
        private VfxLabPreviewAction selectedAction = VfxLabPreviewAction.Skill1;
        private VfxLabPreviewAction activeAction;
        private PrototypeHollowPurplePresentationRuntime.OrbSequence hollowPurpleSequence;
        private Transform travelAnchor;
        private Vector3 travelStart;
        private Vector3 travelEnd;
        private GameObject domainPreviewRoot;
        private UnlimitedVoidProductionVisual domainVisual;
        [SerializeField, InspectorName("영역 연출 설정"), Tooltip("VFXLab 영역 프리뷰의 결계/내부 공간/시네마틱 설정입니다.")]
        private DomainPresentationSettings domainPresentation = new DomainPresentationSettings();
        private DomainPresentationSession domainSession;
        private TechniqueChargeVisual chargeVisual;
        private float sequenceElapsed;
        private float hollowPurpleClock;
        private float basicComboExpiresAt;
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
        private int basicAttackStep = 1;
        private int nextBasicAttackStep = 1;
        private string phaseBeforePause = "IDLE";

        public string SelectedActionLabel => GetSelectedActionLabel();

        public string CurrentPhaseLabel { get; private set; } = "IDLE";
        public bool LoopEnabled => loopEnabled;
        public bool Paused => paused;
        public float PlaybackSpeed => playbackSpeed;
        public bool RuntimeReady => PresentationVfxRuntime.HasRuntime;
        public bool IsInsideDomain => domainSession != null && domainSession.IsEntered;

        public void Configure(
            Transform newPreviewPoint,
            VfxLabPreviewCharacter newPreviewCharacter
        )
        {
            previewPoint = newPreviewPoint;
            previewCharacter = newPreviewCharacter;
            GetPreviewAudio()?.SetPresentationProfile(previewCharacter != null
                ? previewCharacter.AudioProfile
                : null);
        }

        private string GetSelectedActionLabel()
        {
            if (selectedAction == VfxLabPreviewAction.BasicAttack)
                return $"BASIC ATTACK · NEXT {nextBasicAttackStep}";
            if (selectedAction == VfxLabPreviewAction.Dodge) return "DODGE";
            if (selectedAction == VfxLabPreviewAction.BlueFieldDebug) return "BLUE · FIELD ONLY [DEBUG]";
            if (selectedAction == VfxLabPreviewAction.BlueImpactDebug) return "BLUE · IMPACT ONLY [DEBUG]";

            CharacterPresentationProfile profile = previewCharacter != null
                ? previewCharacter.PresentationProfile
                : CharacterPresentationProfiles.Get(PrototypeCharacterId.GojoModern);
            if (profile.CharacterId == PrototypeCharacterId.GojoModern)
            {
                return selectedAction switch
                {
                    VfxLabPreviewAction.Skill2 => "CURSED TECHNIQUE REVERSAL: RED",
                    VfxLabPreviewAction.Ultimate => "HOLLOW PURPLE",
                    VfxLabPreviewAction.Domain => "UNLIMITED VOID",
                    _ => "CURSED TECHNIQUE LAPSE: BLUE",
                };
            }

            return selectedAction switch
            {
                VfxLabPreviewAction.Skill2 => $"SKILL 2 · {profile.Skill2.Label}",
                VfxLabPreviewAction.Ultimate => $"ULTIMATE · {profile.Ultimate.Label}",
                VfxLabPreviewAction.Domain => $"DOMAIN · {profile.Domain.Label}",
                _ => $"SKILL 1 · {profile.Skill1.Label}",
            };
        }

        private bool SupportsBuiltInTechniquePreview => previewCharacter == null
            || previewCharacter.CharacterId == PrototypeCharacterId.GojoModern;

        private static bool IsTechniqueSlot(VfxLabPreviewAction action)
        {
            return action == VfxLabPreviewAction.Skill1
                || action == VfxLabPreviewAction.Skill2
                || action == VfxLabPreviewAction.Ultimate
                || action == VfxLabPreviewAction.Domain;
        }

        private void OnEnable()
        {
            CaptureTimeScale();
            ApplyTimeScale();
        }

        private void Update()
        {
            ResetExpiredBasicCombo();
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
                    Begin(selectedAction);
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
                case VfxLabPreviewAction.Skill1:
                    TickBlue();
                    break;
                case VfxLabPreviewAction.Skill2:
                    TickRed();
                    break;
                case VfxLabPreviewAction.Ultimate:
                    TickHollowPurple();
                    break;
                case VfxLabPreviewAction.Domain:
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
            bool shift = ProductionCombatInput.RunHeld;

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
                Begin(selectedAction);
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
            if (ProductionCombatInput.BasicAttackPressed)
            {
                BeginBasicAttackStep();
            }
            else if (ProductionCombatInput.DodgePressed)
            {
                Begin(VfxLabPreviewAction.Dodge);
            }
            else if (ProductionCombatInput.Skill1Pressed)
            {
                Begin(VfxLabPreviewAction.Skill1);
            }
            else if (ProductionCombatInput.Skill2Pressed)
            {
                Begin(VfxLabPreviewAction.Skill2);
            }
            else if (ProductionCombatInput.UltimatePressed)
            {
                Begin(VfxLabPreviewAction.Ultimate);
            }
            else if (ProductionCombatInput.DomainPressed)
            {
                Begin(VfxLabPreviewAction.Domain);
            }
        }

        private void BeginBasicAttackStep()
        {
            ResetExpiredBasicCombo();
            basicAttackStep = nextBasicAttackStep;
            nextBasicAttackStep = basicAttackStep >= 3 ? 1 : basicAttackStep + 1;
            basicComboExpiresAt = Time.time + BasicComboResetDelay;
            Begin(VfxLabPreviewAction.BasicAttack);
        }

        private void ResetExpiredBasicCombo()
        {
            if (nextBasicAttackStep != 1 && Time.time > basicComboExpiresAt)
            {
                nextBasicAttackStep = 1;
            }
        }

        private static VfxLabPreviewMotion GetBasicAttackMotion(int step)
        {
            return step switch
            {
                2 => VfxLabPreviewMotion.BasicAttack2,
                3 => VfxLabPreviewMotion.BasicAttackFinisher,
                _ => VfxLabPreviewMotion.BasicAttack1,
            };
        }

        private void Begin(VfxLabPreviewAction action)
        {
            if (IsTechniqueSlot(action) && !SupportsBuiltInTechniquePreview)
            {
                StopPreviewContent();
                selectedAction = action;
                activeAction = VfxLabPreviewAction.None;
                running = false;
                CurrentPhaseLabel = "PREVIEW ADAPTER REQUIRED";
                return;
            }

            StopPreviewContent();
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
            selectedAction = action;
            activeAction = action;
            sequenceElapsed = 0f;
            loopWaitElapsed = 0f;
            sequenceStep = 0;
            running = true;
            loopWaiting = false;

            switch (action)
            {
                case VfxLabPreviewAction.BasicAttack:
                    CurrentPhaseLabel = $"BASIC ATTACK {basicAttackStep}";
                    previewCharacter?.SetPreviewMotion(GetBasicAttackMotion(basicAttackStep));
                    GetPreviewAudio()?.PlayBasicSwingRuntime(basicAttackStep);
                    break;
                case VfxLabPreviewAction.Dodge:
                    CurrentPhaseLabel = "DODGE";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Dodge);
                    GetPreviewAudio()?.PlayDodgeRuntime();
                    break;
                case VfxLabPreviewAction.BlueFieldDebug:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    CurrentPhaseLabel = "FIELD · DEBUG";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                    GetPreviewAudio()?.PlayBlueCastRuntime();
                    SpawnBlueField();
                    break;
                case VfxLabPreviewAction.BlueImpactDebug:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    CurrentPhaseLabel = "IMPACT · DEBUG";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                    GetPreviewAudio()?.PlayBlueImpactRuntime();
                    SpawnBlueImpact();
                    break;
                case VfxLabPreviewAction.Skill2:
                    chargeVisual = TechniqueChargeVisual.Spawn(previewCharacter != null ? previewCharacter.transform : transform, false, 0.44f);
                    PositionPreviewAnchorFromCharacter(
                        GojoRedProductionDefaults.PreviewEndForwardDistance
                    );
                    CaptureTravelPath(GojoRedProductionDefaults.SpawnForwardOffset);
                    CurrentPhaseLabel = "ANTICIPATION";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueAnticipation);
                    break;
                case VfxLabPreviewAction.Ultimate:
                    PositionPreviewAnchorFromCharacter(7f);
                    CurrentPhaseLabel = "ANTICIPATION";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueAnticipation);
                    break;
                case VfxLabPreviewAction.Domain:
                    PositionPreviewAnchorFromCharacter(PreviewAnchorForwardDistance);
                    BeginUnlimitedVoid();
                    break;
                default:
                    if (action == VfxLabPreviewAction.Skill1)
                        chargeVisual = TechniqueChargeVisual.Spawn(previewCharacter != null ? previewCharacter.transform : transform, true, 0.58f);
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
                SpawnBasicHit(basicAttackStep);
                sequenceStep = 1;
            }
            float recoverAt = basicAttackStep >= 3 ? 0.34f : 0.24f;
            float completeAt = basicAttackStep >= 3 ? 0.52f : 0.38f;
            if (sequenceStep == 1 && sequenceElapsed >= recoverAt)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
                sequenceStep = 2;
            }
            if (sequenceElapsed >= completeAt)
            {
                CompletePreview();
            }
        }

        private void TickDodge()
        {
            if (previewCharacter != null && previewCharacter.IsEvadeRecovering && sequenceStep == 0)
            {
                CurrentPhaseLabel = "DODGE RECOVER";
                sequenceStep = 1;
            }
            if (previewCharacter == null || !previewCharacter.IsEvading)
            {
                CompletePreview();
            }
        }

        private void TickBlue()
        {
            const float castAt = 0.34f;
            const float releaseAt = 0.58f;
            const float recoverDelayAfterImpact = 0.28f;
            const float completeDelayAfterImpact = 0.62f;
            float impactAt = releaseAt + BlueFieldDuration;

            if (sequenceStep == 0 && sequenceElapsed >= castAt)
            {
                CurrentPhaseLabel = "CAST";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueCast);
                GetPreviewAudio()?.PlayBlueCastRuntime();
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= releaseAt)
            {
                CurrentPhaseLabel = "FIELD · RELEASE";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                SpawnBlueField();
                sequenceStep = 2;
            }
            if (sequenceStep == 2 && sequenceElapsed >= impactAt)
            {
                CurrentPhaseLabel = "IMPACT COLLAPSE";
                GetPreviewAudio()?.PlayBlueImpactRuntime();
                SpawnBlueImpact();
                sequenceStep = 3;
            }
            if (sequenceStep == 3
                && sequenceElapsed >= impactAt + recoverDelayAfterImpact)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRecover);
                sequenceStep = 4;
            }
            if (sequenceElapsed >= impactAt + completeDelayAfterImpact)
            {
                CompletePreview();
            }
        }

        private void TickRed()
        {
            const float releaseAt = 0.44f;
            const float recoverDelayAfterImpact = 0.19f;
            float completeDelayAfterImpact = .35f + GojoPolishSettings.Current.redAftermathDuration;
            float travelDuration = GojoRedProductionDefaults.TravelDuration;
            float impactAt = releaseAt + travelDuration;

            if (sequenceStep == 0 && sequenceElapsed >= 0.26f)
            {
                CurrentPhaseLabel = "CAST";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueCast);
                GetPreviewAudio()?.PlayRedCastRuntime();
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= releaseAt)
            {
                CurrentPhaseLabel = "RED RELEASE";
                if (chargeVisual != null) chargeVisual.Consume();
                chargeVisual = null;
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                SpawnRedRelease();
                sequenceStep = 2;
            }
            if (sequenceStep >= 2 && sequenceStep < 4)
            {
                UpdateTravelAnchor(releaseAt, travelDuration);
            }
            if (sequenceStep == 2 && sequenceElapsed >= impactAt)
            {
                CurrentPhaseLabel = "RED IMPACT";
                GetPreviewAudio()?.PlayRedImpactRuntime(true);
                SpawnRedImpact();
                sequenceStep = 3;
            }
            if (sequenceStep == 3
                && sequenceElapsed >= impactAt + recoverDelayAfterImpact)
            {
                CurrentPhaseLabel = "RECOVER";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRecover);
                sequenceStep = 4;
            }
            if (sequenceElapsed >= impactAt + completeDelayAfterImpact)
            {
                CompletePreview();
            }
        }

        private void TickHollowPurple()
        {
            if (sequenceStep == 0 && sequenceElapsed >= 0.38f)
            {
                CurrentPhaseLabel = "CAST · BLUE / RED FORMATION";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueCast);
                sequenceStep = 1;
            }
            if (sequenceStep == 1 && sequenceElapsed >= 0.62f)
            {
                CurrentPhaseLabel = "HOLLOW PURPLE · MERGE";
                previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRelease);
                GetPreviewAudio()?.PlayPurpleRuntime();
                SpawnCanonicalHollowPurple();
                sequenceStep = hollowPurpleSequence != null ? 2 : 3;
            }
            if (sequenceStep == 2 && hollowPurpleSequence != null)
            {
                float deltaTime = Time.deltaTime;
                hollowPurpleClock += deltaTime;
                CurrentPhaseLabel = hollowPurpleClock < .24f ? "HOLLOW PURPLE · MERGE"
                    : hollowPurpleClock < .24f + GojoPolishSettings.Current.purpleFusionHoldDuration
                        ? "FUSION COMPLETE · HOLD" : "HOLLOW PURPLE · TRAVEL / RESIDUE";
                if (!hollowPurpleSequence.Update(hollowPurpleClock, deltaTime))
                {
                    hollowPurpleSequence.Dispose();
                    hollowPurpleSequence = null;
                    CurrentPhaseLabel = "RECOVER";
                    previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.TechniqueRecover);
                    sequenceStep = 3;
                }
            }
            if (sequenceStep == 3 && sequenceElapsed >= 1.92f)
            {
                CompletePreview();
            }
        }

        private void TickUnlimitedVoid()
        {
            if (domainVisual != null) CurrentPhaseLabel = domainVisual.PhaseLabel;
            if (sequenceStep == 0 && sequenceElapsed >= DomainAnticipationDuration)
            {
                ActivateUnlimitedVoid();
                sequenceStep = 1;
            }
            if (sequenceStep == 1
                && sequenceElapsed >= DomainAnticipationDuration + DomainPreviewDuration)
            {
                CompletePreview();
            }
        }

        private void BeginUnlimitedVoid()
        {
            CurrentPhaseLabel = "DOMAIN ANTICIPATION";
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.DomainAnticipation);
        }

        private void ActivateUnlimitedVoid()
        {
            StopHandles();
            CurrentPhaseLabel = "DOMAIN ACTIVE · UNLIMITED VOID";
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.DomainRelease);
            GetPreviewAudio()?.PlayDomainRuntime();

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
            domainVisual.Configure(domainPresentation);
            domainPreviewRoot.SetActive(true);
            domainVisual.Paused = paused;
            domainSession = domainPreviewRoot.AddComponent<DomainPresentationSession>();
            domainSession.Begin(previewCharacter != null ? previewCharacter.transform : transform,
                System.Array.Empty<Transform>(), domainVisual, domainPresentation, DomainPreviewDuration, previewPoint);
            domainSession.Paused = paused;
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
            GetPreviewAudio()?.PlayBasicHitRuntime(step);
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
                GojoRedPresentationPreset.CreateReleaseRequest(
                    anchor,
                    GojoRedProductionDefaults.Radius,
                    GojoRedProductionDefaults.Range,
                    GojoRedProductionDefaults.ProjectileSpeed,
                    ResolveCharacterForward()
                )
            ));
        }

        private void SpawnRedImpact()
        {
            Track(PresentationVfxRuntime.Spawn(
                GojoRedPresentationPreset.CreateImpactRequest(
                    travelEnd,
                    GojoRedProductionDefaults.Radius,
                    ResolveCharacterForward()
                )
            ));
        }

        private void SpawnCanonicalHollowPurple()
        {
            if (previewCharacter == null)
            {
                CurrentPhaseLabel = "MISSING PREVIEW CHARACTER";
                return;
            }
            hollowPurpleClock = 0f;
            hollowPurpleSequence =
                PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(
                    transform,
                    previewCharacter.transform,
                    hollowPurpleClock
                );
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
            sequenceElapsed = 0f;
            loopWaitElapsed = 0f;
            phaseBeforePause = phase;
            CurrentPhaseLabel = paused ? "PAUSED" : phase;
            previewCharacter?.SetPreviewMotion(VfxLabPreviewMotion.Idle);
        }

        private void StopPreviewContent()
        {
            previewAudio?.StopTransientRuntimePlayback();
            StopHandles();
            if (hollowPurpleSequence != null)
            {
                hollowPurpleSequence.Dispose();
                hollowPurpleSequence = null;
            }
            hollowPurpleClock = 0f;
            if (travelAnchor != null)
            {
                travelAnchor.gameObject.SetActive(false);
                Destroy(travelAnchor.gameObject);
                travelAnchor = null;
            }
            if (domainPreviewRoot != null)
            {
                if (domainSession != null) domainSession.End();
                domainSession = null;
                domainPreviewRoot.SetActive(false);
                Destroy(domainPreviewRoot);
                domainPreviewRoot = null;
                domainVisual = null;
            }
        }

        private void StopHandles()
        {
            if (chargeVisual != null) Destroy(chargeVisual.gameObject);
            chargeVisual = null;
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
                domainVisual.Paused = paused;
            }
            if (domainSession != null) domainSession.Paused = paused;
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

        private PrototypeCombatAudio GetPreviewAudio()
        {
            previewAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);
            return previewAudio;
        }
    }
}
