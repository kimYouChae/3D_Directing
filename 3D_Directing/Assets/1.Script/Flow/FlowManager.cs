using System;
using System.Collections;
using UnityEngine;

// 순서, 현재 Day, 어떤 컷신을 언제 틀지, 언제 조작을 풀지
// 그날 틀 타임라인은 TimelineByDay(SO)에서 꺼냄
public class FlowManager : Singleton<FlowManager>
{
    [SerializeField] private FieldManager field;
    [SerializeField] private Transform roomStart;
    [SerializeField] private Transform fieldStart;

    [Header("===Day===")]
    [SerializeField] private DayData[] days;   // 순서 = Day enum (First, Second, Third), 개수 = 마지막 날

    private static MikuCharacter Miku => GameManager.Instance.Miku;
    private static TimelineManager Timeline => TimelineManager.Instance;

    public Day CurrentDay { get; private set; }
    public event Action<Day> DayChanged;   // 아직 구독자 없음 (나중에 FieldState 밝기)

    private bool waitingDream;     // 컴퓨터 E를 받는 구간인지
    private bool dreamRequested;
    private bool waitingDayEnd;    // 결과 UI 버튼을 받는 구간인지
    private bool dayEndRequested;

    private void Start() => StartCoroutine(Flow());

    private IEnumerator Flow()
    {
        yield return null;   // MikuCharacter.Start(Locomotion 초기화)보다 뒤에 시작

        Miku.Teleport(roomStart);
        yield return Timeline.Play(Timeline.Opening);

        for (int i = 0; i < days.Length; i++)
        {
            // 1일차는 시작 시 1회, 이후엔 전날 "앤드"(아침→밤)가 끝난 직후
            CurrentDay = (Day)i;
            DayChanged?.Invoke(CurrentDay);
            DashLog.Log($"DayChanged day={CurrentDay}");

            yield return RunDay(Timeline.GetDay(CurrentDay), days[i]);
        }

        yield return Timeline.Play(Timeline.Ending);
        DashLog.Log("FlowEnd");
    }

    private IEnumerator RunDay(TimelineByDay timelines, DayData data)
    {
        if (timelines == null) yield break;

        // 방: 시작 -> 해제 -> 컴퓨터 앞에서 E
        yield return Timeline.PlayAll(timelines.Start);
        Miku.ExitCutscene();

        dreamRequested = false;
        waitingDream = true;
        yield return new WaitUntil(() => dreamRequested);
        waitingDream = false;

        // 마스터에게 상호작용 -> 필드 도착 (사이에 해제 없음)
        Miku.EnterCutscene();
        yield return Timeline.PlayAll(timelines.Master);
        Miku.Teleport(fieldStart);
        yield return Timeline.PlayAll(timelines.Arrive);

        // 전투: 도착 컷신 끝 -> 적 생성 -> 조작 해제
        field.Spawn(data, Miku.transform);
        Miku.ExitCutscene();
        DashLog.Log($"BattleStart day={CurrentDay}");

        // 전멸 + 공격·회피·궁극기가 끝나 Locomotion으로 돌아올 때까지
        yield return new WaitUntil(() => field.AliveCount == 0);
        yield return new WaitUntil(() => Miku.IsLocomotion);
        DashLog.Log($"DayClear day={CurrentDay}");

        // 결과 UI: 잠그고(클릭이 공격으로 안 들어가게) 커서 풀기 -> 버튼 대기
        Miku.EnterCutscene();
        yield return WaitResult();

        // 방 복귀 -> 앤드
        field.Cleanup();
        Miku.Teleport(roomStart);
        yield return Timeline.PlayAll(timelines.End);
    }

    private IEnumerator WaitResult()
    {
        LobbyUIManager.Instance.ShowResult();

        dayEndRequested = false;
        waitingDayEnd = true;
        yield return new WaitUntil(() => dayEndRequested);
        waitingDayEnd = false;

        LobbyUIManager.Instance.HideResult();
    }

    // LobbyUIManager의 결과 버튼이 부름
    public void RequestDayEnd()
    {
        if (!waitingDayEnd)
        {
            DashLog.Log("DayEndIgnored");
            return;
        }
        dayEndRequested = true;
        DashLog.Log("DayEndRequest");
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
            default:
                DashLog.Log($"InteractIgnored type={type}");
                break;
        }
    }
}
