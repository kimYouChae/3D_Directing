using System;
using System.Collections;
using UnityEngine;

// 순서, 현재 Day, 어떤 컷신을 언제 틀지, 언제 조작을 풀지
public class FlowManager : MonoBehaviour
{
    [SerializeField] private MikuCharacter miku;
    [SerializeField] private TimelineManager timeline;
    [SerializeField] private Transform roomStart;
    [SerializeField] private Transform fieldStart;

    public Day CurrentDay { get; private set; }
    public event Action<Day> DayChanged;   // 아직 구독자 없음 (나중에 FieldState 밝기)

    private bool waitingDream;    // E 입력을 받는 구간인지
    private bool dreamRequested;

    private void Start() => StartCoroutine(Flow());

    private IEnumerator Flow()
    {
        yield return null;   // MikuCharacter.Start(Locomotion 초기화)보다 뒤에 시작

        // 게임 시작 시 1일차 이벤트 한 번
        CurrentDay = Day.First;
        DayChanged?.Invoke(CurrentDay);
        DashLog.Log($"DayChanged day={CurrentDay}");

        // 방
        miku.Teleport(roomStart);
        yield return timeline.Play(CutsceneType.Opening, CurrentDay);
        miku.ExitCutscene();

        // 방 탐색: 상호작용 구간에서 E
        dreamRequested = false;
        waitingDream = true;
        yield return new WaitUntil(() => dreamRequested);
        waitingDream = false;

        // 필드
        miku.EnterCutscene();
        miku.Teleport(fieldStart);
        yield return timeline.Play(CutsceneType.FieldArrive, CurrentDay);
        yield return timeline.Play(CutsceneType.Intro, CurrentDay);   // 사이에 해제 없음
        miku.ExitCutscene();

        DashLog.Log("FlowBattleReady");
    }

    // InteractZone이 부름 (구간 안 + Locomotion + E는 구간 쪽에서 확인)
    public void RequestDream()
    {
        if (!waitingDream) return;
        dreamRequested = true;
        DashLog.Log("Interact dream");
    }
}
