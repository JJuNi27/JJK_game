using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum CursedAuraBodyRegion { Head, Trunk, Arm, Leg }

    // Cached surface attachment. Replacing this provider is the skin/bone extension point.
    public sealed class CursedAuraSurfacePoint
    {
        public Renderer Source {get;internal set;}
        public Vector3 LocalPosition {get;internal set;}
        public Vector3 LocalNormal {get;internal set;}
        public CursedAuraBodyRegion Region {get;internal set;}
        public bool UsesMeshSurface {get;internal set;}
        public int Triangle {get;internal set;}
        public Vector3 Barycentric {get;internal set;}
        public Vector3 WorldPosition=>Source.transform.TransformPoint(LocalPosition);
        public Vector3 WorldNormal=>Source.transform.worldToLocalMatrix.transpose.MultiplyVector(LocalNormal).normalized;
    }

    internal static class CursedEnergyAuraSurface
    {
        public const int Capacity=48;
        private sealed class Source
        {
            public Renderer renderer;
            public CursedAuraBodyRegion region;
            public readonly List<CursedAuraSurfacePoint> candidates=new List<CursedAuraSurfacePoint>();
            public float area,weight;
        }
        private struct Triangle {public Vector3 a,b,c,n;public float area;public int index;}
        private static float Hash(int n)=>Mathf.Repeat(Mathf.Sin(n*127.1f+37.7f)*43758.5453f,1);

        public static Renderer[] Collect(ShibuyaFighter owner,Renderer[] requested,Shader rimShader)
        {
            bool explicitList=requested!=null&&requested.Length>0;
            var result=new List<Renderer>();
            foreach(var r in explicitList?requested:owner.GetComponentsInChildren<Renderer>()) {
                if(r==null||!r.transform.IsChildOf(owner.transform)||!r.enabled||result.Contains(r))continue;
                if(r.GetComponentInParent<CursedEnergyAuraVisual>()!=null||r.GetComponentInParent<Canvas>()!=null||r.GetComponentInParent<Camera>()!=null)continue;
                if(r.gameObject.CompareTag("EditorOnly")||(r.hideFlags&HideFlags.DontSave)!=0)continue;
                if(r.sharedMaterial!=null&&r.sharedMaterial.shader==rimShader)continue;
                if(!(r is SkinnedMeshRenderer)&&(!(r is MeshRenderer)||r.GetComponent<MeshFilter>()?.sharedMesh==null))continue;
                if(!explicitList) {
                    Vector3 size=r.localBounds.size;
                    size=new Vector3(r.transform.TransformVector(Vector3.right*size.x).magnitude,
                        r.transform.TransformVector(Vector3.up*size.y).magnitude,r.transform.TransformVector(Vector3.forward*size.z).magnitude);
                    // Auto mode excludes thin helper markers; equipment can use the explicit body list.
                    if(Mathf.Min(size.x,Mathf.Min(size.y,size.z))<Mathf.Max(size.x,Mathf.Max(size.y,size.z))*.10f)continue;
                }
                result.Add(r);
            }
            return result.ToArray();
        }

        public static List<CursedAuraSurfacePoint> Build(ShibuyaFighter owner,Renderer[] renderers,CursedEnergyAuraProfile profile)
        {
            var sources=new List<Source>();Bounds bounds=default;bool first=true;
            foreach(var r in renderers) {
                Bounds b=r.localBounds;
                for(int i=0;i<8;i++) {
                    Vector3 v=owner.transform.InverseTransformPoint(r.transform.TransformPoint(b.center+Vector3.Scale(b.extents,
                        new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
                    if(first){bounds=new Bounds(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);
                }
            }
            for(int i=0;i<renderers.Length;i++) {
                var r=renderers[i];Vector3 center=owner.transform.InverseTransformPoint(r.transform.TransformPoint(r.localBounds.center));
                float height=(center.y-bounds.min.y)/Mathf.Max(.01f,bounds.size.y);
                var region=height>.78f?CursedAuraBodyRegion.Head:height<.43f?CursedAuraBodyRegion.Leg:
                    Mathf.Abs(center.x-bounds.center.x)>bounds.extents.x*.60f?CursedAuraBodyRegion.Arm:CursedAuraBodyRegion.Trunk;
                var s=new Source{renderer=r,region=region};
                GenerateCandidates(owner,s,i);if(s.candidates.Count==0)continue;
                s.weight=s.area*(region==CursedAuraBodyRegion.Leg?profile.lowerBodyWeight:region==CursedAuraBodyRegion.Head?.65f:1);
                sources.Add(s);
            }
            var points=new List<CursedAuraSurfacePoint>();
            Allocate(sources,Mathf.Clamp(profile.detailFlowCount,1,32),2,points,false);
            return points;
        }

        private static void GenerateCandidates(ShibuyaFighter owner,Source s,int sourceIndex)
        {
            var triangles=new List<Triangle>();
            Mesh mesh=s.renderer is SkinnedMeshRenderer?null:s.renderer.GetComponent<MeshFilter>()?.sharedMesh;
            bool readable=mesh!=null&&mesh.isReadable;Vector3[] vertices;int[] indices;
            if(readable){vertices=mesh.vertices;indices=mesh.triangles;}
            else {
                // Per-renderer bounds SURFACE fallback, not a whole-character capsule.
                Bounds b=s.renderer.localBounds;vertices=new Vector3[8];
                for(int i=0;i<8;i++)vertices[i]=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                indices=new[]{0,2,3,0,3,1,4,5,7,4,7,6,0,4,6,0,6,2,1,3,7,1,7,5,0,1,5,0,5,4,2,6,7,2,7,3};
            }
            for(int t=0;t+2<indices.Length;t+=3) {
                Vector3 a=vertices[indices[t]],b=vertices[indices[t+1]],c=vertices[indices[t+2]];
                Vector3 cross=Vector3.Cross(b-a,c-a);if(cross.sqrMagnitude<1e-12f)continue;
                Vector3 normal=s.renderer.transform.worldToLocalMatrix.transpose.MultiplyVector(cross.normalized).normalized;
                // Do not emit through the floor, or place a main flame directly over the face.
                if(normal.y<-.65f)continue;
                if(s.region==CursedAuraBodyRegion.Head&&Vector3.Dot(normal,owner.transform.forward)>.60f)continue;
                float area=Vector3.Cross(s.renderer.transform.TransformVector(b-a),s.renderer.transform.TransformVector(c-a)).magnitude*.5f;
                triangles.Add(new Triangle{a=a,b=b,c=c,n=cross.normalized,area=area,index=t/3});s.area+=area;
            }
            if(s.area<1e-8f)return;
            for(int i=0;i<128;i++) {
                float pick=(i+.5f)/128*s.area,cumulative=0;Triangle tri=triangles[triangles.Count-1];
                foreach(var t in triangles){cumulative+=t.area;if(pick<=cumulative){tri=t;break;}}
                float u=Mathf.Sqrt(Hash(i*3+sourceIndex*401+13)),v=Hash(i*3+sourceIndex*401+57);
                Vector3 bary=new Vector3(1-u,u*(1-v),u*v);
                s.candidates.Add(new CursedAuraSurfacePoint{Source=s.renderer,Region=s.region,UsesMeshSurface=readable,
                    LocalPosition=tri.a*bary.x+tri.b*bary.y+tri.c*bary.z,LocalNormal=tri.n,Triangle=tri.index,Barycentric=bary});
            }
        }

        private static void Allocate(List<Source> sources,int count,int minimum,List<CursedAuraSurfacePoint> output,bool main)
        {
            if(sources.Count==0)return;
            var quotas=new int[sources.Count];int assigned=0;
            for(int pass=0;pass<minimum;pass++)for(int i=0;i<sources.Count&&assigned<count;i++){quotas[i]++;assigned++;}
            while(assigned<count) {
                int best=0;float score=-1;
                for(int i=0;i<sources.Count;i++){float value=sources[i].weight/(quotas[i]+1);if(value>score){score=value;best=i;}}
                quotas[best]++;assigned++;
            }
            for(int i=0;i<sources.Count;i++)for(int n=0;n<quotas[i];n++) {
                var source=sources[i];CursedAuraSurfacePoint selected=null;float best=-1;
                foreach(var p in source.candidates) {
                    if(output.Contains(p))continue;
                    float distance=float.PositiveInfinity;
                    foreach(var previous in output)if(previous.Source==source.renderer)
                        distance=Mathf.Min(distance,(p.WorldPosition-previous.WorldPosition).sqrMagnitude);
                    if(float.IsPositiveInfinity(distance)) {
                        // First leg current starts near the foot, on an actual sampled side triangle.
                        float y=p.Source.transform.InverseTransformPoint(p.WorldPosition).y;
                        distance=source.region==CursedAuraBodyRegion.Leg
                            ?source.renderer.localBounds.max.y-y:Hash(p.Triangle*7+n*13+i*31+17);
                    }
                    if(distance>best){best=distance;selected=p;}
                }
                if(selected!=null)output.Add(selected);
            }
        }
    }
}
