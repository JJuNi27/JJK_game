using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Sequence-clocked volume, curved sheets and intermittent discharges. No gameplay.</summary>
    public sealed class PurpleEnergyBody : MonoBehaviour
    {
        private const int RibbonCount = 7, RibbonSteps = 40, BoltCount = 24, MoteCount = 18;
        private Material volumeMaterial, filamentMaterial;
        private readonly Mesh[] ribbons = new Mesh[RibbonCount];
        private readonly Vector3[][] vertices = new Vector3[RibbonCount][];
        private readonly Color[][] colours = new Color[RibbonCount][];
        private readonly LineRenderer[] bolts = new LineRenderer[BoltCount + MoteCount];
        private readonly Vector3[] boltPoints = new Vector3[13];
        private GojoPolishSettings tuning;
        private ProductionPurpleVisual integrated;
        public bool UsesProductionCandidate => integrated != null;

        public void Configure(Vector3 travelVelocity = default, bool useProductionCandidate = false)
        {
            tuning = GojoPolishSettings.Current;
            if (useProductionCandidate && ProductionPurpleVisualProfile.Current != null && ProductionPurpleVisualProfile.Current.useIntegratedCandidate)
            {
                integrated = gameObject.AddComponent<ProductionPurpleVisual>();
                integrated.Configure(travelVelocity);
                return;
            }
            volumeMaterial = new Material(Resources.Load<Shader>("VFX/HollowPurpleVolume")) { name = "PurpleVolume_Runtime" };
            filamentMaterial = new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")) { name = "PurpleFilaments_Runtime" };
            var volume = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            volume.name = "PurpleDepthVolume";
            volume.transform.SetParent(transform, false);
            volume.transform.localScale = Vector3.one * 5.3f;
            var collider = volume.GetComponent<Collider>(); collider.enabled = false; Destroy(collider);
            var renderer = volume.GetComponent<MeshRenderer>(); renderer.sharedMaterial = volumeMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            for (int i = 0; i < RibbonCount; i++)
            {
                var sheet = new GameObject("PurpleShearedRibbon_" + i, typeof(MeshFilter), typeof(MeshRenderer));
                sheet.transform.SetParent(transform, false);
                var mesh = new Mesh { name = "PurpleRibbon_Runtime_" + i }; mesh.MarkDynamic();
                ribbons[i] = mesh; vertices[i] = new Vector3[(RibbonSteps + 1) * 2]; colours[i] = new Color[vertices[i].Length];
                var uv = new Vector2[vertices[i].Length]; var triangles = new int[RibbonSteps * 6];
                for (int j = 0; j <= RibbonSteps; j++)
                {
                    uv[j*2] = new Vector2(j/(float)RibbonSteps,0); uv[j*2+1] = new Vector2(j/(float)RibbonSteps,1);
                    if (j == RibbonSteps) continue;
                    int k=j*6, v=j*2;
                    triangles[k]=v; triangles[k+1]=v+1; triangles[k+2]=v+2;
                    triangles[k+3]=v+2; triangles[k+4]=v+1; triangles[k+5]=v+3;
                }
                mesh.vertices=vertices[i]; mesh.uv=uv; mesh.triangles=triangles;
                sheet.GetComponent<MeshFilter>().sharedMesh=mesh;
                var mr=sheet.GetComponent<MeshRenderer>(); mr.sharedMaterial=filamentMaterial;
                mr.shadowCastingMode=ShadowCastingMode.Off; mr.receiveShadows=false;
            }
            for (int i=0;i<bolts.Length;i++)
            {
                var go=new GameObject(i<BoltCount ? "PurpleDepthDischarge_"+i : "PurpleMicroStreak_"+i);
                go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>(); bolts[i]=line;
                line.sharedMaterial=filamentMaterial; line.useWorldSpace=false; line.loop=false;
                line.positionCount=i<BoltCount?13:2; line.numCornerVertices=1; line.numCapVertices=1;
                line.shadowCastingMode=ShadowCastingMode.Off; line.receiveShadows=false;
            }
        }

        private static float Hash(float x) => Mathf.Repeat(Mathf.Sin(x*127.1f)*43758.5453f,1f);

        public void Render(float clock, float density, float emission, float chaos, float releaseAge, bool travel, float activity = 1f)
        {
            if (integrated != null) { integrated.Render(clock, releaseAge, travel); return; }
            if (volumeMaterial == null) return;
            volumeMaterial.SetFloat("_PhaseTime",clock*tuning.purpleFlowSpeed);
            volumeMaterial.SetFloat("_Density",density*tuning.purpleVolumeDensity);
            volumeMaterial.SetFloat("_Emission",emission*tuning.purpleEmission);
            volumeMaterial.SetFloat("_Chaos",chaos*tuning.purpleTurbulence);
            volumeMaterial.SetFloat("_Haze",tuning.purpleHazeOpacity);
            float flow=clock*tuning.purpleFlowSpeed;
            for(int i=0;i<RibbonCount;i++)
            {
                // Each sheet lies in a different tilted plane; half pass behind the volume.
                Quaternion plane=Quaternion.Euler(23+i*41, i*71, i*37);
                float angle=flow*(i%2==0?1.65f:-1.1f)+i*2.39996f;
                float radius=1.59f+(i%3)*.032f;
                for(int j=0;j<=RibbonSteps;j++)
                {
                    float u=j/(float)RibbonSteps, a=angle+u*(1.8f+i*.23f);
                    float rough=Mathf.Sin(a*5+i+flow*2)*.035f+Mathf.Sin(a*11-flow)*.015f;
                    Vector3 radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                    Vector3 p=radial*(radius+rough*chaos)+Vector3.forward*(Mathf.Sin(a*3+i)*.10f);
                    float taper=Mathf.Pow(Mathf.Max(0f,Mathf.Sin(u*Mathf.PI)),.7f);
                    float width=(i%3==0?.10f:.036f)*taper*tuning.purpleRibbonWidth;
                    vertices[i][j*2]=plane*(p-radial*width);
                    vertices[i][j*2+1]=plane*(p+radial*width);
                    Color c=i%3==0?new Color(1.25f,.035f,.55f):new Color(.22f,.035f,.8f);
                    c.a=taper*(.36f+.30f*Mathf.Sin(u*13+flow+i))*Mathf.Min(emission,1.4f);
                    colours[i][j*2]=colours[i][j*2+1]=c;
                }
                ribbons[i].vertices=vertices[i]; ribbons[i].colors=colours[i]; ribbons[i].RecalculateBounds();
            }
            for(int i=0;i<bolts.Length;i++)
            {
                LineRenderer line=bolts[i];
                float cadence=(8+(i%7)*2.3f)*Mathf.Lerp(.5f,1.35f,activity);
                int beat=Mathf.FloorToInt(flow*cadence+i*.731f);
                bool micro=i>=BoltCount;
                bool branch=i>=16 && i<BoltCount;
                float life=Mathf.Repeat(flow*cadence+i*.731f,1f);
                bool visible=micro ? life<Mathf.Lerp(.2f,.75f,activity) : Hash(beat*7+i*13)>(travel?.40f:Mathf.Lerp(.91f,.50f,activity));
                if(branch) visible &= bolts[(i-16)*2].enabled;
                line.enabled=visible;
                if(!visible) continue;
                float seed=i*19.73f+beat*1.37f;
                float a=i*2.39996f+Hash(seed)*.8f;
                Vector3 radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);
                float depth=(Hash(i*17)-.5f)*4.6f;
                Color c=i%4==0?new Color(3.6f,2.2f,4.2f):i%3==0?new Color(.46f,.46f,3.5f):new Color(2.8f,.12f,1.8f);
                c.a=(1-life*.7f)*Mathf.Min(emission,1.3f)*tuning.purpleLightningIntensity;
                line.startColor=c; c.a*=.1f; line.endColor=c;
                if(micro)
                {
                    Vector3 p=radial*(2+Hash(i*5)*1.6f)+Vector3.forward*depth;
                    line.startWidth=.014f+Hash(i)*.027f; line.endWidth=.003f;
                    line.SetPosition(0,p); line.SetPosition(1,p-radial*(.09f+life*.27f)-Vector3.forward*(travel?.3f:0));
                    continue;
                }
                float length=branch?.65f+Hash(seed)*.9f:1.0f+Hash(seed)*1.8f;
                float width=(i%5==0?.16f:.043f)*(.6f+Hash(i*11));
                if(branch) width=.026f;
                line.startWidth=width*tuning.purpleLightningWidth; line.endWidth=width*.10f;
                Vector3 tangent=new Vector3(-radial.y,radial.x,0);
                for(int j=0;j<13;j++)
                {
                    float u=j/12f;
                    float jag=(Hash(seed+j*31)-.5f)*(.18f+u*.46f);
                    boltPoints[j]=radial*(1.3f+u*length)+(tangent*(Mathf.Sin(u*4+i)*.35f+jag))
                        +Vector3.forward*(depth*(.25f+u*.75f)-(travel?u*u*2.2f:0));
                    if(branch) boltPoints[j]=bolts[(i-16)*2].GetPosition(5)
                        +(radial*.6f+tangent*1.2f+Vector3.forward*depth*.3f)*u+tangent*jag*u;
                }
                line.SetPositions(boltPoints);
            }
        }

        private void OnDestroy()
        {
            if(volumeMaterial!=null) Destroy(volumeMaterial);
            if(filamentMaterial!=null) Destroy(filamentMaterial);
            foreach(var mesh in ribbons) if(mesh!=null) Destroy(mesh);
        }
    }
}
