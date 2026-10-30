using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // F intent adapter only. The common resource controller owns every unit of energy;
    // aura rendering continues to read maximum capacity independently.
    [DefaultExecutionOrder(-40), DisallowMultipleComponent, RequireComponent(typeof(ShibuyaFighter))]
    public sealed class ShibuyaEnergyCharge : MonoBehaviour
    {
        private ShibuyaFighter owner;
        private CursedEnergyController resource;
        private bool held;
        private bool initialized;
        public CursedEnergyController Resource => resource;
        public bool IsCharging { get; private set; }
        public float HoldDuration { get; private set; }
        public event Action<ShibuyaEnergyCharge> ChargeStarted;
        public event Action<ShibuyaEnergyCharge, float> ChargeReleased;
        public event Action<ShibuyaEnergyCharge, float> ChargeInterrupted;

        private void Awake() { owner = GetComponent<ShibuyaFighter>(); BindResource(); }
        private void BindResource()
        {
            var profile = owner != null && owner.Profile != null ? owner.Profile.cursedEnergy : null;
            if (profile == null) return;
            if (resource == null) resource = CursedEnergyController.GetOrCreate(gameObject);
            if (!initialized || resource.Profile != profile) {
                StopCharge();
                resource.ApplyProfile(profile, !initialized);
                initialized = true;
            }
        }
        public void SetHeld(bool value)
        {
            held = value;
            if (!value) StopCharge(true);
        }
        // Input capture/focus loss is cancellation, not a successful F-release hook.
        public void CancelCharge() { held = false; StopCharge(); }
        private void Update()
        {
            BindResource();
            var profile = owner != null && owner.Profile != null ? owner.Profile.cursedEnergy : null;
            var state = owner != null ? owner.State : ShibuyaFighterState.Finished;
            bool eligible = held && owner != null && owner.isActiveAndEnabled && resource != null && resource.isActiveAndEnabled
                && profile != null && profile.RecoveryMode == CursedEnergyRecoveryMode.HoldCharge
                && profile.ChargingEnabled && !owner.IsInCinematic && !owner.IsGuarding && !owner.IsGuardBroken && !owner.IsParryRecovering
                && state != ShibuyaFighterState.Dead && state != ShibuyaFighterState.Finished
                && state != ShibuyaFighterState.Hitstunned && !owner.IsDowned
                && !owner.IsAirborne && !owner.IsHeldByAerialHit;
            if (!eligible) { StopCharge(); return; }
            if (!IsCharging) { IsCharging = true; HoldDuration = 0; ChargeStarted?.Invoke(this); }
            // Scaled game time, not a fighter's local hit-stop clock. Full reserves still count holds.
            HoldDuration += Time.deltaTime;
            resource.Restore(profile.RegenerationPerSecond * Time.deltaTime);
        }
        private void StopCharge(bool released = false)
        {
            bool wasCharging = IsCharging;
            float duration = HoldDuration;
            IsCharging = false; HoldDuration = 0;
            if (wasCharging) {
                if (released) ChargeReleased?.Invoke(this, duration);
                else ChargeInterrupted?.Invoke(this, duration);
            }
        }
        private void OnDisable() => CancelCharge();
#if UNITY_EDITOR
        [ContextMenu("검증/현재 주력 비우기 (Play Mode)")]
        private void EmptyForTest() { if (Application.isPlaying && resource != null) resource.SetCurrentEnergy(0); }
        [ContextMenu("검증/현재 주력 절반 (Play Mode)")]
        private void HalfForTest() { if (Application.isPlaying && resource != null) resource.SetCurrentEnergy(resource.MaxEnergy * .5f); }
        [ContextMenu("검증/현재 주력 최대 (Play Mode)")]
        private void FullForTest() { if (Application.isPlaying && resource != null) resource.SetCurrentEnergy(resource.MaxEnergy); }
#endif
    }
}
