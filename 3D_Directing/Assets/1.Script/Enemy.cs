using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float hp = 30f;

    // 데미지 받기
    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log($"{name} 피격 / 데미지 {damage} / 남은 HP {hp}");

        // hp가 0 이하가 되면 파괴
        if (hp <= 0)
            Destroy(gameObject);
    }
}
