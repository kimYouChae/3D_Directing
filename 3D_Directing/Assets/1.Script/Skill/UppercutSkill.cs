using UnityEngine;

// 스킬2 어퍼컷: 판정 구간 동안 맞은 적을 띄운다
public class UppercutSkill : SkillBase
{
    private readonly float hitStart = 0.2f;     // 판정 시작 진행도 (임시)
    private readonly float hitEnd = 0.8f;       // 판정 끝 진행도 (임시)
    private readonly float launchHeight = 3f;   // 띄우는 높이
    private readonly float launchDuration = 1f; // 떠 있는 시간

    public UppercutSkill(MikuCharacter owner)
        : base(owner, "Uppercut", "Skill2", cooldown: 10f, range: 2f, damage: 1f, useDash: true) { }

    public override void Tick(float t)
    {
        HitWindow(t, hitStart, hitEnd, range, enemy =>
        {
            enemy.TakeDamage(damage);
            enemy.Launch(launchHeight, launchDuration);
        });
    }
}
