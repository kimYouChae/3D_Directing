using UnityEngine;

// 궁극기: Timeline 진행도가 spawnTiming이 되면 장판(UltField)을 한 번 생성 (MagicSkill과 같은 구조)
// 장판은 독립 오브젝트라 컷씬이 끝나도 계속 동작
public class UltSkill : SkillBase
{
    private readonly UltSkillData data;

    private bool spawned;   // 이번 사용에서 장판을 이미 생성했는지

    public UltSkill(MikuCharacter owner, UltSkillData data) : base(owner, data)
    {
        this.data = data;
    }

    protected override void OnBegin()
    {
        spawned = false;
    }

    protected override void OnTick(float t)
    {
        if (spawned || t < data.SpawnTiming) return;
        spawned = true;

        // 장판 반경 = range, 틱 데미지 = damage
        UltField.Spawn(owner.transform.position, Range, data.FieldDuration, data.TickInterval, Damage,
                       data.FreezeDuration, owner.EnemyLayer, data.SnowEffect, data.FreezeEffect);
    }
}
