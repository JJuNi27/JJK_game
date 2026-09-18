using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleBodyHybridD2Prototype : MonoBehaviour
    {
        private Material bodyMaterial,ruptureMaterial;
        private PurpleBodyHybridD2Profile profile;
        private readonly LineRenderer[] lines=new LineRenderer[12];
        private readonly Vector3[][] paths=new Vector3[6][];
        private float diameter;
        public Material BodyMaterial=>bodyMaterial;
        public void Configure(PurpleBodyHybridD2Profile settings)
        {
            profile=settings;var common=settings.preservedD.commonConditions;diameter=common.bodyDiameter;
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);proxy.name="D2_LuminousDensity";proxy.transform.SetParent(transform,false);proxy.transform.localScale=Vector3.one*(diameter/.75f);
            var col=proxy.GetComponent<Collider>();col.enabled=false;Destroy(col);
            bodyMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2")){name="PurpleHybridD2_Body_Runtime"};
            var renderer=proxy.GetComponent<Renderer>();renderer.sharedMaterial=bodyMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            bodyMaterial.SetVector("_D2Energy",new Vector4(settings.luminosity,settings.darkSeparation,settings.plasmaEnergy,settings.ruptureEnergy));
            bodyMaterial.SetVector("_D2Flow",new Vector4(settings.inwardSpeed,settings.turbulenceSpeed,0,0));
            ruptureMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2Rupture")){name="PurpleHybridD2_Rupture_Runtime"};
            for(int i=0;i<6;i++)
            {
                paths[i]=new Vector3[8];
                for(int glow=0;glow<2;glow++)
                {
                    var go=new GameObject("D2_Rupture_"+i+(glow==0?"_Core":"_HotEdge"));go.transform.SetParent(transform,false);
                    var line=go.AddComponent<LineRenderer>();lines[i*2+glow]=line;line.sharedMaterial=ruptureMaterial;line.positionCount=8;
                    line.useWorldSpace=true;line.alignment=LineAlignment.View;line.textureMode=LineTextureMode.Stretch;
                    line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                    line.widthCurve=new AnimationCurve(new Keyframe(0,.65f),new Keyframe(.28f,1),new Keyframe(.61f,.58f),new Keyframe(1,.08f));
                }
            }
            Render(1.5f);
        }
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1f);
        private static Vector3 Direction(float seed)
        {float y=Hash(seed+5)*1.6f-.8f;float angle=Hash(seed+7)*Mathf.PI*2;float s=Mathf.Sqrt(1-y*y);return new Vector3(Mathf.Cos(angle)*s,y,Mathf.Sin(angle)*s);}
        public void Render(float time)
        {
            if(bodyMaterial==null)return;bodyMaterial.SetFloat("_PhaseTime",time);
            ruptureMaterial.SetVector("_BodyCentre",new Vector4(transform.position.x,transform.position.y,transform.position.z,diameter*.5f));
            for(int lane=0;lane<3;lane++)
            {
                float interval=.31f+lane*.137f;float epoch=time/interval+lane*.37f;float phase=Mathf.Repeat(epoch,1f);float seed=Mathf.Floor(epoch)*17+lane*43;
                float pulse=Mathf.SmoothStep(0,1,Mathf.Clamp01(phase/.12f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-.62f)/.38f)));
                float energy=pulse*(.65f+.35f*Mathf.Abs(Mathf.Sin(time*47+seed)))*profile.ruptureEnergy;
                Vector3 axis=Direction(seed),side=Vector3.Cross(axis,Vector3.up).normalized,up=Vector3.Cross(side,axis);
                Vector3 start=transform.position+Direction(seed+11)*(lane==0?.07f:.24f)*diameter;
                Vector3 end=transform.position+axis*diameter*(.47f+Hash(seed+3)*.14f);
                var main=paths[lane*2];var branch=paths[lane*2+1];
                for(int n=0;n<8;n++)
                {
                    float u=n/7f;float envelope=Mathf.Sin(u*Mathf.PI);
                    main[n]=Vector3.Lerp(start,end,u)+(side*(Hash(seed+n*2)-.5f)+up*(Hash(seed+n*3+2)-.5f))*diameter*.16f*envelope;
                }
                Vector3 branchEnd=transform.position+Direction(seed+19)*diameter*(.37f+Hash(seed+4)*.19f);
                for(int n=0;n<8;n++)
                {float u=n/7f;branch[n]=Vector3.Lerp(main[3],branchEnd,u)+side*(Hash(seed+n+50)-.5f)*diameter*.09f*Mathf.Sin(u*Mathf.PI);}
                for(int part=0;part<2;part++)for(int glow=0;glow<2;glow++)
                {
                    var line=lines[(lane*2+part)*2+glow];line.SetPositions(paths[lane*2+part]);
                    line.widthMultiplier=profile.ruptureWidth*(diameter/5f)*(part==0?1:.55f)*(glow==0?1:3.1f)*(.6f+.4f*pulse);
                    var colour=glow==0?new Color(3.8f,2.5f,4.2f,energy):new Color(2.5f,.025f,1.1f,energy*.32f);
                    line.startColor=colour;line.endColor=new Color(colour.r,colour.g,colour.b,colour.a*.35f);
                }
            }
        }
        private void OnDestroy(){if(bodyMaterial!=null)Destroy(bodyMaterial);if(ruptureMaterial!=null)Destroy(ruptureMaterial);}
    }
}
