using UnityEngine;

public class UltEvents : MonoBehaviour
{
    [SerializeField] private HitFeedback hitFeedback;
    [SerializeField] private float hitStopDuration = 0.2f;   // 실제 시간
    [SerializeField] private float hitStopTimeScale = 0f;     // 0이면 완전 정지
    [SerializeField] private float shakeForce = 1f;

    public void OnUltHitStop() => hitFeedback.StartHitStop(hitStopDuration, hitStopTimeScale);
    public void OnUltShake() => hitFeedback.ShakeCamera(shakeForce);
}
