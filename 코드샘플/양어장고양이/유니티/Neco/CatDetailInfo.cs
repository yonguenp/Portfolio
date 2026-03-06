using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class CatDetailInfo : MonoBehaviour
{
    const uint CHEWRR_COST = 1;  // 츄르 사용시 필요 갯수

    [Header("[Cat Detail Info]")]
    public Text catName;

    public Text catMemoryValue;
    public Text catVisitCount;

    public Image catIcon;
    public Color undiscoverColor;

    [Header("[Favorite Info]")]
    //public GameObject favoriteFoodLayer;
    public GameObject favoriteObjectLayer;

    [Header("[Cat Memory List]")]
    public GameObject catMemoryScrollContainer;
    public GameObject catMemoryCloneObject;

    [Header("[Album Button]")]
    public Button photoButton;
    public Button videoButton;
    public Color selectedColor;
    public Color unSelectedColor;

    public GameObject photoRedDot;
    public GameObject videoRedDot;

    [Header("[Chewrr Layer]")]
    public GameObject haveChewrrLayer;
    public GameObject emptyChewrrLayer;

    neco_cat curSelectedCatData;

    bool isSelectedPhoto = true;

    List<neco_cat_memory> albumList = new List<neco_cat_memory>();

    uint userChewrr;

    public void OnClickPhotoButton()
    {
        SelectPhoto();
    }

    void SelectPhoto(bool force = false)
    {
        photoButton.image.color = selectedColor;
        videoButton.image.color = unSelectedColor;

        if (force || isSelectedPhoto == false)
        {
            RefreshAlbumData(true);
        }
    }

    public void OnClickVideoButton()
    {
        SelectVideo();
    }

    void SelectVideo(bool force = false)
    {
        photoButton.image.color = unSelectedColor;
        videoButton.image.color = selectedColor;

        if (force || isSelectedPhoto)
        {
            RefreshAlbumData(false);
        }
    }

    public void OnFavoriteObjectButton()
    {
        if (curSelectedCatData.IsGainCat() == false)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_188"));
            return;
        }

        if (curSelectedCatData.GetLikeObject() == 0)
        {
            List<neco_cat_outbreak> catObjectList = neco_cat_outbreak.GetCatObjectIDList(curSelectedCatData.GetCatID());

            List<uint> validItemsID = new List<uint>();

            List<game_data> objectsList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.OBJECTS);
            foreach (neco_cat_outbreak ob in catObjectList)
            {
                validItemsID.Add(objects.GetSpotItem(ob.GetNecoOutBreakObjectID()));
            }

            if (validItemsID.Count == 0)
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("업데이트예정"));
            }
            else
            {
                curSelectedCatData.SetLikeObject(validItemsID[Random.Range(0, validItemsID.Count)]);
            }
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_189"));
        }

        favoriteObjectLayer.GetComponent<CatDetailFavoriteInfo>().SetCatDetailFavoriteInfo(curSelectedCatData);

        RefreshAlbumData(isSelectedPhoto);
    }

    public void OnClickChewrrButton()
    {
        if (curSelectedCatData.IsGainCat() == false)
        {
            NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_188"));
            return;
        }

        if (userChewrr < CHEWRR_COST)
        {
            if (neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.상점배스구매완료대사 || neco_data.GetPrologueSeq() == neco_data.PrologueSeq.사진찍기돌발대사)
            {
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("LOCALIZE_190"));
            }
            else
            {
                ConfirmPopupData param = new ConfirmPopupData();

                param.titleText = LocalizeData.GetText("LOCALIZE_191");
                param.titleMessageText = LocalizeData.GetText("LOCALIZE_192");

                param.messageText_1 = LocalizeData.GetText("LOCALIZE_193");

                NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(param, CONFIRM_POPUP_TYPE.COMMON, () => {
                    NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.CAT_DETAIL_POPUP);
                    NecoCanvas.GetPopupCanvas().OnShopListPopupShow(NecoShopPanel.SHOP_CATEGORY.CAT_LEAF);
                });                
            }

            return;
        }

        ConfirmPopupData popupData = SetConfirmPopupData();

        NecoCanvas.GetPopupCanvas().OnSystemConfirmPopupShow(popupData, CONFIRM_POPUP_TYPE.COMMON, UseChewrrItem);
    }

    public void SetCatDetailInfoData(neco_cat catData)
    {
        curSelectedCatData = catData;

        if (curSelectedCatData != null)
        {
            SetCatDetailInfoByCatState();
            SetCatFavoriteInfo();

            if (isSelectedPhoto)
                SelectPhoto(true);   // 사진 버튼 상태로 데이터 초기화
            else
                SelectVideo(true);
        }
    }

    void SetCatDetailInfoByCatState()
    {
        // 고양이 얼굴 아이콘 세팅
        var iconPath = curSelectedCatData.GetIconPath();
        catIcon.sprite = Resources.Load<Sprite>(iconPath);

        // 캣 인포에서 고양이 오픈 Ani를 보지 않을 경우 정보표시X
        bool isCatInfoOpen = curSelectedCatData.GetCatState() >= 3;

        if (curSelectedCatData.IsGainCat() && isCatInfoOpen)
        {
            catIcon.color = Color.white;
            catName.text = curSelectedCatData.GetCatName();
            catMemoryValue.text = curSelectedCatData.GetMemoryCount().ToString();
            catVisitCount.text = curSelectedCatData.GetNecoVisitCount().ToString();
        }
        else
        {
            catIcon.color = undiscoverColor;
            catName.text = "????";
            catMemoryValue.text = curSelectedCatData.GetMemoryCount().ToString();
            catVisitCount.text = curSelectedCatData.GetNecoVisitCount().ToString();
        }

        RefreshResource();
    }

    void SetCatFavoriteInfo()
    {
        //uint favoriteFoodItemID = curSelectedCatData.GetLikeFood();
        //favoriteFoodLayer.GetComponent<CatDetailFavoriteInfo>().SetCatDetailFavoriteInfo(favoriteFoodItemID);

        favoriteObjectLayer.GetComponent<CatDetailFavoriteInfo>().SetCatDetailFavoriteInfo(curSelectedCatData);

        //Dictionary<items, uint> foodDic = curSelectedCatData.IsFavoriteAte();

        //int dicCount = foodDic.Count;
        //if (foodDic.Count > BEST_FOOD_COUNT)
        //{
        //    dicCount = BEST_FOOD_COUNT;
        //}

        //var sortedFoodDic = foodDic.OrderBy(x => x.Value).Take(dicCount);

        //foreach (var food in sortedFoodDic)
        //{
        //    GameObject foodObject = Instantiate(FoodInfoCloneObject);
        //    foodObject.transform.SetParent(FoodListObject.transform);
        //    foodObject.transform.localScale = FoodInfoCloneObject.transform.localScale;
        //    foodObject.transform.localPosition = FoodInfoCloneObject.transform.localPosition;

        //    foodObject.GetComponent<CatDetailFoodInfo>().SetCatDetailFoodInfo(food.Key, food.Value);
        //}
    }

    void RefreshAlbumData(bool isPhoto)
    {
        isSelectedPhoto = isPhoto;

        foreach (Transform child in catMemoryScrollContainer.transform)
        {
            if (child.gameObject != catMemoryCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        catMemoryCloneObject.SetActive(true);

        // 앨범 데이터 로드하고 생성
        albumList.Clear();

        if (isPhoto)
        {
            albumList = neco_cat_memory.GetNecoMemoryPhotoOnly(curSelectedCatData.GetCatID());
        }
        else if (isPhoto == false)
        {
            albumList = neco_cat_memory.GetNecoMemoryMovieOnly(curSelectedCatData.GetCatID());
        }

        // 앨범 리스트 정렬
        albumList = albumList.OrderBy(x => x.GetNecoMemoryID()).ToList();
        List<uint> favoriteObject = new List<uint>();
        uint likeObjectID = 0;
        objects obInfo = objects.GetObjectInfo(curSelectedCatData.GetLikeObject());
        if(obInfo != null)
        {
            likeObjectID = obInfo.GetSpotID();
            if(likeObjectID == 26)
            {
                likeObjectID = 5;
            }
            else if(likeObjectID == 25)
            {
                likeObjectID = 20;
            }
        }

        if (likeObjectID != 0)
        {
            List<neco_cat_outbreak> co = neco_cat_outbreak.GetCatObjectIDList(curSelectedCatData.GetCatID());
            
            foreach (neco_cat_outbreak ob in co)
            {
                if(ob.GetNecoOutBreakObjectID() == likeObjectID)
                {
                    if (ob.GetNecoOutBreakType() == neco_cat.CAT_SUDDEN_STATE.PHOTO)
                    {
                        List<uint> list = neco_photo_pool_data.GetPoolList(ob.GetTarget());
                        if(list != null)
                            favoriteObject.AddRange(list);
                    }
                    else
                    {
                        favoriteObject.Add(ob.GetTarget());
                    }
                }
            }
        }

        float rotateTime = 0.1f;
        if (albumList != null)
        {
            foreach (game_data catInfoData in albumList)
            {
                neco_cat_memory memoryInfo = (neco_cat_memory)catInfoData;

                GameObject catInfoUI = Instantiate(catMemoryCloneObject);
                catInfoUI.transform.SetParent(catMemoryScrollContainer.transform);
                catInfoUI.transform.localScale = catMemoryCloneObject.transform.localScale;
                catInfoUI.transform.localPosition = catMemoryCloneObject.transform.localPosition;

                MemoryInfo memoryInfoComponent = catInfoUI.GetComponent<MemoryInfo>();
                memoryInfoComponent.SetMemoryInfo(memoryInfo, curSelectedCatData, rotateTime);
                memoryInfoComponent.SetFavoritObjectAction(favoriteObject.Contains(memoryInfo.GetNecoMemoryID()));

                rotateTime += 0.1f;
            }
        }

        catMemoryCloneObject.SetActive(false);

        catMemoryScrollContainer.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        // 버튼 레드닷 상태 갱신
        UpdateRedDotState();
    }

    ConfirmPopupData SetConfirmPopupData()
    {
        ConfirmPopupData popupData = new ConfirmPopupData();

        popupData.titleText = LocalizeData.GetText("LOCALIZE_194");
        popupData.titleMessageText = LocalizeData.GetText("LOCALIZE_195");

        popupData.messageText_1 = LocalizeData.GetText("LOCALIZE_196");

        popupData.amountIcon = Resources.Load<Sprite>("Sprites/Neco/Ui/Icon_chur");
        popupData.amountText = CHEWRR_COST.ToString("n0"); // todo bt - 추후 데이터 연동 필요

        return popupData;
    }

    void UseChewrrItem_notused()
    {
        // 츄르 아이템 사용 처리
        List<neco_cat_outbreak> catObjectList = neco_cat_outbreak.GetCatObjectIDList(curSelectedCatData.GetCatID());

        List<neco_cat_outbreak> validObjectIDList = new List<neco_cat_outbreak>();

        List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        foreach (game_data data in user_items)
        {
            user_items userItem = (user_items)data;
            items itemData = items.GetItem(userItem.GetItemID());

            if (itemData.GetItemType() == "TOY")
            {
                objects objectData = objects.GetObjectInfo(itemData.GetItemID());
                if (objectData != null)
                {
                    foreach (neco_cat_outbreak o in catObjectList)
                    {
                        if (o.GetNecoOutBreakObjectID() == objectData.GetSpotID())
                        {
                            for (int i = 0; i < o.GetRate(); i++)
                            {
                                validObjectIDList.Add(o);
                            }
                        }
                    }
                }
            }
        }

        if (validObjectIDList == null || validObjectIDList.Count <= 0)
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_197"), LocalizeData.GetText("LOCALIZE_198"));
            return;
        }

        int randomObject = Random.Range(0, validObjectIDList.Count);
        neco_cat_outbreak ob = validObjectIDList[randomObject];
        if (ob == null)
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_197"), LocalizeData.GetText("LOCALIZE_199"));
            return;
        }

        WWWForm param = new WWWForm();
        param.AddField("api", "chore");
        param.AddField("op", 1);

        param.AddField("item", 136);
        param.AddField("cnt", -1);

        NetworkManager.GetInstance().SendApiRequest("chore", 1, param, (response)=> {
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
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            uint catID = curSelectedCatData.GetCatID();
                            uint objectID = ob.GetNecoOutBreakObjectID();

                            neco_object_slots necoSlot = neco_object_slots.GetNecoObjectSlotData(objectID, curSelectedCatData.GetCatID());
                            neco_object_maps necoMap = neco_object_maps.GetNecoObjectMapData(objectID);

                            uint slotID = necoSlot.GetNecoSlot();
                            uint targetMap = necoMap.GetNecoMapID();

                            if (NecoCanvas.GetGameCanvas().curMapID != targetMap)
                            {
                                NecoCanvas.GetGameCanvas().LoadMap(targetMap);
                            }

                            NecoCanvas.GetPopupCanvas().OnPopupClose();
                            NecoCanvas.GetGameCanvas().CallCat(catID, objectID, slotID, (uint)ob.GetNecoOutBreakType());

                            Invoke("RefreshResource", 0.1f);

                            return;
                        }
                    }
                }
            }

            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_197"), LocalizeData.GetText("LOCALIZE_199"));
        });
    }

    void UseChewrrItem()
    {
        WWWForm param = new WWWForm();
        param.AddField("api", "neco");
        param.AddField("op", 4);

        param.AddField("id", curSelectedCatData.GetCatID().ToString());

        NetworkManager.GetInstance().SendApiRequest("neco", 4, param, (response) => {
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
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            uint objectID = row["oid"].Value<uint>();

                            neco_object_maps necoMap = neco_object_maps.GetNecoObjectMapData(objectID);
                            uint targetMap = necoMap.GetNecoMapID();

                            if (NecoCanvas.GetGameCanvas().curMapID != targetMap)
                            {
                                NecoCanvas.GetGameCanvas().LoadMap(targetMap);
                            }

                            NecoCanvas.GetPopupCanvas().OnPopupClose();
                            
                            Invoke("RefreshResource", 0.1f);

                            return;
                        }
                        else
                        {
                            if(rs == 5)
                            {
                                NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_197"), LocalizeData.GetText("LOCALIZE_198"));

                                return;
                            }
                        }
                    }
                }
            }

            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_197"), LocalizeData.GetText("LOCALIZE_199"));
        }, (err) => {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_197"), LocalizeData.GetText("LOCALIZE_199"));
        });
    }

    public void RefreshResource()
    {
        // 츄르 사용 재화 ui 갱신
        NecoCanvas.GetPopupCanvas().RefreshTopUILayer(TOP_UI_PANEL_TYPE.RESOURCE);

        // 츄르 사용 버튼 상태 갱신
        userChewrr = NecoCanvas.GetPopupCanvas().GetUserResourceFromTopUILayer(TOP_UI_RESOURCE_TYPE.CHEWRR);

        haveChewrrLayer.SetActive(userChewrr >= CHEWRR_COST);
        emptyChewrrLayer.SetActive(userChewrr < CHEWRR_COST);

    }

    public void UpdateRedDotState()
    {
        bool photoCheck = false;
        bool videoCheck = false;

        List<neco_cat_memory> memoryList = neco_cat_memory.GetNecoMemoryByCatID(curSelectedCatData.GetCatID());
        foreach (neco_cat_memory memory in memoryList)
        {
            string newMemoryKey = string.Format("{0}_{1}", SamandaLauncher.GetAccountNo(), memory.GetNecoMemoryID());
            if (PlayerPrefs.HasKey(newMemoryKey) && PlayerPrefs.GetInt(newMemoryKey, 0) == 0)
            {
                if (memory.GetNecoMemoryType() != "movie")
                {
                    photoCheck = true;
                }
                else
                {
                    videoCheck = true;
                }

                if (photoCheck && videoCheck) { break; }
            }
        }

        photoRedDot.SetActive(photoCheck);
        videoRedDot.SetActive(videoCheck);
    }

    private void OnDisable()
    {
        isSelectedPhoto = true;
    }
}
