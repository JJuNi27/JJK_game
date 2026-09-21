using UnityEngine;
using UnityEngine.Rendering;
namespace JJKGame.Player
{
    /// <summary>Owned, world-local contact frame. No camera lease or post-process mutation.</summary>
    public sealed class PurpleIngredientCollisionAccent : MonoBehaviour
    {
        private GameObject volume;
        private Material material;
        private float radius,duration,strength;
        public float ContactTime {get;private set;}
        public bool IsVisible=>volume!=null && volume.activeSelf;
        public static float FindContactNormalized(float separation,float scale,float arc)
        {
            // Find the first nominal surface contact of the two unchanged sphere sizes.
            float lo=0,hi=.65f;
            for(int i=0;i<24;i++)
            {
                float n=(lo+hi)*.5f,t=Mathf.SmoothStep(0,1,n);
                float x=Mathf.Lerp(separation,.03f,t),y=Mathf.Sin(t*Mathf.PI)*arc;
                float r=scale*(1+Mathf.Sin(t*Mathf.PI)*.12f)*1.025f;
                if(Mathf.Sqrt(x*x+y*y)>r)lo=n;else hi=n;
            }
            return (lo+hi)*.5f;
        }
        public void Configure(float at,float worldRadius,PurpleIngredientReboot2Profile profile)
        {
            ContactTime=at;radius=worldRadius;duration=Mathf.Clamp(profile.collisionFrames,1,2)/60f;strength=profile.collisionContrast;
            volume=GameObject.CreatePrimitive(PrimitiveType.Cube);volume.name="IngredientContactContrast";volume.transform.SetParent(transform,false);volume.transform.localScale=Vector3.one*radius*4.2f;
            var collider=volume.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
            material=new Material(Resources.Load<Shader>(PurpleIngredientFinalProfile.Current!=null && PurpleIngredientFinalProfile.Current.candidateEnabled?(PurpleIngredientFinal2Profile.Current!=null && PurpleIngredientFinal2Profile.Current.candidateEnabled?"VFX/PurpleIngredientCollisionTear":"VFX/PurpleIngredientCollisionFinal"):"VFX/PurpleIngredientCollision")){name="PurpleIngredientCollision_Runtime"};
            var renderer=volume.GetComponent<Renderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;volume.SetActive(false);
        }
        public void Sample(float elapsed)
        {
            float age=elapsed-ContactTime;bool active=age>=0 && age<duration;volume.SetActive(active);
            if(!active)return;
            material.SetVector("_Centre",new Vector4(transform.position.x,transform.position.y,transform.position.z,radius));
            material.SetFloat("_Stage",age*60);material.SetFloat("_Strength",strength);
        }
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
