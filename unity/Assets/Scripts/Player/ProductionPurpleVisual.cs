using UnityEngine;
namespace JJKGame.Player
{
    public sealed class ProductionPurpleVisual : MonoBehaviour
    {
        private ProductionPurpleVisualProfile profile;
        private ProductionPurpleBodyRuntime body;
        private ProductionPurpleOuterRuntime outer;
        private Vector3 travelVelocity;
        public float MaxResidualDistance=>outer!=null?outer.MaxResidualDistance:0;
        public void Configure(Vector3 velocity)
        {
            profile=ProductionPurpleVisualProfile.Current;travelVelocity=velocity;
            var b=new GameObject("PurpleProductionBody_R3");b.transform.SetParent(transform,false);
            body=b.AddComponent<ProductionPurpleBodyRuntime>();body.Configure(profile.body,GojoPolishSettings.PurpleShellDiameter);
            var o=new GameObject("PurpleProductionOuter_R2");o.transform.SetParent(transform,false);
            outer=o.AddComponent<ProductionPurpleOuterRuntime>();outer.Configure(profile.outer,GojoPolishSettings.PurpleShellDiameter);
        }
        public void Render(float clock,float releaseAge,bool travel)
        {
            body.gameObject.SetActive(profile.bodyEnabled);outer.gameObject.SetActive(profile.outerEnabled);
            if(profile.bodyEnabled)body.Render(clock);
            if(profile.outerEnabled)outer.Sample(clock,travel?travelVelocity:Vector3.zero,travel?releaseAge:0);
        }
    }
}
