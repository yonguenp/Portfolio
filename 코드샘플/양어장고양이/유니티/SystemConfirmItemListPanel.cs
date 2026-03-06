using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SystemConfirmItemListPanel : MonoBehaviour
{
    float gridSpacingY = 0;
    int catchRemoveObj = 0;
    int cellCount = 0;
    List<GameObject> clones = null;

    [Header("Text")]
    public TextLocalize title;
    public TextLocalize titleDesc;
    public GameObject titleDescBg;

    [Header("Option - Desctription Text")]
    public TextLocalize desc1;
    public TextLocalize desc2;

    [Header("Item Clone Target")]
    public GameObject targetClone;

    [Header("Item List")]
    public GridLayoutGroup itemGrid;
    public ScrollRect itemScroll;

    [Header("Buttons")]
    public Button confirmBtn;
    public Button cancelmBtn;

    public void Show(string titleKey, string titleDescKey, List<RewardData> items, UnityAction confirmCall, UnityAction cancelCall = null, string desc1Key = "", string desc2Key = "")
    {
        //초기화
        init();
        confirmBtn.onClick.AddListener(confirmCall);
        confirmBtn.onClick.AddListener(closePopup);
        if (cancelCall != null)
            cancelmBtn.onClick.AddListener(cancelCall);
        cancelmBtn.onClick.AddListener(closePopup);

        title.TextKey = titleKey;
        titleDesc.TextKey = titleDescKey;
        title.SetText();
        titleDesc.SetText();

        if (desc1Key.Length > 0)
        {
            desc1.TextKey = desc1Key;
            desc1.SetText();
            desc1.gameObject.SetActive(true);
            desc1.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }            

        if (desc2Key.Length > 0)
        {
            desc2.TextKey = desc2Key;
            desc2.SetText();
            desc2.gameObject.SetActive(true);
            desc2.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }            

        //그리드 아이템 추가
        foreach(RewardData item in items)
        {
            GameObject obj = Instantiate(targetClone, itemGrid.transform);
            obj.GetComponent<RewardInfo>().SetRewardInfoData(item);
            obj.SetActive(true);
            clones.Add(obj);
        }        

        reSizeGrid();
        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(titleDescBg.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemGrid.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScroll.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScroll.transform.parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(itemScroll.transform.parent.parent.GetComponent<RectTransform>());
    }

    void reSizeGrid()
    {
        //사이즈 재조정


        //콘텐트 높이 = 셀 높이 * row 수 + 그리드 여백y * (row 수 -1);
        //뷰 포트 높이 = 콘텐트 높이
        //뷰 포트 최상단으로 설정
        cellCount = clones.Count - catchRemoveObj;
        float cellHeight = targetClone.transform.GetComponent<RectTransform>().sizeDelta.y;
        int rowCount = cellCount > itemGrid.constraintCount? cellCount/itemGrid.constraintCount+1 : 1;
        Vector2 sizeDelta = itemScroll.content.sizeDelta;
        sizeDelta.y = cellHeight * (rowCount) + gridSpacingY * (rowCount -1);
        itemScroll.content.sizeDelta = sizeDelta;

        
        itemScroll.viewport.sizeDelta = new Vector2(itemScroll.viewport.sizeDelta.x, cellCount > itemGrid.constraintCount ? cellHeight*2 + gridSpacingY : cellHeight);

        if (cellCount >= itemGrid.constraintCount * 2)
        {
            //스크롤뷰 스크롤 활성
            itemScroll.vertical = true;
        }        

        itemScroll.verticalNormalizedPosition = 1f;
    }

    void init()
    {
        if(clones == null)
            clones = new List<GameObject>();
        catchRemoveObj = clones.Count;
        clones.ForEach((obj) => { Destroy(obj); });

        gridSpacingY = itemGrid.spacing.y;

        Vector2 sizeDelta = itemScroll.content.sizeDelta;
        sizeDelta.y = targetClone.GetComponent<RectTransform>().sizeDelta.y;
        itemScroll.content.sizeDelta = sizeDelta;

        sizeDelta = itemScroll.viewport.sizeDelta;
        sizeDelta.y = itemScroll.content.sizeDelta.y;
        itemScroll.viewport.sizeDelta = sizeDelta;

        itemScroll.horizontal = false;
        itemScroll.vertical = false;

        itemScroll.verticalNormalizedPosition = 0f;

        confirmBtn.onClick.RemoveAllListeners();
        cancelmBtn.onClick.RemoveAllListeners();


        title.TextKey = "";
        titleDesc.TextKey = "";
        desc1.TextKey = "";
        desc2.TextKey = "";

        title.enabled = true;
        titleDesc.enabled = true;
        desc1.gameObject.SetActive(false);
        desc2.gameObject.SetActive(false);

        desc1.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;
        desc2.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;
    }

    void closePopup()
    {
        clones.ForEach((obj) => { Destroy(obj); });
        gameObject.SetActive(false);
    }
}

