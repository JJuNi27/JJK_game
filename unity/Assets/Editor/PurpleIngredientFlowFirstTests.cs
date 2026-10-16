using System;
using System.IO;
using System.Linq;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace JJKGame.EditorTools
{
    public sealed class PurpleIngredientFlowFirstTests
    {
        private bool oldReboot, oldFinal, oldFinal2, oldOuter, oldHybrid, oldExpanded, oldElectric;
        private bool oldArc;
        private static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/PurpleIngredientFlowFirstQA"));
        private static int Materials() => Resources.FindObjectsOfTypeAll<Material>().Count(m => m != null && m.name.StartsWith("PurpleIngredient"));

        [SetUp] public void SaveProfiles()
        {
            oldArc=PurpleIngredientElectricArcProfile.Current.candidateEnabled;
            oldReboot = PurpleIngredientReboot2Profile.Current.candidateEnabled;
            oldFinal = PurpleIngredientFinalProfile.Current.candidateEnabled;
            oldFinal2 = PurpleIngredientFinal2Profile.Current.candidateEnabled;
            oldOuter = PurpleIngredientOuterExplorationProfile.Current.candidateEnabled;
            oldHybrid = PurpleIngredientHybridProfile.Current.candidateEnabled;
            oldExpanded = PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled;
            oldElectric = PurpleIngredientFlowFirstProfile.Current.candidateEnabled;
        }

        [TearDown] public void RestoreProfiles()
        {
            PurpleIngredientElectricArcProfile.Current.candidateEnabled=oldArc;
            PurpleIngredientReboot2Profile.Current.candidateEnabled = oldReboot;
            PurpleIngredientFinalProfile.Current.candidateEnabled = oldFinal;
            PurpleIngredientFinal2Profile.Current.candidateEnabled = oldFinal2;
            PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = oldOuter;
            PurpleIngredientHybridProfile.Current.candidateEnabled = oldHybrid;
            PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = oldExpanded;
            PurpleIngredientFlowFirstProfile.Current.candidateEnabled = oldElectric;
        }

        private static void Select(bool electric)
        {
            PurpleIngredientElectricArcProfile.Current.candidateEnabled=false;
            PurpleIngredientReboot2Profile.Current.candidateEnabled = true;
            PurpleIngredientFinalProfile.Current.candidateEnabled = true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled = true;
            PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = false;
            PurpleIngredientHybridProfile.Current.candidateEnabled = false;
            PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = false;
            PurpleIngredientFlowFirstProfile.Current.candidateEnabled = electric;
        }

        [Test] public void CandidateIsSeparateDefaultOffAndReleasesMaterials()
        {
            var profile = PurpleIngredientFlowFirstProfile.Current;
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.candidateEnabled, Is.False, "Saved exploration candidate must be OFF");
            int before = Materials();
            Select(false);
            var baselineHost = new GameObject("Final2FlowFirstLock");
            baselineHost.AddComponent<PurpleFusionIngredient>().Configure(false);
            Assert.That(baselineHost.GetComponent<PurpleIngredientFlowFirstVolume>(), Is.Null);
            Assert.That(baselineHost.GetComponent<PurpleIngredientBurstVolume>(), Is.Not.Null);
            Object.DestroyImmediate(baselineHost);
            EditorUtility.UnloadUnusedAssetsImmediate();
            Assert.That(Materials(), Is.EqualTo(before));
            Select(true);
            foreach (bool red in new[] { false, true })
            {
                var host = new GameObject("FlowFirstLifecycle");
                var ingredient = host.AddComponent<PurpleFusionIngredient>();
                ingredient.Configure(red);
                var arcs = host.GetComponent<PurpleIngredientFlowFirstVolume>();
                Assert.That(arcs, Is.Not.Null);
                Assert.That(host.GetComponent<PurpleIngredientBurstVolume>(), Is.Null);
                Assert.That(host.GetComponent<PurpleIngredientOuterExplorationVolume>(), Is.Null);
                for (int frame = 0; frame < 240; frame++) ingredient.Render(frame / 120f, 0, Vector3.right * 3);
                Assert.That(host.GetComponentsInChildren<LineRenderer>().Any(line => line.enabled), Is.True);
                Object.DestroyImmediate(host);
                EditorUtility.UnloadUnusedAssetsImmediate();
                Assert.That(Materials(), Is.EqualTo(before), "Electric arc and ingredient materials return to baseline");
            }
            foreach(var shaderPath in new[]{"VFX/PurpleIngredientFlowBrush","VFX/PurpleIngredientFlowSprite","VFX/HollowPurpleFilament"})
            {
                var shader = Resources.Load<Shader>(shaderPath);
                Assert.That(shader, Is.Not.Null,shaderPath);
                foreach (var message in ShaderUtil.GetShaderMessages(shader))
                    Assert.That(message.severity, Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),message.message);
            }
        }

        [Test] public void OppositeFlowDirectionsFusionScatterAndRepeatedCleanup()
        {
            Select(true);int baseline=Materials();var settings=GojoPolishSettings.Current;
            for(int repeat=0;repeat<3;repeat++)
            {
                var root=new GameObject("FlowFirstRepeatedCleanup");
                var a=new GameObject("Blue");a.transform.SetParent(root.transform);a.transform.position=new Vector3(-2,3,0);
                var b=new GameObject("Red");b.transform.SetParent(root.transform);b.transform.position=new Vector3(2,3,0);
                var blue=a.AddComponent<PurpleFusionIngredient>();blue.Configure(false);
                var red=b.AddComponent<PurpleFusionIngredient>();red.Configure(true);
                blue.Render(.08f,0,b.transform.position);red.Render(.08f,0,a.transform.position);
                var blueLine=a.transform.Find("FlowBrush_0").GetComponent<LineRenderer>();
                var redLine=b.transform.Find("FlowBrush_0").GetComponent<LineRenderer>();
                float blueStart=blueLine.GetPosition(0).magnitude,redStart=redLine.GetPosition(0).magnitude;
                blue.Render(.16f,0,b.transform.position);red.Render(.16f,0,a.transform.position);
                Assert.That(blueLine.GetPosition(0).magnitude,Is.LessThan(blueStart),"Blue stroke head must move inward");
                Assert.That(redLine.GetPosition(0).magnitude,Is.GreaterThan(redStart),"Red pressure front must expand outward");
                var child=new GameObject("Fusion");child.transform.SetParent(root.transform);
                var fusion=child.AddComponent<PurpleIngredientFlowFirstFusion>();
                fusion.Configure(PurpleIngredientFlowFirstProfile.Current,blue,red,settings.PurpleFusionStart,settings.PurpleHoldStart);
                fusion.Sample(Mathf.Lerp(settings.PurpleFusionStart,settings.PurpleHoldStart,.7f));
                Assert.That(fusion.ScatterCount,Is.EqualTo(16));
                // ParticleSystem.SetParticles is not simulated by the EditMode runner.
                // Actual particle population is asserted by the PlayMode capture below.
                fusion.EndIfFinished(settings.PurpleHoldStart);
                Assert.That(fusion.Finished,Is.True);Assert.That(fusion.ScatterCount,Is.Zero);Assert.That(child.activeSelf,Is.False);
                Object.DestroyImmediate(root);EditorUtility.UnloadUnusedAssetsImmediate();
                Assert.That(Materials(),Is.EqualTo(baseline),"No per-cast material growth");
            }
        }

        // Run only while the Editor is already playing CombatMVP. This is a capture utility,
        // not a Test Runner coroutine: the MCP EditMode runner cannot enter Play Mode reliably.
        public static string CaptureInPlayMode(bool full, int phaseStep = 1)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Enter CombatMVP Play Mode before capture");
            var actor = GameObject.Find("GojoPlayer");
            if (actor == null) throw new InvalidOperationException("CombatMVP GojoPlayer is missing");
            var caster = Camera.main;
            if (caster == null) throw new InvalidOperationException("CombatMVP main camera is missing");
            var profile = PurpleIngredientFlowFirstProfile.Current;
            bool[] old = { PurpleIngredientReboot2Profile.Current.candidateEnabled, PurpleIngredientFinalProfile.Current.candidateEnabled,
                PurpleIngredientFinal2Profile.Current.candidateEnabled, PurpleIngredientOuterExplorationProfile.Current.candidateEnabled,
                PurpleIngredientHybridProfile.Current.candidateEnabled, PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled,
                profile.candidateEnabled, PurpleIngredientElectricArcProfile.Current.candidateEnabled };
            var randomState=UnityEngine.Random.state;
            Vector3 stage = new Vector3(512,1,512);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="FlowFirstQAGround";
            floor.transform.position=new Vector3(stage.x,0,stage.z);floor.transform.localScale=Vector3.one*8;
            Object.Destroy(floor.GetComponent<Collider>());
            var floorMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));floorMaterial.SetColor("_BaseColor",new Color(.035f,.045f,.06f));
            floor.GetComponent<Renderer>().sharedMaterial=floorMaterial;
            Vector3 oldPosition = actor.transform.position;
            Quaternion oldRotation = actor.transform.rotation;
            bool oldCameraEnabled = caster.enabled;
            float oldFieldOfView = caster.fieldOfView;
            RenderTexture oldTarget = caster.targetTexture;
            Vector3 cameraPosition=caster.transform.position; Quaternion cameraRotation=caster.transform.rotation;
            var side = new GameObject("FlowFirstQAObserver").AddComponent<Camera>();
            side.CopyFrom(caster);
            side.enabled = false;
            side.fieldOfView = 45;
            side.farClipPlane = 180;
            var sideData = side.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var casterData = caster.GetComponent<UniversalAdditionalCameraData>();
            sideData.renderPostProcessing = casterData != null && casterData.renderPostProcessing;
            if (casterData != null) sideData.volumeLayerMask = casterData.volumeLayerMask;
            sideData.requiresColorTexture = true;
            sideData.requiresDepthTexture = true;
            var target = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            target.Create();
            caster.targetTexture = target;
            side.targetTexture = target;
            caster.enabled = false;
            caster.fieldOfView = 60;
            Vector3 origin = PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(stage, Vector3.forward);
            string output = Path.Combine(Output, full ? "AB" : "PrototypeStep" + phaseStep, DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(output);
            try
            {
                for (int mode = full ? 0 : 1; mode < 2; mode++)
                    foreach (bool casterView in new[] { false, true })
                    {
                        Select(mode == 1);
                        UnityEngine.Random.InitState(240926);
                        actor.transform.SetPositionAndRotation(stage, Quaternion.identity);
                        caster.transform.SetPositionAndRotation(stage + new Vector3(0, 4, -11), Quaternion.Euler(12, 0, 0));
                        var host = new GameObject("FlowFirstABSequence");
                        var sequence = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, actor.transform, 0, casterView ? caster : null, casterView);
                        string stem = (casterView ? "Caster" : "Side") + (mode == 0 ? "_Baseline" : "_Candidate");
                        var poses=new System.Text.StringBuilder();
                        try
                        {
                            using (var stream = full ? new FileStream(Path.Combine(output, stem + ".rgb24"), FileMode.CreateNew) : null)
                                for (int frame = 0; frame <= (full ? 96 : 3); frame++)
                                {
                                    float t = full ? frame / 30f : phaseStep >= 4 ? 1.58f + frame * .12f : .64f + frame * .25f;
                                    sequence.Update(t, full ? 1 / 30f : .25f);
                                    if(full && mode==1 && frame==48)
                                    {
                                        var fusion=host.GetComponentInChildren<PurpleIngredientFlowFirstFusion>();
                                        Assert.That(fusion,Is.Not.Null);
                                        int live=fusion.GetComponentsInChildren<ParticleSystem>().Sum(ps=>ps.particleCount);
                                        Assert.That(live,Is.GreaterThan(0),"Fusion remnants must really exist in Play Mode");
                                        File.WriteAllText(Path.Combine(output,stem+".particles.json"),"{\"time\":1.6,\"liveParticles\":"+live+"}");
                                    }
                                    foreach(string item in new[]{"HollowPurpleBlueOrbRoot","HollowPurpleRedOrbRoot","HollowPurpleDenseBody"})
                                    {
                                        var pose=host.transform.Find("HollowPurpleCanonicalOrbSequence/"+item);
                                        poses.AppendLine(t.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"|"+item+"|"+pose.position.ToString("F7")+"|"+pose.localScale.ToString("F7")+"|"+pose.gameObject.activeSelf);
                                    }
                                    Vector3 offset = new Vector3(5, 2, -12);
                                    side.transform.SetPositionAndRotation(origin + Vector3.forward * Mathf.Clamp01((t - GojoPolishSettings.Current.PurpleReleaseTime) / 1.6f) * 48 + offset, Quaternion.LookRotation(-offset));
                                    RenderPipeline.SubmitRenderRequest(casterView ? caster : side, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                                    var previous = RenderTexture.active;
                                    RenderTexture.active = target;
                                    var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
                                    image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
                                    image.Apply();
                                    RenderTexture.active = previous;
                                    if (full) stream.Write(image.GetRawTextureData<byte>().ToArray(), 0, 960 * 540 * 3);
                                    if (!full || frame == 23 || frame == 43 || frame == 64 || frame == 96)
                                        File.WriteAllBytes(Path.Combine(output, stem + "_" + frame.ToString("D3") + ".png"), image.EncodeToPNG());
                                    Object.DestroyImmediate(image);
                                }
                            File.WriteAllText(Path.Combine(output,stem+".poses.txt"),poses.ToString());
                        }
                        finally { sequence.Dispose(); Object.Destroy(host); }
                    }
                File.WriteAllText(Path.Combine(output, "format.json"), "{\"width\":960,\"height\":540,\"fps\":30,\"frames\":" + (full ? 97 : 4) + ",\"baseline\":\"Final2\",\"candidate\":\"FlowFirst\"}");
                return output;
            }
            finally
            {
                PurpleIngredientReboot2Profile.Current.candidateEnabled = old[0];
                PurpleIngredientFinalProfile.Current.candidateEnabled = old[1];
                PurpleIngredientFinal2Profile.Current.candidateEnabled = old[2];
                PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = old[3];
                PurpleIngredientHybridProfile.Current.candidateEnabled = old[4];
                PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = old[5];
                profile.candidateEnabled = old[6];
                PurpleIngredientElectricArcProfile.Current.candidateEnabled=old[7];UnityEngine.Random.state=randomState;
                actor.transform.SetPositionAndRotation(oldPosition, oldRotation);
                caster.targetTexture = oldTarget;
                caster.enabled = oldCameraEnabled;
                caster.fieldOfView = oldFieldOfView;
                caster.transform.SetPositionAndRotation(cameraPosition,cameraRotation);
                target.Release();
                Object.Destroy(target);
                Object.Destroy(side.gameObject);
                Object.Destroy(floor);Object.Destroy(floorMaterial);
            }
        }
    }
}
