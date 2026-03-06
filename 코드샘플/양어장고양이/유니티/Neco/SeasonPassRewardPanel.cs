using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SeasonPassRewardPanel : MonoBehaviour
{
    [Header("[Season Reward Info List]")]
    public GameObject seaonRewardListScrollContainer;
    public GameObject normalRewardCloneObject;
    public GameObject specialRewardCloneObject;

    [Header("[Season Step List]")]
    public GameObject step2NotBuyObject;
    public GameObject step2BuyObject;

    public GameObject step3NotBuyObject;
    public GameObject step3BuyObject;

    public GameObject LoadingObject;
    //[Header("[Season Pass Dim]")]
    //public GameObject pass2StepDim;
    //public GameObject pass3StepDim;

    SeasonPassPanel rootParentPanel;

    neco_pass curPassData;

    List<PassRewardInfo> receivePassList = new List<PassRewardInfo>(); // 보상받기 가능한 리스트 목록

    bool isPurchasing = false;
    public void OnClickNextSeasonPassButton()
    {
        if (neco_data.PrologueSeq.배틀패스강조및대사 == neco_data.GetPrologueSeq())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
            return;
        }

        DateTime curTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(NecoCanvas.GetCurTime());
        DateTime endTime = curPassData.GetEndDate();

        if (curTime <= endTime && !isPurchasing)
        {
            ConfirmPopupData param = new ConfirmPopupData();

            param.titleText = LocalizeData.GetText("LOCALIZE_353");
            param.titleMessageText = LocalizeData.GetText("LOCALIZE_325") + (neco_data.Instance.GetPassData().GetCurPassStep() + 1).ToString() + LocalizeData.GetText("LOCALIZE_326");

            param.messageText_1 = LocalizeData.GetText("LOCALIZE_327");

            neco_shop curShopData = null;
            if (neco_data.Instance.GetPassData().GetCurPassStep() == 1)
            {
                curShopData = neco_shop.GetNecoShopData(22);
            }
            else if (neco_data.Instance.GetPassData().GetCurPassStep() == 2)
            {
                curShopData = neco_shop.GetNecoShopData(23);
            }
            param.amountText = string.Format("\\ {0}", curShopData?.GetNecoShopPrice().ToString("n0"));

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON_WITHDRAWAL, () =>
            {
                uint pid = 0;
                string packName = "";
                switch (neco_data.Instance.GetPassData().GetCurPassStep())
                {
                    case 1:
                        pid = 22;
                        packName = "hahaha_pass_1";
                        break;
                    case 2:
                        pid = 23;
                        packName = "hahaha_pass_2";
                        break;
                    case 3:
                    default:
                        return;
                }

                IAPManager.GetInstance().TryPurchase(pid, packName, (responseArr) =>
                {
                    isPurchasing = false;
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_328"), LocalizeData.GetText("LOCALIZE_329"), () => {
                        rootParentPanel.RefreshLayer();
                        rootParentPanel.Invoke("UpdateRedDotState", 0.1f);
                        
                    });
                }, (responseArr) =>
                {
                    isPurchasing = false;
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), LocalizeData.GetText("LOCALIZE_344"));
                });
            });
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_354"));
        }
    }

    public void InitSeasonPassRewardUI(SeasonPassPanel parentPanel)
    {
        rootParentPanel = parentPanel;

        bool reciveAllEnable = false;
        uint maxStatus = 0;
        switch(neco_data.Instance.GetPassData().GetCurPassStep())
        {
            case 1: 
                maxStatus = 1;
                //pass2StepDim.SetActive(true);
                //pass3StepDim.SetActive(true);

                step2NotBuyObject.SetActive(true);
                step2BuyObject.SetActive(false);

                step3NotBuyObject.SetActive(true);
                step3BuyObject.SetActive(false);
                break;
            case 2: 
                maxStatus = 3;
                //pass2StepDim.SetActive(false);
                //pass3StepDim.SetActive(true);

                step2NotBuyObject.SetActive(false);
                step2BuyObject.SetActive(true);

                step3NotBuyObject.SetActive(true);
                step3BuyObject.SetActive(false);
                break;
            case 3: 
                maxStatus = 7;
                //pass2StepDim.SetActive(false);
                //pass3StepDim.SetActive(false);

                step2NotBuyObject.SetActive(false);
                step2BuyObject.SetActive(true);

                step3NotBuyObject.SetActive(false);
                step3BuyObject.SetActive(true);
                break;
        }

        curPassData = neco_pass.GetNecoPassData(neco_data.Instance.GetPassData().GetCurMissionID());
        List<neco_pass_reward_forever> passRewardList = neco_pass_reward_forever.GetNecoPassRewardList();
        if (passRewardList != null)
        {
            foreach (neco_pass_reward_forever mission in passRewardList)
            {
                uint lev = mission.GetNecoPassRewardLevel();
                if (neco_data.Instance.GetPassData().GetCurLevel() >= lev)
                {
                    string findRewardType = string.Empty;
                    findRewardType = string.Format("reward_type", neco_data.Instance.GetPassData().GetCurPassStep());
                    if (mission.GetRewardGradeTypeInfo(findRewardType) == "memory")
                    {
                        continue;
                    }

                    uint status = neco_data.Instance.GetPassData().GetRewardStatus(lev);
                    if (maxStatus > status)
                    {
                        reciveAllEnable = true;
                        break;
                    }
                }
            }
        }
        rootParentPanel.SetActiveReciveAllButton(reciveAllEnable);

        ClearData();

        StopAllCoroutines();
        StartCoroutine(SetSeasonRewardList(passRewardList));

        seaonRewardListScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        //neco_data.Instance.GetPassData().SetPassAlarm(false);
    }

    public void DoReceiveAllReward()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "mission");
        data.AddField("op", 2);

        NetworkManager.GetInstance().SendApiRequest("mission", 2, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;

            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배틀패스강조및대사)
            {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                    mapController.SendMessage("배틀패스보상획득", SendMessageOptions.DontRequireReceiver);
            }

            NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "mission", apiArr, () => {

                foreach (PassRewardInfo info in receivePassList)
                {
                    if (info.CheckIsGetReward())
                    {
                        info.PlayRecieveTween();
                        PassListInfo passInfo = info.GetRootInfo();
                        if (passInfo != null)
                        {
                            for (int i = 1; i <= passInfo.passRewards.Length; i++)
                            {
                                if (passInfo.passRewards[i - 1] != null && passInfo.passRewards[i - 1].GetComponent<PassRewardInfo>() == info)
                                {
                                    passInfo.OnRecivedPassReward(i);
                                }
                            }
                        }
                    }
                }

                //neco_data.Instance.GetPassData().SetPassAlarm(false);
                rootParentPanel.UpdateRedDotState();
                rootParentPanel.SetActiveReciveAllButton(neco_data.Instance.GetPassData().IsPassAlarm());

                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배틀패스보상획득)
                {
                    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                    if (mapController != null)
                        mapController.SendMessage("배틀패스완료", SendMessageOptions.DontRequireReceiver);

                }
                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배틀패스보상받기)
                {
                    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                    if (mapController != null)
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.배틀패스보상받기, SendMessageOptions.DontRequireReceiver);
                }
            });
        });
    }

    IEnumerator SetSeasonRewardList(List<neco_pass_reward_forever> passRewardList)
    {
        LoadingObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);

        if (seaonRewardListScrollContainer != null && normalRewardCloneObject != null && specialRewardCloneObject != null)
        {
            foreach (Transform child in seaonRewardListScrollContainer.transform)
            {
                if (child.gameObject == normalRewardCloneObject || child.gameObject == specialRewardCloneObject)
                {
                    continue;
                }

                Destroy(child.gameObject);
            }


            normalRewardCloneObject.SetActive(false);
            specialRewardCloneObject.SetActive(false);

            receivePassList.Clear();

            // 데이터 리스트 정렬
            passRewardList.OrderBy(x => x.GetNecoPassRewardLevel());

            neco_pass_data curNecoPassData = neco_data.Instance.GetPassData();
            seaonRewardListScrollContainer.SetActive(false);

            int WaitLoadDisplayCount = (int)curNecoPassData.GetCurLevel() + 1;
            if (WaitLoadDisplayCount < 5)
                WaitLoadDisplayCount = 5;

            if (passRewardList != null)
            {
                Transform scrollviewTransform = transform.Find("Scroll View");
                ScrollRect scroll = scrollviewTransform.GetComponent<ScrollRect>();

                foreach (neco_pass_reward_forever rewardData in passRewardList)
                {
                    // todo bt - 아래 방식이 아닌 추후 데이터로 제어하는 것을 고려해야함
                    // 스페셜 보상 체크
                    bool isSpecial = rewardData.GetNecoPassRewardLevel() % 5 == 0 ? true : false;

                    GameObject cloneObject = isSpecial ? specialRewardCloneObject : normalRewardCloneObject;

                    GameObject rewardInfoUI = Instantiate(cloneObject);
                    rewardInfoUI.transform.SetParent(seaonRewardListScrollContainer.transform);
                    rewardInfoUI.transform.localScale = cloneObject.transform.localScale;
                    rewardInfoUI.transform.localPosition = cloneObject.transform.localPosition;
                    rewardInfoUI.SetActive(true);

                    PassListInfo passInfoComponent = rewardInfoUI.GetComponent<PassListInfo>();

                    //float delay = Mathf.Abs((int)curNecoPassData.GetCurLevel() - (int)rewardData.GetNecoPassRewardLevel()) * 0.1f;
                    passInfoComponent.InitPassInfoData(rewardData, rootParentPanel, isSpecial);
                    
                    if (curNecoPassData.GetCurLevel() >= rewardData.GetNecoPassRewardLevel())
                    {
                        receivePassList.AddRange(passInfoComponent.GetAvailRecieveList());
                        if(curNecoPassData.GetCurLevel() == rewardData.GetNecoPassRewardLevel())
                        {
                            seaonRewardListScrollContainer.SetActive(true);
                        }
                    }

                    if (WaitLoadDisplayCount > 0)
                    {
                        WaitLoadDisplayCount--;
                        scroll.DOVerticalNormalizedPos(0.0f, 0.01f);
                    }
                    else
                    {
                        LoadingObject.SetActive(false);
                    }

                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        LoadingObject.SetActive(false);
    }

    public void RefreshLayerForPassTicket()
    {
        receivePassList.Clear();

        foreach (Transform child in seaonRewardListScrollContainer.transform)
        {
            PassListInfo info = child.GetComponent<PassListInfo>();
            if (info != null)
            {
                info.SetPassLineCircleInfoData();

                neco_pass_data curNecoPassData = info.GetCurPassData();
                neco_pass_reward_forever rewardData = info.GetCurRewardData();
                
                if (curNecoPassData != null)
                {
                    if (curNecoPassData.GetCurLevel() >= rewardData.GetNecoPassRewardLevel())
                    {
                        receivePassList.AddRange(info.GetAvailRecieveList());
                        if (curNecoPassData.GetCurLevel() == rewardData.GetNecoPassRewardLevel())
                        {
                            seaonRewardListScrollContainer.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    void DoRecieveAllTween()
    {
        if (receivePassList != null && receivePassList.Count > 0)
        {
            foreach (var info in receivePassList)
            {
                info.PlayRecieveAllTween();
            }

            rootParentPanel.Invoke("RefreshLayer", 1.0f);
        }
    }

    public void RemoveReceiveList(PassRewardInfo removeInfo)
    {
        if (receivePassList != null && receivePassList.Count > 0)
        {
            receivePassList.Remove(removeInfo);
        }
    }

    void ClearData()
    {

    }

}
