using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // Root-owned, opt-in companion. Pressure trajectories also locate the discharge roots.
    public sealed class PurplePressureStormRuntime:MonoBehaviour
    {
        private const int Steps=48,Across=5,Bolts=16,Nodes=25;
        private PurplePressureStormProfile profile;
        private float radius,clock; private bool travelling; private Vector3 forward;
        private Mesh mesh; private Material pressureMaterial,arcMaterial;
        private Vector3[] vertices;private Color[] colours;private Vector2[] uv;
        private LineRenderer[] bolts;private Vector3[][] points;
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1);
        public void Configure(PurplePressureStormProfile settings,float bodyRadius)
        {
            profile=settings;radius=bodyRadius;
            var wind=new GameObject("PressureStorm_SweptPressure");wind.transform.SetParent(transform,false);
            mesh=new Mesh{name="PurplePressureStorm_Pressure_Runtime"};mesh.MarkDynamic();
            wind.AddComponent<MeshFilter>().sharedMesh=mesh;
            pressureMaterial=new Material(Resources.Load<Shader>("VFX/PurplePressureStormFlow")){name="PurplePressureStorm_Flow_Runtime"};
            var renderer=wind.AddComponent<MeshRenderer>();renderer.sharedMaterial=pressureMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            vertices=new Vector3[profile.flowLanes*(Steps+1)*Across];colours=new Color[vertices.Length];uv=new Vector2[vertices.Length];
            int[] indices=new int[profile.flowLanes*Steps*(Across-1)*6];int tri=0;
            for(int lane=0;lane<profile.flowLanes;lane++)for(int n=0;n<=Steps;n++)for(int v=0;v<Across;v++)
            {
                int at=(lane*(Steps+1)+n)*Across+v;uv[at]=new Vector2(n/(float)Steps,v/(float)(Across-1));
                if(n<Steps && v<Across-1)
                {indices[tri++]=at;indices[tri++]=at+Across;indices[tri++]=at+1;indices[tri++]=at+1;indices[tri++]=at+Across;indices[tri++]=at+Across+1;}
            }
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;
            arcMaterial=new Material(Resources.Load<Shader>("VFX/PurplePressureStormArc")){name="PurplePressureStorm_Arc_Runtime"};
            bolts=new LineRenderer[Bolts];points=new Vector3[Bolts][];
            for(int i=0;i<Bolts;i++)
            {
                var go=new GameObject("PressureBoundaryRupture_"+i);go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=arcMaterial;
                line.positionCount=Nodes;line.numCornerVertices=0;line.numCapVertices=0;
                line.alignment=LineAlignment.View;line.textureMode=LineTextureMode.Stretch;
                line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                line.widthCurve=new AnimationCurve(new Keyframe(0,.3f),new Keyframe(.18f,1),new Keyframe(.7f,.55f),new Keyframe(1,0));
                bolts[i]=line;points[i]=new Vector3[Nodes];
            }
        }
        private Vector3 FlowPoint(int lane,float u,out float fade)
        {
            float cycle=.74f+Hash(lane*17+3)*.42f,c=clock*profile.flowSpeed+lane*.173f;
            float age=Mathf.Repeat(c,cycle)/cycle,epoch=Mathf.Floor(c/cycle);
            float seed=lane*71+epoch*13;
            fade=Mathf.Pow(Mathf.Max(0,Mathf.Sin(age*Mathf.PI)),.62f);
            Vector3 tangent=Vector3.Cross(forward,Mathf.Abs(forward.y)<.8f?Vector3.up:Vector3.right).normalized;
            Vector3 up=Vector3.Cross(tangent,forward).normalized;
            float phi=lane*2.39996f+Hash(seed+5)*.75f+u*.48f+age*.42f;
            Vector3 radial=tangent*Mathf.Cos(phi)+up*Mathf.Sin(phi);
            float theta=.12f+age*2.3f+u*(.48f+Hash(seed+8)*.42f);
            float distance=(1.02f+age*1.24f+u*.38f)*(profile.pressureReach/3);
            distance+=Mathf.Sin(u*11+seed+age*7)*.045f;
            Vector3 p=(radial*Mathf.Sin(theta)+forward*Mathf.Cos(theta))*distance;
            if(travelling)p-=forward*(age*.55f+u*.2f);
            return p*radius;
        }
        public void Sample(float time,Vector3 velocity)
        {
            clock=time;travelling=velocity.sqrMagnitude>.01f;
            forward=travelling?transform.InverseTransformDirection(velocity.normalized):Vector3.forward;
            pressureMaterial.SetFloat("_PhaseTime",time);pressureMaterial.SetVector("_Pressure",new Vector4(profile.pressureEmission,profile.pressureOpacity,0,0));
            arcMaterial.SetFloat("_Emission",profile.lightningEmission);
            for(int lane=0;lane<profile.flowLanes;lane++)for(int n=0;n<=Steps;n++)
            {
                float u=n/(float)Steps;Vector3 p=FlowPoint(lane,u,out float fade);
                Vector3 tangent=(FlowPoint(lane,u+.008f,out _)-FlowPoint(lane,u-.008f,out _)).normalized;
                Vector3 across=Vector3.Cross(p.normalized,tangent).normalized;
                float width=radius*profile.flowWidth*(.55f+.45f*Mathf.Sin(u*Mathf.PI))*(.8f+Hash(lane+9)*.7f);
                for(int v=0;v<Across;v++)
                {
                    int at=(lane*(Steps+1)+n)*Across+v;float s=v/(float)(Across-1)-.5f;
                    vertices[at]=p+across*s*width+p.normalized*(Mathf.Sin(u*19+lane+s*4+time*6)*.023f*radius);
                    colours[at]=new Color(Hash(lane*13),0,0,fade);
                }
            }
            mesh.vertices=vertices;mesh.colors=colours;mesh.RecalculateBounds();
            for(int i=0;i<Bolts;i++)
            {
                int lane=(i/2)%profile.flowLanes;bool branch=(i%2)==1;
                float c=time*(7.2f+Hash(lane+83)*4.7f)+lane*.437f,epoch=Mathf.Floor(c),age=Mathf.Repeat(c,1);
                float seed=epoch*73+lane*131,gate=.48f+Hash(seed+8)*.25f;
                float pulse=age<gate?Mathf.Clamp01(age/.08f)*Mathf.Pow(1-age/gate,.45f):0;
                bolts[i].enabled=pulse>.015f;if(!bolts[i].enabled)continue;
                float at=.15f+Hash(seed+4)*.6f;
                Vector3 root=FlowPoint(lane,at,out float flowPower);
                Vector3 tangent=(FlowPoint(lane,at+.025f,out _)-root).normalized;
                Vector3 outward=root.normalized,side=Vector3.Cross(outward,tangent).normalized;
                if(branch)root=points[i-1][11];
                float length=radius*profile.dischargeReach*(.5f+Hash(seed+3)*.75f)*(branch?.48f:1);
                int revision=Mathf.FloorToInt(age*4);
                Vector3 direction=(tangent*(branch?-.3f:.88f)+outward*.5f+side*(branch?.8f:.1f)).normalized;
                for(int n=0;n<Nodes;n++)
                {
                    float s=n/(float)(Nodes-1),taper=Mathf.Sin(s*Mathf.PI);
                    float coarse=(Hash(seed+Mathf.Floor(n/4f)*17+revision*11)-.5f)*.22f;
                    float fine=(Hash(seed+n*31+revision*19)-.5f)*.07f;
                    points[i][n]=root+direction*length*s+outward*radius*s*s*.18f
                        +side*radius*(coarse+fine)*taper+outward*radius*(Hash(seed+n*53)-.5f)*.06f*taper;
                }
                bolts[i].SetPositions(points[i]);bolts[i].widthMultiplier=radius*profile.lightningWidth*(branch?.42f:1)*(1+Hash(seed+12)*.55f);
                var color=new Color(1,1,1,pulse*(.45f+.55f*flowPower));bolts[i].startColor=color;bolts[i].endColor=color;
            }
        }
        private void OnDestroy(){if(mesh!=null)Destroy(mesh);if(pressureMaterial!=null)Destroy(pressureMaterial);if(arcMaterial!=null)Destroy(arcMaterial);}
    }
}
