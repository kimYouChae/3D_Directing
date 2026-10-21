using System.Collections.Generic;
using UnityEngine;

// 스킬1 마법의 원: 생성되고 delay 뒤 반경 안의 적에게 데미지 -> 스스로 Destroy
public class MagicCircle : MonoBehaviour
{
    private float radius;
    private float delay;
    private float damage;
    private LayerMask enemyLayer;

    private float elapsed;
    private Material material;  // 코드로 만든 머티리얼 인스턴스 (직접 Destroy)

    public static MagicCircle Spawn(Vector3 position, float radius, float delay, float damage, LayerMask enemyLayer)
    {
        // 납작한 원기둥으로 바닥 원 표시
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "MagicCircle";
        Destroy(go.GetComponent<Collider>());

        // 바닥에 묻히지 않게 살짝 위로 / Cylinder 기본 지름 1, 높이 2
        go.transform.position = position + Vector3.up * 0.02f;
        go.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);

        MagicCircle circle = go.AddComponent<MagicCircle>();
        circle.radius = radius;
        circle.delay = delay;
        circle.damage = damage;
        circle.enemyLayer = enemyLayer;

        // .material 접근 시 인스턴스가 만들어짐
        circle.material = go.GetComponent<Renderer>().material;
        circle.material.color = new Color(0.6f, 0.2f, 1f);

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

            enemy.TakeDamage(damage);
        }

        DashLog.Log($"MagicCircleExplode pos={transform.position} hit={hitEnemies.Count}");
    }

    // 코드로 만든 Material은 GameObject와 같이 사라지지 않음 -> 누수 방지
    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }
}
