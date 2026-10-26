using UnityEngine;

// 스킬2 어퍼컷 데이터
[CreateAssetMenu(fileName = "UppercutSkillData", menuName = "Skill/Uppercut")]
public class UppercutSkillData : SkillData
{
    [Header("===Uppercut===")]
    [SerializeField] private float hitStart;        // 판정 시작 진행도
    [SerializeField] private float hitEnd;          // 판정 끝 진행도
    [SerializeField] private float launchHeight;    // 띄우는 높이
    [SerializeField] private float launchDuration;  // 떠 있는 시간

    public float HitStart => hitStart;
    public float HitEnd => hitEnd;
    public float LaunchHeight => launchHeight;
    public float LaunchDuration => launchDuration;

    public override ISkill CreateSkill(MikuCharacter owner) 
    {
        return new UppercutSkill(owner, this);
    } 
}
