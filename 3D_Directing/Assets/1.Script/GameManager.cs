using UnityEngine;

// 게임 전역 참조 창구: 바깥(Flow·Timeline·InteractZone 등)에서 미쿠를 찾을 때 여기를 거침
// 미쿠 자신의 부품(StateMachineBehaviour, HitFeedback, 스킬 owner)은 GetComponent/생성자로 그대로 받음
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private MikuCharacter miku;

    public MikuCharacter Miku => miku;
}
