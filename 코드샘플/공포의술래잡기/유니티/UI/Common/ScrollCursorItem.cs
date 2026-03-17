using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCursorItem : MonoBehaviour
{
    protected ScrollCursor ScrollCursor { get; private set; }
    public virtual void OnFocus(bool bFocus)
    {

    }

    public void SetParent(ScrollCursor sc, Transform container)
    {
        ScrollCursor = sc;
        transform.SetParent(container);
    }
}
