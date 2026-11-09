using System.Collections.Generic;
using UnityEngine;

// 필드 전투: 적 생성(alive에 Add) -> 죽으면 Remove -> 전멸 판정, 포탈 열기/닫기, 남은 장판 정리
public class FieldManager : MonoBehaviour
{
    [Header("===Spawn===")]
    [SerializeField] private float minRadius = 3f;      // 기준(미쿠)과 너무 붙지 않게
    [SerializeField] private float maxRadius = 8f;      // 기준 주변 이 반경 안 랜덤 (나중에 디테일: 필드 밖·벽 속 방지)
    [Header("===Portal===")]
    [SerializeField] private GameObject portalPrefab;   // 전멸 후 생성 (InteractZone Type = Portal)
    [SerializeField] private Transform portalPoint;     // 생성 위치

    private GameObject portal;   // 생성된 포탈 (Cleanup에서 삭제)

    private readonly List<Enemy> alive = new List<Enemy>();   // 살아 있는 적

    public int AliveCount => alive.Count;

    public void Spawn(DayData data, Transform center)
    {
        foreach (DayData.EnemySpawn spawn in data.Enemies)
        {
            for (int i = 0; i < spawn.count; i++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(minRadius, maxRadius);
                Vector3 pos = center.position + new Vector3(offset.x, 0f, offset.y);
                Quaternion rot = Quaternion.LookRotation(new Vector3(-offset.x, 0f, -offset.y));   // 기준을 보게

                Enemy enemy = Instantiate(spawn.prefab, pos, rot);
                enemy.Died += OnEnemyDied;
                alive.Add(enemy);
            }
        }

        DashLog.Log($"Spawn day={data.name} count={alive.Count} center={center.position}");
    }

    private void OnEnemyDied(Enemy enemy)
    {
        enemy.Died -= OnEnemyDied;
        alive.Remove(enemy);

        DashLog.Log($"EnemyDied {enemy.name} remain={alive.Count}");
    }

    public void OpenPortal()
    {
        portal = Instantiate(portalPrefab, portalPoint.position, portalPoint.rotation);
        DashLog.Log($"PortalOpen pos={portalPoint.position}");
    }

    // 방으로 돌아갈 때: 포탈 닫기 + 남아 있는 궁극기 장판·마법진 삭제
    public void Cleanup()
    {
        bool hadPortal = portal != null;
        if (hadPortal) Destroy(portal);
        portal = null;

        UltField[] fields = FindObjectsByType<UltField>(FindObjectsSortMode.None);
        MagicCircle[] circles = FindObjectsByType<MagicCircle>(FindObjectsSortMode.None);
        foreach (UltField field in fields) Destroy(field.gameObject);
        foreach (MagicCircle circle in circles) Destroy(circle.gameObject);

        DashLog.Log($"Cleanup portal={hadPortal} field={fields.Length} circle={circles.Length}");
    }
}
