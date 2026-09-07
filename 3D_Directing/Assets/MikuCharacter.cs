using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class MikuCharacter : MonoBehaviour
{
    [SerializeField]
    private Animator playerAnimator;
    [SerializeField]
    private Animator enemyAnimator;

    [SerializeField]
    private bool isPlaying;

    [SerializeField] CinemachineCamera defaultCam;
    [SerializeField] CinemachineCamera ultimateCam;

    const string AttackParameter = "Attack";
    const string HitParameter = "Hit";

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        isPlaying = true;

        defaultCam.Priority = 10;
    }

    void Update()
    {
        if (!isPlaying)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            StartCoroutine(Attack());

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
