using UnityEngine;

namespace JJKGame.Shibuya
{
    public struct ShibuyaInputFrame
    {
        public Vector2 move, look;
        public bool run, jump, attack, lockHeld, auraHeld, dash, guardHeld;
        public bool forwardKey, backwardKey;
    }

    [DefaultExecutionOrder(-50), RequireComponent(typeof(ShibuyaFighter))]
    public sealed class ShibuyaPlayerInput : MonoBehaviour
    {
        [SerializeField] private ShibuyaOrbitCamera orbitCamera;
        private ShibuyaFighter fighter;
        private bool captured;
        private bool previousAttack;
        private bool previousJump;
        private bool previousDash;
        public Vector2 MoveAxes { get; private set; }
        public ShibuyaLockOn LockOn { get; private set; }
        public CursedEnergyAuraController Aura {get;private set;}
        public ShibuyaEnergyCharge EnergyCharge { get; private set; }
        public ShibuyaDamageNumbers DamageNumbers {get;private set;}
#if UNITY_EDITOR
        public System.Func<ShibuyaInputFrame> TestInput;
#endif
        public void Configure(ShibuyaOrbitCamera camera) => orbitCamera = camera;
        private void Awake() => fighter = GetComponent<ShibuyaFighter>();
        private void Start()
        {
            Aura=GetComponent<CursedEnergyAuraController>()??gameObject.AddComponent<CursedEnergyAuraController>();
            EnergyCharge = GetComponent<ShibuyaEnergyCharge>() ?? gameObject.AddComponent<ShibuyaEnergyCharge>();
            var damageNumbers=Resources.Load<ShibuyaDamageNumberProfile>("Shibuya Damage Numbers");
            if(orbitCamera!=null&&damageNumbers!=null){
                DamageNumbers=orbitCamera.GetComponent<ShibuyaDamageNumbers>()??orbitCamera.gameObject.AddComponent<ShibuyaDamageNumbers>();
                DamageNumbers.Configure(fighter,orbitCamera.GetComponent<Camera>(),damageNumbers);
            }
            var settings = Resources.Load<ShibuyaLockOnProfile>("Shibuya Lock On");
            if (orbitCamera != null && settings != null) {
                LockOn = GetComponent<ShibuyaLockOn>() ?? gameObject.AddComponent<ShibuyaLockOn>();
                LockOn.Configure(fighter, orbitCamera.GetComponent<Camera>(), settings);
                fighter.BindLockOn(LockOn); orbitCamera.BindLockOn(LockOn);
                var ui = GetComponent<ShibuyaLockOnPresentation>() ?? gameObject.AddComponent<ShibuyaLockOnPresentation>();
                ui.Configure(LockOn, orbitCamera.GetComponent<Camera>());
            }
            Capture(true);
        }
        private void Update()
        {
            if (fighter.State == ShibuyaFighterState.Dead || fighter.State == ShibuyaFighterState.Finished)
            {
                MoveAxes = Vector2.zero;
                Capture(false);
                fighter.SetIntent(Vector3.zero, false, false);
                if(Aura!=null)Aura.Clear();
                return;
            }
            ShibuyaInputFrame frame;
#if UNITY_EDITOR
            if (TestInput != null) frame = TestInput();
            else
#endif
            {
                if (Input.GetKeyDown(KeyCode.Escape)) Capture(false);
                if (!captured)
                {
                    MoveAxes = Vector2.zero;
                    if (Input.GetMouseButtonDown(0)) Capture(true);
                    fighter.SetIntent(Vector3.zero, false, false);
                    if(Aura!=null)Aura.SetHeld(false);
                    if(EnergyCharge!=null)EnergyCharge.CancelCharge();
                    return;
                }
                frame = new ShibuyaInputFrame {
                    move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                    look = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")),
                    run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                    jump = Input.GetKeyDown(KeyCode.Space), attack = Input.GetMouseButtonDown(0),
                    forwardKey = Input.GetKey(KeyCode.W), backwardKey = Input.GetKey(KeyCode.S),
                    lockHeld = Input.GetMouseButton(2), auraHeld=Input.GetKey(KeyCode.F), dash=Input.GetKeyDown(KeyCode.C), guardHeld=Input.GetMouseButton(1)
                };
            }
            MoveAxes = frame.move; // Keep command intent separate from attack locomotion filtering.
            fighter.SetGuardHeld(frame.guardHeld);
            // Use actual guard state, not the held button (a successful parry consumes that hold).
            bool grabPress = frame.attack && !previousAttack && fighter.IsGuarding;
            if(grabPress)fighter.TryGrab();
            if (LockOn != null) LockOn.HandleButton(frame.lockHeld, Time.unscaledTimeAsDouble);
            orbitCamera.ApplyLook(frame.look);
            Vector3 forward = orbitCamera.transform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = orbitCamera.transform.right; right.y = 0f; right.Normalize();
            bool downSpace=fighter.IsDowned||fighter.IsQuickRecovering;
            if(frame.jump&&!previousJump&&downSpace)fighter.TryQuickRecovery(frame.move,orbitCamera.transform.forward,orbitCamera.transform.right);
            fighter.SetIntent(forward * frame.move.y + right * frame.move.x, frame.run, frame.jump && !previousJump && !downSpace);
            previousJump=frame.jump;
            bool focusHeld = frame.auraHeld && !fighter.IsCounterBusy && !fighter.HasCounterReservation && !fighter.IsGuarding && !fighter.IsGuardBroken && !fighter.IsParryRecovering && !fighter.IsGrabBusy && !fighter.IsQuickRecovering;
            if(Aura!=null)Aura.SetHeld(focusHeld);
            if(EnergyCharge!=null)EnergyCharge.SetHeld(focusHeld);
            // The input boundary consumes presses, never a held button as auto-combo.
            if (frame.attack && !previousAttack && !grabPress && !fighter.TryCounterInput()) fighter.TryAttack(frame.forwardKey == frame.backwardKey
                ? ShibuyaFinisher.Default : frame.forwardKey ? ShibuyaFinisher.Uppercut : ShibuyaFinisher.Down);
            previousAttack = frame.attack;
            if (frame.dash && !previousDash) fighter.TryGroundDash(frame.move, forward, right);
            previousDash = frame.dash;
        }
        private void Capture(bool value)
        {
            if (!value) { previousDash = false; if(fighter!=null){fighter.CancelCounter("input-capture-lost");fighter.CancelGuardInput();fighter.CancelGrabInput();fighter.CancelQuickRecovery();} }
            if(!value&&Aura!=null)Aura.SetHeld(false);
            if(!value&&EnergyCharge!=null)EnergyCharge.CancelCharge();
            if (!value && LockOn != null) LockOn.CancelSearch();
            captured = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) Capture(false); }
        private void OnDisable() { MoveAxes = Vector2.zero; previousAttack = previousJump = previousDash = false; Capture(false); if (LockOn != null) LockOn.Clear(); }
    }
}
