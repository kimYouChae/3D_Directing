using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

// 컷신 타입 (Inspector에 숫자로 저장되므로 새 타입은 맨 뒤에 추가)
public enum CutsceneType
{
    Opening,                 // 오프닝
    FieldArrive,             // 필드 도착 (Day별)
    Intro,                   // 전투 시작 전 Intro
    ExitComputer,            // Exit_Computer
    EnterHead,               // Enter_Head
    ExitDreamIntoComputer,   // Exit_Dream_Into_Computer
    DayFlow,                 // DayFlow
    Ending,                  // Ending
    Ult                      // 궁극기 (Prepare만 사용: 재생·잠금·해제는 MikuCharacter)
}
// None = -1: 기존 Inspector 값(First=0, Second=1, Third=2)과 days[(int)Day] 인덱스를 그대로 유지
public enum Day { None = -1, First, Second, Third }

// 컷신 찾기 -> 잠그기 -> (rig가 있으면) 미쿠 기준으로 정렬 -> 재생 -> 끝까지 대기
// 해제는 하지 않음 (FlowManager 담당: 연속 컷신 사이에 조작이 풀리지 않게)
public class TimelineManager : Singleton<TimelineManager>
{
    [Serializable]
    private struct CutsceneEntry
    {
        public CutsceneType type;
        public Day day;                    // FieldArrive만 사용
        public PlayableDirector director;
        public CutsceneRig rig;            // 비우면 고정 배치, 넣으면 재생 전에 미쿠 위치·방향으로 정렬
    }

    private static MikuCharacter Miku => GameManager.Instance.Miku;
    [SerializeField] private CutsceneEntry[] cutscenes;

    public IEnumerator Play(CutsceneType type, Day day)
    {
        Miku.EnterCutscene();

        PlayableDirector director = Prepare(type, day);
        if (director == null) yield break;

        director.time = 0;
        director.Play();
        DashLog.Log($"CutsceneStart type={type} day={day} miku={Miku.transform.position}");

        yield return null;   // Play 직후 프레임은 건너뛰고 재생 상태를 확인
        DashLog.Log($"CutsceneFrame1 type={type} miku={Miku.transform.position}");
        yield return new WaitUntil(() => director.state != PlayState.Playing);   // Wrap Mode가 Hold면 안 끝남 -> None

        DashLog.Log($"CutsceneEnd type={type} day={day} miku={Miku.transform.position}");
    }

    // 찾기 + (rig가 있으면) 미쿠 기준으로 정렬만. 잠그기·재생은 안 함
    // 궁극기처럼 재생·해제를 직접 관리하는 곳에서 사용 (day는 FieldArrive만 봄)
    public PlayableDirector Prepare(CutsceneType type, Day day = Day.None)
    {
        if (!TryFind(type, day, out CutsceneEntry entry) || entry.director == null)
        {
            DashLog.Log($"CutsceneMissing type={type} day={day}");
            return null;
        }

        if (entry.rig != null) entry.rig.Align(Miku.transform);
        return entry.director;
    }

    private bool TryFind(CutsceneType type, Day day, out CutsceneEntry found)
    {
        foreach (CutsceneEntry entry in cutscenes)
        {
            if (entry.type != type) continue;
            if (type == CutsceneType.FieldArrive && entry.day != day) continue;
            found = entry;
            return true;
        }
        found = default;
        return false;
    }
}
