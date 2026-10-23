using UnityEngine;

// 스킬 공통 데이터 (값은 에셋 Inspector에서만 설정)
// 실행 중 상태(쿨타임 시각, 맞은 적 목록)는 여기 넣지 않는다
// -> SO는 에셋이라 모두가 공유하고, 에디터에선 플레이 중 바뀐 값이 남기 때문
public abstract class SkillData : ScriptableObject
{
    [Header("===Common===")]
    [SerializeField] private string skillName;      // 로그용 이름
    [SerializeField] private string stateName;      // Animator 상태 이름 (Skill1, Skill2, Skill3)
    [SerializeField] private float cooldown;        // 쿨타임
    [SerializeField] private float range;           // 판정 범위
    [SerializeField] private float damage;          // 데미지
    [SerializeField] private bool useDash;          // 흡착(Dash) 사용 여부

    public string SkillName => skillName;
    public string StateName => stateName;
    public float Cooldown => cooldown;
    public float Range => range;
    public float Damage => damage;
    public bool UseDash => useDash;

    // 이 데이터로 실행용 스킬 객체를 만든다 (행동은 스킬마다 다른 클래스)
    public abstract ISkill CreateSkill(MikuCharacter owner);
}
