using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class ChuseokMarbleGamePanel : MonoBehaviour
{
    public GameObject catSelectLayerObject;
    public GameObject marbleBoardLayerObject;

    public Text diceCountText;

    ChuseokUI rootParentPanel;

    public Transform MapTileContainer;
    public GameObject CurCatObject;
    public GameObject[] MapTilePrefab;
    public ScrollRect MapScroller;
    public Dice2D Dice;
    public Button DiceRollButton;
    public Text DiceAmount;

    [Header("[Contents Info]")]
    public GameObject contentsInfoPopup;
    public Text contentsInfoText;

    [Serializable]
    public class Top3CatInfo
    {
        public Image catIcon;
        public Text catName;
        public Text catSelectedCount;
    }

    [Header("[Top3 Cat Info]")]
    public GameObject top3DimmedLayerObject;
    public GameObject top3CatInfoPopup;
    public Top3CatInfo[] top3CatInfo = new Top3CatInfo[3];

    neco_cat[] top3CatDatas = new neco_cat[3];

    System.Action moveDoneCallback = null;

    public GameObject GoalRewardPopup;
    public GameObject TargetEffect;
    public Transform EffectContainer;
    enum MAPTILE_TYPE
    { 
        NONE = 0,
        NORAML,
        START,
        GOAL,
        SONGPYEON,
        CATNIP,

        SADARI_UP,
        SADARI_DOWN,
        
        GOLD,
        MATERIAL,
    };

    private int curSelectCat = 0;
    private int curCatPos = 0;
    List<Transform> Tiles = new List<Transform>();
    List<MAPTILE_TYPE> TileTypes = new List<MAPTILE_TYPE>();
    public void OnClickEvnetGuideButton()
    {
        if (contentsInfoPopup == null) { return; }

        contentsInfoPopup.SetActive(!contentsInfoPopup.activeSelf);

        if (contentsInfoPopup.activeSelf)
        {
            if (contentsInfoText != null)
            {
                chuseok_event eventData = null;
                foreach (neco_event evt in neco_data.Instance.GetEvents())
                {
                    if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                        eventData = (chuseok_event)evt;
                }

                if (eventData != null)
                {
                    chuseok_event.chuseok_marble_data data = eventData.GetMarbleData();

                    int max_back = (int)data.back_dice_max;
                    int max_front = (int)data.front_dice_max;
                    int max_touch = (int)data.touch_dice_max;

                    int back = max_back - (int)data.back_dice_chance;
                    int front = max_front - (int)data.front_dice_chance;
                    int touch = max_touch - (int)data.touch_dice_chance;

                    string text = string.Format(
#if UNITY_IOS
                LocalizeData.GetText("냥이마블도움말_FORIOS")
#else
                LocalizeData.GetText("냥이마블도움말")
#endif
                , back, front, touch, max_back, max_front, max_touch);

                    contentsInfoText.text = text;
                }
            }
        }
    }

    public void OnClickTop3ChoiceButton()
    {
        if (top3CatInfoPopup == null) { return; }

        if (top3CatInfoPopup.activeSelf)    // 팝업이 켜져있으면 off
        {
            top3DimmedLayerObject.SetActive(false);
            top3CatInfoPopup.SetActive(false);
            return;
        }

        top3DimmedLayerObject.SetActive(true);


        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHUSEOK);
        data.AddField("op", 13);

        NetworkManager.GetInstance().SendApiRequest("event", 13, data, (response) =>
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            if (row.ContainsKey("ranks"))
                            {
                                top3DimmedLayerObject.SetActive(false);
                                JArray ranks = (JArray)row["ranks"];

                                if (ranks.Count >= 3)
                                {
                                    for (int i = 0; i < ranks.Count; i++)
                                    {
                                        JArray vals = (JArray)ranks[i];

                                        neco_cat cat = neco_cat.GetNecoCat(vals[0].Value<uint>());
                                        top3CatInfo[i].catIcon.sprite = Resources.Load<Sprite>(cat.GetIconPath());
                                        top3CatInfo[i].catIcon.color = Color.white;
                                        top3CatInfo[i].catName.text = cat.GetCatName();
                                        top3CatInfo[i].catSelectedCount.text = string.Format(LocalizeData.GetText("시도횟수"), vals[1].Value<uint>().ToString("n0"));
                                    }

                                    top3CatInfoPopup.SetActive(true);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }

                        top3DimmedLayerObject.SetActive(true);
                        top3CatInfoPopup.SetActive(true);
                    }
                }
            }
        });
    }

    public void OnClickCatSelectConfirmButton()
    {
        if (curSelectCat == 0)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow("고양이를 선택하시오");
            return;
        }

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHUSEOK);
        data.AddField("op", 11);
        data.AddField("neco", curSelectCat);

        NetworkManager.GetInstance().SendApiRequest("event", 11, data, (response) =>
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("InitSelectedCat", 0.01f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_338"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_316"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });
    }

    public void InitSelectedCat()
    {
        catSelectLayerObject.SetActive(false);
        marbleBoardLayerObject.SetActive(true);

        InitMarbleGamePanel(rootParentPanel);

        for (int i = 0; i < Tiles.Count; i++)
        {
            Tiles[i].DOLocalJump(Tiles[i].transform.localPosition, 50, 1, 1.0f).SetDelay((Tiles.Count - i) * 0.05f);

            foreach (MaskableGraphic image in Tiles[i].GetComponentsInChildren<MaskableGraphic>())
            {
                Color origin = image.color;
                origin.a = 0.0f;
                image.color = origin;
                origin.a = 1.0f;
                image.DOColor(origin, 0.5f).SetDelay((Tiles.Count - i) * 0.05f);
            }

        }
        MapScroller.verticalNormalizedPosition = 1.0f;
        MapScroller.DOVerticalNormalizedPos(0.0f, 2.5f).SetDelay(0.5f);
    }

    public void InitMarbleGamePanel(ChuseokUI rootPanel)
    {
        Dice.Clear();
        GoalRewardPopup.SetActive(false);

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }

        if (eventData == null)
            return;
        
        rootParentPanel = rootPanel;
        Tiles.Clear();
        TileTypes.Clear();

        curSelectCat = (int)eventData.GetMarbleData().curSelectCat;

        if (curSelectCat == 0)
        {
            catSelectLayerObject.SetActive(true);
            marbleBoardLayerObject.SetActive(false);
            return;
        }

        if(eventData.GetMarbleData().mapType.Count <= 0)
        {
            catSelectLayerObject.SetActive(true);
            marbleBoardLayerObject.SetActive(false);
            return;
        }

        catSelectLayerObject.SetActive(false);
        marbleBoardLayerObject.SetActive(true);

        CurCatObject.transform.SetParent(MapTileContainer);

        foreach (Transform t in MapTileContainer)
        {
            if (CurCatObject.transform == t)
                continue;

            foreach(Transform child in t)
            {
                DestroyImmediate(child.gameObject);
            }

            Tiles.Add(t);
        }
        
        for(int i = 0; i < Tiles.Count; i++)
        {
            TileTypes.Add(MAPTILE_TYPE.NORAML);
        }

        Tiles.Reverse();
       
        for (int i = 0; i < Tiles.Count; i++)
        {
            if (eventData.GetMarbleData().mapType.Count <= i)
                continue;
            
            Transform t = Tiles[i];

            uint type = eventData.GetMarbleData().mapType[i];
            if (MapTilePrefab.Length <= type)
                continue;
            GameObject tile = Instantiate(MapTilePrefab[type]);
            tile.transform.SetParent(t);
            
            MAPTILE_TYPE tileType = (MAPTILE_TYPE)type;
            TileTypes[i] = tileType;

            tile = t.GetChild(0).gameObject;
            tile.transform.localPosition = Vector3.zero;
            tile.transform.localScale = Vector3.one;
        }

        InitCat();
        DiceAmount.text = user_items.GetUserItemAmount(143).ToString();
        TileIdleAction();
    }

    void TileIdleAction()
    {
        CancelInvoke("TileIdleAction");

        int index = UnityEngine.Random.Range(curCatPos + 1, curCatPos + 7);
        if (index <= 0 || index >= TileTypes.Count)
        {
            Invoke("TileIdleAction", 1.0f);
            return;
        }

        MAPTILE_TYPE tileType = TileTypes[index];

        if (tileType != MAPTILE_TYPE.MATERIAL
            && tileType != MAPTILE_TYPE.GOLD
            && tileType != MAPTILE_TYPE.SONGPYEON
            && tileType != MAPTILE_TYPE.CATNIP)
        {
            Invoke("TileIdleAction", 1.0f);
            return;
        }

        Transform child = Tiles[index].GetChild(0);
        if (child != null)
            child = child.GetChild(0);

        if (child != null)
        {
            Vector3 originPos = child.localPosition;
            child.DOShakePosition(0.3f, 3).SetLoops(3, LoopType.Yoyo).OnComplete(()=> {
                child.localPosition = originPos;
            });
        }

        Invoke("TileIdleAction", 3.0f + UnityEngine.Random.value * 3.0f);
    }

    public void OnSelectCat(int type)
    {
        curSelectCat = type;
        OnClickCatSelectConfirmButton();
    }

    private void InitCat()
    {
        foreach(Transform child in CurCatObject.transform.GetChild(0))
        {
            child.gameObject.SetActive(false);
        }

        if (curSelectCat <= 0)
            return;

        CurCatObject.transform.GetChild(0).GetChild(curSelectCat - 1).gameObject.SetActive(true);

        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }
        if (eventData == null)
            return;

        if (curCatPos >= 0 && Tiles.Count > curCatPos)
        {
            if (TileTypes[curCatPos] == MAPTILE_TYPE.GOAL)
            {
                //moveDoneCallback 에서 처리
            }

            if (TileTypes[curCatPos] == MAPTILE_TYPE.SADARI_UP)
            {
                int posIndex = (curCatPos) % 10;
                int[] jumpOffset = { 8, 6, 4, 2, 0, 8, 6, 4, 2, 0 };
                OnJump(curCatPos + jumpOffset[posIndex]);
                return;
            }
            if (TileTypes[curCatPos] == MAPTILE_TYPE.SADARI_DOWN)
            {
                int posIndex = (curCatPos) % 10;
                int[] jumpOffset = { 2, 4, 6, 8, 0, 2, 4, 6, 8, 0 };
                OnJump(curCatPos - jumpOffset[posIndex]);
                return;
            }
        }
        else if (curCatPos < 0)
        {
            //moveDoneCallback 에서 처리
        }

        curCatPos = (int)eventData.GetMarbleData().curMapPos - 1;

        CurCatObject.transform.DOKill();

        if (curCatPos >= 0 && Tiles.Count > curCatPos)
        {
            CurCatObject.transform.SetParent(Tiles[curCatPos]);
        }
        else
        {
            CurCatObject.transform.SetParent(MapTileContainer);
        }
        CurCatObject.transform.localPosition = Vector3.zero;
        CurCatObject.transform.localEulerAngles = Vector3.zero;
        CurCatObject.transform.localScale = Vector3.one;

        CurCatObject.transform.DOScale(0.8f, 1.0f).SetLoops(-1, LoopType.Yoyo);
        CurCatObject.transform.DOLocalMoveY(5, 1.0f).SetLoops(-1, LoopType.Yoyo);

        OnFocusCat();

        DiceRollButton.interactable = true;

        moveDoneCallback?.Invoke();
        moveDoneCallback = null;
    }

    private void OnGoal()
    {
        chuseok_event eventData = null;
        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.CHUSEOK)
                eventData = (chuseok_event)evt;
        }
        eventData.GetMarbleData().curSelectCat = 0;

        curSelectCat = 0;
        curCatPos = 0;
        InitMarbleGamePanel(rootParentPanel);
    }

    private void OnJump(int index)
    {
        if (index < 0)
            return;
        if (index >= Tiles.Count)
            return;

        CurCatObject.transform.DOKill();

        CurCatObject.transform.SetParent(MapTileContainer);
        CurCatObject.transform.localPosition = Tiles[curCatPos].localPosition;

        CurCatObject.transform.localEulerAngles = Vector3.zero;
        CurCatObject.transform.localScale = Vector3.one;

        Transform bot = Tiles[curCatPos];
        bot.localScale = Vector3.one * 0.8f;
        bot.DOScale(Vector3.one, 1.0f).SetEase(Ease.OutBounce).OnComplete(()=> { bot.localScale = Vector3.one; });

        bool bFall = true;
        if (curCatPos <= index)
            bFall = false;

        curCatPos = index;

        if (bFall)
        {
            CurCatObject.transform.DOScale(Vector3.zero, 0.5f);
            CurCatObject.transform.DOScale(Vector3.one, 0.5f).SetDelay(0.5f);
            CurCatObject.transform.DOLocalMove(Tiles[curCatPos].localPosition, 1.0f).OnComplete(InitCat);
        }
        else
        {
            CurCatObject.transform.DOScaleY(0.5f, 0.25f);
            CurCatObject.transform.DOScaleY(1.0f, 0.25f).SetDelay(0.25f);
            CurCatObject.transform.DOLocalMove(Tiles[curCatPos].localPosition, 0.5f).SetDelay(0.5f).OnComplete(InitCat);
        }
    }


    public void OnDice()
    {
        if (user_items.GetUserItemAmount(143) <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_490"), LocalizeData.GetText("주사위부족"));
            return;
        }

        DiceRollButton.interactable = false;
        moveDoneCallback = null;
        DiceAmount.text = (user_items.GetUserItemAmount(143) - 1).ToString();

        WWWForm data = new WWWForm();
        data.AddField("uri", "event");
        data.AddField("eid", (int)neco_event.EVENT_TYPE.CHUSEOK);
        data.AddField("op", 12);

        NetworkManager.GetInstance().SendApiRequest("event", 12, data, (response) =>
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
                if (uri == "event")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            int num = row["res"].Value<int>();
                            Dice.OnDice(num, () => {
                                int nextPos = curCatPos + num;
                                if (nextPos >= Tiles.Count)
                                {
                                    num = (Tiles.Count - 1) - curCatPos;
                                    nextPos = Tiles.Count - 1;
                                }

                                if (num > 0)
                                    OnMoveCat(num);
                            });

                            if(row.ContainsKey("rew"))
                            {
                                moveDoneCallback = () => {
                                    if (TileTypes[curCatPos] == MAPTILE_TYPE.GOAL)
                                    {
                                        OnGoalEffect((JObject)row["rew"]);
                                    }
                                    else
                                    {
                                        NecoCanvas.GetPopupCanvas().OnRewardPopupShow(LocalizeData.GetText("LOCALIZE_200"), LocalizeData.GetText("LOCALIZE_201"), "event", apiArr);
                                    }
                                    DiceAmount.text = user_items.GetUserItemAmount(143).ToString();
                                };
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("Event_Res_1"); break;
                                case 2: msg = LocalizeData.GetText("Event_Res_2"); break;
                                case 11: msg = LocalizeData.GetText("Event_Res_11"); break;
                                case 12: msg = LocalizeData.GetText("Event_Res_12"); break;
                                case 13: msg = LocalizeData.GetText("Event_Res_13"); break;
                                case 14: msg = LocalizeData.GetText("Event_Res_14"); break;
                                case 31: msg = LocalizeData.GetText("Event_Res_31"); break;
                                case 32: msg = LocalizeData.GetText("Event_Res_32"); break;
                                case 41: msg = LocalizeData.GetText("Event_Res_41"); break;
                                case 42: msg = LocalizeData.GetText("Event_Res_42"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_316"), msg);
                        }
                    }
                }
            }
        });
    }

    public void OnMoveCat(int num)
    {
        CurCatObject.transform.DOKill();

        CurCatObject.transform.SetParent(MapTileContainer);
        CurCatObject.transform.localPosition = Tiles[curCatPos].localPosition;

        CurCatObject.transform.localEulerAngles = Vector3.zero;
        CurCatObject.transform.localScale = Vector3.one;

        Transform bot = Tiles[curCatPos];
        bot.localScale = Vector3.one * 0.9f;
        bot.DOScale(Vector3.one, 1.0f).SetEase(Ease.OutBounce).OnComplete(() => { bot.localScale = Vector3.one; });

        curCatPos += 1;
        num -= 1;

        if (num == 0)
        {
            CurCatObject.transform.DOLocalJump(Tiles[curCatPos].localPosition, 50, 1, 0.5f).OnComplete(InitCat);
        }
        else
        {
            CurCatObject.transform.DOLocalJump(Tiles[curCatPos].localPosition, 50, 1, 0.5f).OnComplete(()=> { OnMoveCat(num); });
        }

        OnFocusCat();
    }

    public void OnFocusCat()
    {
        float rate = ((int)((curCatPos + 1) / 5.0f)) / 10.0f;

        MapScroller.DOVerticalNormalizedPos(rate, 0.5f);
    }

    private void OnDisable()
    {
        contentsInfoPopup.SetActive(false);

        top3CatInfoPopup.SetActive(false);
    }

    void OnGoalEffect(JObject income)
    {
        moveDoneCallback = null;

        List<RewardData> rewardList = new List<RewardData>();

        if (income.ContainsKey("gold"))
        {
            RewardData reward = new RewardData();
            reward.gold = income["gold"].Value<uint>();
            rewardList.Add(reward);
        }

        if (income.ContainsKey("catnip"))
        {
            RewardData reward = new RewardData();
            reward.catnip = income["catnip"].Value<uint>();
            rewardList.Add(reward);
        }

        if (income.ContainsKey("point"))
        {
            RewardData reward = new RewardData();
            reward.point = income["point"].Value<uint>();
            rewardList.Add(reward);
        }

        if (income.ContainsKey("item"))
        {
            JArray item = (JArray)income["item"];
            foreach (JObject rw in item)
            {
                RewardData reward = new RewardData();
                reward.itemData = items.GetItem(rw["id"].Value<uint>());
                reward.count = rw["amount"].Value<uint>();
                rewardList.Add(reward);
            }
        }

        if (income.ContainsKey("memory"))
        {
            JArray memory = (JArray)income["memory"];

            Dictionary<string, uint> memoryDic = new Dictionary<string, uint>();
            memoryDic.Add("point", 0);
            foreach (JArray rw in memory)
            {
                neco_cat_memory catMemory = neco_cat_memory.GetNecoMemory(rw[0].Value<uint>());
                if (catMemory == null) continue;

                if (rw[1].Value<uint>() > 0)
                {
                    memoryDic["point"] += rw[1].Value<uint>();  // 포인트로 합산
                }
                else if (memoryDic.ContainsKey(catMemory.GetNecoMemoryID().ToString()) == false)
                {
                    memoryDic.Add(catMemory.GetNecoMemoryID().ToString(), 1);
                }
            }

            foreach (var memoryPair in memoryDic)
            {
                RewardData reward = new RewardData();

                if (memoryPair.Key == "point")
                {
                    reward.point = memoryPair.Value;
                    if (reward.point > 0)
                    {
                        rewardList.Add(reward);
                    }
                }
                else
                {
                    reward.memoryData = neco_cat_memory.GetNecoMemory(uint.Parse(memoryPair.Key));
                    rewardList.Add(reward);
                }
            }
        }

        if (rewardList.Count < 2)
        {
            OnGoal();
            return;
        }

        Transform[] rewardLayer = new Transform[2];
        rewardLayer[0] = GoalRewardPopup.transform.Find("RewardInfo1");
        rewardLayer[1] = GoalRewardPopup.transform.Find("RewardInfo2");

        for (int i = 0; i < 2; i++)
        {
            Transform group = rewardLayer[i].Find("IconLayer");

            Image rewardIcon = group.Find("RewardIcon").GetComponent<Image>();
            Text rewardItemNameText = group.Find("RewardNameText").GetComponent<Text>();
            Text rewardCountText = group.Find("Count_Text").GetComponent<Text>();

            RewardData rewardData = rewardList[i];
            if (rewardData.gold > 0)
            {
                rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_coin");

                rewardItemNameText.text = LocalizeData.GetText("LOCALIZE_334");
                rewardCountText.text = string.Format("{0}", rewardData.gold.ToString("n0"));
            }
            else if (rewardData.catnip > 0)
            {
                rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_catleaf");

                rewardItemNameText.text = LocalizeData.GetText("LOCALIZE_348");
                rewardCountText.text = string.Format("{0}", rewardData.catnip.ToString("n0"));
            }
            else if (rewardData.point > 0)
            {
                rewardIcon.sprite = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_Material_point");

                rewardItemNameText.text = LocalizeData.GetText("LOCALIZE_335");
                rewardCountText.text = string.Format("{0}", rewardData.point.ToString("n0"));
            }
            else if (rewardData.itemData != null)
            {
                rewardIcon.sprite = rewardData.itemData.GetItemIcon();
                rewardItemNameText.text = rewardData.itemData.GetItemName();
                rewardCountText.text = string.Format("{0}", rewardData.count.ToString("n0"));
            }
            else if (rewardData.memoryData != null)
            {
                rewardIcon.sprite = Resources.Load<Sprite>(rewardData.memoryData.GetNecoMemoryThumbnail());
                rewardItemNameText.text = "";
                rewardCountText.text = "";
            }
        }

        GoalRewardPopup.SetActive(true);
        foreach (Transform child in EffectContainer)
        {
            DestroyImmediate(child.gameObject);
        }

        ShowEffect();
        
    }

    public void OnGoalRewardClose()
    {
        GoalRewardPopup.SetActive(false);
        OnGoal();
    }

    void ShowEffect()
    {
        if (EffectContainer.transform.childCount > 2)
            return;

        Vector2 size = (EffectContainer.transform as RectTransform).sizeDelta;
        GameObject effect = Instantiate(TargetEffect);
        effect.transform.SetParent(EffectContainer.transform);
        RectTransform rt = effect.GetComponent<RectTransform>();

        rt.localPosition = new Vector3((size.x * 0.5f) - (UnityEngine.Random.value * size.x), 10 + (size.y * 0.5f) - (UnityEngine.Random.value * size.y), TargetEffect.transform.localPosition.z);
        rt.localScale = Vector3.one;

        effect.SetActive(true);

        Invoke("ShowEffect", 0.1f);
    }
}
