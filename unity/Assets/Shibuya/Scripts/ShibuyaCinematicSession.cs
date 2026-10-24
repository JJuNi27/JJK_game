using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // No global manager, global camera search, or shared battle clock. Actors own the lock.
    public sealed class ShibuyaCinematicSession : MonoBehaviour
    {
        public ShibuyaFighter Attacker { get; private set; }
        public ShibuyaFighter Victim { get; private set; }
        public ShibuyaCinematicProfile Profile { get; private set; }
        public bool Active { get; private set; }
        public string EndReason { get; private set; }
        public float Elapsed => (float)(Time.unscaledTimeAsDouble - started);
        public event Action<ShibuyaCinematicSession> Ended;
        private double started;
        private bool permit;
        private ShibuyaFighter permitSource, permitTarget;
        public bool ApplyingAuthoredDamage { get; private set; }

        // resolution must be the return value of the triggering hit's Health.ReceiveDamage.
        public static ShibuyaCinematicSession TryStartAfterApplied(ShibuyaFighter attacker, ShibuyaFighter victim,
            ShibuyaCinematicProfile profile, DamageResolution resolution, ShibuyaCinematicTrigger trigger, int actionId, int eventId)
        {
            if (resolution != DamageResolution.Applied || profile == null || !profile.cinematicEnabled || !profile.IsValid
                || (profile.triggers & trigger) == 0 || attacker == victim || !Live(attacker) || !Live(victim)
                || attacker.Cinematic != null || victim.Cinematic != null || attacker.gameObject.scene != victim.gameObject.scene
                || attacker.WasCinematicEventConsumed(actionId, eventId)
                || Vector3.Distance(attacker.transform.position, victim.transform.position) > profile.maximumSeparation
                || (profile.cancelOnObstruction && Obstructed(attacker, victim))) return null;
            var go = new GameObject("Shibuya Two Actor Cinematic");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, attacker.gameObject.scene);
            var session = go.AddComponent<ShibuyaCinematicSession>();
            session.Attacker = attacker; session.Victim = victim; session.Profile = profile;
            session.started = Time.unscaledTimeAsDouble; session.Active = true;
            attacker.ConsumeCinematicEvent(actionId, eventId);
            attacker.EnterCinematic(session); victim.EnterCinematic(session);
            attacker.RequestParticipantCamera(session); victim.RequestParticipantCamera(session);
            return session;
        }
        private static bool Live(ShibuyaFighter f) => f != null && f.isActiveAndEnabled && !f.Health.IsDead && f.State != ShibuyaFighterState.Finished;
        private void Update()
        {
            if (!Active) return;
            if (!Live(Attacker) || !Live(Victim)) { Finish("participant-ended"); return; }
            if (Profile == null || !Profile.cinematicEnabled || !Profile.IsValid) { Finish("invalid-or-disabled-profile"); return; }
            if (Vector3.Distance(Attacker.transform.position, Victim.transform.position) > Profile.maximumSeparation) { Finish("separation"); return; }
            if (Profile.cancelOnObstruction && Obstructed(Attacker, Victim)) { Finish("obstruction"); return; }
            if (Elapsed >= Profile.duration) Finish("complete");
        }
        public DamageGuardDecision Evaluate(ShibuyaFighter receiver, DamageContext context)
        {
            if (!Active) return DamageGuardDecision.NoDecision;
            // Environmental death and the existing match policy always retain priority.
            if (context.DeliveryType == DamageDeliveryType.Environmental) return DamageGuardDecision.NoDecision;
            if (permit && receiver == permitTarget && context.Source == permitSource.gameObject) {
                permit = false; return DamageGuardDecision.NoDecision; // Single-use; reentrant callbacks cannot reuse it.
            }
            return Profile.blockThirdPartyDamage ? DamageGuardDecision.Block : DamageGuardDecision.NoDecision;
        }
        // Explicitly authored only. Sessions never schedule or synthesize damage by themselves.
        public DamageResolution ApplyAuthoredDamage(ShibuyaFighter source, ShibuyaFighter target, float amount)
        {
            if (!Active || !Profile.allowAuthoredDamage || ApplyingAuthoredDamage || source == target
                || (source != Attacker && source != Victim) || (target != Attacker && target != Victim)) return DamageResolution.Invalid;
            permit = ApplyingAuthoredDamage = true; permitSource = source; permitTarget = target;
            try { return target.Health.ReceiveDamage(new DamageContext(amount, source.gameObject, DamageDeliveryType.PhysicalStrike)); }
            finally { permit = ApplyingAuthoredDamage = false; permitSource = permitTarget = null; }
        }
        public static bool Obstructed(ShibuyaFighter a, ShibuyaFighter b)
        {
            Vector3 from = a.transform.position + Vector3.up, to = b.transform.position + Vector3.up;
            foreach (var hit in Physics.RaycastAll(from, (to - from).normalized, Vector3.Distance(from, to), ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<ShibuyaFighter>() == null) return true;
            return false;
        }
        public void Finish(string reason)
        {
            if (!Active) return;
            Active = false; EndReason = reason;
            if (Attacker != null) Attacker.LeaveCinematic(this);
            if (Victim != null) Victim.LeaveCinematic(this);
            Ended?.Invoke(this);
            Destroy(gameObject);
        }
        private void OnDisable() { Finish("session-disabled"); }
        private void OnDestroy() { Finish("session-destroyed"); }
    }
}
