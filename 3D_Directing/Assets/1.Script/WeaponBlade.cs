using UnityEngine;

public class WeaponBlade : MonoBehaviour
{
    private float length = 0.005f;
    private float radius = 0.02f;

    public float Radius => radius;

    public void GetCapsule(out Vector3 p1, out Vector3 p2)
    {
        Vector3 half = transform.forward * (length * 0.5f);
        p1 = transform.position - half;
        p2 = transform.position + half;
    }

    private void OnDrawGizmos()
    {
        Vector3 half = transform.up * (length);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position - half, radius);
        Gizmos.DrawWireSphere(transform.position + half, radius);
    }
}
