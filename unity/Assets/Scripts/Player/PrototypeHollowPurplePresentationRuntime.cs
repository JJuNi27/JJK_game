using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(2000)]
    [DisallowMultipleComponent]
    public sealed class PrototypeHollowPurplePresentationRuntime : MonoBehaviour
    {
        private const string LegacyRootName = "HollowPurplePrototypeVisual";
        private const string TargetSceneName = "CombatMVP";
        private const float ScanInterval = 0.08f;

        private readonly Dictionary<int, TrackedSource> trackedSources =
            new Dictionary<int, TrackedSource>();
        private readonly List<int> staleSourceIds = new List<int>();
        private readonly List<OrbSequence> sequences = new List<OrbSequence>();

        private float nextScanAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            InstallForCurrentScene();
        }

        private static void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            InstallForCurrentScene();
        }

        private static void InstallForCurrentScene()
        {
            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || FindFirstObjectByType<PrototypeHollowPurplePresentationRuntime>() != null
            )
            {
                return;
            }

            GameObject runner = new GameObject("PrototypeHollowPurplePresentationRuntime");
            runner.AddComponent<PrototypeHollowPurplePresentationRuntime>();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextScanAt)
            {
                nextScanAt = Time.unscaledTime + ScanInterval;
                RefreshTrackedSources();
            }

            UpdateTrackedSources();
            UpdateSequences();
        }

        private void OnDisable()
        {
            foreach (OrbSequence sequence in sequences)
            {
                sequence?.Dispose();
            }
            sequences.Clear();
        }

        private void OnDestroy() => OnDisable();

        private void RefreshTrackedSources()
        {
            GojoTechniqueChainController[] controllers =
                FindObjectsByType<GojoTechniqueChainController>(FindObjectsSortMode.None);

            HashSet<int> liveIds = new HashSet<int>();
            foreach (GojoTechniqueChainController controller in controllers)
            {
                if (controller == null)
                {
                    continue;
                }

                int id = controller.GetInstanceID();
                liveIds.Add(id);
                if (!trackedSources.ContainsKey(id))
                {
                    trackedSources.Add(id, new TrackedSource(controller));
                }
            }

            staleSourceIds.Clear();
            foreach (KeyValuePair<int, TrackedSource> pair in trackedSources)
            {
                if (pair.Value == null || pair.Value.Controller == null || !liveIds.Contains(pair.Key))
                {
                    staleSourceIds.Add(pair.Key);
                }
            }

            foreach (int id in staleSourceIds)
            {
                trackedSources.Remove(id);
            }
        }

        private void UpdateTrackedSources()
        {
            foreach (TrackedSource tracked in trackedSources.Values)
            {
                if (tracked == null || tracked.Controller == null)
                {
                    continue;
                }

                if (tracked.SourceRoot == null)
                {
                    tracked.SourceRoot = tracked.Controller.transform.Find(LegacyRootName);
                    tracked.WasActive = false;
                }

                if (tracked.SourceRoot == null)
                {
                    continue;
                }

                DisableLegacyBeam(tracked.SourceRoot);

                bool sourceActive = tracked.SourceRoot.gameObject.activeInHierarchy;
                if (sourceActive && !tracked.WasActive)
                {
                    StartOrbSequence(tracked.Controller.transform);
                }

                tracked.WasActive = sourceActive;
            }
        }

        private static void DisableLegacyBeam(Transform sourceRoot)
        {
            LineRenderer[] lines = sourceRoot.GetComponentsInChildren<LineRenderer>(true);
            foreach (LineRenderer line in lines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }

            Light[] lights = sourceRoot.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }

        private void StartOrbSequence(Transform fighter)
        {
            OrbSequence sequence = CreateCanonicalOrbSequence(
                transform,
                fighter,
                fighter.GetComponent<GojoTechniqueChainController>().PurplePresentationStartedAt
            );
            if (sequence != null)
            {
                sequences.Add(sequence);
            }
        }

        public static OrbSequence CreateCanonicalOrbSequence(
            Transform runtimeRoot,
            Transform fighter,
            float startTime,
            Camera presentationCamera = null,
            bool enableCasterPresentation = true
        )
        {
            if (runtimeRoot == null || fighter == null)
            {
                return null;
            }

            Vector3 direction = fighter.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector3.forward;
            }
            direction.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 start = FormationOrigin(fighter.position, direction);
            var chain = fighter.GetComponent<GojoTechniqueChainController>();
            Camera view = enableCasterPresentation ? presentationCamera : null;
            if (enableCasterPresentation && view == null && Camera.main != null)
            {
                Camera main = Camera.main;
                var follow = main.GetComponent<JJKGame.CameraSystem.SimpleCameraFollow>();
                var lab = main.GetComponent<JJKGame.Dev.VFXLab.VfxLabOrbitCamera>();
                if ((follow != null && follow.isActiveAndEnabled && follow.Follows(fighter))
                    || (lab != null && lab.isActiveAndEnabled && lab.Presents(fighter))) view = main;
            }
            var sequence = new OrbSequence(runtimeRoot, start, direction, right, startTime,
                chain != null ? chain.PurpleRange : GojoTechniqueChainController.DefaultPurpleRange,
                chain != null ? chain.PurpleLaunchDuration : GojoTechniqueChainController.DefaultPurpleLaunchDuration, view);
            sequence.BindCaster(fighter);
            return sequence;
        }

        public static Vector3 FormationOrigin(Vector3 casterPosition, Vector3 direction)
        {
            return casterPosition + Vector3.up * 1.05f + direction * 1.10f;
        }

        public static Vector3 ReleaseOrigin(Vector3 casterPosition, Vector3 direction)
        {
            Vector3 offset = GojoPolishSettings.Current.purpleHoldOffset;
            return FormationOrigin(casterPosition, direction) + Vector3.up * offset.y + direction * offset.z;
        }

        private void UpdateSequences()
        {
            for (int index = sequences.Count - 1; index >= 0; index--)
            {
                OrbSequence sequence = sequences[index];
                if (sequence == null)
                {
                    sequences.RemoveAt(index);
                    continue;
                }

                if (sequence.Update(Time.unscaledTime, Time.unscaledDeltaTime))
                {
                    continue;
                }

                sequence.Dispose();
                sequences.RemoveAt(index);
            }
        }

        private sealed class TrackedSource
        {
            public GojoTechniqueChainController Controller { get; }
            public Transform SourceRoot { get; set; }
            public bool WasActive { get; set; }

            public TrackedSource(GojoTechniqueChainController controller)
            {
                Controller = controller;
            }
        }

        public sealed class OrbSequence
        {
            private readonly float MergeDuration;
            private readonly float fusionStart, holdStart, releaseTime, blueLeadDuration;
            private readonly float LaunchDuration;
            private readonly float TravelDistance;
            private readonly float residueDuration;
            private readonly float fusionHoldDuration;
            private readonly float visualScale;
            private readonly PurpleTravelAftermath aftermath;

            private readonly GameObject root;
            private readonly Transform blueOrb;
            private readonly Transform redOrb;
            private readonly Transform purpleOrb;
            private readonly PurpleEnergyBody energyBody;
            private readonly PurpleReleasePresentation releasePresentation;
            private Transform caster;
            private GojoTechniqueChainController casterChain;
            private JJKGame.Core.Health casterHealth;
            private bool boundCaster;
            private readonly Light purpleLight;
            private readonly LineRenderer fusionFront;
            private readonly List<Material> runtimeMaterials = new List<Material>();
            private readonly List<Color> materialColors = new List<Color>();
            private readonly PurpleFusionIngredient blueField, redField;
            private readonly float formationScale, formationSeparation;
            private PurpleTerminalBurst terminal;
            private float terminalAt = float.PositiveInfinity;
            private Vector3 terminalPosition;
            public bool HasTerminated => terminal != null;
            public Vector3 TerminalPosition => terminalPosition;
            private readonly ParticleSystem purpleFragments;
            private readonly ParticleSystem purpleTrail;
            private readonly Vector3 start;
            private readonly Vector3 direction;
            private readonly Vector3 right;
            private readonly float startedAt;
            private readonly Vector3 holdOffset;
            private readonly Transform warpedWake;
            private readonly Material wakeMaterial;

            private bool launched;

            public OrbSequence(
                Transform runtimeRoot,
                Vector3 sequenceStart,
                Vector3 sequenceDirection,
                Vector3 sequenceRight,
                float startTime,
                float travelDistance = GojoTechniqueChainController.DefaultPurpleRange,
                float launchDuration = GojoTechniqueChainController.DefaultPurpleLaunchDuration,
                Camera presentationCamera = null
            )
            {
                start = sequenceStart;
                direction = sequenceDirection;
                right = sequenceRight;
                startedAt = startTime;
                LaunchDuration = Mathf.Max(.01f, launchDuration);
                TravelDistance = Mathf.Max(.1f, travelDistance);
                var tuning = GojoPolishSettings.Current;
                MergeDuration = tuning.purpleFusionDuration;
                formationScale = tuning.purpleFormationScale;
                formationSeparation = tuning.purpleFormationSeparation;
                fusionStart = tuning.PurpleFusionStart;
                holdStart = tuning.PurpleHoldStart;
                releaseTime = tuning.PurpleReleaseTime;
                blueLeadDuration = tuning.purpleBlueLeadDuration;
                residueDuration = Mathf.Max(.01f, tuning.purpleScarDuration);
                fusionHoldDuration = Mathf.Max(0f, tuning.purpleFusionHoldDuration);
                visualScale = Mathf.Max(.1f, tuning.purpleVisualScale);
                holdOffset = Vector3.up * tuning.purpleHoldOffset.y
                    + direction * tuning.purpleHoldOffset.z;

                root = new GameObject("HollowPurpleCanonicalOrbSequence");
                root.transform.SetParent(runtimeRoot, true);
                aftermath = root.AddComponent<PurpleTravelAftermath>();
                aftermath.Configure(start + holdOffset, direction, GojoPolishSettings.PurpleShellDiameter * visualScale);

                blueOrb = new GameObject("HollowPurpleBlueOrbRoot").transform;
                blueOrb.SetParent(root.transform, true);
                blueField = blueOrb.gameObject.AddComponent<PurpleFusionIngredient>();
                blueField.Configure(false);
                redOrb = new GameObject("HollowPurpleRedOrbRoot").transform;
                redOrb.SetParent(root.transform, true);
                redField = redOrb.gameObject.AddComponent<PurpleFusionIngredient>();
                redField.Configure(true);

                purpleOrb = new GameObject("HollowPurpleDenseBody").transform;
                purpleOrb.SetParent(root.transform, true);
                purpleOrb.rotation = Quaternion.LookRotation(direction, Vector3.up);
                energyBody = purpleOrb.gameObject.AddComponent<PurpleEnergyBody>();
                energyBody.Configure(direction * (TravelDistance / LaunchDuration), true);
                if (!energyBody.UsesProductionCandidate)
                {
                warpedWake = ProductionSignatureVfxFactory.CreateSphere(purpleOrb,
                    "PurpleWarpedSpaceWake", Vector3.zero, 1f, Color.white, runtimeMaterials, materialColors, 0f);
                Shader wakeShader = Resources.Load<Shader>("VFX/GojoBlueDistortion");
                wakeMaterial = warpedWake.GetComponent<Renderer>().sharedMaterial;
                if (wakeShader != null) wakeMaterial.shader = wakeShader;
                wakeMaterial.SetFloat("_WorldRadius", 3.8f * visualScale);
                wakeMaterial.SetFloat("_RadialSign", -1f);
                wakeMaterial.SetFloat("_Strength", 0f);
                warpedWake.gameObject.SetActive(false);
                }
                releasePresentation = root.AddComponent<PurpleReleasePresentation>();
                releasePresentation.Configure(start + holdOffset, direction, releaseTime, presentationCamera);
                fusionFront = ProductionSignatureVfxFactory.CreateArc(
                    root.transform, "FusionPressureFront", .5f, 17f, 256f, .11f,
                    new Color(2.2f, .7f, 3.1f, .75f), true, runtimeMaterials, materialColors);
                fusionFront.sharedMaterial.shader = Resources.Load<Shader>("VFX/HollowPurpleFilament");
                fusionFront.transform.SetPositionAndRotation(start, Quaternion.LookRotation(direction));
                fusionFront.gameObject.SetActive(false);
                if (!energyBody.UsesProductionCandidate)
                {
                purpleFragments = ProductionSignatureVfxFactory.CreateParticleSystem(
                    purpleOrb, "PurpleOrbitFragments", new Color(0.86f, 0.18f, 1f, 0.62f),
                    runtimeMaterials, materialColors, true, LaunchDuration, 0.12f, 0.24f,
                    0.18f, 0.65f, 0.035f, 0.075f, ParticleSystemShapeType.Sphere,
                    1.72f, false, ParticleSystemSimulationSpace.World, 16, 24f
                );
                purpleTrail = ProductionSignatureVfxFactory.CreateParticleSystem(
                    purpleOrb, "PurpleShortResidualTrail", new Color(0.48f, 0.025f, 0.82f, 0.58f),
                    runtimeMaterials, materialColors, true, LaunchDuration, 0.26f, 0.44f,
                    1.5f, 3.8f, 0.055f, 0.12f, ParticleSystemShapeType.Cone,
                    0.42f, true, ParticleSystemSimulationSpace.World, 8, 30f
                );
                purpleTrail.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                ParticleSystem.ShapeModule trailShape = purpleTrail.shape;
                trailShape.angle = 10f;
                trailShape.length = 0.18f;
                }

                GameObject lightObject = new GameObject("HollowPurpleOrbLight");
                lightObject.transform.SetParent(purpleOrb, false);
                purpleLight = lightObject.AddComponent<Light>();
                purpleLight.type = LightType.Point;
                purpleLight.color = new Color(0.58f, 0.06f, 1f);
                purpleLight.range = 8f;
                purpleLight.intensity = 4.2f;
                purpleLight.shadows = LightShadows.None;

                purpleOrb.gameObject.SetActive(false);
                blueOrb.position = start + Vector3.up * .4f - right * formationSeparation;
                redOrb.position = start + Vector3.up * .4f + right * formationSeparation;
                blueOrb.localScale = Vector3.one * 0.92f;
                redOrb.localScale = Vector3.one * 0.92f;

                redOrb.gameObject.SetActive(false);
                blueOrb.localScale = Vector3.one * .08f;
            }

            public void BindCaster(Transform owner)
            {
                if (casterChain != null) casterChain.PurpleTerminated -= OnTerminated;
                caster = owner; boundCaster = true;
                casterChain = owner != null ? owner.GetComponent<GojoTechniqueChainController>() : null;
                casterHealth = owner != null ? owner.GetComponent<JJKGame.Core.Health>() : null;
                if (casterChain != null) casterChain.PurpleTerminated += OnTerminated;
            }

            private void OnTerminated(Vector3 position, float at, float castStartedAt)
            {
                if (terminal != null || root == null || Mathf.Abs(castStartedAt - startedAt) > .001f) return;
                terminalAt = Mathf.Max(releaseTime, at - startedAt);
                terminalPosition = position;
            }

            public bool Update(float now, float unscaledDeltaTime)
            {
                if (root == null || (boundCaster && (caster == null || !caster.gameObject.activeInHierarchy
                    || (casterChain != null && !casterChain.isActiveAndEnabled)
                    || (casterHealth != null && casterHealth.IsDead))))
                {
                    return false;
                }

                float elapsed = Mathf.Max(0, now - startedAt);
                releasePresentation.Sample(elapsed);
                if (elapsed < fusionStart)
                {
                    UpdateFormation(elapsed);
                    return true;
                }
                if (elapsed < holdStart)
                {
                    UpdateMerge((elapsed - fusionStart) / MergeDuration, unscaledDeltaTime);
                    return true;
                }

                float holdElapsed = elapsed - holdStart;
                if (elapsed < releaseTime)
                {
                    blueOrb.gameObject.SetActive(false);
                    redOrb.gameObject.SetActive(false);
                    purpleOrb.gameObject.SetActive(true);
                    purpleOrb.position = start + holdOffset;
                    purpleOrb.localScale = Vector3.one * visualScale * 1.12f
                        * (1f + .012f * Mathf.Sin(holdElapsed * 9f));
                    float h = Mathf.Clamp01(holdElapsed / Mathf.Max(.001f, fusionHoldDuration));
                    float tension = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2.6f / 3f, 1f, h));
                    float activity = Mathf.SmoothStep(.15f, 1f, Mathf.InverseLerp(.8f / 3f, 2.6f / 3f, h)) * (1f - tension * .65f);
                    float flowClock = Mathf.Min(elapsed, holdStart + fusionHoldDuration - .035f);
                    energyBody.Render(flowClock, Mathf.Lerp(.85f, 1.5f, h), Mathf.Lerp(1.0f, .42f, tension),
                        Mathf.Lerp(.65f, 1.2f, activity), -1f, false, activity);
                    if (warpedWake != null)
                    {
                    warpedWake.gameObject.SetActive(true);
                    warpedWake.localScale = Vector3.one * 5.8f;
                    wakeMaterial.SetFloat("_Strength", Mathf.Lerp(.10f, .32f, activity) * (1f - tension * .75f));
                    }
                    purpleLight.intensity = Mathf.Lerp(2.2f, .7f, tension);
                    fusionFront.gameObject.SetActive(false);
                    return true;
                }
                float launchElapsed = elapsed - releaseTime;
                if (elapsed < Mathf.Min(terminalAt, releaseTime + LaunchDuration))
                {
                    UpdateLaunch(launchElapsed / LaunchDuration, launchElapsed, unscaledDeltaTime);
                    return true;
                }

                if (terminal == null)
                {
                    if (float.IsPositiveInfinity(terminalAt))
                    { terminalAt = releaseTime + LaunchDuration; terminalPosition = start + holdOffset + direction * TravelDistance; }
                    // Fill the final corridor segment before stopping the travel scar.
                    aftermath.Render(terminalPosition, launchElapsed, 1f, true, GojoPolishSettings.PurpleShellDiameter * visualScale * 1.12f);
                    var burst = new GameObject("HollowPurpleTerminalBurst"); burst.transform.SetParent(root.transform, false);
                    terminal = burst.AddComponent<PurpleTerminalBurst>();
                    terminal.Configure(terminalPosition, direction, visualScale * 1.12f);
                }
                float terminalAge = elapsed - terminalAt;
                terminal.Render(terminalAge);
                if (terminalAge <= Mathf.Max(residueDuration, GojoPolishSettings.Current.purpleTerminalDuration))
                {
                    blueOrb.gameObject.SetActive(false);
                    redOrb.gameObject.SetActive(false);
                    purpleOrb.gameObject.SetActive(false);
                    fusionFront.gameObject.SetActive(false);
                    purpleFragments?.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                    purpleTrail?.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                    float fade = 1f - Mathf.Clamp01(terminalAge / residueDuration);
                    aftermath.Render(terminalPosition, launchElapsed, fade, false);
                    for (int i = 0; i < runtimeMaterials.Count; i++)
                    {
                        Color tint = materialColors[i];
                        tint.a *= fade;
                        ProductionSignatureVfxFactory.SetMaterialColor(runtimeMaterials[i], tint);
                        if (runtimeMaterials[i].HasProperty("_Opacity"))
                            runtimeMaterials[i].SetFloat("_Opacity", tint.a);
                    }
                    purpleLight.intensity = 2f * fade;
                    return true;
                }

                return false;
            }

            private void UpdateFormation(float elapsed)
            {
                purpleOrb.gameObject.SetActive(false);
                blueOrb.gameObject.SetActive(true);
                blueOrb.position = start + Vector3.up * .4f - right * formationSeparation;
                blueOrb.localScale = Vector3.one * Mathf.Lerp(.08f, formationScale,
                    Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(.01f, blueLeadDuration * .7f)));
                bool showRed = elapsed >= blueLeadDuration;
                redOrb.gameObject.SetActive(showRed);
                redOrb.position = start + Vector3.up * .4f + right * formationSeparation;
                if (showRed)
                    redOrb.localScale = Vector3.one * Mathf.Lerp(.08f, formationScale,
                        Mathf.SmoothStep(0f, 1f, (elapsed - blueLeadDuration) / Mathf.Max(.01f, (fusionStart-blueLeadDuration)*.65f)));
                blueField.Render(elapsed, 0, redOrb.position);
                if (showRed) redField.Render(elapsed-blueLeadDuration, 0, blueOrb.position);
            }

            private void UpdateMerge(float normalized, float unscaledDeltaTime)
            {
                blueOrb.gameObject.SetActive(true);
                redOrb.gameObject.SetActive(true);
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized));
                float separation = Mathf.Lerp(formationSeparation, 0.03f, t);
                float arc = Mathf.Sin(t * Mathf.PI) * 0.45f;

                blueOrb.position = start - right * separation + Vector3.up * arc;
                redOrb.position = start + right * separation - Vector3.up * arc * 0.35f;
                Vector3 completedOffset = Vector3.Lerp(Vector3.up*.4f, holdOffset, t);
                blueOrb.position += completedOffset;
                redOrb.position += completedOffset;

                float scale = formationScale * (1f + Mathf.Sin(t*Mathf.PI)*.12f) * Mathf.Lerp(1f, .04f, Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1f,t)));
                blueOrb.localScale = Vector3.one * scale;
                redOrb.localScale = Vector3.one * scale;

                // Violet contamination grows only in the last part of the existing fusion.
                float contamination = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.52f, 1f, normalized));
                purpleOrb.gameObject.SetActive(contamination > 0f);
                if (contamination > 0f)
                {
                    purpleOrb.position = start + completedOffset;
                    purpleOrb.localScale = Vector3.one * visualScale * 1.12f * contamination;
                    energyBody.Render(fusionStart + normalized * MergeDuration, .55f + contamination * .3f, .7f, 1.6f, -1f, false);
                }
                blueField.Render(fusionStart + normalized * MergeDuration, t, redOrb.position);
                redField.Render(fusionStart + normalized * MergeDuration - blueLeadDuration, t, blueOrb.position);
            }

            private void UpdateLaunch(float normalized, float launchElapsed, float unscaledDeltaTime)
            {
                if (!launched)
                {
                    launched = true;
                    blueOrb.gameObject.SetActive(false);
                    redOrb.gameObject.SetActive(false);
                    purpleOrb.gameObject.SetActive(true);
                    purpleOrb.position = start + holdOffset;
                    purpleFragments?.Play(true);
                    purpleTrail?.Play(true);
                }

                fusionFront.gameObject.SetActive(launchElapsed < 0.18f);
                fusionFront.transform.position = start + holdOffset;
                fusionFront.transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 11f,
                    1f - Mathf.Pow(1f - Mathf.Clamp01(launchElapsed / 0.22f), 3f));
                Color frontTint = Color.white;
                frontTint.a = 1f - Mathf.Clamp01(launchElapsed / 0.22f);
                fusionFront.startColor = frontTint;
                fusionFront.endColor = frontTint;

                float t = Mathf.Clamp01(normalized);
                float travel = TravelDistance * t;
                // The completed hold position is the release origin, shared by gameplay.
                purpleOrb.position = start + holdOffset + direction * travel;
                if (warpedWake != null)
                {
                warpedWake.gameObject.SetActive(true);
                warpedWake.localPosition = Vector3.back * 1.2f;
                warpedWake.localScale = new Vector3(5.8f, 5.8f, 8.2f);
                wakeMaterial.SetFloat("_Strength", .48f + .13f * Mathf.Sin(launchElapsed * 17f));
                wakeMaterial.SetFloat("_Impact", .65f);
                }

                float ignition = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(launchElapsed / 0.09f)
                );
                float growth = fusionHoldDuration > 0f ? 1.12f : Mathf.Lerp(0.34f, 1.12f, ignition);
                float pulse = 1f + Mathf.Sin(launchElapsed * 20f) * 0.035f;
                purpleOrb.localScale = Vector3.one * growth * pulse * visualScale;
                aftermath.Render(purpleOrb.position, launchElapsed, 1f, true,
                    GojoPolishSettings.PurpleShellDiameter * purpleOrb.lossyScale.x);
                float peak = 1f + (GojoPolishSettings.Current.purpleReleaseEmission - 1f)
                    * Mathf.Exp(-Mathf.Max(0f, launchElapsed - .033f) * 18f);
                // Freeze energy flow for two nominal 60 Hz frames; projectile position and damage clock continue.
                float flowClock = holdStart + fusionHoldDuration - .035f + Mathf.Max(0f, launchElapsed - .033f);
                energyBody.Render(flowClock, 1.12f, peak, 1.15f, launchElapsed, true);

                if (purpleLight != null)
                {
                    purpleLight.intensity =
                        (4.2f + Mathf.Sin(launchElapsed * 18f) * 0.55f) * peak;
                    purpleLight.range = Mathf.Lerp(7f, 10f, t);
                }
            }

            public void Dispose()
            {
                if (casterChain != null) casterChain.PurpleTerminated -= OnTerminated;
                if (releasePresentation != null) releasePresentation.Restore();
                if (root != null)
                {
                    root.SetActive(false);
                    Object.Destroy(root);
                }

                foreach (Material material in runtimeMaterials)
                {
                    if (material != null)
                    {
                        Object.Destroy(material);
                    }
                }
                runtimeMaterials.Clear();
                materialColors.Clear();
            }
        }
    }
}
