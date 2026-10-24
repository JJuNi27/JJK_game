using UnityEngine;

namespace JJKGame.Shibuya
{
    // Input intent and visual eligibility are deliberately separate from fighter action rules.
    [DisallowMultipleComponent, RequireComponent(typeof(ShibuyaFighter))]
    public sealed class CursedEnergyAuraController : MonoBehaviour
    {
        [SerializeField] private CursedEnergyAuraProfile profile;
        [SerializeField] private Transform presentationAnchor;
        [SerializeField] private Renderer[] bodyRenderers;
        [SerializeField] private bool showEmissionPoints;
        private ShibuyaFighter owner;
        private CursedEnergyAuraVisual visual;
        private float envelope;
        private bool suppressed;
        private bool sizeInitialized;
        private float sizeMultiplier=1;
        public bool Held {get;private set;}
        public bool PressedThisFrame {get;private set;}
        public float Envelope => envelope;
        public CursedEnergyAuraProfile Profile => profile;
        public CursedEnergyAuraVisual Visual => visual;
        public float SizeMultiplier => sizeMultiplier;
        public float MaximumEnergy => owner!=null&&owner.Profile!=null&&owner.Profile.cursedEnergy!=null
            ? owner.Profile.cursedEnergy.MaxEnergy : profile!=null?profile.ReferenceMaxEnergy:100;
        private void Awake()
        {
            owner=GetComponent<ShibuyaFighter>();
            if(profile==null)profile=Resources.Load<CursedEnergyAuraProfile>("Cursed Energy Aura");
        }
        public void SetHeld(bool held) { PressedThisFrame=held&&!Held;Held=held; }
        public void SetSuppressed(bool value) {suppressed=value;}
        public void Configure(CursedEnergyAuraProfile value)
        {
            if(profile==value)return;
            if(visual!=null && (value==null || profile==null || profile.visualPrefab!=value.visualPrefab)) {
                Destroy(visual.gameObject);visual=null;
            }
            profile=value;
        }
        private void LateUpdate()
        {
            bool ended=owner==null || !owner.isActiveAndEnabled || owner.Health.IsDead || owner.State==ShibuyaFighterState.Finished;
            if(ended) {Clear();return;}
            bool hit=owner.State==ShibuyaFighterState.Hitstunned || owner.IsHeldByAerialHit || owner.IsAirborne || owner.IsDowned;
            bool wanted=Held && !suppressed && profile!=null && profile.auraEnabled
                && !(profile.suppressWhenHit&&hit) && !(profile.suppressDuringCinematic&&owner.IsInCinematic);
            if(wanted && visual==null && profile.visualPrefab!=null) {
                var root=Instantiate(profile.visualPrefab,presentationAnchor!=null?presentationAnchor:transform,false);
                root.name="CursedEnergyAuraRoot";visual=root.GetComponent<CursedEnergyAuraVisual>();
                if(visual!=null)visual.Bind(owner,bodyRenderers);
            }
            float duration=profile==null?.1f:wanted?profile.fadeIn:profile.fadeOut;
            envelope=Mathf.MoveTowards(envelope,wanted?1:0,Time.unscaledDeltaTime/Mathf.Max(.01f,duration));
            if(profile!=null) {
                float target=profile.SizeMultiplier(MaximumEnergy);
                if(!sizeInitialized||(!wanted&&envelope<=.001f)){sizeMultiplier=target;sizeInitialized=true;}
                else {
                    float response=profile.sizeResponseTime;
                    float blend=float.IsNaN(response)||float.IsInfinity(response)||response<=0?1:1-Mathf.Exp(-Time.unscaledDeltaTime/response);
                    sizeMultiplier=Mathf.Lerp(sizeMultiplier,target,blend);
                    if(Mathf.Abs(sizeMultiplier-target)<.001f)sizeMultiplier=target;
                }
            }
            if(visual!=null)visual.Sample(profile,Mathf.SmoothStep(0,1,envelope),wanted,sizeMultiplier);
        }
        public void Clear()
        {
            Held=PressedThisFrame=false;envelope=0;
            sizeInitialized=false;
            if(visual!=null)visual.StopImmediate();
        }
        private void OnDisable() {Clear();}
        private void OnDestroy() {if(visual!=null)Destroy(visual.gameObject);}
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(UnityEditor.SceneView.currentDrawingSceneView==null||!showEmissionPoints||visual==null)return;
            for(int i=0;i<visual.BodyAnchorCount;i++) {
                Gizmos.color=i<visual.MainFlowCount?Color.yellow:Color.cyan;
                Gizmos.DrawSphere(visual.BodyRootWorldPosition(i),.018f);
            }
        }
#endif
    }
}
