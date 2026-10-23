using UnityEngine;

// 스킬3 내려찍기: 판정 구간 동안 맞은 적 중 충격 반경 안의 적을 밀어낸다
public class SlamSkill : SkillBase
{
    private readonly SlamSkillData data;

    public SlamSkill(MikuCharacter owner, SlamSkillData data) : base(owner, data)
    {
        this.data = data;
    }

    protected override void OnTick(float t)
    {
        HitWindow(t, data.HitStart, data.HitEnd, Range, enemy =>
        {
            enemy.TakeDamage(Damage);

            // 플레이어 -> 적 수평 방향으로 밀기
            Vector3 dir = enemy.transform.position - owner.transform.position;
            dir.y = 0;

            // 충격 반경 밖이면 데미지만
            if (dir.magnitude > data.ImpactRadius) return;

            // 완전히 겹쳐 있으면 플레이어 정면으로
            if (dir.sqrMagnitude < 0.0001f) dir = owner.transform.forward;

            enemy.Knockback(dir, data.KnockbackDistance, data.KnockbackDuration);
        });
    }
}
