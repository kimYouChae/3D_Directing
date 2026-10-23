using UnityEngine;

// 스킬1 마법 데이터
[CreateAssetMenu(fileName = "MagicSkillData", menuName = "Skill/Magic")]
public class MagicSkillData : SkillData
{
    [Header("===Magic===")]
    [SerializeField] private float spawnTiming;     // 원 생성 진행도
    [SerializeField] private float castRange;       // 사거리 (원을 만들 적 탐색)
    [SerializeField] private float explodeDelay;    // 원 생성 후 폭발까지
    [SerializeField] private EffectSpawn circleEffect;  // 원 모양 이펙트 (원과 같이 사라짐, lifeTime 0)
    [SerializeField] private EffectSpawn hitEffect;     // 폭발 때 맞은 적의 몸에 생기는 이펙트

    public float SpawnTiming => spawnTiming;
    public float CastRange => castRange;
    public float ExplodeDelay => explodeDelay;
    public EffectSpawn CircleEffect => circleEffect;
    public EffectSpawn HitEffect => hitEffect;

    public override ISkill CreateSkill(MikuCharacter owner) => new MagicSkill(owner, this);
}
