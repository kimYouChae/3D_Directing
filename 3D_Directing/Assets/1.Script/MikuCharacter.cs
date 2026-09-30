using System;
using System.Collections;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;

public enum PlayerState 
{ 
    Locomotion, 
    Attacking, 
    Cutscene 
}

public class MikuCharacter : MonoBehaviour
{
    public PlayerState playerState;

    [Header("===Component===")]
    [SerializeField]
    private MikuMovement movement;
    [SerializeField]
    private Animator animator;

    [Header("===Timeline===")]
    [SerializeField]
    private PlayableDirector introDirector;

    [Header("===Cinemachine===")]
    [SerializeField] CinemachineCamera defaultCam;

    const string AttackParameter = "Attack";

    #region Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    #endregion

    #region Attack
    [Header("===Attack===")]
    [SerializeField] private bool isWindowOpen = false;
    [SerializeField] private bool comboQueue = false;
    [SerializeField] private bool isPlayingAttack = false;

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

}
