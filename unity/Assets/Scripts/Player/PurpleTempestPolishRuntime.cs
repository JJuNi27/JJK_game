using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // Independent companion: consumes canonical release age; never changes body or clocks.
    public sealed class PurpleTempestPolishRuntime : MonoBehaviour
    {
        const int ChargeGusts=8,TravelGusts=4,Steps=64,Across=7,Bright=12,Dark=8,Nodes=65;
        PurpleTempestPolishProfile profile;
        float radius;
        Mesh windMesh,darkMesh;
        Material windMaterial,brightMaterial,darkMaterial;
        Vector3[] windVertices,darkVertices;
        Color[] windColors,darkColors;
        int[] darkIndices;
        LineRenderer[] arcs;
        Vector3[][] positions;
        readonly Vector4[] events=new Vector4[3];
        Matrix4x4 chargeFrame;
        bool released,hasChargeFrame;
        public string PresentationPhase {get;private set;}="Charge";
        public float ChargeWindWeight {get;private set;}
        public float TravelWindWeight {get;private set;}
        public Vector3 ChargeAnchor=>chargeFrame.MultiplyPoint3x4(Vector3.zero);
        public int VisibleDarkSegments {get;private set;}
        static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1);
        static float Noise(float x,float seed)
        {
            float a=Mathf.Floor(x),f=Mathf.Repeat(x,1);f=f*f*(3-2*f);
            return Mathf.Lerp(Hash(a*17+seed),Hash((a+1)*17+seed),f)*2-1;
        }
        Mesh CreateMesh(string name,Material material,int lanes,int across,out Vector3[] vertices,out Color[] colors)
        {
            var mesh=new Mesh{name=name};mesh.MarkDynamic();
            var go=new GameObject(name);go.transform.SetParent(transform,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            vertices=new Vector3[lanes*Nodes*across];colors=new Color[vertices.Length];var uv=new Vector2[vertices.Length];
            for(int lane=0;lane<lanes;lane++)for(int n=0;n<Nodes;n++)for(int v=0;v<across;v++)
                uv[(lane*Nodes+n)*across+v]=new Vector2(n/(float)Steps,v/(float)(across-1));
            mesh.vertices=vertices;mesh.uv=uv;return mesh;
        }
        public void Configure(PurpleTempestPolishProfile settings,float bodyRadius)
        {
            profile=settings;radius=bodyRadius;
            windMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTempestPolishWind")){name="PurpleTempestPolish_Wind_Runtime"};
            brightMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTempestPolishDischarge")){name="PurpleTempestPolish_Bright_Runtime"};
            darkMaterial=new Material(brightMaterial){name="PurpleTempestPolish_Dark_Runtime"};darkMaterial.SetFloat("_Dark",1);darkMaterial.renderQueue=3038;
            windMesh=CreateMesh("PurpleTempestPolish_Wind_Runtime",windMaterial,ChargeGusts+TravelGusts,Across,out windVertices,out windColors);
            var tris=new int[(ChargeGusts+TravelGusts)*Steps*(Across-1)*6];int index=0;
            for(int lane=0;lane<ChargeGusts+TravelGusts;lane++)for(int n=0;n<Steps;n++)for(int v=0;v<Across-1;v++)
            {
                int at=(lane*Nodes+n)*Across+v;AddQuad(tris,ref index,at,Across);
            }
            windMesh.triangles=tris;
            // Two intersecting, non-camera-facing strips per current; no continuous black outline.
            darkMesh=CreateMesh("PurpleTempestPolish_Dark_Runtime",darkMaterial,Dark*2,Across,out darkVertices,out darkColors);
            darkIndices=new int[Dark*2*Steps*(Across-1)*6];
            arcs=new LineRenderer[Bright];positions=new Vector3[Bright][];
            for(int i=0;i<Bright;i++)
            {
                var go=new GameObject("TempestPolish_WhiteVioletDischarge");go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=brightMaterial;
                line.useWorldSpace=false;line.positionCount=Nodes;line.alignment=LineAlignment.View;line.textureMode=LineTextureMode.Stretch;
                line.numCornerVertices=0;line.numCapVertices=0;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                line.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(.1f,.8f),new Keyframe(.4f,1),new Keyframe(.85f,.65f),new Keyframe(1,0));
                arcs[i]=line;positions[i]=new Vector3[Nodes];
            }
        }
        static void AddQuad(int[] indices,ref int count,int at,int stride)
        {
            indices[count++]=at;indices[count++]=at+stride;indices[count++]=at+1;
            indices[count++]=at+1;indices[count++]=at+stride;indices[count++]=at+stride+1;
        }
        float Coupling(Vector3 p)
        {
            float value=0;var normalized=p/radius;
            for(int i=0;i<events.Length;i++)value+=events[i].w*Mathf.Exp(-(normalized-(Vector3)events[i]).sqrMagnitude*1.6f);
            return Mathf.Clamp01(value)*profile.spatialCoupling;
        }
        void UpdateEvents(float t)
        {
            for(int i=0;i<events.Length;i++)
            {
                float cycle=t*(1.8f+i*.37f)+i*.413f,age=Mathf.Repeat(cycle,1),epoch=Mathf.Floor(cycle);
                float a=t*2.9f+i*2.39996f+Noise(t*.7f,i)*.8f;
                events[i]=new Vector4(Mathf.Cos(a)*1.2f,-.25f+Noise(t*1.1f,i+31)*.8f,Mathf.Sin(a)*1.2f,
                    Mathf.Pow(Mathf.Sin(age*Mathf.PI),.55f)*(.6f+Hash(epoch+i*37)*.4f));
            }
            foreach(var material in new[]{windMaterial,brightMaterial,darkMaterial})
            {
                material.SetFloat("_Coupling",profile.spatialCoupling);
                for(int i=0;i<events.Length;i++)material.SetVector("_Event"+i,events[i]);
                material.SetFloat("_Radius",radius);
            }
        }
        public void Sample(float time,Vector3 velocity,float ageSinceRelease)
        {
            float t=time*profile.stormSpeed;bool travel=ageSinceRelease>=0 && velocity.sqrMagnitude>.01f;
            if(!travel)
            {
                released=false;hasChargeFrame=true;chargeFrame=transform.localToWorldMatrix;
            }
            else if(!released)
            {
                // Reconstruct release location even if the first sample already advanced into travel.
                Vector3 anchor=transform.position-velocity*ageSinceRelease;
                if(!hasChargeFrame)chargeFrame=transform.localToWorldMatrix;
                chargeFrame.SetColumn(3,new Vector4(anchor.x,anchor.y,anchor.z,1));released=true;
            }
            ChargeWindWeight=travel?1-Mathf.SmoothStep(0,1,ageSinceRelease/profile.releaseWindSeconds):1;
            TravelWindWeight=travel?Mathf.SmoothStep(0,1,ageSinceRelease/.12f)*profile.travelWindStrength:0;
            PresentationPhase=!travel?"Charge":ageSinceRelease<profile.releaseWindSeconds?"Release":"Travel";
            Vector3 forward=travel?transform.InverseTransformDirection(velocity.normalized):Vector3.forward;
            Quaternion heading=Quaternion.FromToRotation(Vector3.forward,forward);
            float scale=transform.lossyScale.x;Vector4 sphere=new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*scale*.87f);
            foreach(var material in new[]{windMaterial,brightMaterial,darkMaterial})
            {material.SetVector("_Sphere",sphere);material.SetFloat("_PhaseTime",t);}
            UpdateEvents(t);
            windMaterial.SetFloat("_Opacity",profile.windOpacity);
            brightMaterial.SetFloat("_Emission",profile.brightEmission);darkMaterial.SetFloat("_Emission",profile.darkEmission);
            for(int lane=0;lane<ChargeGusts+TravelGusts;lane++)
            {
                bool flight=lane>=ChargeGusts;int k=flight?lane-ChargeGusts:lane;
                float c=time*profile.windSpeed*(1.1f+Hash(k+4)*.65f)+k*.319f,age=Mathf.Repeat(c,1),epoch=Mathf.Floor(c),seed=k*51+epoch*7;
                float strength=Mathf.Pow(Mathf.Sin(age*Mathf.PI),.55f)*(flight?TravelWindWeight:ChargeWindWeight);
                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)Steps;
                    float theta=time*profile.windSpeed*(6.2f+Hash(k+9)*2.3f)+k*2.39996f+u*(1.6f+Hash(seed)*1.3f);
                    float r=(1+age*(profile.windReach-1)+u*.25f+Noise(u*5-time*3,seed)*.16f)*radius;
                    float y=radius*(-.9f+(k%3)*.18f+Mathf.Sin(theta*1.7f+k)*.22f+u*u*.17f);
                    Vector3 center=flight?heading*new Vector3((k%2==0?-1:1)*radius*(.7f+Mathf.Sin(u*Mathf.PI)*.9f),
                        radius*(-.2f+Noise(u*4-time*4,k)*.2f),radius*(.8f-u*2.8f-age*.7f)):
                        new Vector3(Mathf.Cos(theta)*r,y,Mathf.Sin(theta)*r);
                    float energy=Coupling(center);
                    for(int v=0;v<Across;v++)
                    {
                        float s=v/(float)(Across-1)-.5f;
                        float width=radius*profile.windWidth*(.45f+.55f*Mathf.Sin(u*Mathf.PI))*(.8f+Noise(u*5-time*2,seed)*.35f);
                        Vector3 p=flight?center+heading*new Vector3(s*width*.55f,s*width*.15f,0):
                            center+new Vector3(Mathf.Cos(theta)*s*width,s*width*(.22f+energy*.35f),Mathf.Sin(theta)*s*width);
                        if(!flight && travel)p=transform.InverseTransformPoint(chargeFrame.MultiplyPoint3x4(p));
                        int at=(lane*Nodes+n)*Across+v;windVertices[at]=p;
                        windColors[at]=new Color(Hash(k*7),flight?1:0,energy,strength*(.8f+energy*.35f));
                    }
                }
            }
            windMesh.vertices=windVertices;windMesh.colors=windColors;windMesh.RecalculateBounds();
            int triangleCount=0;VisibleDarkSegments=0;
            for(int lane=0;lane<Dark;lane++)
            {
                float cycle=t*3.1f*(.8f+Hash(lane+26)*.5f)+lane*.387f,age=Mathf.Repeat(cycle,1),epoch=Mathf.Floor(cycle),seed=lane*127+epoch*43;
                float pulse=age<.91f?Mathf.Min(1,age/.08f)*Mathf.Pow(1-age/.91f,.35f):0;
                float angle=t*3.6f+lane*2.39996f+Hash(seed+2)*1.6f;
                float span=.72f+Hash(seed+6)*1.08f;
                Quaternion plane=Quaternion.Euler(25+Hash(seed+10)*130,Hash(lane+3)*360,Hash(lane+5)*180);
                float reach=1.08f+Hash(seed+7)*.52f;
                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)Steps,a=angle+u*span;
                    float r=radius*(reach+Noise(u*13+t*11,seed+41)*.10f+Mathf.Sin(u*8+t*13+seed)*.09f);
                    Vector3 p=plane*new Vector3(Mathf.Cos(a)*r,Noise(u*6+t*7,seed)*radius*.18f,Mathf.Sin(a)*r);
                    float energy=Coupling(p),width=radius*profile.darkWidth*Mathf.Pow(Mathf.Max(0,Mathf.Sin(u*Mathf.PI)),.6f)*
                        (.45f+.65f*(Noise(u*7-t*5,seed+7)*.5f+.5f))*(.8f+energy*.6f);
                    for(int cross=0;cross<2;cross++)for(int v=0;v<Across;v++)
                    {
                        float s=v/(float)(Across-1)-.5f;
                        Vector3 across=plane*(cross==0?new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)):Vector3.up);
                        int at=((lane*2+cross)*Nodes+n)*Across+v;
                        darkVertices[at]=p+across*s*width+plane*Vector3.up*Noise(u*10+t*9,seed+v)*radius*.018f;
                        float cutFade=Mathf.Clamp01((Noise(u*12+Mathf.Floor(t*14)*.73f,seed+91)+.35f)*3);
                        darkColors[at]=new Color(Hash(seed),energy,cutFade,pulse);
                    }
                    if(n<Steps && pulse>.02f)
                    {
                        // Drop actual faces, including both crossings; neighbouring nodes cannot bridge the gap.
                        float mid=(n+.5f)/Steps;
                        bool alive=Noise(mid*12+Mathf.Floor(t*14)*.73f,seed+91)>-.35f;
                        if(alive)
                        {
                            VisibleDarkSegments++;
                            for(int cross=0;cross<2;cross++)for(int v=0;v<Across-1;v++)
                                AddQuad(darkIndices,ref triangleCount,((lane*2+cross)*Nodes+n)*Across+v,Across);
                        }
                    }
                }
            }
            darkMesh.vertices=darkVertices;darkMesh.colors=darkColors;darkMesh.SetTriangles(darkIndices,0,triangleCount,0,false);darkMesh.RecalculateBounds();
            for(int i=0;i<Bright;i++)
            {
                // Retain previous neon identity, count, emission, widths and event cadence.
                float c=t*5.2f*(.8f+Hash(i+14)*.5f)+i*.387f,age=Mathf.Repeat(c,1),epoch=Mathf.Floor(c),seed=i*127+epoch*43;
                float pulse=age<.87f?Mathf.Min(1,age/.08f)*Mathf.Pow(1-age/.87f,.25f):0;
                arcs[i].enabled=pulse>.02f;if(!arcs[i].enabled)continue;
                float angle=t*5.3f+i*2.39996f+Hash(seed+2)*1.6f,span=1.25f+Hash(seed+6)*1.65f;
                Quaternion plane=Quaternion.Euler(25+Hash(seed+10)*130,Hash(i+3)*360,Hash(i+5)*180);
                float reach=1+Hash(seed+7)*(profile.dischargeReach-1),evolve=t*20;
                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)Steps,a=angle+u*span;
                    float r=radius*(reach+Noise(u*13+evolve,seed+41)*.055f+Noise(u*31-evolve,seed+13)*.03f+Mathf.Sin(u*8+t*13+seed)*.09f);
                    Vector3 p=plane*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(u*6+evolve+seed)*radius*.11f,Mathf.Sin(a)*r);
                    float energy=Coupling(p);
                    positions[i][n]=p*(1-energy*.12f)-forward*(travel?radius*.15f:0);
                }
                arcs[i].SetPositions(positions[i]);arcs[i].widthMultiplier=radius*scale*profile.brightWidth*(.7f+Hash(seed+30)*.7f);
                var color=new Color(Hash(seed),0,0,pulse);arcs[i].startColor=color;arcs[i].endColor=color;
            }
        }
        private void OnDestroy()
        {
            if(windMesh!=null)Destroy(windMesh);if(darkMesh!=null)Destroy(darkMesh);
            if(windMaterial!=null)Destroy(windMaterial);if(brightMaterial!=null)Destroy(brightMaterial);if(darkMaterial!=null)Destroy(darkMaterial);
        }
    }
}
