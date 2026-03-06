using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NecoCatListPanel : NecoAnimatePopup
{
    [Header("[Cat Info List]")]
    public GameObject catInfoScrollContainer;
    public GameObject catInfoCloneObject;

    [Header("[Cat Detail Info]")]
    public NecoCatDetailPanel catDetailPanel;

    neco_cat curSelectedCatData = null;
    List<neco_cat> catInfoDataList = new List<neco_cat>();

    public override void OnAnimateDone()
    {

    }
    protected override void OnEnable()
    {
        base.OnEnable();

        curSelectedCatData = null;

        SetCatInfoListUI();

        catInfoScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;        
    }

    public void OnClickCatListButton()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);
    }

    public void OnClickCloseButton()
    {
        // 고양이 첫등장 프롤로그 예외처리
        if (neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.길막이획득프로필팝업확인)
        {
            return;
        }

        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_LIST_POPUP);
    }

    public void SetSelectedCatInfo(neco_cat catData)
    {
        curSelectedCatData = catData;

        //SetCatInfoListUI();

        SetSelectedCatInfoUI(curSelectedCatData);
    }

    public neco_cat GetSelectedCatInfo()
    {
        return curSelectedCatData;
    }

    public List<neco_cat> GetCatInfoDataList()
    {
        return catInfoDataList;
    }

    void SetCatInfoListUI()
    {
        foreach (Transform child in catInfoScrollContainer.transform)
        {
            if (child.gameObject != catInfoCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        catInfoCloneObject.SetActive(true);

        catInfoDataList.Clear();

        List<game_data> catGameDataList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_CAT);
        List<neco_cat> catList = catGameDataList.Cast<neco_cat>().ToList();

        // 고양이 리스트 정렬
        catList = catList.OrderBy(x => x.IsGainCat() == false).ThenBy(x => x.GetCatID()).ToList();
 
        List<CatInfo> catInfoList = new List<CatInfo>();
        if (catList != null)
        {
            int showCount = 0;
            foreach (neco_cat catInfoData in catList)
            {
                if (catInfoData.GetAbleMapList().Count == 0)
                    continue;

                GameObject catInfoUI = Instantiate(catInfoCloneObject);
                catInfoUI.transform.SetParent(catInfoScrollContainer.transform);
                catInfoUI.transform.localScale = catInfoCloneObject.transform.localScale;
                catInfoUI.transform.localPosition = catInfoCloneObject.transform.localPosition;

                CatInfo catInfoComponent = catInfoUI.GetComponent<CatInfo>();
                catInfoComponent.SetCatInfoData(catInfoData, this);
                catInfoList.Add(catInfoComponent);
                catInfoDataList.Add(catInfoData);
                showCount++;
            }

            int lineEnd = showCount % 3;

            for (int i = 0; i < 3 - lineEnd; i++)
            {
                GameObject catInfoUI = Instantiate(catInfoCloneObject);
                catInfoUI.transform.SetParent(catInfoScrollContainer.transform);
                catInfoUI.transform.localScale = catInfoCloneObject.transform.localScale;
                catInfoUI.transform.localPosition = catInfoCloneObject.transform.localPosition;
                
                CatInfo catInfoComponent = catInfoUI.GetComponent<CatInfo>();
                catInfoComponent.SetNotReleasedCat();
            }
        }

        catInfoCloneObject.SetActive(false);

        
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이획득프로필팝업확인)
        {
            foreach(CatInfo info in catInfoList)
            {
                if (info.GetComponent<Button>() != null)
                    info.GetComponent<Button>().interactable = false;
            }
            
            catInfoList[0].Invoke("OnAnimateCatInfo", 1.0f);
        }
    }

    void SetSelectedCatInfoUI(neco_cat selectedCatData)
    {
        if (catDetailPanel != null)
        {
            catDetailPanel.SetCatDetailInfo(selectedCatData, catInfoDataList);
        }
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.조리대UI등장:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_245"));
                return true;
            case neco_data.PrologueSeq.철판제작가이드퀘스트:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ13"));
                return true;
            case neco_data.PrologueSeq.조리대레벨업:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ17"));
                return true;
            case neco_data.PrologueSeq.배스구이강조:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ23"));
                return true;
            case neco_data.PrologueSeq.낚시장난감만들기:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("GQ27"));
                return true;
            case neco_data.PrologueSeq.낚시장난감오브젝트레벨업가이드:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_401"));
                return true;
            case neco_data.PrologueSeq.배틀패스강조및대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
                return true;
        }

        return false;
    }
}
