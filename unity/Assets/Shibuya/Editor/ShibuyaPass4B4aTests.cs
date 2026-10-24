using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4B4aDataTests
    {
        internal static readonly string[] Paths = {
            CursedEnergyAuraAssets.DefaultPath,
            CursedEnergyAuraAssets.Root + "/Presets/Pink Aura.asset",
            CursedEnergyAuraAssets.Root + "/Presets/Green Aura.asset"
        };
        internal static void Approved(CursedEnergyAuraProfile p)
        {
            Assert.That(p.wisps.enabled, Is.False);
            Assert.That(p.preserveLocalFlameShape, Is.True);
            Assert.That(p.extraFieldReach, Is.EqualTo(.65f));
            Assert.That(p.inkBoundaryScale, Is.EqualTo(1.3f));
            Assert.That(p.inkInteriorArea, Is.EqualTo(.8f));
            Assert.That(p.detachedTips, Is.True);
            Assert.That(p.upperFieldConnection, Is.EqualTo(1));
            Assert.That(p.lowerFieldConnection, Is.EqualTo(1));
            Assert.That(p.innerFlowVisibility, Is.EqualTo(.65f));
            Assert.That(p.innerFlowDensity, Is.EqualTo(1));
            Assert.That(p.tipCycleRate, Is.EqualTo(.7f));
        }
        [Test] public void SavedProfilesExactlyMatchLegacyPlusApprovedCandidate()
        {
            foreach (string path in Paths) {
                var actual = AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(path);
                var expected = ScriptableObject.CreateInstance<CursedEnergyAuraProfile>();
                try {
                    // Neutral class defaults remain available for legacy reconstruction. Apply the
                    // EXISTING approved authoring function, with no new hand-picked combination.
                    expected.visualPrefab = actual.visualPrefab; expected.mainColor = actual.mainColor;
                    CursedEnergyAuraEndPreview.Candidate(expected);
                    Approved(actual);
                    Assert.That(JsonUtility.ToJson(actual), Is.EqualTo(JsonUtility.ToJson(expected)), path);
                    Assert.That(actual.SizeMultiplier(100), Is.EqualTo(1));
                    Assert.That(actual.SizeMultiplier(150), Is.EqualTo(1.5f));
                    Assert.That(actual.SizeMultiplier(200), Is.EqualTo(2));
                    Assert.That(actual.maximumSizeMultiplier, Is.EqualTo(3));
                    var editor = UnityEditor.Editor.CreateEditor(actual);
                    try { Assert.That(editor, Is.TypeOf<CursedEnergyAuraProfileInspector>()); }
                    finally { Object.DestroyImmediate(editor); }
                } finally { Object.DestroyImmediate(expected); }
            }
        }
    }

    public sealed class ShibuyaPass4B4aEntryTests
    {
        // Separate test runs each own one Enter/Exit pair. Re-entering twice inside a single
        // EditMode coroutine stalled the Editor at frame 1; retain that original failed run.
        [UnityTest] public IEnumerator SavedDefaultFirstEntryWithoutOverride()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return Entry(); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator SavedDefaultReentryWithoutOverride()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return Entry(); yield return new ExitPlayMode();
        }
        private IEnumerator Entry()
        {
                yield return null;
                var match = Object.FindFirstObjectByType<ShibuyaMatch>();
                var input = match.Player.GetComponent<ShibuyaPlayerInput>();
                var source = Resources.Load<CursedEnergyAuraProfile>("Cursed Energy Aura");
                // No Configure, Candidate, menu or temporary profile before these assertions.
                Assert.That(input.Aura.Profile, Is.SameAs(source));
                Assert.That(AssetDatabase.Contains(input.Aura.Profile), Is.True);
                ShibuyaPass4B4aDataTests.Approved(input.Aura.Profile);
                var resource = input.EnergyCharge.Resource; resource.SetCurrentEnergy(20);
                input.TestInput = () => new ShibuyaInputFrame { auraHeld = true };
                yield return Wait(.4f);
                Assert.That(input.Aura.Profile, Is.SameAs(source));
                Assert.That(input.Aura.Visual.HasVisibleRenderers, Is.True);
                Assert.That(input.Aura.Visual.transform.Find("EnergyWisps").GetComponent<Renderer>().enabled, Is.False);
                Assert.That(input.Aura.Visual.transform.Find("DetachedTips").GetComponent<Renderer>().enabled, Is.True);
                Assert.That(resource.CurrentEnergy, Is.GreaterThan(23).And.LessThanOrEqualTo(100));
                Assert.That(match.PlayerEnergy, Is.SameAs(resource));
                {
                    string folder = ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass4B4aQA");
                    Directory.CreateDirectory(folder);
                    ScreenCapture.CaptureScreenshot(folder + "/normal-F-hud.png"); yield return Wait(.25f);
                    Assert.That(File.Exists(folder + "/normal-F-hud.png"), Is.True);
                }
                input.TestInput = () => default; yield return Wait(.4f);
                Assert.That(input.EnergyCharge.IsCharging, Is.False);
                Assert.That(input.Aura.Visual.HasVisibleRenderers, Is.False);
        }
        private static IEnumerator Wait(float seconds) { double until = Time.unscaledTimeAsDouble + seconds; while (Time.unscaledTimeAsDouble < until) yield return null; }
        [UnityTearDown] public IEnumerator Leave() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
    }

    public sealed class ShibuyaPass4B4aTests : ShibuyaPass4B3cFixture
    {
        [UnityTest] public IEnumerator NormalFAndApprovedPreviewSamePhasePaletteAndCapacity()
        {
            var metrics = new List<object>();
            foreach (string path in ShibuyaPass4B4aDataTests.Paths) {
                var saved = AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(path);
                foreach (float maximum in path == CursedEnergyAuraAssets.DefaultPath ? new[] { 100f, 150, 200 } : new[] { 200f }) {
                    Capacity(maximum); aura.Configure(saved); frame.auraHeld = true;
                    yield return Wait(1.6f);
                    Record("4B4a-comparison-readiness", new { path, maximum, frame = Time.frameCount,
                        sourcePresent = saved != null, prefabPresent = saved != null && saved.visualPrefab != null,
                        runtimeProfile = aura.Profile != null ? aura.Profile.name : "null", held = aura.Held,
                        envelope = aura.Envelope, state = player.State.ToString(), visualPresent = aura.Visual != null });
                    Assert.That(aura.Visual, Is.Not.Null, "F input must create the saved aura before comparison: " + path);
                    var ps = aura.Visual.GetComponentInChildren<ParticleSystem>();
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); ps.useAutoRandomSeed = false; ps.randomSeed = 38421;
                    string prefix = "4B4a/" + saved.name.Replace(' ', '-') + "-" + maximum;
                    FrozenViews(prefix + "/normal-F");
                    var vertices = Field.vertices; float multiple = aura.SizeMultiplier;

                    var expected = ScriptableObject.CreateInstance<CursedEnergyAuraProfile>();
                    expected.name = "PASS4B3 QA approved reference";
                    // Capture reference uses the original neutral defaults plus the existing 3c
                    // candidate, with the SAME saved size curve and palette, not a forced 4x menu.
                    expected.visualPrefab = saved.visualPrefab; expected.mainColor = saved.mainColor;
                    CursedEnergyAuraEndPreview.Candidate(expected);
                    aura.Configure(expected); aura.Visual.Sample(expected, 1, true, multiple);
                    FrozenViews(prefix + "/approved-preview");
                    Assert.That(Field.vertices, Is.EqualTo(vertices));
                    var differences = new List<object>();
                    foreach (string view in new[] { "front", "side", "diagonal", "back", "high", "top" }) {
                        int pixels = DifferentPixels(prefix + "/normal-F/" + view, prefix + "/approved-preview/" + view);
                        differences.Add(new { view, changedPixels = pixels });
                        Assert.That(pixels, Is.Zero, prefix + "/" + view);
                    }
                    metrics.Add(new { palette = saved.name, maximum, multiple, differences });
                    aura.Configure(saved);
                }
            }
            Record("4B4a-render-equivalence", metrics);
        }
        private void FrozenViews(string folder)
        {
            FixedPhase(2); Views(folder); Pose(new Vector3(.02f, 5.5f, .02f)); Capture(folder + "/top");
        }
        private static int DifferentPixels(string a, string b)
        {
            var x = new Texture2D(2, 2); var y = new Texture2D(2, 2);
            try {
                x.LoadImage(File.ReadAllBytes(Path.Combine(Root, a + ".png"))); y.LoadImage(File.ReadAllBytes(Path.Combine(Root, b + ".png")));
                var p = x.GetPixels32(); var q = y.GetPixels32(); Assert.That(q.Length, Is.EqualTo(p.Length));
                return Enumerable.Range(0, p.Length).Count(i => p[i].r != q[i].r || p[i].g != q[i].g || p[i].b != q[i].b);
            } finally { Object.Destroy(x); Object.Destroy(y); }
        }
        [UnityTest] public IEnumerator SavedColorsAndRepeatedHoldReleaseCleanOwnedResources()
        {
            var settings = AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(CursedEnergyAuraAssets.DefaultPath);
            aura.Configure(settings); frame.auraHeld = true; yield return Wait(.5f);
            var visual = aura.Visual;
            foreach (string path in ShibuyaPass4B4aDataTests.Paths) {
                var color = AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(path);
                aura.Configure(color); yield return Wait(.12f);
                ShibuyaPass4B4aDataTests.Approved(aura.Profile);
                Assert.That(visual.EnergyMaterial.GetColor("_MainColor"), Is.EqualTo(color.mainColor));
                Assert.That(visual.EnergyMaterial.GetColor("_InkColor"), Is.EqualTo(color.inkColor));
                Assert.That(visual.EnergyMaterial.GetVector("_InnerFlow").x, Is.EqualTo(.65f));
            }
            for (int i = 0; i < 12; i++) {
                frame.auraHeld = false; yield return Wait(.05f); frame.auraHeld = true; yield return Wait(.08f);
                Assert.That(aura.Visual, Is.SameAs(visual)); Assert.That(visual.OwnedMeshCount, Is.EqualTo(4)); Assert.That(visual.OwnedMaterialCount, Is.EqualTo(4));
            }
            frame.auraHeld = false; yield return Wait(.4f); Assert.That(visual.HasVisibleRenderers, Is.False); Assert.That(visual.ParticleCount, Is.Zero);
            Object.Destroy(visual.gameObject); yield return Wait(.15f);
            int materials = Resources.FindObjectsOfTypeAll<Material>().Count(x => x.name.StartsWith("CursedAura_Owned_"));
            int meshes = Resources.FindObjectsOfTypeAll<Mesh>().Count(x => x.name.StartsWith("CursedAura_OwnedVolume_"));
            Assert.That(materials, Is.Zero); Assert.That(meshes, Is.Zero);
            Record("4B4a-resource-cleanup", new { repeatedCycles = 12, materials, meshes });
        }
    }
}
