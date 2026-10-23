using UnityEngine;

// 스킬3 내려찍기 데이터
[CreateAssetMenu(fileName = "SlamSkillData", menuName = "Skill/Slam")]
public class SlamSkillData : SkillData
{
    [Header("===Slam===")]
    [SerializeField] private float hitStart;            // 판정 시작 진행도
    [SerializeField] private float hitEnd;              // 판정 끝 진행도
    [SerializeField] private float impactRadius;        // 충격 반경
    [SerializeField] private float knockbackDistance;   // 밀어내는 거리
    [SerializeField] private float knockbackDuration;   // 밀려나는 시간

    public float HitStart => hitStart;
    public float HitEnd => hitEnd;
    public float ImpactRadius => impactRadius;
    public float KnockbackDistance => knockbackDistance;
    public float KnockbackDuration => knockbackDuration;

    public override ISkill CreateSkill(MikuCharacter owner) => new SlamSkill(owner, this);
}
