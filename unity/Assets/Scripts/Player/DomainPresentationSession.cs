using System.Collections.Generic;
using JJKGame.CameraSystem;
using JJKGame.Core;
using JJKGame.Dev.VFXLab;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace JJKGame.Player
{
    /// <summary>Owns one reversible presentation transaction. Capture and stun remain in gameplay.</summary>
    [DefaultExecutionOrder(1700)]
    public sealed class DomainPresentationSession : MonoBehaviour
    {
        private readonly DomainInteriorSpace interior = new DomainInteriorSpace();
        private readonly List<Transform> participants = new List<Transform>();
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Color> colors = new List<Color>();
        private DomainPresentationSettings settings;
        private UnlimitedVoidProductionVisual visual;
        private Transform caster, victim, previewAnchor, barrier;
        private Transform resolvedHand, resolvedFace, resolvedEye, victimHead;
        private float actorScale = 1f;
        private Health casterHealth;
        private Camera view;
        private DomainCameraOverride lease;
        private GameObject overlayRoot;
        private RawImage blackout;
        private RenderTexture releaseFrame;
        private Material releaseMaterial;
        private bool worldRestored, releasing;
        private Vector3 releaseTranslation;
        private UniversalAdditionalCameraData releaseCameraData;
        private bool previousDepthTexture;
        public Vector3 ReleaseOrigin { get; private set; }
        private Light detailLight;
        private Vector3 cameraPosition, anchorPosition;
        private Quaternion cameraRotation;
        private float cameraFov, elapsed, lifetime, details, diameter;
        private bool running, ownsCamera, cameraWasAcquired;
        private string shot;
        private string authoredShot;
        private static int nextToken;
        public int CastToken { get; private set; }
        public bool FullCinematic => victim != null;
        public bool IsEntered => interior.IsEntered;
        public float EffectiveBarrierDiameter => diameter;
        public bool Paused { get; set; }
        public event System.Action Ended;
        public bool IsReleasing => releasing;
        public bool IsRunning => running;
        public bool IsActiveGameplay =>
            running
            && !Paused
            && interior.IsEntered
            && !releasing
            && elapsed >= settings.ArrivalDuration(FullCinematic);
        public float ReleaseProgress { get; private set; }

        public static Transform SelectVictim(Transform caster, IReadOnlyList<Transform> captured, Health locked)
        {
            Transform best = null;
            float distance = float.PositiveInfinity;
            if (captured == null || caster == null) return null;
            foreach (Transform candidate in captured)
            {
                if (candidate == null || candidate == caster || !candidate.gameObject.activeInHierarchy) continue;
                Health health = candidate.GetComponent<Health>();
                if (health != null && health.IsDead) continue;
                if (locked != null && (candidate == locked.transform || candidate.IsChildOf(locked.transform)))
                    return candidate;
                float d = (candidate.position - caster.position).sqrMagnitude;
                if (d < distance || (Mathf.Approximately(d, distance) &&
                    (best == null || string.CompareOrdinal(StablePath(candidate), StablePath(best)) < 0)))
                { best = candidate; distance = d; }
            }
            return best;
        }

        private static string StablePath(Transform actor)
        {
            string path = actor.GetSiblingIndex().ToString("D6");
            while (actor.parent != null) { actor = actor.parent; path = actor.GetSiblingIndex().ToString("D6") + "/" + path; }
            return actor.gameObject.scene.path + "/" + path;
        }

        public void Begin(Transform owner, IReadOnlyList<Transform> captured, UnlimitedVoidProductionVisual environment,
            DomainPresentationSettings tuning, float duration, Transform inspectionAnchor = null)
        {
            End();
            if (owner == null || environment == null) return;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            settings = tuning ?? new DomainPresentationSettings();
            caster = owner;
            casterHealth = caster.GetComponent<Health>();
            visual = environment;
            previewAnchor = inspectionAnchor;
            if (previewAnchor != null) anchorPosition = previewAnchor.position;
            if (captured != null) foreach (Transform actor in captured)
                if (actor != null && actor != caster && !participants.Contains(actor)) participants.Add(actor);
            var targetLock = caster.GetComponent<TargetLockController>();
            victim = SelectVictim(caster, participants, targetLock != null ? targetLock.CurrentTarget : null);
            resolvedHand = settings.handAnchor != null ? settings.handAnchor : FindBone(caster, HumanBodyBones.RightHand);
            resolvedFace = settings.faceAnchor != null ? settings.faceAnchor : FindBone(caster, HumanBodyBones.Head);
            resolvedEye = settings.eyeAnchor != null ? settings.eyeAnchor : FindBone(caster, HumanBodyBones.LeftEye);
            victimHead = victim != null ? FindBone(victim, HumanBodyBones.Head) : null;
            actorScale = resolvedFace != null
                ? Mathf.Clamp(Vector3.Dot(resolvedFace.position - caster.position, caster.up) / 1.65f, .5f, 4f) : 1f;
            details = settings.DetailDuration(FullCinematic);
            lifetime = duration;
            elapsed = 0f;
            worldRestored = releasing = false;
            ReleaseProgress = 0f;
            CastToken = ++nextToken;
            running = true;
            shot = null;
            authoredShot = null;
            visual.SetPresentationDelay(details + settings.blackTransitionDuration);
            BuildBarrier();
            view = Camera.main;
            if (view != null)
            {
                cameraPosition = view.transform.position;
                cameraRotation = view.transform.rotation;
                cameraFov = view.fieldOfView;
                lease = view.GetComponent<DomainCameraOverride>() ?? view.gameObject.AddComponent<DomainCameraOverride>();
                ownsCamera = settings.cinematicEnabled && lease.Acquire(this);
                cameraWasAcquired = ownsCamera;
                BuildBlackout();
            }
            Tick(0f);
        }

        private static Transform FindBone(Transform actor, HumanBodyBones bone)
        {
            Animator animator = actor.GetComponentInChildren<Animator>();
            if (animator != null && animator.isHuman && animator.avatar != null)
            {
                Transform mapped = animator.GetBoneTransform(bone);
                if (mapped != null) return mapped;
            }
            // Imported Generic Mixamo rigs still provide exact named transforms; never alter their pose.
            string suffix = bone.ToString();
            foreach (Transform child in actor.GetComponentsInChildren<Transform>())
                if (child.name == suffix || child.name.EndsWith(":" + suffix, System.StringComparison.Ordinal)) return child;
            return null;
        }

        private Vector3 CasterPoint(Vector3 offset) => caster.position + caster.rotation * (offset * actorScale);

        // Animation adapters retain CastToken from Begin. Cues never fire gameplay or postpone restoration.
        public bool TryAuthoredDetailCue(int token, string cue)
        {
            if (!running || token != CastToken || elapsed >= details) return false;
            if (cue != "HAND" && cue != "FACE" && cue != "SIX EYES" && cue != "HERO") return false;
            authoredShot = cue;
            return true;
        }

        private void BuildBarrier()
        {
            Vector3 center = caster.position + Vector3.up;
            diameter = Mathf.Max(0.1f, settings.barrierVisualDiameterMeters);
            if (settings.fitBarrierToCapturedParticipants)
            {
                var bounds = new Bounds(center, Vector3.one * 2f);
                foreach (Transform actor in participants) if (actor != null) bounds.Encapsulate(actor.position + Vector3.up);
                center = bounds.center;
                diameter = Mathf.Max(diameter, bounds.extents.magnitude * 2f + 2f);
            }
            barrier = ProductionSignatureVfxFactory.CreateSphere(transform, "OriginalWorld_BlackBarrier",
                Vector3.zero, 0.01f, Color.black, materials, colors, 0f,
                UnityEngine.Rendering.CullMode.Off, true);
            barrier.position = center;
            barrier.SetParent(null, true); // must remain in original world while the caster travels
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(barrier.gameObject, caster.gameObject.scene);
        }

        private void BuildBlackout()
        {
            overlayRoot = new GameObject("DomainTransitionOcclusion", typeof(Canvas));
            overlayRoot.transform.SetParent(transform, false);
            var canvas = overlayRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var image = new GameObject("Black", typeof(RectTransform), typeof(RawImage));
            image.transform.SetParent(overlayRoot.transform, false);
            blackout = image.GetComponent<RawImage>();
            blackout.raycastTarget = false;
            blackout.color = Color.clear;
            blackout.rectTransform.anchorMin = Vector2.zero;
            blackout.rectTransform.anchorMax = Vector2.one;
            blackout.rectTransform.offsetMin = blackout.rectTransform.offsetMax = Vector2.zero;
            var lightHost = new GameObject("CinematicFaceAccent", typeof(Light));
            lightHost.transform.SetParent(overlayRoot.transform, false);
            detailLight = lightHost.GetComponent<Light>();
            detailLight.type = LightType.Point;
            detailLight.color = new Color(0.55f, 0.72f, 1f);
            detailLight.range = 2.5f * actorScale;
            detailLight.shadows = UnityEngine.LightShadows.None;
        }

        private void LateUpdate()
        {
            if (!running) return;
            if (ownsCamera && view != null && view.GetComponent<VfxLabOrbitCamera>() != null && Input.GetMouseButtonDown(2))
            { lease.Release(this); ownsCamera = false; }
            if (caster == null || !caster.gameObject.activeInHierarchy || (casterHealth != null && casterHealth.IsDead))
            { End(); return; }
            if (!Paused) elapsed += Time.deltaTime;
            Tick(elapsed);
        }

        private void Tick(float time)
        {
            if (time >= lifetime) { End(); return; }
            float releaseDuration = Mathf.Min(settings.barrierReleaseDuration, Mathf.Max(.01f, lifetime * .25f));
            float releaseAt = lifetime - releaseDuration;
            if (releasing || time >= releaseAt)
            {
                if (!releasing) BeginWorldRelease();
                ReleaseProgress = Mathf.Max(ReleaseProgress, Mathf.Clamp01((time - releaseAt) / releaseDuration));
                if (releaseMaterial != null)
                {
                    RenderReleaseInterior();
                    releaseMaterial.SetFloat("_Progress", ReleaseProgress);
                    releaseMaterial.SetVector("_ReleaseOrigin", ReleaseOrigin);
                    releaseMaterial.SetFloat("_ReleaseRadius", ReleaseProgress * settings.interiorRadius);
                    releaseMaterial.SetMatrix("_WorldFromClip", (GL.GetGPUProjectionMatrix(view.projectionMatrix, false)
                        * view.worldToCameraMatrix).inverse);
                }
                else if (blackout != null) blackout.color = new Color(0f, 0f, 0f, 1f - ReleaseProgress);
                return;
            }
            float closeAt = details + settings.barrierCloseDuration;
            float tunnelAt = closeAt + settings.blackTransitionDuration;
            float closure = Mathf.Clamp01((time - details) / Mathf.Max(0.01f, settings.barrierCloseDuration));
            if (barrier != null)
            {
                barrier.localScale = Vector3.one * Mathf.Lerp(0.01f, diameter, Mathf.SmoothStep(0f, 1f, closure));
                ProductionSignatureVfxFactory.SetMaterialColor(materials[0],
                    new Color(0f, 0f, 0f, settings.barrierOpacity * closure), 0f);
            }
            if (time >= closeAt && !interior.IsEntered)
            {
                interior.Enter(caster, participants, settings.interiorOrigin, settings.interiorRadius);
                Vector3 delta = interior.Translation;
                visual.AnchorAt(caster.position, caster.rotation);
                if (previewAnchor != null)
                {
                    previewAnchor.position += delta;
                    caster.GetComponent<VfxLabPreviewCharacter>()?.SetTechniqueAnchor(previewAnchor.position);
                }
                ShiftCamera(delta);
            }
            float occlusion = time < closeAt ? Mathf.SmoothStep(0f, 1f, (closure - 0.65f) / 0.35f)
                : 1f - Mathf.Clamp01((time - tunnelAt) / 0.08f);
            if (blackout != null) blackout.color = new Color(0f, 0f, 0f, occlusion);
            if (detailLight != null)
            {
                detailLight.transform.position = CasterPoint(new Vector3(0.5f, 1.8f, 0.8f));
                detailLight.intensity = ownsCamera && time < details ? 0.8f : 0f;
            }
            if (!ownsCamera || view == null) return;
            float arrivalAt = tunnelAt + visual.TunnelDuration;
            float reactionEnd = arrivalAt + (FullCinematic ? settings.victimShotDuration : 0f);
            float returnAt = reactionEnd + 0.20f;
            string next = "HAND";
            Transform subject = caster;
            Vector3 target = resolvedHand != null ? resolvedHand.position : CasterPoint(settings.handOffset);
            Vector3 position = target + caster.TransformDirection(settings.detailCameraOffset) * actorScale;
            float fov = settings.detailFov;
            if (time < details)
            {
                if (FullCinematic)
                    next = time < settings.handShotDuration ? "HAND"
                        : time < settings.handShotDuration + settings.faceShotDuration ? "FACE"
                        : time < details - settings.heroShotDuration ? "SIX EYES" : "HERO";
                else next = time < settings.handShotDuration ? "HAND" : "HERO";
                next = authoredShot ?? next;
                if (next == "FACE" || next == "SIX EYES")
                {
                    Transform socket = next == "SIX EYES" && resolvedEye != null ? resolvedEye : resolvedFace;
                    target = socket != null ? socket.position : CasterPoint(settings.faceOffset);
                    position = target + caster.TransformVector(next == "FACE"
                        ? new Vector3(0.65f, 0.02f, 0.28f) : new Vector3(0.05f, 0f, 0.43f)) * actorScale;
                    fov = next == "SIX EYES" ? settings.eyeFov : settings.detailFov;
                }
                else if (next == "HERO")
                { target = CasterPoint(Vector3.up * 1.2f); position = CasterPoint(settings.heroCameraOffset); fov = 45f; }
            }
            else if (time < tunnelAt)
            {
                next = "BARRIER";
                target = barrier.position;
                position = target + caster.TransformDirection(new Vector3(0.65f, 0.4f, -1f).normalized) * diameter * 1.25f;
                fov = settings.wideFov;
            }
            else if (time < arrivalAt)
            {
                next = "TUNNEL";
                subject = victim != null ? victim : caster;
                target = subject.position + Vector3.up * 1.15f;
                position = target + caster.TransformDirection(new Vector3(2.4f, 1.2f, -4.8f));
                fov = settings.wideFov;
            }
            else
            {
                next = time < reactionEnd && FullCinematic ? "DomainOverwhelmed" : "ARRIVAL";
                subject = victim != null ? victim : caster;
                target = subject == victim && victimHead != null ? victimHead.position
                    : subject.position + Vector3.up * (subject == caster ? 1.55f * actorScale : 1.55f);
                position = target + subject.TransformDirection(new Vector3(0.35f, 0.12f, 1.8f));
                fov = next == "DomainOverwhelmed" ? 35f : settings.wideFov;
                if (next == "ARRIVAL")
                { target = caster.position + Vector3.up * 1.5f; position = caster.position + new Vector3(4f, 3.5f, -6f); }
            }
            if (time < details)
            {
                // Small prototype rigs can put the detail camera's near plane through the face.
                // Keep the existing camera settings/lease; move only the close-up shot back.
                Vector3 offset = position - target;
                float minimumDistance = view.nearClipPlane + actorScale * .75f;
                if (offset.sqrMagnitude < minimumDistance * minimumDistance)
                    position = target + offset.normalized * minimumDistance;
            }
            Quaternion rotation = Quaternion.LookRotation(target - position, Vector3.up);
            if (time >= returnAt)
            {
                next = "RETURN";
                float blend = Mathf.SmoothStep(0f, 1f, (time - returnAt) / Mathf.Max(0.01f, settings.cameraReturnDuration));
                position = Vector3.Lerp(position, lease.ReturnPosition, blend);
                rotation = Quaternion.Slerp(rotation, lease.ReturnRotation, blend);
                fov = Mathf.Lerp(fov, lease.ReturnFov, blend);
                if (blend >= 1f) { lease.Release(this); ownsCamera = false; return; }
            }
            if (shot != next) { shot = next; settings.onShot?.Invoke(next); }
            lease.Pose(this, position, rotation, fov);
        }

        private void ShiftCamera(Vector3 delta)
        {
            if (view == null) return;
            view.transform.position += delta;
            view.GetComponent<SimpleCameraFollow>()?.TranslatePresentationSpace(delta);
            view.GetComponent<VfxLabOrbitCamera>()?.TranslatePresentationSpace(delta);
            if (ownsCamera) lease.ShiftReturn(delta);
        }

        private void BeginWorldRelease()
        {
            releasing = true;
            releaseTranslation = interior.Translation;
            Vector3 visualPosition = visual.transform.position;
            Quaternion visualRotation = visual.transform.rotation;
            RestoreWorld();
            ReleaseOrigin = caster.position;
            // Keep the existing isolated environment live; only its owning session renders it.
            visual.AnchorAt(visualPosition, visualRotation);
            if (barrier != null) barrier.gameObject.SetActive(false);
            if (detailLight != null) detailLight.intensity = 0f;
            Shader shader = Resources.Load<Shader>("VFX/UnlimitedVoidRelease");
            if (view == null || blackout == null || shader == null ||
                SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            releaseCameraData = view.GetUniversalAdditionalCameraData();
            previousDepthTexture = releaseCameraData.requiresDepthTexture;
            releaseCameraData.requiresDepthTexture = true;
            releaseFrame = new RenderTexture(Mathf.Clamp(view.pixelWidth, 16, 1920),
                Mathf.Clamp(view.pixelHeight, 16, 1080), 24, RenderTextureFormat.ARGB32)
                { name = "DomainReleaseInteriorFrame" };
            releaseFrame.Create();
            releaseMaterial = new Material(shader) { name = "DomainCasterWorldRelease" };
            blackout.texture = releaseFrame;
            blackout.material = releaseMaterial;
            blackout.color = Color.white;
        }

        private void RenderReleaseInterior()
        {
            if (view == null || releaseFrame == null) return;
            Vector3 position = view.transform.position;
            bool overlayEnabled = blackout != null && blackout.enabled;
            var renderers = new List<Renderer>();
            var visibility = new List<bool>();
            SaveActorVisibility(caster, renderers, visibility);
            foreach (Transform actor in participants) SaveActorVisibility(actor, renderers, visibility);
            try
            {
                if (blackout != null) blackout.enabled = false;
                view.transform.position = position + releaseTranslation;
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(view,
                    new UniversalRenderPipeline.SingleCameraRequest { destination = releaseFrame });
            }
            finally
            {
                if (blackout != null) blackout.enabled = overlayEnabled;
                view.transform.position = position;
                for (int i = 0; i < renderers.Count; i++)
                    if (renderers[i] != null) renderers[i].enabled = visibility[i];
            }
        }

        private static void SaveActorVisibility(Transform actor, List<Renderer> renderers, List<bool> visibility)
        {
            if (actor == null) return;
            foreach (Renderer renderer in actor.GetComponentsInChildren<Renderer>())
            {
                // The gameplay visual root is a caster child, but belongs in the snapshot.
                if (renderer.GetComponentInParent<UnlimitedVoidProductionVisual>() != null) continue;
                if (renderers.Contains(renderer)) continue;
                renderers.Add(renderer); visibility.Add(renderer.enabled); renderer.enabled = false;
            }
        }

        private void RestoreWorld()
        {
            if (worldRestored) return;
            worldRestored = true;
            if (ownsCamera && lease != null) lease.Release(this);
            ownsCamera = false;
            if (interior.IsEntered) ShiftCamera(-interior.Translation);
            interior.Exit();
            if (previewAnchor != null)
            {
                previewAnchor.position = anchorPosition;
                if (caster != null) caster.GetComponent<VfxLabPreviewCharacter>()?.SetTechniqueAnchor(anchorPosition);
            }
            if (view != null && (cameraWasAcquired || !DomainCameraOverride.IsOwned(view)))
            { view.transform.SetPositionAndRotation(cameraPosition, cameraRotation); view.fieldOfView = cameraFov; }
            cameraWasAcquired = false;
        }

        public void End()
        {
            if (!running) return;
            running = false;
            RestoreWorld();
            if (ownsCamera && lease != null) lease.Release(this);
            ownsCamera = false;
            releasing = false;
            if (barrier != null) { barrier.gameObject.SetActive(false); Destroy(barrier.gameObject); }
            if (overlayRoot != null) { overlayRoot.SetActive(false); Destroy(overlayRoot); }
            if (releaseCameraData != null) releaseCameraData.requiresDepthTexture = previousDepthTexture;
            releaseCameraData = null;
            if (releaseFrame != null) { releaseFrame.Release(); Destroy(releaseFrame); releaseFrame = null; }
            if (releaseMaterial != null) { Destroy(releaseMaterial); releaseMaterial = null; }
            foreach (Material material in materials) if (material != null) Destroy(material);
            materials.Clear(); colors.Clear(); participants.Clear();
            if (visual != null) visual.gameObject.SetActive(false);
            Ended?.Invoke();
        }

        private void OnDisable() => End();
        private void OnDestroy() => End();
    }
}
