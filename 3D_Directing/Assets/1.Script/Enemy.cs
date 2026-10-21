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

    // 콜라이더 (발밑 위치 계산용)
    private Collider bodyCollider;

    // 띄우기 (y만 움직임)
    private bool isLaunched;
    private float launchElapsed;
    private float launchDuration;
    private float launchHeight;
    private float launchBaseY;      // 띄우기 전 바닥 높이

    // 밀어내기 (x, z만 움직임)
    private bool isKnockback;
    private float knockbackElapsed;
    private float knockbackDuration;
    private Vector3 knockbackStart;
    private Vector3 knockbackEnd;

    // 발밑 위치 (마법 원 생성용)
    public Vector3 FeetPosition => new Vector3(transform.position.x, bodyCollider.bounds.min.y, transform.position.z);

    // 반짝인 뒤 경과 시간 (로그용)
    public float TimeSinceFlash => Time.time - flashTime;
    // 창이 열려 있는지 (경과 시간 <= 창 길이)
    public bool IsPerfectWindowOpen => TimeSinceFlash <= perfectWindow;

    private void Start()
    {
        originalColor = bodyRenderer.material.color;
        nextFlashTime = Time.time + flashInterval;
        bodyCollider = GetComponentInChildren<Collider>();
    }

    private void Update()
    {
        TickLaunch();
        TickKnockback();

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

    #region Hit Reaction
    // 띄우기: height까지 올라갔다가 duration 뒤 원래 높이로 (포물선)
    public void Launch(float height, float duration)
    {
        // 떠 있는 중에 다시 맞으면 원래 바닥 높이 기준으로 다시 띄움
        if (!isLaunched) launchBaseY = transform.position.y;

        isLaunched = true;
        launchElapsed = 0f;
        launchHeight = height;
        launchDuration = duration;

        DashLog.Log($"Launch {name} height={height} duration={duration}");
    }

    private void TickLaunch()
    {
        if (!isLaunched) return;

        launchElapsed += Time.deltaTime;
        float p = Mathf.Min(launchElapsed / launchDuration, 1f);

        // 4p(1-p): p=0 -> 0, p=0.5 -> 1(정점), p=1 -> 0
        Vector3 pos = transform.position;
        pos.y = launchBaseY + launchHeight * 4f * p * (1f - p);
        transform.position = pos;

        if (p >= 1f)
        {
            isLaunched = false;
            DashLog.Log($"LaunchEnd {name} y={pos.y:F3}");
        }
    }

    // 밀어내기: dir 방향으로 distance만큼 duration 동안 (처음 빠르고 끝에 느려짐)
    public void Knockback(Vector3 dir, float distance, float duration)
    {
        dir.y = 0;

        isKnockback = true;
        knockbackElapsed = 0f;
        knockbackDuration = duration;
        knockbackStart = transform.position;
        knockbackEnd = transform.position + dir.normalized * distance;

        DashLog.Log($"Knockback {name} dir={dir.normalized} distance={distance} duration={duration}");
    }

    private void TickKnockback()
    {
        if (!isKnockback) return;

        knockbackElapsed += Time.deltaTime;
        float p = Mathf.Min(knockbackElapsed / knockbackDuration, 1f);

        // ease-out: 1-(1-p)^2
        float eased = 1f - (1f - p) * (1f - p);
        Vector3 flat = Vector3.Lerp(knockbackStart, knockbackEnd, eased);

        // y는 띄우기가 담당하므로 x, z만
        Vector3 pos = transform.position;
        pos.x = flat.x;
        pos.z = flat.z;
        transform.position = pos;

        if (p >= 1f)
        {
            isKnockback = false;
            DashLog.Log($"KnockbackEnd {name} pos={pos}");
        }
    }
    #endregion

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
