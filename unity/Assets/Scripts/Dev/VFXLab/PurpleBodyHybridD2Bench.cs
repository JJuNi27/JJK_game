using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleBodyHybridD2Bench : MonoBehaviour
    {
        private static readonly Vector3 Offset=new Vector3(256,0,0);
        public PurpleBodyHybridDBench PreservedBench { get; private set; }
        public PurpleBodyHybridD2Prototype Hybrid { get; private set; }
        public PurpleBodyExplorationBench Common=>PreservedBench.CommonBench;
        public bool ShowD2 { get; private set; }=true;
        public bool Playing { get; set; }
        public void Configure(PurpleBodyHybridD2Profile profile,Camera labCamera)
        {
            if(PreservedBench!=null)return;
            PreservedBench=gameObject.AddComponent<PurpleBodyHybridDBench>();PreservedBench.Configure(profile.preservedD,labCamera);
            transform.position+=Offset;
            var go=new GameObject("CodexPurpleBodyPrototype_D2");go.transform.SetParent(transform,false);go.transform.position=PreservedBench.Centre+Offset;
            Hybrid=go.AddComponent<PurpleBodyHybridD2Prototype>();Hybrid.Configure(profile);Select(true);Sample(1.5f,PurpleBenchmarkView.Front);
        }
        public void Select(bool d2){ShowD2=d2;PreservedBench.Select(3);PreservedBench.Hybrid.gameObject.SetActive(!d2);Hybrid.gameObject.SetActive(d2);}
        public void Sample(float time,PurpleBenchmarkView view)
        {PreservedBench.Playing=false;PreservedBench.Sample(time,view);Common.BenchmarkCamera.transform.position+=Offset;Hybrid.Render(Common.Clock);}
        private void Update(){if(!Playing || PreservedBench==null)return;Sample(Common.Clock+Time.unscaledDeltaTime,Common.View);if(Common.Clock>=3)Playing=false;}
    }
}
