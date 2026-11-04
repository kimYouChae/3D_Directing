using System.Collections.Generic;
using UnityEngine;

// 궁극기 장판: 생성 후 duration 동안 interval마다 반경 안의 적을 얼리고 데미지 -> 끝나면 Destroy
// 눈 이펙트는 자식이라 장판과 같이 사라짐. 컷씬(스킬)이 끝나도 독립적으로 동작 (MagicCircle과 같은 구조)
public class UltField : MonoBehaviour
{
    private float radius;
    private float duration;
    private float interval;
    private float damage;
    private float freezeDuration;
    private LayerMask enemyLayer;
    private EffectSpawn freezeEffect;

    private float elapsed;
    private float nextTickTime;   // 0 -> 생성 프레임에 첫 틱 (맞는 순간 얼림)

    public static UltField Spawn(Vector3 position, float radius, float duration, float interval, float damage,
                                 float freezeDuration, LayerMask enemyLayer, EffectSpawn snowEffect, EffectSpawn freezeEffect)
    {
        GameObject go = new GameObject("UltField");
        go.transform.position = position;

        // 눈 이펙트를 자식으로 생성 -> 장판 Destroy될 때 같이 사라짐
        snowEffect.Spawn(position, Quaternion.identity, go.transform);

        UltField field = go.AddComponent<UltField>();
        field.radius = radius;
        field.duration = duration;
        field.interval = interval;
        field.damage = damage;
        field.freezeDuration = freezeDuration;
        field.enemyLayer = enemyLayer;
        field.freezeEffect = freezeEffect;

        DashLog.Log($"UltFieldSpawn pos={position} radius={radius} duration={duration}");
        return field;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= nextTickTime)
        {
            nextTickTime += interval;
            TickField();
        }

        if (elapsed >= duration)
        {
            DashLog.Log("UltFieldEnd");
            Destroy(gameObject);
        }
    }

    private void TickField()
    {
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null || !hitEnemies.Add(enemy)) continue;

            enemy.Freeze(freezeDuration, freezeEffect);   // 장판 안에 있는 동안 계속 갱신
            enemy.TakeDamage(damage);                     // Destroy될 수 있어 마지막에
        }

        DashLog.Log($"UltFieldTick t={elapsed:F2} hit={hitEnemies.Count}");
    }
}
