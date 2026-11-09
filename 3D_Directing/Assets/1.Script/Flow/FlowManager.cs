using System;
using System.Collections;
using UnityEngine;

// 순서, 현재 Day, 어떤 컷신을 언제 틀지, 언제 조작을 풀지
public class FlowManager : Singleton<FlowManager>
{
    [SerializeField] private MikuCharacter miku;
    [SerializeField] private TimelineManager timeline;
    [SerializeField] private FieldManager field;
    [SerializeField] private Transform roomStart;
    [SerializeField] private Transform fieldStart;

    [Header("===Day===")]
    [SerializeField] private DayData[] days;   // 순서 = Day enum (First, Second, Third)

    public MikuCharacter Miku => miku;   // InteractZone(프리팹 포함)이 Instance로 꺼내 씀

    public Day CurrentDay { get; private set; }
    public event Action<Day> DayChanged;   // 아직 구독자 없음 (나중에 FieldState 밝기)

    private bool waitingDream;     // 컴퓨터 E를 받는 구간인지
    private bool dreamRequested;
    private bool waitingReturn;    // 포탈 E를 받는 구간인지
    private bool returnRequested;

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

        // 방 탐색: 컴퓨터 앞에서 E
        dreamRequested = false;
        waitingDream = true;
        yield return new WaitUntil(() => dreamRequested);
        waitingDream = false;

        // 필드
        miku.EnterCutscene();
        miku.Teleport(fieldStart);
        yield return timeline.Play(CutsceneType.FieldArrive, CurrentDay);
        yield return timeline.Play(CutsceneType.Intro, CurrentDay);   // 사이에 해제 없음

        // 전투: Intro 끝 -> 적 생성 -> 조작 해제
        field.Spawn(days[(int)CurrentDay], miku.transform);
        miku.ExitCutscene();
        DashLog.Log($"BattleStart day={CurrentDay}");

        // 전멸 + 공격·회피·궁극기가 끝나 Locomotion으로 돌아올 때까지
        yield return new WaitUntil(() => field.AliveCount == 0);
        yield return new WaitUntil(() => miku.IsLocomotion);
        DashLog.Log($"DayClear day={CurrentDay}");

        // 포탈에서 E -> 방으로
        field.OpenPortal();
        returnRequested = false;
        waitingReturn = true;
        yield return new WaitUntil(() => returnRequested);
        waitingReturn = false;

        miku.EnterCutscene();
        field.Cleanup();
        miku.Teleport(roomStart);
        miku.ExitCutscene();

        DashLog.Log("FlowRoomReturn");
    }

    // InteractZone이 부름: 타입에 따라 기다리던 구간만 진행 (아니면 무시)
    public void RequestInteract(InteractType type)
    {
        switch (type)
        {
            case InteractType.Computer when waitingDream:
                dreamRequested = true;
                DashLog.Log("Interact dream");
                break;
            case InteractType.Portal when waitingReturn:
                returnRequested = true;
                DashLog.Log("Interact return");
                break;
            default:
                DashLog.Log($"InteractIgnored type={type}");
                break;
        }
    }
}