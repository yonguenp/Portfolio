using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;

    Mesh mesh = null;
    Vector3 origin;
    float startingAngle;
    float fov;
    float viewDistance;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
        fov = 90;
        viewDistance = 3f;
        SetOrigin(transform.position);
        SetAimDirection(Vector3.up);
    }

    private void LateUpdate()
    {
        var mp = Input.mousePosition;
        mp.z = 10.0f;
        mp = Camera.main.ScreenToWorldPoint(mp);
        var aim = mp - transform.position;
        SetAimDirection(aim);

        int rayCount = 50;
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            float angleRad = angle * (Mathf.PI / 180f);
            var calAngle = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector3 vertex = origin + calAngle * viewDistance;

            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, calAngle, viewDistance, layerMask);
            if (raycastHit2D.collider == null)
                vertex = origin + calAngle * viewDistance;
            else
                vertex = raycastHit2D.point;

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;

            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    void SetAimDirection(Vector3 aimDirection)
    {
        var nor = aimDirection.normalized;
        float n = Mathf.Atan2(nor.y, nor.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;

        startingAngle = n - fov / 2f + 90;
        SBDebug.Log($"start angle = {startingAngle}, n = {n}");
    }

    public void SetFov(float fov)
    {
        this.fov = fov;
    }

    public void SetViewDistance(float viewDistance)
    {
        this.viewDistance = viewDistance;
    }

}
