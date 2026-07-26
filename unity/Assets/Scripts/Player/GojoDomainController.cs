using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class GojoDomainController : MonoBehaviour
    {
        public enum DomainState
        {
            Normal,
            DomainReady,
            WaitLeftClick,
            ReleaseTiming,
            Active,
            Failed,
        }

        [Header("Command Timing")]
        [SerializeField, Min(0.1f)] private float domainReadyTimeout = 3f;
        [SerializeField, Min(0.05f)] private float rightToLeftTimeout = 0.65f;
        [SerializeField, Min(0.05f)] private float targetReleaseTime = 0.90f;
        [SerializeField, Min(0.01f)] private float releaseTolerance = 0.22f;
        [SerializeField, Min(0.1f)] private float failedDuration = 1.2f;

        [Header("Unlimited Void")]
        [SerializeField, Min(0.1f)] private float domainDuration = 3f;
        [SerializeField, Min(0.1f)] private float domainRadius = 30f;
        [SerializeField, Min(0f)] private float domainEnergyCost = 60f;
        [SerializeField] private GameObject domainVisualRoot;

        private GojoTechniqueController techniqueController;
        private CursedEnergyController cursedEnergy;
        private CombatActionGate actionGate;

        public DomainState State { get; private set; } = DomainState.Normal;
        public string StatusText { get; private set; } = "V 키로 영역전개를 준비하세요";
        public bool CapturesMouseInput =>
            State == DomainState.DomainReady
            || State == DomainState.WaitLeftClick
            || State == DomainState.ReleaseTiming;

        public bool IsReleaseTiming => State == DomainState.ReleaseTiming;
        public float ReleaseElapsed =>
            IsReleaseTiming ? Mathf.Max(0f, Time.time - leftClickedAt) : 0f;
        public float ReleaseTimelineDuration =>
            Mathf.Max(0.1f, targetReleaseTime + releaseTolerance + 0.25f);
        public float ReleaseProgressNormalized =>
            Mathf.Clamp01(ReleaseElapsed / ReleaseTimelineDuration);
        public float ReleaseWindowStartNormalized =>
            Mathf.Clamp01((targetReleaseTime - releaseTolerance) / ReleaseTimelineDuration);
        public float ReleaseWindowEndNormalized =>
            Mathf.Clamp01((targetReleaseTime + releaseTolerance) / ReleaseTimelineDuration);
        public float DomainEnergyCost => domainEnergyCost;

        private float stateStartedAt;
        private float rightPressedAt;
        private float leftClickedAt;

        private void Awake()
        {
            EnsureTechniqueControllers();
            techniqueController = GetComponent<GojoTechniqueController>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            actionGate = CombatActionGate.GetOrCreate(gameObject);
            EnsureRuntimeVisual();
            SetDomainVisual(false);
            ResetCommand();
        }

        private void Update()
        {
            if (Input.GetKeyDown(CombatInputBindings.Domain))
            {
                RequestDomain();
            }

            if (Input.GetKeyDown(CombatInputBindings.CancelCommand))
            {
                ResetCommand();
            }

            HandleSealInput();
            UpdateStateTimeouts();
        }

        public void RequestDomain()
        {
            if (State != DomainState.Normal)
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate != null && !actionGate.CanStartDomain)
            {
                StatusText = "현재 행동 중에는 영역전개 입력을 시작할 수 없습니다";
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (cursedEnergy != null && !cursedEnergy.CanSpend(domainEnergyCost))
            {
                cursedEnergy.NotifyInsufficient("무량공처", domainEnergyCost);
                StatusText = $"주력 부족: 무량공처에는 {domainEnergyCost:0} 필요";
                return;
            }

            ChangeState(
                DomainState.DomainReady,
                "영역 준비: 마우스 오른쪽 버튼을 누르고 유지하세요"
            );
        }

        public void ResetCommand()
        {
            State = DomainState.Normal;
            stateStartedAt = Time.time;
            rightPressedAt = 0f;
            leftClickedAt = 0f;
            StatusText =
                $"{CombatInputBindings.DomainLabel} 키로 영역전개 준비 · 주력 {domainEnergyCost:0} · "
                + $"{CombatInputBindings.CancelCommandLabel} 입력 취소";
            SetDomainVisual(false);
        }

        private void HandleSealInput()
        {
            if (State == DomainState.DomainReady && Input.GetMouseButtonDown(1))
            {
                rightPressedAt = Time.time;
                ChangeState(
                    DomainState.WaitLeftClick,
                    "오른쪽 버튼을 유지한 채 왼쪽 버튼을 클릭하세요"
                );
                return;
            }

            if (State == DomainState.WaitLeftClick)
            {
                if (Input.GetMouseButtonUp(1))
                {
                    Fail("실패: 왼쪽 클릭 전에 오른쪽 버튼을 놓았습니다");
                    return;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (!Input.GetMouseButton(1))
                    {
                        Fail("실패: 오른쪽 버튼을 유지하지 않았습니다");
                        return;
                    }

                    if (Time.time - rightPressedAt > rightToLeftTimeout)
                    {
                        Fail("실패: 왼쪽 클릭이 너무 늦었습니다");
                        return;
                    }

                    leftClickedAt = Time.time;
                    ChangeState(
                        DomainState.ReleaseTiming,
                        "초록색 타이밍 구간에서 오른쪽 버튼을 놓으세요"
                    );
                }

                return;
            }

            if (State == DomainState.ReleaseTiming && Input.GetMouseButtonUp(1))
            {
                float releaseTime = Time.time - leftClickedAt;
                float error = Mathf.Abs(releaseTime - targetReleaseTime);

                if (error <= releaseTolerance)
                {
                    ActivateDomain();
                }
                else
                {
                    Fail($"타이밍 실패: {releaseTime:0.00}초에 버튼을 놓았습니다");
                }
            }
        }

        private void UpdateStateTimeouts()
        {
            float elapsed = Time.time - stateStartedAt;

            if (State == DomainState.DomainReady && elapsed > domainReadyTimeout)
            {
                Fail("실패: 장인 입력 제한시간을 초과했습니다");
            }
            else if (
                State == DomainState.WaitLeftClick
                && Time.time - rightPressedAt > rightToLeftTimeout
            )
            {
                Fail("실패: 왼쪽 클릭이 너무 늦었습니다");
            }
            else if (
                State == DomainState.ReleaseTiming
                && Time.time - leftClickedAt > targetReleaseTime + releaseTolerance + 0.5f
            )
            {
                Fail("실패: 오른쪽 버튼을 너무 늦게 놓았습니다");
            }
            else if (State == DomainState.Active && elapsed > domainDuration)
            {
                ResetCommand();
            }
            else if (State == DomainState.Failed && elapsed > failedDuration)
            {
                ResetCommand();
            }
        }

        private void ActivateDomain()
        {
            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (
                cursedEnergy != null
                && !cursedEnergy.TrySpend(domainEnergyCost, "무량공처")
            )
            {
                Fail("실패: 영역전개에 필요한 주력이 부족합니다");
                return;
            }

            ChangeState(DomainState.Active, "영역전개 · 무량공처");
            SetDomainVisual(true);

            Collider[] colliders = Physics.OverlapSphere(transform.position, domainRadius);
            HashSet<IDomainStunnable> affectedTargets = new HashSet<IDomainStunnable>();

            foreach (Collider hit in colliders)
            {
                MonoBehaviour[] behaviours = hit.GetComponentsInParent<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour is IDomainStunnable target && affectedTargets.Add(target))
                    {
                        target.ApplyDomainStun(domainDuration);
                    }
                }
            }
        }

        private void Fail(string message)
        {
            ChangeState(DomainState.Failed, message);
        }

        private void ChangeState(DomainState nextState, string message)
        {
            State = nextState;
            stateStartedAt = Time.time;
            StatusText = message;
        }

        private void EnsureTechniqueControllers()
        {
            CursedEnergyController.GetOrCreate(gameObject);
            CombatActionGate.GetOrCreate(gameObject);

            if (GetComponent<TargetLockController>() == null)
            {
                gameObject.AddComponent<TargetLockController>();
            }

            GojoTechniqueController technique = GetComponent<GojoTechniqueController>();
            if (technique == null)
            {
                technique = gameObject.AddComponent<GojoTechniqueController>();
            }

            if (GetComponent<GojoTechniqueChainController>() == null)
            {
                gameObject.AddComponent<GojoTechniqueChainController>();
            }
        }

        private void EnsureRuntimeVisual()
        {
            if (domainVisualRoot != null)
            {
                return;
            }

            GameObject runtimeVisual = new GameObject("UnlimitedVoidPrototypeVisual");
            runtimeVisual.transform.SetParent(transform, false);
            runtimeVisual.SetActive(false);

            UnlimitedVoidPrototypeVisual visual =
                runtimeVisual.AddComponent<UnlimitedVoidPrototypeVisual>();
            visual.Configure(domainRadius);
            domainVisualRoot = runtimeVisual;
        }

        private void SetDomainVisual(bool visible)
        {
            if (domainVisualRoot != null)
            {
                domainVisualRoot.SetActive(visible);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, domainRadius);
        }
    }
}
