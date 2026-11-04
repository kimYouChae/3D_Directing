using UnityEngine;

public class ImpactFrameFX : MonoBehaviour
{
    [SerializeField] private Material material;
    [Range(0f, 1f)] public float intensity = 0f;

    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

    private void Update() => Apply();

    // 애니메이션(Timeline 미리보기 포함)이 필드 값을 바꾼 직후에 유니티가 불러줌
    private void OnDidApplyAnimationProperties() => Apply();

    // Inspector에서 슬라이더를 직접 움직였을 때
    private void OnValidate() => Apply();

    private void OnDisable()
    {
        if (material != null) material.SetFloat(IntensityId, 0f);
    }

    private void Apply()
    {
        if (material == null) { Debug.LogWarning("[ImpactFX] material 비어 있음"); return; }

        material.SetFloat(IntensityId, intensity);
    }
}
