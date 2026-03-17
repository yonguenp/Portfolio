using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GachaResultPopup : Popup
{
    [Header("[결과]")]
    [SerializeField] Button moreChanceBtn;

    [SerializeField] UIGachaItem[] gachaItems;
    [SerializeField] GameObject btnGroup;

    [SerializeField] Button openAllBtn;
    [SerializeField] SkeletonGraphic start_cardSpine;

    [Header("[서브 팝업]")]
    [SerializeField] GameObject subPopup;
    [SerializeField] Button okBtn;


    [SerializeField] Image priceTypeIcon;
    [SerializeField] Text priceDesc;
    [SerializeField] Image priceType;

    [SerializeField] Text curMyAssets;

    [SerializeField] Sprite GoldIcon;
    [SerializeField] Sprite DiaIcon;
    int curUpLineCount = 0;
    System.Action closeCB = null;
    public List<SBWeb.ResponseReward> resultDatas = null;

    public bool isSkip = true;
    public List<int> changeToCharacter = new List<int>();
    public int openCardCnt = 0;

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
    }

    public override void Close()
    {
        closeCB?.Invoke();
        base.Close();
        Clear();

        if (PopupCanvas.Instance.PopupEscList.Contains(this))
            PopupCanvas.Instance.PopupEscList.Remove(this);
    }

    public override void RefreshUI()
    {
        base.RefreshUI();
        Clear();
        SetActiveSubPopUp(false);
    }

    public void Init(List<SBWeb.ResponseReward> datas, System.Action action, System.Action close_cb = null)
    {
        closeCB = close_cb;
        resultDatas = datas;
        Clear();
        
        start_cardSpine.Clear();
        openAllBtn.gameObject.SetActive(false);
        AudioSource spineAudio = null;
        if (resultDatas.Count == 1)
        {
            spineAudio = Managers.Sound.Play("effect/EF_GACHA_CARD", Sound.Effect);
            start_cardSpine.startingAnimation = "arrange_single";
            start_cardSpine.startingLoop = false;
        }
        else
        {
            spineAudio = Managers.Sound.Play("effect/EF_GACHA_CARDS", Sound.Effect);
            start_cardSpine.startingAnimation = "arrange_10";
            start_cardSpine.startingLoop = false;
        }
        start_cardSpine.Initialize(true);
        start_cardSpine.color = new Color(1, 1, 1, 1);
        start_cardSpine.gameObject.SetActive(true);

        start_cardSpine.AnimationState.Complete += delegate
        {
            for (int i = 0; i < resultDatas.Count; i++)
            {
                gachaItems[i].SetActive(true);
            }
            start_cardSpine.color = new Color(1, 1, 1, 0);
            openAllBtn.interactable = true;
            openAllBtn.gameObject.SetActive(true);
            if (spineAudio != null)
                spineAudio.Stop();
        };
        CloseNewCharacterPopup();
        btnGroup.SetActive(false);

        moreChanceBtn.gameObject.SetActive(action != null);

        okBtn.onClick.RemoveAllListeners();
        okBtn.onClick.AddListener(() =>
        {
            SetActiveSubPopUp(false);
            action?.Invoke();
        });

        int idx = 0;
        foreach (var data in resultDatas)
        {
            switch (data.Type)
            {
                case SBWeb.ResponseReward.RandomRewardType.EQUIP:
                case SBWeb.ResponseReward.RandomRewardType.ITEM:
                    {
                        ItemGameData gameItem = ItemGameData.GetItemData(data.Id);
                        gachaItems[idx].ItemInit(gameItem, data.Amount, data.originReward);
                    }
                    break;
                case SBWeb.ResponseReward.RandomRewardType.CHARACTER:
                    {
                        CharacterGameData characterData = CharacterGameData.GetCharacterData(data.Id);
                        gachaItems[idx].CharacterInit(characterData, data.originReward);
                    }
                    break;
            }
            idx++;
        }
    }

    public void OpenAllBtn()
    {
        openAllBtn.interactable = false;
        openAllBtn.gameObject.SetActive(false);

        StartCoroutine(OpenAllBtnCo());
    }
    IEnumerator OpenAllBtnCo()
    {
        for (int i = 0; i < gachaItems.Length; i++)
        {
            if (!gachaItems[i].GetSkeleton().gameObject.activeSelf)
                continue;
            if (gachaItems[i].GetSkeleton().color.a < 1.0f)
                continue;

            isSkip = false;
            gachaItems[i].OnButton();
            yield return new WaitUntil(() => isSkip == true);
        }

        openAllBtn.interactable = true;
        openAllBtn.gameObject.SetActive(false);

        yield return new WaitForSeconds(2.0f);

        btnGroup.SetActive(true);
        yield return null;
    }

    public void IsSkip()
    {
        isSkip = true;
    }

    void Clear()
    {
        StopAllCoroutines();

        foreach (var item in gachaItems)
        {
            item.SetActive(false);
            item.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        openCardCnt = 0;
        changeToCharacter.Clear();
    }


    public void SetActiveSubPopUp(bool isOn)
    {
        subPopup.SetActive(isOn);

        if (isOn)
        {
            if (!RefreshSubPopup())
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_data_error"));
                subPopup.SetActive(false);
                return;
            }
        }
    }


    public bool RefreshSubPopup()
    {
        GachaPopup popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.GACHA_POPUP) as GachaPopup;
        if (popup == null || popup.curSelectData == null)
            return false;

        GachaTypesGameData curTarget = popup.curSelectData.onceInfo;
        if (resultDatas.Count > 1)
            curTarget = popup.curSelectData.repeatInfo;

        switch (curTarget.priceInfo.type)
        {
            case ASSET_TYPE.GOLD:
                priceTypeIcon.sprite = GoldIcon;
                priceType.sprite = GoldIcon;
                curMyAssets.text = StringManager.GetString("보유유저골드", Managers.UserData.MyGold);
                break;
            case ASSET_TYPE.DIA:
                priceTypeIcon.sprite = DiaIcon;
                priceType.sprite = DiaIcon;
                curMyAssets.text = StringManager.GetString("보유유저다이아", Managers.UserData.MyDia);
                break;
            case ASSET_TYPE.ITEM:
                priceTypeIcon.sprite = ItemGameData.GetItemIcon(curTarget.priceInfo.param);
                priceType.sprite = ItemGameData.GetItemIcon(curTarget.priceInfo.param);
                curMyAssets.text = StringManager.GetString("보유아이템갯수", Managers.UserData.GetMyItemCount(curTarget.priceInfo.param));
                break;
        }

        priceDesc.text = StringManager.GetString("필요유저재화안내", curTarget.priceInfo.amount);

        return true;
    }

    public void CloseNewCharacterPopup()
    {
        PopupCanvas.Instance.ClosePopup(PopupCanvas.POPUP_TYPE.NEW_CHARACTER_POPUP);
    }


    public void ButtonGroupOn()
    {
        openAllBtn.gameObject.SetActive(false);
        btnGroup.SetActive(true);
    }
    public int OnGachaItemCnt()
    {
        int sum = 0;

        foreach (var item in gachaItems)
        {
            if (item.gameObject.activeSelf)
                sum++;
        }
        return sum;
    }
    public bool FindCharacterData(int id)
    {
        foreach (var item in resultDatas)
        {
            if (item.Type == SBWeb.ResponseReward.RandomRewardType.CHARACTER && item.Id == id)
                return true;
        }

        return false;
    }

    public void SetSkip()
    {
        CancelInvoke("SetSkip");

        isSkip = true;
    }
}
