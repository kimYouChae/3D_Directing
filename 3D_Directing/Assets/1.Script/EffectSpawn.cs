using System;
using UnityEngine;

// 이펙트 하나를 어디에, 어떻게 만들지 (Inspector / SO에서 설정)
// Hovl 이펙트는 Loop라 스스로 안 사라짐 -> lifeTime 뒤 Destroy
[Serializable]
public struct EffectSpawn
{
    public GameObject prefab;
    public Vector3 offset;     // 기준 회전 기준 로컬 오프셋 (예: 몸 중간 = y)
    public Vector3 rotation;   // 추가 회전 (Euler, 예: X -90)
    public float scale;        // 1 = 원본 크기
    public float lifeTime;     // 이 시간 뒤 Destroy (0이면 안 지움 -> 부모와 같이 사라질 때)

    // pos, rot 기준으로 생성 (prefab이 비었으면 아무것도 안 하고 null)
    public GameObject Spawn(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (prefab == null) return null;

        Vector3 spawnPos = pos + rot * offset;
        Quaternion spawnRot = rot * Quaternion.Euler(rotation);
        GameObject go = UnityEngine.Object.Instantiate(prefab, spawnPos, spawnRot, parent);
        go.transform.localScale = Vector3.one * scale;

        if (lifeTime > 0f) UnityEngine.Object.Destroy(go, lifeTime);
        return go;
    }
}
