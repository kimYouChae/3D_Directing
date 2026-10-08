using UnityEngine;

public class DashApproach : StateMachineBehaviour
{
    [SerializeField] private float maxDashTime = 1.5f;

    private MikuCharacter character;
    private bool dashEnded;
    private float elapsed;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        character = animator.GetComponent<MikuCharacter>();
        dashEnded = false;
        elapsed = 0f;
        animator.ResetTrigger("DashEnd");   // 이전에 남은 트리거 제거

        DashLog.Log($"DashEnter state={stateInfo.shortNameHash} remain={character.RemainDistance():F3}");
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        elapsed += Time.deltaTime;

        // 움직이기 
        character.MoveToTarget();

        // DashEnD 를 실행할 준비가 되었는지 확인 
        bool ready = character.CanStartAttack();

        if (!dashEnded) 
        {
            // 전환 준비가 되었거나
            // max 시간 초과시
            // > DashEnd 실행 
            if ( ready || elapsed >= maxDashTime)
            {
                DashLog.Log($"DashEnd reason={(ready ? "ready" : "timeout")} elapsed={elapsed:F3} remain={character.RemainDistance():F3}");

                animator.SetTrigger("DashEnd");
                dashEnded = true;
            }
        }
        
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
