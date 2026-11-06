using UnityEngine;

// 컷신 카메라들의 부모 (보통 Director 오브젝트에 붙임). 재생 전에 미쿠 위치·방향으로 옮김 (TimelineManager가 호출)
// 자식 카메라는 미쿠가 원점에 서 있다고 보고 배치
public class CutsceneRig : MonoBehaviour
{
    public void Align(Transform target)
    {
        transform.SetPositionAndRotation(target.position, target.rotation);
        DashLog.Log($"RigAlign {name} pos={target.position}");
    }
}
