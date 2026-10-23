using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

// 타격 연출 (히트스톱, 카메라 셰이크)
// 기본 공격(CheckHit)과 스킬(SkillBase.HitWindow)이 여기만 부른다
public class HitFeedback : MonoBehaviour
{
    [Header("===HitStop===")]
    // attackHitStopDuration / attackHitStopTimeScale: 기본 공격(Attack_1~3)만 사용 -> PlayAttackHit에서 StartHitStop에 넘김
    // 스킬은 이 값을 안 쓰고 자기 SO의 hitStopDuration / hitStopTimeScale을 넘김
    // (얼마나 오래·얼마나 느리게 모두 호출한 쪽이 정함)
    [SerializeField] private float attackHitStopDuration;   // 기본 공격 멈춤 시간 (실제 시간)
    [SerializeField] private float attackHitStopTimeScale;  // 기본 공격 멈춘 동안 timeScale (0이면 완전 정지)
    private bool isHitStop;
    private float hitStopEndTime;   // 끝나는 시각 (unscaledTime)

    [Header("===CameraShake===")]
    [SerializeField] private CinemachineImpulseSource hitImpulse;   // 셰이크 신호 (모양은 Inspector, 세기는 스킬 SO)

    private MikuCharacter owner;

    private void Start()
    {
        owner = GetComponent<MikuCharacter>();

        // 셰이크는 실제 시간으로 재생 -> 히트스톱(timeScale↓) 중에도 흔들림
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
    }

    private void Update()
    {
        TickHitStop();
    }

    // 기본 공격이 이번 프레임에 새로 맞힌 적이 있을 때 (여러 명이어도 한 번)
    public void PlayAttackHit(AttackStep step, List<Enemy> newHits)
    {
        StartHitStop(attackHitStopDuration, attackHitStopTimeScale);   // 기본 공격은 SO가 없어서 Inspector 값 사용
    }

    #region HitStop
    // 히트스톱: 맞은 순간 timeScale을 낮췄다가 실제 시간으로 duration 뒤 복구
    // 게임 시간으로 재면 느려진 동안 시간도 같이 멈춰서 안 풀림 -> unscaledTime
    public void StartHitStop(float duration, float timeScale)
    {
        // 퍼펙트 회피 슬로우가 timeScale을 쓰는 중이면 건드리지 않음
        if (owner.IsPerfectDodge) return;

        isHitStop = true;
        hitStopEndTime = Time.unscaledTime + duration;
        Time.timeScale = timeScale;

        DashLog.Log($"HitStopStart duration={duration} ts={timeScale} real={Time.unscaledTime:F3}");
    }

    private void TickHitStop()
    {
        if (isHitStop && Time.unscaledTime >= hitStopEndTime) EndHitStop();
    }

    // 회피 시작 시 MikuCharacter도 부름 (퍼펙트 슬로우보다 먼저 복구)
    public void EndHitStop()
    {
        if (!isHitStop) return;

        isHitStop = false;
        Time.timeScale = 1f;

        DashLog.Log($"HitStopEnd real={Time.unscaledTime:F3}");
    }
    #endregion

    #region CameraShake
    // 카메라 셰이크: Impulse Source가 신호를 내면 CM_Default의 Impulse Listener가 흔듦
    public void ShakeCamera(float force)
    {
        hitImpulse.GenerateImpulseWithForce(force);
        DashLog.Log($"CameraShake force={force} real={Time.unscaledTime:F3}");
    }
    #endregion
}
