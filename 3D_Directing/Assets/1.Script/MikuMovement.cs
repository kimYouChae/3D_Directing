
using UnityEngine;
using UnityEngine.InputSystem;

public class MikuMovement : MonoBehaviour
{
    [SerializeField] MikuCharacter character;

    [Header("Component")]
    [SerializeField] public GameObject cinemachindCameraTarget;
    // [SerializeField] private PlayerInput playerInput;
    [SerializeField] private CharacterController controller;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private Animator animator;

    #region Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    #endregion

    #region Camera Rotationa
    private float lookSensitivity = 0.05f;

    private bool lockCameraPosition = false;

    private const float threshold = 0.01f;
    
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private float topClamp = 70f;
    private float bottomClamp = -30f;

    #endregion 

    #region Animation
    private bool hasAnimator;

    private float animationBlend;
    #endregion

    #region Move
    private float speed;
    private float speedChangerate = 10f; // 속도내기 <-> 감소하기 사이 비율

    private float moveSpeed = 2f;
    private float sprintSpeed = 5.3f;

    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float rotationSmothTime = 0.12f;  // 캐릭터 회전 속도
    #endregion

    #region Jump
    private float jumpHeight = 1.2f;   // 점프 높이
    private float gravity = -15f;       // 중력값

    private float jumpTimeOut = 0.50f;  // 점프 - 점프 쿨타임 
    private float falltimeOut = 0.15f; 
    // 땅에서 떨어진 후 "낙하 애니메이션 켤 유예시간 , x만큼 떠있어야 공중에 뜬것 < 이라는 판정시간 

    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    #endregion

    #region Grounded
    private bool grounded = true;   // 땅에 있는지 여부
    private float grounrdedOffset = -0.14f;
    private float groundedRadious = 0.28f;
    public LayerMask groundLayer;   // 캐릭터가 땅으로 인식하는 레이어
    #endregion

    private void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    private void Start()
    {
        // 커서 잠금 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller = GetComponent<CharacterController>();
        
        // playerInput = GetComponent<PlayerInput>();
        hasAnimator = TryGetComponent(out animator);

        // 점프 타임아웃 세팅 
        jumpTimeoutDelta = jumpTimeOut;
        fallTimeoutDelta = falltimeOut;
    }

    private void Update()
    {
        if (character.playerState != PlayerState.Locomotion)
            return;

        ReadInput();
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
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
        moveInput = new Vector2(x,y).normalized;

        // 마우스 
        lookInput = mouse.delta.ReadValue();

        // 점프 ( 키가 눌린 첫 프레임만 )
        if (kb.spaceKey.wasPressedThisFrame) jumpInput = true;

        // 달리기 ( 키가 눌려있는 모든 프레임 동안 )
        sprintInput = kb.leftShiftKey.isPressed;
    }

    private void JumpAndGravity()
    {
        if (grounded)
        {
            // reset the fall timeout timer
            fallTimeoutDelta = falltimeOut;

            if (hasAnimator)
            {
               // 애니메이터 업데이트
            }

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // Jump
            if (jumpInput&& jumpTimeoutDelta <= 0.0f)
            {
                // H * -2 * G : 등가속도 운동 공식 
                // 대충 jumpHeight만큼 뛰게 됨 . 
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (hasAnimator)
                {
                    // 점프 애니메이션
                }
            }

            // jump timeout
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            jumpTimeoutDelta = jumpTimeOut;

            // fall timeout
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (hasAnimator)
                {
                    // 떨어지는 애니메이션
                }
            }

            // 끝나면 false로
            jumpInput = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck() 
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - grounrdedOffset,
            transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadious, groundLayer,
            QueryTriggerInteraction.Ignore);
        
        if (hasAnimator)
        {
            // 땅에 착지하는 애니메이션
        }
    }

    private void Move() 
    {
        // 1. 목표 속도 정하기
        float targetSpeed = sprintInput ? sprintSpeed : moveSpeed;

        if (moveInput == Vector2.zero) targetSpeed = 0.0f;

        // 2. 실제 속도를 목표 속도에 부드럽게 
        // 직전 프레임에 얼마나 움직였는지(속도만)
        float currentHorizontalSpeed
            = new Vector3(controller.velocity.x,
            0.0f,
            controller.velocity.z).magnitude;

        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // 아직 멀었으면 -> 부드럽게 이동 
            // Lerp : 현재값과 목표값을 비율로 좁힘

            speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                Time.deltaTime * speedChangerate);
        }
        else 
        {
            // 가까우면 > 속도 딱 맞추기
            speed = targetSpeed;
        }

        //애니메이션 blend
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangerate);
        if(animationBlend < 0.01f) animationBlend = 0.0f;

        // 3. 캐릭터 회전 
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y);

        // 움직이고 있으면 
        if (moveInput != Vector2.zero) 
        {
            // 입력 방향 각도 + 카메라가 보는 각도 
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                mainCamera.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, 
                targetRotation, ref rotationVelocity, rotationSmothTime);

            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        Vector3 targetDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

        // 플레이어 움직임 
        controller.Move( targetDirection.normalized * (speed * Time.deltaTime) +
            new Vector3(0f, verticalVelocity, 0) * Time.deltaTime);
    
        // 애니메이션 실행 
    }

    private void CameraRotation() 
    {
        // 마우스 미세 떨림 검사
        // && 연출 중 조작 막을 플래그
        if (lookInput.sqrMagnitude >= threshold
            && !lockCameraPosition) 
        {
            cinemachineTargetYaw += lookInput.x * lookSensitivity;
            cinemachineTargetPitch += lookInput.y * lookSensitivity;
        }

        // Yaw : 좌우 ( 각도 제한 없음)
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        // Pitch : 상하 ( -30 ~ 70사이 )
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // 회전 적용
        cinemachindCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch,
            cinemachineTargetYaw , 0);
    }

    private float ClampAngle(float lfAngle, float lfMin, float lfMax) 
    {
        // 각도를 -360 ~ 360사이로 돌림
        if(lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
