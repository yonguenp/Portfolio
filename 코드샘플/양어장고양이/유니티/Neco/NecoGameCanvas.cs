using Coffee.UIEffects;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoGameCanvas : NecoCanvas
{
    float TimeChecker = 0.0f;
    int TimeOffset = 0;

    public GameObject MapPanel;
    public SkeletonGraphic MapDecoration;
    public GameObject[] MapList;
    public GameObject UI_Curtain;
    public NecoCatUpdater catUpdater;

    public uint curMapID = 0;
    private MapObjectController mapController = null;
    

    protected override void Awake()
    {
        NetworkManager.GetInstance().TimerReset = ResetTimer;

        InitDebugData();

        base.Awake();

        SamandaLauncher.OnGamePlayBeforeBannerAndNotice(() =>
        {
            SamandaLauncher.SetOnHideCallback(() => {
                MapObjectControllerPrologue prologue = mapController as MapObjectControllerPrologue;
                if (prologue != null)
                    prologue.CheckPrologueSeq();
            });

            SamandaLauncher.OnHideScreen();
            SamandaLauncher.SetOnHideCallback(null);

            GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
            if (canvas)
            {
                canvas.GetComponent<SamandaStarter>().SetBackKeyCallback(null);
            }

        });

        SamandaLauncher.SetCommandCallback(OnCommand);

        List<game_data> cardData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);

        if (cardData != null)
        {
            string accountNo = SamandaLauncher.GetAccountNo();
            foreach (user_card data in cardData)
            {
                if (data != null)
                {
                    PlayerPrefs.SetInt(string.Format("{0}_{1}_{2}", accountNo, data.GetCardID(), data.GetCardUniqueID()), 1);
                }
            }
        }

        List<neco_user_cat> userCatList = neco_user_cat.GetGainUserCatList();
        if (userCatList != null)
        {
            foreach (neco_user_cat userCat in userCatList)
            {
                List<uint> catMemoryList = userCat.GetMemories();
                foreach (uint memoryID in catMemoryList)
                {
                    PlayerPrefs.SetInt(string.Format("{0}_{1}", SamandaLauncher.GetAccountNo(), memoryID), 1);
                }
            }
        }

        NecoCanvas.GetUICanvas()?.RefreshMainMenuRedDot();
    }

    public void OnCommand(string command)
    {
        switch (command)
        {
            case "package_shop_open":
                {
                    if (neco_data.PrologueSeq.프리플레이 > neco_data.GetPrologueSeq())
                    {
                        return;
                    }

                    SamandaLauncher.SetOnHideCallback(null);
                    SamandaLauncher.OnHideScreen();
                    GameObject canvas = GameObject.Find("SAMANDA_CANVAS");
                    if (canvas)
                    {
                        canvas.GetComponent<SamandaStarter>().SetBackKeyCallback(null);
                    }

                    NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.PACKAGE);
                }
                break;
        }
    }

    void Update()
    {
        TimeChecker += Time.deltaTime;
        if (TimeChecker > 1.0f)
        {
            TimeChecker -= 1.0f;
            TimeOffset += 1;
        }
    }

    private void OnEnable()
    {
        //temp
        InitMap();
        RunCatSystem();
    }

    private void InitDebugData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_SPOT);
        if (necoData != null)
        {
            foreach (neco_spot data in necoData)
            {
                if (data != null)
                {
                    data.RefreshItem();
                }
            }
        }
    }

    private void ClearMap()
    {
        foreach(Transform map in MapPanel.transform)
        {
            if(map != mapController.transform)
                Destroy(map.gameObject);
        }

        if (mapController != null)
        {
            RectTransform rt = mapController.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;

            rt = mapController.MapBackgroundImage.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
        }
    }
    public void ResetTimer()
    {
        TimeChecker = 0.0f;
        TimeOffset = 0;
    }

    public int GetCurTime()
    {
        return NetworkManager.GetInstance().ServerTime + TimeOffset;
    }

    public void RunCatSystem()
    {
        Debug.Log("Stop Coroutine : RunCatSystem");
        StopAllCoroutines();

        if (neco_data.Instance.GetNextObjectUpdate() > 0)
        {
            StartCoroutine(ObjectUpdateCoroutine());
        }
    }

    public void InitMap()
    {
        //todo 현재 유저 타깃 맵 번호 세팅

        LoadMap(curMapID);
    }

    public void LoadMap(uint mapID)
    {
        curMapID = mapID;
        
        LoadMap();
    }

    private void LoadMap()
    {
        if(mapController != null)
            GetUICanvas()?.gameObject.SetActive(true);

        GetPopupCanvas()?.gameObject.SetActive(true);

        neco_map mapData = neco_map.GetNecoMap(curMapID);

        if(MapList.Length <= curMapID || MapList[curMapID] == null)
        {
            Debug.LogError("이상한 맵아이디 발견 : " + curMapID.ToString());
            return;
        }

        GameObject mapObject = Instantiate(MapList[curMapID].gameObject);
        mapObject.transform.SetParent(MapPanel.transform);

        RectTransform rt = mapObject.GetComponent<RectTransform>();
        rt.localPosition = Vector3.zero;
        rt.localScale = Vector3.one;

        MapObjectController prevController = mapController;

        mapController = mapObject.GetComponent<MapObjectController>();
        mapController.OnInitMap(mapData);

        if (prevController == null)
        {
            GetUICanvas().RefreshNewCatAlarm();

            neco_data.PrologueSeq seq = (neco_data.PrologueSeq)((uint)GameDataManager.Instance.GetUserData().data["contents"]);
            if (neco_data.PrologueSeq.프리플레이 <= seq)
            {
                WWWForm data = new WWWForm();
                data.AddField("api", "attendance");
                data.AddField("op", 1);

                NetworkManager.GetInstance().SendApiRequest("attendance", 1, data, (response) =>
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
                        if (uri == "attendance")
                        {
                            Debug.Log(row);

                            JToken resultCode = row["rs"];
                            if (resultCode != null && resultCode.Type == JTokenType.Integer)
                            {
                                int rs = resultCode.Value<int>();
                                if (rs == 0)
                                {
                                    if (row["today"].Value<uint>() > 0)
                                    {
                                        NecoCanvas.GetPopupCanvas().OnPopupShow(NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP);
                                    }
                                    else
                                    {
                                        NecoAttendancePopup popup = NecoCanvas.GetPopupCanvas().PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP].GetComponent<NecoAttendancePopup>();
                                        if(popup)
                                        {
                                            popup.SetAttendanceData(response);
                                        }

                                        NecoCanvas.GetPopupCanvas().OnCheckEventAttendance();
                                    }
                                }
                                else
                                {
                                    string msg = rs.ToString();
                                    switch (rs)
                                    {
                                        case 1: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                        case 2: msg = LocalizeData.GetText("LOCALIZE_316"); break;
                                    }

                                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                }
                            }
                        }
                    }
                });                
            }

        }
        else
        {
            GetUICanvas().OnUIClose(NecoUICanvas.UI_TYPE.NEW_CAT_ALARM);

            Button[] button = prevController.GetComponentsInChildren<Button>();
            foreach(Button b in button)
            {
                b.interactable = false;
            }

            List<uint> mapList = new List<uint>();
            if (neco_map.GetNecoMap(8).IsOpened())
                mapList.Add(8);
            if (neco_map.GetNecoMap(9).IsOpened())
                mapList.Add(9);
            if (neco_map.GetNecoMap(6).IsOpened())
                mapList.Add(6);
            if (neco_map.GetNecoMap(10).IsOpened())
                mapList.Add(10);
            if (neco_map.GetNecoMap(1).IsOpened())
                mapList.Add(1);
            if (neco_map.GetNecoMap(2).IsOpened())
                mapList.Add(2);
            if (neco_map.GetNecoMap(3).IsOpened())
                mapList.Add(3);
            if (neco_map.GetNecoMap(4).IsOpened())
                mapList.Add(4);
            if (neco_map.GetNecoMap(5).IsOpened())
                mapList.Add(5);
            if (neco_map.GetNecoMap(7).IsOpened())
                mapList.Add(7);

            List<uint> leftPool = new List<uint>();
            List<uint> rightPool = new List<uint>();
            uint id = prevController.GetMapData().GetMapID();
            for(int i = 0; i < mapList.Count; i++)
            {
                if(mapList[i] == id)
                {
                    for (int j = 1; j <= 2; j++)
                    {
                        rightPool.Add(mapList[(i + j) % mapList.Count]);
                        leftPool.Add(mapList[(mapList.Count + (i - j)) % mapList.Count]);
                    }
                }
            }

            Vector2 size = prevController.MapBackgroundImage.sizeDelta;
            float half = size.x / 2 * prevController.MapBackgroundImage.localScale.x;
            
            if (rightPool.Contains(mapController.GetMapData().GetMapID()))
            {   
                rt.localPosition = new Vector3(half, 0, 0);
                prevController.MapBackgroundImage.DOLocalMoveX(-half, 0.2f).OnComplete(ClearMap);
                mapController.MapBackgroundImage.DOLocalMoveX(-half, 0.2f).OnComplete(() => {
                    rt = mapController.MapBackgroundImage.GetComponent<RectTransform>();
                    rt.localPosition = Vector3.zero;
                });
            }
            else
            {
                rt.localPosition = new Vector3(-half, 0, 0);
                prevController.MapBackgroundImage.DOLocalMoveX(half, 0.2f).OnComplete(ClearMap);
                mapController.MapBackgroundImage.DOLocalMoveX(half, 0.2f).OnComplete(() => {
                    rt = mapController.MapBackgroundImage.GetComponent<RectTransform>();
                    rt.localPosition = Vector3.zero;
                });
            }

            if(Camera.main.GetComponent<SuperBlur.SuperBlurBase>().enabled == false)
            {
                GetPopupCanvas().BlurActionForMapChange();
            }

            GetUICanvas().Invoke("RefreshNewCatAlarm", 0.5f);
        }

        if (MapDecoration != null)
        {
            MapDecoration.Initialize(false);

            CancelInvoke("OnDecorationAnimation");
            MapDecoration.AnimationState.SetAnimation(0, "animation", false);
            MapDecoration.AnimationState.TimeScale = 1.0f;
            Invoke("OnDecorationAnimation", 0.6f);
        }
    }

    public void OnDecorationAnimation()
    {
        CancelInvoke("OnDecorationAnimation");
        MapDecoration.AnimationState.SetAnimation(0, "animation", false);
        MapDecoration.AnimationState.TimeScale = UnityEngine.Random.Range(0.05f, 0.3f);
        Invoke("OnDecorationAnimation", 0.6f * (1.0f / MapDecoration.AnimationState.TimeScale));
    }

    public void OnSetFoods(List<neco_spot> spots, items item, Action callback = null)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "feed");
        data.AddField("op", 1);
        data.AddField("auto", 1);
        data.AddField("item", item.GetItemID().ToString());
        List<string> param = new List<string>();
        foreach(neco_spot spot in spots)
        {
            param.Add(spot.GetSpotID().ToString());
        }
        data.AddField("oid", string.Join(",", param));

        NetworkManager.GetInstance().SendPostSimple("feed", 1, data, (response) =>
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
                if (uri == "feed")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RunCatSystem", 0.1f);
                            //if(neco_data.DebugSeq == 0)
                            //{
                            //    if (!neco_data.ShownFlag.Contains(1131))
                            //    {
                            //        CallCat(1, 1, 3, 1);
                            //    }
                            //}
                            //if (neco_data.DebugSeq == 1)
                            //{
                            //    if (!neco_data.ShownFlag.Contains(2111))
                            //    {
                            //        CallCat(2, 1, 2, 1);
                            //    }
                            //}
                            //if (neco_data.DebugSeq == 2)
                            //{
                            //    if (!neco_data.ShownFlag.Contains(3131))
                            //    {
                            //        CallCat(3, 1, 3, 1);
                            //    }                                
                            //}


                            if (mapController != null)
                            {
                                mapController.RefreshCatSilhouettes();
                            }

                            if (row.ContainsKey("rew"))
                            {
                                JObject income = (JObject)row["rew"];
                                if (income.ContainsKey("gold"))
                                {
                                    NecoCanvas.GetUICanvas()?.OnCatVisitCoin(0, income["gold"].Value<uint>());
                                }
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_313"); break;
                            }

                            if (PlayerPrefs.GetInt("AUTO_DISPENSER") > 0)
                            {
                                PlayerPrefs.SetInt("AUTO_DISPENSER", 0);
                                msg += "\n" + LocalizeData.GetText("LOCALIZE_258");
                            }
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), msg);
                        }
                    }
                }
                if (uri == "user")
                {
                    NetworkManager.GetInstance().OnResponseUser(row);
                    NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                }
                if (uri == "item")
                {
                    NetworkManager.GetInstance().OnResponseItem(row);
                }
                if (uri == "object")
                {
                    NetworkManager.GetInstance().OnResponseNecoObject(row);
                }
            }

            callback?.Invoke();
        });
    }
    public void OnSetFood(neco_spot spot, items item)
    {
        neco_map mapData = neco_map.GetNecoMap(curMapID);
        if (mapData == null)
        {
            return;
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "feed");
        data.AddField("op", 1);
        data.AddField("item", item.GetItemID().ToString());
        data.AddField("oid", spot.GetSpotID().ToString());

        NetworkManager.GetInstance().SendApiRequest("feed", 1, data, (response) =>
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
                if (uri == "feed")
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("RunCatSystem", 0.1f);
                            //if(neco_data.DebugSeq == 0)
                            //{
                            //    if (!neco_data.ShownFlag.Contains(1131))
                            //    {
                            //        CallCat(1, 1, 3, 1);
                            //    }
                            //}
                            //if (neco_data.DebugSeq == 1)
                            //{
                            //    if (!neco_data.ShownFlag.Contains(2111))
                            //    {
                            //        CallCat(2, 1, 2, 1);
                            //    }
                            //}
                            //if (neco_data.DebugSeq == 2)
                            //{
                            //    if (!neco_data.ShownFlag.Contains(3131))
                            //    {
                            //        CallCat(3, 1, 3, 1);
                            //    }                                
                            //}


                            if (mapController != null)
                            {
                                mapController.RefreshCatSilhouettes();
                            }
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_313"); break;
                            }

                            if (PlayerPrefs.GetInt("AUTO_DISPENSER") > 0)
                            {
                                PlayerPrefs.SetInt("AUTO_DISPENSER", 0);
                                msg += "\n" + LocalizeData.GetText("LOCALIZE_258");
                            }
                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), msg);
                        }
                    }
                }
            } 
        });
        
    }


    public IEnumerator ObjectUpdateCoroutine()
    {
        if (neco_data.Instance.GetNextObjectUpdate() > 0)
        {
            if (neco_data.Instance.GetNextObjectUpdate() > GetCurTime())
            {
                uint checker = 5;
                
                while(neco_data.Instance.GetNextObjectUpdate() - (uint)GetCurTime() > checker)
                {
                    Debug.Log("Object next update Remain : " + (neco_data.Instance.GetNextObjectUpdate() - (uint)GetCurTime()).ToString() + " Sec..");                    
                    yield return new WaitForSeconds(checker);
                }

                Debug.Log("Object next update Remain : " + (neco_data.Instance.GetNextObjectUpdate() - (uint)GetCurTime()).ToString() + " Sec..");
                yield return new WaitForSeconds(neco_data.Instance.GetNextObjectUpdate() - (uint)GetCurTime());
            }
            
            neco_data.Instance.SetNextObjectUpdate(0);
            Debug.Log("Object update now !");

            WWWForm data = new WWWForm();
            data.AddField("api", "object");
            data.AddField("op", 1);

            NetworkManager.GetInstance().SendPostSimple("object", 1, data, (response) =>
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
                    if (uri == "object")
                    {
                        NetworkManager.GetInstance().OnResponseNecoObject(row);

                        JToken opCode = row["op"];
                        if (opCode != null && opCode.Type == JTokenType.Integer)
                        {
                            int op = opCode.Value<int>();
                            switch (op)
                            {
                                case 1:
                                {
                                    JToken resultCode = row["rs"];
                                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                    {
                                        int rs = resultCode.Value<int>();
                                        if (rs == 0)
                                        {
                                            RunCatSystem();
                                        }
                                        else
                                        {
                                            string msg = rs.ToString();
                                            switch (rs)
                                            {
                                                case 1: msg = LocalizeData.GetText("LOCALIZE_499"); break;
                                                case 2: msg = LocalizeData.GetText("LOCALIZE_502"); break;
                                                case 3: msg = LocalizeData.GetText("LOCALIZE_278"); break;
                                                case 4: msg = LocalizeData.GetText("LOCALIZE_504"); break;
                                            }

                                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    if (uri == "user")
                    {
                        NetworkManager.GetInstance().OnResponseUser(row);
                        NecoCanvas.GetUICanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);
                    }
                }
                
            });
        }
    }

    public void RefreshSpots()
    {
        neco_map mapData = neco_map.GetNecoMap(curMapID);
        if (mapData != null)
        {
            List<neco_spot> spots = mapData.GetSpots();
            if (spots != null)
            {
                foreach (neco_spot spot in spots)
                {
                    spot.Refresh();
                }
            }
        }
    }

    //public void TmpCraftingItem()
    //{
    //    neco_spot.GetNecoSpot(2).SetItem(items.GetItem(106));

    //    LoadForegroundMap();
    //}


    public void LoadForegroundMap()
    {
        //LoadMap(1);
    }

    public void CallCat(uint catid, uint spotid, uint slotid, uint sudden, Action cb = null)
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "neco");
        data.AddField("op", 1);
        data.AddField("id", catid.ToString());
        data.AddField("oid", spotid.ToString());
        data.AddField("slot", slotid.ToString());
        data.AddField("sudden", sudden.ToString());

        NetworkManager.GetInstance().SendApiRequest("neco", 1, data, (response) =>
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
                if (uri == "neco")
                {
                    JToken opCode = row["op"];
                    if (opCode != null && opCode.Type == JTokenType.Integer)
                    {
                        int op = opCode.Value<int>();
                        switch (op)
                        {
                            case 1:
                                {
                                    JToken resultCode = row["rs"];
                                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                                    {
                                        int rs = resultCode.Value<int>();
                                        if (rs == 0)
                                        {
                                            Invoke("RefreshSpots", 0.1f);
                                            Invoke("RunCatSystem", 0.1f);

                                            neco_data.ShownFlag.Add(uint.Parse(catid.ToString() + spotid.ToString() + slotid.ToString() + sudden.ToString()));

                                            cb?.Invoke();
                                        }
                                        else
                                        {
                                            string msg = rs.ToString();
                                            switch (rs)
                                            {
                                                case 1: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                                case 2: msg = LocalizeData.GetText("LOCALIZE_338"); break;
                                                case 3: msg = LocalizeData.GetText("LOCALIZE_505"); break;
                                                case 4: msg = LocalizeData.GetText("LOCALIZE_333"); break;
                                            }

                                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                                        }
                                    }

                                }
                                break;
                        }
                    }
                }
            }
        });
    }

    public void OnUICurtain(bool on)
    {
        UI_Curtain.SetActive(true);

        Image image = UI_Curtain.GetComponent<Image>();
        image.DOKill();

        if (on)
        {
            image.DOColor(Color.black, 0.1f);
            NecoCanvas.GetUICanvas().gameObject.SetActive(false);
        }
        else
        {
            image.DOColor(new Color(0f, 0f, 0f, 0f), 0.1f).OnComplete(() => {
                UI_Curtain.SetActive(false);
                NecoCanvas.GetUICanvas().gameObject.SetActive(true);
            });
        }
    }

    public MapObjectController GetCurMapController()
    {
        return mapController;
    }

    public void RefreshCat(neco_cat cat)
    {
        if (mapController != null)
            mapController.RefreshCat(cat);
    }

    public void OnMoveLeftMap()
    {
        if (!GetUICanvas().IsMapMovable())
        {
            return;
        }

        if (mapController != null)
        {
            if (!mapController.IsMovableMap())
            {
                //GetPopupCanvas().OnToastPopupShow("새로운 고양이가 있습니다.");
                return;
            }
        }

        if (IsMovableLeft())
        {
            LoadMap(GetNextMovableMap(-1));
            GetUICanvas().RefreshNewCatAlarm();
        }
    }
    public void OnMoveRightMap()
    {
        if (!GetUICanvas().IsMapMovable())
        {
            return;
        }

        if (mapController != null)
        {
            if (!mapController.IsMovableMap())
            {
                //GetPopupCanvas().OnToastPopupShow("새로운 고양이부터 만나보세요.");
                return;
            }
        }

        if (IsMovableRight())
        {
            LoadMap(GetNextMovableMap(1));
            GetUICanvas().RefreshNewCatAlarm();
        } 
    }

    public bool IsMovableLeft()
    {
        return GetNextMovableMap(-1) != curMapID;
    }

    public bool IsMovableRight()
    {
        return GetNextMovableMap(1) != curMapID;
    }

    public uint GetNextMovableMap(int dir) // -1 left 1 right
    {
        List<uint> mapList = new List<uint>();
        if(neco_map.GetNecoMap(8).IsOpened())
            mapList.Add(8);
        if (neco_map.GetNecoMap(9).IsOpened())
            mapList.Add(9);
        if (neco_map.GetNecoMap(6).IsOpened())
            mapList.Add(6);
        if (neco_map.GetNecoMap(10).IsOpened())
            mapList.Add(10);
        if (neco_map.GetNecoMap(1).IsOpened())
            mapList.Add(1);
        if (neco_map.GetNecoMap(2).IsOpened())
            mapList.Add(2);
        if (neco_map.GetNecoMap(3).IsOpened())
            mapList.Add(3);
        if (neco_map.GetNecoMap(4).IsOpened())
            mapList.Add(4);
        if (neco_map.GetNecoMap(5).IsOpened())
            mapList.Add(5);
        if (neco_map.GetNecoMap(7).IsOpened())
            mapList.Add(7);

        int next = -1;

        uint ret = curMapID;
        for (int i = 0; i < mapList.Count; i++)
        {
            if (mapList[i] == ret)
            {
                for (int j = 0; j < (mapList.Count / 2); j++)
                {
                    next = i + (dir * (j + 1));

                    if (next < 0)
                    {
                        ret = mapList[mapList.Count + next];
                    }
                    else if (next >= mapList.Count)
                    {
                        ret = mapList[next - mapList.Count];
                    }
                    else
                    {
                        ret = mapList[next];
                    }
                                        
                    return ret;
                    //오브젝트있는 맵만열어줄까?
                    //foreach (neco_spot spot in neco_map.GetNecoMap(ret).GetSpots())
                    //{
                    //    if (spot.GetCurItem() != null)
                    //    {
                    //        return ret;
                    //    }
                    //}
                }

                return curMapID;
            }
        }

        return curMapID;
    }

    public void ResetBackgroundSize(Vector2 size)
    {
        MapDecoration.transform.DOLocalMoveY((size.y / 2) - (100.0f * (size.y / 1280.0f)), 0.1f);
        MapDecoration.transform.localScale = Vector3.one * (size.y / 1280.0f);
        //OnDecorationAnimation();
    }

    void OnApplicationFocus(bool hasFocus)
    {
#if UNITY_EDITOR
        return;
#endif
        if (hasFocus)
        {
            RefreshSpots();
            RunCatSystem();
            catUpdater.StartNecoCatUpdate();

            CheckAttendance();
        }
    }

    public void CheckAttendance()
    {
        NecoPopupCanvas popupCanvas = NecoCanvas.GetPopupCanvas();
        if (popupCanvas == null)
        {
            return;
        }

        NecoAttendancePopup popup = popupCanvas.PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP].GetComponent<NecoAttendancePopup>();
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
        if (neco_data.PrologueSeq.프리플레이 <= seq)
        {
            if (popup != null && popup.IsCheckAble())
            {
                popupCanvas.OnPopupClose();
                popupCanvas.OnPopupShow(NecoPopupCanvas.POPUP_TYPE.ATTENDANCE_POPUP);
            }
            else
            {
                //if (popupCanvas.PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.CHUSEOK_EVENT].GetComponent<ChuseokUI>().EnableAttendance())
                //{
                //    popupCanvas.OnPopupClose();
                //    NecoCanvas.GetPopupCanvas().OnCheckEventAttendance();
                //}

                if (popupCanvas.PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.HALLOWEEN_POPUP].GetComponent<HalloweenAttendancePanel>().EnableAttendance())
                {
                    popupCanvas.OnPopupClose();
                    NecoCanvas.GetPopupCanvas().OnCheckEventAttendance();
                }
            }
        }
    }
}
