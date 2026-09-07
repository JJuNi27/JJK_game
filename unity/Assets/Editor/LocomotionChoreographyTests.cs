using JJKGame.Core;
using NUnit.Framework;
using UnityEngine;

namespace JJKGame.EditorTools
{
    public sealed class LocomotionChoreographyTests
    {
        [TestCase(30)]
        [TestCase(60)]
        [TestCase(144)]
        public void GojoDistanceDoesNotDependOnFrameRate(int fps)
        {
            var motion = new EvadeMotion();
            motion.Begin(EvadeProfile.CreateGojo());
            float distance = 0f;
            while (motion.IsActive) distance += motion.Advance(1f / fps);
            Assert.That(distance, Is.EqualTo(3.9f).Within(0.001f));
            Assert.That(motion.Advance(1f), Is.Zero);
        }

        [Test]
        public void ReactiveBurstAndRecoveryAreIndependent()
        {
            var motion = new EvadeMotion();
            motion.Begin(EvadeProfile.CreateGojo());
            Assert.That(motion.Advance(0.1f), Is.EqualTo(2.78f).Within(0.001f));
            Assert.That(motion.Advance(0.1f), Is.EqualTo(1.12f).Within(0.001f));
            Assert.That(motion.IsRecovering, Is.True);
            Assert.That(motion.Advance(0.02f), Is.Zero);
            Assert.That(motion.IsActive, Is.True);
            motion.Advance(0.03f);
            Assert.That(motion.IsActive, Is.False);
        }

        [Test]
        public void HitchPauseAndCancellationCannotAddDistance()
        {
            var motion = new EvadeMotion();
            motion.Begin(EvadeProfile.CreateGojo());
            Assert.That(motion.Advance(0f), Is.Zero);
            Assert.That(motion.Advance(-1f), Is.Zero);
            Assert.That(motion.Advance(1f), Is.EqualTo(3.9f).Within(0.001f));
            motion.Begin(EvadeProfile.CreateGojo());
            motion.Advance(0.04f);
            motion.Cancel();
            Assert.That(motion.Advance(1f), Is.Zero);
        }

        [Test]
        public void BurstStopsAtWallIncludingSingleFrameHitch()
        {
            var actor = new GameObject("EvadeTestActor");
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                var motor = actor.AddComponent<CharacterController>();
                motor.radius = 0.42f;
                motor.height = 2f;
                actor.transform.position = Vector3.up;
                wall.transform.position = new Vector3(0f, 1f, 1.5f);
                wall.transform.localScale = new Vector3(4f, 4f, 0.1f);
                Physics.SyncTransforms();
                var motion = new EvadeMotion();
                motion.Begin(EvadeProfile.CreateGojo());
                EvadeMotion.Move(motor, Vector3.forward, motion.Advance(1f), 0f);
                Assert.That(actor.transform.position.z, Is.LessThan(1.1f));
                Assert.That(actor.transform.position.z, Is.GreaterThan(0.5f));
                Assert.That(motion.Advance(1f), Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(actor);
                Object.DestroyImmediate(wall);
            }
        }

        [Test]
        public void ReleaseIsExactlyOnceAndOldTokensAreRejected()
        {
            var clock = new TechniqueReleaseClock();
            int old = clock.Begin(0f, 0.24f, true);
            clock.Cancel();
            int current = clock.Begin(1f, 0.24f, true);
            Assert.That(clock.RequestRelease(old), Is.False);
            Assert.That(clock.TryConsume(1.1f), Is.False);
            Assert.That(clock.RequestRelease(current), Is.True);
            Assert.That(clock.RequestRelease(current), Is.False);
            Assert.That(clock.TryConsume(1.1f), Is.True);
            Assert.That(clock.TryConsume(2f), Is.False);
            Assert.That(clock.RequestRelease(current), Is.False);
        }

        [Test]
        public void DefaultBindingRetainsTimerAndMissingEventFallsBack()
        {
            var clock = new TechniqueReleaseClock();
            int token = clock.Begin(0f, 0.24f, false);
            Assert.That(clock.RequestRelease(token), Is.False);
            Assert.That(clock.TryConsume(0.239f), Is.False);
            Assert.That(clock.TryConsume(0.24f), Is.True);
            clock.Begin(1f, 0.30f, true);
            Assert.That(clock.TryConsume(1.31f), Is.True);
        }
    }
}
