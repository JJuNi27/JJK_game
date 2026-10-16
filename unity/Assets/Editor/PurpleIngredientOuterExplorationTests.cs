using System.Collections;
using System.Linq;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.EditorTools
{
    public sealed class PurpleIngredientOuterExplorationTests
    {
        private static int OwnedMaterials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("PurpleIngredient"));

        [TearDown] public void Cleanup()
        {
            var p=PurpleIngredientOuterExplorationProfile.Current;if(p!=null)p.candidateEnabled=false;
            if(PurpleIngredientFinal2Profile.Current!=null)PurpleIngredientFinal2Profile.Current.candidateEnabled=false;
            if(PurpleIngredientFinalProfile.Current!=null)PurpleIngredientFinalProfile.Current.candidateEnabled=false;
            if(PurpleIngredientReboot2Profile.Current!=null)PurpleIngredientReboot2Profile.Current.candidateEnabled=false;
            foreach(var host in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(go=>go.name=="OuterExplorationTest"))Object.DestroyImmediate(host);
            // All owned objects are destroyed synchronously in EditMode.
        }

        [Test] public void OuterExplorationEvolvesInDepthAndCleansUp()
        {
            var reboot=PurpleIngredientReboot2Profile.Current;var final=PurpleIngredientFinalProfile.Current;var final2=PurpleIngredientFinal2Profile.Current;var profile=PurpleIngredientOuterExplorationProfile.Current;
            Assert.That(profile,Is.Not.Null);Assert.That(Resources.Load<Shader>("VFX/PurpleIngredientOuterExploration"),Is.Not.Null);
            reboot.candidateEnabled=true;final.candidateEnabled=true;final2.candidateEnabled=true;profile.candidateEnabled=true;
            int materialBaseline=OwnedMaterials();
            foreach(bool red in new[]{false,true})
            {
                var host=new GameObject("OuterExplorationTest");var ingredient=host.AddComponent<PurpleFusionIngredient>();ingredient.Configure(red);
                var volume=host.GetComponent<PurpleIngredientOuterExplorationVolume>();Assert.That(volume,Is.Not.Null);
                var filters=host.GetComponentsInChildren<MeshFilter>().Where(f=>f.sharedMesh!=null&&f.sharedMesh.name.StartsWith("PurpleIngredientOuterExploration_")).ToArray();
                Assert.That(filters.Length,Is.EqualTo(4));
                var first=filters.Select(f=>f.sharedMesh.vertices.ToArray()).ToArray();ingredient.Render(.14f,0,Vector3.right*3);var second=filters.Select(f=>f.sharedMesh.vertices.ToArray()).ToArray();ingredient.Render(.23f,0,Vector3.right*3);var third=filters.Select(f=>f.sharedMesh.vertices.ToArray()).ToArray();
                float changeA=0,changeB=0;float depthMax=0;
                for(int g=0;g<filters.Length;g++){changeA+=first[g].Zip(second[g],(x,y)=>Vector3.Distance(x,y)).Average();changeB+=second[g].Zip(third[g],(x,y)=>Vector3.Distance(x,y)).Average();depthMax=Mathf.Max(depthMax,filters[g].sharedMesh.bounds.size.z);}
                Assert.That(changeA,Is.GreaterThan(.01f),"Outer shape must evolve");
                Assert.That(changeB,Is.GreaterThan(.01f),"Outer shape must keep evolving");
                Assert.That(depthMax,Is.GreaterThan(.25f),"Outer event must have depth");
                Object.DestroyImmediate(host);EditorUtility.UnloadUnusedAssetsImmediate();Assert.That(OwnedMaterials(),Is.LessThanOrEqualTo(materialBaseline));
            }
            foreach(var message in ShaderUtil.GetShaderMessages(Shader.Find("JJKGame/Candidate/Purple Ingredient Outer Exploration")))Assert.That(message.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),message.message);
        }
    }
}
