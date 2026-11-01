using System;
using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // Fixed topology per pooled spawn; GPU uses longitudinal coordinates and anchors.
    public sealed class ShibuyaBlackFlashGeometry
    {
        readonly List<Vector3> vertices = new List<Vector3>(8192);
        readonly List<int> triangles = new List<int>(32768);
        readonly List<Color> colors = new List<Color>(8192);
        readonly List<Vector4> motion = new List<Vector4>(8192);
        readonly List<Vector4> anchors = new List<Vector4>(8192);
        readonly Vector3[] path = new Vector3[16];
        System.Random random;
        float R(float low, float high) => Mathf.Lerp(low, high, (float)random.NextDouble());
        Vector3 Unit() { float z=R(-1,1),a=R(0,Mathf.PI*2),s=Mathf.Sqrt(1-z*z);return new Vector3(s*Mathf.Cos(a),z,s*Mathf.Sin(a)); }
        void Vertex(Vector3 p,Color c,Vector4 data,Vector3 anchor) {
            vertices.Add(p);colors.Add(c);motion.Add(data);anchors.Add(new Vector4(anchor.x,anchor.y,anchor.z,0));
        }
        void Triangle(int a,int b,int c){triangles.Add(a);triangles.Add(b);triangles.Add(c);}
        void Tube(int count,float width,Color color,float kind,float flatten=1,float birth=0)
        {
            const int sides=6;int start=vertices.Count;float strokeSeed=R(0,97);Vector3 root=path[0];
            for(int j=0;j<count;j++) {
                Vector3 tangent=(path[Mathf.Min(j+1,count-1)]-path[Mathf.Max(0,j-1)]).normalized;
                Vector3 axis=Vector3.Cross(tangent,Mathf.Abs(tangent.y)>.92f?Vector3.right:Vector3.up).normalized;
                Vector3 other=Vector3.Cross(tangent,axis);float f=(float)j/(count-1);
                float radius=Mathf.Max(.001f,width*Mathf.Lerp(1,.012f,Mathf.Pow(f,.72f))*(j%2==0?1:.85f));
                for(int k=0;k<sides;k++) {float angle=k*Mathf.PI*2/sides;
                    Color tint=color;if(kind>.5f&&kind<2.5f)tint.g=(float)k/sides;
                    Vertex(path[j]+radius*(axis*Mathf.Cos(angle)+other*Mathf.Sin(angle)*flatten),tint,new Vector4(f,kind,strokeSeed,birth),root);}
                if(j==0)continue;
                for(int k=0;k<sides;k++) {int a=start+(j-1)*sides+k,b=start+(j-1)*sides+(k+1)%sides,c=start+j*sides+k,d=start+j*sides+(k+1)%sides;Triangle(a,b,c);Triangle(b,d,c);}
            }
            for(int k=1;k<sides-1;k++){Triangle(start,start+k,start+k+1);int end=start+(count-1)*sides;Triangle(end,end+k+1,end+k);}
        }
        void Mass(Vector3 center,Vector3 scale,float rough,Color color,float kind)
        {
            const int sides=16,rows=10;int start=vertices.Count;float seed=R(0,97);var data=new Vector4(0,kind,seed,0);
            Vertex(center+Vector3.up*scale.y,color,data,center);
            for(int j=1;j<rows;j++)for(int k=0;k<sides;k++) {
                float a=k*Mathf.PI*2/sides,b=j*Mathf.PI/rows;
                Vector3 u=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));
                float tear=1+rough*Mathf.Sin(a*5+b*3+seed)*Mathf.Sin(b*7-a*3);
                Vertex(center+Vector3.Scale(u,scale)*tear,color,data,center);
            }
            int bottom=vertices.Count;Vertex(center-Vector3.up*scale.y,color,data,center);
            for(int k=0;k<sides;k++) {
                Triangle(start,start+1+(k+1)%sides,start+1+k);
                for(int j=0;j<rows-2;j++){int a=start+1+j*sides+k,b=start+1+j*sides+(k+1)%sides;Triangle(a,b,a+sides);Triangle(b,b+sides,a+sides);}
                int last=start+1+(rows-2)*sides;Triangle(bottom,last+k,last+(k+1)%sides);
            }
        }
        void Branch(Vector3 origin,Vector3 direction,float length,int count,float noise=.1f)
        {
            path[0]=origin;Vector3 drift=Vector3.Cross(direction,Unit()).normalized;
            for(int i=1;i<count;i++){float f=(float)i/(count-1);path[i]=origin+direction*length*f+drift*Mathf.Sin(f*5)*length*.14f+Unit()*length*noise*Mathf.Sin(f*Mathf.PI);}
        }
        public void Build(Mesh mesh,int layer,ShibuyaBlackFlashProfile p,int seed)
        {
            random=new System.Random(seed);vertices.Clear();triangles.Clear();colors.Clear();motion.Clear();anchors.Clear();
            if(layer==0) {
                for(int i=0;i<11;i++){Branch(Vector3.zero,Unit(),R(.65f,1.6f),3,.015f);Tube(3,R(.012f,.035f),Color.white,6);}
            } else if(layer==1) {
                // Compact seed plus open spaces between curved torn lobes, no overlapping spheres.
                Mass(Vector3.zero,new Vector3(.6f,.7f,.8f)*p.coreSize*Mathf.Clamp(p.coreDensity,.15f,1),.18f,Color.white,0);
                for(int i=0;i<4;i++) {
                    float a=i*1.63f+R(-.3f,.3f);Vector3 d=new Vector3(Mathf.Cos(a),Mathf.Sin(a),R(-.15f,.65f)).normalized;
                    Branch(d*.1f,d,p.coreSize*R(1.15f,2.1f),9,.075f);Tube(9,p.coreSize*R(.4f,.6f),Color.white,0,.45f);
                }
            } else if(layer==2) {
                int count=Mathf.Clamp(p.branchCount,3,24),primary=Mathf.Clamp(p.primaryBranchCount,2,count);
                for(int i=0;i<count;i++) {
                    bool main=i<primary;float angle=i*2.39996f+R(-.4f,.4f);
                    Vector3 d=main?new Vector3(Mathf.Cos(angle)*1.4f,Mathf.Sin(angle)*1.4f,R(.4f,.9f)+p.forwardBias).normalized:Unit();
                    float length=p.branchLength*(main?R(.85f,1.3f):R(.3f,.62f));Vector3 root=d*(main?.13f:R(.45f,.85f));
                    Branch(root,d,length,14,main?.035f:.1f);Vector3 forkA=path[4],forkB=path[8],forkC=path[10];
                    Tube(14,p.branchWidth*(main?R(1.15f,1.85f):.23f),Color.white,main?1:2,main?.7f:.8f);
                    for(int j=0;j<Mathf.Clamp(p.forksPerBranch,0,3);j++) {
                        Vector3 fork=j==0?forkA:j==1?forkB:forkC;
                        Branch(fork,(d*.4f+Unit()).normalized,length*R(.24f,.5f),6,.13f);
                        Tube(6,p.branchWidth*R(.12f,.22f),Color.white,2,.8f,j==0?.25f:.5f);
                    }
                }
            } else if(layer==3) {
                for(int i=0;i<7;i++) {
                    float offset=R(0,6.28f),span=R(.5f,1.4f);Quaternion rotation=Quaternion.Euler(R(-65,65),R(0,180),R(-25,25));
                    for(int j=0;j<15;j++){float a=offset+span*j/14;path[j]=rotation*new Vector3(Mathf.Cos(a)*(1+.12f*Mathf.Sin(a*7)),Mathf.Sin(a),.2f*Mathf.Sin(a*4));}
                    Tube(15,R(.012f,.033f),new Color(.8f,.85f,1,.55f),5);
                }
            } else {
                for(int i=0;i<Mathf.Clamp(p.smokeCount,0,32);i++) {
                    Vector3 d=(Unit()+Vector3.forward*.4f).normalized;float size=Mathf.Max(.02f,p.smokeSize)*R(.6f,1.5f);
                    Mass(d*R(.55f,1.45f),new Vector3(size,size*R(.9f,1.6f),size*.85f),.16f,new Color(.15f,.15f,.15f,1),3);
                }
                for(int i=0;i<12;i++){Vector3 d=Unit();Branch(d*R(.75f,1.65f),d,R(.25f,.7f),6,.16f);Tube(6,R(.013f,.026f),Color.white,4,.8f);}
            }
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,motion);mesh.SetUVs(1,anchors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            // Expand culling bounds for GPU drift, not geometry dimensions.
            var bounds=mesh.bounds;bounds.Expand(Mathf.Max(2,p.smokeSpread*2));mesh.bounds=bounds;
        }
    }
}
