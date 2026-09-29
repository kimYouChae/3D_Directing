using UnityEngine;

public class Attack_Basic : StateMachineBehaviour
{
    /// <summary>
    /// 콤보에 들어가는 스킬 3개 모두 해당 스크립트를 사용함
    /// canCombo 는 스킬 연계가 가능한것만 true
    /// ex) 스킬1 ,2는 true / 스킬 3은 false
    /// </summary>

    [Range(0f, 1f)] public float hitboxOn = 0.3f;
    [Range(0f, 1f)] public float hitboxOff = 0.5f;
    [Range(0f, 1f)] public float comboWindow = 0.5f; // 콤보 입력이 가능한 진행률
    public bool canCombo = true;
 
    private MikuCharacter character;
    private bool isHitboxOn;
    private bool isWindowOpen;  // 입력 받을 수 있는 구간이 열려있는지 

    // 해당 상태 실행될 때 
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character = animator.GetComponent<MikuCharacter>();
        isHitboxOn = false;
        isWindowOpen = true;
    }

    // enter ~ exit 사이 매 프레임마다 
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 0~1 진행률    
        float t = stateInfo.normalizedTime;

        if (!isHitboxOn && t > hitboxOn) 
        {
            // 캐릭터의 hitBox ON
            character.HitBoxOn();
            isHitboxOn = true;
        }
        if (isHitboxOn && t < hitboxOff) 
        {
            // 캐릭터의 hitBox Off
            character.HitBoxOff();
            isHitboxOn = false;
        }

        // 콤보 입력 받기
        if (canCombo && isWindowOpen && t > comboWindow) 
        {
            isWindowOpen = false;
            character.OpenComboWindow();
        }


    }

    // 상태가 끝날 때
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character.OnAttackEnd();
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
