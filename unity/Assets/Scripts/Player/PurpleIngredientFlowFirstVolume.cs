using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    /// <summary>Short brush strokes advect through a force field; no orbiting solid geometry.</summary>
    public sealed class PurpleIngredientFlowFirstVolume : MonoBehaviour
    {
        private const int Nodes=20;
        private PurpleIngredientFlowFirstProfile profile;
        private bool blue;
        private Material brush;
        private LineRenderer[] flows,ground;
        private Material orbMaterial,flareMaterial;
        private ParticleSystem orbs,flares;
        private ParticleSystem.Particle[] orbParticles,flareParticles;
        public Vector3[] OrbWorldPositions { get; private set; }
        private readonly Vector3[] points=new Vector3[Nodes];
        public static float H(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+19.3f)*43758.5453f,1);
        public void Configure(bool inward,PurpleIngredientFlowFirstProfile settings)
        {
            blue=inward;profile=settings;
            brush=new Material(Resources.Load<Shader>("VFX/PurpleIngredientFlowBrush")){name="PurpleIngredientFlowFirst_Brush"};
            flows=new LineRenderer[profile.flowCount];ground=new LineRenderer[4];
            for(int i=0;i<flows.Length;i++)flows[i]=CreateLine("FlowBrush_"+i);
            for(int i=0;i<ground.Length;i++)ground[i]=CreateLine("SpaceGust_"+i);
            orbMaterial=new Material(Resources.Load<Shader>("VFX/PurpleIngredientFlowSprite")){name="PurpleIngredientFlowFirst_Orbs"};
            flareMaterial=new Material(orbMaterial){name="PurpleIngredientFlowFirst_Flares"};flareMaterial.SetFloat("_Shape",1);
            orbParticles=new ParticleSystem.Particle[profile.orbCount];flareParticles=new ParticleSystem.Particle[3];
            OrbWorldPositions=new Vector3[profile.orbCount];
            orbs=CreateParticles(transform,"SmallEnergyOrbs",orbMaterial,profile.orbCount);
            flares=CreateParticles(transform,"BriefCrossFlares",flareMaterial,3);
        }
        private LineRenderer CreateLine(string name)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=Nodes;
            line.sharedMaterial=brush;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
            line.numCornerVertices=0;line.textureMode=LineTextureMode.Stretch;
            line.widthCurve=new AnimationCurve(new Keyframe(0,.22f),new Keyframe(.23f,1),new Keyframe(.72f,.65f),new Keyframe(1,0));
            line.enabled=false;return line;
        }
        public void Sample(float clock,float fusion,Vector3 opposite)
        {
            bool on=blue?profile.blueFlowEnabled:profile.redFlowEnabled;
            float weight=on?PurpleIngredientBurstVolume.Weight(fusion):0;
            brush.SetFloat("_PhaseTime",clock);
            brush.SetFloat("_Gain",profile.flowBrightness*2.5f);
            Color color=blue?new Color(.045f,.62f,1):new Color(1,.035f,.12f);
            color=Color.Lerp(color,new Color(.8f,.04f,1),Mathf.Clamp01((fusion-.2f)*.7f));
            for(int i=0;i<flows.Length;i++)
            {
                float tick=clock*profile.flowSpeed*(blue?.8f+H(i+8)*.55f:1.15f+H(i+9)*.75f)+i*(blue?.317f:.417f);
                float epoch=Mathf.Floor(tick),p=Mathf.Repeat(tick,1),seed=i*31+epoch*57;
                var line=flows[i];line.enabled=weight>.001f;
                if(!line.enabled)continue;
                var plane=Quaternion.Euler(H(seed+1)*180,H(seed+2)*360,H(seed+3)*360);
                for(int j=0;j<Nodes;j++)
                {
                    float u=j/(float)(Nodes-1),q=Mathf.Clamp01(p-u*.25f);
                    float radius,angle;
                    if(blue)
                    {
                        radius=Mathf.Lerp(profile.reach,.48f,Mathf.Pow(q,1.65f));
                        angle=seed*.23f+(1-q)*1.85f;
                    }
                    else
                    {
                        // A bowed pressure front grows away from the surface, rather than orbiting.
                        radius=Mathf.Lerp(1.04f,profile.reach,Mathf.Pow(p,.64f))-u*.28f;
                        angle=seed*.37f+u*(.23f+p*.34f)+.04f*Mathf.Sin(p*17+seed);
                    }
                    points[j]=plane*new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,Mathf.Sin(q*9+seed)*(blue?.12f:.07f));
                }
                line.SetPositions(points);line.widthMultiplier=profile.brushWidth*(.65f+H(seed+4)*.65f)*(blue?1:1.25f*Mathf.Sin(p*Mathf.PI));
                float fade=Mathf.SmoothStep(0,1,p/.13f)*(1-Mathf.SmoothStep(0,1,(p-.85f)/.15f))*weight;
                Color c=color;c.a=fade;line.startColor=c;c.a*=.15f;line.endColor=c;
            }
            for(int i=0;i<ground.Length;i++)
            {
                var line=ground[i];line.enabled=weight>.001f && profile.groundStrength>0;if(!line.enabled)continue;
                float p=Mathf.Repeat(clock*1.15f+i*.263f,1),angle=i*1.57f+clock*.21f;
                float groundY=(.09f-transform.position.y)/Mathf.Max(.01f,transform.lossyScale.y);
                for(int j=0;j<Nodes;j++)
                {
                    float q=Mathf.Clamp01(p-j/(float)(Nodes-1)*.19f);
                    float r=blue?Mathf.Lerp(profile.reach*1.35f,.75f,q*q):Mathf.Lerp(.85f,profile.reach*1.25f,Mathf.Sqrt(q));
                    float a=angle+(blue?(1-q)*1.5f:q*.3f);
                    points[j]=new Vector3(Mathf.Cos(a)*r,groundY,Mathf.Sin(a)*r);
                }
                line.SetPositions(points);line.widthMultiplier=profile.brushWidth*1.4f;
                Color c=color;c.a=Mathf.Sin(p*Mathf.PI)*profile.groundStrength*weight;line.startColor=c;c.a=0;line.endColor=c;
            }
            SampleDetails(clock,fusion,color);
        }
        public static ParticleSystem CreateParticles(Transform parent,string name,Material mat,int count)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=count;main.startSpeed=0;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.renderMode=ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.minParticleSize=0;renderer.maxParticleSize=.5f;
            return ps;
        }
        private void SampleDetails(float clock,float fusion,Color tint)
        {
            if(!profile.detailsEnabled){orbs.Clear();flares.Clear();return;}
            float fade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.68f,.91f,fusion));
            for(int i=0;i<orbParticles.Length;i++)
            {
                float tick=clock*(blue?.8f:1.65f)*(1+H(i+3)*.45f)+i*.217f;
                float epoch=Mathf.Floor(tick),p=Mathf.Repeat(tick,1),seed=epoch*71+i*19+(blue?0:113);
                float advance=blue?Mathf.Clamp01((p-.17f)/.83f):Mathf.Pow(p,.7f);
                float radius=blue?Mathf.Lerp(2.7f,.65f,advance*advance):Mathf.Lerp(1.05f,3.1f,advance);
                float angle=seed+(blue?(1-advance)*1.65f:p*.23f);
                Vector3 local=Quaternion.Euler(H(seed+2)*170,H(seed+3)*360,0)*new Vector3(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius,.2f*Mathf.Sin(seed+p*4));
                OrbWorldPositions[i]=transform.TransformPoint(local);
                var particle=new ParticleSystem.Particle{position=OrbWorldPositions[i],startLifetime=10,remainingLifetime=10,startSize=(.065f+H(seed+5)*.095f)*transform.lossyScale.x};
                Color c=tint;c.a=Mathf.Sin(p*Mathf.PI)*fade;particle.startColor=c;orbParticles[i]=particle;
            }
            orbs.SetParticles(orbParticles,orbParticles.Length);
            for(int i=0;i<flareParticles.Length;i++)
            {
                float tick=clock*(1.1f+H(i+3)*.6f)+i*.373f,epoch=Mathf.Floor(tick),p=Mathf.Repeat(tick,1);
                float seed=epoch*37+i*23+(blue?0:351),life=.18f;
                Vector3 direction=Quaternion.Euler(H(seed)*360,H(seed+1)*360,0)*Vector3.right;
                Color c=tint;c.a=p<life?Mathf.Sin(p/life*Mathf.PI)*fade:0;
                flareParticles[i]=new ParticleSystem.Particle{position=transform.TransformPoint(direction*(1.3f+H(seed+2)*1.25f)),startLifetime=10,remainingLifetime=10,startColor=c,startSize=(.18f+H(seed+4)*.28f)*transform.lossyScale.x,rotation=H(seed+5)*65-30};
            }
            flares.SetParticles(flareParticles,flareParticles.Length);
        }
        private void OnDestroy(){Release(brush);Release(orbMaterial);Release(flareMaterial);}
        private static void Release(Object obj){if(obj==null)return;if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
    }
}
