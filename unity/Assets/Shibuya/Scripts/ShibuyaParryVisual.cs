using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Shibuya
{
    [DisallowMultipleComponent]
    public sealed class ShibuyaParryVisual : MonoBehaviour
    {
        private ShibuyaFighter owner;
        private GameObject effect;
        private Mesh mesh;
        private Material material;
        private MeshRenderer surface;
        private MaterialPropertyBlock properties;
        private float began=-100;
        public bool IsVisible=>surface!=null&&surface.enabled;
        public MeshRenderer SurfaceRenderer=>surface;
        private void Awake()
        {
            owner=GetComponent<ShibuyaFighter>();
            effect=new GameObject("ParryPresentation");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(effect,gameObject.scene);
            mesh=new Mesh{name="Shibuya Parry (runtime)"};
            mesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(-1,1,0),new Vector3(1,1,0),new Vector3(1,-1,0)};
            mesh.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();
            effect.AddComponent<MeshFilter>().sharedMesh=mesh;surface=effect.AddComponent<MeshRenderer>();
            material=new Material(owner.ParrySettings.effectShader){name="Shibuya Parry (runtime)"};surface.sharedMaterial=material;
            surface.shadowCastingMode=ShadowCastingMode.Off;surface.receiveShadows=false;surface.enabled=false;properties=new MaterialPropertyBlock();
        }
        private void OnEnable(){owner.ParrySucceeded+=Success;}
        private void OnDisable(){if(owner!=null)owner.ParrySucceeded-=Success;if(surface!=null)surface.enabled=false;began=-100;}
        private void Success(ShibuyaFighter f)
        {
            began=Time.time;effect.transform.SetPositionAndRotation(f.ParryContactWorld,Quaternion.LookRotation(f.ParryContactNormal));
        }
        private void LateUpdate()
        {
            var p=owner.ParrySettings;
            surface.enabled=p!=null&&owner.isActiveAndEnabled&&!owner.Health.IsDead&&owner.State!=ShibuyaFighterState.Finished&&Time.time-began<p.effectDuration;
            if(!surface.enabled)return;
            effect.transform.localScale=Vector3.one*p.effectSize;
            properties.SetColor("_Tint",p.effectColor);properties.SetFloat("_Progress",Mathf.Clamp01((Time.time-began)/Mathf.Max(.01f,p.effectDuration)));
            properties.SetFloat("_Intensity",p.effectIntensity);surface.SetPropertyBlock(properties);
        }
        private void OnDestroy(){if(effect!=null)Destroy(effect);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
