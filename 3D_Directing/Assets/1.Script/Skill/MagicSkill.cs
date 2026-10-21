using UnityEngine;

// 스킬1 마법: 사거리 안의 모든 적 발밑에 원 생성 -> 일정 시간 뒤 폭발
public class MagicSkill : SkillBase
{
    private readonly float spawnTiming = 0.5f;  // 원 생성 진행도 (임시)
    private readonly float castRange = 2f;      // 사거리 (원을 만들 적 탐색, 임시)
    private readonly float explodeDelay = 1f;   // 원 생성 후 폭발까지 (임시)

    private bool spawned;   // 이번 사용에서 원을 이미 만들었는지

    public MagicSkill(MikuCharacter owner)
        : base(owner, "Magic", "Skill1", cooldown: 5f, range: 0.4f, damage: 1f, useDash: false) { }

    protected override void OnBegin()
    {
        spawned = false;
    }

    public override void Tick(float t)
    {
        if (spawned || t < spawnTiming) return;
        spawned = true;

        // 폭발 반경 = 판정 범위
        foreach (Enemy enemy in FindEnemies(owner.transform.position, castRange))
            MagicCircle.Spawn(enemy.FeetPosition, range, explodeDelay, damage, owner.EnemyLayer);
    }
}
