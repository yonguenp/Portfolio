using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendFilter : MonoBehaviour
{
    public enum FILTER_TYPE
    {
        RANK = 1,
        GAIN_DATE = 2,
        NAME = 3
    };

    [SerializeField]
    GameObject filterListUI;
    [SerializeField]
    ScrollUIController targetScroll;

    [SerializeField]
    Text txtFilter = null;

    [SerializeField]
    GameObject downAlignObject = null;
      [SerializeField]
    GameObject upAlignObject = null;
    

    bool isFilterRevers = false;
    FILTER_TYPE filterType = FILTER_TYPE.RANK;

    public void OnAlignButton()
    {
        isFilterRevers = !isFilterRevers;
        RefreshAlignButton();
        OnFiltering(filterType);
    }

    public void RefreshAlignButton()
    {
        downAlignObject.SetActive(!isFilterRevers);
        upAlignObject.SetActive(isFilterRevers);
    }

    public void OnFilterButton()
    {
        filterListUI.SetActive(!filterListUI.activeSelf);
    }

    private void OnEnable()
    {
        filterListUI.SetActive(false);
        OnFiltering(filterType);
    }

    private void OnDisable()
    {
        filterListUI.SetActive(false);
    }

    public void OnFiltering(int type)
    {
        OnFiltering((FILTER_TYPE)type);
        filterListUI.SetActive(false);
    }

    public void OnFiltering(FILTER_TYPE type)
    {
        filterType = type;
        switch (filterType)
        {
            case FILTER_TYPE.GAIN_DATE:
                if(isFilterRevers) targetScroll.OnSorting(SortGainDateRevers);
                else targetScroll.OnSorting(SortGainDate);
                txtFilter.text = StringManager.GetString("ui_sort_time");
                break;
            case FILTER_TYPE.RANK:
                if(isFilterRevers) targetScroll.OnSorting(SortRankRevers);
                else targetScroll.OnSorting(SortRank);
                txtFilter.text = StringManager.GetString("ui_sort_rank");
                break;
            case FILTER_TYPE.NAME:
                if(isFilterRevers) targetScroll.OnSorting(SortNameRevers);
                else targetScroll.OnSorting(SortName);
                txtFilter.text = StringManager.GetString("ui_sort_name");
                break;
        }
    }

    public static int SortGainDate(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        var yc = (y as FriendItem).FriendProfile();
        var xc = (x as FriendItem).FriendProfile();

        return xc.lastLogin > yc.lastLogin ? -1 : 1;
    }

    public static int SortGainDateRevers(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        var yc = (y as FriendItem).FriendProfile();
        var xc = (x as FriendItem).FriendProfile();

        return xc.lastLogin < yc.lastLogin ? -1 : 1;
    }

     public static int SortRank(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        var yc = (y as FriendItem).FriendProfile();
        var xc = (x as FriendItem).FriendProfile();

        return xc.point > yc.point ? -1 : 1;
    }

    public static int SortRankRevers(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        var yc = (y as FriendItem).FriendProfile();
        var xc = (x as FriendItem).FriendProfile();

        return xc.point < yc.point ? -1 : 1;
    }

    public static int SortName(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        var yc = (y as FriendItem).FriendProfile();
        var xc = (x as FriendItem).FriendProfile();
        return  xc.nick.CompareTo(yc.nick);
    }

    public static int SortNameRevers(ScrollUIControllerItem x, ScrollUIControllerItem y)
    {
        var yc = (y as FriendItem).FriendProfile();
        var xc = (x as FriendItem).FriendProfile();
        return  yc.nick.CompareTo(xc.nick);
    }
}
