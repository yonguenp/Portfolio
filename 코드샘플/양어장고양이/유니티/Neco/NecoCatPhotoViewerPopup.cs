using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class NecoCatPhotoViewerPopup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RawImage CardDetailImage;
    public VideoPlayer videoPlayer;
    public Text TitleText;

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
    Vector2 MAX_SIZE = Vector2.zero;
    Vector2 CONTENT_SIZE = Vector2.zero;
    List<neco_cat_memory> viewList = null;
    neco_cat_memory curCatMemory = null;

    public void OnClickCloseButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_PHOTO_VIEWER_POPUP);
    }

    public void OnShowCatPhoto(neco_cat_memory curMemory, List<neco_cat_memory> memories = null)
    {
        minRatio = 0.0f;
        maxRatio = 0.0f;
        MAX_SIZE = Vector2.zero;
        CONTENT_SIZE = Vector2.zero;

        curCatMemory = curMemory;
        viewList = memories;
        if(viewList == null)
        {
            viewList = new List<neco_cat_memory>();
            viewList.Add(curCatMemory);
        }

        videoPlayer.Stop();
        VideoClip preClip = videoPlayer.clip;
        Resources.UnloadAsset(preClip);

        RenderTexture target = videoPlayer.targetTexture;
        if (target)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = rt;
        }

       
        string path = curCatMemory.GetNecoMemorySource();
        string type = curCatMemory.GetNecoMemoryType();
        switch (type)
        {
            case "ani":
                VideoClip clip = Resources.Load<VideoClip>(path);
                if (clip)
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    videoPlayer.clip = clip;
                    videoPlayer.isLooping = true;
                    CardDetailImage.texture = videoPlayer.targetTexture;
                    CardDetailImage.uvRect = new Rect(Vector2.zero, Vector2.one);
                    videoPlayer.enabled = true;

                    videoPlayer.prepareCompleted += OnVideoPrepared;
                    videoPlayer.Prepare();
                }
                break;
            case "photo":
            default:
                videoPlayer.Stop();
                videoPlayer.enabled = false;
                Sprite sprite = Resources.Load<Sprite>(path);
                CardDetailImage.texture = sprite.texture;
                //CardDetailImage.uvRect = userCardData.GetUVRect();
                break;
        }

        Canvas canvas = transform.GetComponentInParent<Canvas>();
        RectTransform canvasRT = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRT.rect.size;

        RectTransform containerRT = CardDetailImage.transform.parent.GetComponent<RectTransform>();
        canvasSize.y = canvasSize.y - containerRT.offsetMax.y - containerRT.offsetMin.y;

        MAX_SIZE = canvasSize;
        CONTENT_SIZE = new Vector2(1200, 900);

        minRatio = MAX_SIZE.x / CONTENT_SIZE.x;
        maxRatio = MAX_SIZE.y / CONTENT_SIZE.y;

        {
            RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
            rt.sizeDelta = CONTENT_SIZE * minRatio;
            rt.localPosition = Vector2.zero;
        }

        gameObject.SetActive(true);

        PictureFixPosCheck(false);

        TitleText.text = curCatMemory.GetNecoMemoryTitle();        
    }

    public void OnVideoPrepared(VideoPlayer player)
    {
        player.Play();
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (currentMainFinger == -1)
        {
            currentMainFinger = data.pointerId;
            prevPoint = data.position;

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
            prevPoint = newPoint;

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

        _processPinch();
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

    private void PictureFixPosCheck(bool EnableSwipe)
    {
        RectTransform rt = CardDetailImage.GetComponent<RectTransform>();

        float curRatio = rt.sizeDelta.x / CONTENT_SIZE.x;
        if (curRatio < minRatio)
            rt.sizeDelta = CONTENT_SIZE * minRatio;
        if (curRatio > maxRatio)
            rt.sizeDelta = CONTENT_SIZE * maxRatio;

        Vector2 pos = rt.localPosition;
        float limit = Mathf.Abs((rt.sizeDelta.x / 2) - (MAX_SIZE.x / 2));
        if (pos.x >= limit)
        {
            if (EnableSwipe && Mathf.Abs(pos.x - limit) > (MAX_SIZE.x / 4))
            {
                //Debug.LogError("OnLeftSwipe");
                if (OnLeftSwipe())
                {
                    currentMainFinger = -1;
                    return;
                }
            }
            pos.x = limit;
        }
        if (pos.x <= limit * -1.0f)
        {
            if (EnableSwipe && Mathf.Abs(pos.x - limit) > (MAX_SIZE.x / 4))
            {
                //Debug.LogError("OnRightSwipe");
                if (OnRightSwipe())
                {
                    currentMainFinger = -1;
                    return;
                }
            }
            pos.x = limit * -1.0f;
        }

        if (curRatio <= minRatio)
        {
            pos.y = 0;
        }
        else
        {
            limit = Mathf.Abs((rt.sizeDelta.y / 2) - (MAX_SIZE.y / 2));
            if (pos.y > limit)
            {
                pos.y = limit;
            }
            if (pos.y < limit * -1.0f)
            {
                pos.y = limit * -1.0f;
            }
        }

        rt.localPosition = pos;
    }

    private void _processSwipe()
    {
        RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
        Vector2 pos = (Vector2)rt.localPosition + screenTravel;

        float limit = Mathf.Abs((rt.sizeDelta.x / 2) - (MAX_SIZE.x / 2));
        float curRatio = rt.sizeDelta.x / CONTENT_SIZE.x;

        rt.localPosition = pos;
        if (curRatio <= minRatio)
        {
            pos.y = 0;
        }
        else
        {
            limit = Mathf.Abs((rt.sizeDelta.y / 2) - (MAX_SIZE.y / 2));
            if (pos.y > limit)
            {
                pos.y = limit;
            }
            if (pos.y < limit * -1.0f)
            {
                pos.y = limit * -1.0f;
            }
        }

        rt.localPosition = pos;
    }

    private void _processPinch()
    {
        RectTransform rt = CardDetailImage.GetComponent<RectTransform>();
        float curRatio = Mathf.Max(((rt.sizeDelta.x / CONTENT_SIZE.x) + (pinchDelta / CONTENT_SIZE.x)), minRatio);
        curRatio = Mathf.Min(curRatio, maxRatio);
        rt.sizeDelta = CONTENT_SIZE * curRatio;
    }

    bool OnRightSwipe()
    {
        if (viewList != null)
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                if (viewList[i] == curCatMemory)
                {
                    int targetIndex = i + 1;
                    if (targetIndex >= viewList.Count)
                        targetIndex = 0;

                    if (viewList[targetIndex] == curCatMemory)
                    {
                        PictureFixPosCheck(false);
                        return false;
                    }

                    OnShowCatPhoto(viewList[targetIndex], viewList);
                    AudioSource source = GetComponent<AudioSource>();
                    if (source)
                    {
                        source.Play();
                    }
                    return true;
                }
            }
        }

        return false;
    }

    bool OnLeftSwipe()
    {
        if (viewList != null)
        {
            for (int i = 0; i < viewList.Count; i++)
            {
                if (viewList[i] == curCatMemory)
                {
                    int targetIndex = i - 1;
                    if (targetIndex < 0)
                        targetIndex = viewList.Count - 1;

                    if (viewList[targetIndex] == curCatMemory)
                    {
                        PictureFixPosCheck(false);
                        return false;
                    }

                    OnShowCatPhoto(viewList[targetIndex], viewList);
                    AudioSource source = GetComponent<AudioSource>();
                    if (source)
                    {
                        source.Play();
                    }

                    return true;
                }
            }
        }

        return false;
    }

    public void OnExitViewer()
    {
        OnClickCloseButton();
    }
}
