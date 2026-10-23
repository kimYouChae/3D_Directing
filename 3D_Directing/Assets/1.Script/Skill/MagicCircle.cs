using System.Collections.Generic;
using UnityEngine;

// 스킬1 마법의 원: 생성되고 delay 뒤 반경 안의 적에게 데미지 -> 스스로 Destroy
// 원 모양은 circleEffect (자식이라 원과 같이 사라짐), 폭발 때 맞은 적의 몸에 hitEffect
public class MagicCircle : MonoBehaviour
{
    private float radius;
    private float delay;
    private float damage;
    private LayerMask enemyLayer;
    private EffectSpawn hitEffect;

    private float elapsed;

    public static MagicCircle Spawn(Vector3 position, float radius, float delay, float damage, LayerMask enemyLayer,
                                    EffectSpawn circleEffect, EffectSpawn hitEffect)
    {
        GameObject go = new GameObject("MagicCircle");
        go.transform.position = position;

        // 원 이펙트를 자식으로 생성 -> 원이 Destroy될 때 같이 사라짐
        circleEffect.Spawn(position, Quaternion.identity, go.transform);

        MagicCircle circle = go.AddComponent<MagicCircle>();
        circle.radius = radius;
        circle.delay = delay;
        circle.damage = damage;
        circle.enemyLayer = enemyLayer;
        circle.hitEffect = hitEffect;

        DashLog.Log($"MagicCircleSpawn pos={position} radius={radius}");
        return circle;
    }

    private void Update()
    {
        // 게임 시간 기준 (슬로우 중이면 같이 느려짐)
        elapsed += Time.deltaTime;
        if (elapsed < delay) return;

        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy == null || !hitEnemies.Add(enemy)) continue;

            // 맞은 적의 몸에 이펙트 (데미지로 적이 Destroy돼도 이펙트는 따로 남음)
            hitEffect.Spawn(enemy.BodyPosition, Quaternion.identity);
            enemy.TakeDamage(damage);
        }

        DashLog.Log($"MagicCircleExplode pos={transform.position} hit={hitEnemies.Count}");
    }
}
