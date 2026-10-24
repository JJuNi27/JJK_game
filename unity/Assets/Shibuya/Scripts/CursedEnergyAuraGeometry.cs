using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Shibuya
{
    // Closed, curved flame volumes. No camera-facing or cardinal-direction walls.
    internal static class CursedEnergyAuraGeometry
    {
        private static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+37.7f)*43758.5453f,1);
        internal const int TipCount=8;
        internal static float TipSeed(int slot)=>Hash(slot+93);
        internal static float TipPhase(int slot)=>Hash(slot*7+39);
        public static Mesh BuildTips()
        {
            const int rings=9,sides=7;
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var seeds=new List<Vector2>();var anchors=new List<Vector2>();var tris=new List<int>();
            for(int slot=0;slot<TipCount;slot++) {
                float seed=TipSeed(slot);int first=vertices.Count;
                for(int r=0;r<rings;r++) {
                    float t=r/(float)(rings-1),width=Mathf.Sin(t*Mathf.PI)*Mathf.Pow(1-t,.7f)*.045f;
                    Vector3 center=new Vector3(Mathf.Sin(t*3.8f+seed*4)*t*.06f,t*(.20f+seed*.12f),Mathf.Sin(t*5)*t*.04f);
                    for(int side=0;side<sides;side++) {
                        float a=side/(float)sides*Mathf.PI*2;
                        vertices.Add(center+new Vector3(Mathf.Cos(a)*width,0,Mathf.Sin(a)*width*.8f));
                        uv.Add(new Vector2(side/(float)sides,t));seeds.Add(new Vector2(seed,TipPhase(slot)));anchors.Add(new Vector2(slot,3));
                        if(r==rings-1)continue;
                        int at=first+r*sides+side,next=first+r*sides+(side+1)%sides;
                        tris.Add(at);tris.Add(next+sides);tris.Add(next);tris.Add(at);tris.Add(at+sides);tris.Add(next+sides);
                    }
                }
            }
            var mesh=new Mesh{name="CursedAura_OwnedVolume_Tips"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetUVs(1,seeds);mesh.SetUVs(2,anchors);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
            mesh.bounds=new Bounds(Vector3.up,new Vector3(8,7,8));return mesh;
        }
        public static Mesh Build(int role,IReadOnlyList<CursedAuraSurfacePoint> points,int mainCount)
        {
            bool wisp=role==2;
            // Cyan mass and ink hull share the exact seeds/path. Ink is a torn boundary,
            // not a separate swarm of black tentacles. Only the small wisps use a new path.
            int family=wisp?2:0,count=wisp?points.Count-mainCount:mainCount,rings=wisp?13:23,sides=wisp?10:14;
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var seeds=new List<Vector2>();var anchors=new List<Vector2>();var tris=new List<int>();
            for(int slot=0;slot<count;slot++) {
                float seed=Hash(slot+family*31),angle=slot*2.399963f+family*.77f;
                int pointIndex=wisp?mainCount+slot:slot;
                Vector3 radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                Vector3 tangent=new Vector3(-radial.z,0,radial.x);
                // Main roots overlap inside actual chest/back/waist/shoulder regions.
                // The existing wisp layer supplies the small arm/body tongues.
                var region=points[pointIndex].Region;
                float regionScale=region==CursedAuraBodyRegion.Head?.50f:region==CursedAuraBodyRegion.Arm?.85f:1;
                float length=(wisp?Mathf.Lerp(.40f,.85f,seed):Mathf.Lerp(.95f,1.45f,seed))*regionScale;
                float width=(wisp?Mathf.Lerp(.035f,.075f,seed):Mathf.Lerp(.16f,.23f,seed))*Mathf.Sqrt(regionScale);
                int first=vertices.Count;
                for(int r=0;r<rings;r++) {
                    float t=r/(float)(rings-1);
                    float hook=Mathf.Clamp01((t-.42f)/.58f),curl=hook*Mathf.PI*(1.1f+seed*.5f);
                    Vector3 center=radial*(Mathf.Sin(t*Mathf.PI*.8f)*.18f)
                        +Vector3.up*(t*length-hook*hook*length*.12f)
                        +tangent*(Mathf.Sin(t*Mathf.PI)*(seed-.5f)*.22f+Mathf.Sin(curl)*hook*.18f);
                    // A broad submerged root joins the body silhouette; the thin tail tears away.
                    float taper=wisp?Mathf.Pow(Mathf.Max(0,Mathf.Sin(Mathf.PI*t)),.62f)*Mathf.Pow(1-t,1.45f)*2.1f
                        :Mathf.Pow(1-t,1.35f)*(.72f+.65f*Mathf.Sin(Mathf.PI*t));
                    for(int side=0;side<sides;side++) {
                        float a=side/(float)sides*Mathf.PI*2;
                        Vector3 crossTangent=tangent*Mathf.Cos(t*2.2f)+radial*Mathf.Sin(t*2.2f);
                        Vector3 crossDepth=Vector3.Cross(Vector3.up,crossTangent);
                        vertices.Add(center+(crossTangent*Mathf.Cos(a)+crossDepth*Mathf.Sin(a)*.86f)*width*taper);
                        // Ink MUST share lifecycle phase as well as shape with the energy.
                        uv.Add(new Vector2(side/(float)sides,t));seeds.Add(new Vector2(seed,Hash(slot*7+family*13)));
                        anchors.Add(new Vector2(pointIndex,wisp?1:0));
                        if(r<rings-1) {
                            int at=first+r*sides+side,next=first+r*sides+(side+1)%sides;
                            tris.Add(at);tris.Add(next);tris.Add(next+sides);
                            tris.Add(at);tris.Add(next+sides);tris.Add(at+sides);
                        }
                    }
                }
            }
            var mesh=new Mesh{name="CursedAura_OwnedVolume_"+role};
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetUVs(1,seeds);mesh.SetUVs(2,anchors);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
            mesh.bounds=new Bounds(new Vector3(0,1.5f,0),new Vector3(4,5,4));return mesh;
        }
    }
}
