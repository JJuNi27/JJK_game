using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    /// <summary>Short, owned post-fusion accent. No body, camera, gameplay or timing writes.</summary>
    public sealed class PurpleFusionBirthAccent : MonoBehaviour
    {
        private Material flashMaterial,fragmentMaterial;
        private GameObject shell;
        private readonly LineRenderer[] fragments=new LineRenderer[6];
        private float radius,duration,strength;
        public void Configure(float worldRadius,PurpleFinalPolishProfile profile)
        {
            radius=worldRadius;duration=profile.birthDuration;strength=profile.birthStrength;
            shell=GameObject.CreatePrimitive(PrimitiveType.Cube);shell.name="LocalizedBirthFlashAndPressure";shell.transform.SetParent(transform,false);
            shell.transform.localScale=Vector3.one*radius*4.6f;
            var collider=shell.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            flashMaterial=new Material(Resources.Load<Shader>("VFX/PurpleFusionBirth")){name="PurplePolish_Birth_Runtime"};
            var renderer=shell.GetComponent<Renderer>();renderer.sharedMaterial=flashMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            fragmentMaterial=new Material(Resources.Load<Shader>("VFX/HollowPurpleFilament")){name="PurplePolish_BirthFragments_Runtime"};
            for(int i=0;i<fragments.Length;i++)
            {
                var go=new GameObject("BirthElectricFragment_"+i);go.transform.SetParent(transform,false);
                var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=3;line.sharedMaterial=fragmentMaterial;
                line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.numCapVertices=0;fragments[i]=line;
            }
            Sample(-1);
        }
        public void Sample(float age)
        {
            bool active=age>=-.055f && age<duration && strength>0;
            shell.SetActive(active);
            if(active)
            {
                flashMaterial.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius));
                flashMaterial.SetVector("_Birth",new Vector4(age,duration,strength,0));
            }
            for(int i=0;i<fragments.Length;i++)
            {
                var line=fragments[i];line.enabled=active && age>=0;if(!line.enabled)continue;
                float t=Mathf.Clamp01(age/duration),fade=Mathf.Clamp01(t/.07f)*Mathf.Pow(1-t,1.4f);
                Vector3 d=Quaternion.Euler(i*57+23,i*137+11,i*43)*Vector3.up;
                Vector3 side=Vector3.Cross(d,Vector3.forward).normalized;
                float r=radius*(1.02f+t*(1.1f+i%3*.12f)),length=radius*(.22f+ .17f*Mathf.Sin(t*Mathf.PI));
                line.SetPosition(0,d*r);line.SetPosition(1,d*(r-length*.45f)+side*radius*.075f);line.SetPosition(2,d*(r-length));
                line.startWidth=.11f*(1-t);line.endWidth=.012f;
                line.startColor=new Color(3.2f,1.4f,2.9f,fade*strength);line.endColor=new Color(2.5f,.02f,1.1f,fade*.15f*strength);
            }
        }
        private void OnDestroy(){if(flashMaterial!=null)Destroy(flashMaterial);if(fragmentMaterial!=null)Destroy(fragmentMaterial);}
    }
}
