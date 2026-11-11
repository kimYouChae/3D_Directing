using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 필드 전투: 적 생성(alive에 Add) -> 죽으면 Remove -> 전멸 판정, 포탈 열기/닫기, 남은 장판 정리
public class FieldManager : MonoBehaviour
{
    [Header("===Spawn===")]
    // 웨이브별 고정 생성 위치 (회전 = 적이 바라볼 방향). 0번부터, 적이 더 많으면 순환
    [FormerlySerializedAs("spawnPoints")]   // 기존 연결 유지
    [SerializeField] private Transform[] wave1Points;
    [SerializeField] private Transform[] wave2Points;
    [Header("===Portal===")]
    [SerializeField] private GameObject portalPrefab;   // 전멸 후 생성 (InteractZone Type = Portal)
    [SerializeField] private Transform portalPoint;     // 생성 위치

    private GameObject portal;   // 생성된 포탈 (Cleanup에서 삭제)

    private readonly List<Enemy> alive = new List<Enemy>();   // 살아 있는 적

    public int AliveCount => alive.Count;

    public void SpawnWave1(DayData data) => Spawn(data.Wave1, wave1Points, "Wave1");
    public void SpawnWave2(DayData data) => Spawn(data.Wave2, wave2Points, "Wave2");

    private void Spawn(DayData.EnemySpawn[] wave, Transform[] spawnPoints, string label)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            DashLog.Log($"SpawnNoPoint label={label}");
            return;
        }

        int index = 0;   // 웨이브마다 0번 위치부터
        if (wave != null)
        {
            foreach (DayData.EnemySpawn spawn in wave)
            {
                for (int i = 0; i < spawn.count; i++)
                {
                    Transform point = spawnPoints[index % spawnPoints.Length];   // 적이 위치보다 많으면 처음으로
                    index++;

                    Enemy enemy = Instantiate(spawn.prefab, point.position, point.rotation);
                    enemy.Died += OnEnemyDied;
                    alive.Add(enemy);
                }
            }
        }

        DashLog.Log($"Spawn label={label} count={index} points={spawnPoints.Length}");
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
