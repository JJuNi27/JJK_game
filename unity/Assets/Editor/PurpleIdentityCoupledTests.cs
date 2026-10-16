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
    public sealed class PurpleIdentityCoupledTests
    {
        [Test] public void SavedOffShaderAndKoreanInspector()
        {
            var p=PurpleIdentityCoupledProfile.Current;
            Assert.That(p,Is.Not.Null);Assert.That(p.candidateEnabled,Is.False);
            var shader=Resources.Load<Shader>("VFX/PurpleIdentityCoupled");
            Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            var corona=Resources.Load<Shader>("VFX/PurpleIdentityCoupledCorona");
            Assert.That(corona,Is.Not.Null);Assert.That(corona.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(corona).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            var editor=Editor.CreateEditor(p);
            try { Assert.That(editor,Is.TypeOf<PurpleIdentityCoupledProfileEditor>()); }
            finally { Object.DestroyImmediate(editor); }
        }
        private static int OwnedMaterials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name=="PurpleIdentityCoupled_Runtime" || m.name.StartsWith("PurpleProduction_"));
        private static int OwnedMeshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleProduction_"));
        [UnityTest] public IEnumerator BodyOnlyAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            yield return new EnterPlayMode();
            var p=PurpleIdentityCoupledProfile.Current;bool saved=p.candidateEnabled;
            int materials=OwnedMaterials(),meshes=OwnedMeshes();
            p.candidateEnabled=true;
            try
            {
                for(int repeat=0;repeat<3;repeat++)
                {
                    var root=new GameObject("IdentityLifecycle");
                    var visual=root.AddComponent<ProductionPurpleVisual>();visual.Configure(Vector3.forward*30);
                    var body=root.GetComponentInChildren<ProductionPurpleBodyRuntime>();
                    var renderer=body.transform.Find("R3_TornPlasmaVolume").GetComponent<Renderer>();
                    var outer=root.GetComponentInChildren<ProductionPurpleOuterRuntime>();
                    var halo=outer.transform.Find("OuterR2_VolumetricHalo").GetComponent<Renderer>();
                    visual.Render(2.7f,-1,false);
                    Assert.That(renderer.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/PurpleIdentityCoupled")));
                    Assert.That(halo.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/PurpleIdentityCoupledCorona")));
                    var count=OwnedMaterials();
                    for(int frame=0;frame<20;frame++)visual.Render(2.7f+frame/60f,-1,false);
                    Assert.That(OwnedMaterials(),Is.EqualTo(count));
                    visual.Render(5.4f,.26f,true);
                    Assert.That(renderer.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R3")));
                    Assert.That(halo.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/PurpleTravelHalo")));
                    var terminal=new GameObject("IdentityTerminalGuard").AddComponent<PurpleTerminalBurst>();
                    terminal.Configure(Vector3.forward*6,Vector3.forward,1);
                    Assert.That(terminal.GetComponentInChildren<ProductionPurpleBodyRuntime>(true),Is.Null);
                    Object.Destroy(terminal.gameObject);root.SetActive(false);Object.Destroy(root);
                    for(int wait=0;wait<6;wait++)
                    {
                        yield return null;
                        if(OwnedMaterials()==materials && OwnedMeshes()==meshes)break;
                    }
                    Assert.That(OwnedMaterials(),Is.EqualTo(materials));Assert.That(OwnedMeshes(),Is.EqualTo(meshes));
                }
            }
            finally { p.candidateEnabled=saved; }
            yield return new ExitPlayMode();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            PurpleIdentityCoupledProfile.Current.candidateEnabled=false;
            if(EditorApplication.isPlaying)yield return new ExitPlayMode();
        }
    }
}
