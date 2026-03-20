using UnityEngine;
using UnityEditor;

public static class RemovePhysicsComponents
{
    [MenuItem("Tools/Remove All Physics Components")]
    public static void Run()
    {
        var types = new System.Type[]
        {
            typeof(Rigidbody),
            typeof(Rigidbody2D),
            typeof(BoxCollider),
            typeof(BoxCollider2D),
            typeof(CircleCollider2D),
            typeof(CapsuleCollider2D),
            typeof(PolygonCollider2D),
            typeof(EdgeCollider2D),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(MeshCollider),
        };

        int removed = 0;
        var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var go in allGOs)
        {
            foreach (var t in types)
            {
                var comp = go.GetComponent(t);
                if (comp != null)
                {
                    Debug.Log(string.Format("[RemovePhysics] {0} → {1} 제거", go.name, t.Name));
                    Object.DestroyImmediate(comp);
                    removed++;
                }
            }
        }

        Debug.Log(string.Format("[RemovePhysics] 완료: 총 {0}개 컴포넌트 제거", removed));
    }
}
