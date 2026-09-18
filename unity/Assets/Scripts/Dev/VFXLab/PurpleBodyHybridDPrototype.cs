using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Dev.PurpleBodyExploration
{
    /// <summary>Independent D density field. Reads common comparison values, never writes shared assets.</summary>
    public sealed class PurpleBodyHybridDPrototype : MonoBehaviour
    {
        private Material material;
        public Material PreviewMaterial => material;
        public void Configure(PurpleBodyHybridDProfile profile)
        {
            var shared=profile.commonConditions;
            var proxy=GameObject.CreatePrimitive(PrimitiveType.Cube);
            proxy.name="HybridD_DensityProxy";proxy.transform.SetParent(transform,false);
            proxy.transform.localScale=Vector3.one*(shared.bodyDiameter/.75f);
            var col=proxy.GetComponent<Collider>();col.enabled=false;Destroy(col);
            material=new Material(Resources.Load<Shader>("VFX/HollowPurpleHybridD")) { name="PurpleHybridD_Body_Runtime" };
            var renderer=proxy.GetComponent<Renderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            material.SetFloat("_CoreRatio",shared.coreRatio);material.SetFloat("_CoreEmission",shared.coreEmission);
            material.SetFloat("_Density",shared.density);material.SetFloat("_BodyEmission",shared.bodyEmission);
            material.SetColor("_NearBlack",shared.nearBlack);material.SetColor("_DeepViolet",shared.deepViolet);
            material.SetColor("_Violet",shared.violet);material.SetColor("_Magenta",shared.magenta);material.SetColor("_HotPink",shared.hotPink);
            material.SetVector("_HybridShape",new Vector4(profile.tearingAmplitude,profile.internalDepth,profile.streamEnergy,profile.edgeEnergy));
            material.SetVector("_HybridFlow",new Vector4(profile.implosionSpeed,profile.surfaceSpeed,0,0));
            Render(1.5f);
        }
        public void Render(float time) { if(material!=null) material.SetFloat("_PhaseTime",time); }
        private void OnDestroy() { if(material!=null) Destroy(material); }
    }
}
