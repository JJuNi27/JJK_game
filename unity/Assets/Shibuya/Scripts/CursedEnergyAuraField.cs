using System;
using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // One height-wise union envelope. This is a carrier for the field, not rendered body parts.
    // Only the six source bounds are sliced when a pose changes; no triangle resampling/BakeMesh.
    internal sealed class CursedEnergyAuraField
    {
        internal const int Rows=41,Sides=48,Stride=Sides+1;
        private readonly Renderer[] sources;
        private readonly Matrix4x4[] matrices;
        private readonly Vector3[][] corners;
        private readonly Vector3[] vertices=new Vector3[Rows*Stride];
        private readonly Vector2[] centers=new Vector2[Rows];
        private readonly float[,] radius=new float[Rows,Sides];
        private readonly float[] heights=new float[Rows];
        private readonly bool[] valid=new bool[Rows];
        private readonly List<Vector2> slice=new List<Vector2>(96),hull=new List<Vector2>(192);
        private readonly List<Vector3> normals=new List<Vector3>(Rows*Stride);
        private static readonly Comparison<Vector2> SortPoints=(a,b)=>a.x==b.x?a.y.CompareTo(b.y):a.x.CompareTo(b.x);
        private static readonly int[] Edges={0,1,2,3,4,5,6,7,0,2,1,3,4,6,5,7,0,4,1,5,2,6,3,7};
        private bool fitted;
        private float oldThickness=-1,oldWidth=-1,oldHeight=-1;
        private float oldExpansion=-1;
        private float oldVerticalResponse=-1;
        private float oldReach=-1;
        private float oldUpper=-1,oldLower=-1;
        public int FitCount {get;private set;}
        public double LastFitMilliseconds {get;private set;}
        public Bounds SourceBounds {get;private set;}
        public CursedEnergyAuraField(Renderer[] sources)
        {
            this.sources=sources;matrices=new Matrix4x4[sources.Length];corners=new Vector3[sources.Length][];
            for(int i=0;i<sources.Length;i++)corners[i]=new Vector3[8];
        }
        public Mesh CreateMesh(int role)
        {
            var mesh=new Mesh{name="CursedAura_OwnedVolume_"+role};mesh.MarkDynamic();
            var uv=new Vector2[vertices.Length];var seed=new Vector2[vertices.Length];var mode=new Vector2[vertices.Length];
            var indices=new int[(Rows-1)*Sides*6];int at=0;
            for(int row=0;row<Rows;row++)for(int side=0;side<=Sides;side++) {
                int index=row*Stride+side;uv[index]=new Vector2(side/(float)Sides,row/(float)(Rows-1));
                seed[index]=new Vector2(.37f,0);mode[index]=new Vector2(0,2);
                if(row==Rows-1||side==Sides)continue;
                indices[at++]=index;indices[at++]=index+Stride;indices[at++]=index+Stride+1;
                indices[at++]=index;indices[at++]=index+Stride+1;indices[at++]=index+1;
            }
            mesh.vertices=vertices;mesh.uv=uv;mesh.uv2=seed;mesh.SetUVs(2,new List<Vector2>(mode));mesh.triangles=indices;
            mesh.bounds=new Bounds(Vector3.up,new Vector3(4,5,4));return mesh;
        }
        private static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        private void MakeHull()
        {
            slice.Sort(SortPoints);hull.Clear();
            foreach(var p in slice) {
                while(hull.Count>=2&&Cross(hull[hull.Count-1]-hull[hull.Count-2],p-hull[hull.Count-1])<=.000001f)hull.RemoveAt(hull.Count-1);
                hull.Add(p);
            }
            int lower=hull.Count;
            for(int i=slice.Count-2;i>=0;i--) {
                var p=slice[i];
                while(hull.Count>lower&&Cross(hull[hull.Count-1]-hull[hull.Count-2],p-hull[hull.Count-1])<=.000001f)hull.RemoveAt(hull.Count-1);
                hull.Add(p);
            }
            if(hull.Count>1)hull.RemoveAt(hull.Count-1);
        }
        public bool Update(Transform target,Mesh energy,Mesh ink,CursedEnergyAuraProfile p,float expansion=1)
        {
            bool changed=!fitted||oldThickness!=p.fieldThickness||oldWidth!=p.width||oldHeight!=p.height||oldExpansion!=expansion||oldVerticalResponse!=p.verticalSizeResponse||oldReach!=p.extraFieldReach||oldUpper!=p.upperFieldConnection||oldLower!=p.lowerFieldConnection;
            for(int i=0;i<sources.Length;i++) {
                if(sources[i]==null)continue;
                Matrix4x4 m=target.worldToLocalMatrix*sources[i].transform.localToWorldMatrix;
                for(int k=0;k<16;k++)if(Mathf.Abs(m[k]-matrices[i][k])>.00001f){changed=true;break;}
                matrices[i]=m;
            }
            if(!changed)return false;
            long start=System.Diagnostics.Stopwatch.GetTimestamp();bool first=true;Bounds bounds=default;
            for(int i=0;i<sources.Length;i++) {
                if(sources[i]==null)continue;Bounds b=sources[i].localBounds;
                for(int c=0;c<8;c++) {
                    var v=matrices[i].MultiplyPoint3x4(b.center+Vector3.Scale(b.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)));
                    corners[i][c]=v;if(first){bounds=new Bounds(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);
                }
            }
            if(first)return false;
            SourceBounds=bounds;
            // Only the space outside the original body grows. Feet and source body do not scale.
            float thickness=p.fieldThickness*p.width/.75f*expansion
                + Mathf.Max(0,expansion-p.BaseExpansion)*.22f*Mathf.Clamp01(p.extraFieldReach);
            float top=bounds.max.y+bounds.size.y*.08f*p.height/2.4f*p.VerticalExpansion(expansion);
            for(int row=0;row<Rows;row++) {
                float y=Mathf.Lerp(bounds.min.y+.001f,top,row/(float)(Rows-1));heights[row]=y;slice.Clear();
                for(int i=0;i<sources.Length;i++) {
                    if(sources[i]==null)continue;
                    for(int e=0;e<Edges.Length;e+=2) {
                        Vector3 a=corners[i][Edges[e]],b=corners[i][Edges[e+1]];
                        if(Mathf.Abs(a.y-b.y)<.000001f)continue;
                        float t=(y-a.y)/(b.y-a.y);if(t<0||t>1)continue;
                        Vector3 v=Vector3.Lerp(a,b,t);slice.Add(new Vector2(v.x,v.z));
                    }
                }
                valid[row]=slice.Count>=3;if(!valid[row])continue;MakeHull();
                Vector2 center=Vector2.zero;foreach(var h in hull)center+=h;center/=hull.Count;centers[row]=center;
                for(int side=0;side<Sides;side++) {
                    float angle=side/(float)Sides*Mathf.PI*2;Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));float hit=float.PositiveInfinity;
                    for(int j=0;j<hull.Count;j++) {
                        Vector2 a=hull[j],edge=hull[(j+1)%hull.Count]-a;float determinant=Cross(direction,edge);
                        if(Mathf.Abs(determinant)<.000001f)continue;
                        float distance=Cross(a-center,edge)/determinant,u=Cross(a-center,direction)/determinant;
                        if(distance>=0&&u>=0&&u<=1)hit=Mathf.Min(hit,distance);
                    }
                    radius[row,side]=float.IsInfinity(hit)?.01f:hit;
                }
            }
            for(int row=0;row<Rows;row++)if(!valid[row]) {
                int previous=row-1,next=row+1;while(previous>=0&&!valid[previous])previous--;while(next<Rows&&!valid[next])next++;
                int a=previous>=0?previous:Mathf.Min(next,Rows-1),b=next<Rows?next:a;
                float blend=a==b?0:(heights[row]-heights[a])/Mathf.Max(.001f,heights[b]-heights[a]);
                centers[row]=Vector2.Lerp(centers[a],centers[b],blend);
                float cap=next>=Rows?1-Mathf.SmoothStep(0,1,(heights[row]-heights[a])/Mathf.Max(.001f,top-heights[a])):1;
                for(int side=0;side<Sides;side++)radius[row,side]=Mathf.Lerp(radius[a,side],radius[b,side],blend)*cap;
            }
            for(int row=0;row<Rows;row++)for(int side=0;side<=Sides;side++) {
                int s=side%Sides;float angle=s/(float)Sides*Mathf.PI*2;
                float r=(radius[row,(s+Sides-1)%Sides]+radius[row,s]*2+radius[row,(s+1)%Sides])*.25f;
                // Round the section's planar faces, retaining the body's changing height profile.
                // The carrier must not read as the block model's second set of clothes.
                float a=Mathf.Max(.005f,radius[row,0]),b=Mathf.Max(.005f,radius[row,Sides/4]);
                float ellipse=a*b/Mathf.Sqrt(b*b*Mathf.Cos(angle)*Mathf.Cos(angle)+a*a*Mathf.Sin(angle)*Mathf.Sin(angle));
                r=Mathf.Lerp(r,ellipse,.65f);
                Vector2 center=centers[row];
                if(row>0&&row<Rows-1){r=(r*4+radius[row-1,s]+radius[row+1,s])/6;center=(center*2+centers[row-1]+centers[row+1])*.25f;}
                r+=thickness*(.72f+.10f*Mathf.Sin(angle*3+row*.31f));
                float u=row/(float)(Rows-1),upper=Mathf.Clamp01((u-.8f)/.2f),lower=Mathf.Clamp01((.1f-u)/.1f);
                float bend=Mathf.SmoothStep(0,1,upper)*Mathf.Clamp01(p.upperFieldConnection)
                    +Mathf.SmoothStep(0,1,lower)*Mathf.Clamp01(p.lowerFieldConnection);
                // Continue existing meridians inward; no disc, fan, new mesh, or sealed dome.
                // A tiny nonzero terminal ring avoids collapsed/zero-area triangles.
                r=Mathf.Lerp(r,.014f,bend);
                float y=heights[row]+Mathf.Sin(upper*Mathf.PI)*.06f*Mathf.Sin(angle*3+u*7)*p.upperFieldConnection;
                vertices[row*Stride+side]=new Vector3(center.x+Mathf.Cos(angle)*r,y,center.y+Mathf.Sin(angle)*r);
            }
            energy.vertices=vertices;energy.RecalculateNormals();energy.GetNormals(normals);
            for(int row=0;row<Rows;row++){int a=row*Stride,b=a+Sides;Vector3 n=(normals[a]+normals[b]).normalized;normals[a]=normals[b]=n;}
            energy.SetNormals(normals);ink.vertices=vertices;ink.SetNormals(normals);
            var renderBounds=new Bounds(vertices[0],Vector3.zero);foreach(var v in vertices)renderBounds.Encapsulate(v);
            renderBounds.Expand(.5f*Mathf.Max(1,expansion));energy.bounds=ink.bounds=renderBounds;
            oldThickness=p.fieldThickness;oldWidth=p.width;oldHeight=p.height;oldExpansion=expansion;oldVerticalResponse=p.verticalSizeResponse;oldReach=p.extraFieldReach;oldUpper=p.upperFieldConnection;oldLower=p.lowerFieldConnection;fitted=true;FitCount++;
            LastFitMilliseconds=(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000d/System.Diagnostics.Stopwatch.Frequency;return true;
        }
        // A field-crest location, not a new body emitter. Matches the main vertex displacement.
        public Vector3 Crest(int slot,int cycle,float time,CursedEnergyAuraProfile p,float expansion,out Vector3 normal)
        {
            int row=5+(slot*7+cycle*11)%29;float best=-100;int chosen=0;
            for(int side=0;side<Sides;side++) {
                var v=vertices[row*Stride+side];float angle=side/(float)Sides*Mathf.PI*2;
                float wave=Mathf.Sin(angle*3+v.y*5.5f-time*2.8f)+.45f*Mathf.Sin(angle*5-v.y*8+time*2);
                // Different slots cover the circumference instead of sharing the single maximum.
                float preference=wave+.5f*Mathf.Cos(angle-slot*2.399963f-cycle*.7f);
                if(preference>best){best=preference;chosen=side;}
            }
            int index=row*Stride+chosen;Vector3 at=vertices[index];normal=normals[index];
            float a=chosen/(float)Sides*Mathf.PI*2;
            float w=Mathf.Sin(a*3+at.y*5.5f-time*2.8f)+.45f*Mathf.Sin(a*5-at.y*8+time*2);
            float crest=Mathf.Pow(Mathf.Clamp01(w*.5f+.35f),4);
            float preserve=p.preserveLocalFlameShape?Mathf.Clamp01(expansion/p.BaseExpansion-1):0;
            var growth=Vector3.Lerp(new Vector3(expansion,p.VerticalExpansion(expansion),expansion),Vector3.one*p.BaseExpansion,preserve);
            at+=Vector3.Scale(normal*(p.fieldThickness*(.12f+.32f*w)+crest*.13f)*p.irregularity,growth);
            at.y+=crest*.10f*Mathf.Lerp(p.VerticalExpansion(expansion),p.BaseExpansion,preserve);
            return at;
        }
    }
}
