using System;
using System.Collections.Generic;
using UnityEngine;

// 스킬 공통 (쿨타임, 범위 안 적 찾기, 중복 타격 방지)
// 수치는 SkillData(SO)에서 읽고, 실행 중 상태(쿨타임 시각, 맞은 적)는 여기서 가진다
public abstract class SkillBase : ISkill
{
    protected readonly MikuCharacter owner;
    private readonly SkillData data;

    // 이번 사용에서 이미 맞은 적 (중복 타격 방지)
    protected readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    // 마지막 사용 시간 (NegativeInfinity -> 처음엔 바로 사용 가능)
    private float lastUseTime = float.NegativeInfinity;

    // 이번 사용에서 첫 타격 연출(히트스톱·셰이크)을 했는지 (한 번 사용에 한 번만)
    private bool hasHitFeedback;

    // 이번 사용에서 시작 이펙트를 이미 만들었는지
    private bool castSpawned;

    public string Name => data.SkillName;
    public string StateName => data.StateName;
    public bool UseDash => data.UseDash;
    public bool IsReady => Time.time >= lastUseTime + data.Cooldown;
    public float CooldownRemain => Mathf.Max(0f, lastUseTime + data.Cooldown - Time.time);

    protected float Range => data.Range;     // 판정 범위
    protected float Damage => data.Damage;   // 데미지

    protected SkillBase(MikuCharacter owner, SkillData data)
    {
        this.owner = owner;
        this.data = data;
    }

    public void Begin()
    {
        lastUseTime = Time.time;
        hitEnemies.Clear();
        hasHitFeedback = false;
        castSpawned = false;
        OnBegin();
    }

    // 스킬별 초기화가 필요하면 재정의
    protected virtual void OnBegin() { }

    // 매 프레임 (SkillState -> MikuCharacter.TickSkill)
    // 진행도가 castTiming에 처음 닿은 프레임 -> 시작 이펙트 (0이면 애니 시작 즉시)
    // (Tick은 스킬 애니에서만 불림 -> 흡착 대시가 끝난 자리에서 생성됨)
    public void Tick(float t)
    {
        if (!castSpawned && t >= data.CastTiming)
        {
            castSpawned = true;
            if (data.CastEffect.Spawn(owner.transform.position, owner.transform.rotation) != null)
                DashLog.Log($"CastEffect skill={Name} t={t:F3}");
        }

        OnTick(t);
    }

    // 스킬별 동작 (진행도 t)
    protected abstract void OnTick(float t);

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

        bool anyHit = false;   // 이번 프레임에 새로 맞은 적이 있는지 (히트스톱·셰이크용)

        foreach (Enemy enemy in FindEnemies(owner.transform.position, radius))
        {
            // 이미 맞은 적이면 pass
            if (!hitEnemies.Add(enemy)) continue;

            DashLog.Log($"SkillHit skill={Name} enemy={enemy.name} t={t:F3}");
            onHit(enemy);
            anyHit = true;
        }

        // 처음 맞힌 순간 한 번만 (값이 0이면 그 연출은 없는 스킬)
        if (anyHit && !hasHitFeedback)
        {
            if (data.HitStopDuration > 0f) owner.HitFeedback.StartHitStop(data.HitStopDuration, data.HitStopTimeScale);
            if (data.CameraShakeForce > 0f) owner.HitFeedback.ShakeCamera(data.CameraShakeForce);
            hasHitFeedback = true;
        }
    }
}
