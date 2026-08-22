using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 정점 색에 음영을 구워 넣은 공용 메쉬.
///
/// 이 게임은 전부 Sprites/Default(언릿)로 그려서 라이트가 안 먹는다. 그래서
/// 큐브가 단색 사각형으로 보이고 입체감이 전혀 없었다. Sprites/Default는
/// 정점 색을 곱하므로(<c>OUT.color = IN.color * _Color</c>) 면별 밝기를
/// 메쉬에 미리 구워두면 셰이더·라이트 변경 없이 입체로 보인다.
///
/// 머티리얼 색은 그대로 곱해지므로 기존 HP 색상·피격 플래시·위험 깜빡임
/// 로직은 손댈 필요가 없다.
/// </summary>
public static class BrickBreakerMeshes
{
    // 빛은 왼쪽 위 앞에서 온다고 가정한다.
    static readonly Vector3 LightDir = new Vector3(-1f, 1f, -1f).normalized;

    static Mesh cube;
    static Mesh sphere;

    /// <summary>면마다 밝기가 다른 1×1×1 큐브. Unity 기본 Cube와 크기가 같다.</summary>
    public static Mesh Cube
    {
        get
        {
            if (cube) return cube;

            // (법선, 면 내 u축, 면 내 v축, 밝기)
            var faces = new (Vector3 n, Vector3 u, Vector3 v, float b)[]
            {
                (Vector3.back,    Vector3.right,   Vector3.up,      1.00f), // 플레이어를 향한 면
                (Vector3.up,      Vector3.right,   Vector3.forward, 0.92f),
                (Vector3.left,    Vector3.forward, Vector3.up,      0.78f),
                (Vector3.right,   Vector3.forward, Vector3.up,      0.60f),
                (Vector3.forward, Vector3.right,   Vector3.up,      0.50f),
                (Vector3.down,    Vector3.right,   Vector3.forward, 0.42f),
            };

            var verts = new List<Vector3>(24);
            var norms = new List<Vector3>(24);
            var cols  = new List<Color>(24);
            var uvs   = new List<Vector2>(24);
            var tris  = new List<int>(36);

            foreach (var f in faces)
            {
                int b0 = verts.Count;
                Vector3 c = f.n * 0.5f;
                verts.Add(c - f.u * 0.5f - f.v * 0.5f);
                verts.Add(c + f.u * 0.5f - f.v * 0.5f);
                verts.Add(c + f.u * 0.5f + f.v * 0.5f);
                verts.Add(c - f.u * 0.5f + f.v * 0.5f);

                var col = new Color(f.b, f.b, f.b, 1f);
                for (int i = 0; i < 4; i++) { norms.Add(f.n); cols.Add(col); }

                uvs.Add(new Vector2(0f, 0f)); uvs.Add(new Vector2(1f, 0f));
                uvs.Add(new Vector2(1f, 1f)); uvs.Add(new Vector2(0f, 1f));

                tris.Add(b0); tris.Add(b0 + 2); tris.Add(b0 + 1);
                tris.Add(b0); tris.Add(b0 + 3); tris.Add(b0 + 2);
            }

            cube = new Mesh { name = "BB_ShadedCube", hideFlags = HideFlags.HideAndDontSave };
            cube.SetVertices(verts);
            cube.SetNormals(norms);
            cube.SetColors(cols);
            cube.SetUVs(0, uvs);
            cube.SetTriangles(tris, 0);
            cube.RecalculateBounds();
            return cube;
        }
    }

    /// <summary>법선·광원 내적을 정점 색으로 구운 구. Unity 기본 Sphere와 크기가 같다.</summary>
    public static Mesh Sphere
    {
        get
        {
            if (sphere) return sphere;

            // 기본 구 메쉬를 얻으려면 프리미티브를 한 번 만들어야 한다.
            var probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var src   = probe.GetComponent<MeshFilter>().sharedMesh;

            sphere = Object.Instantiate(src);
            sphere.name      = "BB_ShadedSphere";
            sphere.hideFlags = HideFlags.HideAndDontSave;

            var n = sphere.normals;
            var c = new Color[n.Length];
            for (int i = 0; i < n.Length; i++)
            {
                float b = 0.42f + 0.58f * Mathf.Clamp01(Vector3.Dot(n[i], LightDir));
                c[i] = new Color(b, b, b, 1f);
            }
            sphere.colors = c;

            if (Application.isPlaying) Object.Destroy(probe);
            else                       Object.DestroyImmediate(probe);
            return sphere;
        }
    }

    static Mesh tetra;

    /// <summary>정사면체. 큐브의 교대 꼭짓점 4개를 쓰면 정확한 정사면체가 된다.</summary>
    public static Mesh Tetra
    {
        get
        {
            if (tetra) return tetra;

            float a = 0.875f;   // 셀 반칸 — 박스와 같은 자리를 차지한다
            Vector3[] v =
            {
                new Vector3( a,  a,  a),
                new Vector3( a, -a, -a),
                new Vector3(-a,  a, -a),
                new Vector3(-a, -a,  a),
            };
            int[][] faces = { new[]{1,2,3}, new[]{0,3,2}, new[]{0,1,3}, new[]{0,2,1} };
            float[] bright = { 0.95f, 0.78f, 0.60f, 0.45f };

            var verts = new List<Vector3>(12);
            var norms = new List<Vector3>(12);
            var cols  = new List<Color>(12);
            var uvs   = new List<Vector2>(12);
            var tris  = new List<int>(12);

            for (int f = 0; f < 4; f++)
            {
                var  p0 = v[faces[f][0]]; var p1 = v[faces[f][1]]; var p2 = v[faces[f][2]];
                var  n  = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                var  c  = new Color(bright[f], bright[f], bright[f], 1f);
                int  b0 = verts.Count;

                verts.Add(p0); verts.Add(p1); verts.Add(p2);
                for (int k = 0; k < 3; k++) { norms.Add(n); cols.Add(c); }
                uvs.Add(new Vector2(0,0)); uvs.Add(new Vector2(1,0)); uvs.Add(new Vector2(0.5f,1));
                tris.Add(b0); tris.Add(b0+1); tris.Add(b0+2);
            }

            tetra = new Mesh { name = "BB_Tetra", hideFlags = HideFlags.HideAndDontSave };
            tetra.SetVertices(verts); tetra.SetNormals(norms); tetra.SetColors(cols);
            tetra.SetUVs(0, uvs);     tetra.SetTriangles(tris, 0);
            tetra.RecalculateBounds();
            return tetra;
        }
    }

    /// <summary>메쉬 렌더러 한 쌍을 붙인 GameObject를 만든다. 콜라이더는 붙지 않는다.</summary>
    public static GameObject Make(string name, Mesh mesh, Material mat)
    {
        var go = new GameObject(name);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.material            = mat;
        mr.shadowCastingMode   = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows      = false;
        mr.lightProbeUsage     = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        return go;
    }
}
