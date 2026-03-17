using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EscapeDoorHelper))]
public class EscapeDoor : BaseObject
{
    public int EscapeDoorId = 0;
    public float width = 0;
    public float height = 0;

    float minX = 0;
    float minY = 0;
    float maxX = 0;
    float maxY = 0;

    public EscapeEvent escapeEvent = null;

    GameObject model = null;

    public bool IsOpen { get; set; } = false;


    private void Start()
    {
        minX = -(width / 2);
        maxX = (width / 2);
        minY = -(height / 2);
        maxY = (height / 2);

        Managers.Object.AddEscapeDoor(this);
        // Managers.Object.BindEscapeKey(this);
        if (escapeEvent != null)
        {
            escapeEvent.Init();
            escapeEvent.Show(false);
        }

        var tr = transform.Find("model");
        if (tr != null)
        {
            model = tr.gameObject;
            SBDebug.LogError("Find not model from EscapeDoor");
        }
    }

    //bool InRect(float x, float y)
    //{
    //    var pos = new Vector3(x, y);
    //    var subPos = transform.position - pos;

    //    if (minX < subPos.x &&
    //        maxX > subPos.x &&
    //        minY < subPos.y &&
    //        maxY > subPos.y)
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    public void OpenDoor()
    {
        IsOpen = true;
        gameObject.SetActive(false);
        if (escapeEvent != null)
        {
            escapeEvent.Init();
            escapeEvent.Show(true);
        }
    }

    public override void ShowRenderer(bool isShow)
    {
        //나중에 수정합시다.
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = isShow;
        if (!isShow)
        {
            IsOpen = true;
            if (escapeEvent != null)
            {
                escapeEvent.Init();
                escapeEvent.Show(true);
            }
        }
    }
}
