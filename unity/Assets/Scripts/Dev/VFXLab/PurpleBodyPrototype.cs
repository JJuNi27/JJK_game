using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Dev.PurpleBodyExploration
{
    public enum PurpleBodyVariant { DenseDarkMass, BrokenTurbulentSphere, DeepImplosionMass }

    /// <summary>One proxy cube bounds a procedural density field; no particles, lines or gameplay.</summary>
    public sealed class PurpleBodyPrototype : MonoBehaviour
    {
        private Material material;
        public PurpleBodyVariant Variant { get; private set; }
        public float SampleTime { get; private set; }
        public Material PreviewMaterial => material;
        public void Configure(PurpleBodyExplorationProfile settings,PurpleBodyVariant variant)
        {
            Variant=variant;
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);
            proxy.name="BoundedDensityField"; proxy.transform.SetParent(transform,false);
            proxy.transform.localScale=Vector3.one*(settings.bodyDiameter/.75f);
            var collider=proxy.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
            material=new Material(Resources.Load<Shader>("VFX/HollowPurpleBodyExploration")) { name="PurpleBodyExplore_"+variant+"_Runtime" };
            var renderer=proxy.GetComponent<Renderer>(); renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            material.SetFloat("_Variant",(int)variant);
            material.SetFloat("_CoreRatio",settings.coreRatio);
            material.SetFloat("_Density",settings.density);
            material.SetFloat("_BodyEmission",settings.bodyEmission);
            material.SetFloat("_CoreEmission",settings.coreEmission);
            material.SetColor("_NearBlack",settings.nearBlack);
            material.SetColor("_DeepViolet",settings.deepViolet);
            material.SetColor("_Magenta",settings.magenta);
            material.SetColor("_Violet",settings.violet);
            material.SetColor("_HotPink",settings.hotPink);
            material.SetFloat("_Breakup",variant==PurpleBodyVariant.BrokenTurbulentSphere?settings.brokenSilhouette:
                variant==PurpleBodyVariant.DeepImplosionMass?settings.brokenSilhouette*.25f:0f);
            material.SetFloat("_Depth",settings.depthStrength);
            material.SetVector("_FlowRates",new Vector4(settings.inwardSpeed,settings.surfaceSpeed,settings.outwardSpeed,0));
            Render(1.5f);
        }
        public void Render(float time)
        {
            SampleTime=time;
            if(material!=null) material.SetFloat("_PhaseTime",time);
        }
        private void OnDestroy() { if(material!=null) Destroy(material); }
    }
}
