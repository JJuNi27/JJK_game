using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JJKGame.Dev.PurpleBodyExploration
{
    public enum PurpleBenchmarkView { Front, Side, SlightLow, Orbit }

    /// <summary>Explicitly opened, isolated render-target bench. Never changes the live lab camera or scene objects.</summary>
    public sealed class PurpleBodyExplorationBench : MonoBehaviour
    {
        private readonly PurpleBodyPrototype[] candidates=new PurpleBodyPrototype[3];
        private Material floorMaterial;
        private RenderTexture target;
        private Camera benchmarkCamera;
        private Vector3 centre;
        private bool initialized;
        public PurpleBodyVariant Selected { get; private set; }
        public PurpleBenchmarkView View { get; private set; }
        public float Clock { get; private set; }=1.5f;
        public bool Playing { get; set; }
        public RenderTexture Preview => target;
        public Camera BenchmarkCamera => benchmarkCamera;
        public Vector3 Centre => centre;
        public PurpleBodyPrototype Candidate(PurpleBodyVariant variant) => candidates[(int)variant];

        public void Configure(PurpleBodyExplorationProfile profile,Camera labCamera,int width=960,int height=540)
        {
            if(initialized) return;
            initialized=true;
            gameObject.hideFlags=HideFlags.DontSave;
            transform.position=new Vector3(1024,0,1024);
            centre=transform.position+Vector3.up*(profile.bodyDiameter*.5f+.35f);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube); floor.name="CodexBenchmarkFloor";
            floor.transform.SetParent(transform,false); floor.transform.localPosition=new Vector3(0,-.10f,0);
            floor.transform.localScale=new Vector3(20,.18f,20);
            var col=floor.GetComponent<Collider>(); col.enabled=false; Destroy(col);
            floorMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit")) { name="PurpleBodyExplore_Floor_Runtime" };
            floorMaterial.SetColor("_BaseColor",new Color(.28f,.32f,.36f)); floorMaterial.SetFloat("_Smoothness",.3f);
            floor.GetComponent<Renderer>().sharedMaterial=floorMaterial;
            for(int i=0;i<3;i++)
            {
                var go=new GameObject("CodexPurpleBodyPrototype_"+(char)('A'+i)); go.transform.SetParent(transform,false);
                go.transform.position=centre;
                candidates[i]=go.AddComponent<PurpleBodyPrototype>(); candidates[i].Configure(profile,(PurpleBodyVariant)i);
                go.SetActive(i==0);
            }
            var cameraObject=new GameObject("CodexPurpleBenchmarkCamera"); cameraObject.transform.SetParent(transform,false);
            benchmarkCamera=cameraObject.AddComponent<Camera>();
            if(labCamera!=null) benchmarkCamera.CopyFrom(labCamera);
            benchmarkCamera.tag="Untagged";
            benchmarkCamera.nearClipPlane=.1f; benchmarkCamera.farClipPlane=40;
            benchmarkCamera.orthographic=false; benchmarkCamera.fieldOfView=55;
            benchmarkCamera.clearFlags=CameraClearFlags.SolidColor;
            benchmarkCamera.backgroundColor=labCamera!=null?labCamera.backgroundColor:new Color(.18f,.20f,.23f);
            benchmarkCamera.allowHDR=true; benchmarkCamera.allowMSAA=false;
            var data=cameraObject.AddComponent<UniversalAdditionalCameraData>();
            var source=labCamera!=null?labCamera.GetComponent<UniversalAdditionalCameraData>():null;
            data.renderPostProcessing=source!=null && source.renderPostProcessing;
            data.volumeLayerMask=source!=null?source.volumeLayerMask:(LayerMask)(~0);
            data.volumeTrigger=source!=null && source.volumeTrigger!=null?source.volumeTrigger:labCamera!=null?labCamera.transform:null;
            data.requiresDepthTexture=true; data.requiresColorTexture=true;
            // URP still shades in HDR internally; use an sRGB display target for both the window and PNG readback.
            target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB) { name="PurpleBodyExplore_Preview_Runtime",hideFlags=HideFlags.DontSave };
            target.Create(); benchmarkCamera.targetTexture=target;
            Sample(1.5f,PurpleBenchmarkView.Front);
        }

        public void Select(PurpleBodyVariant variant)
        {
            Selected=variant;
            for(int i=0;i<candidates.Length;i++) if(candidates[i]!=null) candidates[i].gameObject.SetActive(i==(int)variant);
        }
        public void Sample(float time,PurpleBenchmarkView view)
        {
            Clock=Mathf.Clamp(time,0,3); View=view;
            foreach(var body in candidates) if(body!=null) body.Render(Clock);
            if(benchmarkCamera==null) return;
            Vector3 offset=view==PurpleBenchmarkView.Side?new Vector3(9,0,0):
                view==PurpleBenchmarkView.SlightLow?new Vector3(5.4f,-.75f,-7.2f):
                view==PurpleBenchmarkView.Orbit?Quaternion.Euler(-4,Mathf.Lerp(-50,50,Clock/3f),0)*new Vector3(0,0,-9):new Vector3(0,0,-9);
            benchmarkCamera.transform.SetPositionAndRotation(centre+offset,Quaternion.LookRotation(-offset,Vector3.up));
        }
        private void Update()
        {
            if(!Playing || !initialized) return;
            Sample(Clock+Time.unscaledDeltaTime,View);
            if(Clock>=3) Playing=false;
        }
        public void RenderNow()
        {
            if(benchmarkCamera==null || target==null) return;
            RenderPipeline.SubmitRenderRequest(benchmarkCamera,new UniversalRenderPipeline.SingleCameraRequest { destination=target });
        }
        private void OnDisable() { if(benchmarkCamera!=null) benchmarkCamera.enabled=false; }
        private void OnDestroy()
        {
            if(benchmarkCamera!=null) benchmarkCamera.targetTexture=null;
            if(target!=null) { target.Release(); Destroy(target); }
            if(floorMaterial!=null) Destroy(floorMaterial);
        }
    }
}
