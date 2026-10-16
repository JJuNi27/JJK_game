using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // Same FlowFirst sampling/lifecycle interface, so the existing fusion remnants consume these orbs.
    public sealed class PurpleIngredientPairPolishVolume : PurpleIngredientFlowFirstVolume
    {
        const int Nodes=20;
        PurpleIngredientPairPolishProfile tuning;
        PurpleIngredientFlowFirstProfile source;
        bool blue;
        Material brush,orbMaterial,flareMaterial;
        LineRenderer[] flows,ground;
        ParticleSystem orbs,flares;
        ParticleSystem.Particle[] particles,flashes;
        readonly Vector3[] points=new Vector3[Nodes];
        Vector3 toward;
        float coupling;
        public float MeanVisibleRadius { get; private set; }
        public float ForcePeak { get; private set; }
        public override void Configure(bool inward,PurpleIngredientFlowFirstProfile settings)
        {
            blue=inward;source=settings;tuning=PurpleIngredientPairPolishProfile.Current;
            brush=new Material(Resources.Load<Shader>("VFX/PurpleIngredientPairBrush")){name="PurpleIngredientPair_Brush"};
            brush.SetFloat("_Polarity",blue?-1:1);brush.SetFloat("_Breakup",tuning.coverageBreakup);
            orbMaterial=new Material(Resources.Load<Shader>("VFX/PurpleIngredientFlowSprite")){name="PurpleIngredientPair_Orbs"};orbMaterial.SetFloat("_Gain",4);
            flareMaterial=new Material(orbMaterial){name="PurpleIngredientPair_Flares"};flareMaterial.SetFloat("_Shape",1);flareMaterial.SetFloat("_Gain",5);
            flows=new LineRenderer[source.flowCount];ground=new LineRenderer[4];
            for(int i=0;i<flows.Length;i++)flows[i]=Line("PairForceFlow_"+i);
            for(int i=0;i<ground.Length;i++)ground[i]=Line("PairSpaceResponse_"+i);
            OrbWorldPositions=new Vector3[source.orbCount];particles=new ParticleSystem.Particle[source.orbCount];flashes=new ParticleSystem.Particle[3];
            orbs=CreateParticles(transform,"SmallEnergyOrbs",orbMaterial,particles.Length);
            flares=CreateParticles(transform,"BriefCrossFlares",flareMaterial,flashes.Length);
        }
        LineRenderer Line(string label)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=Nodes;line.sharedMaterial=brush;
            line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.textureMode=LineTextureMode.Stretch;
            line.widthCurve=new AnimationCurve(new Keyframe(0,.08f),new Keyframe(.34f,.75f),new Keyframe(.77f,1),new Keyframe(1,.12f));
            line.enabled=false;return line;
        }
        void Event(float clock,int i,out float p,out float seed)
        {
            // Different frequencies and offsets prevent synchronized pressure gaps. No Random state writes.
            float rate=blue?tuning.blueRate*(.8f+H(i+8)*.55f):tuning.redRate*(.7f+H(i+9)*.85f);
            float tick=clock*rate+i*(blue?.317f:.417f);
            p=Mathf.Repeat(tick,1);seed=i*31+Mathf.Floor(tick)*57+(blue?0:113);
        }
        Vector3 Path(float p,float seed)
        {
            float reach=source.reach*tuning.reachMultiplier;
            var plane=Quaternion.Euler(H(seed+1)*180,H(seed+2)*360,H(seed+3)*360);
            Vector3 local;
            if(blue)
            {
                float advance=Mathf.Pow(Mathf.Clamp01(p),2.3f);
                float r=Mathf.Lerp(reach,.24f,advance);
                float a=seed*.23f+(1-p)*2.35f;
                local=plane*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,Mathf.Sin(p*7+seed)*.13f);
            }
            else
            {
                float q=Mathf.Clamp01((p-tuning.redTension)/(1-tuning.redTension));
                float r=Mathf.Lerp(1.02f,reach,Mathf.Pow(q,.57f));
                local=plane*new Vector3(Mathf.Cos(seed*.37f)*r,Mathf.Sin(seed*.37f)*r,.12f*Mathf.Sin(seed));
            }
            // Bend force paths toward the shared gap; never move either ingredient transform.
            local+=toward*coupling*Mathf.Sin(Mathf.Clamp01(p)*Mathf.PI)*(blue?.65f:-.38f);
            return local;
        }
        public override void Sample(float clock,float fusion,Vector3 opposite)
        {
            toward=transform.InverseTransformDirection((opposite-transform.position).normalized);
            coupling=tuning.relationshipBend*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.04f,.48f,fusion));
            float weight=(blue?source.blueFlowEnabled:source.redFlowEnabled)?PurpleIngredientBurstVolume.Weight(fusion):0;
            var tint=blue?new Color(.025f,.62f,1):new Color(1,.045f,.105f);
            tint=Color.Lerp(tint,new Color(.75f,.06f,1),Mathf.Clamp01((fusion-.3f)*.5f));
            brush.SetFloat("_PhaseTime",clock);brush.SetFloat("_Gain",tuning.emission*2.5f);
            MeanVisibleRadius=0;ForcePeak=0;
            for(int i=0;i<flows.Length;i++)
            {
                Event(clock,i,out float p,out float seed);
                var line=flows[i];line.enabled=weight>.001f;if(!line.enabled)continue;
                float burst=Mathf.Clamp01((p-tuning.redTension)/(1-tuning.redTension));
                float strength=blue?Mathf.SmoothStep(0,1,p/.09f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.91f,1,p))):
                    (p<tuning.redTension?.13f*Mathf.Sin(p/tuning.redTension*Mathf.PI):Mathf.Pow(1-burst,.65f));
                for(int j=0;j<Nodes;j++)
                {
                    float u=j/(float)(Nodes-1);
                    if(blue)points[j]=Path(Mathf.Clamp01(p-(1-u)*tuning.blueTrailSpan*(1-.45f*p)),seed);
                    else
                    {
                        var centre=Path(p,seed);var n=centre.normalized;
                        var side=Vector3.Cross(n,Mathf.Abs(n.y)>.8f?Vector3.right:Vector3.up).normalized;
                        var up=Vector3.Cross(side,n);
                        float spread=tuning.pressureSpread*(.12f+burst*1.35f);
                        float bend=(u-.5f)*spread;
                        points[j]=centre+side*bend-up*burst*(.1f+.16f*Mathf.Sin(seed+u*7))-
                            n*(u-.5f)*(u-.5f)*spread*.7f;
                    }
                }
                line.SetPositions(points);
                line.widthMultiplier=tuning.flowWidth*(.8f+H(seed+4)*.5f)*(blue?1-.6f*p:.4f+1.35f*burst);
                Color c=tint;c.a=strength*weight;line.endColor=c;c.a*=blue?.28f:.8f;line.startColor=c;
                MeanVisibleRadius+=Path(p,seed).magnitude/flows.Length;ForcePeak=Mathf.Max(ForcePeak,strength);
            }
            for(int i=0;i<ground.Length;i++)
            {
                Event(clock*.7f,i+17,out float p,out float seed);
                float advance=blue?p*p:Mathf.Sqrt(Mathf.Clamp01((p-tuning.redTension)/(1-tuning.redTension)));
                float groundY=(.1f-transform.position.y)/Mathf.Max(.01f,transform.lossyScale.y);
                for(int j=0;j<Nodes;j++)
                {
                    float u=j/(float)(Nodes-1),q=Mathf.Clamp01(advance-(1-u)*.16f);
                    float r=blue?Mathf.Lerp(source.reach*tuning.reachMultiplier*1.2f,.65f,q):Mathf.Lerp(.7f,source.reach*tuning.reachMultiplier,q);
                    float a=seed*.3f+(blue?(1-q)*1.5f:u*.15f);
                    points[j]=new Vector3(Mathf.Cos(a)*r,groundY,Mathf.Sin(a)*r);
                }
                var line=ground[i];line.enabled=weight>.001f;line.SetPositions(points);line.widthMultiplier=tuning.flowWidth*1.7f;
                Color c=tint;c.a=Mathf.Sin(p*Mathf.PI)*tuning.groundResponse*weight;line.endColor=c;c.a*=.15f;line.startColor=c;
            }
            float details=source.detailsEnabled?weight:0;
            for(int i=0;i<particles.Length;i++)
            {
                Event(clock,i%flows.Length,out float p,out float seed);
                var pos=Path(p,seed);OrbWorldPositions[i]=transform.TransformPoint(pos);
                float visible=blue?Mathf.SmoothStep(0,1,p/.1f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.94f,1,p))):
                    (p<tuning.redTension?.24f:Mathf.Pow(1-Mathf.Clamp01((p-tuning.redTension)/(1-tuning.redTension)),.8f));
                Color c=tint;c.a=visible*details;
                particles[i]=new ParticleSystem.Particle{position=OrbWorldPositions[i],startLifetime=10,remainingLifetime=10,startColor=c,
                    startSize=tuning.moteSize*(.8f+H(seed+5)*.7f)*(blue?1-.65f*p:1+.4f*p)*transform.lossyScale.x};
            }
            orbs.SetParticles(particles,particles.Length);
            for(int i=0;i<flashes.Length;i++)
            {
                Event(clock,i*3,out float p,out float seed);
                // The flare is tied to arrival / pressure release, not an unrelated repeating decoration.
                float eventP=blue?Mathf.InverseLerp(.84f,.98f,p):Mathf.InverseLerp(tuning.redTension,tuning.redTension+.13f,p);
                float peak=(eventP>0 && eventP<1)?Mathf.Sin(eventP*Mathf.PI):0;
                var pos=Path(blue?.9f:tuning.redTension,seed).normalized*(blue?1.035f:1.12f);
                Color c=Color.Lerp(tint,Color.white,blue?.3f:.08f);c.a=peak*details;
                flashes[i]=new ParticleSystem.Particle{position=transform.TransformPoint(pos),startLifetime=10,remainingLifetime=10,startColor=c,
                    startSize=tuning.arrivalFlare*peak*transform.lossyScale.x,rotation=H(seed+5)*90};
            }
            flares.SetParticles(flashes,flashes.Length);
        }
        protected override void OnDestroy()
        {
            Release(brush);Release(orbMaterial);Release(flareMaterial);
            base.OnDestroy();
        }
        static void Release(Object item){if(item==null)return;if(Application.isPlaying)Destroy(item);else DestroyImmediate(item);}
    }
}
