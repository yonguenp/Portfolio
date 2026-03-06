using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoUICanvas : NecoCanvas
{
    public enum UI_TYPE
    {
        MAIN_UI,
        GUIDE_QUEST,
        TOP_INFO_UI,
        NEW_CAT_ALARM,
    };

    public GameObject[] UIObject;
    public delegate void Callback();

    [Header("[Season Pass Layer]")]
    public GameObject[] PassButtonAnimation;

    [Header("MainMenu Red Dot List")]
    public GameObject mainMenuRedDot;
    public GameObject catListRedDot;
    public GameObject albumRedDot;

    [Header("Top MainMenu Red Dot List")]
    public GameObject topMenuRedDot_On;
    public GameObject topMenuRedDot_Off;
    public GameObject postBoxRedDot_On;
    public GameObject postBoxRedDot_Off;
    public GameObject attendanceRedDot_On;
    public GameObject achievementRedDot_On;

    public GameObject passRedDot;
    public GameObject fishingRedDot;

    [Header("[Chat Layer]")]
    public ChatUI ChatUI;

    [Header("[MainMenu Scripts]")]
    public MainMenuPanel mainMenuPanel;

    public void OnUIShow(UI_TYPE type)
    {
        UIObject[(int)type].SetActive(true);
    }

    public void OnUIClose()
    {
        foreach (GameObject popup in UIObject)
        {
            popup.SetActive(false);
        }
    }

    public void OnUIClose(UI_TYPE type)
    {
        UIObject[(int)type].SetActive(false);
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        OnFishingAlarm();
        OnPassAlram();
        RefreshMainMenuRedDot();
        RefreshTopMenuRedDot();
    }

    public void ResetBackgroundSize(Vector2 size)
    {
        (UIObject[(int)UI_TYPE.MAIN_UI].transform as RectTransform).sizeDelta = size;
        (UIObject[(int)UI_TYPE.NEW_CAT_ALARM].transform as RectTransform).sizeDelta = size;
        (UIObject[(int)UI_TYPE.TOP_INFO_UI].transform as RectTransform).localPosition = new Vector3((UIObject[(int)UI_TYPE.TOP_INFO_UI].transform as RectTransform).localPosition.x, ((transform as RectTransform).sizeDelta.y * 0.5f) - 30 + ((size.y - 1280) * 0.5f), 0);

        size = (transform as RectTransform).sizeDelta;

        float ratio = (size.x / size.y);
        float scale = Mathf.Min(1.0f, ratio * (20.0f/9.0f));

        UIObject[(int)UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().ResetBackgroundSize(scale);
        UIObject[(int)UI_TYPE.MAIN_UI].GetComponent<MainMenuPanel>().ResetBackgroundSize(scale);        
    }

    public void OnRefreshGuideUI(Callback callback = null)
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (seq > neco_data.PrologueSeq.배틀패스보상받기)
        {
            string title = LocalizeData.GetText("GQ2");
            string msg = "";
            WWWForm data = new WWWForm();
            data.AddField("api", "chore");
            data.AddField("op", 1);

            switch (seq - 1)
            {
                case neco_data.PrologueSeq.배틀패스보상받기:
                    data.AddField("item", 99);
                    data.AddField("cnt", 6);
                    msg = LocalizeData.GetText("LOCALIZE_461");
                    break;
                case neco_data.PrologueSeq.제작대2레벨:
                    data.AddField("gold", 250);
                    msg = LocalizeData.GetText("GQ31");
                    break;
                case neco_data.PrologueSeq.방문고양이터치:
                    data.AddField("gold", 250);
                    msg = LocalizeData.GetText("GQ32");
                    break;
                case neco_data.PrologueSeq.말풍선아이템받기:
                    data.AddField("item", 104);
                    data.AddField("cnt", 20);
                    msg = LocalizeData.GetText("GQ33");
                    break;
                case neco_data.PrologueSeq.우드락제작:
                    data.AddField("item", 103);
                    data.AddField("cnt", 5);
                    msg = LocalizeData.GetText("GQ34");
                    break;
                case neco_data.PrologueSeq.화이트캣하우스제작:
                    data.AddField("gold", 250);
                    msg = LocalizeData.GetText("GQ35");
                    break;
                case neco_data.PrologueSeq.하트30회:
                    data.AddField("item", 99);
                    data.AddField("cnt", 10);
                    msg = LocalizeData.GetText("GQ36");
                    break;
                case neco_data.PrologueSeq.양어장2레벨:
                    data.AddField("item", 100);
                    data.AddField("cnt", 10);
                    msg = LocalizeData.GetText("GQ37");
                    break;
                case neco_data.PrologueSeq.바구니2레벨:
                    data.AddField("item", 101);
                    data.AddField("cnt", 10);
                    msg = LocalizeData.GetText("GQ38");
                    break;
                case neco_data.PrologueSeq.골드받기:
                    data.AddField("item", 99);
                    data.AddField("cnt", 35);
                    msg = LocalizeData.GetText("GQ39");
                    break;
                case neco_data.PrologueSeq.돌발아이콘터치:
                    data.AddField("item", 61);
                    data.AddField("cnt", 30);
                    msg = LocalizeData.GetText("GQ40");
                    break;
                case neco_data.PrologueSeq.제작대3레벨:
                    data.AddField("item", 101);
                    data.AddField("cnt", 20);
                    msg = LocalizeData.GetText("GQ41");
                    break;
                case neco_data.PrologueSeq.바구니3레벨:
                    data.AddField("item", 99);
                    data.AddField("cnt", 20);
                    msg = LocalizeData.GetText("GQ42");
                    break;
                case neco_data.PrologueSeq.화장실제작:
                    data.AddField("item", 102);
                    data.AddField("cnt", 10);
                    msg = LocalizeData.GetText("GQ43");
                    break;
                case neco_data.PrologueSeq.통발2레벨물고기10개획득:
                    data.AddField("gold", 500);
                    msg = LocalizeData.GetText("GQ44");
                    break;
                case neco_data.PrologueSeq.제작대4레벨:
                    data.AddField("gold", 1000);
                    msg = LocalizeData.GetText("GQ45");
                    break;
                case neco_data.PrologueSeq.보온캣하우스제작:
                    data.AddField("gold", 1000);
                    msg = LocalizeData.GetText("GQ46");
                    break;                
                default:
                    if (seq - 1 >= neco_data.PrologueSeq.양어장3레벨 && seq - 1 <= neco_data.PrologueSeq.알록달록이동식캣하우스5레벨)
                    {
                        neco_data.PrologueSeq pivot = seq - 1;
                        switch (pivot)
                        {
                            case neco_data.PrologueSeq.양어장3레벨:
                                data.AddField("item", 56);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.통발3레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.조리대3레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.빙어튀김요리:
                                data.AddField("item", 59);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.통발4레벨:
                                data.AddField("item", 60);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.조리대4레벨:
                                data.AddField("item", 61);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.잉어찜요리:
                                data.AddField("item", 56);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.바구니4레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.양어장4레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.통발5레벨:
                                data.AddField("item", 59);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.제작대5레벨:
                                data.AddField("item", 60);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.무스크래쳐제작:
                                data.AddField("item", 61);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.양어장5레벨:
                                data.AddField("item", 56);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.조리대5레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.민물고기찜요리:
                                data.AddField("item", 58);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.바구니5레벨:
                                data.AddField("item", 59);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.통발6레벨:
                                data.AddField("item", 60);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.제작대6레벨:
                                data.AddField("item", 61);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.원목캣타워제작:
                                data.AddField("item", 56);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.양어장6레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.조리대6레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.바다고기회요리:
                                data.AddField("item", 59);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.바구니6레벨:
                                data.AddField("item", 60);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.통발7레벨:
                                data.AddField("item", 61);
                                data.AddField("cnt", 50);
                                break;
                            case neco_data.PrologueSeq.제작대7레벨:
                                data.AddField("item", 56);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.반자동화장실제작:
                                data.AddField("item", 57);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.양어장7레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.조리대7레벨:
                                data.AddField("item", 59);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.장어구이요리:
                                data.AddField("item", 60);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.바구니7레벨:
                                data.AddField("item", 61);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.통발8레벨:
                                data.AddField("item", 56);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.제작대8레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.파이프캣타워제작:
                                data.AddField("item", 58);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.양어장8레벨:
                                data.AddField("item", 59);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.조리대8레벨:
                                data.AddField("item", 60);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.참치구이요리:
                                data.AddField("item", 61);
                                data.AddField("cnt", 100);
                                break;
                            case neco_data.PrologueSeq.바구니8레벨:
                                data.AddField("item", 56);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.통발9레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.제작대9레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.나무위캣하우스제작:
                                data.AddField("item", 59);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.양어장9레벨:
                                data.AddField("item", 60);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.조리대9레벨:
                                data.AddField("item", 61);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.고급바다고기회요리:
                                data.AddField("item", 56);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.바구니9레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.통발10레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.제작대10레벨:
                                data.AddField("item", 59);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.크리스마스캣타워제작:
                                data.AddField("item", 60);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.양어장10레벨:
                                data.AddField("item", 61);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.조리대10레벨:
                                data.AddField("item", 56);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.무지개배스찜요리:
                                data.AddField("item", 57);
                                data.AddField("cnt", 150);
                                break;
                            case neco_data.PrologueSeq.바구니10레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 150);
                                break;

                            case neco_data.PrologueSeq.제작대11레벨:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.캣닢급식소제작:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.캣닢급식소3레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.캣닢급식소4레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 20);
                                break;
                            case neco_data.PrologueSeq.캣닢급식소5레벨:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.화이트캣하우스5레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.화이트캣하우스6레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.화이트캣하우스7레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.화이트캣하우스8레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.화이트캣하우스9레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.화이트캣하우스10레벨:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.제작대12레벨:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.플로랄캣하우스제작:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.플로랄캣하우스3레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.플로랄캣하우스4레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.플로랄캣하우스5레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.알록달록이동식캣하우스제작:
                                data.AddField("item", 125);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.알록달록이동식캣하우스3레벨:
                                data.AddField("item", 57);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.알록달록이동식캣하우스4레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;
                            case neco_data.PrologueSeq.알록달록이동식캣하우스5레벨:
                                data.AddField("item", 58);
                                data.AddField("cnt", 200);
                                break;

                            default:
                                data.AddField("gold", 1);
                                break;
                        }


                        //if(pivot <= neco_data.PrologueSeq.빙어튀김요리)
                        //    data.AddField("gold", 300);
                        //else if (pivot <= neco_data.PrologueSeq.양어장4레벨)
                        //    data.AddField("gold", 1500);
                        //else if(pivot <= neco_data.PrologueSeq.바구니5레벨)
                        //    data.AddField("gold", 6000);
                        //else if(pivot < neco_data.PrologueSeq.바구니6레벨)
                        //    data.AddField("gold", 18000);
                        //else if (pivot < neco_data.PrologueSeq.바구니7레벨)
                        //    data.AddField("gold", 40000);
                        //else if (pivot < neco_data.PrologueSeq.바구니8레벨)
                        //    data.AddField("gold", 100000);
                        //else if (pivot < neco_data.PrologueSeq.바구니9레벨)
                        //    data.AddField("gold", 250000);
                        //else if (pivot <= neco_data.PrologueSeq.바구니10레벨)
                        //    data.AddField("gold", 500000);
                        //else
                        //    data.AddField("gold", 1);

                        int diff = (seq - 1) - neco_data.PrologueSeq.양어장3레벨;
                        msg = LocalizeData.GetText("GQ" + (48 + diff).ToString());
                    }
                    else
                    {
                        SetGuideUI();
                        return;
                    }
                    break;
            }

            //if(Random.value <= 0.5f)
            //{
            //    data.AddField("gold", 100);
            //    reward.type = "gold";
            //    reward.amount = 100;
            //}
            //else
            //{
            //    if (Random.value <= 0.5f)
            //    {
            //        data.AddField("item", 11);
            //        reward.type = "item";
            //        reward.amount = 1;
            //    }
            //    else
            //    {
            //        data.AddField("exp", 10);
            //        reward.type = "exp";
            //        reward.amount = 10;
            //    }
            //}

            NetworkManager.GetInstance().SendApiRequest("chore", 1, data, (response) =>
            {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "chore")
                    {
                        JToken opCode = row["op"];
                        if (opCode != null && opCode.Type == JTokenType.Integer)
                        {
                            int op = opCode.Value<int>();
                            switch (op)
                            {
                                case 1:
                                    RewardData rw = new RewardData();
                                    if (row.ContainsKey("rew"))
                                    {
                                        JObject income = (JObject)row["rew"];
                                        if (income.ContainsKey("gold"))
                                        {
                                            rw.gold = income["gold"].Value<uint>();
                                        }
                                        if (income.ContainsKey("item"))
                                        {
                                            JArray item = (JArray)income["item"];
                                            foreach (JObject it in item)
                                            {
                                                rw.itemData = items.GetItem(it["id"].Value<uint>());
                                                rw.count = it["amount"].Value<uint>();
                                            }
                                        }
                                    }

                                    GetPopupCanvas().OnPopupClose();
                                    GetPopupCanvas().OnImageToastPopupShow(title, msg, rw, () => {
                                        callback?.Invoke();
                                        SetGuideUI();
                                    });
                                    //GetPopupCanvas().OnSingleRewardPopup(title, msg, rw, SetGuideUI);
                                    break;
                            }
                        }
                    }
                }
            });
        }
        else
        {
            SetGuideUI();
        }

    }

    public void SetGuideUI()
    {
        if (UIObject[(int)UI_TYPE.GUIDE_QUEST].activeSelf == false)
        {
            UIObject[(int)UI_TYPE.GUIDE_QUEST].SetActive(true);
        }

        GuideQuest guide = UIObject[(int)UI_TYPE.GUIDE_QUEST].GetComponent<GuideQuest>();
        if (!guide.SetGuideUI())
        {
            UIObject[(int)UI_TYPE.GUIDE_QUEST].SetActive(false);
        }

        SetGuideUIPopupCanvas();
    }

    public void SetGuideUIPopupCanvas()
    {
        if (GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.gameObject.activeSelf == false)
        {
            GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.gameObject.SetActive(true);
        }

        GuideQuest guide = GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.GetComponent<GuideQuest>();
        if (!guide.SetGuideUI())
        {
            GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.gameObject.SetActive(false);
        }
    }

    public void OnTutorialGuideUI(string text, string rewardTitle, string rewardMsg, bool withReward = true)
    {
        if (withReward)
        {
            WWWForm data = new WWWForm();
            data.AddField("api", "chore");
            data.AddField("op", 1);

            switch (neco_data.GetPrologueSeq())
            {
                case neco_data.PrologueSeq.조리대UI닫힘:
                    data.AddField("item", 56);
                    data.AddField("cnt", 20);
                    break;
                case neco_data.PrologueSeq.첫밥주기완료:
                    data.AddField("item", 57);
                    data.AddField("cnt", 20);
                    break;
                case neco_data.PrologueSeq.고양이10번터치가이드퀘스트완료:
                    data.AddField("item", 101);
                    data.AddField("cnt", 2);
                    break;
                case neco_data.PrologueSeq.철판제작가이드퀘스트완료:
                    data.AddField("gold", 500);
                    break;
                case neco_data.PrologueSeq.조리대레벨업완료:
                    data.AddField("gold", 500);
                    break;
                case neco_data.PrologueSeq.상점배스구매완료:
                    data.AddField("gold", 500);
                    break;
                case neco_data.PrologueSeq.배스구이완료후밥그릇강조:
                    data.AddField("item", 102);
                    data.AddField("cnt", 11);
                    break;
                case neco_data.PrologueSeq.첫보은받기성공:
                    data.AddField("item", 57);
                    data.AddField("cnt", 20);
                    break;
                case neco_data.PrologueSeq.낚시장난감만들기완료:
                    data.AddField("item", 104);
                    data.AddField("cnt", 5);
                    break;
                default:
                    data.AddField("gold", 600);
                    break;
            }

            NetworkManager.GetInstance().SendApiRequest("chore", 1, data, (response) =>
            {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    return;
                }

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "chore")
                    {
                        JToken opCode = row["op"];
                        if (opCode != null && opCode.Type == JTokenType.Integer)
                        {
                            int op = opCode.Value<int>();
                            switch (op)
                            {
                                case 1:
                                    RewardData rw = new RewardData();
                                    if (row.ContainsKey("rew"))
                                    {
                                        JObject income = (JObject)row["rew"];
                                        if (income.ContainsKey("gold"))
                                        {
                                            rw.gold = income["gold"].Value<uint>();
                                        }
                                        if (income.ContainsKey("item"))
                                        {
                                            JArray item = (JArray)income["item"];
                                            foreach (JObject it in item)
                                            {
                                                rw.itemData = items.GetItem(it["id"].Value<uint>());
                                                rw.count = it["amount"].Value<uint>();
                                            }
                                        }
                                    }
                                    GetPopupCanvas().OnImageToastPopupShow(rewardTitle, rewardMsg, rw);
                                    //GetPopupCanvas().OnSingleRewardPopup(rewardTitle, rewardMsg, rw);
                                    break;
                            }
                        }
                    }
                }
            });
        }

        SetTutorialGuideUI(text);
    }

    public void ResumeTutorialUI()
    {
        MainMenuPanel MainUIPanel = gameObject.GetComponentInChildren<MainMenuPanel>(true);
        SetTutorialGuideUI(PlayerPrefs.GetString("TutorialGuideUI", ""));
    }

    public void SetTutorialGuideUI(string text)
    {
        PlayerPrefs.SetString("TutorialGuideUI", text);

        if (UIObject[(int)UI_TYPE.GUIDE_QUEST].activeSelf == false)
        {
            UIObject[(int)UI_TYPE.GUIDE_QUEST].SetActive(true);
        }

        if (string.IsNullOrEmpty(text))
        {
            UIObject[(int)UI_TYPE.GUIDE_QUEST].SetActive(false);
        }
        else
        {
            GuideQuest guide = UIObject[(int)UI_TYPE.GUIDE_QUEST].GetComponent<GuideQuest>();
            guide.SetTutorialGuideUI(text);
        }

        SetTutorialGuideUIPopupCanvas(text);
    }

    public void SetTutorialGuideUIPopupCanvas(string text)
    {
        if (GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.gameObject.activeSelf == false)
        {
            GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.gameObject.SetActive(true);
        }

        if (string.IsNullOrEmpty(text))
        {
            GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.gameObject.SetActive(false);
        }
        else
        {
            GuideQuest guide = GetPopupCanvas().topUIInfoLayer.GetComponent<NecoTopUIInfoPanel>().guideQuestPanel.GetComponent<GuideQuest>();
            guide.SetTutorialGuideUI(text);
        }
    }

    public void OnLeftMap()
    {
        NecoCanvas.GetGameCanvas().OnMoveLeftMap();
    }

    public void OnRightMap()
    {
        NecoCanvas.GetGameCanvas().OnMoveRightMap();
    }

    public void SetMapMoveButtons(bool enable)
    {
        MainMenuPanel MainUIPanel = gameObject.GetComponentInChildren<MainMenuPanel>(true);
        MainUIPanel.MidUIPanel.SetActive(enable);

        if(enable)
        {
            foreach(Transform child in MainUIPanel.MidUIPanel.transform)
            {
                child.gameObject.SetActive(false);
            }
            //MainUIPanel.MidUIPanel.transform.Find("left").GetComponent<Button>().interactable = GetGameCanvas().IsMovableLeft();
            //MainUIPanel.MidUIPanel.transform.Find("right").GetComponent<Button>().interactable = GetGameCanvas().IsMovableRight();
        }
    }

    public bool IsMapMovable()
    {
        return neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.스와이프가이드;
    }

    public void RefreshTopUILayer(TOP_UI_PANEL_TYPE refreshType = TOP_UI_PANEL_TYPE.ALL, bool both = true)
    {
        UIObject[(int)UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().RefreshPanelData(refreshType);
        if(both)
            GetPopupCanvas().RefreshTopUILayer(refreshType, false);
    }

    public void OnCatVisitCoin(uint catid, uint coin)
    {
        if (!GetPopupCanvas().OnCatVisitCoin(catid, coin))
            UIObject[(int)UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().OnCatVisitCoin(catid, coin);
    }

    public bool IsTopUILayerOpen(TOP_UI_PANEL_TYPE type)
    {
       return UIObject[(int)UI_TYPE.TOP_INFO_UI].GetComponent<NecoTopUIInfoPanel>().IsUIOpen(type);
    }

    public void RefreshNewCatAlarm()
    {
        List<uint> readyCatList = neco_data.Instance.GetReadyCatList();

        if (readyCatList == null || readyCatList.Count <= 0 || neco_data.GetPrologueSeq() < neco_data.PrologueSeq.챕터5시작)
        {
            GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.NEW_CAT_ALARM);
            return;
        }

        GetUICanvas().OnUIShow(NecoUICanvas.UI_TYPE.NEW_CAT_ALARM);

        if (UIObject[(int)UI_TYPE.NEW_CAT_ALARM] != null && UIObject[(int)UI_TYPE.NEW_CAT_ALARM].activeInHierarchy)
        {
            UIObject[(int)UI_TYPE.NEW_CAT_ALARM].GetComponent<NecoNewCatIconAlarmPopup>().RefreshNewCatAlarm();
        }
    }

    public void RefreshMainMenuRedDot()
    {
        // 새로운 고양이 & 추억 알람
        bool catAlarm = UpdateCatListAlarm();

        // 앨범 알람
        bool albumAlarm = UpdateAlbumAlarm();

        // 메인 메뉴 알람
        mainMenuRedDot.SetActive(catAlarm || albumAlarm);
    }

    public void UpdateMainMenuRedDot()
    {
        mainMenuRedDot.SetActive(catListRedDot.activeSelf || albumRedDot.activeSelf);
    }

    public void RefreshTopMenuRedDot()
    {
        // 출석 체크
        bool attendanceState = false;
        GameObject attendanceObject = NecoCanvas.GetPopupCanvas()?.PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP];
        if (attendanceObject != null)
        {
            NecoAttendancePopup attendancePopup = attendanceObject.GetComponent<NecoAttendancePopup>();
            if (attendancePopup)
            {
                attendanceState = attendancePopup.IsCheckAble();
            }
        }
        attendanceRedDot_On.SetActive(attendanceState);

        // 업적 체크
        bool achievementState = achievementRedDot_On.activeInHierarchy;

        // 우편물 체크
        bool postState = UpdatePostState();

        // 우상단 햄버거 메뉴
        topMenuRedDot_On.SetActive(attendanceState || achievementState || postState);
        topMenuRedDot_Off.SetActive(attendanceState || achievementState || postState);
    }

    public void SetAchievementRedDotState(bool state)
    {
        achievementRedDot_On.SetActive(state);
    }

    public bool UpdatePostState()
    {
        bool result = neco_data.Instance.GetNewPostTimestamp() > neco_data.Instance.GetOpenPostTimestamp();

        postBoxRedDot_On.SetActive(result);
        postBoxRedDot_Off.SetActive(result);

        return result;
    }

    public void OnPassAlram()
    {
        bool alarm = neco_data.Instance.GetPassData().IsPassAlarm() || neco_data.Instance.GetPassData().IsDailyAlarm() || neco_data.Instance.GetPassData().IsSeasonAlarm();
        PassButtonAnimation[0].SetActive(alarm);
        PassButtonAnimation[1].SetActive(!alarm);

        passRedDot.SetActive(alarm);
    }

    public void OnFishingAlarm()
    {
        fishingRedDot.SetActive(neco_data.Instance.FishingData.IsNewBaits());
    }

    public bool UpdateAlbumAlarm()
    {
        albumRedDot.SetActive(false);

        List<game_data> userData_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        if (userData_list != null)
        {
            foreach (game_data data in userData_list)
            {
                user_card userCard = (user_card)data;

                string newPhotoKey = string.Format("{0}_{1}_{2}", SamandaLauncher.GetAccountNo(), userCard.GetCardID(), userCard.GetCardUniqueID());
                if (PlayerPrefs.GetInt(newPhotoKey, 0) == 0)
                {
                    albumRedDot.SetActive(true);
                    return true;
                }
            }
        }

        return false;
    }

    public void SetAlbumAlarmState(bool state)
    {
        albumRedDot.SetActive(state);

        UpdateMainMenuRedDot();
    }

    public bool UpdateCatListAlarm()
    {
        // 새로운 고양이 획득 알람
        List<uint> newCatList = neco_data.Instance.GetNewCatList();
        bool catAlarm = (newCatList != null && newCatList.Count > 0);

        bool catMemoryAlarm = false;
        List<neco_user_cat> userCatList = neco_user_cat.GetGainUserCatList();
        if (userCatList != null)
        {
            foreach (neco_user_cat userCat in userCatList)
            {
                List<uint> catMemoryList = userCat.GetMemories();
                foreach (uint memoryID in catMemoryList)
                {
                    string newMemoryKey = string.Format("{0}_{1}", SamandaLauncher.GetAccountNo(), memoryID);
                    if (PlayerPrefs.HasKey(newMemoryKey) && PlayerPrefs.GetInt(newMemoryKey, 0) == 0)
                    {
                        catMemoryAlarm = true;
                        catListRedDot.SetActive(catAlarm || catMemoryAlarm);
                        return catAlarm || catMemoryAlarm;
                    }
                }
            }
        }

        catListRedDot.SetActive(catAlarm || catMemoryAlarm);
        return catAlarm || catMemoryAlarm;
    }
}
