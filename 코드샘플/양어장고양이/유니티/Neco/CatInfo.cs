using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class CatInfo : MonoBehaviour
{
    public Text catName;
    public GameObject new_alarm;
    public GameObject red_dot;
    public Image catIcon;
    public Image coverCatIcon;
    public Color undiscoverColor;
    public GameObject gaugeObject;

    neco_cat curCatData;

    NecoCatListPanel parentPanelUI;

    Button buttonUI;
    RewardData rewards;
    private void Awake()
    {
        buttonUI = GetComponent<Button>();
    }

    private void OnEnable()
    {
        //newVisitAlarm.SetActive(false);
    }

    public void SetNotReleasedCat()
    {
        curCatData = null;
        parentPanelUI = null;

        red_dot.SetActive(false);
        new_alarm.SetActive(false);
        gaugeObject.SetActive(false);

        catName.text = "????";
        buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("업데이트예정"));
        }));
    }

    public void SetCatInfoData(neco_cat data, NecoCatListPanel parentPanel)
    {
        if (data == null) { return; }
        if (parentPanel == null) { return; }
        if (buttonUI == null) { return; }

        curCatData = data;
        parentPanelUI = parentPanel;

        SetCatInfoByCatState();

        buttonUI.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            //if (curCatData.GetCatState() >= 3)
            //{
            //    parentPanelUI.SetSelectedCatInfo(curCatData);
            //    parentPanelUI.gameObject.SetActive(false);
            //}

            if(curCatData.GetCatState() == 2)
            {
                OnAnimateCatInfo();
            }
            else
            {
                parentPanelUI.SetSelectedCatInfo(curCatData);
                parentPanelUI.gameObject.SetActive(false);
            }

        }));

    }

    public neco_cat GetData()
    {
        return curCatData;
    }

    void SetCatInfoByCatState()
    {
        if (curCatData == null) { return; }

        // 고양이 얼굴 아이콘 세팅
        var iconPath = curCatData.GetIconPath();
        catIcon.sprite = Resources.Load<Sprite>(iconPath);
        coverCatIcon.sprite = Resources.Load<Sprite>(iconPath);

        // 기타 정보 세팅

        new_alarm.SetActive(false);
        red_dot.SetActive(false);
        gaugeObject.SetActive(false);
        Transform Max = gaugeObject.transform.Find("Max");
        if (Max != null)
        {
            Max.gameObject.SetActive(false);
        }

        if (curCatData.GetCatState() >= 3)
        {
            catIcon.color = Color.white;
            catName.text = curCatData.GetCatName();
            coverCatIcon.gameObject.SetActive(false);

            Transform GaugeFilled = gaugeObject.transform.Find("GaugeFilled");
            
            if (GaugeFilled != null)
            {
                gaugeObject.SetActive(true);
                int total = neco_cat_memory.GetNecoMemoryCount(curCatData.GetCatID());
                int cur = (int)curCatData.GetMemoryCount();
                float rate = (float)cur / total;

                GaugeFilled.GetComponent<Image>().fillAmount = rate;

                if(Max != null)
                {
                    Max.gameObject.SetActive(total == cur);
                }
            }
        }
        else
        {
            if(curCatData.GetCatState() == 2)
            {
                new_alarm.SetActive(true);

                Sequence seq = DOTween.Sequence();

                seq.Append(new_alarm.transform.DOScale(1.2f, 0.5f));
                seq.Append(new_alarm.transform.DOScale(1.0f, 0.5f));
                seq.SetLoops(-1, LoopType.Yoyo);
            }

            catIcon.color = Color.white;
            coverCatIcon.gameObject.SetActive(true);
            coverCatIcon.fillAmount = 1.0f;
            catName.text = "????";
        }

        // 레드닷 조건 체크
        UpdateRedDotState();
    }

    public void OnAnimateCatInfo()
    {
        if (!new_alarm.activeSelf)
        {
            RewardDone();
            return;
        }

        new_alarm.SetActive(false);

        coverCatIcon.fillAmount = 1.0f;
        //coverCatIcon.transform.parent.localScale = Vector3.one * 1.1f;
        coverCatIcon.DOFillAmount(0.0f, 0.7f).OnComplete(()=> {
            WWWForm data = new WWWForm();
            data.AddField("api", "neco");
            data.AddField("op", 3);
            data.AddField("id", curCatData.GetCatID().ToString());

            NetworkManager.GetInstance().SendApiRequest("neco", 3, data, (response) =>
            {
                JObject root = JObject.Parse(response);
                JToken apiToken = root["api"];
                if (null == apiToken || apiToken.Type != JTokenType.Array
                    || !apiToken.HasValues)
                {
                    RewardDone();
                    return;
                }

                rewards = null;

                JArray apiArr = (JArray)apiToken;
                foreach (JObject row in apiArr)
                {
                    string uri = row.GetValue("uri").ToString();
                    if (uri == "neco")
                    {
                        JToken opCode = row["op"];
                        if (opCode != null && opCode.Type == JTokenType.Integer)
                        {
                            int op = opCode.Value<int>();
                            switch (op)
                            {
                                case 3:
                                    {
                                        if (row.ContainsKey("rew"))
                                        {
                                            if (row["rew"].HasValues)
                                            {
                                                JObject income = (JObject)row["rew"];
                                                if (income.ContainsKey("gold"))
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.gold = income["gold"].Value<uint>();
                                                    rewards = reward;
                                                }

                                                if (income.ContainsKey("catnip"))
                                                {
                                                    RewardData reward = new RewardData();
                                                    reward.catnip = income["catnip"].Value<uint>();
                                                    rewards = reward;
                                                }

                                                if (income.ContainsKey("item"))
                                                {
                                                    JArray item = (JArray)income["item"];
                                                    foreach (JObject rw in item)
                                                    {
                                                        RewardData reward = new RewardData();
                                                        reward.itemData = items.GetItem(rw["id"].Value<uint>());
                                                        reward.count = rw["amount"].Value<uint>();
                                                        rewards = reward;
                                                    }
                                                }
                                            }
                                        }

                                        NecoCanvas.GetPopupCanvas().OnSuccessEffectPopupShow(EFFECT_TYPE.NEW_CAT, curCatData.GetCatID(), 0, ShowRewardPopup);
                                    }
                                    break;
                            }
                        }
                    }
                }
            });
        });

        catName.text = "????";
        catName.color = Color.white;
        Sequence seq = DOTween.Sequence();
        seq.Append(catName.DOColor(new Color(1f, 1f, 1f, 0f), 0.35f));
        seq.Append(catName.DOText(curCatData.GetCatName(), 0.1f, false));
        seq.Append(catName.DOColor(new Color(1f, 1f, 1f, 1f), 0.35f));
    }

    void ShowNecoDetailPopup()
    {
        if (parentPanelUI == null) { return; }

        parentPanelUI.SetSelectedCatInfo(curCatData);
        parentPanelUI.gameObject.SetActive(false);
    }

    public void ShowRewardPopup()
    {
        //coverCatIcon.transform.parent.localScale = Vector3.one * 1.0f;        
        NecoCanvas.GetPopupCanvas().OnSingleRewardPopup(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), rewards, RewardDone);
    }

    public void RewardDone()
    {
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이획득프로필팝업확인)
        {
            MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
            if (mapController != null)
            {
                mapController.SendMessage("길막이배고파연출", SendMessageOptions.DontRequireReceiver);
                return;
            }
        }

        ShowNecoDetailPopup();
    }

   void UpdateRedDotState()
    {
        bool newCatAlarm = curCatData != null ? curCatData.GetCatState() == 2 : false;
        bool newMemoryAlarm = false;

        List<neco_cat_memory> memoryList = neco_cat_memory.GetNecoMemoryByCatID(curCatData.GetCatID());
        foreach (neco_cat_memory memory in memoryList)
        {
            string newMemoryKey = string.Format("{0}_{1}", SamandaLauncher.GetAccountNo(), memory.GetNecoMemoryID());
            if (PlayerPrefs.HasKey(newMemoryKey) && PlayerPrefs.GetInt(newMemoryKey, 0) == 0)
            {
                newMemoryAlarm = true;
                break;
            }
        }

        red_dot.SetActive(newCatAlarm || newMemoryAlarm);
    }
}
