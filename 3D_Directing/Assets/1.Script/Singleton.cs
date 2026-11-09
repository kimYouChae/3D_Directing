using UnityEngine;

// 씬에 배치된 매니저용 싱글톤 (OHO Singleton 참고 + 개선)
// - 씬에 없으면 자동 생성하지 않고 에러 (Inspector 연결이 필요한 매니저가 빈 채로 생기지 않게)
// - 중복이면 컴포넌트만 제거, 파괴되면 instance 정리, 종료 중엔 접근해도 새로 찾지 않음
// - 자식은 Awake 대신 OnSingletonAwake를 override (Awake를 새로 만들면 CS0108 경고)
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    private static bool isQuitting;

    public static T Instance
    {
        get
        {
            if (instance != null) return instance;
            if (isQuitting) return null;

            instance = FindAnyObjectByType<T>();
            if (instance == null)
                Debug.LogError($"[Singleton] 씬에 {typeof(T).Name}이(가) 없습니다");

            return instance;
        }
    }

    protected void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[Singleton] {typeof(T).Name} 중복 -> {name}의 컴포넌트 제거");
            enabled = false;   // Destroy는 프레임 끝에 처리됨 -> 그 전에 Start(Flow 코루틴 등)가 돌지 않게
            Destroy(this);
            return;
        }

        instance = (T)this;
        isQuitting = false;   // 도메인 리로드를 끈 경우 지난 플레이의 값이 남지 않게
        OnSingletonAwake();
    }

    // 자식 초기화가 필요하면 override
    protected virtual void OnSingletonAwake() { }

    protected virtual void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void OnApplicationQuit() => isQuitting = true;
}
