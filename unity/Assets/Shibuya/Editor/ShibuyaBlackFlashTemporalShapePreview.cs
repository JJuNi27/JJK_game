using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public enum ShibuyaTemporalShapeKind { Base, Temporal, Shape, TemporalShape }
    public enum ShibuyaTemporalShapeView { Composite, Core, Rupture, RedDiagnostic, Residue }

    public sealed class ShibuyaBlackFlashTemporalShapePreview : IDisposable
    {
        public const string SettingsPath="Assets/Shibuya/Editor/Black Flash Temporal Shape QA.asset";
        public readonly GameObject Root;
        public readonly ShibuyaBlackFlashVisual Visual;
        public readonly ShibuyaBlackFlashProfile Profile;
        public readonly MeshRenderer[] Renderers;
        public readonly ShibuyaTemporalShapeKind Kind;
        public readonly float BirthGrowth, ShapeFit;
        readonly List<Material> materials=new List<Material>();
        readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        public int Strokes {get;private set;}
        public static readonly string[] Labels={"BASE — 기존 REV1","T — 성장 시간만","S — 파열 형상만","TS — 시간 + 형상"};
        public static readonly string[] Views={"전체 합성","검은 핵만","주 파열 메시만","붉은 rim·선·방전 진단","공통 잔류 효과만"};

        public ShibuyaBlackFlashTemporalShapePreview(ShibuyaTemporalShapeKind kind,Vector3 point,Vector3 forward,int seed,ShibuyaBlackFlashTemporalShapeProfile settings)
        {
            Kind=kind;
            var original=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");
            Profile=Object.Instantiate(original);Profile.name="BlackFlash TS owned profile";Profile.hideFlags=HideFlags.DontSave;
            BirthGrowth=original.ruptureGrowthTime;
            if(kind==ShibuyaTemporalShapeKind.Temporal||kind==ShibuyaTemporalShapeKind.TemporalShape)Profile.ruptureGrowthTime=settings.fastGrowth;
            Profile.inkShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shibuya/Editor/ShibuyaBlackFlashTemporalShape.shader");
            if(Profile.inkShader==null)throw new InvalidOperationException("QA shader missing");
            Material ink=Own(Profile.inkShader),light=Own(Profile.lightShader),smoke=Own(Profile.inkShader);smoke.SetFloat("_ZWrite",0);
            Root=new GameObject("BlackFlash TS "+kind){hideFlags=HideFlags.DontSave};
            Visual=Root.AddComponent<ShibuyaBlackFlashVisual>();Visual.Initialize(ink,light,smoke);
            Visual.Play(point,forward,Profile,seed,Time.unscaledTimeAsDouble);
            Renderers=Root.GetComponentsInChildren<MeshRenderer>();
            var branch=Renderers[2].GetComponent<MeshFilter>().sharedMesh;
            float radius=branch.vertices.Max(v=>v.magnitude);
            ShapeFit=1;
            if(kind==ShibuyaTemporalShapeKind.Shape||kind==ShibuyaTemporalShapeKind.TemporalShape)ShapeFit=Reshape(branch,settings,radius);
            else Strokes=Groups(branch).Count-1;
            // One common BASE-derived scale, not candidate-by-candidate screen area normalization.
            float coreRadius=Renderers[1].GetComponent<MeshFilter>().sharedMesh.vertices.Max(v=>v.magnitude)*Profile.coreExpansion;
            Root.transform.localScale=Vector3.one*(2/Mathf.Max(radius,coreRadius));
            foreach(var t in Root.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
            Root.SetActive(false);
        }
        Material Own(Shader shader){var m=new Material(shader){name="BlackFlash TS owned material",hideFlags=HideFlags.DontSave};materials.Add(m);return m;}
        static List<int> Groups(Mesh mesh)
        {
            var uv=new List<Vector4>();mesh.GetUVs(0,uv);var starts=new List<int>();
            for(int i=0;i<uv.Count;i+=6)if(i==0||(uv[i].x==0&&uv[i-1].x>0))starts.Add(i);
            starts.Add(uv.Count);return starts;
        }
        float Reshape(Mesh mesh,ShibuyaBlackFlashTemporalShapeProfile settings,float originalRadius)
        {
            var vertices=mesh.vertices;var uv=new List<Vector4>();var anchor=new List<Vector4>();mesh.GetUVs(0,uv);mesh.GetUVs(1,anchor);
            var starts=Groups(mesh);Strokes=starts.Count-1;
            for(int group=0;group<starts.Count-1;group++) {
                int start=starts[group],rings=(starts[group+1]-start)/6;
                var centers=new Vector3[rings];
                for(int j=0;j<rings;j++)for(int k=0;k<6;k++)centers[j]+=vertices[start+j*6+k]/6;
                bool main=uv[start].y<1.5f;float cut=main?0:settings.disconnectedStart;
                Vector3 newRoot=Vector3.Lerp(centers[Mathf.FloorToInt(cut*(rings-1))],centers[Mathf.Min(rings-1,Mathf.FloorToInt(cut*(rings-1))+1)],cut*(rings-1)%1);
                for(int j=0;j<rings;j++) {
                    float f=(float)j/(rings-1),mapped=(cut+(1-cut)*f)*(rings-1);int lo=Mathf.FloorToInt(mapped);
                    Vector3 center=Vector3.Lerp(centers[lo],centers[Mathf.Min(lo+1,rings-1)],mapped-lo);
                    Vector3 tangent=(centers[Mathf.Min(j+1,rings-1)]-centers[Mathf.Max(0,j-1)]).normalized;
                    Vector3 wide=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.92f?Vector3.right:Vector3.up).normalized,thin=Vector3.Cross(tangent,wide);
                    for(int k=0;k<6;k++) {
                        int index=start+j*6+k;Vector3 delta=vertices[index]-centers[j];
                        float irregular=1+settings.edgeIrregularity*Mathf.Sin(group*2.7f+j*2.1f+k*1.9f)*Mathf.Sin(f*Mathf.PI);
                        vertices[index]=center+(wide*Vector3.Dot(delta,wide)*(main?settings.mainWidth:settings.fragmentWidth)+thin*Vector3.Dot(delta,thin)*(main?.75f:1))*irregular;
                        anchor[index]=new Vector4(newRoot.x,newRoot.y,newRoot.z,0);
                    }
                }
            }
            float fit=originalRadius/vertices.Max(v=>v.magnitude);
            for(int i=0;i<vertices.Length;i++){vertices[i]*=fit;anchor[i]*=fit;}
            mesh.vertices=vertices;mesh.SetUVs(1,anchor);mesh.RecalculateNormals();mesh.RecalculateBounds();var b=mesh.bounds;b.Expand(4);mesh.bounds=b;return fit;
        }
        public void Sample(float age,ShibuyaTemporalShapeView view=ShibuyaTemporalShapeView.Composite)
        {
            Root.SetActive(age>=0&&age<Profile.Lifetime);if(!Root.activeSelf)return;
            Visual.Sample(age);
            for(int i=0;i<Renderers.Length;i++) {
                var r=Renderers[i];bool selected=view==ShibuyaTemporalShapeView.Composite||view==ShibuyaTemporalShapeView.Core&&i==1||view==ShibuyaTemporalShapeView.Rupture&&i==2||view==ShibuyaTemporalShapeView.RedDiagnostic&&(i==1||i==2||i==4)||view==ShibuyaTemporalShapeView.Residue&&i==4;
                r.enabled&=selected;r.GetPropertyBlock(block);block.SetFloat("_BirthGrowth",BirthGrowth);block.SetFloat("_InspectRed",view==ShibuyaTemporalShapeView.RedDiagnostic?1:0);r.SetPropertyBlock(block);
            }
        }
        public object Metrics(float age)
        {
            float growth=Profile.ruptureGrowthTime,g=Mathf.Clamp01((age-.008f)/growth),s=.24f+.76f*g;
            return new{age,kind=Kind.ToString(),growth,birthGrowth=BirthGrowth,mainTip=.008f+.92f*growth,vertexComplete=.008f+growth,forkTips=new[]{.008f+BirthGrowth*.25f+.92f*growth,.008f+BirthGrowth*.5f+.92f*growth},vertexScale=s,mainReveal=Mathf.Min(1,g/.92f),firstPossibleCoreClip=Profile.coreDuration*.6815f,firstPossibleBranchClip=Profile.branchDuration*.6815f,renderers=Renderers.Select(r=>new{r.name,r.enabled,scale=new[]{r.transform.localScale.x,r.transform.localScale.y,r.transform.localScale.z},queue=r.sharedMaterial.renderQueue,zWrite=r.sharedMaterial.HasProperty("_ZWrite")?r.sharedMaterial.GetFloat("_ZWrite"):0}).ToArray(),vertices=Visual.VertexCount,strokes=Strokes,shapeFit=ShapeFit};
        }
        public void Dispose()
        {
            if(Root!=null)Object.DestroyImmediate(Root);
            foreach(var m in materials)if(m!=null)Object.DestroyImmediate(m);
            if(Profile!=null)Object.DestroyImmediate(Profile);
        }
    }
    public sealed class ShibuyaBlackFlashTemporalShapeWindow : EditorWindow
    {
        ShibuyaBlackFlashTemporalShapeProfile settings;
        ShibuyaBlackFlashTemporalShapePreview preview;
        int kind,view;bool loop,hold;float phase=.1f;double started;int prepared;
        [MenuItem("JJK Game/시부야 PASS4H/흑섬 시간·형상 R&D")]
        static void Open()=>GetWindow<ShibuyaBlackFlashTemporalShapeWindow>("흑섬 시간·형상 R&D");
        void OnEnable(){EditorApplication.update+=Tick;settings=AssetDatabase.LoadAssetAtPath<ShibuyaBlackFlashTemporalShapeProfile>(ShibuyaBlackFlashTemporalShapePreview.SettingsPath);}
        void OnDisable(){EditorApplication.update-=Tick;Stop();}
        void Stop(){preview?.Dispose();preview=null;}
        void Begin()
        {
            Stop();if(!EditorApplication.isPlaying)return;var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null)return;
            preview=new ShibuyaBlackFlashTemporalShapePreview((ShibuyaTemporalShapeKind)kind,match.Dummy.transform.position+Vector3.up,Vector3.forward,71,settings);prepared=Time.frameCount;started=-1;
        }
        void Tick()
        {
            if(preview==null)return;if(!EditorApplication.isPlaying){Stop();return;}
            if(started<0){if(Time.frameCount==prepared)return;started=Time.unscaledTimeAsDouble;}
            float age=hold?phase:(float)(Time.unscaledTimeAsDouble-started);if(loop&&!hold)age%=1;
            preview.Sample(age,(ShibuyaTemporalShapeView)view);Repaint();
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("QA 전용 · 무피해 · 실제 전투 REV1 유지. 붉은 모드는 동일 패스의 색 분리 진단이며 Frame Debugger 캡처가 아닙니다.",MessageType.Info);
            settings=(ShibuyaBlackFlashTemporalShapeProfile)EditorGUILayout.ObjectField("실험 설정",settings,typeof(ShibuyaBlackFlashTemporalShapeProfile),false);
            EditorGUI.BeginChangeCheck();kind=EditorGUILayout.Popup("비교 후보",kind,ShibuyaBlackFlashTemporalShapePreview.Labels);if(EditorGUI.EndChangeCheck()&&preview!=null)Begin();
            view=EditorGUILayout.Popup("레이어 표시",view,ShibuyaBlackFlashTemporalShapePreview.Views);loop=EditorGUILayout.Toggle("반복 재생",loop);hold=EditorGUILayout.Toggle("시각 고정 (검수용)",hold);if(hold)phase=EditorGUILayout.Slider("고정 시각 (초)",phase,0,.75f);
            using(new EditorGUI.DisabledScope(!EditorApplication.isPlaying||settings==null)){if(GUILayout.Button("동일 시드 71로 재생"))Begin();}
            if(GUILayout.Button("임시 자원 정리"))Stop();
        }
    }
}
