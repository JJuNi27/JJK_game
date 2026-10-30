using UnityEngine;
namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        private Vector3 facingIntent;
        private bool hasFacingIntent;
        // Read-only action/queue contract. No new balance values or duplicate reservation rule.
        public bool CanAcceptGroundCombatIntent=>isActiveAndEnabled&&profile!=null&&!Health.IsDead&&!stopped
            &&IsGrounded&&!IsActionLocked&&!IsInHitStop&&!IsInCinematic&&!groundDashing&&!guarding&&!IsGuardBroken
            &&!airSession&&!chasing&&!chaseQueued&&!jumpIntent&&!FocusingEnergy;
        public bool IsGroundAttackInProgress=>attacking&&!executingAirAttack;
        public int GroundAttackStepCount=>StepCount;
        public float GroundAttackReservationOpensAt=>Mathf.Max(0,AttackDuration-ReservationWindow);
        public bool CanReserveGroundAttack=>CanAcceptGroundCombatIntent&&IsGroundAttackInProgress
            &&strikeIndex<StepCount-1&&!HasBufferedAttack&&AttackElapsed>=GroundAttackReservationOpensAt;
        public bool SetFacingIntent(Vector3 direction)
        {
            direction.y=0;
            if(!CanAcceptGroundCombatIntent||attacking||direction.sqrMagnitude<.000001f){ClearFacingIntent();return false;}
            facingIntent=direction.normalized;hasFacingIntent=true;return true;
        }
        public void ClearFacingIntent(){hasFacingIntent=false;facingIntent=Vector3.zero;}
        private void ApplyFacingIntent(float delta)
        {
            if(!hasFacingIntent||!CanAcceptGroundCombatIntent||attacking)return;
            transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(facingIntent),Mathf.Max(.1f,profile.turnSpeed)*delta);
        }
        // A completed sequence may leave a manual continuation window. Start a fresh group only
        // once its active attack/recovery is over; never cancel an ongoing attack to do this.
        public bool TryStartGroundAttackSequence()
        {
            if(!CanAcceptGroundCombatIntent||attacking||HasBufferedAttack)return false;
            CancelCombo();return TryAttack(ShibuyaFinisher.Default);
        }
        public bool WithdrawBufferedGroundAttack(int sourceAction)
        {
            if(!IsGroundAttackInProgress||AcceptedAttackCount!=sourceAction||!buffered)return false;
            buffered=false;bufferedUntil=float.NegativeInfinity;ReleaseOutgoingLink();return true;
        }
    }
}
