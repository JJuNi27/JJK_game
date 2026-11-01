using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    // Editor assembly only. No match bootstrap, combat subscription, or serialized scene link.
    public enum ShibuyaBlackFlashRndKind { Legacy, Volume, Flipbook, Hybrid }

    public sealed class ShibuyaBlackFlashRndPreview : IDisposable
    {
        public const float BodyLife=.30f, TotalLife=.75f;
        public readonly GameObject Root;
        public readonly Texture2D Atlas;
        public readonly ShibuyaBlackFlashRndKind Kind;
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Material> materials=new List<Material>();
        readonly List<MeshRenderer> renderers=new List<MeshRenderer>();
        readonly List<int> modes=new List<int>();
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        readonly Transform billboard;
        readonly ShibuyaBlackFlashVisual legacy;
        readonly Vector3 direction;
        readonly Color red;
        public int VertexCount {get {if(legacy!=null)return legacy.VertexCount;int n=0;foreach(var m in meshes)n+=m.vertexCount;return n;}}
        public static readonly string[] Labels={"기존 REV1 (크기 정규화)","A: 입체 먹물 파열","B: 단일 플립북","C: 휘어진 다층 혼합"};

        public ShibuyaBlackFlashRndPreview(ShibuyaBlackFlashRndKind kind,Vector3 point,Vector3 forward,int seed=71,float diameter=4)
        {
            Kind=kind;direction=forward.sqrMagnitude>.001f?forward.normalized:Vector3.forward;
            var profile=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");red=profile.lightningColor;
            Root=new GameObject("BlackFlash RND "+kind){hideFlags=HideFlags.DontSave};
            Root.transform.SetPositionAndRotation(point,Quaternion.LookRotation(direction));
            if(kind==ShibuyaBlackFlashRndKind.Legacy) {
                Material ink=Own(profile.inkShader),light=Own(profile.lightShader),smoke=Own(profile.inkShader);smoke.SetFloat("_ZWrite",0);
                legacy=Root.AddComponent<ShibuyaBlackFlashVisual>();legacy.Initialize(ink,light,smoke);legacy.Play(point,direction,profile,seed,Time.unscaledTimeAsDouble);
                float radius=.1f;
                foreach(var mf in legacy.GetComponentsInChildren<MeshFilter>())if(mf.name=="TornCore"||mf.name=="BranchingRupture")
                    foreach(var v in mf.sharedMesh.vertices)radius=Mathf.Max(radius,v.magnitude*(mf.name=="TornCore"?profile.coreExpansion:1));
                Root.transform.localScale=Vector3.one*(diameter/(2*radius));Root.SetActive(false);return;
            }
            Root.transform.localScale=Vector3.one*(diameter*.5f);
            Shader shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shibuya/Editor/ShibuyaBlackFlashRnd.shader");
            if(shader==null)throw new InvalidOperationException("RND shader missing");
            Material body=Own(shader),soft=Own(shader);soft.SetFloat("_ZWrite",0);
            if(kind==ShibuyaBlackFlashRndKind.Volume)Add(BuildTears(seed),body,0,"TornVolumes");
            else {
                Atlas=BuildAtlas(seed);body.SetTexture("_Atlas",Atlas);
                if(kind==ShibuyaBlackFlashRndKind.Flipbook)billboard=Add(BuildSheet(false),body,1,"SingleFlipbook").transform;
                else {
                    for(int i=0;i<3;i++) {
                        var r=Add(BuildSheet(true),body,1,"CurvedDepthSheet"+i);
                        r.transform.localRotation=Quaternion.Euler(i==1?18:-12,i*60,0);
                        r.transform.localPosition=new Vector3(0,0,(i-1)*.12f);
                    }
                    Add(BuildFragments(seed),body,4,"DepthFragments");
                }
                // Normalize visible atlas support, not just the transparent quad bounds.
                var pixels=Atlas.GetPixels32();float extent=.01f;
                for(int y=0;y<Atlas.height;y+=2)for(int x=0;x<Atlas.width;x+=2)if(pixels[y*Atlas.width+x].a>128) {
                    float u=((x%192)+.5f)/192,w=((y%192)+.5f)/192;
                    Vector3 p=new Vector3(u*2-1,w*2-1,kind==ShibuyaBlackFlashRndKind.Hybrid?.28f*Mathf.Sin(u*Mathf.PI*2)*Mathf.Sin(w*Mathf.PI):0);
                    foreach(var renderer in renderers)if(renderer.name!="DepthFragments")extent=Mathf.Max(extent,(renderer.transform.localRotation*p+renderer.transform.localPosition).magnitude);
                }
                for(int i=0;i<renderers.Count;i++)if(modes[i]==1){renderers[i].transform.localScale=Vector3.one/extent;renderers[i].transform.localPosition/=extent;}
            }
            Add(BuildCommon(seed),soft,2,"CommonSmoke");
            Add(BuildFlash(seed),soft,3,"CommonContact");
            Root.SetActive(false);
        }
        Material Own(Shader shader){var m=new Material(shader){name="BlackFlash RND owned",hideFlags=HideFlags.DontSave};materials.Add(m);return m;}
        MeshRenderer Add(Mesh mesh,Material material,int mode,string name)
        {
            meshes.Add(mesh);var go=new GameObject(name){hideFlags=HideFlags.DontSave};go.transform.SetParent(Root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;
            r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;
            renderers.Add(r);modes.Add(mode);return r;
        }
        public void Sample(float age,Camera camera)
        {
            Root.SetActive(age>=0&&age<TotalLife);if(age<0||age>=TotalLife)return;
            if(legacy!=null){legacy.Sample(age);return;}
            if(billboard!=null&&camera!=null) {
                billboard.rotation=camera.transform.rotation;
                Vector3 projected=camera.transform.InverseTransformDirection(direction);
                if(new Vector2(projected.x,projected.y).sqrMagnitude>.03f)
                    billboard.rotation*=Quaternion.Euler(0,0,Mathf.Atan2(projected.y,projected.x)*Mathf.Rad2Deg);
            }
            for(int i=0;i<renderers.Count;i++) {
                block.Clear();block.SetFloat("_Age",age);block.SetFloat("_Mode",modes[i]);block.SetColor("_Red",red);
                block.SetFloat("_Opacity",1);
                block.SetFloat("_Layer",Kind==ShibuyaBlackFlashRndKind.Hybrid&&modes[i]==1?i:-1);
                renderers[i].SetPropertyBlock(block);
            }
        }
        public void Dispose()
        {
            if(Root!=null)Object.DestroyImmediate(Root);
            foreach(var m in meshes)if(m!=null)Object.DestroyImmediate(m);
            foreach(var m in materials)if(m!=null)Object.DestroyImmediate(m);
            if(Atlas!=null)Object.DestroyImmediate(Atlas);
        }
        static Mesh Finish(List<Vector3> v,List<int> t,List<Vector4> uv,List<Vector4> anchors=null)
        {
            var m=new Mesh{name="BlackFlash RND mesh",hideFlags=HideFlags.DontSave};m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);
            if(anchors!=null)m.SetUVs(1,anchors);m.RecalculateNormals();m.RecalculateBounds();var b=m.bounds;b.Expand(2);m.bounds=b;return m;
        }
        static float Next(System.Random r,float a,float b)=>Mathf.Lerp(a,b,(float)r.NextDouble());
        static Mesh BuildTears(int seed)
        {
            var r=new System.Random(seed);var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector4>();var anchors=new List<Vector4>();
            for(int blade=0;blade<7;blade++) {
                float a=blade*2.39996f+Next(r,-.3f,.3f),length=Next(r,.65f,1),width=Next(r,.18f,.35f);
                Vector3 axis=new Vector3(Mathf.Cos(a),Mathf.Sin(a),Next(r,-.25f,.8f)).normalized;
                Vector3 side=Vector3.Cross(axis,Vector3.forward).normalized,depth=Vector3.Cross(axis,side);int start=v.Count;
                Vector3 anchor=axis*.12f;
                for(int j=0;j<9;j++) {
                    float f=j/8f;Vector3 center=anchor+axis*length*f+depth*Mathf.Sin(f*4)*.18f;
                    float w=Mathf.Max(.006f,Mathf.Sin(f*Mathf.PI)*width*(j%2==0?1:.73f));
                    for(int k=0;k<6;k++) {
                        float angle=k*Mathf.PI/3;
                        v.Add(center+side*Mathf.Cos(angle)*w+depth*Mathf.Sin(angle)*Mathf.Max(.01f,w*.25f));
                        uv.Add(new Vector4(f,blade*.137f,0,0));anchors.Add(new Vector4(anchor.x,anchor.y,anchor.z,blade));
                    }
                    if(j>0)for(int k=0;k<6;k++){int p=start+(j-1)*6+k,q=start+(j-1)*6+(k+1)%6,c=start+j*6+k,d=start+j*6+(k+1)%6;t.AddRange(new[]{p,q,c,q,d,c});}
                }
                for(int k=1;k<5;k++){t.AddRange(new[]{start,start+k+1,start+k});int end=start+8*6;t.AddRange(new[]{end,end+k,end+k+1});}
            }
            float extent=0;foreach(var p in v)extent=Mathf.Max(extent,p.magnitude);for(int i=0;i<v.Count;i++)v[i]/=extent;
            return Finish(v,t,uv,anchors);
        }
        static Mesh BuildSheet(bool curved)
        {
            var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector4>();int n=curved?12:1;
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++) {
                float u=(float)x/n,w=(float)y/n;v.Add(new Vector3(u*2-1,w*2-1,curved?.28f*Mathf.Sin(u*Mathf.PI*2)*Mathf.Sin(w*Mathf.PI):0));uv.Add(new Vector4(u,w,0,0));
                if(x<n&&y<n){int a=y*(n+1)+x;t.AddRange(new[]{a,a+n+1,a+1,a+1,a+n+1,a+n+2});}
            }
            return Finish(v,t,uv);
        }
        static void Tetra(List<Vector3> v,List<int> t,List<Vector4> uv,List<Vector4> anchors,Vector3 p,float size,float id)
        {
            int s=v.Count;v.AddRange(new[]{p+new Vector3(-size,-size,0),p+new Vector3(size,-size,0),p+new Vector3(0,size*1.7f,0),p+new Vector3(0,0,size)});
            for(int i=0;i<4;i++){uv.Add(new Vector4(0,id,0,0));anchors.Add(new Vector4(p.x,p.y,p.z,id));}
            t.AddRange(new[]{s,s+1,s+2,s,s+3,s+1,s+1,s+3,s+2,s+2,s+3,s});
        }
        static Mesh BuildFragments(int seed)
        {
            var r=new System.Random(seed);var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector4>();var a=new List<Vector4>();
            for(int i=0;i<12;i++){float angle=Next(r,0,6.28f);Vector3 p=new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),Next(r,-.5f,.7f))*Next(r,.3f,.8f);Tetra(v,t,uv,a,p,Next(r,.025f,.065f),i*.23f);}
            return Finish(v,t,uv,a);
        }
        static Mesh BuildCommon(int seed)
        {
            var r=new System.Random(seed);var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector4>();var a=new List<Vector4>();
            for(int i=0;i<6;i++) {
                Vector3 p=new Vector3(Next(r,-.5f,.5f),Next(r,-.5f,.5f),Next(r,-.2f,.6f));float size=Next(r,.07f,.13f);
                // Six crossed, curved octahedral surfaces form a small common smoke proxy.
                int s=v.Count;v.AddRange(new[]{p+Vector3.up*size,p-Vector3.up*size,p+Vector3.right*size,p-Vector3.right*size,p+Vector3.forward*size,p-Vector3.forward*size});
                for(int j=0;j<6;j++){uv.Add(new Vector4(0,i*.73f,0,0));a.Add(new Vector4(p.x,p.y,p.z,i));}
                t.AddRange(new[]{s,s+2,s+4,s,s+4,s+3,s,s+3,s+5,s,s+5,s+2,s+1,s+4,s+2,s+1,s+3,s+4,s+1,s+5,s+3,s+1,s+2,s+5});
            }
            return Finish(v,t,uv,a);
        }
        static Mesh BuildFlash(int seed)
        {
            var r=new System.Random(seed);var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector4>();var a=new List<Vector4>();
            for(int i=0;i<7;i++){float angle=Next(r,0,6.28f);Tetra(v,t,uv,a,new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),Next(r,-.3f,.3f))*.12f,.04f,i);}
            return Finish(v,t,uv,a);
        }
        // Original procedural artwork, not extracted reference pixels. 16 discrete frames
        // retain broad ink surfaces and replace contours rather than reveal tree segments.
        public static Texture2D BuildAtlas(int seed)
        {
            const int cell=192,size=cell*4;var pixels=new Color32[size*size];var r=new System.Random(seed);
            float phase=Next(r,0,6.28f);var radii=new float[1024];
            for(int frame=0;frame<16;frame++) {
                float time=frame/15f,shape=phase+frame*.71f;
                for(int i=0;i<radii.Length;i++) {
                    float a=i*Mathf.PI*2/radii.Length;
                    float broad=Mathf.Pow(Mathf.Max(0,Mathf.Cos(a*5+shape*.45f)),3);
                    float tear=Mathf.Pow(Mathf.Max(0,Mathf.Sin(a*11-shape)),10);
                    radii[i]=(.34f+.28f*broad+.23f*tear+.045f*Mathf.Sin(a*29+shape))* (frame==0?.6f:1);
                }
                for(int y=0;y<cell;y++)for(int x=0;x<cell;x++) {
                    float px=(x+.5f)/cell*2-1,py=(y+.5f)/cell*2-1;
                    float angle=Mathf.Atan2(py*.95f,px);if(angle<0)angle+=Mathf.PI*2;
                    float distance=radii[(int)(angle/(Mathf.PI*2)*1023)]-Mathf.Sqrt(px*px+py*py*.9f);
                    float holes=0;
                    if(frame>=7) {
                        float cut=Mathf.Sin(px*23+shape)*Mathf.Sin(py*19-shape*.7f);
                        holes=Mathf.SmoothStep(0,1,(time-.40f)*2.3f+cut*.5f);
                    }
                    float alpha=Mathf.SmoothStep(0,1,(distance+.006f)/.012f)*(1-holes);
                    if(frame>=12)alpha*=Mathf.Clamp01((16-frame)/4f);
                    float edge=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.008f,.035f,Mathf.Max(0,distance)));
                    int index=((frame/4)*cell+y)*size+(frame%4)*cell+x;
                    pixels[index]=new Color32((byte)(edge*255),0,0,(byte)(alpha*255));
                }
            }
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false,true){name="BlackFlash RND atlas",hideFlags=HideFlags.DontSave,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            texture.SetPixels32(pixels);texture.Apply(false,false);return texture;
        }
    }

    public sealed class ShibuyaBlackFlashRndWindow:EditorWindow
    {
        [SerializeField] int variant=1,seed=71;
        [SerializeField] float diameter=4,speed=1;
        [SerializeField] bool loop;
        ShibuyaBlackFlashRndPreview preview;double started;bool pendingStart;int preparedFrame;
        public bool WaitingForFirstFrame=>pendingStart;
        public float PreviewAge {get;private set;}
        [MenuItem("JJK Game/시부야 PASS4H/흑섬 시각 R&D")]
        public static void Open()=>GetWindow<ShibuyaBlackFlashRndWindow>("흑섬 시각 R&D");
        void OnEnable()=>EditorApplication.update+=Tick;
        void OnDisable(){EditorApplication.update-=Tick;Stop();}
        void Stop(){preview?.Dispose();preview=null;pendingStart=false;}
        public void BeginPreview(ShibuyaMatch match)
        {
            Stop();if(match==null)return;
            preview=new ShibuyaBlackFlashRndPreview((ShibuyaBlackFlashRndKind)variant,match.Dummy.transform.position+Vector3.up,match.Dummy.transform.position-match.Player.transform.position,seed,Mathf.Clamp(diameter,1,8));
            // Atlas generation is synchronous QA work. Anchor playback to the NEXT
            // actual runtime frame, so generation time never consumes the burst.
            preparedFrame=Time.frameCount;pendingStart=true;PreviewAge=0;
        }
        void Tick()
        {
            if(!Application.isPlaying){Stop();return;}if(preview==null)return;
            if(pendingStart){if(Time.frameCount==preparedFrame)return;started=Time.unscaledTimeAsDouble;pendingStart=false;}
            float t=(float)(Time.unscaledTimeAsDouble-started)*speed;
            if(loop)t%=1.1f;PreviewAge=t;preview.Sample(t,Camera.main);Repaint();
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("QA 전용 무피해 시제품입니다. 실제 전투 기본 REV1은 교체하지 않습니다. 1배속 비교를 먼저 확인하세요.",MessageType.Info);
            var so=new SerializedObject(this);so.Update();var p=so.FindProperty("variant");p.intValue=EditorGUILayout.Popup("제작 방식",p.intValue,ShibuyaBlackFlashRndPreview.Labels);
            EditorGUILayout.PropertyField(so.FindProperty("seed"),new GUIContent("동일 형태 시드"));
            EditorGUILayout.PropertyField(so.FindProperty("diameter"),new GUIContent("공통 표시 지름 (m)"));
            var s=so.FindProperty("speed");s.floatValue=EditorGUILayout.Slider("재생 배속",s.floatValue,.25f,1);
            EditorGUILayout.PropertyField(so.FindProperty("loop"),new GUIContent("반복 재생"));so.ApplyModifiedProperties();
            using(new EditorGUI.DisabledScope(!Application.isPlaying))if(GUILayout.Button("선택한 시제품 재생 (무피해)")) {
                BeginPreview(Object.FindFirstObjectByType<ShibuyaMatch>());
            }
            if(GUILayout.Button("미리보기 정리"))Stop();
            EditorGUILayout.LabelField("상태",preview==null?"중지":"무피해 재생 중");
        }
    }
}
