using JJKGame.CameraSystem;
using UnityEngine;
using UnityEngine.UI;

namespace JJKGame.Player
{
    /// <summary>Short Purple shot on the existing camera lease. Entirely sequence-clocked.</summary>
    [DefaultExecutionOrder(2100)]
    public sealed class PurpleReleasePresentation : MonoBehaviour
    {
        private Camera view;
        private DomainCameraOverride lease;
        private GameObject graphicRoot;
        private Material graphicMaterial;
        private GojoPolishSettings tuning;
        private Vector3 origin, forward, right, caster;
        private float releaseAt, elapsed;
        private float impactStartedAt;
        private bool releaseCrossed;
        private bool ownsCamera, finished;
        public bool OwnsCamera => ownsCamera;
        public bool ImpactVisible => graphicRoot != null && graphicRoot.activeSelf;

        public void Configure(Vector3 releaseOrigin, Vector3 castDirection, float releaseTime, Camera camera = null)
        {
            tuning=GojoPolishSettings.Current;
            origin=releaseOrigin; forward=castDirection; right=Vector3.Cross(Vector3.up,forward);
            caster=origin-Vector3.up*(1.05f+tuning.purpleHoldOffset.y)-forward*(1.1f+tuning.purpleHoldOffset.z);
            releaseAt=releaseTime; view=camera;
            if(view==null || !view.isActiveAndEnabled || DomainCameraOverride.IsOwned(view)) { finished=true; return; }
            if(tuning.purpleCameraChoreography)
            {
                lease=view.GetComponent<DomainCameraOverride>();
                if(lease==null) lease=view.gameObject.AddComponent<DomainCameraOverride>();
                ownsCamera=lease.Acquire(this);
            }
            graphicMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleImpact")) { name="PurpleImpact_Runtime" };
            graphicRoot=new GameObject("PurpleImpactFrame",typeof(RectTransform),typeof(Canvas));
            graphicRoot.transform.SetParent(transform,false);
            var canvas=graphicRoot.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera;
            canvas.worldCamera=view; canvas.planeDistance=view.nearClipPlane+.015f;
            canvas.sortingOrder=30000;
            var graphic=new GameObject("ProceduralMangaGraphic",typeof(RectTransform),typeof(RawImage));
            graphic.transform.SetParent(graphicRoot.transform,false);
            var rect=graphic.GetComponent<RectTransform>(); rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            var raw=graphic.GetComponent<RawImage>(); raw.material=graphicMaterial; raw.raycastTarget=false;
            graphicRoot.SetActive(false);
        }

        public void Sample(float sequenceElapsed)
        {
            elapsed=sequenceElapsed;
            Apply();
        }

        private void LateUpdate() => Apply();

        private void Apply()
        {
            if(finished) return;
            if(view==null || !view.isActiveAndEnabled || (ownsCamera && (lease==null || !lease.IsOwnedBy(this)))) { Restore(); return; }
            float age=elapsed-releaseAt;
            float impactDuration=tuning.PurpleImpactDuration;
            float impactAt=releaseAt-impactDuration;
            if(!releaseCrossed && elapsed>=impactAt)
            {
                releaseCrossed=true;
                // Keep a readable entry even if a hitch skips the scheduled graphic window.
                impactStartedAt=elapsed>=releaseAt?elapsed:impactAt;
            }
            float end=tuning.purpleCameraFollowThrough;
            if(age>=end) { Restore(); return; }
            // An unrelated cinematic that acquired the camera must never inherit this overlay.
            if(!ownsCamera && DomainCameraOverride.IsOwned(view)) { Restore(); return; }
            if(ownsCamera)
            {
                float summon=Mathf.SmoothStep(0,1,elapsed/.4f);
                float fusion=Mathf.SmoothStep(0,1,Mathf.InverseLerp(tuning.PurpleFusionStart,tuning.PurpleHoldStart,elapsed));
                float charge=Mathf.Clamp01((elapsed-tuning.PurpleHoldStart)/tuning.purpleFusionHoldDuration);
                float shift=summon;
                Vector3 summonPosition=caster+right*(tuning.purpleCameraSideOffset*.75f)-forward*9.3f+Vector3.up*3.3f;
                Vector3 holdPosition=caster+right*tuning.purpleCameraSideOffset-forward*8.2f+Vector3.up*2.5f;
                holdPosition=Vector3.Lerp(summonPosition,holdPosition,fusion);
                holdPosition+=right*Mathf.Sin(charge*2.1f)*.5f+forward*charge*.35f;
                Vector3 focus=Vector3.Lerp(caster+Vector3.up*1.5f,origin,.48f+.2f*fusion);
                float tension=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.6f,1f,charge));
                holdPosition=Vector3.Lerp(holdPosition,focus+(holdPosition-focus)*.91f,tension);
                float follow=Mathf.Clamp01(age/.30f);
                Vector3 shotPosition=holdPosition+forward*follow*.9f-right*follow*.75f;
                Vector3 shotFocus=focus+forward*follow*3.4f;
                float impulseAge=elapsed-impactAt;
                float impulse=impulseAge>=0?Mathf.Exp(-impulseAge*19)*tuning.purpleCameraImpulse:0;
                shotPosition+=right*(Mathf.Sin(age*137)*impulse)+Vector3.up*(Mathf.Cos(age*93)*impulse*.6f);
                float fov=lease.ReturnFov-tuning.purpleCameraTensionFov*tension;
                if(age>=0) fov=lease.ReturnFov+tuning.purpleReleaseFov*Mathf.Exp(-age*13);
                float restore=Mathf.SmoothStep(0,1,Mathf.InverseLerp(end*.45f,end,age));
                float blend=shift*(1-restore);
                lease.Pose(this,Vector3.Lerp(lease.ReturnPosition,shotPosition,blend),
                    Quaternion.Slerp(lease.ReturnRotation,Quaternion.LookRotation(shotFocus-shotPosition,Vector3.up),blend),
                    Mathf.Lerp(lease.ReturnFov,fov,blend));
            }
            float impactAge=elapsed-impactStartedAt;
            bool impact=tuning.purpleImpactFrames && releaseCrossed && impactAge>=0 && impactAge<impactDuration;
            if(graphicRoot!=null) graphicRoot.SetActive(impact);
            if(impact)
            {
                Vector3 centre=view.WorldToViewportPoint(origin);
                Vector3 edge=view.WorldToViewportPoint(origin+view.transform.up*2.1f);
                graphicMaterial.SetVector("_Centre",new Vector4(Mathf.Clamp(centre.x,.18f,.82f),Mathf.Clamp(centre.y,.2f,.8f),
                    Mathf.Clamp(Mathf.Abs(edge.y-centre.y),.10f,.24f),view.aspect));
                float beat=Mathf.Clamp(impactAge/impactDuration*4f,0,3.999f);
                graphicMaterial.SetFloat("_Stage",Mathf.Floor(beat));
                graphicMaterial.SetFloat("_BeatProgress",Mathf.Repeat(beat,1));
                graphicMaterial.SetFloat("_Opacity",1f);

            }
        }

        public void Restore()
        {
            if(graphicRoot!=null) graphicRoot.SetActive(false);
            if(ownsCamera && lease!=null) lease.Release(this);
            ownsCamera=false; finished=true;
        }
        private void OnDisable() => Restore();
        private void OnDestroy()
        {
            Restore();
            if(graphicMaterial!=null) Destroy(graphicMaterial);
        }
    }
}
