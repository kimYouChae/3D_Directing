using UnityEngine;
using UnityEngine.InputSystem;

// 컴퓨터 앞 상호작용 구간: 구간 안 + Locomotion + E -> FlowManager.RequestDream
// Collider는 Is Trigger 필수
public class InteractZone : MonoBehaviour
{
    [SerializeField] private FlowManager flow;
    [SerializeField] private MikuCharacter miku;

    private bool inside;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<MikuCharacter>() != miku) return;
        inside = true;
        DashLog.Log("InteractZone enter");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<MikuCharacter>() != miku) return;
        inside = false;
        DashLog.Log("InteractZone exit");
    }

    private void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || !kb.eKey.wasPressedThisFrame) return;

        if (!inside || !miku.IsLocomotion)
        {
            DashLog.Log($"InteractBlocked inside={inside} locomotion={miku.IsLocomotion}");
            return;
        }

        flow.RequestDream();
    }
}
