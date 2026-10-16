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
    public sealed class PurpleExplosionTurbulenceTests
    {
        [Test] public void SavedOffShaderAndKoreanInspector()
        {
            var p=PurpleExplosionTurbulenceProfile.Current;
            Assert.That(p,Is.Not.Null);Assert.That(p.candidateEnabled,Is.False);
            var shader=Resources.Load<Shader>("VFX/PurpleExplosionTurbulence");
            Assert.That(shader,Is.Not.Null);Assert.That(shader.isSupported,Is.True);
            Assert.That(ShaderUtil.GetShaderMessages(shader).Count(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),Is.Zero);
            var editor=Editor.CreateEditor(p);
            try { Assert.That(editor,Is.TypeOf<PurpleExplosionTurbulenceProfileEditor>()); }
            finally { Object.DestroyImmediate(editor); }
        }

        private static int Materials()=>Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name=="PurpleExplosionTurbulence_Runtime" || m.name.StartsWith("PurpleProduction_"));
        private static int Meshes()=>Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("PurpleProduction_"));

        [UnityTest] public IEnumerator BodyOnlyTravelTerminalAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            yield return new EnterPlayMode();
            var p=PurpleExplosionTurbulenceProfile.Current;bool saved=p.candidateEnabled;
            int materials=Materials(),meshes=Meshes();
            p.candidateEnabled=true;
            try
            {
                for(int repeat=0;repeat<4;repeat++)
                {
                    var root=new GameObject("TurbulenceLifecycle");
                    var visual=root.AddComponent<ProductionPurpleVisual>();visual.Configure(Vector3.forward*30);
                    visual.Render(2.7f,-1,false);
                    var body=root.GetComponentInChildren<ProductionPurpleBodyRuntime>();
                    var renderer=body.transform.Find("R3_TornPlasmaVolume").GetComponent<Renderer>();
                    Assert.That(renderer.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/PurpleExplosionTurbulence")));
                    int peak=Materials();
                    for(int frame=0;frame<30;frame++)visual.Render(2.7f+frame/60f,-1,false);
                    Assert.That(Materials(),Is.EqualTo(peak));
                    visual.Render(5.4f,.26f,true);
                    Assert.That(renderer.sharedMaterial.shader,Is.EqualTo(Resources.Load<Shader>("VFX/HollowPurpleHybridD2R3")),"Travel must retain the original R3 material");
                    var terminal=new GameObject("TurbulenceTerminalGuard").AddComponent<PurpleTerminalBurst>();
                    terminal.Configure(Vector3.forward*6,Vector3.forward,1);
                    Assert.That(terminal.GetComponentInChildren<ProductionPurpleBodyRuntime>(true),Is.Null);
                    Assert.That(terminal.GetComponentInChildren<PurpleEnergyBody>(true).UsesProductionCandidate,Is.False);
                    Object.Destroy(terminal.gameObject);root.SetActive(false);Object.Destroy(root);
                    // Destroy is deferred, including resources released by nested OnDestroy callbacks.
                    for(int wait=0;wait<6;wait++)
                    {
                        yield return null;
                        if(Materials()==materials && Meshes()==meshes)break;
                    }
                    Assert.That(Materials(),Is.EqualTo(materials));Assert.That(Meshes(),Is.EqualTo(meshes));
                }
            }
            finally { p.candidateEnabled=saved; }
            yield return new ExitPlayMode();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            PurpleExplosionTurbulenceProfile.Current.candidateEnabled=false;
            if(EditorApplication.isPlaying)yield return new ExitPlayMode();
        }
    }
}
