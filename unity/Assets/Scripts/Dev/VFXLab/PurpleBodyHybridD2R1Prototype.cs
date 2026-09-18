using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleBodyHybridD2R1Prototype : MonoBehaviour
    {
        private PurpleBodyHybridD2R1Profile profile;
        private Material bodyMaterial,ruptureMaterial;
        private readonly LineRenderer[] lines=new LineRenderer[9];
        private readonly Vector3[][] paths=new Vector3[9][];
        private readonly Vector4[] sources=new Vector4[4];
        private static readonly Vector3[] Anchors={new Vector3(-.16f,.105f,-.085f),new Vector3(.145f,-.10f,.045f),new Vector3(.045f,.155f,.16f)};
        private float diameter;
        public void Configure(PurpleBodyHybridD2R1Profile settings)
        {
            profile=settings;diameter=settings.frozenD2.preservedD.commonConditions.bodyDiameter;
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);proxy.name="R1_InternalVolume";proxy.transform.SetParent(transform,false);proxy.transform.localScale=Vector3.one*(diameter/.75f);
            var collider=proxy.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            bodyMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R1")){name="PurpleHybridD2R1_Body_Runtime"};
            var renderer=proxy.GetComponent<Renderer>();renderer.sharedMaterial=bodyMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            bodyMaterial.SetVector("_R1Settings",new Vector4(settings.shellEmission,settings.shellDensity,settings.internalEnergy,settings.instability));
            bodyMaterial.SetVector("_R1Flow",new Vector4(settings.inwardSpeed,settings.turbulenceSpeed,0,0));
            ruptureMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R1Rupture")){name="PurpleHybridD2R1_Rupture_Runtime"};
            for(int i=0;i<9;i++)
            {
                var go=new GameObject(i<6?"R1_HeroRupture_"+i:"R1_OuterFragment_"+i);go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();lines[i]=line;line.sharedMaterial=ruptureMaterial;line.positionCount=i<6?10:3;
                paths[i]=new Vector3[line.positionCount];line.useWorldSpace=true;line.alignment=LineAlignment.View;
                line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.textureMode=LineTextureMode.Stretch;
                line.widthCurve=new AnimationCurve(new Keyframe(0,.65f),new Keyframe(.27f,1),new Keyframe(.43f,.58f),new Keyframe(.62f,.85f),new Keyframe(1,.04f));
            }
            Render(1.5f);
        }
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1f);
        private static Vector3 Direction(float seed){float y=Hash(seed+5)*1.7f-.85f;float a=Hash(seed+7)*Mathf.PI*2;float s=Mathf.Sqrt(1-y*y);return new Vector3(Mathf.Cos(a)*s,y,Mathf.Sin(a)*s);}
        private Vector3 WorldSource(int i)=>transform.position+(Vector3)sources[i]*(diameter/.75f);
        public void Render(float t)
        {
            if(bodyMaterial==null)return;
            sources[0]=new Vector4(.009f*Mathf.Sin(t*3.1f),.007f*Mathf.Cos(t*4.3f),.006f*Mathf.Sin(t*2.7f),.061f);
            Vector4 powers=Vector4.zero;
            for(int i=0;i<4;i++)
            {
                if(i>0){Vector3 p=Quaternion.Euler(0,t*(i%2==0?-13:9),t*(i==3?7:-5))*Anchors[i-1];p+=Direction(i*11)*(.008f*Mathf.Sin(t*(2.1f+i)+i));sources[i]=new Vector4(p.x,p.y,p.z,.043f+(i==2?.007f:0));}
                float spike=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*(7.3f+i*.93f)+i*2.2f)*Mathf.Cos(t*2.17f+i)),8);
                powers[i]=(i==0?1f:.58f)+profile.instability*(spike*.9f+.22f*Mathf.Sin(t*(2.7f+i*.71f)+i*1.8f));
            }
            bodyMaterial.SetVectorArray("_Sources",sources);bodyMaterial.SetVector("_SourcePower",powers);bodyMaterial.SetFloat("_PhaseTime",t);
            ruptureMaterial.SetVector("_BodyCentre",new Vector4(transform.position.x,transform.position.y,transform.position.z,diameter*.5f));
            for(int lane=0;lane<3;lane++)
            {
                float epoch=t/(.37f+lane*.173f)+lane*.37f;float cycle=Mathf.Floor(epoch);float phase=Mathf.Repeat(epoch,1);float seed=cycle*17+lane*43;
                float pulse=Mathf.SmoothStep(0,1,Mathf.Clamp01(phase/.065f))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((phase-(.3f+Hash(seed)*.3f))/.3f)));
                float spike=.72f+.28f*Mathf.Abs(Mathf.Sin(t*39+seed));
                Vector3 axis=Direction(seed),side=Vector3.Cross(axis,Vector3.up).normalized,up=Vector3.Cross(side,axis);
                Vector3 start=WorldSource(lane==0?0:lane);Vector3 end=transform.position+axis*diameter*.5f*profile.outerReach;
                var main=paths[lane*2];var branch=paths[lane*2+1];
                for(int n=0;n<10;n++){float u=n/9f;main[n]=Vector3.Lerp(start,end,u)+(side*(Hash(seed+n*2)-.5f)+up*(Hash(seed+n*3+2)-.5f))*diameter*.22f*Mathf.Sin(u*Mathf.PI);}
                Vector3 branchEnd=transform.position+Direction(seed+19)*diameter*(.43f+.14f*Hash(seed+1));
                for(int n=0;n<10;n++){float u=n/9f;branch[n]=Vector3.Lerp(main[4],branchEnd,u)+side*(Hash(seed+n+50)-.5f)*diameter*.12f*Mathf.Sin(u*Mathf.PI);}
                for(int part=0;part<2;part++)
                {
                    var line=lines[lane*2+part];line.SetPositions(paths[lane*2+part]);
                    line.widthMultiplier=profile.heroWidth*(diameter/5)*(part==0?1:.40f)*(lane==0?1.15f:.85f)*(.7f+.3f*pulse);
                    line.startColor=new Color(1,1,1,pulse*spike);line.endColor=new Color(1,1,1,pulse*spike*.7f);
                }
                float fragmentPhase=Mathf.Repeat(t/(.61f+lane*.17f)+lane*.42f,1);
                var fragment=paths[lane+6];Vector3 origin=transform.position+axis*diameter*(.50f+fragmentPhase*.17f);
                fragment[0]=origin;fragment[1]=origin+axis*diameter*.022f+side*diameter*.015f;fragment[2]=origin+axis*diameter*.045f;
                var f=lines[lane+6];f.SetPositions(fragment);f.widthMultiplier=diameter*.016f;
                float visibility=Mathf.Sin(fragmentPhase*Mathf.PI)*pulse;f.startColor=new Color(1,0,1,visibility);f.endColor=new Color(1,0,1,0);
            }
        }
        private void OnDestroy(){if(bodyMaterial!=null)Destroy(bodyMaterial);if(ruptureMaterial!=null)Destroy(ruptureMaterial);}
    }
}
