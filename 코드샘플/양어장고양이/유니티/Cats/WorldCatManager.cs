using Spine;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorldCatManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public FarmCanvas FarmCanvas;
    public ScrollRect scrollRect;
    public CatSkeletonGraphic catAnimationSample;
    public CatFollowerGraphic catCursorSample;
    
    public List<CatSkeletonGraphic> attachedCats = new List<CatSkeletonGraphic>();

    private int currentMainFinger = -1;
    private int currentSecondFinger = -1;
    private Vector2 posA;
    private Vector2 posB;
    private float previousDistance = -1f;
    private float distance;
    private float pinchDelta = 0f;

    private void Awake()
    {
        Clear();
    }

    private void OnEnable()
    {
        Clear();
        Init();
    }

    public void Init()
    {
        List<game_data> cats = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CATS);
        foreach(user_cats cat in cats)
        {
            cat_def catInfo = cat_def.GetCatInfo(cat.GetCatID());
            SetCatAnimation(catInfo);
        }        
    }

    public void SetCatAnimation(cat_def cat)
    {
        GameObject newCatAnimationObject = Instantiate(catAnimationSample.gameObject);
        GameObject newCatCursorObject = Instantiate(catCursorSample.gameObject);

        CatSkeletonGraphic newCatAnimation = newCatAnimationObject.GetComponent<CatSkeletonGraphic>();
        CatFollowerGraphic newCatCursor = newCatCursorObject.GetComponent<CatFollowerGraphic>();
        
        newCatAnimationObject.transform.SetParent(catAnimationSample.transform.parent);
        newCatAnimationObject.transform.position = catAnimationSample.transform.position;
        newCatAnimationObject.transform.localScale = catAnimationSample.transform.localScale;
        newCatAnimationObject.transform.SetSiblingIndex(catAnimationSample.transform.GetSiblingIndex());
        newCatAnimationObject.gameObject.SetActive(true);

        newCatCursor.transform.SetParent(catCursorSample.transform.parent);
        newCatCursor.transform.position = catCursorSample.transform.position;
        newCatCursor.transform.localScale = catCursorSample.transform.localScale;
        newCatCursor.gameObject.SetActive(false);

        newCatCursor.Init(newCatAnimation, this);
        newCatAnimation.InitAnimation(cat, newCatCursor);        

        attachedCats.Add(newCatAnimation);
    }

    public void Clear()
    {
        catAnimationSample.gameObject.SetActive(false);
        catCursorSample.gameObject.SetActive(false);

        foreach (CatSkeletonGraphic cat in attachedCats)
        {
            Destroy(cat.gameObject);
        }

        attachedCats.Clear();
    }

    private void Update()
    {
        //CatSkeletonGraphic pivot = null;
        //foreach (CatSkeletonGraphic cat in attachedCats)
        //{
        //    if (cat != null)
        //    {
        //        if (pivot != null)
        //        {
        //            if (cat.cursor.transform.localPosition.y > pivot.cursor.transform.localPosition.y)
        //            {
        //                int sibling = cat.cursor.transform.GetSiblingIndex();
        //                cat.cursor.transform.SetSiblingIndex(pivot.cursor.transform.GetSiblingIndex());
        //                pivot.cursor.transform.SetSiblingIndex(sibling);                        
        //            }
        //        }

        //        pivot = cat;
        //    }
        //}
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (currentMainFinger == -1)
        {
            currentMainFinger = data.pointerId;
            posA = data.position;

            if (currentSecondFinger == -1)
            {
                scrollRect.OnBeginDrag(data);
                FarmCanvas.FarmUIPanel.OnDragStartMap();
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
            posA = data.position;
        }

        if (currentSecondFinger == -1)
        {
            scrollRect.OnDrag(data);
            return;
        }

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
        if (currentMainFinger == data.pointerId)
        {
            currentMainFinger = -1;

            scrollRect.OnEndDrag(data);
            FarmCanvas.FarmUIPanel.OnDragEndMap();
        }
        if (currentSecondFinger == data.pointerId)
        {
            currentSecondFinger = -1;
        }
    }

    private void _processPinch()
    {
        RectTransform rt = transform as RectTransform;
        Vector2 MAX_SIZE = (FarmCanvas.transform as RectTransform).sizeDelta;
        Vector2 CONTENT_SIZE = rt.sizeDelta;

        float minRatio = 1.0f;
        float maxRatio = ((MAX_SIZE.y * 4) / CONTENT_SIZE.y);

        float pivot = rt.localScale.y;
        pivot += pivot * (pinchDelta / MAX_SIZE.x);

        float curRatio = Mathf.Max(pivot, minRatio);
        curRatio = Mathf.Min(curRatio, maxRatio);
        rt.localScale = Vector2.one * curRatio;
    }

    public void OnSelectCat(CatSkeletonGraphic cat)
    {
        if (cat == null)
            return;

        if (cat.curCatData == null)
            return;

        FarmCanvas.OnCatInfoUI(cat);
    }

    public void OnSelectAction(CatSkeletonGraphic cat)
    {
        if (cat == null)
            return;

        if (cat.curAnimationData == null)
            return;

        
        cat_action_def action = cat.cursor.GetActionData();
        FarmCanvas.OnCatInteraction(action);
    }
}
