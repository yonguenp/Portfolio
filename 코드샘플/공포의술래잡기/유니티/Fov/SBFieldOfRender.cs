using System.Collections.Generic;
using UnityEngine;


public class SBFieldOfRender : SBFieldOfView
{
    [SerializeField]
    protected LayerMask viewMask;
    [SerializeField]
    protected float meshResolution = 1;
    [SerializeField]
    protected int edgeResolveIteration = 0;
    [SerializeField]
    protected float edgeDstThreshold = 0f;
    [SerializeField]
    protected float maskCutawayDst = 1f;
    [SerializeField]
    protected MeshFilter viewMeshFilter;

    Mesh viewMesh = null;


    protected override void Start()
    {
        base.Start();

        if (viewMeshFilter != null)
        {
            viewMesh = new Mesh();
            viewMesh.name = "View Mesh";
            viewMeshFilter.mesh = viewMesh;
        }
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);

        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.transform.eulerAngles.y - viewAngle * .5f + stepAngleSize * i;
            var newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    var edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = ViewPosition;

        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);// + Vector3.forward * maskCutawayDst;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIteration; i++)
        {
            float angle = (minAngle + maxAngle) * .5f;
            var newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);

        RaycastHit2D hit = Physics2D.Raycast(transform.position + ViewPosition, dir, CurRadius, viewMask);

        if (hit)
        {

            Vector2 point = hit.point;
            //switch (Direction)
            //{
            //    case SBFOVDirection.Left:
            //        point.x -= hit.collider.bounds.size.x;
            //        break;
            //    case SBFOVDirection.Right:
            //        point.x += hit.collider.bounds.size.x;
            //        break;
            //    case SBFOVDirection.Down:
            //        point.y -= hit.collider.bounds.size.y;
            //        break;
            //    case SBFOVDirection.Down_Left:
            //        point.x -= hit.collider.bounds.size.x;
            //        point.y -= hit.collider.bounds.size.y;
            //        break;
            //    case SBFOVDirection.Down_Right:
            //        point.x += hit.collider.bounds.size.x;
            //        point.y -= hit.collider.bounds.size.y;
            //        break;
            //    case SBFOVDirection.Up_Left:
            //        point.x -= hit.collider.bounds.size.x;
            //        point.y += hit.collider.bounds.size.y;
            //        break;
            //    case SBFOVDirection.Up_Right:
            //        point.x += hit.collider.bounds.size.x;
            //        point.y += hit.collider.bounds.size.y;
            //        break;
            //    case SBFOVDirection.Up:
            //    default:
            //        point.y += hit.collider.bounds.size.y;
            //        break;
            //}
            return new ViewCastInfo(true, point, hit.distance, globalAngle);
        }
        else
            return new ViewCastInfo(false, transform.position + ViewPosition + dir * CurRadius, CurRadius, globalAngle);
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    #region 하위 클래스 구현부
    protected override void TargetOn(Transform target)
    {
    }
    protected override void TargetOff(Transform target)
    {
    }
    #endregion
}
