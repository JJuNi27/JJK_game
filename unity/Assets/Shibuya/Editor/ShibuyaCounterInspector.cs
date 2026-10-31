using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaCounterProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaCounterInspector : ShibuyaInspector
    {
        public override void OnInspectorGUI(){base.OnInspectorGUI();
            EditorGUILayout.HelpBox("성공한 패링 뒤 새 LMB 입력으로만 실행합니다. 선택 대상만 예약하며 일반 거리 이탈·가드·회피로는 무효화되지 않습니다. 벽·특수 무적·시네마틱은 취소 예외입니다. 예약 상한은 타격·이동 전체 길이와 패링 복귀 시간을 수용해야 합니다.",MessageType.Info);
            if(!((ShibuyaCounterProfile)target).IsValid)EditorGUILayout.HelpBox("반격 시퀀스 또는 예약 시간이 유효하지 않습니다.",MessageType.Warning);}
    }
}
