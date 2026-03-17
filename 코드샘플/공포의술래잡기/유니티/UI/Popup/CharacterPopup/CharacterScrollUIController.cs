using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScrollUIController : ScrollUIController
{

    [SerializeField]
    RectTransform notmineScrollContainer;
    [SerializeField]
    ScrollUIControllerItem notmineScrollItem;
    [SerializeField]
    RectTransform lineTransform;

    [SerializeField]
    CharacterFilterUI filterUI;
    public override void Clear()
    {
        base.Clear();

        if (notmineScrollItem != null)
            notmineScrollItem.SetActive(false);

        if (notmineScrollContainer != null)
        {
            List<Transform> dels = new List<Transform>();
            foreach (Transform child in notmineScrollContainer)
            {
                if (notmineScrollItem.transform != child)
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

    public ScrollUIControllerItem AddItemNotMine(GameData data, ScrollUIControllerItem.ScrollItemSelectCallback cb = null)
    {
        CharacterListItem newItem = AddItemNotMine();
        if (newItem == null)
        {
            SBDebug.LogError("뭔가잘못됨");
            return null;
        }
        newItem.SetDataForNotMine(data, cb);

        return newItem;
    }

    CharacterListItem AddItemNotMine()
    {
        notmineScrollItem.SetActive(true);

        GameObject item = Instantiate(notmineScrollItem.gameObject);
        item.transform.SetParent(notmineScrollContainer);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;

        notmineScrollItem.SetActive(false);

        return item.GetComponent<CharacterListItem>();
    }

    public override void OnSorting()
    {
        List<ScrollUIControllerItem> children = new List<ScrollUIControllerItem>();
        foreach (Transform child in notmineScrollContainer)
        {
            if (notmineScrollItem.transform != child)
            {
                ScrollUIControllerItem item = child.GetComponent<ScrollUIControllerItem>();
                if (item != null && item.gameObject.activeInHierarchy)
                {
                    children.Add(item);
                }
            }
        }

        if (children.Count > 0)
        {
            try
            {
                if (filterUI.desc)
                    children.Sort(CharacterFilterUI.SortGradeNotMineDesc);
                else
                    children.Sort(CharacterFilterUI.SortGradeNotMineAsc);
            }
            catch
            {
                SBDebug.Log("SortError");
            }
        }

        int i = 0;
        foreach (ScrollUIControllerItem child in children)
        {
            child.transform.SetSiblingIndex(i++);
        }

        base.OnSorting();

        lineTransform.gameObject.SetActive(children.Count > 0);
    }

    private void Update()
    {
        
        Vector2 size = GetScroll().content.sizeDelta;
        size.y = ScrollContainer.sizeDelta.y;
        
        if (lineTransform.gameObject.activeInHierarchy)
        {
            size.y += lineTransform.sizeDelta.y + notmineScrollContainer.sizeDelta.y;
            Vector3 linePos = lineTransform.localPosition;
            linePos.y = ScrollContainer.sizeDelta.y * -1.0f;
            lineTransform.localPosition = linePos;

            Vector3 notminePos = notmineScrollContainer.localPosition;
            notminePos.y = linePos.y + (lineTransform.sizeDelta.y * -1.0f);
            notmineScrollContainer.localPosition = notminePos;
        }

        GetScroll().content.sizeDelta = size;
    }

    
}
