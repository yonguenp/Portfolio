using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapObjectSpot : MonoBehaviour
{
    public uint SpotID;

    public GameObject Noraml_State;
    public GameObject Object_State;
    public GameObject Appear_State;
    public NecoCatSpotContainer[] CatSpot = new NecoCatSpotContainer[3];

    [Header("[Food Plate Time]")]
    public GameObject foodRemainCountObject;


    public GameObject EmptyDurabilityIcon;

    private Coroutine coroutineFoodTimeCount = null;

    protected GameObject[] MapObject = new GameObject[(int)neco_spot.SPOT_STATE.STATE_MAX];

    protected neco_spot curSpotData = null;
    protected neco_spot.SPOT_STATE curUIState = neco_spot.SPOT_STATE.UNKNOWN;

    public virtual uint GetSpotID()
    {
        return SpotID;
    }

    protected virtual void Awake()
    {
        MapObject[(int)neco_spot.SPOT_STATE.NOTHING] = Noraml_State;
        MapObject[(int)neco_spot.SPOT_STATE.OBJECT_SET] = Object_State;
        MapObject[(int)neco_spot.SPOT_STATE.ON_CAT] = Appear_State;
    }

    private void OnDestroy()
    {
        if (curSpotData != null)
        {
            if(curSpotData.GetUI() == this)
                curSpotData.SetUI(null);
        }
    }

    public void OnInitSpot()
    {
        BowlAnimation(false);
        OnInitObjectSpot(neco_spot.GetNecoSpot(SpotID));
    }

    public void OnInitObjectSpot(neco_spot spotData)
    {
        ClearMapObject();

        curSpotData = spotData;

        if (curSpotData != null)
        {
            curSpotData.SetUI(this);
        }
    }

    public virtual void RefreshSpot()
    {
        CancelInvoke("RefreshSpot");
        ClearMapObject();

        if (curSpotData == null)
            return;

        int index = -1;
       

        if (curSpotData.GetSpotType() == neco_spot.SPOT_TYPE.FOOD_SPOT)
        {
            neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();
            
            index = (int)curSpotData.GetSpotState();
            //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();
            bool refresh = true;
            if (seq > neco_data.PrologueSeq.첫밥주기완료 && seq <= neco_data.PrologueSeq.고양이10번터치가이드퀘스트)
                refresh = false;
            
            foreach (GameObject foodObj in MapObject)
            {
                if (foodObj != null && refresh)
                {
                    Image image = foodObj.GetComponent<Image>();
                    Sprite sprite = null;
                    bool isFilledAnimaiton = false;
                    bool hasFood = curSpotData.GetCurItem() != null && curSpotData.GetItemRemain() > 0;
                    if (hasFood)
                    {
                        sprite = neco_food.GetFoodImage(curSpotData.GetCurItem().GetItemID());

                        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배스구이밥그릇에담음 && neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.챕터2끝)
                        {
                            Debug.Log("강제 밥그릇 갱신 중단");
                        }
                        else
                        {
                            Invoke("RefreshSpot", curSpotData.GetItemRemain());
                        }
                        //RefreshFoodData();        // 05.18 - 밥그릇 시간 표시 off

                        //if (neco_data.ClientDEBUG_Seq.MOTHER_FOOD == seq)
                        //{
                        //    neco_data.SetDebugSeq(neco_data.ClientDEBUG_Seq.MOTHER_ACTION);
                        //    seq = neco_data.ClientDEBUG_Seq.MOTHER_ACTION;
                        //}


                        isFilledAnimaiton = curSpotData.GetCurItem().GetItemID() == 55;
                    }
                    else
                    {
                        if (neco_data.GetPrologueSeq() >= neco_data.PrologueSeq.배스구이밥그릇에담음 && neco_data.GetPrologueSeq() <= neco_data.PrologueSeq.챕터2끝)
                        {
                            sprite = neco_food.GetFoodImage(80);
                            hasFood = true;
                            index = (int)neco_spot.SPOT_STATE.OBJECT_SET;

                        }
                    }

                    BowlAnimation(!hasFood);
                    

                    image.DOKill();
                    image.sprite = sprite;

                    if (sprite != null)
                    {
                        image.SetNativeSize();

                        bool isNew = (int)curSpotData.GetItemMaxDuration() - (int)curSpotData.GetItemRemain() < 2;
                        if (isFilledAnimaiton && index != (int)neco_spot.SPOT_STATE.OBJECT_SET)
                        {
                            isNew = false;
                        }

                        if (isNew)
                        {
                            image.color = new Color(1f, 1f, 1f, 0f);
                            image.DOColor(Color.white, 1.0f);

                            if (isFilledAnimaiton)
                            {
                                image.fillAmount = 0.0f;
                                image.transform.localPosition = new Vector3(0f, 13f, 0f);
                                image.DOFillAmount(1.0f, 1.0f);
                            }
                            else
                            {
                                image.fillAmount = 1.0f;
                                image.transform.localPosition = new Vector3(0f, 43f, 0f);
                                image.transform.DOLocalMoveY(13f, 1.0f);
                            }
                        }
                        else
                        {
                            image.color = Color.white;
                        }
                    }
                    else
                    {
                        image.color = new Color(1f, 1f, 1f, 0f);
                    }
                }
            }

            //if (neco_data.ClientDEBUG_Seq.MOTHER_FOOD == seq || neco_data.ClientDEBUG_Seq.FOOD_TOUCH_1ST == seq || neco_data.ClientDEBUG_Seq.FOOD_TOUCH_2ND == seq)
            //{
            //    transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            //}
            //else
            //{
            //    transform.DOKill();
            //    transform.localScale = Vector3.one;
            //}
        }
        else
        {
            switch (curSpotData.GetSpotState())
            {
                case neco_spot.SPOT_STATE.UNKNOWN:
                    //gameObject.SetActive(false);
                    return;
                case neco_spot.SPOT_STATE.NOTHING:
                    index = (int)neco_spot.SPOT_STATE.NOTHING;
                    break;
                case neco_spot.SPOT_STATE.OBJECT_SET:
                    index = (int)neco_spot.SPOT_STATE.OBJECT_SET;
                    break;
                case neco_spot.SPOT_STATE.ON_CAT:
                    index = (int)neco_spot.SPOT_STATE.ON_CAT;
                    break;
            }

            if(EmptyDurabilityIcon != null)
            {
                //아이콘 제작안됨.
                EmptyDurabilityIcon.SetActive(false);

                //EmptyDurabilityIcon.SetActive(curSpotData.GetSpotItemDurability() <= 0 && curSpotData.GetCurItem() != null);
                //if(EmptyDurabilityIcon.activeSelf)
                //{
                //    Button button = EmptyDurabilityIcon.GetComponent<Button>();
                //    if(button == null)
                //    {
                //        button = EmptyDurabilityIcon.AddComponent<Button>();
                //        button.onClick.AddListener(()=> {

                //            recipe recipeData = null;
                //            List<game_data> recipelist = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
                //            if (recipelist != null)
                //            {

                //                foreach (recipe data in recipelist)
                //                {
                //                    if (data.GetOutputItem(0).Key == curSpotData.GetCurItem().GetItemID())
                //                    {
                //                        recipeData = data;
                //                    }
                //                }
                //            }

                //            if(recipeData == null)
                //            {
                //                return;
                //            }

                //            NecoCanvas.GetPopupCanvas().OnLevelupPopupShow(SUPPLY_UI_TYPE.OBJECT, ()=> {
                //                Invoke("RefreshSpot", 0.1f);
                //            }, recipeData);
                //        });
                //    }
                //}
            }
        }

        curUIState = (neco_spot.SPOT_STATE)index;
        
        if (MapObject[index] != null)
        {
            MapObject[index].SetActive(true);            
        }

        if (curUIState == neco_spot.SPOT_STATE.ON_CAT)
        {
            neco_cat cat = curSpotData.GetCurSpotCat(2);
            if(cat == null)
            {
                if (MapObject[(int)neco_spot.SPOT_STATE.OBJECT_SET] != null)
                {
                    MapObject[(int)neco_spot.SPOT_STATE.OBJECT_SET].SetActive(true);
                }
            }
        }

        foreach (NecoCatSpotContainer catContiner in CatSpot)
        {
            catContiner.RefreshCat(curSpotData);
        }
    }

    public void OnSpotSelect()
    {
        if (curSpotData == null)
            return;

        if (OnCheckPlace())
            return;

        switch (curUIState)
        {
            case neco_spot.SPOT_STATE.UNKNOWN:
                return;
            case neco_spot.SPOT_STATE.NOTHING:
                OnOpenEnableObjectList();
                return;
            case neco_spot.SPOT_STATE.OBJECT_SET:
                OnOpenCurObjectStatus();
                return;
            case neco_spot.SPOT_STATE.ON_CAT:
                //OnCatStatus();
                return;
        }
    }

    public void OnCatSelect()
    {
        //OnCatStatus();
    }

    protected void ClearMapObject()
    {
        foreach (GameObject mapObj in MapObject)
        {
            if (mapObj != null)
                mapObj.SetActive(false);
        }
    }

    private void OnOpenEnableObjectList()
    {
        switch (curSpotData.GetSpotType())
        {
            case neco_spot.SPOT_TYPE.UNKNOWN:
                return;
            case neco_spot.SPOT_TYPE.FOOD_SPOT:
                OnFoodStatus();
                return;
            case neco_spot.SPOT_TYPE.OBJECT_SPOT:
                OnObjectList();
                return;
        }
    }

    private void OnFoodList()
    {

    }

    private void OnObjectList()
    {
        //popup object list
    }

    private void OnOpenCurObjectStatus()
    {
        switch (curSpotData.GetSpotType())
        {
            case neco_spot.SPOT_TYPE.UNKNOWN:
                return;
            case neco_spot.SPOT_TYPE.FOOD_SPOT:
                OnFoodStatus();
                return;
            case neco_spot.SPOT_TYPE.OBJECT_SPOT:
                OnObjectStatus();
                return;
        }
    }

    public void OnFoodStatus()
    {
        if (CheckPrologueWithToastAlarm()) { return; }

        NecoCanvas.GetPopupCanvas().OnFeedPopupShow(curSpotData);

        //neco_data.ClientDEBUG_Seq seq = neco_data.GetDebugSeq();
        //if (neco_data.ClientDEBUG_Seq.FOOD_TOUCH_1ST == seq)
        //{
        //    neco_data.SetDebugSeq(neco_data.ClientDEBUG_Seq.FISH_TRAP_BUTTON_OPEN);
        //}
    }

    private void OnObjectStatus()
    {
        //popup object status
    }

    private bool OnCheckPlace()
    {
        //if (CatGiftIcon != null && CatGiftIcon.activeSelf)
        //{
        //    if (curSpotData.GetGiftHistory().Count > 0)
        //    {
        //        NecoCanvas.GetGameCanvas().GetGiftItem(curSpotData, CatGiftIcon.transform);
        //        return true;
        //    }
        //}

        return false;
    }

    void CheckCatAction()
    {
        //bool enabledCatAction = false;
        //if (curSpotData.GetSpotState() == neco_spot.SPOT_STATE.ON_CAT)
        //{
        //    neco_cat cat = curSpotData.GetCurSpotCat();
        //    if (cat != null)
        //    {
        //        enabledCatAction = cat.IsActionEnable();
        //    }
        //}

        //if (CatEventActionIcon != null)
        //{
        //    CatEventActionIcon.SetActive(enabledCatAction);
        //}
    }

    void RefreshFoodData()
    {
        if (foodRemainCountObject == null) 
        {
            RefreshSpot();
            return; 
        }

        if (coroutineFoodTimeCount != null)
        {
            StopCoroutine(coroutineFoodTimeCount);
        }

        coroutineFoodTimeCount = StartCoroutine(RefreshFoodTime());
    }

    IEnumerator RefreshFoodTime()
    {
        foodRemainCountObject.SetActive(true);

        while (curSpotData.GetItemRemain() > 0)
        {
            foodRemainCountObject.GetComponent<FoodRemainTimeLayer>().SetRemainTime(curSpotData.GetItemRemain());
            yield return new WaitForSeconds(1.0f);
        }

        foodRemainCountObject.SetActive(false);
        RefreshSpot();
    }

    public void StopFoodTimerForPrologue()
    {
        if (foodRemainCountObject == null) { return; }

        if (coroutineFoodTimeCount != null)
        {
            StopCoroutine(coroutineFoodTimeCount);
        }
    }

    public IEnumerator RefreshFoodTimeForPrologue()
    {
        foodRemainCountObject.SetActive(true);
        uint time = 10;
        if(curSpotData.GetItemRemain() > time)
        {
            time = curSpotData.GetItemRemain();
        }

        while (time > 0)
        {
            foodRemainCountObject.GetComponent<FoodRemainTimeLayer>().SetRemainTime(time--);
            yield return new WaitForSeconds(1.0f);
        }

        curSpotData.GoneSpotCat(2);
        foodRemainCountObject.SetActive(false);

        neco_spot.GetNecoSpot(1).SetItem(null);
        neco_spot.GetNecoSpot(1).SetItemRemainTick(0);

        RefreshSpot();
    }

    [ContextMenu("BowlAnimation")]
    public void BowlAnimation()
    {
        BowlAnimation(true);
    }

    public void BowlAnimation(bool play)
    {
        CancelInvoke("BowlAnimation");
        if (NecoCanvas.GetGameCanvas() == null)
            return;
        
        bool isEquiped = PlayerPrefs.GetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_142", 0) > 0 || PlayerPrefs.GetInt("SKIN_" + SamandaLauncher.GetAccountNo() + "_155", 0) > 0;
        bool isLoading = (NecoCanvas.GetGameCanvas().MapPanel.transform.childCount > 1);
        bool animPlay = !isLoading && play && !isEquiped;

        Transform foodObjectSpot = transform.Find("foodObjectSpot");
        if (foodObjectSpot && foodObjectSpot.gameObject.activeSelf)  
        {
            Transform bowl = foodObjectSpot.Find("emptyBowl");
            if (bowl)
            {
                RectTransform rt = bowl as RectTransform;
                rt.localRotation = Quaternion.identity;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;

                Animation anim = bowl.GetComponent<Animation>();
                if (anim)
                {
                    if (animPlay)
                    {
                        anim.enabled = true;
                        anim.Play("Bowl_ani");
                    }
                    else
                    {
                        anim.Stop();
                        anim.enabled = false;
                    }
                }
            }

            Transform emptyDispenser = foodObjectSpot.Find("emptyDispenser");
            if (emptyDispenser && emptyDispenser.gameObject.activeSelf)
            {
                Animation anim = emptyDispenser.GetComponent<Animation>();
                if (anim)
                {
                    RectTransform rt = emptyDispenser as RectTransform;
                    rt.localRotation = Quaternion.identity;
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;

                    Transform imageTransform = emptyDispenser.Find("Image");
                    if(imageTransform)
                    {
                        rt = imageTransform as RectTransform;
                        rt.localRotation = Quaternion.identity;
                        rt.anchoredPosition = Vector2.zero;
                        rt.localScale = Vector3.one;
                    }

                    Transform shadowTransform = emptyDispenser.Find("shadow");
                    if (shadowTransform)
                    {
                        rt = shadowTransform as RectTransform;
                        rt.sizeDelta = new Vector2(234,40);
                    }

                    if (animPlay)
                    {
                        anim.enabled = true;
                        anim.Play("Foodbowl_2_ani");
                    }
                    else
                    {
                        anim.Stop();
                        anim.enabled = false;
                    }
                }
            }

            if (play)
            {
                if(isLoading)
                    Invoke("BowlAnimation", 1.0f);
                else
                    Invoke("BowlAnimation", 3.0f);
            }
        }
    }

    bool CheckPrologueWithToastAlarm()
    {
        neco_data.PrologueSeq seq = neco_data.GetPrologueSeq();

        switch (seq)
        {
            case neco_data.PrologueSeq.길막이낚시장난감배치:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.길막이만지기돌발발생:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
            case neco_data.PrologueSeq.사진찍기돌발대사:
                NecoCanvas.GetPopupCanvas().OnToastPopupShow(LocalizeData.GetText("tutorial_block"));
                return true;
        }

        return false;
    }
}
