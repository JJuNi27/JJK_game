using UnityEngine;
namespace JJKGame.Player
{
    /// <summary>Candidate-only remnants; ends before the protected completed Purple hold.</summary>
    public sealed class PurpleIngredientFlowFirstFusion : MonoBehaviour
    {
        private PurpleIngredientFlowFirstProfile profile;
        private PurpleIngredientFlowFirstVolume blue,red;
        private float scatterAt,endAt,accentAt,fusionAt,bodyRadius;
        private bool captured;
        private Material orbMaterial,flareMaterial;
        private ParticleSystem scatter,flares;
        private Material accentMaterial;
        private readonly LineRenderer[] spikes=new LineRenderer[10],tears=new LineRenderer[5];
        private readonly Vector3[] origins=new Vector3[16],velocity=new Vector3[16];
        private readonly ParticleSystem.Particle[] particles=new ParticleSystem.Particle[16],flashes=new ParticleSystem.Particle[4];
        public int ScatterCount { get; private set; }
        public bool Finished { get; private set; }
        public void Configure(PurpleIngredientFlowFirstProfile settings,PurpleFusionIngredient a,PurpleFusionIngredient b,float fusionStart,float holdStart)
        {
            profile=settings;blue=a.GetComponent<PurpleIngredientFlowFirstVolume>();red=b.GetComponent<PurpleIngredientFlowFirstVolume>();
            var tuning=GojoPolishSettings.Current;
            float contact=PurpleIngredientCollisionAccent.FindContactNormalized(tuning.purpleFormationSeparation,tuning.purpleFormationScale,PurpleIngredientReboot2Profile.Current.fusionVerticalArc);
            scatterAt=Mathf.Lerp(fusionStart,holdStart,contact);endAt=holdStart;
            accentAt=Mathf.Lerp(fusionStart,holdStart,.62f);
            fusionAt=fusionStart;bodyRadius=GojoPolishSettings.PurpleShellDiameter*tuning.purpleVisualScale*1.12f*.5f;
            orbMaterial=new Material(Resources.Load<Shader>("VFX/PurpleIngredientFlowSprite")){name="PurpleIngredientFlowFirst_FusionOrbs"};
            orbMaterial.SetFloat("_Gain",4);flareMaterial=new Material(orbMaterial){name="PurpleIngredientFlowFirst_FusionFlares"};flareMaterial.SetFloat("_Shape",1);
            scatter=PurpleIngredientFlowFirstVolume.CreateParticles(transform,"FusionBlueRedScatter",orbMaterial,16);
            flares=PurpleIngredientFlowFirstVolume.CreateParticles(transform,"RemainingIngredientFlares",flareMaterial,4);
            accentMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")){name="PurpleIngredientFlowFirst_PurpleAccents"};
            accentMaterial.SetColor("_Color",new Color(5.5f,.7f,4.8f));
            for(int i=0;i<spikes.Length;i++)spikes[i]=AccentLine("PurpleRadialSpike_"+i,4);
            for(int i=0;i<tears.Length;i++)tears[i]=AccentLine("PurpleShortTear_"+i,7);
        }
        private LineRenderer AccentLine(string name,int count)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();
            line.useWorldSpace=true;line.positionCount=count;line.sharedMaterial=accentMaterial;line.enabled=false;
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;return line;
        }
        public void EndIfFinished(float elapsed)
        {
            if(elapsed<endAt)return;scatter.Clear();flares.Clear();ScatterCount=0;Finished=true;gameObject.SetActive(false);
        }
        public void Sample(float elapsed)
        {
            SamplePurple(elapsed);
            if(Finished || elapsed<scatterAt || !profile.scatterEnabled)return;
            if(!captured)
            {
                Vector3 centre=(blue.transform.position+red.transform.position)*.5f;
                for(int i=0;i<16;i++)
                {
                    var source=i<8?blue:red;var samples=source.OrbWorldPositions;
                    origins[i]=samples[(i*5)%samples.Length];
                    Vector3 dir=(origins[i]-centre).normalized;
                    velocity[i]=(dir+Vector3.up*.18f)*(8+PurpleIngredientFlowFirstVolume.H(i+8)*8);
                }
                captured=true;
            }
            float age=elapsed-scatterAt,duration=endAt-scatterAt;
            float fade=1-Mathf.SmoothStep(0,1,age/duration);
            ScatterCount=0;
            for(int i=0;i<16;i++)
            {
                Color c=i<8?new Color(.03f,.65f,1,fade):new Color(1,.03f,.08f,fade);
                particles[i]=new ParticleSystem.Particle{position=origins[i]+velocity[i]*(age-.7f*age*age),startLifetime=10,remainingLifetime=10,startColor=c,startSize=(.14f+PurpleIngredientFlowFirstVolume.H(i)*.1f)*Mathf.Sqrt(fade)};
                if(fade>.01f)ScatterCount++;
            }
            scatter.SetParticles(particles,particles.Length);
            for(int i=0;i<flashes.Length;i++)
            {
                Color c=i<2?new Color(.04f,.65f,1):new Color(1,.03f,.1f);c.a=Mathf.Sin(Mathf.Clamp01(age/duration)*Mathf.PI)*.6f;
                flashes[i]=new ParticleSystem.Particle{position=particles[i*4].position,startColor=c,startSize=.34f,startLifetime=10,remainingLifetime=10,rotation=i*27};
            }
            flares.SetParticles(flashes,flashes.Length);
        }
        private void SamplePurple(float elapsed)
        {
            bool active=profile.purpleAccentEnabled && elapsed>=accentAt && elapsed<endAt;
            float age=elapsed-accentAt,progress=Mathf.Clamp01(age/(endAt-accentAt));
            float envelope=Mathf.Sin(progress*Mathf.PI)*profile.accentStrength;
            Vector3 centre=(blue.transform.position+red.transform.position)*.5f;
            float fusion=Mathf.InverseLerp(fusionAt,endAt,elapsed);
            float radius=bodyRadius*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.52f,1,fusion))*1.06f;
            for(int i=0;i<spikes.Length+tears.Length;i++)
            {
                bool spike=i<spikes.Length;var line=spike?spikes[i]:tears[i-spikes.Length];
                float tick=age*(7+PurpleIngredientFlowFirstVolume.H(i+11)*7)+i*.371f;
                float epoch=Mathf.Floor(tick),phase=Mathf.Repeat(tick,1),seed=epoch*73+i*31;
                line.enabled=active && phase<.62f;if(!line.enabled)continue;
                Vector3 direction=Quaternion.Euler(PurpleIngredientFlowFirstVolume.H(seed)*360,PurpleIngredientFlowFirstVolume.H(seed+1)*360,0)*Vector3.right;
                Vector3 tangent=Vector3.Cross(direction,Mathf.Abs(direction.y)<.8f?Vector3.up:Vector3.forward).normalized;
                float length=(spike?1.4f: .75f)+PurpleIngredientFlowFirstVolume.H(seed+3)*(spike?2.7f:1.4f);
                for(int j=0;j<line.positionCount;j++)
                {
                    float u=j/(float)(line.positionCount-1);
                    float jag=(PurpleIngredientFlowFirstVolume.H(seed+j*13)-.5f)*Mathf.Sin(u*Mathf.PI)*(spike?.16f:.9f);
                    line.SetPosition(j,centre+direction*(radius+u*length)+tangent*jag);
                }
                line.startWidth=(spike?.28f:.10f)*envelope;line.endWidth=.005f;
                Color c=Color.white;c.a=(1-phase/.62f)*envelope;line.startColor=c;c.a=0;line.endColor=c;
            }
        }
        private void OnDestroy(){Release(orbMaterial);Release(flareMaterial);Release(accentMaterial);}
        private static void Release(Object obj){if(obj==null)return;if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
    }
}
