using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // Independent companion: consumes canonical release age; never changes body or clocks.
    public sealed class PurpleTempestFinalCleanupRuntime : MonoBehaviour
    {
        const int ChargeGusts=8,TravelGusts=4,Steps=64,Across=7,Bright=12,Dark=8,Nodes=65;
        PurpleTempestFinalCleanupProfile profile;
        float radius;
        public float ReleaseWindWeight {get;private set;}
        Mesh windMesh,darkMesh;
        Material windMaterial,brightMaterial,darkMaterial;
        Vector3[] windVertices,darkVertices;
        Color[] windColors,darkColors;
        int[] darkIndices;
        LineRenderer[] arcs;
        Vector3[][] positions;
        // Existing surfaces provide bounded soft occlusion proxies. No extra render pass,
        // depth texture, camera hook or global depth-write change is introduced.
        readonly Vector4[] coverCenters=new Vector4[40],coverU=new Vector4[40],coverV=new Vector4[40],hotSites=new Vector4[12];
        Material[] layerMaterials;
        public int CoverPatchCount {get;private set;}
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
        public void Configure(PurpleTempestFinalCleanupProfile settings,float bodyRadius)
        {
            profile=settings;radius=bodyRadius;
            windMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTempestFinalCleanupWind")){name="PurpleTempestFinalCleanup_Wind_Runtime"};
            brightMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTempestFinalCleanupDischarge")){name="PurpleTempestFinalCleanup_Bright_Runtime"};
            darkMaterial=new Material(brightMaterial){name="PurpleTempestFinalCleanup_Dark_Runtime"};darkMaterial.SetFloat("_Dark",1);darkMaterial.renderQueue=3038;
            layerMaterials=new[]{windMaterial,brightMaterial,darkMaterial};
            windMesh=CreateMesh("PurpleTempestFinalCleanup_Wind_Runtime",windMaterial,ChargeGusts+TravelGusts,Across,out windVertices,out windColors);
            var tris=new int[(ChargeGusts+TravelGusts)*Steps*(Across-1)*6];int index=0;
            for(int lane=0;lane<ChargeGusts+TravelGusts;lane++)for(int n=0;n<Steps;n++)for(int v=0;v<Across-1;v++)
            {
                int at=(lane*Nodes+n)*Across+v;AddQuad(tris,ref index,at,Across);
            }
            windMesh.triangles=tris;
            // Two intersecting, non-camera-facing strips per current; no continuous black outline.
            darkMesh=CreateMesh("PurpleTempestFinalCleanup_Dark_Runtime",darkMaterial,Dark*2,Across,out darkVertices,out darkColors);
            darkIndices=new int[Dark*2*Steps*(Across-1)*6];
            arcs=new LineRenderer[Bright];positions=new Vector3[Bright][];
            for(int i=0;i<Bright;i++)
            {
                var go=new GameObject("TempestFinalCleanup_WhiteVioletDischarge");go.transform.SetParent(transform,false);
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
            foreach(var material in layerMaterials)
            {
                material.SetFloat("_Coupling",profile.spatialCoupling);
                for(int i=0;i<events.Length;i++)material.SetVector("_Event"+i,events[i]);
                material.SetFloat("_Radius",radius);
                material.SetFloat("_LayerOcclusion",profile.layerOcclusion);
                material.SetFloat("_CoverageBreakup",profile.windCoverageBreakup);
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
            ReleaseWindWeight=travel?Mathf.Sin(Mathf.Clamp01(ageSinceRelease/profile.releaseWindSeconds)*Mathf.PI):0;
            windMaterial.SetFloat("_Release",ReleaseWindWeight);
            TravelWindWeight=travel?Mathf.SmoothStep(0,1,ageSinceRelease/.12f)*profile.travelWindStrength:0;
            PresentationPhase=!travel?"Charge":ageSinceRelease<profile.releaseWindSeconds?"Release":"Travel";
            Vector3 forward=travel?transform.InverseTransformDirection(velocity.normalized):Vector3.forward;
            Quaternion heading=Quaternion.FromToRotation(Vector3.forward,forward);
            float scale=transform.lossyScale.x;Vector4 sphere=new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*scale*.87f);
            foreach(var material in layerMaterials)
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
                    // A broad air sheet spirals inward across the lower space. Release
                    // compresses that same anchored sheet and redirects it forward.
                    float radial=profile.windReach-(age*.9f+u*.4f)*(profile.windReach-1);
                    float r=(radial+Noise(u*4-time*5,seed)*.23f)*radius;
                    float y=radius*(-1.0f+(k%3)*.24f+Mathf.Sin(theta+k)*.18f+u*u*.3f);
                    Vector3 charge=new Vector3(Mathf.Cos(theta)*r,y,Mathf.Sin(theta)*r);
                    if(travel)charge=Vector3.Lerp(charge,heading*new Vector3(charge.x*.48f,charge.y*.68f,
                        radius*(.8f+ageSinceRelease/profile.releaseWindSeconds*3.5f-u*1.6f)),ReleaseWindWeight*.9f);
                    // One existing lane supplies front compression, two supply lateral
                    // shear and the fourth supplies short rear pressure (not a cyclone).
                    Vector3 center;
                    if(!flight)center=charge;
                    else if(k==0)center=heading*new Vector3(Mathf.Cos(u*5.8f)*radius*(.4f+age*.7f),
                        Mathf.Sin(u*5.8f)*radius*(.4f+age*.7f),radius*(.95f+age*.5f-u*.25f));
                    else if(k<3)center=heading*new Vector3((k==1?-1:1)*radius*(.85f+u*1.1f+age*.4f),
                        radius*(-.35f+Noise(u*4-time*6,k)*.3f),radius*(.7f-u*3.3f-age*.8f));
                    else center=heading*new Vector3(Noise(u*5-time*5,k)*radius*.45f,
                        Noise(u*4+time*4,k+13)*radius*.35f,-radius*(.7f+u*3+age));
                    float energy=Coupling(center);
                    for(int v=0;v<Across;v++)
                    {
                        float s=v/(float)(Across-1)-.5f;
                        float width=radius*profile.windWidth*(.65f+.45f*Mathf.Sin(u*Mathf.PI))*(1+Noise(u*5-time*5,seed)*.4f);
                        Vector3 p=flight?center+heading*new Vector3(s*width*.7f,s*width*.35f,Noise(u*9-time*4,seed+v)*radius*.08f):
                            center+new Vector3(Mathf.Cos(theta)*s*width,s*width*(.35f+energy*.4f),Mathf.Sin(theta)*s*width);
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
                float span=.36f+Hash(seed+6)*.6f;
                Quaternion plane=Quaternion.Euler(25+Hash(seed+10)*130,Hash(lane+3)*360,Hash(lane+5)*180);
                float reach=.96f+Hash(seed+7)*.66f;
                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)Steps,a=angle+u*span;
                    float r=radius*(reach+Noise(u*6+t*6,seed+41)*.16f+Mathf.Sin(u*8+t*13+seed)*.10f);
                    Vector3 p=plane*new Vector3(Mathf.Cos(a)*r,Noise(u*6+t*7,seed)*radius*.18f,Mathf.Sin(a)*r);
                    // The torn current and the neon use the same moving pressure site,
                    // with different depth offsets rather than unrelated orbital centres.
                    Vector3 site=(Vector3)events[lane%events.Length]*radius;
                    p=Vector3.Lerp(p,site+plane*new Vector3((u-.5f)*radius*span,
                        Noise(u*5+t*6,seed)*radius*.25f,0),.45f);
                    float energy=Coupling(p),width=radius*profile.darkWidth*Mathf.Pow(Mathf.Max(0,Mathf.Sin(u*Mathf.PI)),.6f)*
                        (.65f+.8f*(Noise(u*5-t*6,seed+7)*.5f+.5f))*(1+energy*.65f);
                    for(int cross=0;cross<2;cross++)for(int v=0;v<Across;v++)
                    {
                        float s=v/(float)(Across-1)-.5f;
                        Vector3 across=plane*(cross==0?new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)):Vector3.up);
                        int at=((lane*2+cross)*Nodes+n)*Across+v;
                        Vector3 depthAxis=plane*(cross==0?Vector3.up:new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));
                        // Rounded local section, never a macro body displacement or flapping sheet.
                        float bulge=(1-4*s*s)*(.6f+.4f*Noise(u*5+t*3,seed+cross*17));
                        float fold=Noise(u*6+s*2+t*4,seed+cross*29)*.3f;
                        darkVertices[at]=p+across*s*width+depthAxis*radius*profile.darkDepth*(bulge+fold);
                        float cutFade=Mathf.Clamp01((Noise(u*9+s*2.7f+t*3.7f,seed+91+cross*13)+.5f)*3);
                        darkColors[at]=new Color(Hash(seed),energy,cutFade,pulse);
                    }
                    if(n<Steps && pulse>.02f)
                    {
                        // Erosion moves across width/depth instead of sawing off both
                        // full cross-sections at the same longitudinal station.
                        float mid=(n+.5f)/Steps;
                        bool any=false;
                        for(int cross=0;cross<2;cross++)for(int v=0;v<Across-1;v++)
                        {
                            float s=(v+.5f)/(Across-1)-.5f;
                            float erosion=Noise(mid*9+s*2.7f+t*3.7f,seed+91+cross*13);
                            if(erosion>-.5f)
                            {AddQuad(darkIndices,ref triangleCount,((lane*2+cross)*Nodes+n)*Across+v,Across);any=true;}
                        }
                        if(any)VisibleDarkSegments++;
                    }
                }
            }
            darkMesh.vertices=darkVertices;darkMesh.colors=darkColors;darkMesh.SetTriangles(darkIndices,0,triangleCount,0,false);darkMesh.RecalculateBounds();
            for(int i=0;i<Bright;i++)
            {
                // Same neon count/palette, with independent lifetimes and flow-boundary paths.
                float c=t*(3.5f+Hash(i+14)*4.8f)+i*.387f,age=Mathf.Repeat(c,1),epoch=Mathf.Floor(c),seed=i*127+epoch*43;
                float pulse=age<.87f?Mathf.Min(1,age/.08f)*Mathf.Pow(1-age/.87f,.25f):0;
                arcs[i].enabled=pulse>.02f;hotSites[i]=Vector4.zero;if(!arcs[i].enabled)continue;
                float angle=t*5.3f+i*2.39996f+Hash(seed+2)*1.6f,span=.65f+Hash(seed+6)*2.3f;
                Quaternion plane=Quaternion.Euler(25+Hash(seed+10)*130,Hash(i+3)*360,Hash(i+5)*180);
                float reach=1+Hash(seed+7)*(profile.dischargeReach-1),evolve=t*20;
                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)Steps,a=angle+u*span;
                    float r=radius*(reach+Noise(u*13+evolve,seed+41)*.055f+Noise(u*31-evolve,seed+13)*.03f+Mathf.Sin(u*8+t*13+seed)*.09f);
                    Vector3 p=plane*new Vector3(Mathf.Cos(a)*r,Mathf.Sin(u*6+evolve+seed)*radius*.11f,Mathf.Sin(a)*r);
                    float energy=Coupling(p);
                    // Curved transverse events link a current's kink to the wind boundary;
                    // other lanes keep the familiar close white-violet swirl.
                    if(i%3!=0)
                    {
                        Vector3 site=(Vector3)events[(i/3+i)%events.Length]*radius;
                        Vector3 offset=plane*new Vector3(Noise(epoch*.37f,i+5),Noise(epoch*.41f,i+21),Noise(epoch*.29f,i+71))*radius*profile.arcSpread;
                        Vector3 start=site*.78f+offset;
                        Vector3 end=site*(1.12f+Hash(seed+17)*.8f)+plane*Vector3.forward*radius*profile.arcSpread*(Hash(seed+53)*2-1);
                        Vector3 tangent=Vector3.Cross(site.normalized,plane*Vector3.up).normalized;
                        p=Vector3.Lerp(start,end,u)
                            +tangent*radius*(u-.5f)*span
                            +plane*Vector3.up*Mathf.Sin(u*Mathf.PI)*radius*(.25f+Hash(seed+29)*.65f)
                            +plane*Vector3.forward*Noise(u*24+evolve,seed)*radius*.10f;
                        energy=Coupling(p);
                    }
                    positions[i][n]=p*(1-energy*.1f)-forward*(travel?radius*.15f:0);
                }
                Vector3 hot=transform.TransformPoint(positions[i][16+(i*7)%32]);
                hotSites[i]=new Vector4(hot.x,hot.y,hot.z,pulse);
                arcs[i].SetPositions(positions[i]);arcs[i].widthMultiplier=radius*scale*profile.brightWidth*(.7f+Hash(seed+30)*.7f);
                var color=new Color(Hash(seed),0,0,pulse);arcs[i].startColor=color;arcs[i].endColor=color;
            }
            UploadCoverPatches();
        }
        void SurfacePatch(int index,Vector3[] vertices,Color[] colors,int lane,int node,int halfSpan)
        {
            int at=(lane*Nodes+node)*Across;
            Vector3 p=vertices[at+Across/2];
            Vector3 u=(vertices[(lane*Nodes+node+halfSpan)*Across+Across/2]-vertices[(lane*Nodes+node-halfSpan)*Across+Across/2])*.5f;
            Vector3 v=(vertices[at+Across-1]-vertices[at])*.5f;
            p=transform.TransformPoint(p);u=transform.TransformVector(u);v=transform.TransformVector(v);
            // Orthogonal patch coordinates; soft finite footprints, not whole-path hiding.
            v-=Vector3.Project(v,u);
            coverCenters[index]=new Vector4(p.x,p.y,p.z,Mathf.Clamp01(colors[at+Across/2].a*colors[at+Across/2].b));
            if(index<24)coverCenters[index].w=Mathf.Clamp01(colors[at+Across/2].a);
            u=u/Mathf.Max(u.sqrMagnitude,.00001f);v=v/Mathf.Max(v.sqrMagnitude,.00001f);
            coverU[index]=new Vector4(u.x,u.y,u.z,0);coverV[index]=new Vector4(v.x,v.y,v.z,0);
        }
        void UploadCoverPatches()
        {
            for(int lane=0;lane<ChargeGusts+TravelGusts;lane++)
                for(int k=0;k<2;k++)SurfacePatch(lane*2+k,windVertices,windColors,lane,k==0?18:46,12);
            for(int lane=0;lane<Dark*2;lane++)SurfacePatch(24+lane,darkVertices,darkColors,lane,32,22);
            CoverPatchCount=40;
            foreach(var material in layerMaterials)
            {
                material.SetVectorArray("_CoverCenters",coverCenters);material.SetVectorArray("_CoverU",coverU);material.SetVectorArray("_CoverV",coverV);
                material.SetVectorArray("_HotSites",hotSites);
            }
        }
        private void OnDestroy()
        {
            if(windMesh!=null)Destroy(windMesh);if(darkMesh!=null)Destroy(darkMesh);
            if(windMaterial!=null)Destroy(windMaterial);if(brightMaterial!=null)Destroy(brightMaterial);if(darkMaterial!=null)Destroy(darkMaterial);
        }
    }
}
