using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>One bounded, world-space terminal rupture, owned and clocked by its cast.</summary>
    public sealed class PurpleTerminalBurst : MonoBehaviour
    {
        private readonly LineRenderer[] tears=new LineRenderer[28], shells=new LineRenderer[9];
        private readonly Transform[] debris=new Transform[18];
        private readonly Transform[] dustPuffs=new Transform[8];
        private readonly Vector3[] points=new Vector3[24];
        private Material filament, stone, distortion, dust;
        private PurpleEnergyBody compression;
        private Transform warp;
        private Vector3 ground;
        private bool hasGround;
        private float scale, duration;
        public float Age { get; private set; }

        public void Configure(Vector3 position,Vector3 forward,float bodyScale)
        {
            transform.SetPositionAndRotation(position,Quaternion.LookRotation(forward));
            var tuning=GojoPolishSettings.Current;
            scale=bodyScale*tuning.purpleTerminalScale; duration=tuning.purpleTerminalDuration;
            filament=new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")) { name="PurpleTerminalFilament_Runtime" };
            stone=new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name="PurpleTerminalDebris_Runtime" };
            stone.SetColor("_BaseColor",new Color(.085f,.065f,.09f));
            dust=new Material(Resources.Load<Shader>("VFX/HollowPurpleDust")) { name="PurpleTerminalDust_Runtime" };
            var body=new GameObject("TerminalCompressionCore"); body.transform.SetParent(transform,false);
            compression=body.AddComponent<PurpleEnergyBody>(); compression.Configure();
            for(int i=0;i<tears.Length;i++) tears[i]=Line("TerminalBranchedTear_"+i);
            for(int i=0;i<shells.Length;i++) shells[i]=Line("TerminalBrokenPressureShell_"+i);
            var hits=Physics.RaycastAll(position+Vector3.up*2,Vector3.down,12,~0,QueryTriggerInteraction.Ignore);
            float nearest=float.PositiveInfinity;
            foreach(var hit in hits)
                if(hit.collider.GetComponentInParent<Health>()==null && hit.distance<nearest)
                { nearest=hit.distance; ground=hit.point+hit.normal*.08f; hasGround=true; }
            for(int i=0;i<debris.Length;i++)
            {
                var shard=GameObject.CreatePrimitive(PrimitiveType.Cube); shard.name="TerminalSurfaceShard_"+i;
                shard.transform.SetParent(transform,false); debris[i]=shard.transform;
                var col=shard.GetComponent<Collider>(); col.enabled=false; Destroy(col);
                var mr=shard.GetComponent<Renderer>(); mr.sharedMaterial=stone; mr.shadowCastingMode=ShadowCastingMode.Off;
                shard.SetActive(false);
            }
            for(int i=0;i<dustPuffs.Length;i++)
            {
                var puff=GameObject.CreatePrimitive(PrimitiveType.Quad); puff.name="TerminalSurfaceDust_"+i;
                puff.transform.SetParent(transform,false); dustPuffs[i]=puff.transform;
                var col=puff.GetComponent<Collider>(); col.enabled=false; Destroy(col);
                var mr=puff.GetComponent<Renderer>(); mr.sharedMaterial=dust; mr.shadowCastingMode=ShadowCastingMode.Off;
                puff.SetActive(false);
            }
            var w=GameObject.CreatePrimitive(PrimitiveType.Sphere); w.name="TerminalSpatialRupture";
            warp=w.transform; warp.SetParent(transform,false);
            var wc=w.GetComponent<Collider>(); wc.enabled=false; Destroy(wc);
            distortion=new Material(Resources.Load<Shader>("VFX/GojoBlueDistortion")) { name="PurpleTerminalWarp_Runtime" };
            w.GetComponent<Renderer>().sharedMaterial=distortion; w.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            distortion.SetFloat("_RadialSign",1); Render(0);
        }

        private LineRenderer Line(string name)
        {
            var go=new GameObject(name); go.transform.SetParent(transform,false);
            var l=go.AddComponent<LineRenderer>(); l.sharedMaterial=filament; l.useWorldSpace=false;
            l.positionCount=24; l.numCapVertices=1; l.shadowCastingMode=ShadowCastingMode.Off; return l;
        }
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f)*43758.5453f,1);

        public void Render(float age)
        {
            Age=age;
            float t=Mathf.Clamp01(age/duration), rupture=Mathf.Clamp01((age-.065f)/.28f);
            float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.22f,1,t));
            compression.gameObject.SetActive(age<.17f);
            if(age<.17f)
            {
                float compress=1-Mathf.Clamp01(age/.065f)*.63f;
                compression.transform.localScale=Vector3.one*scale*(age<.065f?compress:.4f+rupture*2.5f);
                compression.Render(age,1.6f,age<.065f?.65f:5f*(1-(age-.065f)/.105f),1,age,false,.85f);
            }
            float radius=(.65f+Mathf.Pow(rupture,.6f)*4.8f)*scale;
            warp.gameObject.SetActive(age>.04f && t<.65f);
            warp.localScale=Vector3.one*radius*2;
            distortion.SetFloat("_WorldRadius",radius); distortion.SetFloat("_Strength",fade*.6f); distortion.SetFloat("_Impact",fade);
            for(int i=0;i<tears.Length;i++)
            {
                bool branch=i>=18;
                var l=tears[i]; l.enabled=age>=.065f && t<.85f && (t<.3f || Hash(i*9+Mathf.Floor(age*19))>.55f);
                if(!l.enabled) continue;
                Vector3 dir=Quaternion.Euler(i*71,i*137,i*47)*Vector3.up;
                Vector3 tangent=Vector3.Cross(dir,Vector3.forward).normalized;
                float length=radius*(.65f+Hash(i*13)*.65f);
                Vector3 origin=branch?tears[(i-18)*2].GetPosition(10):Vector3.zero;
                for(int j=0;j<24;j++)
                {
                    float u=j/23f;
                    points[j]=origin+dir*length*u*(branch?.48f:1)+tangent*(Hash(i*37+j*7+Mathf.Floor(age*24))-.5f)*.65f*u*scale;
                }
                l.SetPositions(points); l.startWidth=(branch?.05f:.12f+Hash(i)*.10f)*scale*fade; l.endWidth=.004f;
                Color c=i%4==0?new Color(4,2.8f,4.8f,fade):new Color(1.8f,.08f,3,fade*.8f);
                l.startColor=c; c.a=0; l.endColor=c;
            }
            for(int i=0;i<shells.Length;i++)
            {
                var l=shells[i]; l.enabled=age>=.065f && t<.75f;
                Quaternion plane=Quaternion.Euler(i*37,i*71,i*43);
                for(int j=0;j<24;j++)
                {
                    float a=i*2.4f+j/23f*(.65f+Hash(i)*.8f);
                    points[j]=plane*new Vector3(Mathf.Cos(a),Mathf.Sin(a),Mathf.Sin(a*7+i)*.09f)*radius*(.7f+Hash(i*5)*.25f);
                }
                l.SetPositions(points); l.startWidth=.12f*scale*fade; l.endWidth=.012f;
                Color c=new Color(.8f,.07f,1.6f,fade*.5f); l.startColor=c; c.a=0; l.endColor=c;
            }
            for(int i=0;i<debris.Length;i++)
            {
                var d=debris[i]; d.gameObject.SetActive(hasGround && age>.065f && t<1);
                float a=i*2.399f, travel=Mathf.Max(0,age-.065f);
                d.position=ground+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(.8f+travel*(2+Hash(i)*6))*scale
                    +Vector3.up*Mathf.Max(0,travel*(3+Hash(i*5)*3)-travel*travel*5);
                d.localScale=new Vector3(.10f+Hash(i)*.25f,.05f,.18f+Hash(i*7)*.24f)*scale*fade;
                d.localRotation=Quaternion.Euler(i*37+age*170,i*71,i*53+age*90);
            }
            dust.SetFloat("_Opacity",fade); dust.SetFloat("_PhaseTime",age*2);
            for(int i=0;i<dustPuffs.Length;i++)
            {
                var d=dustPuffs[i]; d.gameObject.SetActive(hasGround && age>.09f && t<1);
                float a=i*2.399f;
                d.position=ground+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(.5f+rupture*2.7f)*scale+Vector3.up*(.25f+t*.45f);
                d.localScale=new Vector3(1.5f+t*2, .55f+t, 1)*scale;
                // Crossed vertical cards keep dust readable to every observer without a camera dependency.
                d.rotation=Quaternion.Euler(0,i*47,0);
            }
        }
        private void OnDestroy()
        { if(filament!=null) Destroy(filament); if(stone!=null) Destroy(stone); if(distortion!=null) Destroy(distortion); if(dust!=null) Destroy(dust); }
    }
}
