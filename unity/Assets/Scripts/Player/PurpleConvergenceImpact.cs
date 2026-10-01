using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    /// <summary>Opt-in contact punctuation. No camera lease, permanent pose or Volume writes.</summary>
    public sealed class PurpleConvergenceImpact:MonoBehaviour
    {
        private PurpleConvergenceImpactProfile profile;
        private Camera view;
        private bool flashVisible;
        private Material material;
        private float startedAt,previewNow;
        private bool preview,triggered,offsetApplied,disposed;
        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private static readonly Dictionary<Camera,PurpleConvergenceImpact> RenderOwners=new Dictionary<Camera,PurpleConvergenceImpact>();
        public float ContactTime {get;private set;}
        public bool Triggered=>triggered;
        public bool FlashVisible=>flashVisible;
        public int TriggerCount {get;private set;}
        public float Age=>triggered?(preview?previewNow:Time.unscaledTime)-startedAt:float.PositiveInfinity;
        public void Configure(float contact,Camera camera,PurpleConvergenceImpactProfile settings)
        {
            ContactTime=contact;profile=settings;view=camera;
            material=new Material(Resources.Load<Shader>("VFX/PurpleConvergenceFlash")){name="PurpleConvergenceImpact_Flash"};
            RenderPipelineManager.beginCameraRendering+=BeginCamera;
            RenderPipelineManager.endCameraRendering+=EndCamera;
        }
        // Deterministic capture clock only; live gameplay always uses unscaled time and the shared controller.
        public void SetPreview(Camera camera,float wallTime){preview=true;view=camera;previewNow=wallTime;}
        public void SetPreviewTime(float wallTime){previewNow=wallTime;UpdateVisual();}
        public void Sample(float sequenceElapsed)
        {
            if(disposed || profile==null || view==null)return;
            if(!triggered && sequenceElapsed>=ContactTime)
            {
                triggered=true;TriggerCount++;startedAt=preview?previewNow:Time.unscaledTime;
                // Late attach/seek beyond the merge must not replay an old hit.
                if(sequenceElapsed-ContactTime>.15f){startedAt-=1;return;}
                if(!preview && Application.isPlaying && Time.timeScale>0)
                    PrototypeHitStopController.Request(profile.hitStopDuration,profile.hitStopScale);
            }
            UpdateVisual();
        }
        private void Update()=>UpdateVisual();
        private void UpdateVisual()
        {
            if(disposed || profile==null)return;
            float age=Age,duration=Mathf.Clamp(profile.flashFrames,1,2)/60f;
            bool visible=view!=null && age>=0 && age<duration-.000001f;
            flashVisible=visible;
            if(visible)material.SetFloat("_Strength",profile.flashStrength*(age<duration*.5f?1f:.72f));
        }
        public void ApplyRenderOffset(Camera camera)
        {
            if(disposed || !isActiveAndEnabled || camera!=view || offsetApplied || profile==null || RenderOwners.ContainsKey(camera))return;
            float age=Age;if(age<0 || age>=profile.impulseDuration)return;
            float decay=Mathf.Pow(1-age/profile.impulseDuration,2),phase=age*105;
            savedPosition=view.transform.position;savedRotation=view.transform.rotation;
            RenderOwners[camera]=this;offsetApplied=true;
            view.transform.position+=view.transform.right*(Mathf.Cos(phase)*profile.impulseDistance*decay)+view.transform.up*(Mathf.Sin(phase*.73f)*profile.impulseDistance*.55f*decay);
            view.transform.rotation=savedRotation*Quaternion.Euler(0,0,Mathf.Cos(phase)*profile.impulseRoll*decay);
        }
        public void RestoreRenderOffset()
        {
            if(!offsetApplied)return;
            if(view!=null){view.transform.SetPositionAndRotation(savedPosition,savedRotation);RenderOwners.Remove(view);}
            offsetApplied=false;
        }
        private void BeginCamera(ScriptableRenderContext context,Camera camera)=>ApplyRenderOffset(camera);
        private void EndCamera(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=view)return;
            RestoreRenderOffset();
            if(disposed || !isActiveAndEnabled || !flashVisible || material==null)return;
            // Draw after this camera's post-processing: never mutate its Volume or tone mapper.
            var command=CommandBufferPool.Get("PurpleConvergenceWhiteFlash");
            try
            {
                command.SetRenderTarget(camera.targetTexture!=null?new RenderTargetIdentifier(camera.targetTexture):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));
                command.SetViewport(camera.pixelRect);
                command.DrawProcedural(Matrix4x4.identity,material,0,MeshTopology.Triangles,3);
                context.ExecuteCommandBuffer(command);context.Submit();
            }
            finally{CommandBufferPool.Release(command);}
        }
        private void OnDisable(){RestoreRenderOffset();flashVisible=false;}
        public void Dispose()
        {
            disposed=true;
            flashVisible=false;
            RestoreRenderOffset();RenderPipelineManager.beginCameraRendering-=BeginCamera;RenderPipelineManager.endCameraRendering-=EndCamera;
            if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
            material=null;
        }
        private void OnDestroy()=>Dispose();
    }
}
