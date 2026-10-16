using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float hp = 30f;

    [Header("===Perfect Dodge (Test)===")]
    [SerializeField] private float flashInterval = 3f;    // 테스트용 반짝임 간격
    [SerializeField] private float perfectWindow = 0.3f;  // 창 길이 (게임 시간)
    [SerializeField] private Renderer bodyRenderer;       // 창 열림 표시용
    [SerializeField] private Color flashColor = Color.red;

    private float flashTime = float.NegativeInfinity;     // 마지막 반짝임 시각
    private float nextFlashTime;
    private Color originalColor;

    // 반짝인 뒤 경과 시간 (로그용)
    public float TimeSinceFlash => Time.time - flashTime;
    // 창이 열려 있는지 (경과 시간 <= 창 길이)
    public bool IsPerfectWindowOpen => TimeSinceFlash <= perfectWindow;

    private void Start()
    {
        originalColor = bodyRenderer.material.color;
        nextFlashTime = Time.time + flashInterval;
    }

    private void Update()
    {
        if (Time.time >= nextFlashTime)
        {
            Flash();
            nextFlashTime += flashInterval;
        }

        // 창이 열린 동안만 반짝임 색
        bodyRenderer.material.color = IsPerfectWindowOpen ? flashColor : originalColor;
    }

    private void Flash()
    {
        flashTime = Time.time;
        DashLog.Log($"EnemyFlash {name}");
    }

    // 퍼펙트로 쓰였으면 창 닫기 (한 공격에 한 번)
    // NegativeInfinity -> 경과 시간이 무한대가 되어 자동으로 닫힌 상태
    public void ConsumePerfectWindow()
    {
        flashTime = float.NegativeInfinity;
    }

    // 데미지 받기
    public void TakeDamage(float damage)
    {
        hp -= damage;
        // Debug.Log($"{name} 피격 / 데미지 {damage} / 남은 HP {hp}");

        // hp가 0 이하가 되면 파괴
        if (hp <= 0)
            Destroy(gameObject);
    }
}
