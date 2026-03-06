using UnityEditor;
using UnityEngine;
using System.Text;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(EscapeDoorHelper))]
public class EscapeDoorHelperEditor : Editor
{

    EscapeDoorHelper doorTarget = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("DoorId 셋팅"))
        {
            var escapeKeyObj = doorTarget.DoorSwitch.gameObject;
            if (escapeKeyObj == null)
            {
                EditorUtility.DisplayDialog("escapeEvent 확인해주세요.", "escapeEvent 확인되지 않음", "확인");
                return;
            }

            var bc = escapeKeyObj.GetComponent<BoxCollider2D>();
            if (bc == null)
            {
                var textsb = new StringBuilder().AppendFormat("[Objects]{0}", escapeKeyObj.name);
                EditorUtility.DisplayDialog("BoxCollider2D를 넣어주세요.", textsb.ToString(), "확인");
                return;
            }

            float offx = 0.5f;
            float offy = 0.5f;

            offx += bc.offset.x / bc.size.x;
            offy += bc.offset.y / bc.size.y;

            var width = (int)bc.size.x * 100;
            var height = (int)bc.size.y * 100;

            float round_x = (float)Math.Round(bc.transform.position.x, 1);
            float round_y = (float)Math.Round(bc.transform.position.y, 1);

            var x = (int)(round_x * 100) - (int)(offx * width);
            var y = (int)(round_y * 100) - (int)(offy * height);

            var id = x * 100000 + y;

            doorTarget.SetDoorId(id);
        }
    }

    private void OnEnable()
    {
        doorTarget = (EscapeDoorHelper)base.target;

    }
}
