using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public interface IShibuyaCinematicView
    {
        bool RequestCinematic(ShibuyaFighter localOwner, ShibuyaCinematicSession session);
    }
    public sealed partial class ShibuyaFighter : IDamageGuard
    {
        public ShibuyaCinematicSession Cinematic { get; private set; }
        public bool IsInCinematic => Cinematic != null && Cinematic.Active;
        private int cinematicAction = int.MinValue;
        private readonly HashSet<int> cinematicEvents = new HashSet<int>();
        private Quaternion cinematicFacing;
        private readonly List<IShibuyaCinematicView> localViews = new List<IShibuyaCinematicView>();
        public void RegisterLocalView(IShibuyaCinematicView view) { if (view != null && !localViews.Contains(view)) localViews.Add(view); }
        public void UnregisterLocalView(IShibuyaCinematicView view) => localViews.Remove(view);
        internal void RequestParticipantCamera(ShibuyaCinematicSession session) { foreach (var view in localViews.ToArray()) view.RequestCinematic(this, session); }
        internal bool WasCinematicEventConsumed(int action, int evt) => cinematicAction == action && cinematicEvents.Contains(evt);
        internal void ConsumeCinematicEvent(int action, int evt) { if (cinematicAction != action) { cinematicEvents.Clear(); cinematicAction = action; } cinematicEvents.Add(evt); }
        internal void EnterCinematic(ShibuyaCinematicSession session)
        {
            CancelGuardInput(); CancelGroundDash();
            Cinematic = session; cinematicFacing = transform.rotation;
            buffered = airBuffered = chaseQueued = jumpIntent = false; moveIntent = Vector3.zero;
            ReleaseOutgoingLink(); ClearIncomingLink();
        }
        internal void LeaveCinematic(ShibuyaCinematicSession session) { if (Cinematic == session) { Cinematic = null; moveIntent = Vector3.zero; jumpIntent = false; } }
        private void CancelCinematic(string reason) { if (IsInCinematic) Cinematic.Finish(reason); }
        public DamageGuardDecision EvaluateDamage(DamageContext context)
        {
            if(IsInCinematic)return Cinematic.Evaluate(this,context);
            var evade=EvaluateGroundDashDamage(context);
            return evade==DamageGuardDecision.NoDecision?EvaluateHeldGuard(context):evade;
        }
        private bool ConsumeCinematicInput(bool attack, Vector3 movement)
        {
            if (!IsInCinematic) return false;
            var settings = Cinematic.Profile;
            if ((attack && settings.cancelOnAttackInput) || (!attack && movement.sqrMagnitude > .01f && settings.cancelOnMoveInput))
                Cinematic.Finish("participant-input-cancel");
            return true; // A canceling input is not replayed as a new attack.
        }
        private void UpdateCinematicMotion()
        {
            var settings = Cinematic.Profile;
            if (settings.holdFacing) transform.rotation = cinematicFacing;
            else if (moveIntent.sqrMagnitude > .001f) transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(moveIntent), profile.turnSpeed * Time.deltaTime);
            if (settings.holdMotion) return;
            // Existing physics only, through this fighter's controller; attack/input movement stays suspended.
            float delta = Time.deltaTime;
            UpdateVerticalMotion(delta, true);
            Vector3 displacement = (knockback + Vector3.up * verticalSpeed) * delta + AdvanceAirHorizontal(delta) + AdvanceDistanceKnockback(delta);
            displacement = ConstrainFighterMovement(SeparateLandingCapsules(displacement, delta));
            int steps = Mathf.Max(1, Mathf.CeilToInt(displacement.magnitude / Mathf.Max(.05f, motor.radius * .5f)));
            CollisionFlags collisions = CollisionFlags.None; Vector3 before = transform.position;
            for (int i = 0; i < steps; i++) collisions |= motor.Move(displacement / steps);
            FinishVerticalMotion(collisions);
            AfterAerialMotor(before, collisions, delta);
            knockback = Vector3.MoveTowards(knockback, Vector3.zero, profile.knockbackDamping * delta);
        }
        private void TryCinematicAfterHit(ShibuyaFighter victim, ShibuyaHitEvent evt, int action, int eventId)
        {
            if (evt.cinematic == null) return;
            ShibuyaCinematicTrigger trigger = CurrentFinisher == ShibuyaFinisher.Uppercut ? ShibuyaCinematicTrigger.Uppercut
                : CurrentFinisher == ShibuyaFinisher.Down ? ShibuyaCinematicTrigger.Down : ShibuyaCinematicTrigger.Default;
            int finalIndex = moveset == null ? profile.strikes.Length - 1 : moveset.finisherStepIndex;
            if (executingAirAttack || strikeIndex != finalIndex) trigger = ShibuyaCinematicTrigger.Authored;
            ShibuyaCinematicSession.TryStartAfterApplied(this, victim, evt.cinematic, DamageResolution.Applied, trigger, action, eventId);
        }
    }
}
