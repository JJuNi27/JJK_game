using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    public interface IDomainInteriorTransition : IDisposable
    {
        bool IsEntered { get; }
        Vector3 Translation { get; }
        void Enter(Transform caster, IReadOnlyList<Transform> participants, Vector3 origin, float radius);
        void Exit();
    }

    /// <summary>Same-scene isolated instance. No actor cloning, reparenting, or reference replacement.</summary>
    public sealed class DomainInteriorSpace : IDomainInteriorTransition
    {
        private sealed class ReturnState
        {
            public Transform Actor;
            public Vector3 Position;
            public Quaternion Rotation;
            public CharacterController Motor;
            public bool MotorEnabled;
            public NavMeshAgent Agent;
            public bool AgentEnabled;
            public Rigidbody Body;
            public Vector3 Velocity, AngularVelocity;
            public bool Kinematic;
        }

        private readonly List<ReturnState> returns = new List<ReturnState>();
        private GameObject environment;
        public bool IsEntered { get; private set; }
        public Vector3 Translation { get; private set; }
        public int ParticipantCount => returns.Count;

        public void Enter(Transform caster, IReadOnlyList<Transform> participants, Vector3 origin, float radius)
        {
            if (IsEntered || caster == null) return;
            Translation = origin - caster.position;
            environment = new GameObject("UnlimitedVoid_IsolatedInteriorSpace");
            SceneManager.MoveGameObjectToScene(environment, caster.gameObject.scene);
            environment.transform.position = origin;
            var floor = environment.AddComponent<BoxCollider>();
            floor.center = Vector3.down * 0.5f;
            floor.size = new Vector3(radius * 2f, 1f, radius * 2f);
            Save(caster);
            if (participants != null) foreach (Transform participant in participants) Save(participant);
            IsEntered = true; // permits rollback if entry fails partway through
            try
            {
                foreach (ReturnState state in returns)
                    Teleport(state, state.Position + Translation, state.Rotation, false);
                Physics.SyncTransforms();
            }
            catch { Exit(); throw; }
        }

        private void Save(Transform actor)
        {
            if (actor == null) return;
            foreach (ReturnState saved in returns) if (saved.Actor == actor) return;
            var state = new ReturnState { Actor = actor, Position = actor.position, Rotation = actor.rotation,
                Motor = actor.GetComponent<CharacterController>(), Body = actor.GetComponent<Rigidbody>(),
                Agent = actor.GetComponent<NavMeshAgent>() };
            state.MotorEnabled = state.Motor != null && state.Motor.enabled;
            state.AgentEnabled = state.Agent != null && state.Agent.enabled;
            if (state.Body != null)
            {
                state.Kinematic = state.Body.isKinematic;
                state.Velocity = state.Body.linearVelocity;
                state.AngularVelocity = state.Body.angularVelocity;
            }
            returns.Add(state);
        }

        private static void Teleport(ReturnState state, Vector3 position, Quaternion rotation, bool restoring)
        {
            if (state.Actor == null) return;
            if (state.Motor != null) state.Motor.enabled = false;
            if (state.Agent != null) state.Agent.enabled = false;
            if (state.Body != null) state.Body.isKinematic = true;
            state.Actor.SetPositionAndRotation(position, rotation);
            if (state.Body != null)
            {
                state.Body.position = position;
                state.Body.rotation = rotation;
                state.Body.isKinematic = state.Kinematic;
                if (!state.Kinematic)
                {
                    state.Body.linearVelocity = state.Velocity;
                    state.Body.angularVelocity = state.AngularVelocity;
                }
            }
            if (state.Motor != null) state.Motor.enabled = state.MotorEnabled;
            // Prototype bots have no NavMesh dependency. An optional agent resumes on its original mesh.
            if (restoring && state.Agent != null) state.Agent.enabled = state.AgentEnabled;
        }

        public void Exit()
        {
            if (!IsEntered && environment == null) return;
            foreach (ReturnState state in returns) Teleport(state, state.Position, state.Rotation, true);
            returns.Clear();
            Physics.SyncTransforms();
            if (environment != null)
            {
                environment.SetActive(false);
                if (Application.isPlaying) UnityEngine.Object.Destroy(environment);
                else UnityEngine.Object.DestroyImmediate(environment);
            }
            environment = null;
            IsEntered = false;
        }

        public void Dispose() => Exit();
    }
}
