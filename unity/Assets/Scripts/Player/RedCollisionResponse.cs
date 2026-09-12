using UnityEngine;
using UnityEngine.Events;

namespace JJKGame.Player
{
    public enum RedCollisionKind { DestructiblePassThrough, HardSolid, Ignore }

    /// <summary>Optional environment adapter. Character Health keeps its normal damage contract.</summary>
    public sealed class RedCollisionResponse : MonoBehaviour
    {
        [Header("레드 충돌 반응")]
        [Tooltip("파괴 가능 물체는 충격을 전달한 뒤 관통합니다. 단단한 지형은 투사체를 멈춥니다.")]
        public RedCollisionKind response = RedCollisionKind.DestructiblePassThrough;
        [Tooltip("월드 충돌 지점입니다. 파괴/충격 연출을 연결하세요.")]
        public UnityEvent<Vector3> onImpact = new UnityEvent<Vector3>();
    }
}
