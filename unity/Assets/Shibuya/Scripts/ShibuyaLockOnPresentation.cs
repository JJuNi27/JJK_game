using UnityEngine;

namespace JJKGame.Shibuya
{
    [RequireComponent(typeof(ShibuyaLockOn))]
    public sealed class ShibuyaLockOnPresentation : MonoBehaviour
    {
        private ShibuyaLockOn source;
        private Camera view;
        public void Configure(ShibuyaLockOn selection, Camera camera) { source = selection; view = camera; }
        private void OnGUI()
        {
            if (source == null || view == null || source.Settings == null || !source.Settings.showUI) return;
            var p = source.Settings;
            if (source.IsSearching) Marker(source.Candidate, "◇ 후보", p.candidateColor);
            Marker(source.LockedTarget, source.IsOccluded ? "◇ 가림" : "◆ 락온", source.IsOccluded ? p.occludedColor : p.lockedColor);
            Color old = GUI.color;
            GUI.color = Color.white;
            if (source.LockedTarget != null) {
                var screen=view.WorldToViewportPoint(source.TargetPoint);
                bool onScreen=screen.z>0 && screen.x>=0 && screen.x<=1 && screen.y>=0 && screen.y<=1;
                GUI.Label(new Rect(16, 150, 420, 26), $"락온: {source.LockedTarget.name}  {source.Distance:F1}m"
                    + (source.IsOccluded ? " / 환경 가림" : onScreen ? " / 화면 안" : " / 화면 밖"));
            }
            else if (source.HasLostFeedback) GUI.Label(new Rect(16, 150, 420, 26), "대상 상실: " + source.LossReason);
            else if (source.IsSearching) GUI.Label(new Rect(16, 150, 420, 26), "중앙 후보 탐색 — 휠 버튼을 놓아 선택");
            GUI.color = old;
        }
        private void Marker(ShibuyaFighter target, string text, Color color)
        {
            if (target == null) return;
            Vector3 screen = view.WorldToScreenPoint(target.GetComponent<CharacterController>().bounds.center + source.Settings.aimOffset);
            if (screen.z <= 0) return;
            Color old = GUI.color; GUI.color = color; float size = source.Settings.markerSize;
            GUI.Label(new Rect(screen.x-size, Screen.height-screen.y-size*.5f, size*4, size*2), text); GUI.color = old;
        }
        private void LateUpdate()
        {
            if (source == null || source.Settings == null || !source.Settings.showAimDebug) return;
            var aim = source.GetAim(transform.position+Vector3.up);
            Debug.DrawRay(aim.Origin, aim.Direction*source.Settings.debugAimLength, Color.cyan);
        }
    }
}
