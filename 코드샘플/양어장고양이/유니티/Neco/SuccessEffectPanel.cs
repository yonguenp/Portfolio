using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EFFECT_TYPE
{
    NEW_CAT,                // 새로운 고양이 획득
    NEW_OBJECT,             // 새로운 제작 오브젝트(설치물) 획득
    NEW_RECIPE_RESULT,      // 새로운 요리, 제작 재료 획득
    NEW_MEMORIES,           // 새로운 추억 획득

    NEW_EFFECT_MAX
}

public class SuccessEffectPanel : MonoBehaviour
{
    public GameObject[] effectList = new GameObject[(int)EFFECT_TYPE.NEW_EFFECT_MAX];

    #region NEW CAT EFFECT
    public enum CAT_NAME
    {
        MOTHER = 1,
        GILMAK = 2,
        SAMSECK = 3,
        NAONG = 4,
        YATONG = 5,
        YEONNIM = 6,
        CASANOVA = 7,
        DDUNGDDANG = 8,
        MARILYN = 9,
        DODO = 10,
        JO = 11,
        MOO = 12,
        REGI = 13,
        BINZIP = 14,
        EOLUNG = 15,
        JACTONG = 16,
        CHAOS = 17,
        SPOT_KITTY = 18,
        WHITE_KITTY = 19,

        CAT_MAX
    }

    [Header("[NEW CAT EFFECT]")]
    public Text newCatNameText;

    public GameObject[] catList = new GameObject[(int)CAT_NAME.CAT_MAX];
    Dictionary<uint, int> catListDic = new Dictionary<uint, int>();

    public AudioSource catSoundAudioSource;
    public AudioClip[] catSoundList;

    #endregion

    #region NEW OBJECT EFFECT
    public enum OBJECT_NAME // recipe 테이블 id 기준
    {
        FISHING_ROD = 29,
        TOILET = 30,
        FEEDING_MACHINE = 31,
        WHITE_CAT_HOUSE = 32,
        WATER = 33,
        
        WARM_CAT_HOUSE = 35,
        SCRATCHER = 36,
        CUSHION = 37,
        TREE_CAT_TOWER = 38,

        AUTO_TOILET = 39,
        CAMPBOX = 40,
        PIPE_CATTOWER = 41,
        SAMSEK_BED = 42,
        TREE_ON_CATHOUSE = 43,
        CATWHEEL = 44,

        XMAS_CAT_TOWER = 45,
        MOO_CAT_TOWER = 48,

        CATNIP_FARM = 49,
        COLORFUL_CATTOWER = 52,
        FLORAR_CATHOUSE = 53,      
    }

    [Header("[NEW OBJECT EFFECT]")]
    public Text newObjectNameText;

    public GameObject[] objectList;
    Dictionary<uint, int> objectListDic = new Dictionary<uint, int>();

    #endregion

    #region NEW RECIPE RESULT EFFECT

    [Header("[NEW RECIPE RESULT EFFECT]")]
    public Text newRecipeNotiText;
    public Image newRecipeIconImage;
    public Text newRecipeNameText;
    public Text newRecipeCountText;

    #endregion

    #region NEW MEMORIES EFFECT

    [Header("[NEW MEMORIES EFFECT]")]
    public Image newMemoriesImage;
    public Image newMemoriesPhotoIconImage;
    public Image newMemoriesVideoIconImage;
    public Text newMemoriesNameText;
    public Text newMemoriesCountText;
    public NecoCatDetailPanel catDetailPanel;
    #endregion

    public delegate void Callback();

    Callback closeCallback = null;

    uint curContentsID = 0;
    uint curCount = 0;
    EFFECT_TYPE curEffectType = EFFECT_TYPE.NEW_CAT;

    public void OnClickOkButton()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SUCCESS_EFFECT_POPUP);
        closeCallback?.Invoke();

        MapObjectController mapController = NecoCanvas.GetGameCanvas().GetCurMapController();
        if (mapController != null)
        {
            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.조리대UI등장)
            {
                mapController.SendMessage("첫요리완료", SendMessageOptions.DontRequireReceiver);
            }
            if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.배스구이강조)
            {
                mapController.SendMessage("배스구이완료", SendMessageOptions.DontRequireReceiver);
            }

            switch (curEffectType)
            {
                case EFFECT_TYPE.NEW_RECIPE_RESULT:
                    {
                        switch(neco_data.GetPrologueSeq())
                        {
                            case neco_data.PrologueSeq.빙어튀김요리:
                                if(curContentsID == 6)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.빙어튀김요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.잉어찜요리:
                                if (curContentsID == 8)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.잉어찜요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.민물고기찜요리:
                                if (curContentsID == 12)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.민물고기찜요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.바다고기회요리:
                                if (curContentsID == 14)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.바다고기회요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.장어구이요리:
                                if (curContentsID == 13)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.장어구이요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.참치구이요리:
                                if (curContentsID == 18)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.참치구이요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.고급바다고기회요리:
                                if (curContentsID == 19)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.고급바다고기회요리, SendMessageOptions.DontRequireReceiver);
                                break;
                            case neco_data.PrologueSeq.무지개배스찜요리:
                                if (curContentsID == 20)
                                    mapController.SendMessage("OnGuideQuestCheckOut", neco_data.PrologueSeq.무지개배스찜요리, SendMessageOptions.DontRequireReceiver);
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public void InitSuccessEffect(EFFECT_TYPE effectType, uint ID, uint count = 0, Callback _closeCallback = null)
    {
        curEffectType = effectType;
        closeCallback = _closeCallback;
        curContentsID = ID;
        curCount = count;

        if (curContentsID == 0) { return; }

        ResetAllData();

        InitListToDicData();

        switch (curEffectType)
        {
            case EFFECT_TYPE.NEW_CAT:
                SetNewCatEffect();
                break;
            case EFFECT_TYPE.NEW_OBJECT:
                SetNewObjectEffect();
                break;
            case EFFECT_TYPE.NEW_RECIPE_RESULT:
                SetNewRecipeResultEffect();
                break;
            case EFFECT_TYPE.NEW_MEMORIES:
                Invoke("SetNewMemoriesEffect", 0.1f);
                break;
        }
    }

    void SetNewCatEffect()
    {
        neco_cat curData = neco_cat.GetNecoCat(curContentsID);

        if (curData == null) { return; }
        if (newCatNameText == null) { return; }
        if (catList == null || catList.Length <= 0) { return; }
        if (catListDic == null || catListDic.Count <= 0) { return; }

        if (catListDic.ContainsKey(curData.GetCatID()))
        {
            int indexValue = catListDic[curData.GetCatID()];
            catList[indexValue].SetActive(true);
        }

        newCatNameText.text = curData.GetCatName();

        effectList[(int)EFFECT_TYPE.NEW_CAT].SetActive(true);

        // 사운드 재생
        PlayCatSound(curData.GetCatID());

        // 프롤롤그 체크
        if (neco_data.GetPrologueSeq() == neco_data.PrologueSeq.길막이획득프로필팝업확인)
        {
            NecoCanvas.GetPopupCanvas().OnTouchDefend(1.0f, null);
        }
    }

    void PlayCatSound(uint curCatID)
    {
        if (catSoundAudioSource != null)
        {
            if (catSoundList != null)
            {
                if (catSoundList.Length <= curCatID)
                {
                    Debug.LogError("catSoundList.Length <= curCatID");
                    return;
                }

                if (catSoundList[curCatID] != null)
                {
                    catSoundAudioSource.clip = catSoundList[curCatID];
                    catSoundAudioSource.Play();
                }
            }
        }
    }

    void SetNewObjectEffect()
    {
        recipe curData = recipe.GetRecipe(curContentsID);

        if (curData == null) { return; }
        if (newObjectNameText == null) { return; }
        if (objectList == null || objectList.Length <= 0) { return; }
        if (objectListDic == null || objectListDic.Count <= 0) { return; }

        if (objectListDic.ContainsKey(curData.GetRecipeID()))
        {
            int indexValue = objectListDic[curData.GetRecipeID()];
            objectList[indexValue].SetActive(true);
        }

        newObjectNameText.text = curData.GetRecipeName();

        effectList[(int)EFFECT_TYPE.NEW_OBJECT].SetActive(true);
    }

    void SetNewRecipeResultEffect()
    {
        recipe curData = recipe.GetRecipe(curContentsID);

        if (curData == null) { return; }
        if (newRecipeIconImage == null || newRecipeNameText == null || newRecipeCountText == null) { return; }

        newRecipeNotiText.text = curData.GetRecipeType() == "FOOD" ? LocalizeData.GetText("LOCALIZE_381") : LocalizeData.GetText("LOCALIZE_382");

        newRecipeIconImage.sprite = Resources.Load <Sprite>(curData.GetRecipeIcon());
        newRecipeNameText.text = curData.GetRecipeName();
        newRecipeCountText.text = string.Format("{0}", curCount);

        effectList[(int)EFFECT_TYPE.NEW_RECIPE_RESULT].SetActive(true);
    }

    void SetNewMemoriesEffect()
    {
        neco_cat_memory curData = neco_cat_memory.GetNecoMemory(curContentsID);
        neco_cat curCatData = neco_cat.GetNecoCat(curData.GetNecoMemoryCatID());
        neco_user_cat userCatInfo = neco_user_cat.GetUserCatInfo(curCatData.GetCatID());

        if (curData == null || curCatData == null || userCatInfo == null) { return; }
        if (newMemoriesImage == null || newMemoriesNameText == null || newMemoriesCountText == null) { return; }
        if (newMemoriesPhotoIconImage == null || newMemoriesVideoIconImage == null) { return; }

        newMemoriesImage.sprite = Resources.Load<Sprite>(curData.GetNecoMemoryThumbnail());
        newMemoriesNameText.text = curCatData.GetCatName();

        newMemoriesPhotoIconImage.gameObject.SetActive(curData.GetNecoMemoryType() == "photo" || curData.GetNecoMemoryType() == "ani");
        newMemoriesVideoIconImage.gameObject.SetActive(curData.GetNecoMemoryType() == "movie");

        uint curCount = userCatInfo.GetMovieMemoryCount();      // successEffect는 movie 획득 연출만 노출됨.
        int maxCount = curData.GetTotalNecoMemroyCountByType(curData.GetNecoMemoryType());

        newMemoriesCountText.text = string.Format("({0}/{1})", curCount, maxCount);

        //newMemoriesCountText.DOTextInt(curCount, curCount + 1, 1.0f, it => string.Format("{0}/{1}", it, maxCount));

        effectList[(int)EFFECT_TYPE.NEW_MEMORIES].SetActive(true);
    }

    void InitListToDicData()
    {
        // enum 리스트와 id를 일치 시키기 위한 데이터 재가공

        if (catListDic.Count > 0 && objectListDic.Count > 0) { return; }

        catListDic.Clear();
        int listIndex = 0;
        foreach (int index in Enum.GetValues(typeof(CAT_NAME)))
        {
            catListDic.Add((uint)index, listIndex);
            listIndex++;
        }

        objectListDic.Clear();
        listIndex = 0;
        foreach (int index in Enum.GetValues(typeof(OBJECT_NAME)))
        {
            objectListDic.Add((uint)index, listIndex);
            listIndex++;
        }
    } 

    void ResetAllData()
    {
        // 이펙트 레이어 초기화
        foreach (GameObject layer in effectList)
        {
            if (layer == null) continue;

            layer.SetActive(false);
        }

        // 고양이 리스트 초기화
        foreach (GameObject catObject in catList)
        {
            if (catObject == null) continue;

            catObject.SetActive(false);
        }

        // 오브젝트 리스트 초기화
        foreach (GameObject objectitem in objectList)
        {
            if (objectitem == null) continue;

            objectitem.SetActive(false);
        }

        catListDic.Clear();
        objectListDic.Clear();
    }

    public void OnCatDetail()
    {
        neco_cat_memory curData = neco_cat_memory.GetNecoMemory(curContentsID);
        neco_cat curCatData = neco_cat.GetNecoCat(curData.GetNecoMemoryCatID());

        List<neco_cat> dummy = new List<neco_cat>();
        dummy.Add(curCatData);

        catDetailPanel.SetCatDetailInfo(curCatData, dummy, false);
    }
}
