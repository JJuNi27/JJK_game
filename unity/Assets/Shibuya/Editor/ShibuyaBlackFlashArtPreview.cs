using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public enum ShibuyaBlackFlashArtView { BlackMass, RedEdge, Combined }
    public sealed class ShibuyaBlackFlashArtPreview : IDisposable
    {
        public const string ProfilePath="Assets/Shibuya/Editor/Black Flash Art Directed QA.asset";
        public readonly GameObject Root;
        public readonly ShibuyaBlackFlashArtProfile Profile;
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<Material> materials=new List<Material>();
        readonly List<MeshRenderer> renderers=new List<MeshRenderer>();
        readonly List<bool> reds=new List<bool>();
        readonly List<int> kinds=new List<int>();
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        public int VertexCount=>meshes.Sum(m=>m.vertexCount);
        public int RendererCount=>renderers.Count;
        public float Age {get;private set;}
        public static readonly string[] ViewLabels={"L2 검은 본체만","L3 붉은 외곽·방전만","L2 + L3","기존 REV1 비교"};
        public ShibuyaBlackFlashArtPreview(Vector3 point,Vector3 direction,ShibuyaBlackFlashArtProfile settings)
        {
            Profile=Object.Instantiate(settings);Profile.name="BlackFlash Art owned profile";Profile.hideFlags=HideFlags.DontSave;
            Root=new GameObject("BlackFlash Art QA"){hideFlags=HideFlags.DontSave};Root.transform.SetPositionAndRotation(point,Quaternion.LookRotation(direction));
            var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shibuya/Editor/ShibuyaBlackFlashArt.shader");
            if(shader==null||Profile.atlas==null)throw new InvalidOperationException("Art shader/atlas missing");
            var black=Own(shader,false);var red=Own(shader,true);
            for(int i=0;i<2;i++) {
                var mesh=Sheet(Profile.worldSize*(i==0?1:Profile.sideScale),Profile.curvature,i);meshes.Add(mesh);
                var body=Add(mesh,black,false,0,"L2 curved ink "+i);
                var edge=Add(mesh,red,true,0,"L3 attached edge "+i);
                Quaternion rotation=Quaternion.Euler(0,i*90,0);body.transform.localRotation=rotation;edge.transform.localRotation=rotation;
            }
            var volume=Volume();meshes.Add(volume);Add(volume,black,false,1,"L2 contact volume");
            if(Profile.discharges){var bolts=Discharges();meshes.Add(bolts);Add(bolts,red,true,2,"L3 independent discharges");}
            foreach(var t in Root.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
            Root.SetActive(false);
        }
        Material Own(Shader shader,bool red)
        {
            var m=new Material(shader){name="BlackFlash Art owned material",hideFlags=HideFlags.DontSave};
            m.SetTexture("_Atlas",Profile.atlas);m.SetFloat("_Layer",red?1:0);m.SetFloat("_DstBlend",red?1:10);m.SetFloat("_ZWrite",red?0:1);m.SetFloat("_Offset",red?-1:0);m.renderQueue=red?3010:2980;
            materials.Add(m);return m;
        }
        MeshRenderer Add(Mesh mesh,Material material,bool red,int kind,string label)
        {
            var go=new GameObject(label){hideFlags=HideFlags.DontSave};go.transform.SetParent(Root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;
            renderers.Add(r);reds.Add(red);kinds.Add(kind);return r;
        }
        static Mesh Finish(List<Vector3> v,List<int> tris,List<Vector2> uv)
        {
            var m=new Mesh{name="BlackFlash Art owned mesh",hideFlags=HideFlags.DontSave};m.SetVertices(v);m.SetTriangles(tris,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        static Mesh Sheet(float size,float depth,int layer)
        {
            const int n=16;var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++) {
                float u=(float)x/n,w=(float)y/n;
                v.Add(new Vector3((u-.5f)*size,(w-.5f)*size,depth*Mathf.Sin((u-.5f)*Mathf.PI)*Mathf.Sin(w*Mathf.PI)+(layer==0?-.05f:.09f)));
                uv.Add(new Vector2(u,w));
            }
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){int a=y*(n+1)+x;tri.AddRange(new[]{a,a+n+1,a+1,a+1,a+n+1,a+n+2});}
            return Finish(v,tri,uv);
        }
        static Mesh Volume()
        {
            var points=new[]{new Vector3(-.34f,0,0),new Vector3(.28f,.02f,0),new Vector3(0,-.27f,0),new Vector3(.04f,.32f,0),new Vector3(0,0,-.28f),new Vector3(0,0,.26f)};
            return Finish(points.ToList(),new List<int>{4,0,3,4,3,1,4,1,2,4,2,0,5,3,0,5,1,3,5,2,1,5,0,2},Enumerable.Repeat(Vector2.zero,6).ToList());
        }
        static Mesh Discharges()
        {
            // Two authored broken paths, no procedural branch graph or radial ray fan.
            var paths=new[]{new[]{new Vector3(.55f,.65f,-.3f),new Vector3(.9f,.78f,-.25f),new Vector3(.95f,.98f,-.28f),new Vector3(1.18f,1.01f,-.16f),new Vector3(1.42f,1.35f,-.1f)},new[]{new Vector3(-.5f,-.65f,-.26f),new Vector3(-.9f,-.74f,-.3f),new Vector3(-.98f,-.55f,-.2f),new Vector3(-1.25f,-.83f,-.25f),new Vector3(-1.55f,-.88f,-.08f)}};
            var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
            foreach(var path in paths){int start=v.Count;for(int i=0;i<path.Length;i++){
                float f=(float)i/(path.Length-1);Vector3 tangent=(path[Mathf.Min(i+1,path.Length-1)]-path[Mathf.Max(0,i-1)]).normalized;Vector3 normal=Vector3.Cross(tangent,Vector3.forward).normalized*.022f*(1-f*.8f);
                v.Add(path[i]+normal);v.Add(path[i]-normal);uv.Add(new Vector2(f,0));uv.Add(new Vector2(f,1));
                if(i>0){int a=start+(i-1)*2;tri.AddRange(new[]{a,a+1,a+2,a+2,a+1,a+3});}
            }}return Finish(v,tri,uv);
        }
        public void Sample(float age,ShibuyaBlackFlashArtView view)
        {
            Age=age;Root.SetActive(age>=0&&age<Profile.duration);if(!Root.activeSelf)return;
            for(int i=0;i<renderers.Count;i++) {
                renderers[i].enabled=view==ShibuyaBlackFlashArtView.Combined||reds[i]&&view==ShibuyaBlackFlashArtView.RedEdge||!reds[i]&&view==ShibuyaBlackFlashArtView.BlackMass;
                block.Clear();block.SetFloat("_Age",age);block.SetFloat("_Kind",kinds[i]);block.SetVector("_Times",new Vector4(Profile.firstEnd,Profile.peakEnd,Profile.tearEnd,Mathf.Max(Profile.tearEnd+.01f,Profile.duration)));
                var order=Profile.poseOrder;block.SetVector("_PoseOrder",order!=null&&order.Length==4?new Vector4(order[0],order[1],order[2],order[3]):new Vector4(1,0,2,3));
                block.SetColor("_Red",Profile.red);block.SetFloat("_Edge",Profile.edgePixels);block.SetFloat("_Halo",Profile.haloPixels);block.SetFloat("_Coverage",Profile.edgeCoverage);renderers[i].SetPropertyBlock(block);
            }
        }
        public void Dispose()
        {
            if(Root!=null)Object.DestroyImmediate(Root);
            foreach(var m in meshes)if(m!=null)Object.DestroyImmediate(m);
            foreach(var m in materials)if(m!=null)Object.DestroyImmediate(m);
            if(Profile!=null)Object.DestroyImmediate(Profile);
        }
    }
    public sealed class ShibuyaBlackFlashArtWindow : EditorWindow
    {
        ShibuyaBlackFlashArtProfile profile;ShibuyaBlackFlashArtPreview preview;ShibuyaBlackFlashRndPreview legacy;
        int mode;bool loop=true,hold;float phase=.06f;double start=-1;int prepared;
        [MenuItem("JJK Game/시부야 PASS4H/흑섬 아트 디렉션 L2·L3")]
        static void Open()=>GetWindow<ShibuyaBlackFlashArtWindow>("흑섬 L2·L3");
        void OnEnable(){profile=AssetDatabase.LoadAssetAtPath<ShibuyaBlackFlashArtProfile>(ShibuyaBlackFlashArtPreview.ProfilePath);EditorApplication.update+=Tick;}
        void OnDisable(){EditorApplication.update-=Tick;Stop();}
        void Stop(){preview?.Dispose();legacy?.Dispose();preview=null;legacy=null;}
        void Begin()
        {
            Stop();var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(!EditorApplication.isPlaying||match==null||profile==null)return;
            Vector3 direction=(match.Dummy.transform.position-match.Player.transform.position).normalized;
            Vector3 point=match.Dummy.transform.position+Vector3.up-direction*match.Dummy.GetComponent<CharacterController>().radius;
            if(mode==3)legacy=new ShibuyaBlackFlashRndPreview(ShibuyaBlackFlashRndKind.Legacy,point,direction,71,4);
            else preview=new ShibuyaBlackFlashArtPreview(point,direction,profile);
            prepared=Time.frameCount;start=-1;
        }
        void Tick()
        {
            if(preview==null&&legacy==null)return;if(!EditorApplication.isPlaying){Stop();return;}
            if(start<0){if(Time.frameCount==prepared)return;start=Time.unscaledTimeAsDouble;}
            float age=hold?phase:(float)(Time.unscaledTimeAsDouble-start);if(loop&&!hold)age%=1;
            preview?.Sample(age,(ShibuyaBlackFlashArtView)mode);legacy?.Sample(age,Camera.main);Repaint();
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("독립 QA · 무피해 · 기존 전투 REV1 유지. L2/L3만 표시합니다. 새 아트의 시각 승인은 사용자 검수 대기입니다.",MessageType.Info);
            profile=(ShibuyaBlackFlashArtProfile)EditorGUILayout.ObjectField("QA 설정",profile,typeof(ShibuyaBlackFlashArtProfile),false);
            EditorGUI.BeginChangeCheck();mode=EditorGUILayout.Popup("표시 모드",mode,ShibuyaBlackFlashArtPreview.ViewLabels);if(EditorGUI.EndChangeCheck()&&(preview!=null||legacy!=null))Begin();
            loop=EditorGUILayout.Toggle("반복 재생",loop);hold=EditorGUILayout.Toggle("검수 시각 고정",hold);if(hold)phase=EditorGUILayout.Slider("고정 시각 (초)",phase,0,.75f);
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying||profile==null)){if(GUILayout.Button("접촉 위치에서 재생"))Begin();}
            if(GUILayout.Button("임시 자원 정리"))Stop();
        }
    }
}
