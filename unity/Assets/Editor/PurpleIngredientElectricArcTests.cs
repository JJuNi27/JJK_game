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
    public sealed class PurpleIngredientElectricArcTests
    {
        private bool oldReboot, oldFinal, oldFinal2, oldOuter, oldHybrid, oldExpanded, oldElectric;
        private static string Output => Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/PurpleIngredientElectricArcQA"));
        private static int Materials() => Resources.FindObjectsOfTypeAll<Material>().Count(m => m != null && m.name.StartsWith("PurpleIngredient"));

        [SetUp] public void SaveProfiles()
        {
            oldReboot = PurpleIngredientReboot2Profile.Current.candidateEnabled;
            oldFinal = PurpleIngredientFinalProfile.Current.candidateEnabled;
            oldFinal2 = PurpleIngredientFinal2Profile.Current.candidateEnabled;
            oldOuter = PurpleIngredientOuterExplorationProfile.Current.candidateEnabled;
            oldHybrid = PurpleIngredientHybridProfile.Current.candidateEnabled;
            oldExpanded = PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled;
            oldElectric = PurpleIngredientElectricArcProfile.Current.candidateEnabled;
        }

        [TearDown] public void RestoreProfiles()
        {
            PurpleIngredientReboot2Profile.Current.candidateEnabled = oldReboot;
            PurpleIngredientFinalProfile.Current.candidateEnabled = oldFinal;
            PurpleIngredientFinal2Profile.Current.candidateEnabled = oldFinal2;
            PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = oldOuter;
            PurpleIngredientHybridProfile.Current.candidateEnabled = oldHybrid;
            PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = oldExpanded;
            PurpleIngredientElectricArcProfile.Current.candidateEnabled = oldElectric;
        }

        private static void Select(bool electric)
        {
            PurpleIngredientReboot2Profile.Current.candidateEnabled = true;
            PurpleIngredientFinalProfile.Current.candidateEnabled = true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled = true;
            PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = false;
            PurpleIngredientHybridProfile.Current.candidateEnabled = false;
            PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = false;
            PurpleIngredientElectricArcProfile.Current.candidateEnabled = electric;
        }

        [Test] public void CandidateIsSeparateDefaultOffAndReleasesMaterials()
        {
            var profile = PurpleIngredientElectricArcProfile.Current;
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.candidateEnabled, Is.False, "Saved exploration candidate must be OFF");
            int before = Materials();
            Select(false);
            var baselineHost = new GameObject("Final2ElectricArcLock");
            baselineHost.AddComponent<PurpleFusionIngredient>().Configure(false);
            Assert.That(baselineHost.GetComponent<PurpleIngredientElectricArcVolume>(), Is.Null);
            Assert.That(baselineHost.GetComponent<PurpleIngredientBurstVolume>(), Is.Not.Null);
            Object.DestroyImmediate(baselineHost);
            EditorUtility.UnloadUnusedAssetsImmediate();
            Assert.That(Materials(), Is.EqualTo(before));
            Select(true);
            foreach (bool red in new[] { false, true })
            {
                var host = new GameObject("ElectricArcLifecycle");
                var ingredient = host.AddComponent<PurpleFusionIngredient>();
                ingredient.Configure(red);
                var arcs = host.GetComponent<PurpleIngredientElectricArcVolume>();
                Assert.That(arcs, Is.Not.Null);
                Assert.That(host.GetComponent<PurpleIngredientBurstVolume>(), Is.Null);
                Assert.That(host.GetComponent<PurpleIngredientOuterExplorationVolume>(), Is.Null);
                for (int frame = 0; frame < 240; frame++) ingredient.Render(frame / 120f, 0, Vector3.right * 3);
                Assert.That(arcs.PeakVisibleCount, Is.GreaterThan(2));
                Object.DestroyImmediate(host);
                EditorUtility.UnloadUnusedAssetsImmediate();
                Assert.That(Materials(), Is.EqualTo(before), "Electric arc and ingredient materials return to baseline");
            }
            var shader = Resources.Load<Shader>("VFX/HollowPurpleFilament");
            Assert.That(shader, Is.Not.Null);
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
                Assert.That(message.severity, Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error));
        }

        // Run only while the Editor is already playing CombatMVP. This is a capture utility,
        // not a Test Runner coroutine: the MCP EditMode runner cannot enter Play Mode reliably.
        public static string CaptureInPlayMode(bool full)
        {
            if (!Application.isPlaying) throw new InvalidOperationException("Enter CombatMVP Play Mode before capture");
            var actor = GameObject.Find("GojoPlayer");
            if (actor == null) throw new InvalidOperationException("CombatMVP GojoPlayer is missing");
            var caster = Camera.main;
            if (caster == null) throw new InvalidOperationException("CombatMVP main camera is missing");
            var profile = PurpleIngredientElectricArcProfile.Current;
            bool[] old = { PurpleIngredientReboot2Profile.Current.candidateEnabled, PurpleIngredientFinalProfile.Current.candidateEnabled,
                PurpleIngredientFinal2Profile.Current.candidateEnabled, PurpleIngredientOuterExplorationProfile.Current.candidateEnabled,
                PurpleIngredientHybridProfile.Current.candidateEnabled, PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled,
                profile.candidateEnabled };
            Vector3 stage = new Vector3(512, 0, 512);
            Vector3 oldPosition = actor.transform.position;
            Quaternion oldRotation = actor.transform.rotation;
            bool oldCameraEnabled = caster.enabled;
            float oldFieldOfView = caster.fieldOfView;
            RenderTexture oldTarget = caster.targetTexture;
            float oldTimeScale = Time.timeScale;
            var side = new GameObject("ElectricArcQAObserver").AddComponent<Camera>();
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
            Time.timeScale = 0;
            Vector3 origin = PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(stage, Vector3.forward);
            string output = Path.Combine(Output, full ? "AB" : "Prototype", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
            Directory.CreateDirectory(output);
            try
            {
                for (int mode = full ? 0 : 1; mode < 2; mode++)
                    foreach (bool casterView in new[] { false, true })
                    {
                        Select(mode == 1);
                        actor.transform.SetPositionAndRotation(stage, Quaternion.identity);
                        caster.transform.SetPositionAndRotation(stage + new Vector3(0, 4, -11), Quaternion.Euler(12, 0, 0));
                        var host = new GameObject("ElectricArcABSequence");
                        var sequence = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, actor.transform, 0, casterView ? caster : null, casterView);
                        string stem = (casterView ? "Caster" : "Side") + (mode == 0 ? "_Baseline" : "_Candidate");
                        try
                        {
                            using (var stream = full ? new FileStream(Path.Combine(output, stem + ".rgb24"), FileMode.CreateNew) : null)
                                for (int frame = 0; frame <= (full ? 96 : 3); frame++)
                                {
                                    float t = full ? frame / 30f : .64f + frame * .25f;
                                    sequence.Update(t, full ? 1 / 30f : .25f);
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
                        }
                        finally { sequence.Dispose(); Object.Destroy(host); }
                    }
                File.WriteAllText(Path.Combine(output, "format.json"), "{\"width\":960,\"height\":540,\"fps\":30,\"frames\":" + (full ? 97 : 4) + ",\"baseline\":\"Final2\",\"candidate\":\"ElectricArc\"}");
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
                actor.transform.SetPositionAndRotation(oldPosition, oldRotation);
                caster.targetTexture = oldTarget;
                caster.enabled = oldCameraEnabled;
                caster.fieldOfView = oldFieldOfView;
                Time.timeScale = oldTimeScale;
                target.Release();
                Object.Destroy(target);
                Object.Destroy(side.gameObject);
            }
        }
    }
}
