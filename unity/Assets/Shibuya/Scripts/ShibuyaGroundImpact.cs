using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Shibuya
{
    // One reusable temporary mark per receiver; no scene assets, renderer features or floor edits.
    [DisallowMultipleComponent]
    public sealed class ShibuyaGroundImpact : MonoBehaviour
    {
        private GameObject mark;
        private Mesh mesh;
        private Material material;
        private float began, hold, fade, darkness;
        private int generation;
        public bool IsVisible => mark != null && mark.activeSelf;
        public int PlayCount { get; private set; }
        public int MeshIdentity => mesh == null ? 0 : mesh.GetInstanceID();

        public void Play(RaycastHit surface, ShibuyaDownSettings settings)
        {
            if(surface.collider==null || surface.collider.GetComponentInParent<ShibuyaFighter>()!=null
                || Vector3.Dot(surface.normal,Vector3.up)<.65f) return;
            if(mark==null) {
                mark=new GameObject("Shibuya Temporary Ground Crack");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(mark,gameObject.scene);
                mesh=new Mesh {name="Shibuya Pooled Ground Crack"};
                mark.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=mark.AddComponent<MeshRenderer>();
                material=new Material(Shader.Find("Universal Render Pipeline/Unlit")) {name="Shibuya Ground Crack Runtime"};
                material.SetFloat("_Surface",1); material.SetFloat("_ZWrite",0);
                material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_Cull",(float)CullMode.Off);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.renderQueue=3001;
                renderer.sharedMaterial=material; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            }
            mark.transform.SetPositionAndRotation(surface.point,Quaternion.FromToRotation(Vector3.up,surface.normal));
            Build(settings.crackRadius,surface);
            began=Time.time; hold=settings.crackHoldTime; fade=Mathf.Max(.01f,settings.crackFadeTime);
            darkness=settings.crackDarkness; PlayCount++; mark.SetActive(true); UpdateColor(1);
        }
        private void Build(float radius,RaycastHit surface)
        {
            var vertices=new List<Vector3>(); var triangles=new List<int>();
            var random=new System.Random(++generation*7919);
            for(int branch=0;branch<9;branch++) {
                float angle=(branch+(float)random.NextDouble()*.6f)*Mathf.PI*2/9;
                Vector3 previous=Vector3.zero;
                for(int segment=1;segment<=5;segment++) {
                    float a=angle+((float)random.NextDouble()-.5f)*.45f;
                    Vector3 next=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius*segment/5;
                    AddSegment(previous,next,.025f*(1f-segment/7f),surface,vertices,triangles);
                    if(segment==3) AddSegment(next,next+new Vector3(Mathf.Cos(a+.8f),0,Mathf.Sin(a+.8f))*radius*.25f,
                        .012f,surface,vertices,triangles);
                    previous=next;
                }
            }
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateBounds();
        }
        private void AddSegment(Vector3 a,Vector3 b,float width,RaycastHit surface,List<Vector3> vertices,List<int> triangles)
        {
            Vector3 right=Vector3.Cross((b-a).normalized,Vector3.up)*width;
            Vector3[] points={a-right,a+right,b-right*.6f,b+right*.6f};
            for(int i=0;i<points.Length;i++) {
                Vector3 world=mark.transform.TransformPoint(points[i]);
                if(!Physics.Raycast(world+surface.normal*.25f,-surface.normal,out var hit,.5f,~0,QueryTriggerInteraction.Ignore)
                    || hit.collider!=surface.collider || Vector3.Dot(hit.normal,surface.normal)<.8f) return;
                points[i]=mark.transform.InverseTransformPoint(hit.point+hit.normal*.012f);
            }
            int n=vertices.Count; vertices.AddRange(points);
            triangles.AddRange(new[]{n,n+1,n+2,n+2,n+1,n+3});
        }
        private void Update()
        {
            if(!IsVisible) return;
            float age=Time.time-began;
            if(age>=hold+fade) {Clear(); return;}
            UpdateColor(age<=hold?1:1-Mathf.SmoothStep(0,1,(age-hold)/fade));
        }
        private void UpdateColor(float alpha) => material.SetColor("_BaseColor",new Color(.035f,.025f,.045f,darkness*alpha));
        public void Clear() { if(mark!=null) mark.SetActive(false); }
        private void OnDisable() => Clear();
        private void OnDestroy()
        {
            if(mark!=null) Destroy(mark);
            if(mesh!=null) Destroy(mesh);
            if(material!=null) Destroy(material);
        }
    }
}
