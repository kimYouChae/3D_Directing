using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public enum CutsceneType { Opening, FieldArrive, Intro }   // 나머지 타입은 쓸 때 추가
public enum Day { First, Second, Third }

// 컷신 찾기 -> 잠그기 -> (rig가 있으면) 미쿠 기준으로 정렬 -> 재생 -> 끝까지 대기
// 해제는 하지 않음 (FlowManager 담당: 연속 컷신 사이에 조작이 풀리지 않게)
public class TimelineManager : MonoBehaviour
{
    [Serializable]
    private struct CutsceneEntry
    {
        public CutsceneType type;
        public Day day;                    // FieldArrive만 사용
        public PlayableDirector director;
        public CutsceneRig rig;            // 비우면 고정 배치, 넣으면 재생 전에 미쿠 위치·방향으로 정렬
    }

    [SerializeField] private MikuCharacter miku;
    [SerializeField] private CutsceneEntry[] cutscenes;

    public IEnumerator Play(CutsceneType type, Day day)
    {
        miku.EnterCutscene();

        if (!TryFind(type, day, out CutsceneEntry entry) || entry.director == null)
        {
            DashLog.Log($"CutsceneMissing type={type} day={day}");
            yield break;
        }

        if (entry.rig != null) entry.rig.Align(miku.transform);

        PlayableDirector director = entry.director;
        director.time = 0;
        director.Play();
        DashLog.Log($"CutsceneStart type={type} day={day} rig={entry.rig != null} miku={miku.transform.position}");

        yield return null;   // Play 직후 프레임은 건너뛰고 재생 상태를 확인
        DashLog.Log($"CutsceneFrame1 type={type} miku={miku.transform.position}");
        yield return new WaitUntil(() => director.state != PlayState.Playing);   // Wrap Mode가 Hold면 안 끝남 -> None

        DashLog.Log($"CutsceneEnd type={type} day={day} miku={miku.transform.position}");
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
