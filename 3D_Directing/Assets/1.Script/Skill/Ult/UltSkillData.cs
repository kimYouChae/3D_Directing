using UnityEngine;
using UnityEngine.Serialization;

// 궁극기 데이터 (공통 range = 장판 반경, damage = 틱 데미지, castEffect는 비워둘 것)
[CreateAssetMenu(fileName = "UltSkillData", menuName = "Skill/Ult")]
public class UltSkillData : SkillData
{
    [Header("===Ult===")]
    [FormerlySerializedAs("freezeTiming")]
    [SerializeField, Range(0f, 1f)] private float spawnTiming;  // 장판 생성 진행도 (Timeline 기준)
    [FormerlySerializedAs("dotDuration")]
    [SerializeField] private float fieldDuration;               // 장판 유지 시간
    [FormerlySerializedAs("dotInterval")]
    [SerializeField] private float tickInterval;                // 얼림+데미지 간격
    [SerializeField] private float freezeDuration;              // 마지막 틱 이후 얼어 있는 시간
    [SerializeField] private EffectSpawn snowEffect;            // 눈 (장판 자식, lifeTime 0)
    [SerializeField] private EffectSpawn freezeEffect;          // 적에게 붙는 얼음 (lifeTime 0)

    public float SpawnTiming => spawnTiming;
    public float FieldDuration => fieldDuration;
    public float TickInterval => tickInterval;
    public float FreezeDuration => freezeDuration;
    public EffectSpawn SnowEffect => snowEffect;
    public EffectSpawn FreezeEffect => freezeEffect;

    public override ISkill CreateSkill(MikuCharacter owner) => new UltSkill(owner, this);
}
