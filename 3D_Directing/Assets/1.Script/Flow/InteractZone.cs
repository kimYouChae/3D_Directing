using UnityEngine;
using UnityEngine.InputSystem;

// 상호작용 대상 (Inspector에서 고름 -> FlowManager가 타입에 따라 이동)
public enum InteractType { Computer, Portal }

// 상호작용 구간: 구간 안 + Locomotion + E -> FlowManager.RequestInteract(type)
// 프리팹(포탈)으로 생성돼도 쓸 수 있게 씬 참조는 FlowManager.Instance로 찾음
// Collider는 Is Trigger 필수
public class InteractZone : MonoBehaviour
{
    [SerializeField] private InteractType type;

    private bool inside;

    private static MikuCharacter Miku => FlowManager.Instance.Miku;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<MikuCharacter>() != Miku) return;
        inside = true;
        DashLog.Log($"InteractZone enter type={type}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<MikuCharacter>() != Miku) return;
        inside = false;
        DashLog.Log($"InteractZone exit type={type}");
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.eKey.wasPressedThisFrame) return;

        if (!inside || !Miku.IsLocomotion)
        {
            DashLog.Log($"InteractBlocked type={type} inside={inside} locomotion={Miku.IsLocomotion}");
            return;
        }

        FlowManager.Instance.RequestInteract(type);
    }
}
