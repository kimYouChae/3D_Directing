using UnityEngine;

public class UltEvents : MonoBehaviour
{
    // 미쿠의 HitFeedback (GameManager 경유). 종료·씬 언로드 중엔 null일 수 있음
    private static HitFeedback MikuHitFeedback
    {
        get
        {
            GameManager gm = GameManager.Instance;
            return gm != null && gm.Miku != null ? gm.Miku.HitFeedback : null;
        }
    }

    [SerializeField] private float hitStopDuration = 0.2f;   // 실제 시간
    [SerializeField] private float hitStopTimeScale = 0f;     // 0이면 완전 정지
    [SerializeField] private float shakeForce = 1f;

    public void OnUltHitStop() => MikuHitFeedback.StartHitStop(hitStopDuration, hitStopTimeScale);
    public void OnUltShake() => MikuHitFeedback.ShakeCamera(shakeForce);
}
