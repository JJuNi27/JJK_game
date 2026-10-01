using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    // Separate, root-owned exploration. Does not write to body materials or shared settings.
    public sealed class PurpleTempestLayersRuntime:MonoBehaviour
    {
        const int Gusts=8,Steps=64,Across=7,Bright=12,Dark=8,Nodes=65;
        PurpleTempestLayersProfile profile;
        float radius;
        Mesh windMesh;
        Material windMaterial,brightMaterial,darkMaterial;
        Vector3[] windVertices;Color[] windColors;
        LineRenderer[] arcs;Vector3[][] positions;
        static float Hash(float x)=>Mathf.Repeat(Mathf.Sin(x*127.1f+31.7f)*43758.5453f,1);
        static float Noise(float x,float seed)
        {
            float a=Mathf.Floor(x),f=Mathf.Repeat(x,1);f=f*f*(3-2*f);
            return Mathf.Lerp(Hash(a*17+seed),Hash((a+1)*17+seed),f)*2-1;
        }
        public void Configure(PurpleTempestLayersProfile settings,float bodyRadius)
        {
            profile=settings;radius=bodyRadius;
            windMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTempestWind")){name="PurpleTempest_Wind_Runtime"};
            brightMaterial=new Material(Resources.Load<Shader>("VFX/PurpleTempestDischarge")){name="PurpleTempest_Bright_Runtime"};
            darkMaterial=new Material(brightMaterial){name="PurpleTempest_Dark_Runtime"};
            darkMaterial.SetFloat("_Dark",1);
            windMesh=new Mesh{name="PurpleTempest_Wind_Runtime"};windMesh.MarkDynamic();
            var wind=new GameObject("Tempest_WhiteGreyPressure");wind.transform.SetParent(transform,false);
            wind.AddComponent<MeshFilter>().sharedMesh=windMesh;
            var renderer=wind.AddComponent<MeshRenderer>();renderer.sharedMaterial=windMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            windVertices=new Vector3[Gusts*(Steps+1)*Across];windColors=new Color[windVertices.Length];
            var uv=new Vector2[windVertices.Length];var tris=new int[Gusts*Steps*(Across-1)*6];int index=0;
            for(int lane=0;lane<Gusts;lane++)for(int n=0;n<=Steps;n++)for(int v=0;v<Across;v++)
            {
                int at=(lane*(Steps+1)+n)*Across+v;uv[at]=new Vector2(n/(float)Steps,v/(float)(Across-1));
                if(n<Steps && v<Across-1){tris[index++]=at;tris[index++]=at+Across;tris[index++]=at+1;tris[index++]=at+1;tris[index++]=at+Across;tris[index++]=at+Across+1;}
            }
            windMesh.vertices=windVertices;windMesh.uv=uv;windMesh.triangles=tris;
            arcs=new LineRenderer[Bright+Dark];positions=new Vector3[arcs.Length][];
            for(int i=0;i<arcs.Length;i++)
            {
                var go=new GameObject(i<Bright?"Tempest_WhiteVioletDischarge":"Tempest_BlackVioletCurrent");go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.sharedMaterial=i<Bright?brightMaterial:darkMaterial;
                line.useWorldSpace=false;line.positionCount=Nodes;line.alignment=LineAlignment.View;line.textureMode=LineTextureMode.Stretch;
                line.numCornerVertices=0;line.numCapVertices=0;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
                line.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(.1f,.8f),new Keyframe(.4f,1),new Keyframe(.85f,.65f),new Keyframe(1,0));
                arcs[i]=line;positions[i]=new Vector3[Nodes];
            }
        }
        public void Sample(float time,Vector3 velocity)
        {
            float t=time*profile.stormSpeed;bool travel=velocity.sqrMagnitude>.01f;
            Vector3 back=travel?-transform.InverseTransformDirection(velocity.normalized):Vector3.zero;
            float scale=transform.lossyScale.x;
            Vector4 sphere=new Vector4(transform.position.x,transform.position.y,transform.position.z,radius*scale*.87f);
            windMaterial.SetVector("_Sphere",sphere);windMaterial.SetFloat("_PhaseTime",t);
            brightMaterial.SetVector("_Sphere",sphere);brightMaterial.SetFloat("_PhaseTime",t);
            darkMaterial.SetVector("_Sphere",sphere);darkMaterial.SetFloat("_PhaseTime",t);
            windMaterial.SetFloat("_Opacity",profile.windOpacity);
            brightMaterial.SetFloat("_Emission",profile.brightEmission);darkMaterial.SetFloat("_Emission",profile.darkEmission);
            for(int lane=0;lane<Gusts;lane++)
            {
                float c=time*profile.windSpeed*(1.1f+Hash(lane+4)*.65f)+lane*.319f,age=Mathf.Repeat(c,1),epoch=Mathf.Floor(c);
                float seed=lane*51+epoch*7;
                float strength=Mathf.Pow(Mathf.Sin(age*Mathf.PI),.55f);
                for(int n=0;n<=Steps;n++)
                {
                    float u=n/(float)Steps;
                    float theta=time*profile.windSpeed*(6.2f+Hash(lane+9)*2.3f)+lane*2.39996f+u*(1.6f+Hash(seed)*1.3f);
                    float r=(1.0f+age*(profile.windReach-1)+u*.25f+Mathf.Sin(theta*2.7f+seed)*.1f)*radius;
                    float y=radius*(-.9f+(lane%3)*.18f+Mathf.Sin(theta*1.7f+lane)*.22f+u*u*.17f);
                    for(int v=0;v<Across;v++)
                    {
                        float s=v/(float)(Across-1)-.5f,rr=r+s*radius*profile.windWidth*(.5f+.5f*Mathf.Sin(u*Mathf.PI));
                        int at=(lane*(Steps+1)+n)*Across+v;
                        windVertices[at]=new Vector3(Mathf.Cos(theta)*rr,y+s*radius*.12f,Mathf.Sin(theta)*rr)+back*radius*age*.8f;
                        windColors[at]=new Color(Hash(lane*7),0,0,strength);
                    }
                }
            }
            windMesh.vertices=windVertices;windMesh.colors=windColors;windMesh.RecalculateBounds();
            for(int i=0;i<arcs.Length;i++)
            {
                bool dark=i>=Bright;int lane=dark?i-Bright:i;
                float c=t*(dark?3.1f:5.2f)*( .8f+Hash(i+14)*.5f)+i*.387f,age=Mathf.Repeat(c,1),epoch=Mathf.Floor(c);
                float seed=i*127+epoch*43,gate=dark?.96f:.87f;
                float pulse=age<gate?Mathf.Min(1,age/.08f)*Mathf.Pow(1-age/gate,.25f):0;
                arcs[i].enabled=pulse>.02f;if(!arcs[i].enabled)continue;
                float angle=t*(dark?3.6f:5.3f)+lane*2.39996f+Hash(seed+2)*1.6f;
                float span=(dark?1.0f:1.25f)+Hash(seed+6)*(dark?1.15f:1.65f);
                var plane=Quaternion.Euler(25+Hash(seed+10)*130,Hash(i+3)*360,Hash(i+5)*180);
                float reach=dark?1.15f+Hash(seed+7)*.48f:1.0f+Hash(seed+7)*(profile.dischargeReach-1);
                float evolve=t*(dark?11:20);
                for(int n=0;n<Nodes;n++)
                {
                    float u=n/(float)(Nodes-1),a=angle+u*span;
                    float wriggle=Noise(u*13+evolve,seed+41)*(dark?.095f:.055f)+Noise(u*31-evolve,seed+13)*.03f;
                    float r=radius*(reach+wriggle+Mathf.Sin(u*8+t*13+seed)*.09f);
                    Vector3 p=new Vector3(Mathf.Cos(a)*r,Mathf.Sin(u*6+evolve+seed)*radius*.11f,Mathf.Sin(a)*r);
                    positions[i][n]=plane*p+back*radius*.15f;
                }
                arcs[i].SetPositions(positions[i]);arcs[i].widthMultiplier=radius*scale*(dark?profile.darkWidth:profile.brightWidth)*(.7f+Hash(seed+30)*.7f);
                var color=new Color(Hash(seed),0,0,pulse);arcs[i].startColor=color;arcs[i].endColor=color;
            }
        }
        private void OnDestroy(){if(windMesh!=null)Destroy(windMesh);if(windMaterial!=null)Destroy(windMaterial);if(brightMaterial!=null)Destroy(brightMaterial);if(darkMaterial!=null)Destroy(darkMaterial);}
    }
}
