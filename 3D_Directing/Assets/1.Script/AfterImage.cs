using UnityEngine;
using UnityEngine.Rendering;

public class AfterImage : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private Material material;
    private Mesh[] meshes;
    private Color baseColor;
    private float lifeTime;
    private float elapsed;

    public static void Spawn(SkinnedMeshRenderer[] renderers, Material source, float lifeTime)
    {
        GameObject root = new GameObject("AfterImage");
        root.AddComponent<AfterImage>().Init(renderers, source, lifeTime);
    }

    private void Init(SkinnedMeshRenderer[] renderers, Material source, float lifeTime)
    {
        this.lifeTime = lifeTime;
        material = new Material(source);    // 잔상마다 알파를 따로 줄이기 위해 복제
        baseColor = material.GetColor(BaseColorId);
        meshes = new Mesh[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer smr = renderers[i];
            if (!smr.enabled || !smr.gameObject.activeInHierarchy) continue;

            // 현재 포즈를 고정 메시로 굽기 (true: 스케일 포함)
            Mesh mesh = new Mesh();
            smr.BakeMesh(mesh, true);
            meshes[i] = mesh;

            GameObject part = new GameObject(smr.name);
            part.transform.SetParent(transform);
            part.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);

            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer mr = part.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = ShadowCastingMode.Off;

            // 서브메시 수만큼 같은 흰색 머티리얼
            Material[] mats = new Material[mesh.subMeshCount];
            for (int m = 0; m < mats.Length; m++) mats[m] = material;
            mr.sharedMaterials = mats;
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        Color c = baseColor;
        c.a = baseColor.a * Mathf.Clamp01(1f - elapsed / lifeTime);
        material.SetColor(BaseColorId, c);

        if (elapsed >= lifeTime) Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 코드로 만든 Mesh, Material은 GameObject와 같이 사라지지 않음
        foreach (Mesh mesh in meshes)
            if (mesh != null) Destroy(mesh);
        Destroy(material);
    }

}
