using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace JJKGame.Player
{
    /// <summary>Separate strong contact candidate. Holds presentation only, then catches up before Birth.</summary>
    public sealed class PurpleConvergenceDiagnostic:MonoBehaviour
    {
        private PurpleConvergenceDiagnosticProfile profile;
        private Camera view;
        private Material flash;
        private Volume volume;
        private VolumeProfile volumeProfile;
        private int volumeLayer;
        private UniversalAdditionalCameraData cameraData;
        private LayerMask savedMask;
        private bool savedPost,rendering,triggered,preview,disposed,ended;
        private float startTime,previewTime;
        private Vector3 savedPosition;private Quaternion savedRotation;
        private static readonly Dictionary<Camera,PurpleConvergenceDiagnostic> RenderOwners=new Dictionary<Camera,PurpleConvergenceDiagnostic>();
        public float ContactTime {get;private set;}
        public float Age=>triggered?(preview?previewTime:Time.unscaledTime)-startTime:float.PositiveInfinity;
        public float FlashWeight
        {
            get{float age=Age,duration=profile.flashFrames/60f,peak=profile.peakFrames/60f;if(age<0 || age>=duration-.000001f)return 0;return age<=peak?1:Mathf.Pow(1-Mathf.SmoothStep(0,1,(age-peak)/(duration-peak)),2);}
        }
        public bool Holding=>triggered && Age>=0 && Age<profile.holdDuration;
        public float HitStopStart {get;private set;}
        public float HitStopEnd {get;private set;}=float.NaN;
        public float LastRenderedBloom {get;private set;}
        public float LastRenderedExposure {get;private set;}
        public float LastRenderedPulseWeight {get;private set;}
        public void Configure(float contact,Vector3 centre,Camera camera,PurpleConvergenceDiagnosticProfile settings)
        {
            profile=settings;ContactTime=contact;transform.position=centre;view=camera;
            flash=new Material(Resources.Load<Shader>("VFX/PurpleConvergenceRadial")){name="PurpleConvergenceDiagnostic_Radial"};
            flash.SetFloat("_Radius",profile.radialRadius);flash.SetFloat("_Core",profile.hotCoreRadius);
            volumeLayer=PickFreeVolumeLayer();
            var go=new GameObject("ConvergenceDiagnosticTransientVolume");go.transform.SetParent(transform,false);go.layer=volumeLayer;
            volume=go.AddComponent<Volume>();volume.isGlobal=true;volume.priority=10000;volume.weight=0;
            volumeProfile=ScriptableObject.CreateInstance<VolumeProfile>();volumeProfile.name="PurpleConvergenceDiagnostic_Volume";
            var bloom=volumeProfile.Add<Bloom>(false);bloom.intensity.Override(profile.bloomIntensity);bloom.threshold.Override(.7f);
            var colour=volumeProfile.Add<ColorAdjustments>(false);colour.postExposure.Override(profile.exposure);
            volume.sharedProfile=volumeProfile;
            RenderPipelineManager.beginCameraRendering+=BeginCamera;RenderPipelineManager.endCameraRendering+=EndCamera;
        }
        private static int PickFreeVolumeLayer()
        {
            int used=0;foreach(var v in Resources.FindObjectsOfTypeAll<Volume>())if(v.gameObject.scene.IsValid())used|=1<<v.gameObject.layer;
            for(int layer=31;layer>=1;layer--)if((used&(1<<layer))==0)return layer;
            throw new System.InvalidOperationException("No unused transient Volume layer available");
        }
        public void SetPreview(Camera camera,float wall){preview=true;view=camera;previewTime=wall;}
        public void SetPreviewTime(float wall){previewTime=wall;FinishHoldIfNeeded();}
        public void Sample(float rawElapsed)
        {
            if(disposed)return;
            if(!triggered && rawElapsed>=ContactTime && rawElapsed-ContactTime<.15f)
            {
                triggered=true;startTime=preview?previewTime:Time.unscaledTime;HitStopStart=startTime;
                if(!preview)
                {
                    Debug.Log($"ConvergenceDiagnostic hitstop start={HitStopStart:F6} durationRequested={profile.holdDuration:F3}");
                    if(Application.isPlaying && Time.timeScale>0)PrototypeHitStopController.Request(profile.holdDuration,profile.hitStopScale);
                }
            }
            FinishHoldIfNeeded();
        }
        public float MapPresentationTime(float rawElapsed)
        {
            if(!triggered || Age<0)return rawElapsed;
            if(Age<profile.holdDuration)return ContactTime;
            float catchup=Mathf.SmoothStep(0,1,(Age-profile.holdDuration)/profile.catchupDuration);
            return Mathf.Lerp(ContactTime,rawElapsed,catchup);
        }
        private void Update()=>FinishHoldIfNeeded();
        private void FinishHoldIfNeeded()
        {
            if(!triggered || ended || Age<profile.holdDuration)return;
            ended=true;HitStopEnd=preview?previewTime:Time.unscaledTime;
            if(!preview)Debug.Log($"ConvergenceDiagnostic hitstop end={HitStopEnd:F6} actualDuration={HitStopEnd-HitStopStart:F6}");
        }
        public void ApplyRender(Camera camera)
        {
            if(disposed || !isActiveAndEnabled || camera!=view || rendering || RenderOwners.ContainsKey(camera))return;
            float age=Age;if(age<0 || age>=Mathf.Max(profile.pulseDuration,profile.impulseDuration))return;
            rendering=true;RenderOwners[camera]=this;savedPosition=camera.transform.position;savedRotation=camera.transform.rotation;
            float impulse=Mathf.Pow(1-Mathf.Clamp01(age/profile.impulseDuration),3);
            // One hard kick, one small rebound; no long oscillating shake.
            float kick=Mathf.Cos(age/profile.impulseDuration*Mathf.PI*1.5f)*impulse;
            camera.transform.position+=camera.transform.right*(profile.impulseDistance*kick)+camera.transform.up*(profile.impulseDistance*.4f*kick);
            camera.transform.rotation=savedRotation*Quaternion.Euler(0,0,profile.impulseRoll*kick);
            cameraData=camera.GetComponent<UniversalAdditionalCameraData>();
            if(cameraData!=null)
            {
                savedMask=cameraData.volumeLayerMask;savedPost=cameraData.renderPostProcessing;
                cameraData.volumeLayerMask=savedMask.value|(1<<volumeLayer);cameraData.renderPostProcessing=true;
                volume.weight=Mathf.Pow(1-Mathf.Clamp01(age/profile.pulseDuration),2);
                // Respect cameras whose URP volume framework is updated via scripting.
                if(VolumeManager.instance.isInitialized)camera.UpdateVolumeStack();
            }
        }
        public void RestoreRender()
        {
            if(!rendering)return;
            if(view!=null){view.transform.SetPositionAndRotation(savedPosition,savedRotation);RenderOwners.Remove(view);}
            if(cameraData!=null){cameraData.volumeLayerMask=savedMask;cameraData.renderPostProcessing=savedPost;}
            if(volume!=null)volume.weight=0;
            if(cameraData!=null && view!=null && VolumeManager.instance.isInitialized)view.UpdateVolumeStack();
            rendering=false;
        }
        private void BeginCamera(ScriptableRenderContext context,Camera camera)=>ApplyRender(camera);
        private void EndCamera(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=view)return;
            Vector3 centre=view.WorldToViewportPoint(transform.position);
            var stack=VolumeManager.instance.stack;
            LastRenderedBloom=stack.GetComponent<Bloom>().intensity.value;
            LastRenderedExposure=stack.GetComponent<ColorAdjustments>().postExposure.value;
            LastRenderedPulseWeight=volume!=null?volume.weight:0;
            RestoreRender();if(disposed || !isActiveAndEnabled || FlashWeight<=0 || centre.z<=0)return;
            flash.SetVector("_Centre",new Vector4(centre.x,centre.y,view.aspect,0));flash.SetFloat("_Strength",profile.flashStrength*FlashWeight);
            var command=CommandBufferPool.Get("ConvergenceDiagnosticRadial");
            try{command.SetRenderTarget(camera.targetTexture!=null?new RenderTargetIdentifier(camera.targetTexture):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));command.SetViewport(camera.pixelRect);command.DrawProcedural(Matrix4x4.identity,flash,0,MeshTopology.Triangles,3);context.ExecuteCommandBuffer(command);context.Submit();}
            finally{CommandBufferPool.Release(command);}
        }
        private void OnDisable()=>RestoreRender();
        public void Dispose()
        {
            if(disposed)return;disposed=true;RestoreRender();RenderPipelineManager.beginCameraRendering-=BeginCamera;RenderPipelineManager.endCameraRendering-=EndCamera;
            if(triggered && !ended){HitStopEnd=preview?previewTime:Time.unscaledTime;if(!preview)Debug.Log($"ConvergenceDiagnostic presentation cancelled at={HitStopEnd:F6}; shared hitstop retains its unscaled deadline");}
            if(volume!=null){volume.weight=0;volume.enabled=false;volume.sharedProfile=null;}
            if(volumeProfile!=null){foreach(var component in volumeProfile.components)Release(component);Release(volumeProfile);}
            Release(flash);
        }
        private static void Release(Object o){if(o==null)return;if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
        private void OnDestroy()=>Dispose();
    }
}
