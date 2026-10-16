using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Dev.VFXLab;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace JJKGame.EditorTools
{
    [DefaultExecutionOrder(2000)]
    internal sealed class TrackAAttackPoseProbe : MonoBehaviour
    {
        public Transform Arm { get; set; }
        public float MaxAngle { get; private set; }

        private void LateUpdate()
        {
            if (Arm != null)
            {
                MaxAngle = Mathf.Max(MaxAngle,
                    Quaternion.Angle(Arm.localRotation, Quaternion.identity));
            }
        }
    }

    public sealed class TrackAStabilizationTests
    {
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            ProductionCombatInput.MoveReplay = null;
            ProductionCombatInput.BasicAttackReplay = null;
            ProductionCombatInput.DodgeReplay = null;
            ProductionCombatInput.DomainReplay = null;
            ProductionCombatInput.DomainModifierPressedReplay = null;
            ProductionCombatInput.DomainModifierHeldReplay = null;
            ProductionCombatInput.DomainModifierReleasedReplay = null;
            if (UnityEditor.EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();
            }
        }

        [UnityTest]
        public IEnumerator SelectedGojoDomainActiveGameplayAllowsMovementComboAndDodge()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return VerifySelectedGojoDomainActiveGameplayPath();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifySelectedGojoDomainActiveGameplayPath()
        {
            MatchTeamSelectionStore.SetPlayerTeam(
                MatchTeamSelection.Solo(PrototypeCharacterId.GojoModern)
            );
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/CombatMVP.unity",
                new LoadSceneParameters(LoadSceneMode.Single)
            );
            yield return Wait(0.5f);
            GameObject player = GameObject.Find("GojoPlayer");
            Assert.That(player != null, Is.True, "CombatMVP Gojo player did not stabilize");
            GojoDomainController domain = player.GetComponent<GojoDomainController>();
            BasicAttack attack = player.GetComponent<BasicAttack>();
            ThirdPersonPlayerController movement = player.GetComponent<ThirdPersonPlayerController>();
            CombatActionGate gate = player.GetComponent<CombatActionGate>();
            Assert.That(domain, Is.Not.Null);
            Assert.That(attack, Is.Not.Null);
            Assert.That(movement, Is.Not.Null);
            Assert.That(gate, Is.Not.Null);

            Set(domain, "domainEnergyCost", 0f);
            Set(domain, "targetReleaseTime", 0.05f);
            Set(domain, "releaseTolerance", 0.5f);
            Set(domain, "domainActiveDuration", 3f);
            Set(attack, "firstHitCooldown", 0.05f);
            Set(attack, "secondHitCooldown", 0.05f);
            Set(attack, "thirdHitCooldown", 0.05f);
            Set(attack, "comboResetDelay", 0.5f);
            DomainPresentationSettings presentation =
                (DomainPresentationSettings)Get(domain, "domainPresentation");
            presentation.cinematicEnabled = false;
            presentation.barrierCloseDuration = 0.01f;
            presentation.blackTransitionDuration = 0.02f;
            presentation.barrierReleaseDuration = 0.2f;
            presentation.voidReadDuration = 0.1f;
            presentation.cloudBeatInterval = 0.1f;
            presentation.cloudBloomDuration = 0.1f;
            presentation.Group1Time = 0f;
            presentation.Group2Time = 0f;
            presentation.Group3Time = 0f;

            ProductionCombatInput.DomainReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.DomainReady,
                1f, "domain-ready");
            ProductionCombatInput.DomainReplay = null;
            ProductionCombatInput.DomainModifierPressedReplay = () => true;
            ProductionCombatInput.DomainModifierHeldReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.WaitLeftClick,
                1f, "wait-left-click");
            ProductionCombatInput.DomainModifierPressedReplay = null;
            ProductionCombatInput.BasicAttackReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.ReleaseTiming,
                1f, "release-timing");
            ProductionCombatInput.BasicAttackReplay = null;
            yield return new WaitForSeconds(0.055f);
            ProductionCombatInput.DomainModifierHeldReplay = () => false;
            ProductionCombatInput.DomainModifierReleasedReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.Active,
                1f, "domain-active-transition");
            ProductionCombatInput.DomainModifierHeldReplay = null;
            ProductionCombatInput.DomainModifierReleasedReplay = null;

            Assert.That(gate.CurrentState, Is.EqualTo(CombatActionState.DomainActive));
            Assert.That(gate.CanStartBasicAttack, Is.False,
                "Transition/arrival must retain the physical action lock");
            Assert.That(gate.CanStartDodge, Is.False,
                "Transition/arrival must retain the dodge lock");

            yield return WaitFor(() => domain.AllowsPhysicalCombatActions, 3f,
                "domain-active-gameplay");
            Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Active));
            Assert.That(gate.CurrentState, Is.EqualTo(CombatActionState.DomainActive));
            Assert.That(gate.CanStartBasicAttack, Is.True);
            Assert.That(gate.CanStartDodge, Is.True);
            Assert.That(gate.CanStartTechnique, Is.False);
            Assert.That(gate.CanStartUltimate, Is.False);
            Assert.That(gate.CanStartDomain, Is.False);

            Vector3 movementStart = player.transform.position;
            ProductionCombatInput.MoveReplay = () => Vector2.up;
            yield return Wait(0.1f);
            ProductionCombatInput.MoveReplay = null;
            Assert.That(Vector3.Distance(player.transform.position, movementStart), Is.GreaterThan(0.01f));

            var startedSteps = new List<int>();
            attack.AttackStarted += step => startedSteps.Add(step);
            for (int expectedStep = 1; expectedStep <= 3; expectedStep++)
            {
                int previousCount = startedSteps.Count;
                ProductionCombatInput.BasicAttackReplay = () => true;
                yield return WaitFor(() => startedSteps.Count > previousCount, 1f,
                    "domain-basic-attack-" + expectedStep);
                ProductionCombatInput.BasicAttackReplay = null;
                Assert.That(startedSteps[startedSteps.Count - 1], Is.EqualTo(expectedStep));
                yield return Wait(0.06f);
            }

            ProductionCombatInput.MoveReplay = () => Vector2.right;
            ProductionCombatInput.DodgeReplay = () => true;
            yield return WaitFor(() => movement.IsDodging, 1f, "domain-dodge");
            ProductionCombatInput.DodgeReplay = null;
            ProductionCombatInput.MoveReplay = null;
            Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Active));
            Assert.That(gate.CurrentState, Is.EqualTo(CombatActionState.DomainActive));
        }

        [UnityTest]
        public IEnumerator SelectedGojoToDomainExitRestoresRealBasicAttackPath()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return VerifySelectedGojoDomainInputPath();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifySelectedGojoDomainInputPath()
        {
            MatchTeamSelectionStore.SetPlayerTeam(
                MatchTeamSelection.Solo(PrototypeCharacterId.GojoModern)
            );
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/CombatMVP.unity",
                new LoadSceneParameters(LoadSceneMode.Single)
            );
            yield return Wait(0.5f);
            GameObject player = GameObject.Find("GojoPlayer");
            Assert.That(player != null, Is.True, "CombatMVP Gojo player did not stabilize");
            GojoDomainController domain = player.GetComponent<GojoDomainController>();
            Assert.That(domain != null, Is.True, "CombatMVP Gojo domain controller is missing");
            BasicAttack attack = player.GetComponent<BasicAttack>();
            Assert.That(attack, Is.Not.Null);
            Assert.That(player.GetComponent<PrototypeCharacterController>(), Is.Not.Null);
            Assert.That(player.GetComponent<PrototypeFighterPresentationController>(), Is.Not.Null,
                "CombatMVP player lifecycle must install its attack presentation consumer");

            Set(domain, "domainEnergyCost", 0f);
            Set(domain, "targetReleaseTime", 0.05f);
            Set(domain, "releaseTolerance", 0.5f);
            Set(domain, "domainActiveDuration", 0.1f);
            DomainPresentationSettings presentation = (DomainPresentationSettings)Get(domain, "domainPresentation");
            presentation.cinematicEnabled = false;
            presentation.barrierCloseDuration = 0.01f;
            presentation.blackTransitionDuration = 0.02f;
            presentation.barrierReleaseDuration = 0.2f;
            presentation.voidReadDuration = 0.1f;
            presentation.cloudBeatInterval = 0.1f;
            presentation.cloudBloomDuration = 0.1f;
            presentation.Group1Time = 0f;
            presentation.Group2Time = 0f;
            presentation.Group3Time = 0f;

            ProductionCombatInput.DomainReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.DomainReady, 1f,
                "domain-ready");
            ProductionCombatInput.DomainReplay = null;

            ProductionCombatInput.DomainModifierPressedReplay = () => true;
            ProductionCombatInput.DomainModifierHeldReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.WaitLeftClick, 1f,
                "wait-left-click");
            ProductionCombatInput.DomainModifierPressedReplay = null;

            ProductionCombatInput.BasicAttackReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.ReleaseTiming, 1f,
                "release-timing");
            ProductionCombatInput.BasicAttackReplay = null;
            Assert.That(attack.DisplayChainStep, Is.Zero,
                "The domain hand-seal click must not leak into basic melee");

            yield return new WaitForSeconds(0.055f);
            ProductionCombatInput.DomainModifierHeldReplay = () => false;
            ProductionCombatInput.DomainModifierReleasedReplay = () => true;
            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.Active, 1f,
                "domain-active");
            ProductionCombatInput.DomainModifierReleasedReplay = null;
            ProductionCombatInput.DomainModifierHeldReplay = null;

            DomainPresentationSession session =
                player.GetComponentInChildren<DomainPresentationSession>();
            Assert.That(session, Is.Not.Null);
            Assert.That(session.IsRunning, Is.True);
            bool observedNaturalTerminalState = false;
            session.Ended += () =>
            {
                CombatActionGate terminalGate = player.GetComponent<CombatActionGate>();
                observedNaturalTerminalState = true;
                Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Normal));
                Assert.That(terminalGate.CurrentState, Is.EqualTo(CombatActionState.Normal));
                Assert.That(terminalGate.CanStartBasicAttack, Is.True);
                Assert.That(terminalGate.CanStartDodge, Is.True);
                Assert.That(terminalGate.CanStartTechnique, Is.False);
                Assert.That(terminalGate.CanStartUltimate, Is.False);
                Assert.That(terminalGate.CanStartDomain, Is.False);
            };

            yield return WaitFor(() => domain.State == GojoDomainController.DomainState.Normal, 4f,
                "domain-ended");
            Assert.That(observedNaturalTerminalState, Is.True,
                "Natural presentation completion must own the Domain terminal transition");
            Assert.That(player.GetComponent<TechniqueBurnoutController>().IsBurnedOut, Is.True);
            CombatActionGate restoredGate = player.GetComponent<CombatActionGate>();
            Assert.That(restoredGate.CurrentState, Is.EqualTo(CombatActionState.Normal));
            Assert.That(restoredGate.CanStartBasicAttack, Is.True);
            Assert.That(restoredGate.CanStartDodge, Is.True);
            Assert.That(restoredGate.CanStartTechnique, Is.False);
            Assert.That(restoredGate.CanStartUltimate, Is.False);
            Assert.That(restoredGate.CanStartDomain, Is.False);

            Transform origin = player.transform.Find("AttackOrigin");
            GameObject target = new GameObject("TrackA_PhysicalTarget", typeof(Health), typeof(BoxCollider));
            target.transform.position = origin.position;
            Physics.SyncTransforms();
            Transform arm = player.transform.Find("PrototypeGojoAvatar/RightArm");
            Assert.That(arm, Is.Not.Null);
            TrackAAttackPoseProbe poseProbe = player.AddComponent<TrackAAttackPoseProbe>();
            poseProbe.Arm = arm;
            int startedStep = 0;
            attack.AttackStarted += step => startedStep = step;

            ProductionCombatInput.BasicAttackReplay = () => true;
            yield return WaitFor(() => startedStep > 0, 1f, "basic-attack-started");
            ProductionCombatInput.BasicAttackReplay = null;
            yield return new WaitForSeconds(0.12f);
            yield return new WaitForEndOfFrame();

            Assert.That(startedStep, Is.EqualTo(1));
            Assert.That(attack.DisplayChainStep, Is.EqualTo(1));
            Assert.That(target.GetComponent<Health>().CurrentHealth,
                Is.LessThan(target.GetComponent<Health>().MaxHealth));
            FighterAnimationStateSnapshot snapshot = player.GetComponent<FighterAnimationStateSource>().Snapshot;
            Assert.That(snapshot.CharacterId, Is.EqualTo(PrototypeCharacterId.GojoModern));
            Assert.That(snapshot.BasicAttackStep, Is.EqualTo(1));
            PrototypeFighterPresentationController pose =
                player.GetComponent<PrototypeFighterPresentationController>();
            Assert.That(pose.enabled, Is.True);
            Assert.That(Get(pose, "activeVisualRoot"), Is.SameAs(arm.parent));
            Assert.That(Get(pose, "rightArm"), Is.SameAs(arm));
            Assert.That(poseProbe.MaxAngle, Is.GreaterThan(5f));

            ThirdPersonPlayerController movement = player.GetComponent<ThirdPersonPlayerController>();
            ProductionCombatInput.DodgeReplay = () => true;
            yield return WaitFor(() => movement.IsDodging, 1f, "post-domain-dodge");
            ProductionCombatInput.DodgeReplay = null;
            Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Normal));
        }

        [Test]
        public void GenericVfxLabSlotsPreserveExistingSerializedValues()
        {
            Assert.That((int)VfxLabPreviewAction.Skill1, Is.EqualTo(3));
            Assert.That((int)VfxLabPreviewAction.Skill2, Is.EqualTo(4));
            Assert.That((int)VfxLabPreviewAction.Ultimate, Is.EqualTo(5));
            Assert.That((int)VfxLabPreviewAction.Domain, Is.EqualTo(6));
            Assert.That(VfxLabPreviewAction.Blue, Is.EqualTo(VfxLabPreviewAction.Skill1));
            Assert.That(VfxLabPreviewAction.UnlimitedVoid, Is.EqualTo(VfxLabPreviewAction.Domain));
        }

        [Test]
        public void CombatAudioProfileMapsGenericSlotsAndKeepsLegacyFallbackFields()
        {
            CombatAudioProfile profile = ScriptableObject.CreateInstance<CombatAudioProfile>();
            Assert.That(profile.GetTechnique(CharacterPresentationSkillSlot.Skill1), Is.SameAs(profile.skill1));
            Assert.That(profile.GetTechnique(CharacterPresentationSkillSlot.Skill2), Is.SameAs(profile.skill2));
            Assert.That(profile.GetTechnique(CharacterPresentationSkillSlot.Ultimate), Is.SameAs(profile.ultimate));
            Assert.That(profile.GetTechnique(CharacterPresentationSkillSlot.Domain), Is.SameAs(profile.domain));

            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            Assert.That(typeof(PrototypeCombatAudio).GetField("blueVoice", fields), Is.Not.Null);
            Assert.That(typeof(PrototypeCombatAudio).GetField("redVoice", fields), Is.Not.Null);
            Assert.That(typeof(PrototypeCombatAudio).GetField("purpleVoice", fields), Is.Not.Null);
            Assert.That(typeof(PrototypeCombatAudio).GetField("domainVoice", fields), Is.Not.Null);
            Object.DestroyImmediate(profile);
        }

        private static IEnumerator WaitFor(System.Func<bool> predicate, float timeout, string state)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!predicate())
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline),
                    $"Timed out waiting for runtime state: {state}");
                yield return null;
            }
        }

        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            double deadline = UnityEditor.EditorApplication.timeSinceStartup + 30d;
            while (Time.time < end)
            {
                Assert.That(UnityEditor.EditorApplication.timeSinceStartup, Is.LessThan(deadline));
                yield return null;
            }
        }

        private static object Get(object target, string field)
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(target);
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }
    }
}
