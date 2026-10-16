using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // World-scale presentation owned by the canonical sequence, never an ingredient/projectile child.
    public sealed class PurpleIngredientCasterPressure : MonoBehaviour
    {
        const int Gusts=10,Steps=36,Across=7;
        PurpleIngredientCasterPressureProfile profile;
        Transform owner;
        Vector3 direction;
        float releaseTime,fusionStart,holdStart;
        bool released;
        Material material;
        Mesh mesh;
        MeshRenderer renderer;
        Vector3[] vertices;
        Color[] colors;
        public Vector3 Anchor {get;private set;}
        public float ChargeWeight {get;private set;}
        public float ForwardWeight {get;private set;}
        public string Phase {get;private set;}="Charge";
        public bool Visible=>renderer!=null && renderer.enabled;
        static float H(float n)=>Mathf.Repeat(Mathf.Sin(n*127.1f+31.7f)*43758.5453f,1);
        static float N(float x,float seed){float a=Mathf.Floor(x),p=Mathf.Repeat(x,1);p=p*p*(3-2*p);return Mathf.Lerp(H(a*17+seed),H((a+1)*17+seed),p)*2-1;}
        public void Configure(Transform caster,Vector3 heading,float fusion,float hold,float release,PurpleIngredientCasterPressureProfile settings)
        {
            owner=caster;direction=heading.normalized;fusionStart=fusion;holdStart=hold;releaseTime=release;profile=settings;
            Anchor=caster.position;
            transform.SetPositionAndRotation(Anchor,Quaternion.LookRotation(direction));
            material=new Material(Resources.Load<Shader>("VFX/PurpleIngredientCasterPressure")){name="PurpleIngredientCasterPressure_Air_Runtime"};
            mesh=new Mesh{name="PurpleIngredientCasterPressure_Air_Runtime"};mesh.MarkDynamic();
            vertices=new Vector3[Gusts*(Steps+1)*Across];colors=new Color[vertices.Length];var uv=new Vector2[vertices.Length];
            var indices=new int[Gusts*Steps*(Across-1)*6];int count=0;
            for(int g=0;g<Gusts;g++)for(int n=0;n<=Steps;n++)for(int v=0;v<Across;v++)
            {
                int at=(g*(Steps+1)+n)*Across+v;uv[at]=new Vector2(n/(float)Steps,v/(float)(Across-1));
                if(n<Steps && v<Across-1){indices[count++]=at;indices[count++]=at+Across;indices[count++]=at+1;indices[count++]=at+1;indices[count++]=at+Across;indices[count++]=at+Across+1;}
            }
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;
            var go=new GameObject("CasterPressure_AirVolume");go.transform.SetParent(transform,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.enabled=false;
        }
        public void Sample(float clock,Vector3 bluePosition,Vector3 redPosition)
        {
            if(owner==null || profile==null){if(renderer!=null)renderer.enabled=false;return;}
            float releaseAge=clock-releaseTime;
            if(releaseAge<0){released=false;Anchor=owner.position;}
            else if(!released){Anchor=owner.position;released=true;}
            transform.SetPositionAndRotation(Anchor,Quaternion.LookRotation(direction));
            float q=Mathf.Clamp01(releaseAge/profile.releaseSeconds);
            ChargeWeight=releaseAge<0?Mathf.SmoothStep(0,1,clock/.22f):1-Mathf.SmoothStep(0,1,q);
            ForwardWeight=releaseAge<0?Mathf.SmoothStep(0,1,Mathf.InverseLerp(releaseTime-.18f,releaseTime,clock))*.32f:Mathf.Sin(q*Mathf.PI);
            Phase=releaseAge<0?"Charge":q<1?"Release":"Ended";
            renderer.enabled=ChargeWeight>.001f;
            if(!renderer.enabled)return;
            float t=clock*profile.stormSpeed;
            float proximity=Mathf.SmoothStep(0,1,Mathf.InverseLerp(fusionStart,holdStart,clock));
            Vector3 blue=transform.InverseTransformPoint(bluePosition),red=transform.InverseTransformPoint(redPosition);
            var pair=PurpleIngredientPairPolishProfile.Current;
            float redTick=Mathf.Repeat(clock*(pair!=null?pair.redRate:1.45f),1);
            float tension=pair!=null?pair.redTension:.23f;
            float redBurst=redTick<tension?0:Mathf.Pow(1-(redTick-tension)/(1-tension),2);
            for(int g=0;g<Gusts;g++)
            {
                float tick=t*(1.2f+H(g+9)*1.15f)+g*.381966f,age=Mathf.Repeat(tick,1),epoch=Mathf.Floor(tick),seed=g*71+epoch*43;
                float baseAngle=H(seed+3)*Mathf.PI*2+t*(g%2==0?1.3f:-.9f);
                float span=.95f+H(seed+4)*1.65f;
                float energy=Mathf.Min(1,age/.07f)*Mathf.Pow(Mathf.Max(0,1-age),.42f);
                for(int n=0;n<=Steps;n++)
                {
                    float u=n/(float)Steps;
                    float theta=baseAngle+u*span*(g%3==0?-1:1)+age*1.8f;
                    float radius=profile.stormRadius*(.42f+H(seed+5)*.44f)*(1-.24f*u-.13f*age);
                    radius+=N(u*3+t*2,seed+11)*.42f;
                    float height=g<7?.12f+(1-u)*(.25f+H(seed+7)*1.15f):.5f+u*profile.stormHeight;
                    height+=N(u*3+t*1.5f,seed+13)*.18f;
                    var center=new Vector3(Mathf.Cos(theta)*radius,height,Mathf.Sin(theta)*radius);
                    // Only a local directional nudge; the field's anchor always stays on the caster.
                    Vector3 towardBlue=blue-center,awayRed=center-red;
                    float blueNear=Mathf.Exp(-towardBlue.sqrMagnitude/9),redNear=Mathf.Exp(-awayRed.sqrMagnitude/9);
                    center+=profile.ingredientCoupling*(towardBlue.normalized*blueNear*.9f+awayRed.normalized*redNear*redBurst*.85f)*(clock<holdStart?1:0);
                    float width=profile.flowWidth*(.7f+H(seed+8)*.55f)*(1+.18f*proximity)*(1-.45f*u);
                    Vector3 across=new Vector3(Mathf.Cos(theta),g<7?.18f:.55f,Mathf.Sin(theta));
                    for(int v=0;v<Across;v++)
                    {
                        float s=v/(float)(Across-1)-.5f;
                        Vector3 p=center+across*s*width;
                        // Modest thickness avoids edge-on cards, without large cloth deformation.
                        p.y+=(1-4*s*s)*width*.13f+N(u*4+s*3+t*2,seed+19)*width*.07f;
                        p.x+=N(u*5+s*2+t*3,seed+37)*width*.08f;
                        p.z+=ForwardWeight*profile.forwardSweep*(.35f+u);
                        p.x*=1-ForwardWeight*.22f;
                        int at=(g*(Steps+1)+n)*Across+v;
                        vertices[at]=p;colors[at]=new Color(H(seed+21),proximity,energy,ChargeWeight*energy);
                    }
                }
            }
            mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateBounds();
            material.SetFloat("_PhaseTime",t);material.SetFloat("_Opacity",profile.opacity);material.SetFloat("_Crest",profile.crestEmission);
        }
        public void Dispose()
        {
            if(renderer!=null)renderer.enabled=false;
            if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);material=null;}
            if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);mesh=null;}
        }
        void OnDestroy()=>Dispose();
    }
}
