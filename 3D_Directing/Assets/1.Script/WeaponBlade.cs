using UnityEngine;

public class WeaponBlade : MonoBehaviour
{
    private float length = 0.3f;
    private float radius = 0.02f;

    public float Radius => radius;

    // OverlapCapsule에 넘길 양 끝 구의 중심
    public void GetCapsule(out Vector3 p1, out Vector3 p2)
    {
        // 파는 로컬 Y축 방향으로 길다
        // 양 끝 구의 중심을 radius만큼 안쪽에 둬야 전체 길이가 length가 됨
        float halfSegment = Mathf.Max(0f, length * 0.5f - radius);
        Vector3 half = transform.up * halfSegment;

        p1 = transform.position - half;
        p2 = transform.position + half;
    }

    private void OnDrawGizmos()
    {
        // 판정과 같은 함수를 써서 기즈모 = 실제 판정
        GetCapsule(out Vector3 p1, out Vector3 p2);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(p1, radius);
        Gizmos.DrawWireSphere(p2, radius);
        Gizmos.DrawLine(p1, p2);
    }
}
