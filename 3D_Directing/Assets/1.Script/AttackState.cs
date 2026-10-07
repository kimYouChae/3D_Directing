using UnityEngine;

public class AttackState : StateMachineBehaviour
{
    /// <summary>
    /// 콤보에 들어가는 스킬 3개 모두 해당 스크립트를 사용함
    /// canCombo 는 스킬 연계가 가능한것만 true
    /// ex) 스킬1 ,2는 true / 스킬 3은 false
    /// </summary>

    [Range(0f, 1f)] public float comboWindow = 0.5f; // 콤보 입력이 가능한 진행률
    [Range(0f, 1f)] public float changeState = 0.8f; // 다음 Attack으로 넘어가는 진행률 
    [Range(0f, 1f)] public float hitTiming = 0.2f; // hit 시작하는 진행률 
    [Range(0f, 1f)] public float hitTimingEnd = 0.7f; // hit 끝내는 진행률
    public AttackStep step;

    private MikuCharacter character;
    private bool isWindowOpen;  // 입력 받을 수 있는 구간이 열려있는지 
    private bool doCombo;

    // 해당 상태 실행될 때 
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character = animator.GetComponent<MikuCharacter>();

        character.IsWindowOpen = false;
        character.ComboQueue = false;
        character.IsPlayingAttack = true;

        isWindowOpen = false;
        doCombo = false;

        // 새 공격 시작 -> 맞은 적 목록 초기화
        character.ClearHitEnemies();

        DashLog.Log($"AttackEnter step={step} remain={character.RemainDistance():F3}");
    }

    // enter ~ exit 사이 매 프레임마다 
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var t = stateInfo.normalizedTime;

        // hit 구간 안에 있으면 매 프레임 판정 (중복은 HashSet이 걸러줌)
        if (t >= hitTiming && t <= hitTimingEnd)
            character.CheckHit(step);

        if (t > comboWindow && !isWindowOpen) 
        {
            character.IsWindowOpen = true;
            isWindowOpen = true;
        }

        if (!doCombo && t > changeState && character.ComboQueue)
        {
            character.DoCombo();
            doCombo = true;
        }
    }

    // 상태가 끝날 때
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // character.IsPlayingAttack = false;
        // character.ComboQueue = false;
        // character.IsWindowOpen = false;
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
