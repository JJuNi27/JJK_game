using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 4F adapter from semantic CombatAudioEvent requests to the current
    /// prototype audio implementations. Production audio can replace this adapter
    /// without forcing gameplay producers to know AudioClip/AudioSource details.
    /// </summary>
    [DefaultExecutionOrder(1430)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeCombatAudioEventBridge : MonoBehaviour
    {
        private Health ownHealth;
        private PrototypeCombatAudio prototypeAudio;
        private SukunaCombatAudio sukunaAudio;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (
                    attack == null
                    || attack.GetComponent<PrototypeCombatAudioEventBridge>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeCombatAudioEventBridge>();
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            prototypeAudio = PrototypeCombatAudio.GetOrCreate(gameObject);
        }

        private void OnEnable()
        {
            CombatAudioEvents.Raised -= HandleAudioEvent;
            CombatAudioEvents.Raised += HandleAudioEvent;
        }

        private void OnDisable()
        {
            CombatAudioEvents.Raised -= HandleAudioEvent;
        }

        private void HandleAudioEvent(CombatAudioEvent audioEvent)
        {
            if (ownHealth == null || audioEvent.Owner == null || audioEvent.Owner != ownHealth)
            {
                return;
            }

            prototypeAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);
            if (prototypeAudio == null)
            {
                return;
            }

            switch (audioEvent.EventId)
            {
                case CombatAudioEventId.BasicSwing:
                    prototypeAudio.PlayBasicSwingRuntime(Mathf.Max(1, audioEvent.Variant));
                    break;
                case CombatAudioEventId.BasicHit:
                    prototypeAudio.PlayBasicHitRuntime(Mathf.Max(1, audioEvent.Variant));
                    break;
                case CombatAudioEventId.Dodge:
                    prototypeAudio.PlayDodgeRuntime();
                    break;
                case CombatAudioEventId.GojoBlueCast:
                    prototypeAudio.PlayBlueCastRuntime();
                    break;
                case CombatAudioEventId.GojoBlueImpact:
                    prototypeAudio.PlayBlueImpactRuntime();
                    break;
                case CombatAudioEventId.GojoRedCast:
                    prototypeAudio.PlayRedCastRuntime();
                    break;
                case CombatAudioEventId.GojoRedImpact:
                case CombatAudioEventId.TechniqueImpact:
                    prototypeAudio.PlayRedImpactRuntime();
                    break;
                case CombatAudioEventId.HollowPurple:
                    prototypeAudio.PlayPurpleRuntime();
                    break;
                case CombatAudioEventId.UnlimitedVoid:
                    prototypeAudio.PlayDomainRuntime();
                    break;
                case CombatAudioEventId.MalevolentShrine:
                    GetSukunaAudio()?.PlayDomainRuntime();
                    break;
                case CombatAudioEventId.Fuga:
                    HandleFugaAudio(audioEvent);
                    break;
                case CombatAudioEventId.DivineDog:
                    if (audioEvent.Variant <= 1)
                    {
                        prototypeAudio.PlayBlueCastRuntime();
                    }
                    else
                    {
                        prototypeAudio.PlayBasicHitRuntime(2);
                    }
                    break;
                case CombatAudioEventId.Nue:
                    if (audioEvent.Variant <= 1)
                    {
                        prototypeAudio.PlayBlueCastRuntime();
                    }
                    else
                    {
                        prototypeAudio.PlayRedImpactRuntime();
                    }
                    break;
                case CombatAudioEventId.PlayerHit:
                    prototypeAudio.PlayPlayerHitRuntime();
                    break;
                case CombatAudioEventId.Victory:
                    prototypeAudio.PlayVictoryRuntime();
                    break;
                case CombatAudioEventId.Defeat:
                    prototypeAudio.PlayDefeatRuntime();
                    break;
            }
        }

        private void HandleFugaAudio(CombatAudioEvent audioEvent)
        {
            if (audioEvent.Amplified)
            {
                GetSukunaAudio()?.PlayDomainFugaRuntime();
                return;
            }

            if (audioEvent.Variant <= 1)
            {
                prototypeAudio.PlayBasicSwingRuntime(3);
            }
            else
            {
                prototypeAudio.PlayRedImpactRuntime();
            }
        }

        private SukunaCombatAudio GetSukunaAudio()
        {
            sukunaAudio ??= SukunaCombatAudio.GetOrCreate(gameObject);
            return sukunaAudio;
        }
    }
}
