using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public sealed class PurpleOuterR2Bench : MonoBehaviour
    {
        private static readonly Vector3 Offset=new Vector3(1792,0,0);
        public PurpleBodyExplorationBench Common {get;private set;}
        public PurpleBodyHybridD2R3Prototype Body {get;private set;}
        public PurpleOuterR2Runtime Outer {get;private set;}
        public PurpleOuterR1Runtime FrozenOuter {get;private set;}
        public int Selected {get;private set;}=2;
        public bool ShowOuter {get;private set;}=true;
        public bool Playing {get;set;}
        public void Configure(PurpleOuterR2Profile profile,Camera camera)
        {
            var conditions=profile.frozenR3.frozenR2.frozenR1.frozenD2.preservedD.commonConditions;
            Common=gameObject.AddComponent<PurpleBodyExplorationBench>();Common.Configure(conditions,camera);
            transform.position+=Offset;
            for(int i=0;i<3;i++)Common.Candidate((PurpleBodyVariant)i).gameObject.SetActive(false);
            var body=new GameObject("Frozen_R3_Body");body.transform.SetParent(transform,false);body.transform.position=Common.Centre+Offset;
            Body=body.AddComponent<PurpleBodyHybridD2R3Prototype>();Body.Configure(profile.frozenR3);
            var outer=new GameObject("OuterR2_Additive_Exploration");outer.transform.SetParent(transform,false);outer.transform.position=body.transform.position;
            Outer=outer.AddComponent<PurpleOuterR2Runtime>();Outer.Configure(profile,conditions.bodyDiameter);
            var previous=new GameObject("Frozen_OuterR1_Comparison");previous.transform.SetParent(transform,false);previous.transform.position=body.transform.position;
            FrozenOuter=previous.AddComponent<PurpleOuterR1Runtime>();FrozenOuter.Configure(Resources.Load<PurpleOuterR1Profile>("VFX/PurpleOuterR1Profile"),conditions.bodyDiameter);
            SelectCandidate(2);
            Sample(1.5f,PurpleBenchmarkView.Front);
        }
        public void Select(bool enabled)=>SelectCandidate(enabled?2:0);
        public void SelectCandidate(int candidate){Selected=Mathf.Clamp(candidate,0,2);ShowOuter=Selected==2;Outer.gameObject.SetActive(ShowOuter);FrozenOuter.gameObject.SetActive(Selected==1);}
        public void Sample(float time,PurpleBenchmarkView view)
        {
            Common.Playing=false;Common.Sample(time,view);Common.BenchmarkCamera.transform.position+=Offset;
            Body.Render(Common.Clock);Outer.Sample(Common.Clock);FrozenOuter.Sample(Common.Clock);
        }
        private void Update(){if(!Playing || Common==null)return;Sample(Common.Clock+Time.unscaledDeltaTime,Common.View);if(Common.Clock>=3)Playing=false;}
    }
}
