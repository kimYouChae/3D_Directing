using System.Collections;
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
    [SerializeField] CinemachineCamera ultimateCam;

    const string AttackParameter = "Attack";
    const string HitParameter = "Hit";

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        isPlaying = true;

        defaultCam.Priority = 10;

        playerState = PlayerState.Locomotion;
        PlayOpening();
    }

    private void PlayOpening() 
    {
        playerState = PlayerState.Cutscene;

        introDirector.stopped += IntroStopped;
        introDirector.Play();
    }

    private void IntroStopped(PlayableDirector pd) 
    {
        playerState = PlayerState.Locomotion;
    }

    private IEnumerator Attack() 
    {
        isPlaying = false;

        // 연출 캠 우선순위 올리기 
        ultimateCam.Priority = 20;
        yield return new WaitForSeconds(0.2f);

        playerAnimator.SetTrigger(AttackParameter);     
        enemyAnimator.SetTrigger(HitParameter);
        yield return new WaitForSeconds(1f);

        // 연출 캠 우선순위 원래대로 
        ultimateCam.Priority = 0;

        isPlaying = true;
    }
}
