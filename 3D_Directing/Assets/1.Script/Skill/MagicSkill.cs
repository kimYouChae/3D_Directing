using UnityEngine;

// 스킬1 마법: 사거리 안의 모든 적 발밑에 원 생성 -> 일정 시간 뒤 폭발
public class MagicSkill : SkillBase
{
    private readonly MagicSkillData data;

    private bool spawned;   // 이번 사용에서 원을 이미 만들었는지

    public MagicSkill(MikuCharacter owner, MagicSkillData data) : base(owner, data)
    {
        this.data = data;
    }

    protected override void OnBegin()
    {
        spawned = false;
    }

    public override void Tick(float t)
    {
        if (spawned || t < data.SpawnTiming) return;
        spawned = true;

        // 폭발 반경 = 판정 범위
        foreach (Enemy enemy in FindEnemies(owner.transform.position, data.CastRange))
            MagicCircle.Spawn(enemy.FeetPosition, Range, data.ExplodeDelay, Damage, owner.EnemyLayer);
    }
}
