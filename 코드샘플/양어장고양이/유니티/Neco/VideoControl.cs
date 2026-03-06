using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public sealed class VideoControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public VideoClip clip;
    public RenderTexture targetTexture;
    
    private RawImage rawImage;

    private Vector2 prevPoint;
    private Vector2 newPoint;
    private Vector2 screenTravel;
    private int currentMainFinger = -1;
    private int currentSecondFinger = -1;
    private Vector2 posA;
    private Vector2 posB;
    private float previousDistance = -1f;
    private float distance;
    private float pinchDelta = 0f;

    float minRatio = 0.0f;
    float maxRatio = 0.0f;
    float limit = 0.0f;
    Vector2 MAX_SIZE = Vector2.zero;
    Vector2 CONTENT_SIZE = Vector2.zero;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        if(rawImage == null)
        {
            rawImage = gameObject.AddComponent<RawImage>();
        }
    }

    private void OnEnable()
    {
        PlayVideo();

        Canvas canvas = NecoCanvas.GetGameCanvas().GetComponent<Canvas>();
        RectTransform canvasRT = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRT.rect.size;

        MAX_SIZE = canvasSize;
        CONTENT_SIZE = (transform as RectTransform).sizeDelta;

        minRatio = MAX_SIZE.x / CONTENT_SIZE.x;
        maxRatio = MAX_SIZE.y / CONTENT_SIZE.y;

        PictureFixPosCheck(false);
    }

    void PlayVideo()
    {
        rawImage.texture = targetTexture;

        VideoManager.GetInstance().PlayBackgroundVideo(clip, targetTexture);
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
                _processSwipe();
            }
            else
            {

            }

            posA = data.position;
        }

        if (currentSecondFinger == -1) return;

        if (currentMainFinger == data.pointerId) posA = data.position;
        if (currentSecondFinger == data.pointerId) posB = data.position;

        figureDelta();
        pinchDelta = distance - previousDistance;
        previousDistance = distance;

        //_processPinch();
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

        PictureFixPosCheck(EnableSwipe);
    }

    private void _processSwipe()
    {
        return;

        RectTransform rt = transform as RectTransform;
        Vector2 pos = (Vector2)rt.localPosition + screenTravel;

        if (pos.x >= limit)
        {
            pos.x = limit;
        }
        if (pos.x <= limit * -1.0f)
        {
            pos.x = limit * -1.0f;
        }

        pos.y = 0;

        rt.localPosition = pos;
    }

    private void _processPinch()
    {
        //RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
        //float curRatio = Mathf.Max(((rt.sizeDelta.x / CONTENT_SIZE.x) + (pinchDelta / CONTENT_SIZE.x)), minRatio);
        //curRatio = Mathf.Min(curRatio, maxRatio);
        //rt.sizeDelta = CONTENT_SIZE * curRatio;
    }

    private void PictureFixPosCheck(bool EnableSwipe)
    {
        if((screenTravel.x) > (Screen.width / 2))
        {
            NecoCanvas.GetGameCanvas().OnMoveLeftMap();
            return;
        }

        if ((screenTravel.x) < (Screen.width / 2) * -1.0f)
        {
            NecoCanvas.GetGameCanvas().OnMoveRightMap();
            return;
        }

        return;

        RectTransform rt = transform as RectTransform;
        Vector2 rtSize = rt.sizeDelta * rt.localScale;
        MAX_SIZE = (NecoCanvas.GetGameCanvas().transform as RectTransform).sizeDelta;

        Vector2 pos = rt.localPosition;
        limit = Mathf.Abs((rtSize.x / 2) - (MAX_SIZE.x / 2));
        if (pos.x >= limit)
        {
            if (EnableSwipe && Mathf.Abs(pos.x - limit) > (MAX_SIZE.x / 4))
            {
                //Debug.LogError("OnLeftSwipe");
                //if (OnLeftSwipe())
                //{
                //    currentMainFinger = -1;
                //    return;
                //}
            }
            pos.x = limit;
        }
        if (pos.x <= limit * -1.0f)
        {
            if (EnableSwipe && Mathf.Abs(pos.x - limit) > (MAX_SIZE.x / 4))
            {
                //Debug.LogError("OnRightSwipe");
                //if (OnRightSwipe())
                //{
                //    currentMainFinger = -1;
                //    return;
                //}
            }
            pos.x = limit * -1.0f;
        }

        pos.y = 0;

        rt.localPosition = pos;
    }
}