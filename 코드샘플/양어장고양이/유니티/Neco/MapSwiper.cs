using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public sealed class MapSwiper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector2 prevPoint;
    private Vector2 newPoint;
    private Vector2 screenTravel;
    private int currentMainFinger = -1;
    private int currentSecondFinger = -1;
    private Vector2 posA;
    private Vector2 posB;
    private float previousDistance = -1f;
    private float distance;
    private Coroutine moveCoroutine = null;

    public GameObject Left = null;
    public GameObject Right = null;

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        Clear();
    }

    public void Clear()
    {
        if (Left)
            Left.SetActive(false);
        if (Right)
            Right.SetActive(false);

        screenTravel = Vector2.zero;
        prevPoint = newPoint;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        SwipeFixPosCheck();
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (currentMainFinger == -1)
        {
            currentMainFinger = data.pointerId;
            prevPoint = data.position;
            newPoint = data.position;
            screenTravel = Vector2.zero;
            posA = data.position;
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            return;
        }

        if (currentSecondFinger == -1)
        {
            currentSecondFinger = data.pointerId;
            posB = data.position;

            figureDelta();
            previousDistance = distance;

            return;
        }

        Debug.Log("third+ finger! (ignore)");
    }

    public void OnDrag(PointerEventData data)
    {
        if (currentMainFinger == data.pointerId)
        {
            newPoint = data.position;
            screenTravel = newPoint - prevPoint;
            //prevPoint = newPoint;

            if (currentSecondFinger == -1)
            {
                if(_processSwipe())
                {
                    screenTravel = Vector2.zero;
                    prevPoint = newPoint;
                    if (moveCoroutine != null)
                    {
                        StopCoroutine(moveCoroutine);
                        moveCoroutine = null;
                    }
                    if (Left)
                        Left.SetActive(false);
                    if (Right)
                        Right.SetActive(false);
                }
            }
            else
            {

            }

            posA = data.position;
        }

        if (moveCoroutine == null)
        {
            float pivotDistance = 60.0f;

            if (Mathf.Abs(screenTravel.x) > pivotDistance)
            {
                moveCoroutine = StartCoroutine(MapMoveGuide((screenTravel.x) > pivotDistance ? Left : Right));
            }
        }

        if (currentSecondFinger == -1) return;

        if (currentMainFinger == data.pointerId) posA = data.position;
        if (currentSecondFinger == data.pointerId) posB = data.position;

        figureDelta();
        previousDistance = distance;
    }

    private void figureDelta()
    {
        // when/if two touches, keep track of the distance between them
        distance = Vector2.Distance(posA, posB);
    }

    public void OnPointerUp(PointerEventData data)
    {
        bool EnableSwipe = false;
        if (currentMainFinger == data.pointerId)
        {
            currentMainFinger = -1;
            EnableSwipe = true;            
        }
        if (currentSecondFinger == data.pointerId)
        {
            currentSecondFinger = -1;
        }

        SwipeFixPosCheck();
    }

    private bool _processSwipe()
    {
        bool ret = false;

        NecoGameCanvas canvas = NecoCanvas.GetGameCanvas();
        if (canvas)
        {
            RectTransform rt = canvas.transform as RectTransform;
            if (rt)
            {
                Vector2 halfOffset = rt.sizeDelta * 0.5f;
                MapObjectController controller = canvas.GetCurMapController();
                if (controller)
                {
                    RectTransform bg = controller.MapBackgroundImage;
                    Vector2 bgHalfSize = bg.sizeDelta * bg.localScale * 0.5f;

                    Vector2 limitOffset = bgHalfSize - halfOffset;

                    if (limitOffset.x > 0)
                    {
                        Vector2 curPos = bg.localPosition;
                        curPos.x += screenTravel.x;

                        float x = Mathf.Clamp(curPos.x, limitOffset.x * -1.0f, limitOffset.x);
                        ret = x == curPos.x;
                        curPos.x = x;
                        bg.localPosition = curPos;
                    }
                }
            }
        }

        return ret;
    }

    private void SwipeFixPosCheck()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (Left)
            Left.SetActive(false);
        if(Right)
            Right.SetActive(false);

        float pivotDistance = 60.0f;
        if((screenTravel.x) > pivotDistance)
        {
            if (neco_data.PrologueSeq.스와이프가이드 == neco_data.GetPrologueSeq())
            {
                return;
            }
            NecoCanvas.GetGameCanvas().OnMoveLeftMap();
            screenTravel.x = 0.0f;
            return;
        }

        if ((screenTravel.x) < pivotDistance * -1.0f)
        {
            NecoCanvas.GetGameCanvas().OnMoveRightMap();
            screenTravel.x = 0.0f;
            return;
        }
    }


    private IEnumerator MapMoveGuide(GameObject ui)
    {
        if (ui != null)
        {
            ui.SetActive(true);
            
            yield return new WaitForSeconds(0.5f);

            Clear();
        }
    }
}