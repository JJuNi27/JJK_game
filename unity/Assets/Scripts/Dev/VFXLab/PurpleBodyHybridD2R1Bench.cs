using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleBodyHybridD2R1Bench : MonoBehaviour
    {
        private static readonly Vector3 Offset=new Vector3(768,0,0);
        public PurpleBodyExplorationBench Common { get; private set; }
        public PurpleBodyHybridD2Prototype Frozen { get; private set; }
        public PurpleBodyHybridD2R1Prototype Hybrid { get; private set; }
        public bool ShowR1 { get; private set; }=true;
        public bool Playing { get; set; }
        public void Configure(PurpleBodyHybridD2R1Profile profile,Camera camera)
        {
            if(Common!=null)return;
            Common=gameObject.AddComponent<PurpleBodyExplorationBench>();Common.Configure(profile.frozenD2.preservedD.commonConditions,camera);
            transform.position+=Offset;
            for(int i=0;i<3;i++)Common.Candidate((PurpleBodyVariant)i).gameObject.SetActive(false);
            var frozen=new GameObject("Frozen_D2_Comparison");frozen.transform.SetParent(transform,false);frozen.transform.position=Common.Centre+Offset;
            Frozen=frozen.AddComponent<PurpleBodyHybridD2Prototype>();Frozen.Configure(profile.frozenD2);
            var refined=new GameObject("CodexPurpleBody_D2R1");refined.transform.SetParent(transform,false);refined.transform.position=Common.Centre+Offset;
            Hybrid=refined.AddComponent<PurpleBodyHybridD2R1Prototype>();Hybrid.Configure(profile);
            Select(true);Sample(1.5f,PurpleBenchmarkView.Front);
        }
        public void Select(bool refined){ShowR1=refined;Frozen.gameObject.SetActive(!refined);Hybrid.gameObject.SetActive(refined);}
        public void Sample(float time,PurpleBenchmarkView view)
        {
            Common.Playing=false;Common.Sample(time,view);Common.BenchmarkCamera.transform.position+=Offset;
            Frozen.Render(Common.Clock);Hybrid.Render(Common.Clock);
        }
        private void Update(){if(!Playing || Common==null)return;Sample(Common.Clock+Time.unscaledDeltaTime,Common.View);if(Common.Clock>=3)Playing=false;}
    }
}
