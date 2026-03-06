using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NecoNewCatIconAlarmPopup : MonoBehaviour
{
    [Serializable]
    public class NewCatObject
    {
        public GameObject catObject;
        public GameObject newIconObject;
    }

    enum PASS_STATE
    {
        HERE,
        LEFT,
        RIGHT,
    }

    public NewCatObject[] Cats = new NewCatObject[13];

    [Header("[Icon List Bar]")]
    public GameObject catListLayer;
    public RectTransform leftLayoutObject;
    public RectTransform rightLayoutObject;

    neco_map curMapData;

    private void Awake()
    {
        InitButtonState();
    }

    public void OnClickNewCatAlarm(uint catID)
    {
        if (neco_data.PrologueSeq.스와이프가이드 == neco_data.GetPrologueSeq())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_402"));
            return;
        }

        uint firstAppearMapID = neco_data.Instance.FirstAppearCatMapID(catID);

        if (NecoCanvas.GetGameCanvas().curMapID != firstAppearMapID)
        {
            NecoCanvas.GetGameCanvas().LoadMap(firstAppearMapID);
        }
    }

    public void RefreshNewCatAlarm()
    {
        ResetIcon();

        MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (mapController != null)
        {
            curMapData = mapController.GetMapData();
        }

        List<uint> readyCatList = neco_data.Instance.GetReadyCatList();

        foreach (uint catID in readyCatList)
        {
            neco_cat catData = neco_cat.GetNecoCat(catID);
            if (catData != null && catData.IsGainCat())
            {
                continue;
            }

            uint firstAppearMapID = neco_data.Instance.FirstAppearCatMapID(catID);

            // catID 조회하여 해당 고양이 첫 등장 맵 조회
            SetNewCatIcon(catID, CalculateShortPass(firstAppearMapID));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(leftLayoutObject);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rightLayoutObject);
    }

    void SetNewCatIcon(uint catID, PASS_STATE state)
    {
        switch (state)
        {
            case PASS_STATE.HERE:
                Cats[catID].catObject.SetActive(false);
                break;
            case PASS_STATE.LEFT:
                Cats[catID].catObject.transform.SetParent(leftLayoutObject);
                Cats[catID].catObject.transform.localScale = new Vector3(-1, 1, 1);
                Cats[catID].catObject.SetActive(true);
                Cats[catID].newIconObject.transform.localRotation = Quaternion.Euler(0, 0, 40.0f);
                Cats[catID].newIconObject.transform.localScale = new Vector3(-1, 1, 1);

                Cats[catID].catObject.transform.DOKill();
                Cats[catID].catObject.transform.DOLocalMoveX(10, 0.5f).SetRelative().SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
                break;
            case PASS_STATE.RIGHT:
                Cats[catID].catObject.transform.SetParent(rightLayoutObject);
                Cats[catID].catObject.transform.localScale = new Vector3(1, 1, 1);
                Cats[catID].catObject.SetActive(true);
                Cats[catID].newIconObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                Cats[catID].newIconObject.transform.localScale = new Vector3(1, 1, 1);

                Cats[catID].catObject.transform.DOKill();
                Cats[catID].catObject.transform.DOLocalMoveX(-10, 0.5f).SetRelative().SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    PASS_STATE CalculateShortPass(uint catApearMapID)
    {
        int rightPassCount = 0;
        int lefttPassCount = 0;

        //GameObject[] list = NecoCanvas.GetGameCanvas().MapList;

        List<uint> mapList = new List<uint>();
        mapList.Add(8);
        mapList.Add(9);
        mapList.Add(6);
        mapList.Add(10);
        mapList.Add(1);
        mapList.Add(2);
        mapList.Add(3);
        mapList.Add(4);
        mapList.Add(5);
        mapList.Add(7);

        int currentIndex = mapList.FindIndex(x => x == curMapData.GetMapID());
        int targetIndex = mapList.FindIndex(x => x == catApearMapID);

        if (targetIndex > currentIndex)
        {
            rightPassCount = targetIndex - currentIndex;
            lefttPassCount = mapList.Count - rightPassCount;
        }
        else if (targetIndex < currentIndex)
        {
            lefttPassCount = currentIndex - targetIndex;
            rightPassCount = mapList.Count - lefttPassCount;
        }
        else
        {
            return PASS_STATE.HERE;
        }

        return lefttPassCount >= rightPassCount ? PASS_STATE.RIGHT : PASS_STATE.LEFT;
    }

    void InitButtonState()
    {
        for (uint i = 0; i < Cats.Length; ++i)
        {
            if (Cats[i].catObject == null) { continue; }

            uint tempIndex = i; // Closure Problem   
            Cats[tempIndex].catObject.GetComponentInChildren<Button>().onClick.AddListener(() => OnClickNewCatAlarm(tempIndex));
        }
    }

    void ResetIcon()
    {
        foreach (NewCatObject icon in Cats)
        {
            if (icon.catObject == null || icon.newIconObject == null) { continue; }

            icon.catObject.transform.SetParent(catListLayer.transform);
            icon.catObject.SetActive(false);

            icon.catObject.transform.DORewind();
            icon.catObject.transform.DOKill();

            //icon.newIconObject.transform.DORewind();
            //icon.newIconObject.transform.DOKill();
        }
    }
}
