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
        public void Configure(Transform root, Transform armL, Transform armR, Transform legL, Transform legR)
        { visualRoot = root; leftArm = armL; rightArm = armR; leftLeg = legL; rightLeg = legR; }
        private void Awake()
        {
            fighter = GetComponent<ShibuyaFighter>();
            renderers = visualRoot.GetComponentsInChildren<Renderer>();
            block = new MaterialPropertyBlock();
        }
        private void LateUpdate()
        {
            ShibuyaFighterState state = fighter.State;
            float walk = Mathf.Sin(Time.time * 12f) * Mathf.Clamp01(fighter.PlanarSpeed / 5f) * 30f;
            leftArm.localRotation = Quaternion.Euler(walk, 0, 0);
            rightArm.localRotation = Quaternion.Euler(-walk, 0, 0);
            leftLeg.localRotation = Quaternion.Euler(-walk, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(walk, 0, 0);
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localPosition = Vector3.zero;
            if (state == ShibuyaFighterState.Attacking)
            {
                float phase = Mathf.Sin(fighter.AttackProgress * Mathf.PI);
                rightArm.localRotation = Quaternion.Euler(-100f * phase, 0f, -8f * phase);
                leftArm.localRotation = Quaternion.Euler(-28f * phase, 0f, 10f);
                visualRoot.localRotation = Quaternion.Euler(0f, -14f * phase, 0f);
                visualRoot.localPosition = Vector3.forward * (0.12f * phase);
            }
            else if (state == ShibuyaFighterState.Hitstunned)
                visualRoot.localRotation = Quaternion.Euler(-15f, 0, 5f);
            else if (state == ShibuyaFighterState.Dead)
            {
                visualRoot.localRotation = Quaternion.Euler(-80f, 0f, 0f);
                visualRoot.localPosition = new Vector3(0, 0.28f, -0.4f);
            }
            bool flash = Time.time < fighter.HitFlashUntil;
            foreach (Renderer renderer in renderers)
            {
                if (flash) { block.SetColor("_BaseColor", Color.white); renderer.SetPropertyBlock(block); }
                else renderer.SetPropertyBlock(null);
            }
        }
    }
}
