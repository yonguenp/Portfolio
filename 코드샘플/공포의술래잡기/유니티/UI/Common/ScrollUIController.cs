using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollUIController : MonoBehaviour
{
    [SerializeField]
    ScrollRect scroll;
    [SerializeField]
    RectTransform scrollContainer;
    [SerializeField]
    ScrollUIControllerItem scrollItem;

    public RectTransform ScrollContainer { get { return scrollContainer; } protected set { scrollContainer = value; } }
    public ScrollUIControllerItem ScrollItem { get { return scrollItem; } protected set { scrollItem = value; } }

    protected Comparison<ScrollUIControllerItem> sortFunc = null;
    
    public ScrollRect GetScroll() { return scroll; }
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);

        if (active)
        {
            OnSorting();
        }
    }

    public virtual void Clear()
    {
        if (ScrollItem != null)
            ScrollItem.SetActive(false);

        if (ScrollContainer != null)
        {
            List<Transform> dels = new List<Transform>();
            foreach (Transform child in ScrollContainer)
            {
                if (ScrollItem.transform != child)
                {
                    dels.Add(child);
                }
            }

            foreach (Transform child in dels)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    public void SetScrollList(List<GameData> list, ScrollUIControllerItem.ScrollItemSelectCallback cb = null)
    {
        Clear();

        foreach (GameData data in list)
        {
            AddItem(data, cb);
        }
    }

    public virtual ScrollUIControllerItem AddItem(GameData data, ScrollUIControllerItem.ScrollItemSelectCallback cb = null)
    {
        ScrollUIControllerItem newItem = AddItem();
        if (newItem == null)
        {
            SBDebug.LogError("뭔가잘못됨");
            return null;
        }
        newItem.SetData(data, cb);

        return newItem;
    }

    protected virtual ScrollUIControllerItem AddItem()
    {
        ScrollItem.SetActive(true);

        GameObject item = Instantiate(ScrollItem.gameObject);
        item.transform.SetParent(ScrollContainer);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        ScrollItem.SetActive(false);

        return item.GetComponent<ScrollUIControllerItem>();
    }

    public virtual void OnSorting()
    {
        if (sortFunc != null)
        {
            List<ScrollUIControllerItem> children = new List<ScrollUIControllerItem>();
            foreach (Transform child in ScrollContainer)
            {
                if (ScrollItem.transform != child)
                {
                    ScrollUIControllerItem item = child.GetComponent<ScrollUIControllerItem>();
                    if (item != null && item.gameObject.activeInHierarchy)
                    {
                        children.Add(item);
                    }
                }
            }

            try
            {
                children.Sort(sortFunc);
            }
            catch
            {
                SBDebug.Log("SortError");
            }

            int i = 0;
            foreach (ScrollUIControllerItem child in children)
            {
                child.transform.SetSiblingIndex(i++);
            }
        }

        scroll.normalizedPosition = ScrollContainer.pivot;
    }

    public void OnSorting(Comparison<ScrollUIControllerItem> func)
    {
        sortFunc = func;

        OnSorting();
    }
}
