using System;
using System.Collections;
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

public class MikuCharacter : MonoBehaviour
{
    public PlayerState playerState;

    public MikuMovement movement;

    [Header("===Component===")]
    [SerializeField]
    private Animator playerAnimator;
    [SerializeField]
    private Animator enemyAnimator;
    [SerializeField]
    private bool isPlaying;

    [Header("===Timeline===")]
    [SerializeField]
    private PlayableDirector introDirector;

    [Header("===Cinemachine===")]
    [SerializeField] CinemachineCamera defaultCam;
    [SerializeField] CinemachineCamera AttackAction_First;
    [SerializeField] CinemachineCamera AttackAction_Second;
    [SerializeField] CinemachineCamera AttackAction_Third;

    const string AttackParameter = "Attack";

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        isPlaying = true;

        defaultCam.Priority = 10;

        playerState = PlayerState.Locomotion;
        // PlayOpening();
    }

    private void PlayOpening() 
    {
        playerState = PlayerState.Cutscene;

        // intro가 끝나면 액션 등록 
        introDirector.stopped += IntroStopped;
        // intro 실행 
        introDirector.Play();
    }

    private void IntroStopped(PlayableDirector pd) 
    {
        playerState = PlayerState.Locomotion;
    }

    public void Attack() 
    {
        playerState = PlayerState.Attacking;

        playerAnimator.SetTrigger(AttackParameter);
    }

    public void OpenComboWindow() 
    {
        // 클릭 입력을 받을 수 있게  
        movement.SetAttackInput(true);
    }

    public void HitBoxOn() 
    {
        Debug.Log("히트박스 On");
    }

    public void HitBoxOff()
    {
        Debug.Log("히트박스 OFf");
    }

    public void OnAttackEnd() 
    {
        Debug.Log("공격이 끝");

        playerState = PlayerState.Locomotion;
        movement.SetAttackInput(true); 

    }
}
