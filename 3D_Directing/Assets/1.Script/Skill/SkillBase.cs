using System;
using System.Collections.Generic;
using UnityEngine;

// 스킬 공통 (쿨타임, 범위 안 적 찾기, 중복 타격 방지)
// 데이터는 지금은 생성자로 하드코딩 -> 나중에 ScriptableObject로 교체
public abstract class SkillBase : ISkill
{
    protected readonly MikuCharacter owner;
    protected readonly float cooldown;  // 쿨타임
    protected readonly float range;     // 판정 범위
    protected readonly float damage;    // 데미지

    // 이번 사용에서 이미 맞은 적 (중복 타격 방지)
    protected readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    // 마지막 사용 시간 (NegativeInfinity -> 처음엔 바로 사용 가능)
    private float lastUseTime = float.NegativeInfinity;

    public string Name { get; }
    public string StateName { get; }
    public bool UseDash { get; }
    public bool IsReady => Time.time >= lastUseTime + cooldown;
    public float CooldownRemain => Mathf.Max(0f, lastUseTime + cooldown - Time.time);

    protected SkillBase(MikuCharacter owner, string name, string stateName,
                        float cooldown, float range, float damage, bool useDash)
    {
        this.owner = owner;
        Name = name;
        StateName = stateName;
        this.cooldown = cooldown;
        this.range = range;
        this.damage = damage;
        UseDash = useDash;
    }

    public void Begin()
    {
        lastUseTime = Time.time;
        hitEnemies.Clear();
        OnBegin();
    }

    // 스킬별 초기화가 필요하면 재정의
    protected virtual void OnBegin() { }

    public abstract void Tick(float t);

    // center 반경 안의 적 (콜라이더가 자식이어도 찾도록 GetComponentInParent)
    protected List<Enemy> FindEnemies(Vector3 center, float radius)
    {
        List<Enemy> result = new List<Enemy>();
        Collider[] hits = Physics.OverlapSphere(center, radius, owner.EnemyLayer);

        foreach (Collider hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null && !result.Contains(enemy))
                result.Add(enemy);
        }
        return result;
    }

    // 판정 구간(start~end) 안이면 범위 안의 '새로 맞은' 적마다 onHit 호출
    protected void HitWindow(float t, float start, float end, float radius, Action<Enemy> onHit)
    {
        if (t < start || t > end) return;

        foreach (Enemy enemy in FindEnemies(owner.transform.position, radius))
        {
            // 이미 맞은 적이면 pass
            if (!hitEnemies.Add(enemy)) continue;

            DashLog.Log($"SkillHit skill={Name} enemy={enemy.name} t={t:F3}");
            onHit(enemy);
        }
    }
}
