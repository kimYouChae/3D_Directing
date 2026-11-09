using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// None = -1: 기존 Inspector 값(First=0, Second=1, Third=2)과 days[(int)Day] 인덱스를 그대로 유지
public enum Day { None = -1, First, Second, Third }

// 타임라인 파일로 Director 찾기 -> 잠그기 -> (rig가 있으면) 미쿠 기준으로 정렬 -> 재생 -> 끝까지 대기
// 해제는 하지 않음 (FlowManager 담당: 연속 컷신 사이에 조작이 풀리지 않게)
public class TimelineManager : Singleton<TimelineManager>
{
    [Serializable]
    private struct CutsceneEntry
    {
        public PlayableDirector director;   // Playable 칸의 타임라인 파일이 찾는 키
        public CutsceneRig rig;             // 비우면 고정 배치, 넣으면 재생 전에 미쿠 위치·방향으로 정렬
    }

    private static MikuCharacter Miku => GameManager.Instance.Miku;

    [Header("===공통===")]
    [SerializeField] private PlayableAsset opening;
    [SerializeField] private PlayableAsset ending;
    [SerializeField] private PlayableAsset ult;   // Prepare만 사용: 재생·잠금·해제는 MikuCharacter

    [Header("===Day===")]
    [SerializeField] private TimelineByDay[] days;   // 순서 = Day enum (First, Second, Third)

    [Header("===씬 Director (타임라인마다 1개)===")]
    [SerializeField] private CutsceneEntry[] cutscenes;

    private readonly Dictionary<PlayableAsset, CutsceneEntry> table = new();

    public PlayableAsset Opening => opening;
    public PlayableAsset Ending => ending;

    protected override void OnSingletonAwake()
    {
        foreach (CutsceneEntry entry in cutscenes)
        {
            PlayableAsset asset = entry.director != null ? entry.director.playableAsset : null;
            if (asset == null)
            {
                DashLog.Log($"CutsceneNoAsset director={(entry.director != null ? entry.director.name : "null")}");
                continue;
            }
            if (!table.TryAdd(asset, entry))
                DashLog.Log($"CutsceneDuplicate asset={asset.name} director={entry.director.name}");
        }
    }

    public TimelineByDay GetDay(Day day)
    {
        int index = (int)day;
        if (index < 0 || index >= days.Length || days[index] == null)
        {
            DashLog.Log($"TimelineDayMissing day={day}");
            return null;
        }
        return days[index];
    }

    // 리스트를 순서대로 재생 (사이에 해제 없음). 비어 있으면 아무것도 안 함
    public IEnumerator PlayAll(IReadOnlyList<PlayableAsset> list)
    {
        if (list == null) yield break;
        foreach (PlayableAsset asset in list)
            yield return Play(asset);
    }

    public IEnumerator Play(PlayableAsset asset)
    {
        Miku.EnterCutscene();

        PlayableDirector director = Prepare(asset);
        if (director == null) yield break;

        director.time = 0;
        director.Play();
        DashLog.Log($"CutsceneStart asset={asset.name} miku={Miku.transform.position}");

        yield return null;   // Play 직후 프레임은 건너뛰고 재생 상태를 확인
        DashLog.Log($"CutsceneFrame1 asset={asset.name} miku={Miku.transform.position}");
        yield return new WaitUntil(() => director.state != PlayState.Playing);   // Wrap Mode가 Hold면 안 끝남 -> None

        DashLog.Log($"CutsceneEnd asset={asset.name} miku={Miku.transform.position}");
    }

    public PlayableDirector PrepareUlt() => Prepare(ult);

    // 찾기 + (rig가 있으면) 미쿠 기준으로 정렬만. 잠그기·재생은 안 함
    public PlayableDirector Prepare(PlayableAsset asset)
    {
        if (asset == null || !table.TryGetValue(asset, out CutsceneEntry entry))
        {
            DashLog.Log($"CutsceneMissing asset={(asset != null ? asset.name : "null")}");
            return null;
        }

        if (entry.rig != null) entry.rig.Align(Miku.transform);
        return entry.director;
    }
}
