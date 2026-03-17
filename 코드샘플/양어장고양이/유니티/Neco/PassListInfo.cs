using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassListInfo : MonoBehaviour
{
    const int PASS_REWARD_COUNT = 3;

    [Header("[Pass Info Layer]")]
    public GameObject[] passRewards;
    public GameObject selectObject;
    public Image backgroundImage;
    public Color originColor;
    public Color dimmedColor;

    [Header("[Level Circle Layer]")]
    public GameObject basicCircleObject;
    public Text basicCircleLevelText;
    
    public GameObject lineCircleObject;
    public Text lineCircleLevelText;
    public GameObject lineFullCircleObject;
    public Image lineImage;

    neco_pass_data curNecoPassData;
    neco_pass_reward_forever curPassRewardData;
    SeasonPassPanel rootParentPanel;

    bool isSpecialGift;

    public void OnClickGetButton(int step)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "mission");
        data.AddField("op", 2);
        data.AddField("level", curPassRewardData.GetNecoPassRewardLevel().ToString());
        data.AddField("step", step);

        NetworkManager.GetInstance().SendApiRequest("mission", 2, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배틀패스강조및대사)
            {
                MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                if (mapController != null)
                    mapController.SendMessage("배틀패스보상획득", SendMessageOptions.DontRequireReceiver);
            }

            JArray apiArr = (JArray)apiToken;

            NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "mission", apiArr, () =>
            {
                OnRecivedPassReward(step);
                rootParentPanel.UpdateRedDotState();

                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배틀패스보상획득)
                {
                    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                    if (mapController != null)
                        mapController.SendMessage("배틀패스완료", SendMessageOptions.DontRequireReceiver);

                }
                if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배틀패스보상받기 && curPassRewardData.GetNecoPassRewardLevel() == 2)
                {
                    MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
                    if (mapController != null)
                        mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.배틀패스보상받기, SendMessageOptions.DontRequireReceiver);
                }
            });
        });
    }

    public void OnClickUsePassTicketButton()
    {
        if (neco_data.PrologueSeq.배틀패스강조및대사 == neco_data.GetPrologueSeq())
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("패틀패스강조"));
            return;
        }

        if (user_items.GetUserItemAmount(135) > 0)
        {
            ConfirmPopupData param = new ConfirmPopupData();

            param.titleText = LocalizeData.GetText("LOCALIZE_318");
            param.titleMessageText = LocalizeData.GetText("LOCALIZE_319");
            param.messageText_1 = LocalizeData.GetText("LOCALIZE_320");

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, ()=> {
                WWWForm data = new WWWForm();
                data.AddField("api", "mission");
                data.AddField("op", 3);

                NetworkManager.GetInstance().SendApiRequest("mission", 3, data, (response) =>
                {
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_319"), LocalizeData.GetText("LOCALIZE_321"), ()=> {
                        ClearUI();
                        InitUI();
                        rootParentPanel.RefreshLayerForPassTicket();

                        NecoCanvas.GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().resourceInfoPanel.RefreshResourceData();
                    });
                });
            });
        }
        else
        {
            ConfirmPopupData popupData = new ConfirmPopupData();

            popupData.titleText = LocalizeData.GetText("LOCALIZE_183");
            popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_322");

            popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_513");

            NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, () => {
                NecoCanvas.GetPopupCanvas().OnPopupClose();
                NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CAT_LEAF);
            });
        }
    }

    public void OnRecivedPassReward(int step)
    {
        Invoke("SetPassInfoData", 0.1f);
        if (step > 0 && step <= PASS_REWARD_COUNT)
        {
            passRewards[step - 1]?.GetComponent<PassRewardInfo>().PlayRecieveTween();
        }
    }

    void UpdateReceiveAllState()
    {
        bool reciveAllEnable = false;
        uint maxStatus = 0;
        switch (neco_data.Instance.GetPassData().GetCurPassStep())
        {
            case 1:
                maxStatus = 1;
                break;
            case 2:
                maxStatus = 3;
                break;
            case 3:
                maxStatus = 7;
                break;
        }

        neco_pass curPassData = neco_pass.GetNecoPassData(neco_data.Instance.GetPassData().GetCurMissionID());
        List<neco_pass_reward_forever> passRewardList = neco_pass_reward_forever.GetNecoPassRewardList();
        if (passRewardList != null)
        {
            foreach (neco_pass_reward_forever mission in passRewardList)
            {
                string findRewardType = string.Empty;
                findRewardType = string.Format("reward_grade", neco_data.Instance.GetPassData().GetCurPassStep());
                if (mission.GetRewardGradeTypeInfo(findRewardType) == "memory")
                {
                    continue;
                }

                uint lev = mission.GetNecoPassRewardLevel();
                if (neco_data.Instance.GetPassData().GetCurLevel() >= lev)
                {
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
        //neco_data.Instance.GetPassData().SetPassAlarm(reciveAllEnable);
        //rootParentPanel.UpdateRedDotState();
    }

    public void OnVIP_LevelUP()
    {
        rootParentPanel.RefreshSeasonPass();
    }

    public void RefreshRootPanel()
    {
        rootParentPanel.RefreshLayer();
    }

    public void RemoveAvailRecieveData(PassRewardInfo removeInfo)
    {
        rootParentPanel.seasonRewardPanel.RemoveReceiveList(removeInfo);
    }

    public void InitPassInfoData(neco_pass_reward_forever passReward, SeasonPassPanel parentPanel, bool isSpecial)
    {
        if (passReward == null) { return; }
        
        curPassRewardData = passReward;
        rootParentPanel = parentPanel;
        isSpecialGift = isSpecial;

        curNecoPassData = neco_data.Instance.GetPassData();

        ClearUI();
        InitUI();
    }

    void ClearUI()
    {
        selectObject.SetActive(false);
        foreach(GameObject pr in passRewards)
        {
            pr.SetActive(false);
        }

        basicCircleObject.SetActive(false);
        lineCircleObject.SetActive(false);
    }

    void InitUI()
    {
        SetPassInfoData();
        SetPassLineCircleInfoData();
        SetUIData();
    }

    public List<PassRewardInfo> GetAvailRecieveList()
    {
        List<PassRewardInfo> rewardList = new List<PassRewardInfo>();
        for (uint i = 1; i <= passRewards.Length; i++)
        {
            if (passRewards[i - 1] == null) continue;
            if (!curPassRewardData.IsRecivedReward(i))
            {   
                rewardList.Add(passRewards[i - 1].GetComponent<PassRewardInfo>());
            }
        }
        
        return rewardList;
    }

    void SetPassInfoData()
    {
        List<KeyValuePair<string, KeyValuePair<uint, uint>>> rewardPairList = curPassRewardData.GetRewardGradeList();

        if (passRewards == null || passRewards.Length <= 0) { return; }
        if (rewardPairList == null || rewardPairList.Count <= 0) { return; }
        //if (passRewards.Length != rewardPairList.Count) { return; }

        uint passStep = 1;
        uint[] photos = neco_pass_forever.GetSeasonPhotos((int)curNecoPassData.GetCurMissionID());

        for (int i = 0; i < passRewards.Length; ++i)
        {
            KeyValuePair<string, KeyValuePair<uint, uint>> rewardPair = rewardPairList[i];

            string rewardType = rewardPair.Key;
            uint rewardID = rewardPair.Value.Key;
            uint rewardCount = rewardPair.Value.Value;
            if(isSpecialGift)
                rewardID = photos[(int)curPassRewardData.GetNecoPassRewardLevel() / 5 - 1];

            if (passRewards[i] != null)
            {
                passRewards[i].SetActive(true);

                PassRewardInfo rewardInfoComponent = passRewards[i].GetComponent<PassRewardInfo>();
                rewardInfoComponent.InitPassRewardInfo(passStep++, curPassRewardData, rewardType, rewardID, rewardCount, this);
            }
        }

        UpdateReceiveAllState();
    }

    public void SetPassLineCircleInfoData()
    {
        if (curPassRewardData == null)
            return;

        uint curLevel = curPassRewardData.GetNecoPassRewardLevel();

        // 1레벨 패스는 따로 처리
        if (curLevel == 1)
        {
            basicCircleObject.SetActive(true);
            lineCircleObject.SetActive(false);

            basicCircleLevelText.text = curLevel.ToString();
        }
        else
        {
            basicCircleObject.SetActive(false);
            lineCircleObject.SetActive(true);
            lineImage.fillAmount = 0;

            uint curPassLevel = curNecoPassData.GetCurLevel();
            uint curPassExp = curNecoPassData.GetTotalExp();

            lineCircleLevelText.text = curLevel.ToString();
            Transform PassBuy = lineCircleObject.transform.Find("PassBuy");
            PassBuy?.gameObject.SetActive(false);

            if (curPassLevel >= curLevel)
            {
                lineFullCircleObject.SetActive(true);
                lineImage.fillAmount = 1.0f;
            }
            else if (curPassLevel + 1 == curLevel)
            {
                neco_pass_reward_forever prevRewardData = neco_pass_reward_forever.GetNecoPassReward(curPassLevel);
                curPassExp -= prevRewardData.GetNecoPassRewardExp();

                uint totalAmount = curPassRewardData.GetNecoPassRewardExp() - prevRewardData.GetNecoPassRewardExp();

                lineImage.fillAmount = (float)curPassExp / totalAmount;

                if (PassBuy != null)
                {
                    PassBuy.gameObject.SetActive(true);
                    Transform icon = PassBuy.Find("Icon");
                    Transform text = PassBuy.Find("Text");
                    if (icon.GetComponent<Button>() == null)
                    {
                        Button button = icon.gameObject.AddComponent<Button>();
                        button.onClick.AddListener(OnClickUsePassTicketButton);
                    }

                    if (user_items.GetUserItemAmount(135) <= 0)
                    {
                        Coffee.UIEffects.UIEffect effect = icon.GetComponent<Coffee.UIEffects.UIEffect>();
                        if (effect == null)
                            effect = icon.gameObject.AddComponent<Coffee.UIEffects.UIEffect>();

                        effect.effectMode = Coffee.UIEffects.EffectMode.Grayscale;
                        effect.effectFactor = 1.0f;
                        effect.colorMode = Coffee.UIEffects.ColorMode.Multiply;
                        effect.colorFactor = 1.0f;
                        effect.blurMode = Coffee.UIEffects.BlurMode.None;

                        Outline outline = text.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.effectColor = Color.gray;
                        }

                        text.GetComponent<Text>().text = LocalizeData.GetText("LOCALIZE_322");
                        text.GetComponent<Text>().resizeTextForBestFit = true;
                    }
                    else
                    {
                        Coffee.UIEffects.UIEffect effect = icon.GetComponent<Coffee.UIEffects.UIEffect>();
                        if (effect != null)
                            Destroy(effect);

                        Outline outline = text.GetComponent<Outline>();
                        if (outline != null)
                        {
                            outline.effectColor = new Color(0.6039216f, 0.145098f, 0.1647059f, 1.0f);
                        }

                        text.GetComponent<Text>().text = LocalizeData.GetText("LOCALIZE_323");
                        text.GetComponent<Text>().resizeTextForBestFit = true;
                    }
                }
            }
        }
    }
    

    void SetUIData()
    {
        if (isSpecialGift)
        {
            selectObject.SetActive(curPassRewardData.GetNecoPassRewardLevel() > curNecoPassData.GetCurLevel());
            backgroundImage.color = curPassRewardData.GetNecoPassRewardLevel() > curNecoPassData.GetCurLevel() ? dimmedColor : originColor;
        }
        else
        {
            selectObject.SetActive(false);
            backgroundImage.color = curPassRewardData.GetNecoPassRewardLevel() > curNecoPassData.GetCurLevel() ? dimmedColor : originColor;
        }
    }

    public neco_pass_data GetCurPassData()
    {
        return curNecoPassData;
    }

    public neco_pass_reward_forever GetCurRewardData()
    {
        return curPassRewardData;
    }

    void ClearData()
    {

    }
}
