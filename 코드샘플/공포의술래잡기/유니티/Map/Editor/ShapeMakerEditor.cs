using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering.Universal;

[CustomEditor(typeof(ShapeMaker))]
public class ShapeMakerEditor : Editor
{
    ShapeMaker shape = null;
    Vector3 prevVec = Vector2.zero;
    float snapValue = 0.25f;

    private void OnEnable() {
        shape = (ShapeMaker)target;
        prevVec = shape.transform.position;
        Debug.Log(shape.gameObject.name);
    }

    public override void OnInspectorGUI ()
    {
        base.OnInspectorGUI ();

        if(shape == null) return;

        float posX = shape.transform.position.x;
        float modX = posX % 1;
        if(modX < snapValue)
            posX = (int)posX;
        else if(modX > snapValue && modX < 1 - snapValue)
            posX = (int)(posX)+ 0.5f;
        else 
            posX = (int)(posX + 1);

        float posY = shape.transform.position.y;
        float modY = posY % 1;
        if(modY < snapValue)
            posY = (int)posY;
        else if(modY > snapValue && modY < 1 - snapValue)
            posY = (int)(posY)+ 0.5f;
        else 
            posY = (int)(posY + 1);

        shape.transform.position = new Vector3(posX, posY, shape.transform.position.z);
       
        shape.Width = int.Parse( EditorGUILayout.TextField("Width", shape.Width.ToString()) );
        shape.Height = int.Parse( EditorGUILayout.TextField("Height", shape.Height.ToString()) );

        if(GUILayout.Button("Box2D 생성"))
        {
            if(shape.Width == 0 || shape.Height == 0)
            {
                Debug.LogError("width, height 0이 아니어야 합니다.");
                return;
            }

            BoxCollider2D b2d = shape.gameObject.GetComponent<BoxCollider2D>();
            if(b2d == null)
                b2d = shape.gameObject.AddComponent<BoxCollider2D>();
            b2d.size = new Vector2(shape.Width, shape.Height);

            var sc = shape.gameObject.GetComponent<ShadowCaster2D>();
            if(sc != null)
            {
                DestroyImmediate(sc);
                sc = shape.gameObject.AddComponent<ShadowCaster2D>();
            }
                
        }

        if(GUILayout.Button("ShadowCaster2D 생성"))
        {
            var sc = shape.gameObject.GetComponent<ShadowCaster2D>();
            if(sc == null)
                sc = shape.gameObject.AddComponent<ShadowCaster2D>();
        }

        if(GUILayout.Button("초기화"))
        {
            var b2d = shape.gameObject.GetComponent<BoxCollider2D>();
            if(b2d != null)
                DestroyImmediate(b2d);

            var sc = shape.gameObject.GetComponent<ShadowCaster2D>();
            if(sc != null)
                DestroyImmediate(sc);
        }

        if(GUILayout.Button("Box2d 삭제"))
        {
            var b2d = shape.gameObject.GetComponent<BoxCollider2D>();
            if(b2d != null)
                DestroyImmediate(b2d);
        }

        if(GUILayout.Button("ShadowCaster2D 삭제"))
        {
            var sc = shape.gameObject.GetComponent<ShadowCaster2D>();
            if(sc != null)
                DestroyImmediate(sc);
        }
    }
}
