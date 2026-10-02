using System;
using System.Collections;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public enum PlayerState 
{ 
    Locomotion, 
    Attacking, 
    Cutscene 
}
public enum AttackStep { First, Second, Third }

public class MikuCharacter : MonoBehaviour
{
    public PlayerState playerState;

    [Header("===Component===")]
    [SerializeField]
    private MikuMovement movement;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private GameObject mainCamera;

    [Header("===Timeline===")]
    [SerializeField]
    private PlayableDirector introDirector;

    [Header("===Cinemachine===")]
    [SerializeField] CinemachineCamera defaultCam;

    [Header("===Targeting===")]
    [SerializeField] private float searchRadius = 1.7f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform targetTransform;
    private float attackRange = 0.7f;       // 흡착 목표 거리 
    private float dashThreshold = 1f;     // 이보다 멀면 대시
    private float snapSpeed;
    private float dashClipLength = 0.6f;

    const string AttackParameter = "Attack";
    const string HasTargetParameter = "HasTarget";
    const string DashSpeedParameter = "DashSpeed";

    #region Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    #endregion

    #region Attack
    [Header("===Attack===")]
    [SerializeField] private WeaponBlade weapon;

    [SerializeField] private bool isWindowOpen = false;
    [SerializeField] private bool comboQueue = false;
    [SerializeField] private bool isPlayingAttack = false;

    [SerializeField] private Transform blade;   // 칼 오브젝트 (대파)
    [SerializeField] private float bladeLength;
    public bool ComboQueue { get => comboQueue; set => comboQueue = value; }
    public bool IsWindowOpen { get => isWindowOpen; set => isWindowOpen = value; }
    public bool IsPlayingAttack { get => isPlayingAttack; set => isPlayingAttack = value; }
    #endregion

    private void Start()
    {
        defaultCam.Priority = 10;
        playerState = PlayerState.Locomotion;

        animator = GetComponent<Animator>();    
    }

    private void Update()
    {
        ReadInput();

        switch (playerState) 
        {
            case PlayerState.Locomotion:
                movement.Tick(jumpInput, sprintInput, moveInput);
                jumpInput = false;
                break;
            case PlayerState.Attacking:
                break;
            case PlayerState.Cutscene:
                break;
        }
    }
    private void LateUpdate()
    {
        movement.CameraTick(lookInput);
    }

    private void Attack() 
    {
        animator.SetBool(HasTargetParameter , false);

        targetTransform = FindTarget();
        Debug.Log(targetTransform != null ? $"타겟: {targetTransform.name}" : "타겟 없음");
        if (targetTransform != null) 
        {
            Vector3 dir = targetTransform.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);

            float dist = dir.magnitude; // 적과 나 사이의 거리
            bool needDash = dist > dashThreshold;   // dashThresh 보다 길면 true
            animator.SetBool(HasTargetParameter, needDash); // dash 애니메이션 

            // 대시가 필요하면
            if (needDash)
                snapSpeed = (dist - attackRange) / dashClipLength;
        }

        playerState = PlayerState.Attacking;

        animator.SetTrigger(AttackParameter);
    }

    public void DoCombo() 
    {
        if (comboQueue) 
        {
            Attack();
        }
        else
        {
            playerState = PlayerState.Locomotion;
        }
    }

    public void Dohit(AttackStep attackStep) 
    {
            
    }

    public void temp(float damage) 
    {
        weapon.GetCapsule(out Vector3 p1, out Vector3 p2);

        // 캡슐(point1,2와 반지름)만큼 충돌 검사
        Collider[] hits = Physics.OverlapCapsule(p1, p2, weapon.Radius, enemyLayer);

        foreach (var hit in hits)
        { 
            Debug.Log($"{hit.name} 피격 / 데미지 {damage}");
        
        }
    }

    public void OnHitBox() 
    {
    
    }
    public void OffHitBox() 
    {
    
    }

    public void IdleState() 
    {
        playerState = PlayerState.Locomotion;
    }

    private void ReadInput()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // WASD 입력
        float x = 0, y = 0;
        if (kb.aKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.sKey.isPressed) y -= 1f;
        if (kb.wKey.isPressed) y += 1f;
        moveInput = new Vector2(x, y).normalized;

        // 마우스 
        lookInput = mouse.delta.ReadValue();

        // 점프 ( 키가 눌린 첫 프레임만 )
        if (kb.spaceKey.wasPressedThisFrame) jumpInput = true;

        // 달리기 ( 키가 눌려있는 모든 프레임 동안 )
        sprintInput = kb.leftShiftKey.isPressed;

        // 마우스 입력된 순간 
        if (mouse.leftButton.wasPressedThisFrame)
        {
            // attack중이고
            if (isPlayingAttack)
            {
                // 입력 윈도우가 켜져있으면 
                if (isWindowOpen)
                {
                    // 콤보 세팅
                    comboQueue = true;
                }
            }
            else 
            {
                // Attack중이 아니면 
                Attack();
            }
        }

    }

    private Transform FindTarget() 
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, enemyLayer);
        if (hits.Length == 0) return null;

        Transform closer = null;
        float closerDistace = float.MaxValue;

        // 카메라 앞쪽 
        Vector3 camForward = mainCamera.transform.forward;
        camForward.y = 0;

        foreach (Collider collider in hits) 
        {
            // 거리 구하기 
            Vector3 dir = collider.transform.position - transform.position;
            // 높이는 계산 X 
            dir.y = 0;

            float angle = Vector3.Angle(camForward, dir);

            // 뒤의 적은 제외
            if (angle > 120f) continue;

            // 가장 가까운 거리 비교
            float dist = dir.magnitude;
            if (dist < closerDistace)
            {
                closerDistace = dist;
                closer = collider.transform;
            }
        } 

        return closer;
    }

    public void SnapToTarget() 
    {
        if (targetTransform == null) return;

        Vector3 dir = targetTransform.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude; // 벡터사이의 거리

        // 거리가 attackRange보다 멀때만 흡착
        // 가까우면 X 
        if (dist > attackRange) 
        {
            // 방향 * 1초안에 가야할 속도 * 프레임별로 가야하니까 deltaTime(0.0167)
            // = 이번 프레임 이동량
            Vector3 move = dir.normalized * snapSpeed * Time.deltaTime;
            
            // 거리 보정 
            // 이번 프레임에 움직여야 할 거리보다, 남은 거리가 더 적으면
            // 적은 거리만큼 움직여야함 ! 
            // 이번 프레임거리만큼 움직이면 -> attackRange 보다 더 가까이 다다가게됨 
            if (move.magnitude > dist - attackRange) 
            {
                // 이번 프레임 이동량 = 남은거리 만큼 
                move = dir.normalized * (dist - attackRange);  
            }

            // 움직이기 
            // ex) (0.033, 0, 0.044)
            movement.MoveRaw(move); 
        }
        // 거리가 AttackRange보다 크면 
        // -> Dash 애니메이션 종료 
        else
        {
            animator.SetTrigger("DashEnd");   // 도착했으니 공격으로
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere (transform.position, dashThreshold);
    }
}
