using System;
using UnityEngine;
using UnityEngine.Serialization;

// 하루 데이터: 웨이브별 적 종류 + 개수 (밝기는 FieldState 만들 때 추가)
// 1웨이브 전멸 -> 2웨이브 생성 -> 2웨이브 전멸 -> 포탈
[CreateAssetMenu(fileName = "DayData", menuName = "Flow/DayData")]
public class DayData : ScriptableObject
{
    [Serializable]
    public struct EnemySpawn
    {
        public Enemy prefab;
        public int count;
    }

    [FormerlySerializedAs("enemies")]   // 기존 Inspector 값 유지
    [SerializeField] private EnemySpawn[] wave1;
    [SerializeField] private EnemySpawn[] wave2;

    public EnemySpawn[] Wave1 => wave1;
    public EnemySpawn[] Wave2 => wave2;
}
