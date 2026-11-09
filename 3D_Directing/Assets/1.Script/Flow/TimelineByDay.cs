using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// 그날 틀 타임라인 목록 (무엇을 틀지). 어떻게 틀지(Director·Rig·바인딩)는 씬의 TimelineManager가 가짐
// 각 리스트는 순서대로 재생, 비워 두면 건너뜀
[CreateAssetMenu(menuName = "Flow/TimelineByDay")]
public class TimelineByDay : ScriptableObject
{
    [SerializeField] private PlayableAsset[] start;    // 하루 시작 (방)
    [SerializeField] private PlayableAsset[] master;   // 마스터(컴퓨터)에게 상호작용
    [SerializeField] private PlayableAsset[] arrive;   // 필드 도착 (예: FieldArrive_Day1, Intro)
    [SerializeField] private PlayableAsset[] end;      // 하루 끝 (방 복귀 후)

    public IReadOnlyList<PlayableAsset> Start => start;
    public IReadOnlyList<PlayableAsset> Master => master;
    public IReadOnlyList<PlayableAsset> Arrive => arrive;
    public IReadOnlyList<PlayableAsset> End => end;
}
