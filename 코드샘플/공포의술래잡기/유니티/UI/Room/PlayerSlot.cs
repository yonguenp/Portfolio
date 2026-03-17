using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField]
    protected Text playerName;
    [SerializeField]
    SelectedCharacter selectedCharacter;
    [SerializeField]
    protected GameObject readyPanel;
    [SerializeField]
    Text characterName;

    [SerializeField]
    UIGrade grade;
    [SerializeField]
    UIEnchant enchant;
    [SerializeField]
    Text levelText;

    [SerializeField]
    Image BackPanel;
    [SerializeField]
    Image FramePanel;
    [SerializeField]
    Image NamePanel;

    [SerializeField]
    Image RankImage;
    [SerializeField]
    GameObject Top;
    [SerializeField]
    Text ClanName;

    protected string userName = "";
    protected bool isChaser = false;
    protected bool isRoom = true;
    protected int index = 0;
    public void ClearUI()
    {
        CharacterClear();
        ReadyClear();
        if (playerName != null) playerName.text = "";
        if (characterName != null) characterName.text = "";
        if (grade != null) grade.gameObject.SetActive(false);
        if (enchant) enchant.gameObject.SetActive(false);
        if (levelText) levelText.gameObject.SetActive(false);
        if (RankImage) RankImage.gameObject.SetActive(false);

        if (Top) Top.SetActive(false);
        if (ClanName) ClanName.text = "";
    }

    protected virtual void CharacterClear()
    {
        selectedCharacter.ClearUI();
    }

    void ReadyClear()
    {
        if (readyPanel != null)
            readyPanel.SetActive(false);

        if (BackPanel != null)
        {
            //if (ColorUtility.TryParseHtmlString("#676171", out color))
            BackPanel.color = Color.white;
            FramePanel.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/room_frame_03");
        }
        if (NamePanel != null)
        {
            //if (ColorUtility.TryParseHtmlString("#A49DAF", out color))
            NamePanel.color = Color.white;
            NamePanel.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/room_frame_01");
        }
    }

    public void SetSlotPlayerInRoom(SBSocketSharedLib.RoomPlayerInfo data)
    {
        SetSlotPlayer(data);
    }

    public void SetSlotPlayerInLoding(SBSocketSharedLib.RoomPlayerInfo data, bool chaser, int index)
    {
        SetSlotPlayer(data, chaser, false, index);
        selectedCharacter.SetAnimation("r_run_0");
        selectedCharacter.GetSkeletonGraphic().Skeleton.ScaleX = -1.0f;
    }

    public void SetSlotPlayer(SBSocketSharedLib.RoomPlayerInfo data, bool _chaser = false, bool _room = true, int idx = 0)
    {
        CharacterGameData characterData = null;

        if (data != null)
        {
            characterData = CharacterGameData.GetCharacterData(data.SelectedCharacter.CharacterType);
            if (_chaser == false && characterData != null && characterData.IsChaserCharacter())
            {
                SetReadyPlayer(true);
                return;
            }
        }

        isChaser = _chaser;
        isRoom = _room;
        index = idx;

        gameObject.SetActive(true);

        ClearUI();

        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // SetReadyPlayer(data.IsReady);
        SetPlayerCharacter(data.SelectedCharacter.CharacterType, data.SelectedCharacter.ItemNos);

        if (characterName != null)
        {
            //characterName.SetText(characterData == null ? "" : characterData.GetName(), characterData.IsChaserCharacter() ? SHelper.TEXT_TYPE.CHASER_CHARACTER : SHelper.TEXT_TYPE.SURVIVOR_CHARACTER);
            characterName.SetText(characterData == null ? "" : characterData.GetName());//, characterData.IsChaserCharacter() ? SHelper.TEXT_TYPE.CHASER_CHARACTER : SHelper.TEXT_TYPE.SURVIVOR_CHARACTER);
        }

        //CBT에서 등급 제거
        //grade?.gameObject.SetActive(true);
        grade?.gameObject.SetActive(false);
        enchant?.gameObject.SetActive(true);
        levelText?.gameObject.SetActive(true);

        grade?.SetGrade(characterData.char_grade);
        enchant?.SetEnchant(data.SelectedCharacter.Enhance_Step);
        if (levelText != null)
            levelText.text = "Lv." + data.SelectedCharacter.Level;

        userName = data.UserName;


        if (RankImage)
        {
            //RankImage.gameObject.SetActive(true);
            RankImage.sprite = RankType.GetRankFromPoint(data.RankPoint).rank_resource;
        }
        SetNamePanel();

        //data clan정보 체크
        if (!string.IsNullOrEmpty(data.ClanName))
        {
            if (Top) Top.SetActive(true);
            if (ClanName) ClanName.text = data.ClanName;
        }
        else
        {
            if (Top) Top.SetActive(false);
            if (ClanName) ClanName.text = "";
        }
    }

    protected virtual void SetPlayerCharacter(int type, IList<int> equips)
    {
        CharacterClear();
        selectedCharacter.SetCharacter(type, equips != null && equips.Count > 0 ? equips[0] : 0);
    }

    public void SetReadyPlayer(bool ready)
    {
        ReadyClear();

        if (ready)
        {
            if (readyPanel != null)
                readyPanel.SetActive(ready);

            if (BackPanel != null)
            {
                //if (ColorUtility.TryParseHtmlString("#F8E278", out color))
                //    BackPanel.color = color;
                BackPanel.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/room_frame_bg_02");
            }
            if (NamePanel != null)
            {
                //if (ColorUtility.TryParseHtmlString("#E1AA5D", out color))
                //    NamePanel.color = color;
                NamePanel.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/room_frame_02");
            }
            if (FramePanel != null)
            {
                FramePanel.sprite = Managers.Resource.Load<Sprite>("Texture/UI/Lobby/room_frame_04");
            }

            SetNamePanel();
        }
    }

    public virtual void SetNamePanel()
    {
        bool isReady = false;
        if (readyPanel != null)
            isReady = readyPanel.activeSelf;

        SHelper.TEXT_TYPE type = SHelper.TEXT_TYPE.UNKNOWN;
        if(userName == Managers.UserData.MyName)
        {
            type = SHelper.TEXT_TYPE.ME_IN_LOADING;
        }
        else if (isChaser)
        {
            type = SHelper.TEXT_TYPE.CHASER_CHARACTER;
        }
        else
        {
            type = SHelper.TEXT_TYPE.SURVIVOR_CHARACTER;
        }

        playerName.SetText(userName, type);

        if (isRoom)
        {
            if (isReady)
            {
                playerName.color = Color.black;
                playerName.GetComponent<Outline>().enabled = false;
            }
            else
            {
                playerName.color = Color.white;
                playerName.GetComponent<Outline>().enabled = true;
            }
        }

        if (characterName != null)
            characterName.gameObject.SetActive(!isReady);
    }

    public void SetHideChaser()
    {
        CharacterClear();
        selectedCharacter.SetCharacter(-1, 0);
    }

    [ContextMenu("Auto Serialize")]
    public void AutoSerialize()
    {
        playerName = transform.Find("NamePanel").Find("Text").GetComponent<Text>();
        selectedCharacter = transform.Find("SelectedCharacter").GetComponent<SelectedCharacter>();
        if (transform.Find("ReadyPanel") != null)
            readyPanel = transform.Find("ReadyPanel").gameObject;
    }
}
