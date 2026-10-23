using UnityEngine;

// 스킬2 어퍼컷: 판정 구간 동안 맞은 적을 띄운다
public class UppercutSkill : SkillBase
{
    private readonly UppercutSkillData data;

    public UppercutSkill(MikuCharacter owner, UppercutSkillData data) : base(owner, data)
    {
        this.data = data;
    }

    protected override void OnTick(float t)
    {
        HitWindow(t, data.HitStart, data.HitEnd, Range, enemy =>
        {
            enemy.TakeDamage(Damage);
            enemy.Launch(data.LaunchHeight, data.LaunchDuration);
        });
    }
}
