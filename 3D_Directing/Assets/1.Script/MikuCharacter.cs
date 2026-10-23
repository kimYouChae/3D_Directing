
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering;

public enum PlayerState 
{ 
    Locomotion, 
    Attacking,
    Dodging,
    Skill,
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
    [SerializeField] private float searchRadius;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform targetTransform;
    [Range(0.3f, 1f)][SerializeField] private float attackRange;       // 흡착 목표 거리 
    [Range(0.3f, 1f)][SerializeField] private float dashThreshold;     // 이보다 멀면 대시
    private float snapSpeed;
    [SerializeField] AnimationClip dashAnimation;

    const string AttackParameter = "Attack";
    const string HasTargetParameter = "HasTarget";
    const string DashSpeedParameter = "DashSpeed";
    const string SkillIndexParameter = "SkillIndex";   // Dash 뒤 어느 스킬로 갈지 (0 = 기본 공격)
    const string DashStateName = "Dash";

    #region Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    #endregion

    #region Dash
    [Header("===Dash===")]
    private float dashAverageSpeed = 3f; // 평균 대시 속도 
    private float dashMaxSpeed = 0.3f; // 최소 대시 시간 (> 들어가는 전환 + 나가는 전환)
    private float dashMinSpeed = 0.6f; // 최대 대시 시간 
    private float dashToAttackAnimationBlendTime = 0.15f;

    private bool isApproaching;     // 콤보 사이 접근 대시 중인지 (DashApproach / AttackState가 관리)
    public bool IsApproaching { get => isApproaching; set => isApproaching = value; }
    [SerializeField, Range(0f, 0.5f)] private float dashCancelGrace;   // DashEnd 뒤에도 스킬 캔슬을 받아 주는 시간
    private float dashEndTime = float.NegativeInfinity;                       // 마지막 DashEnd 시간
    #endregion


    #region Attack
    [Header("===Attack===")]
    [SerializeField] private WeaponBlade weapon;

    [SerializeField] private bool isWindowOpen = false;
    [SerializeField] private bool comboQueue = false;
    [SerializeField] private bool isPlayingAttack = false;

    [SerializeField] private Transform blade;   // 칼 오브젝트 (대파)
    [SerializeField] private float bladeLength;

    // 이번 공격에서 이미 맞은 적 (중복 타격 방지)
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
    public bool ComboQueue { get => comboQueue; set => comboQueue = value; }
    public bool IsWindowOpen { get => isWindowOpen; set => isWindowOpen = value; }
    public bool IsPlayingAttack { get => isPlayingAttack; set => isPlayingAttack = value; }
    #endregion

    #region HitStop
    [Header("===HitStop===")]
    [SerializeField] private float hitStopDuration;    // 기본 공격 멈춤 시간 (실제 시간, 스킬은 SO)
    [SerializeField] private float hitStopTimeScale;   // 멈춘 동안 timeScale (공격·스킬 공통, 0이면 완전 정지)
    private bool isHitStop;
    private float hitStopEndTime;   // 끝나는 시각 (unscaledTime)
    #endregion

    #region CameraShake
    [Header("===CameraShake===")]
    [SerializeField] private CinemachineImpulseSource hitImpulse;   // 스킬 타격 셰이크 (모양은 Inspector, 세기는 스킬 SO)
    #endregion

    #region Dodge
    [Header("===Dodge===")]
    [SerializeField] AnimationClip dodgeAnimation;
    private float dodgeDistance = 2f;   // 이동거리
    private float dodgeDuration = 0.5f; // 대시 시간
    private AnimationCurve dodgeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // 퍼펙트 회피 슬로우 (x: 회피 진행도 0~1, y: timeScale)
    [SerializeField] private AnimationCurve perfectDodgeTimeCurve;
    // 퍼펙트 회피 판정 반경 (이 안에 창이 열린 적이 있으면 퍼펙트)
    [SerializeField] private float perfectDodgeRadius;
    private bool isPerfectDodge;    // 이번 회피가 퍼펙트인지

    // 퍼펙트 회피 화면 어두워짐 (x: 회피 진행도 0~1, y: Volume weight)
    [SerializeField] private Volume dodgeFXVolume;
    [SerializeField] private AnimationCurve perfectDodgeVolumeCurve;

    // 퍼펙트 회피 잔상
    [SerializeField] private Material afterImageMaterial;
    [SerializeField] private float afterImageInterval;   // 진행도 간격
    [SerializeField] private float afterImageLifeTime;   // 잔상 유지 시간 (게임 시간)
    private SkinnedMeshRenderer[] afterImageRenderers;
    private float nextAfterImageT;  // 다음 잔상을 찍을 진행도

    private Vector3 lastMoveDir;    // 마지막 이동 방향 (월드)
    private Vector3 dodgeDir;       // 닷지 방향 
    private float dodgeElapsed;     // 진행도 

    const string DodgeStateName = "Dodge";
    const string DodgeSpeedParameter = "DodgeSpeed";
    #endregion

    #region Skill
    [Header("===Skill===")]
    // 스킬 데이터(SO), 1·2·3 키 순서
    // 순서 주의: SkillIndex = 순서 + 1 -> Animator의 Dash -> Skill2/3 전이 조건과 맞물림 (1: 마법, 2: 어퍼컷, 3: 내려찍기)
    [SerializeField] private SkillData[] skillDatas;
    private ISkill[] skills;        // 1, 2, 3 키 순서
    private ISkill currentSkill;    // 지금 사용 중인 스킬 (없으면 null)
    public LayerMask EnemyLayer => enemyLayer;
    #endregion

    private void Start()
    {
        defaultCam.Priority = 10;
        playerState = PlayerState.Locomotion;

        animator = GetComponent<Animator>();    
        afterImageRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // 셰이크는 실제 시간으로 재생 -> 히트스톱(timeScale↓) 중에도 흔들림
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;

        // 스킬 데이터(SO)로 실행용 스킬 생성 (실행 중 상태는 스킬 객체가 가짐)
        skills = new ISkill[skillDatas.Length];
        for (int i = 0; i < skillDatas.Length; i++)
            skills[i] = skillDatas[i].CreateSkill(this);
    }

    private void Update()
    {
        TickHitStop();
        ReadInput();

        switch (playerState) 
        {
            case PlayerState.Locomotion:
                movement.Tick(jumpInput, sprintInput, moveInput);
                Vector3 before = transform.position;
                Debug.DrawLine(before, transform.position, Color.cyan, 2f);   // 일반 이동 경로 (하늘색)
                jumpInput = false;
                break;
            case PlayerState.Attacking:
                break;
            case PlayerState.Dodging:
                TickDodge();
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
        isApproaching = false;
        dashEndTime = float.NegativeInfinity;   // 새 공격 시작 -> 이전 대시의 유예는 끝

        targetTransform = FindTarget();
        if(targetTransform == null)
            Debug.Log( "타겟 없음");

        playerState = PlayerState.Attacking;
        
        if (targetTransform != null)
            PrepareApproach();


        animator.SetTrigger(AttackParameter);
    }

    // 타깃을 바라보고, 멀면 흡착 대시 속도·애니 속도 계산 (공격·스킬 공용)
    // 반환: 대시가 필요한지
    private bool PrepareApproach()
    {
        Vector3 dir = targetTransform.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        float dist = dir.magnitude; // 적과 나 사이의 거리
        bool needDash = dist > dashThreshold;   // dashThresh 보다 길면 true
        animator.SetBool(HasTargetParameter, needDash); // dash 애니메이션 

        // 대시가 필요하면
        if (needDash) 
        {
            // 실제로 이동할 거리 
            float travel = dist - attackRange;
            // 대시 시간 = 거리 / 임시 시간 
            // 최소, 최대 설정 
            float dashTime = Mathf.Clamp(travel / dashAverageSpeed, dashMaxSpeed, dashMinSpeed);

            // 스냅 속도 = 거리 / 시간 
            snapSpeed = travel / dashTime;

            // 애니메이션 배속 ( 배속이 0.5이면 애니메이션 재생속도가 2배 )
            animator.SetFloat(DashSpeedParameter, dashAnimation.length / dashTime);

            DashLog.Log($"Attack target={targetTransform.name} dist={dist:F3} dashTime={dashTime:F3}");
        }
        else DashLog.Log($"Attack(no dash) target={targetTransform.name} dist={dist:F3}");

        return needDash;
    }

    public void DoCombo() 
    {
        // 스킬로 캔슬된 뒤에는 콤보로 끼어들지 않음
        if (playerState == PlayerState.Skill) return;

        // AttackState에서 실행, 
        // 입력이 들어왔으면 Attack
        // 아니면 움직임 상태로 

        if (comboQueue) 
        {
            Attack();
        }
        else
        {
            DashLog.Log($"BackToLocomotion remain={RemainDistance():F3}");
            playerState = PlayerState.Locomotion;
        }
    }

    // 공격 시작 시 맞은 적 목록 초기화
    public void ClearHitEnemies() 
    {
        hitEnemies.Clear();
    }

    public void CheckHit(AttackStep stemp) 
    {
        weapon.GetCapsule(out Vector3 p1, out Vector3 p2);

        // 캡슐(point1,2와 반지름)만큼 충돌 검사
        Collider[] hits = Physics.OverlapCapsule(p1, p2, weapon.Radius, enemyLayer);
        Debug.DrawLine(p1, p2, Color.red, 1f);

        bool anyHit = false;   // 이번 프레임에 새로 맞은 적이 있는지 (히트스톱용)

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null) continue;

            // 이미 맞은 적이면 pass
            if (!hitEnemies.Add(enemy)) continue;

            //##TODO : 데미지 하드코딩 수정 필요 
            enemy.TakeDamage(30);
            anyHit = true;
        }

        // 여러 명이 맞아도 한 번만
        if (anyHit) StartHitStop(hitStopDuration);
    }

    public void IdleState() 
    {
        // Dogde에서 Idle로 전환할 수 있도록 막음 
        if (playerState == PlayerState.Dodging) return; 
        currentSkill = null;

        playerState = PlayerState.Locomotion;

        DashLog.Log($"BackToLocomotion remain={RemainDistance():F3}");
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

        // 대시 입력
        if (moveInput != Vector2.zero)
        {
            float yaw = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg
                        + mainCamera.transform.eulerAngles.y;
            lastMoveDir = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
        }

        // 마우스 
        lookInput = mouse.delta.ReadValue();

        // 점프 ( 키가 눌린 첫 프레임만 )
        if (kb.spaceKey.wasPressedThisFrame) jumpInput = true;

        // 달리기 ( 키가 눌려있는 모든 프레임 동안 )
        sprintInput = kb.leftShiftKey.isPressed;

        // 왼 마우스 입력된 순간 
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
            else if(playerState == PlayerState.Locomotion)
            {
                // Attack, Dash중이 아니면 
                Attack();
            }
        }

        // 오른쪽 마우스 입력된 순간
        if (mouse.rightButton.wasPressedThisFrame) 
        {
            // 닷지 하고있거나 컷씬이 아닐 때
            // Attack과 Dash에서는 닷지 가능
            if (playerState != PlayerState.Dodging
                && playerState != PlayerState.Cutscene
                && playerState != PlayerState.Skill)
            {
                StartDodge(CheckPerfectDodge());
            }
        }

        // 입력 감지 로그 (스킬 판정과 별개로, 키가 눌린 프레임 그대로)
        if (kb.digit1Key.wasPressedThisFrame) DashLog.Log($"RawInput key=1 state={playerState} approaching={isApproaching}");
        if (kb.digit2Key.wasPressedThisFrame) DashLog.Log($"RawInput key=2 state={playerState} approaching={isApproaching}");
        if (kb.digit3Key.wasPressedThisFrame) DashLog.Log($"RawInput key=3 state={playerState} approaching={isApproaching}");

        // 스킬 (1, 2, 3)
        if (kb.digit1Key.wasPressedThisFrame) TryUseSkill(0);
        if (kb.digit2Key.wasPressedThisFrame) TryUseSkill(1);
        if (kb.digit3Key.wasPressedThisFrame) TryUseSkill(2);

    }

    #region Skill
    // 스킬을 쓸 수 있는 상태인지 (reason: 막힌 이유, 로그용)
    private bool CanUseSkill(out string reason)
    {
        switch (playerState)
        {
            case PlayerState.Locomotion:
                reason = null;
                return true;

            // 일반 회피는 캔슬 가능, 퍼펙트 회피는 불가
            case PlayerState.Dodging:
                reason = isPerfectDodge ? "perfectDodge" : null;
                return !isPerfectDodge;

            // 콤보 공격 애니 중은 불가, 콤보 사이 접근 대시 중은 가능
            case PlayerState.Attacking:
                // 대시 중이거나, 대시가 끝난 직후(유예 시간 안)면 캔슬 가능
                bool inGrace = Time.time - dashEndTime <= dashCancelGrace;
                reason = (isApproaching || inGrace) ? null : "attackAnim";
                return isApproaching || inGrace;

            // Skill, Cutscene
            default:
                reason = playerState.ToString();
                return false;
        }
    }

    private void TryUseSkill(int index)
    {
        // 키는 3개인데 데이터가 덜 들어간 경우
        if (index >= skills.Length) return;
        ISkill skill = skills[index];

        // 평소 / 일반 회피 중 / 콤보 사이 접근 대시 중에만 사용 가능
        if (!CanUseSkill(out string reason))
        {
            DashLog.Log($"SkillBlocked skill={skill.Name} reason={reason}");
            return;
        }

        if (!skill.IsReady)
        {
            DashLog.Log($"SkillBlocked skill={skill.Name} reason=cooldown remain={skill.CooldownRemain:F2}");
            return;
        }

        PlayerState from = playerState;

        // 일반 회피를 캔슬 -> 상태가 Skill로 바뀌어 TickDodge가 더 이상 안 불림 (이동은 여기서 멈춤)
        if (from == PlayerState.Dodging)
            DashLog.Log($"DodgeCancel elapsed={dodgeElapsed:F3}");

        // 대시가 끝난 뒤 유예 시간 안에 들어온 캔슬 (유예 값 조절용 로그)
        if (from == PlayerState.Attacking && !isApproaching)
            DashLog.Log($"DashGraceCancel since={Time.time - dashEndTime:F3}");
        playerState = PlayerState.Skill;
        currentSkill = skill;

        // 기본 공격을 캔슬하고 들어온 경우 콤보 입력 정리
        comboQueue = false;
        isWindowOpen = false;
        isPlayingAttack = false;

        skill.Begin();

        // 흡착: 타깃이 dashThreshold보다 멀면 Dash 상태를 거쳐 스킬로
        // (Dash 상태의 DashEnd 전환이 SkillIndex로 분기)
        bool dash = false;
        if (skill.UseDash)
        {
            targetTransform = FindTarget();
            if (targetTransform != null) dash = PrepareApproach();
        }

        bool reuseDash = false;
        if (dash)
        {
            animator.SetInteger(SkillIndexParameter, index + 1);

            // 이미 첫 Dash를 재생 중이면 그대로 두고 DashEnd 분기만 바꿈
            // (자기 자신으로 CrossFade하면 처음부터 다시 시작되는지 보장이 없음)
            reuseDash = animator.GetCurrentAnimatorStateInfo(0).IsName(DashStateName)
                        && !animator.IsInTransition(0);
            if (!reuseDash) animator.CrossFadeInFixedTime(DashStateName, 0.05f);
        }
        else
        {
            animator.CrossFadeInFixedTime(skill.StateName, 0.1f);
        }

        DashLog.Log($"SkillStart skill={skill.Name} idx={index + 1} dash={dash} reuseDash={reuseDash} from={from}");
    }

    // DashApproach가 DashEnd를 켤 때: 접근 대시 끝 + 유예 시간 시작
    public void OnDashEnd()
    {
        isApproaching = false;
        dashEndTime = Time.time;
    }

    // SkillState.OnStateEnter: 스킬 상태에 들어왔으면 흡착 분기 값은 원래대로
    // (안 되돌리면 다음 기본 공격 흡착이 스킬로 빠짐)
    public void OnSkillStateEnter()
    {
        animator.SetInteger(SkillIndexParameter, 0);
    }

    // SkillState.OnStateUpdate: 애니메이션 진행도로 스킬 진행
    public void TickSkill(float t)
    {
        currentSkill?.Tick(t);
    }
    #endregion

    private void StartDodge(bool perfect = false)
    {
        // 히트스톱 중 회피 캔슬 -> 먼저 timeScale 복구 (퍼펙트 슬로우를 덮어쓰지 않게)
        EndHitStop();

        PlayerState from = playerState;
        playerState = PlayerState.Dodging;
        dodgeElapsed = 0;

        isPerfectDodge = perfect;
        if (perfect) StartPerfectDodgeFX();

        // 닷지 방향 정하기 
        dodgeDir = lastMoveDir == Vector3.zero ? Vector3.forward : lastMoveDir;
        transform.rotation = Quaternion.LookRotation(dodgeDir);


        // 클립 재생 속도 > 대시 시간에 맞춤
        if (dodgeAnimation != null)
            animator.SetFloat(DodgeSpeedParameter, dodgeAnimation.length / dodgeDuration);

        // 애니메이션 이름으로 전환 
        // 애니메이터에 화살표 연결없이 이름으로 전환할 수 있음
        // (애니메이션 이름, 블랜드 시간)
        animator.CrossFadeInFixedTime(DodgeStateName, 0.05f);

        DashLog.Log($"DodgeStart from={from} dir={dodgeDir} perfect={perfect} real={Time.unscaledTime:F3}");
    }

    private void TickDodge() 
    {
        // 진행도 계산: 지난 프레임 t0 → 이번 프레임 t1 (0~1)
        float t0 = dodgeElapsed / dodgeDuration;
        dodgeElapsed += Time.deltaTime;
        float t1 = Mathf.Min(dodgeElapsed / dodgeDuration, 1f);

        // 슬로우 + 화면 암전 
        if (isPerfectDodge) TickPerfectDodgeFX(t1);

        // 이번 프레임 이동량 = 커브 증가분 × 총거리
        float step = (dodgeCurve.Evaluate(t1) - dodgeCurve.Evaluate(t0)) * dodgeDistance;
        movement.MoveRaw(dodgeDir * step);

        // 끝났으면 종료
        if (t1 >= 1f) EndDodge();
        
        // 이번프레임에 걸린시간 , 진행도 , 이번 프레임 이동량
        DashLog.Log($"DodgeMove dt={Time.deltaTime:F4} t={t1:F3} step={step:F3} ts={Time.timeScale:F2} w={dodgeFXVolume.weight:F2}");
    }

    private void EndDodge() 
    {
        playerState = PlayerState.Locomotion;

        // 연출 복구 (회피 종료 지점은 여기 하나뿐)
        if (isPerfectDodge)
        {
            EndPerfectDodgeFX();
            isPerfectDodge = false;
        }

        DashLog.Log($"DodgeEnd elapsed={dodgeElapsed:F3} real={Time.unscaledTime:F3}");
    }

    // 반경 안에 창이 열린 적이 있으면 그 창을 소비하고 true
    private bool CheckPerfectDodge()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, perfectDodgeRadius, enemyLayer);
        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null || !enemy.IsPerfectWindowOpen) continue;

            DashLog.Log($"PerfectCheck hit={enemy.name} since={enemy.TimeSinceFlash:F3}");
            enemy.ConsumePerfectWindow();   // 한 번 반짝임에 퍼펙트 한 번
            return true;
        }
        return false;
    }

    #region HitStop
    // 히트스톱: 맞은 순간 timeScale을 낮췄다가 실제 시간으로 duration 뒤 복구
    // 게임 시간으로 재면 느려진 동안 시간도 같이 멈춰서 안 풀림 -> unscaledTime
    public void StartHitStop(float duration)
    {
        // 퍼펙트 회피 슬로우가 timeScale을 쓰는 중이면 건드리지 않음
        if (isPerfectDodge) return;

        isHitStop = true;
        hitStopEndTime = Time.unscaledTime + duration;
        Time.timeScale = hitStopTimeScale;

        DashLog.Log($"HitStopStart duration={duration} real={Time.unscaledTime:F3}");
    }

    private void TickHitStop()
    {
        if (isHitStop && Time.unscaledTime >= hitStopEndTime) EndHitStop();
    }

    private void EndHitStop()
    {
        if (!isHitStop) return;

        isHitStop = false;
        Time.timeScale = 1f;

        DashLog.Log($"HitStopEnd real={Time.unscaledTime:F3}");
    }
    #endregion

    #region CameraShake
    // 카메라 셰이크: Impulse Source가 신호를 내면 CM_Default의 Impulse Listener가 흔듦
    public void ShakeCamera(float force)
    {
        hitImpulse.GenerateImpulseWithForce(force);
        DashLog.Log($"CameraShake force={force} real={Time.unscaledTime:F3}");
    }
    #endregion

    #region PerfectDodgeFX
    // 퍼펙트 회피 연출 (슬로우 + 화면 어두워짐 + 잔상)
    // 세 연출 모두 회피 진행도(t, 0~1) 하나로 움직인다

    private void StartPerfectDodgeFX()
    {
        Time.timeScale = perfectDodgeTimeCurve.Evaluate(0f);
        dodgeFXVolume.weight = perfectDodgeVolumeCurve.Evaluate(0f);
        nextAfterImageT = 0f;
    }

    private void TickPerfectDodgeFX(float t)
    {
        // 슬로우: 진행도에 따라 timeScale 조절
        // 0이 되면 deltaTime도 0 -> t가 멈춰 회피가 안 끝나므로 하한 0.05
        Time.timeScale = Mathf.Max(perfectDodgeTimeCurve.Evaluate(t), 0.05f);

        // 화면 어두워짐: 같은 진행도로 Volume weight 조절
        dodgeFXVolume.weight = perfectDodgeVolumeCurve.Evaluate(t);

        // 잔상: 진행도가 interval만큼 오를 때마다 스냅샷
        if (t >= nextAfterImageT)
        {
            AfterImage.Spawn(afterImageRenderers, afterImageMaterial, afterImageLifeTime);
            nextAfterImageT += afterImageInterval;
        }
    }

    private void EndPerfectDodgeFX()
    {
        Time.timeScale = 1f;
        dodgeFXVolume.weight = 0f;
    }
    #endregion

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

    public void MoveToTarget()
    {
        if (targetTransform == null) return;

        Vector3 dir = targetTransform.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        // 거리가 attackRange보다 멀때만 흡착
        // 가까우면 X 
        // 0.01f : 허용 오차 
        if (dist > attackRange + 0.01f)
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

            DashLog.Log($"Move dt={Time.deltaTime:F4} remain={dist - attackRange:F3} move={move.magnitude:F3}");
        }
    }

    // 타겟까지 남은 거리 (attackRange 기준). 타겟이 없으면 -1
    public float RemainDistance()
    {
        if (targetTransform == null) return -1f;

        Vector3 dir = targetTransform.position - transform.position;
        dir.y = 0;
        return dir.magnitude - attackRange;
    }

    // 공격으로 전환해도 되는지 여부를 리턴

    public bool CanStartAttack() 
    {
        if (targetTransform == null) return true;

        Vector3 dir = targetTransform.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        // 남은 거리가 0.25초안에 끝나면 미리 dashend로 전환하기
        // ( dash -> attack애니메이션 전환시간 )
        // 남은 거리 < 속도 * 시간 
        float remain = dist - attackRange;

        // 지금 속도로 0.25초 동안 갈 수 있는 거리 ( 거 = 속 * 시 )
        // 갈 수 있으면 true, 아니면 false 
        // N : Dash > Attack 애니메이션으로 전환 blend 시간 
        return remain <= snapSpeed * dashToAttackAnimationBlendTime;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere (transform.position, dashThreshold);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, perfectDodgeRadius);
    }
}
