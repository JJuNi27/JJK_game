using UnityEngine;

namespace JJKGame.Shibuya
{
    public struct ShibuyaInputFrame
    {
        public Vector2 move, look;
        public bool run, jump, attack;
    }

    [DefaultExecutionOrder(-50), RequireComponent(typeof(ShibuyaFighter))]
    public sealed class ShibuyaPlayerInput : MonoBehaviour
    {
        [SerializeField] private ShibuyaOrbitCamera orbitCamera;
        private ShibuyaFighter fighter;
        private bool captured;
        private bool previousAttack;
        public Vector2 MoveAxes { get; private set; }
#if UNITY_EDITOR
        public System.Func<ShibuyaInputFrame> TestInput;
#endif
        public void Configure(ShibuyaOrbitCamera camera) => orbitCamera = camera;
        private void Awake() => fighter = GetComponent<ShibuyaFighter>();
        private void Start() => Capture(true);
        private void Update()
        {
            if (fighter.State == ShibuyaFighterState.Dead || fighter.State == ShibuyaFighterState.Finished)
            {
                MoveAxes = Vector2.zero;
                Capture(false);
                fighter.SetIntent(Vector3.zero, false, false);
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
                    return;
                }
                frame = new ShibuyaInputFrame {
                    move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                    look = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")),
                    run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift),
                    jump = Input.GetKeyDown(KeyCode.Space), attack = Input.GetMouseButtonDown(0)
                };
            }
            MoveAxes = frame.move; // Keep command intent separate from attack locomotion filtering.
            orbitCamera.ApplyLook(frame.look);
            Vector3 forward = orbitCamera.transform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = orbitCamera.transform.right; right.y = 0f; right.Normalize();
            fighter.SetIntent(forward * frame.move.y + right * frame.move.x, frame.run, frame.jump);
            // The input boundary consumes presses, never a held button as auto-combo.
            if (frame.attack && !previousAttack) fighter.TryAttack();
            previousAttack = frame.attack;
        }
        private void Capture(bool value)
        {
            captured = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
        private void OnApplicationFocus(bool focus) { if (!focus) Capture(false); }
        private void OnDisable() { MoveAxes = Vector2.zero; previousAttack = false; Capture(false); }
    }
}
