using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchGuide : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public vpr_manager VideoPlayerManager;
    void OnEnable()
    {
        //gameObject.GetComponent<SpriteRenderer>().DOFade(0.5f, 0.4f).SetLoops(-1, LoopType.Yoyo);
    }
    public void OnPointerDown(PointerEventData data)
    {
        //VideoPlayerManager.resume_current_clip();
    }
    public void OnDrag(PointerEventData data)
    {
        //VideoPlayerManager.resume_current_clip();
    }

    public void OnPointerUp(PointerEventData data)
    {
        //VideoPlayerManager.resume_current_clip();
    }
}
