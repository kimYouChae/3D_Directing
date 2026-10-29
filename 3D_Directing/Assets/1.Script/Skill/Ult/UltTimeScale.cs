using UnityEngine;

public class UltTimeScale : MonoBehaviour
{
    [Range(0.05f, 2f)] public float scale = 1f;   // 0이면 Timeline이 멈추므로 하한 0.05

    [SerializeField] private HitFeedback hitFeedback;   // 미쿠의 HitFeedback

    void Update()
    {
        //히트스톱이 timeScale을 잡고 있는 동안은 덮어쓰지 않음
        if (hitFeedback != null && hitFeedback.IsHitStop) return;

        Time.timeScale = scale;
    }

    void OnDisable()
    {
        // 히트스톱 중에 꺼지면 복구는 히트스톱에 맡김
        if (hitFeedback != null && hitFeedback.IsHitStop) return;

        Time.timeScale = 1f;   // 구간이 끝나면 반드시 복구
    }
}
