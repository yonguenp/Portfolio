using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollEventHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    ScrollRect target;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (target)
            target.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (target)
            target.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (target)
            target.OnEndDrag(eventData);
    }
}
