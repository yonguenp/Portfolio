using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;

public class ScrollCursor : ScrollRect
{
    protected override void Awake()
    {
        onValueChanged.AddListener(ScrollChanged);
    }
    public void SetCursor(int index)
    {
        float ratio = 1.0f / (content.childCount - 1);

        float curRatio = (index * ratio);
        if (index == 0)
            curRatio = 0.0f;
        if (index == content.childCount - 1)
            curRatio = 1.0f;

        horizontalNormalizedPosition = curRatio;
        RefreshCursor();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        ClearCursor();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        RefreshCursor();
    }

    public void ScrollChanged(Vector2 val)
    {
        CancelInvoke("RefreshCursor");
        Invoke("RefreshCursor", 0.1f);
    }

    void ClearCursor()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            ScrollCursorItem item = child.gameObject.GetComponent<ScrollCursorItem>();
            if (item != null)
            {
                item.OnFocus(false);
            }
        }
    }

    void RefreshCursor()
    {
        this.DOKill();
        
        float ratio = 1.0f / (content.childCount - 1);        
        float mod = horizontalNormalizedPosition % ratio;
        if (mod != 0.0f)
        {
            if (mod > (ratio * 0.5f))
                this.DOHorizontalNormalizedPos(horizontalNormalizedPosition + ratio - mod, 0.5f).OnComplete(RefreshFocus);
            else
                this.DOHorizontalNormalizedPos(horizontalNormalizedPosition - mod, 0.5f).OnComplete(RefreshFocus);
        }
        else
        {
            RefreshFocus();
        }
    }

    void RefreshFocus()
    {
        int curIndex = GetFocusIndex();

        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            ScrollCursorItem item = child.gameObject.GetComponent<ScrollCursorItem>();   
            if(item != null)
            {
                item.OnFocus(i == curIndex);
            }
        }
    }

    public int GetFocusIndex()
    {
        float ratio = 1.0f / (content.childCount - 1);
        int index = (int)((horizontalNormalizedPosition / ratio) + 0.5f);
        if (index < 0)
            index = 0;
        if (index >= content.childCount)
            index = content.childCount - 1;
        return index;
    }

    public ScrollCursorItem GetFocusItem()
    {
        return content.GetChild(GetFocusIndex()).gameObject.GetComponent<ScrollCursorItem>();
    }

    public void AddItem(GameObject item)
    {
        ScrollCursorItem targetItem = item.gameObject.GetComponent<ScrollCursorItem>();
        targetItem.SetParent(this, content);
    }

    public void ClearItem()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnLeftCursor()
    {
        this.DOKill();

        float ratio = 1.0f / (content.childCount - 1);
        float goal = horizontalNormalizedPosition - ratio;
        if (goal <= 0.0f)
            goal = 0.0f;

        ClearCursor();
        this.DOHorizontalNormalizedPos(goal, 0.3f).OnComplete(RefreshCursor);
    }

    public void OnRightCursor()
    {
        this.DOKill();

        float ratio = 1.0f / (content.childCount - 1);
        float goal = horizontalNormalizedPosition + ratio;
        if (goal >= 1.0f)
            goal = 1.0f;

        ClearCursor();
        this.DOHorizontalNormalizedPos(goal, 0.3f).OnComplete(RefreshCursor);
    }
}
