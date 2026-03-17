using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudPosText : HudPos
{
    Text text = null;
    Text text2 = null;

    // Start is called before the first frame update
    public GameObject InitPos(Vector2 pos)
    {
        var go = transform.Find("Text");
        text = go.GetComponent<Text>();
        text.text = $"[{pos}]";

        go = transform.Find("Text2");
        text2 = go.GetComponent<Text>();
        text2.text = $"[{pos}]";

        go.transform.localScale = new Vector3(1, 1, 1);

        return go.gameObject;
    }

    public override void Init(GameObject character, Camera rc, RectTransform canvas, Transform parent, float playTime = 0)
    {
        base.Init(character, rc, canvas, parent);
    }

    public void SetPos(Vector2 pos)
    {
        if (text != null) text.text = $"[{pos}]";
    }

    public void SetServerPos(Vector2 pos)
    {
        if (text2 != null) text2.text = $"[{pos}]";
    }
}
