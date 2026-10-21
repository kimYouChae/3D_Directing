using UnityEngine;

// 스킬 공통 약속
// MikuCharacter는 이 인터페이스만 보고 스킬을 실행한다
public interface ISkill
{
    string Name { get; }
    string StateName { get; }       // Animator 상태 이름 (Skill1, Skill2, Skill3)
    bool UseDash { get; }           // 흡착(Dash) 사용 여부
    bool IsReady { get; }           // 쿨타임이 끝났는지
    float CooldownRemain { get; }   // 남은 쿨타임 (로그용)

    // 버튼으로 사용이 확정됐을 때 (쿨타임 시작, 내부 초기화)
    void Begin();

    // 스킬 애니메이션 진행도(t)마다 호출 (SkillState.OnStateUpdate)
    void Tick(float t);
}
