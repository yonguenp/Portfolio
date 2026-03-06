using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanCreateSubPopup : MonoBehaviour
{
    [SerializeField]
    InputField InputClanName;
    [SerializeField]
    protected GameObject[] AutoJoinOption;
    [SerializeField]
    Text RankCondition;
    [SerializeField]
    GameObject RankConditionCheck;
    [SerializeField]
    Text GoldCondition;
    [SerializeField]
    GameObject GoldConditionCheck;
    [SerializeField]
    GameObject ClanDesc;
    [SerializeField]
    protected GameObject ClanDescInput;
    [SerializeField]
    UIClanEmblem ClanEmblem;

    [SerializeField]
    GameObject CreateConfirmPopup;
    [SerializeField]
    Text ConfirmClanName;

    [SerializeField]
    ClanEmblemSubPopup SettingEmblemPopup;


    protected bool autoJoin = true;
    bool enableCreate = false;
    protected int curEmblem = 0;

    private void OnEnable()
    {
        InitUI();
    }

    protected virtual void InitUI()
    {
        if (InputClanName != null)
            InputClanName.text = "";

        autoJoin = true;
        AutoJoinOption[0].SetActive(autoJoin);
        AutoJoinOption[1].SetActive(!autoJoin);

        if (RankCondition != null && RankConditionCheck != null && GoldCondition != null && GoldConditionCheck != null)
        {
            Color red = new Color(1.0f, 0.2117647f, 0.3098039f);
            Color green = new Color(0.3607843f, 1.0f, 0.5607843f);

            bool rank = Managers.UserData.MyPoint >= GameConfig.Instance.CLAN_MAKE_POINT;
            RankCondition.color = rank ? green : red;
            RankConditionCheck.SetActive(rank);
            bool gold = Managers.UserData.MyGold >= GameConfig.Instance.CLAN_MAKE_GOLD;
            GoldCondition.text = GameConfig.Instance.CLAN_MAKE_GOLD.ToString();
            GoldCondition.color = gold ? green : red;
            GoldConditionCheck.SetActive(gold);

            enableCreate = rank && gold;
        }

        ClanDescInput.SetActive(true);

        List<GameData> emblems = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_emblem);
        List<ClanEmblemData> myEmblems = new List<ClanEmblemData>();
        foreach (ClanEmblemData data in emblems)
        {
            switch (data.type)
            {
                case ClanEmblemData.EMBLEM_TYPE.CHECK_ITEM:
                    {
                        if (Managers.UserData.GetMyItemCount(data.param) > 0)
                        {
                            myEmblems.Add(data);
                        }
                    }
                    break;
                default:
                    myEmblems.Add(data);
                    break;
            }
        }

        OnEmblemChanged(myEmblems[Random.Range(0, myEmblems.Count)].GetID());
    }

    public void AutoJoinToggle()
    {
        autoJoin = !autoJoin;
        AutoJoinOption[0].SetActive(autoJoin);
        AutoJoinOption[1].SetActive(!autoJoin);
    }

    public void OnEmblemChangeButton()
    {
        SettingEmblemPopup.OnCreateEmblem(curEmblem, this);
        SettingEmblemPopup.gameObject.SetActive(true);
    }

    public void OnEmblemChanged(int index)
    {
        curEmblem = index;
        ClanEmblem.Init((ClanEmblemData)GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_emblem, index));
    }

    public void OnCreateClan()
    {
        if(!enableCreate)
        {
            PopupCanvas.Instance.ShowFadeText("클랜생성조건미달");
            return;
        }

        string clanName = InputClanName.text;
        InputClanName.text = "";

        if (!enableCreate)
        {
            PopupCanvas.Instance.ShowFadeText("클랜이름미입력");
            return;
        }

        ConfirmClanName.text = clanName;
        CreateConfirmPopup.GetComponent<ClanCreateConfirmPopup>().SetData(clanName, curEmblem);
        CreateConfirmPopup.SetActive(true);
    }

    public void OnCreateConfirm()
    {
        if (!enableCreate)
        {
            PopupCanvas.Instance.ShowFadeText("클랜생성조건미달");
            return;
        }

        ClanPopup ClanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        ClanPopup.ClanRequestCreate(InputClanName.text, ClanDescInput.GetComponent<InputField>().text, autoJoin, curEmblem);
    }

}
