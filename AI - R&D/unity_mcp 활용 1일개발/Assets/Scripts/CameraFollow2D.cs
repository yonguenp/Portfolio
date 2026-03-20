using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(4f, 1.5f, -10f);
    public float smooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, 0f) + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        smoothedPosition.z = -10f; // Force Z rendering depth
        transform.position = smoothedPosition;
    }
}
