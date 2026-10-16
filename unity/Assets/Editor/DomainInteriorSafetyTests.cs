using JJKGame.CameraSystem;
using JJKGame.Player;
using NUnit.Framework;
using UnityEngine;

namespace JJKGame.EditorTools
{
    public sealed class DomainInteriorSafetyTests
    {
        [Test]
        public void MultipleParticipantsRestoreIndependentlyAndRepeatedExitDoesNotDrift()
        {
            var caster = new GameObject("Caster");
            var a = new GameObject("EnemyA");
            var b = new GameObject("EnemyB");
            var space = new DomainInteriorSpace();
            try
            {
                caster.transform.SetPositionAndRotation(new Vector3(2,0,3), Quaternion.Euler(0,42,0));
                a.transform.SetPositionAndRotation(new Vector3(-7,0,8), Quaternion.Euler(0,110,0));
                b.transform.SetPositionAndRotation(new Vector3(8,0,-5), Quaternion.Euler(0,-67,0));
                Transform[] actors = { caster.transform, a.transform, b.transform };
                Vector3[] positions = { actors[0].position, actors[1].position, actors[2].position };
                Quaternion[] rotations = { actors[0].rotation, actors[1].rotation, actors[2].rotation };
                var motor = a.AddComponent<CharacterController>();
                b.AddComponent<CharacterController>().enabled = false;
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    space.Enter(caster.transform, new[] { a.transform, b.transform, a.transform }, new Vector3(0,2048,0), 90);
                    Assert.That(space.ParticipantCount, Is.EqualTo(3));
                    Assert.That(a.transform.position - caster.transform.position, Is.EqualTo(positions[1] - positions[0]));
                    Assert.That(motor.enabled, Is.True);
                    Assert.That(b.GetComponent<CharacterController>().enabled, Is.False);
                    for (int i = 0; i < actors.Length; i++) actors[i].position += new Vector3(i,2,4);
                    space.Exit(); space.Exit();
                    for (int i = 0; i < actors.Length; i++)
                    {
                        Assert.That(actors[i].position, Is.EqualTo(positions[i]));
                        Assert.That(Quaternion.Angle(actors[i].rotation, rotations[i]), Is.LessThan(.001f));
                    }
                }
            }
            finally { space.Dispose(); Object.DestroyImmediate(caster); Object.DestroyImmediate(a); Object.DestroyImmediate(b); }
        }

        [Test]
        public void DestroyedParticipantCannotPreventOtherParticipantsReturning()
        {
            var caster = new GameObject("Caster");
            var victim = new GameObject("Victim");
            using (var space = new DomainInteriorSpace())
            {
                try
                {
                    space.Enter(caster.transform, new[] { victim.transform }, new Vector3(0,2048,0), 90);
                    Object.DestroyImmediate(victim);
                    space.Exit();
                    Assert.That(caster.transform.position, Is.EqualTo(Vector3.zero));
                }
                finally { Object.DestroyImmediate(caster); if (victim != null) Object.DestroyImmediate(victim); }
            }
        }

        [Test]
        public void CameraLeaseRejectsAnotherOwnerAndRestoresShiftedPoseAndFov()
        {
            var host = new GameObject("Camera", typeof(Camera), typeof(DomainCameraOverride));
            var owner = new GameObject("Owner");
            var other = new GameObject("Other");
            try
            {
                var camera = host.GetComponent<Camera>(); camera.fieldOfView = 57;
                host.transform.SetPositionAndRotation(new Vector3(1,3,-8), Quaternion.Euler(20,12,0));
                var original = host.transform.rotation;
                var lease = host.GetComponent<DomainCameraOverride>();
                Assert.That(lease.Acquire(owner), Is.True);
                Assert.That(lease.Acquire(other), Is.False);
                lease.Pose(owner, Vector3.one, Quaternion.identity, 20);
                lease.ShiftReturn(new Vector3(0,2048,0));
                lease.Release(other);
                Assert.That(lease.IsActive, Is.True);
                lease.Release(owner);
                Assert.That(host.transform.position, Is.EqualTo(new Vector3(1,2051,-8)));
                Assert.That(Quaternion.Angle(host.transform.rotation, original), Is.LessThan(.001f));
                Assert.That(camera.fieldOfView, Is.EqualTo(57));
                Assert.That(lease.Acquire(owner), Is.True);
                lease.Pose(owner, Vector3.zero, Quaternion.identity, 10);
                lease.Release(owner);
                lease.Release(owner);
                Assert.That(camera.fieldOfView, Is.EqualTo(57));
            }
            finally { Object.DestroyImmediate(host); Object.DestroyImmediate(owner); Object.DestroyImmediate(other); }
        }

        [Test]
        public void VictimSelectionUsesDistanceThenStableHierarchyRegardlessOfOverlapOrder()
        {
            var caster = new GameObject("Caster");
            var a = new GameObject("A"); var b = new GameObject("B");
            try
            {
                a.transform.position = Vector3.right * 2; b.transform.position = Vector3.left * 2;
                Assert.That(DomainPresentationSession.SelectVictim(caster.transform, new[] {b.transform,a.transform}, null), Is.EqualTo(a.transform));
                Assert.That(DomainPresentationSession.SelectVictim(caster.transform, new[] {a.transform,b.transform}, null), Is.EqualTo(a.transform));
                b.transform.position = Vector3.left;
                Assert.That(DomainPresentationSession.SelectVictim(caster.transform, new[] {a.transform,b.transform}, null), Is.EqualTo(b.transform));
                Assert.That(DomainPresentationSession.SelectVictim(caster.transform, System.Array.Empty<Transform>(), null), Is.Null);
                Assert.That(new DomainPresentationSettings().DetailDuration(false), Is.LessThan(new DomainPresentationSettings().DetailDuration(true)));
            }
            finally { Object.DestroyImmediate(caster); Object.DestroyImmediate(a); Object.DestroyImmediate(b); }
        }
    }
}
