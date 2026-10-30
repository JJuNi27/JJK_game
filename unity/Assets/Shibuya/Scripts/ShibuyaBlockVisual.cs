using UnityEngine;

namespace JJKGame.Shibuya
{
    [RequireComponent(typeof(ShibuyaFighter))]
    public sealed class ShibuyaBlockVisual : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot, leftArm, rightArm, leftLeg, rightLeg;
        private ShibuyaFighter fighter;
        private Renderer[] renderers;
        private MaterialPropertyBlock block;
        private Transform[] upperBody;
        private Vector3[] upperPositions;
        private Quaternion[] upperRotations;
        private Vector3 rootPosition, upperPivot;
        private Quaternion rootRotation, leftLegRotation, rightLegRotation;
        private Vector3 reactionFrom, reactionTarget, reactionEuler;
        private float reactionStartedAt, reactionStartTime, reactionRecoveryTime;
        private bool reacting;
        public Vector3 ReactionEuler => reactionEuler;
        public bool IsReacting => reacting;
        public void Configure(Transform root, Transform armL, Transform armR, Transform legL, Transform legR)
        { visualRoot = root; leftArm = armL; rightArm = armR; leftLeg = legL; rightLeg = legR; }
        private void Awake()
        {
            fighter = GetComponent<ShibuyaFighter>();
            renderers = visualRoot.GetComponentsInChildren<Renderer>();
            block = new MaterialPropertyBlock();
            rootPosition = visualRoot.localPosition; rootRotation = visualRoot.localRotation;
            leftLegRotation = leftLeg.localRotation; rightLegRotation = rightLeg.localRotation;
            // Existing block-rig presentation bindings only; no gameplay rule depends on these names.
            upperBody = new[] { visualRoot.Find("Torso"), visualRoot.Find("Head"),
                visualRoot.Find("FaceDirection"), leftArm, rightArm };
            upperPositions = new Vector3[upperBody.Length]; upperRotations = new Quaternion[upperBody.Length];
            for (int i=0; i<upperBody.Length; i++) if (upperBody[i] != null) {
                upperPositions[i] = upperBody[i].localPosition; upperRotations[i] = upperBody[i].localRotation;
            }
            upperPivot = (leftLeg.localPosition + rightLeg.localPosition) * 0.5f;
        }
        private void OnEnable() { fighter.HitReacted += OnHitReaction; }
        private void OnDisable() {
            if (fighter != null) fighter.HitReacted -= OnHitReaction;
            ClearReaction();
            if (upperBody != null) RestorePose();
            if (renderers != null) foreach (Renderer renderer in renderers) if (renderer != null) renderer.SetPropertyBlock(null);
        }
        private void OnHitReaction(ShibuyaHitReaction hit)
        {
            ShibuyaCombatProfile p = fighter.Profile;
            if (!p.hitReactionEnabled || hit.StunDuration <= 0f || hit.LocalDirection.sqrMagnitude < 0.000001f) {
                ClearReaction(); return;
            }
            Vector3 direction = hit.LocalDirection;
            float frontBack = direction.z < 0f ? p.frontHitReactionStrength : p.backHitReactionStrength;
            float side = direction.x > 0f ? p.leftHitReactionStrength : p.rightHitReactionStrength;
            float multiplier = Mathf.Max(0f, hit.IsHeavy ? p.heavyHitReactionMultiplier : p.normalHitReactionMultiplier);
            reactionFrom = reactionEuler;
            // Continuous components, not four snapped quadrants. A new hit replaces, never adds to, the target pose.
            reactionTarget = new Vector3(direction.z * Mathf.Max(0f, frontBack) * p.hitReactionLeanAngle,
                direction.x * Mathf.Max(0f, side) * p.hitReactionTwistAngle,
                -direction.x * Mathf.Max(0f, side) * p.hitReactionLeanAngle) * multiplier;
            reactionStartedAt = fighter.CombatTime;
            // Freeze the impact pose itself during a local hit-stop, rather than freeze an untouched neutral pose.
            reactionStartTime = hit.HasHitStop ? 0f : Mathf.Min(Mathf.Max(0f, p.hitReactionStartTime), hit.StunDuration);
            reactionRecoveryTime = Mathf.Min(Mathf.Max(0.01f, p.hitReactionRecoveryTime),
                Mathf.Max(0f, hit.StunDuration - reactionStartTime));
            reacting = true;
        }
        private void ClearReaction() { reacting = false; reactionEuler = Vector3.zero; }
        private void RestorePose()
        {
            visualRoot.localPosition = rootPosition; visualRoot.localRotation = rootRotation;
            for (int i=0; i<upperBody.Length; i++) if (upperBody[i] != null) {
                upperBody[i].localPosition = upperPositions[i]; upperBody[i].localRotation = upperRotations[i];
            }
            leftLeg.localRotation = leftLegRotation; rightLeg.localRotation = rightLegRotation;
        }
        private void ApplyReactionPose()
        {
            if (!reacting) return;
            if (!fighter.Profile.hitReactionEnabled) { ClearReaction(); return; }
            float age = Mathf.Max(0f, fighter.CombatTime - reactionStartedAt);
            if (age < reactionStartTime)
                reactionEuler = Vector3.Lerp(reactionFrom, reactionTarget, Mathf.SmoothStep(0f, 1f, age / reactionStartTime));
            else if (age < reactionStartTime + reactionRecoveryTime)
                reactionEuler = reactionTarget * (1f - Mathf.SmoothStep(0f, 1f,
                    (age - reactionStartTime) / Mathf.Max(0.000001f, reactionRecoveryTime)));
            else { ClearReaction(); return; }
            Quaternion pose = Quaternion.Euler(reactionEuler);
            for (int i=0; i<upperBody.Length; i++) if (upperBody[i] != null) {
                upperBody[i].localPosition = upperPivot + pose * (upperPositions[i] - upperPivot);
                upperBody[i].localRotation = pose * upperBody[i].localRotation;
            }
        }
        private void LateUpdate()
        {
            ShibuyaFighterState state = fighter.State;
            RestorePose();
            float walk = fighter.IsInAirSession || state == ShibuyaFighterState.Airborne || fighter.IsDowned || state == ShibuyaFighterState.Hitstunned || state == ShibuyaFighterState.Dead
                || state == ShibuyaFighterState.Finished ? 0f
                : Mathf.Sin(fighter.CombatTime * 12f) * Mathf.Clamp01(fighter.PlanarSpeed / 5f) * 30f;
            leftArm.localRotation = Quaternion.Euler(walk, 0, 0);
            rightArm.localRotation = Quaternion.Euler(-walk, 0, 0);
            leftLeg.localRotation = Quaternion.Euler(-walk, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(walk, 0, 0);
            if (state == ShibuyaFighterState.Attacking)
            {
                ShibuyaStrikeDefinition strike = fighter.CurrentStrike;
                float elapsed = fighter.PresentationElapsed;
                // Wind up during startup, snap on active entry, recover only after active ends.
                float phase = elapsed < strike.Startup ? -0.25f * Mathf.Sin(elapsed / strike.Startup * Mathf.PI * 0.5f)
                    : elapsed <= strike.ActiveEnd ? Mathf.Lerp(0.75f, 1f, Mathf.Clamp01((elapsed - strike.Startup) / 0.025f))
                    : 1f - Mathf.SmoothStep(0f, 1f, (elapsed - strike.ActiveEnd) / Mathf.Max(0.01f, strike.recovery));
                if (strike.PowerWindup > 0f && elapsed < strike.Startup)
                    phase = elapsed < strike.PowerWindup
                        ? -0.50f * Mathf.SmoothStep(0f, 1f, elapsed / strike.PowerWindup)
                        : Mathf.Lerp(-0.50f, -0.15f, (elapsed - strike.PowerWindup) / Mathf.Max(0.001f, strike.Startup - strike.PowerWindup));
                bool left = strike.motion == ShibuyaStrikeMotion.LeftJab || strike.motion == ShibuyaStrikeMotion.HeavyCross;
                bool hook = strike.motion == ShibuyaStrikeMotion.RightHook;
                float force = phase * strike.presentationIntensity;
                Transform strikingArm = left ? leftArm : rightArm;
                Transform guardingArm = left ? rightArm : leftArm;
                strikingArm.localRotation = Quaternion.Euler(-100f * force, hook ? -45f * force : 0f,
                    (left ? 8f : -8f) * force + (hook ? -35f * force : 0f));
                guardingArm.localRotation = Quaternion.Euler(-28f * phase, 0f, left ? -10f : 10f);
                visualRoot.localRotation = Quaternion.Euler(0f, (left ? 18f : -18f) * force, 0f);
                visualRoot.localPosition = Vector3.forward * (0.12f * force);
                if (strike.motion == ShibuyaStrikeMotion.Uppercut) {
                    rightArm.localRotation = Quaternion.Euler(-25f - 135f * Mathf.Clamp01(phase), 0, -12f);
                    leftArm.localRotation = Quaternion.Euler(-35f, 0, 12f);
                    visualRoot.localRotation = Quaternion.Euler(-12f * phase, -15f * phase, 0);
                } else if (strike.motion == ShibuyaStrikeMotion.DownSlam) {
                    float angle = Mathf.Lerp(-165f, -20f, Mathf.Clamp01(phase));
                    rightArm.localRotation = Quaternion.Euler(angle, 0, -12f);
                    leftArm.localRotation = Quaternion.Euler(angle, 0, 12f);
                    visualRoot.localRotation = Quaternion.Euler(20f * Mathf.Clamp01(phase), 0, 0);
                }
            }
            else if (fighter.IsParryRecovering) {
                float weight=Mathf.Sin((1-fighter.ParryMotionProgress)*Mathf.PI*.5f)*fighter.ParrySettings.motionStrength;
                leftArm.localRotation=Quaternion.Euler(-80-30*weight,-50*weight,-20);
                rightArm.localRotation=Quaternion.Euler(-45,25*weight,20);
                visualRoot.localRotation=rootRotation*Quaternion.Euler(0,25*weight,0);
            }
            else if (fighter.IsGuarding) {
                float angle=fighter.GuardSettings.guardArmAngle;
                leftArm.localRotation=Quaternion.Euler(-angle,0,-20);
                rightArm.localRotation=Quaternion.Euler(-angle,0,20);
            }
            else if (fighter.IsGroundDashing) {
                Vector3 step = transform.InverseTransformDirection(fighter.GroundDashDirection);
                float lean = fighter.GroundDashLeanAngle * Mathf.Sin(fighter.GroundDashProgress * Mathf.PI);
                visualRoot.localRotation = rootRotation * Quaternion.Euler(step.z * lean, 0, -step.x * lean);
                leftArm.localRotation = rightArm.localRotation = Quaternion.Euler(25, 0, 0);
            }
            else if(fighter.IsChasing) {
                visualRoot.localRotation=Quaternion.Euler(fighter.ChaseLeanAngle,0,0);
                leftArm.localRotation=rightArm.localRotation=Quaternion.Euler(25,0,0);
            }
            else if (state == ShibuyaFighterState.Dead)
            {
                visualRoot.localRotation = Quaternion.Euler(-80f, 0f, 0f);
                visualRoot.localPosition = new Vector3(0, 0.28f, -0.4f);
            }
            if (state == ShibuyaFighterState.Dead || state == ShibuyaFighterState.Finished) ClearReaction();
            else if (fighter.IsDowned) {
                ClearReaction();
                float weight = fighter.DownPoseWeight;
                visualRoot.localRotation = Quaternion.Euler(fighter.DownPoseAngle * weight, 0, 0);
                visualRoot.localPosition = new Vector3(0, .28f * weight, .25f * weight);
            } else {
                ApplyReactionPose();
                if(fighter.IsGuardBroken && fighter.GuardSettings!=null)visualRoot.localRotation*=Quaternion.Euler(fighter.GuardSettings.breakLeanAngle,0,0);
                if (fighter.IsAirborne) visualRoot.localRotation *= Quaternion.Euler(-fighter.AirReactionLean, 0, 0);
            }
            bool flash = fighter.CombatTime < fighter.HitFlashUntil;
            foreach (Renderer renderer in renderers)
            {
                if (flash) { block.SetColor("_BaseColor", Color.white); renderer.SetPropertyBlock(block); }
                else renderer.SetPropertyBlock(null);
            }
        }
    }
}
