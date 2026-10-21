using UnityEngine;

// 스킬3 내려찍기: 판정 구간 동안 맞은 적 중 충격 반경 안의 적을 밀어낸다
public class SlamSkill : SkillBase
{
    private readonly float hitStart = 0.2f;         // 판정 시작 진행도 (임시)
    private readonly float hitEnd = 0.8f;           // 판정 끝 진행도 (임시)
    private readonly float impactRadius = 2f;       // 충격 반경
    private readonly float knockbackDistance = 2f;  // 밀어내는 거리
    private readonly float knockbackDuration = 1f;  // 밀려나는 시간

    public SlamSkill(MikuCharacter owner)
        : base(owner, "Slam", "Skill3", cooldown: 15f, range: 2f, damage: 1f, useDash: true) { }

    public override void Tick(float t)
    {
        HitWindow(t, hitStart, hitEnd, range, enemy =>
        {
            enemy.TakeDamage(damage);

            // 플레이어 -> 적 수평 방향으로 밀기
            Vector3 dir = enemy.transform.position - owner.transform.position;
            dir.y = 0;

            // 충격 반경 밖이면 데미지만
            if (dir.magnitude > impactRadius) return;

            // 완전히 겹쳐 있으면 플레이어 정면으로
            if (dir.sqrMagnitude < 0.0001f) dir = owner.transform.forward;

            enemy.Knockback(dir, knockbackDistance, knockbackDuration);
        });
    }
}
