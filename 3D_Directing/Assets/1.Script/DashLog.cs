using UnityEngine;

public static class DashLog
{
    public static bool Enabled = true;   // 디버깅 끝나면 false

    public static void Log(string msg)
    {
        if (Enabled) Debug.Log($"[DASH][{Time.frameCount}][{Time.time:F3}] {msg}");
    }
}
