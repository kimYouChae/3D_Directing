using System;
using UnityEngine;

// 하루 데이터: 그날 필드에 나올 적 종류 + 개수 (밝기는 FieldState 만들 때 추가)
[CreateAssetMenu(fileName = "DayData", menuName = "Flow/DayData")]
public class DayData : ScriptableObject
{
    [Serializable]
    public struct EnemySpawn
    {
        public Enemy prefab;
        public int count;
    }

    [SerializeField] private EnemySpawn[] enemies;

    public EnemySpawn[] Enemies => enemies;
}
