using UnityEngine;

public class SkillState : StateMachineBehaviour
{
    /// <summary>
    /// Skill1, Skill2, Skill3 상태 모두 해당 스크립트를 사용함
    /// 애니메이션 진행도를 MikuCharacter -> 현재 스킬에 넘겨준다
    /// </summary>

    private MikuCharacter character;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character = animator.GetComponent<MikuCharacter>();
        character.OnSkillStateEnter();

        DashLog.Log($"SkillEnter state={stateInfo.shortNameHash}");
    }

    // enter ~ exit 사이 매 프레임마다
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character.TickSkill(stateInfo.normalizedTime);
    }
}
