using UnityEngine;

public class UltTimeScale : MonoBehaviour
{
    [Range(0.05f, 2f)] public float scale = 1f;   // 0이면 Timeline이 멈추므로 하한 0.05

    void Update()
    {
        Time.timeScale = scale;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;   // 구간이 끝나면 반드시 복구
    }
}
