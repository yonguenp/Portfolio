using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatUI : MonoBehaviour
{
    enum STAT_TYPE
    {
        UNKNOWN = 0,
        SUVIVOR = 1,
        CHASER = 2,
    };

    [SerializeField]
    CharacterPopup popup;

    [SerializeField]
    GameObject survivorBG;
    [SerializeField]
    GameObject chaserBG;

    [SerializeField]
    SelectedCharacter selectedCharacter;
    [SerializeField]
    Text characterName;
    [SerializeField]
    Text characterDesc;
    [SerializeField]
    GameObject selectedIcon;
    [SerializeField]
    UIEnchant enchant;
    [SerializeField]
    UIGrade grade;
    [SerializeField]
    Text level;
    [SerializeField]
    Image skillIcon;
    [SerializeField]
    Text skillName;
    [SerializeField]
    Text skillLevel;
    [SerializeField]
    Text skillDesc;

    [SerializeField]
    StatItem statSample;
    [SerializeField]
    StatItem playdataSample;
    [SerializeField]
    StatItem resultSample;

    [SerializeField]
    Image exp;
    [SerializeField]
    Text remainExp;

    [SerializeField]
    Text skillCoolTime;

    [SerializeField]
    CharacterScrollUIController CharacterList;

    [SerializeField]
    CharacterLevelUpUI levelUPUI;
    [SerializeField]
    CharacterEnchantUpUI enchantUPUI;
    [SerializeField]
    CharacterSkillUpUI skillUPUI;

    [SerializeField]
    Image[] CharacterTypeTaps;
    [SerializeField]
    Text[] CharacterTypeTexts;
    [SerializeField]
    Sprite[] TapImages;

    [SerializeField]
    GameObject StatPanel;
    [SerializeField]
    GameObject PlayDataPanel;

    [SerializeField] Button LevelUPButton;
    [SerializeField] Button EnchantUPButton;
    [SerializeField] Button MoveToGachaButton;
    [SerializeField] Button SkillUPButton;
    [SerializeField] Button DefaultCharacterSetButton;

    [SerializeField] GameObject InfoIcon;
    [SerializeField] GameObject NotMineText;

    [Serializable]
    public class TalentUI
    {
        public Text icon_text;
        public Text noneText;
        public Text typeText;
        public Text valueText;
        public Button btn;
        public GameObject main;
        public GameObject lockDim;
        public UIGrade uIGrade;
    }
    [Header("[재능개방 관련]")]
    [SerializeField]
    TalentUI[] talentGroups;

    [Header("[장비 관련]")]
    [SerializeField] Image equip_icon;
    [SerializeField] Text equip_text;

    STAT_TYPE uiType = STAT_TYPE.UNKNOWN;
    int curChaser = -1;
    int curSurvivor = -1;

    public void Awake()
    {

    }

    public bool IsActive()
    {
        return gameObject.activeInHierarchy;
    }

    public void SubPopupSetActive(bool enable)
    {
        levelUPUI.SetActive(enable);
        enchantUPUI.SetActive(enable);
        skillUPUI.SetActive(enable);
    }
    public void SetActive(bool active)
    {
        if (active && !gameObject.activeInHierarchy)
        {
            levelUPUI.SetActive(false);
            enchantUPUI.SetActive(false);
            skillUPUI.SetActive(false);
        }

        gameObject.SetActive(active);

        if (active)
        {
            if (uiType == STAT_TYPE.UNKNOWN)
                uiType = STAT_TYPE.SUVIVOR;

            StatPanel.SetActive(true);
            PlayDataPanel.SetActive(false);
            RefreshList();
        }
        else
        {
            curChaser = Managers.UserData.MyDefaultChaserCharacter;
            curSurvivor = Managers.UserData.MyDefaultSurvivorCharacter;
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        survivorBG.SetActive(false);
        chaserBG.SetActive(false);

        if (curChaser <= 0)
            curChaser = Managers.UserData.MyDefaultChaserCharacter;
        if (curSurvivor <= 0)
            curSurvivor = Managers.UserData.MyDefaultSurvivorCharacter;

        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        if (targetCharcter <= 0)
            return;

        CharacterGameData targetCharacterData = CharacterGameData.GetCharacterData(targetCharcter);
        UserCharacterData userCharacterData = Managers.UserData.GetMyCharacterInfo(targetCharcter);

        selectedCharacter.SetCharacter(targetCharacterData.GetID(), userCharacterData != null ? userCharacterData.curEquip : null);
        selectedCharacter.GetSkeletonGraphic().color = userCharacterData != null ? Color.white : new Color(0.3f, 0.3f, 0.3f);
        characterName.text = targetCharacterData.GetName();
        grade.SetGrade(targetCharacterData.char_grade);

        skillName.text = targetCharacterData.GetSkillData().GetName();
        skillIcon.sprite = targetCharacterData.GetSkillData().GetIcon();
        characterDesc.text = targetCharacterData.GetDesc();

        int targetLevel = GameConfig.Instance.MAX_CHARACTER_LEVEL;
        int targetEnchant = GameConfig.Instance.MAX_CHARACTER_REINFORCE;
        int targetSkillLevel = GameConfig.Instance.MAX_CHARACTER_SKILL_LEVEL; //targetEnchant;
        int curExp = 0;

        LevelUPButton.interactable = false;
        EnchantUPButton.interactable = false;
        DefaultCharacterSetButton.interactable = false;
        SkillUPButton.interactable = false;

        LevelUPButton.gameObject.SetActive(true);
        EnchantUPButton.gameObject.SetActive(true);
        SkillUPButton.gameObject.SetActive(true);
        MoveToGachaButton.gameObject.SetActive(false);

        InfoIcon.SetActive(false);
        NotMineText.SetActive(false);
        if (userCharacterData != null)
        {
            targetLevel = userCharacterData.lv;
            targetEnchant = userCharacterData.enchant;
            targetSkillLevel = userCharacterData.skillLv;
            curExp = userCharacterData.exp;

            LevelUPButton.interactable = userCharacterData.lv < (userCharacterData.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL);
            EnchantUPButton.interactable = userCharacterData.enchant < GameConfig.Instance.MAX_CHARACTER_REINFORCE;
            SkillUPButton.interactable = userCharacterData.skillLv < GameConfig.Instance.MAX_CHARACTER_SKILL_LEVEL;

            DefaultCharacterSetButton.interactable = true;

            InfoIcon.SetActive(true);
        }
        else
        {
            if (!StatPanel.activeInHierarchy)
                ToggleInfo();

            LevelUPButton.gameObject.SetActive(false);
            EnchantUPButton.gameObject.SetActive(false);
            SkillUPButton.gameObject.SetActive(false);
            MoveToGachaButton.gameObject.SetActive(true);
            NotMineText.SetActive(true);
            NotMineText.transform.GetComponentInChildren<Image>().sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_exclamation_black_white");
        }

        foreach (Image graphic in LevelUPButton.GetComponentsInChildren<Image>())
        {
            graphic.color = LevelUPButton.interactable ? LevelUPButton.colors.normalColor : LevelUPButton.colors.disabledColor;
        }

        foreach (Text graphic in LevelUPButton.GetComponentsInChildren<Text>())
        {
            Color color = graphic.color;
            color.a = LevelUPButton.interactable ? LevelUPButton.colors.normalColor.a : LevelUPButton.colors.disabledColor.a;
            graphic.color = color;
        }

        foreach (Image graphic in EnchantUPButton.GetComponentsInChildren<Image>())
        {
            graphic.color = EnchantUPButton.interactable ? EnchantUPButton.colors.normalColor : EnchantUPButton.colors.disabledColor;
        }

        foreach (Text graphic in EnchantUPButton.GetComponentsInChildren<Text>())
        {
            Color color = graphic.color;
            color = EnchantUPButton.interactable ? EnchantUPButton.colors.normalColor : EnchantUPButton.colors.disabledColor;
            graphic.color = color;
        }
        int maxLevel = GameConfig.Instance.MAX_CHARACTER_REINFORCE * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL;
        if (userCharacterData != null)
            maxLevel = userCharacterData.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL;

        enchant.SetEnchant(targetEnchant);
        level.text = targetLevel.ToString() + " / " + maxLevel.ToString();
        skillLevel.text = "Lv." + targetSkillLevel.ToString();
        selectedIcon.SetActive(targetCharcter == Managers.UserData.MyDefaultChaserCharacter || targetCharcter == Managers.UserData.MyDefaultSurvivorCharacter);

        SkillGameData skill = targetCharacterData.GetSkillData();
        if (skill != null)
        {
            skillCoolTime.text = skill.CoolTime.ToString() + StringManager.GetString("초");

            SkillBaseGameData curSkillLvData = skill.GetMajorSkill(targetSkillLevel);
            if (curSkillLvData != null)
            {
                skillDesc.text = curSkillLvData.GetDesc();
            }
        }

        foreach (Transform child in statSample.transform.parent)
        {
            if (child != statSample.transform)
                Destroy(child.gameObject);
        }

        statSample.gameObject.SetActive(true);

        List<StatItem.STAT_TYPE> displayStat = new List<StatItem.STAT_TYPE>();
        if (uiType == STAT_TYPE.SUVIVOR)
        {
            displayStat.Add(StatItem.STAT_TYPE.HP);
            displayStat.Add(StatItem.STAT_TYPE.MOVE_SPEED);
            displayStat.Add(StatItem.STAT_TYPE.MAX_BATTERY);
        }
        if (uiType == STAT_TYPE.CHASER)
        {
            displayStat.Add(StatItem.STAT_TYPE.ATK);
            displayStat.Add(StatItem.STAT_TYPE.MOVE_SPEED);
            displayStat.Add(StatItem.STAT_TYPE.ATTACK_COOL_TIME);
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject item = Instantiate(statSample.gameObject);
            item.transform.SetParent(statSample.transform.parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.GetComponent<StatItem>().SetStat(targetCharacterData, displayStat[i]);
        }

        statSample.gameObject.SetActive(false);

        foreach (Transform child in playdataSample.transform.parent)
        {
            if (child != playdataSample.transform)
                Destroy(child.gameObject);
        }

        playdataSample.gameObject.SetActive(true);

        displayStat.Clear();
        if (uiType == STAT_TYPE.SUVIVOR)
        {
            displayStat.Add(StatItem.STAT_TYPE.PLAY_TIME);
            displayStat.Add(StatItem.STAT_TYPE.CHARGE_COUNT);
            displayStat.Add(StatItem.STAT_TYPE.GET_COUNT);
        }
        if (uiType == STAT_TYPE.CHASER)
        {
            displayStat.Add(StatItem.STAT_TYPE.PLAY_TIME);
            displayStat.Add(StatItem.STAT_TYPE.KILL_COUNT);
            displayStat.Add(StatItem.STAT_TYPE.HIT_COUNT);
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject item = Instantiate(playdataSample.gameObject);
            item.transform.SetParent(playdataSample.transform.parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.GetComponent<StatItem>().SetStat(targetCharacterData, displayStat[i]);
        }

        playdataSample.gameObject.SetActive(false);

        foreach (Transform child in resultSample.transform.parent)
        {
            if (child != resultSample.transform)
                Destroy(child.gameObject);
        }

        resultSample.gameObject.SetActive(true);

        displayStat.Clear();
        displayStat.Add(StatItem.STAT_TYPE.WIN_COUNT);
        displayStat.Add(StatItem.STAT_TYPE.LOSE_COUNT);
        displayStat.Add(StatItem.STAT_TYPE.HIGH_SCORE);

        for (int i = 0; i < 3; i++)
        {
            GameObject item = Instantiate(resultSample.gameObject);
            item.transform.SetParent(resultSample.transform.parent);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.GetComponent<StatItem>().SetStat(targetCharacterData, displayStat[i]);
        }

        resultSample.gameObject.SetActive(false);

        CharacterLevelGameData levelData = targetCharacterData.levelData[targetLevel];
        Vector2 size = (exp.transform.parent as RectTransform).sizeDelta;
        size.y = (exp.transform as RectTransform).sizeDelta.y;

        if (maxLevel != targetLevel)
        {
            int prevLv = targetLevel - 1;
            int prevLvExp = 0;
            if (prevLv > 0 && targetCharacterData.levelData.Length > prevLv)
            {
                prevLvExp = targetCharacterData.levelData[prevLv].need_exp;
            }

            size.x = (exp.transform.parent as RectTransform).sizeDelta.x * ((float)(curExp - prevLvExp) / (levelData.need_exp - prevLvExp));
        }

        if (size.x > (exp.transform.parent as RectTransform).sizeDelta.x - 10)
            size.x = (exp.transform.parent as RectTransform).sizeDelta.x - 10;

        (exp.transform as RectTransform).sizeDelta = size;


        if (maxLevel == targetLevel)
        {
            remainExp.text = StringManager.GetString("char_info_nextex") + ": MAX";
            remainExp.color = new Color(0.4431373f, 0.9960784f, 0.3333333f);
        }
        else
        {
            remainExp.text = StringManager.GetString("char_info_nextex") + $": {levelData.need_exp - curExp}";
            remainExp.color = Color.white;
        }

        //캐릭터 선택 시 재능개화 관련 UI
        if (selectedCharacter != null)
        {
            var character_d = Managers.UserData.GetMyCharacterInfo(selectedCharacter.characterid);
            int idx = 1;
            foreach (var item in talentGroups)
            {
                CharacterTalent talentData = null;
                switch (idx)
                {
                    case 1:
                        if (character_d == null || character_d.lv < 5)
                        {
                            item.main.SetActive(false);
                            item.lockDim.SetActive(true);
                            item.lockDim.transform.Find("Icon").GetComponent<Image>().sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_lock");
                            item.lockDim.GetComponentInChildren<Text>().text = StringManager.GetString("ui_ch_lv", 5) + " " + StringManager.GetString("ui_lv_open");
                        }
                        else
                            item.lockDim.SetActive(false);
                        item.uIGrade.SetGrade(-1);

                        if (character_d != null && character_d.talent1 > 0)
                        {
                            talentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, character_d.talent1) as CharacterTalent;
                            item.uIGrade.SetGrade(talentData.talent_rank);
                        }

                        break;
                    case 2:
                        if (character_d == null || character_d.lv < 15)
                        {
                            item.main.SetActive(false);
                            item.lockDim.SetActive(true);
                            item.lockDim.transform.Find("Icon").GetComponent<Image>().sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_lock");
                            item.lockDim.GetComponentInChildren<Text>().text = StringManager.GetString("ui_ch_lv", 15) + " " + StringManager.GetString("ui_lv_open");
                        }
                        else
                            item.lockDim.SetActive(false);
                        item.uIGrade.SetGrade(-1);

                        if (character_d != null && character_d.talent2 > 0)
                        {
                            talentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, character_d.talent2) as CharacterTalent;
                            item.uIGrade.SetGrade(talentData.talent_rank);
                        }
                        break;
                    case 3:
                        if (character_d == null || character_d.lv < 25)
                        {
                            item.main.SetActive(false);
                            item.lockDim.SetActive(true);
                            item.lockDim.transform.Find("Icon").GetComponent<Image>().sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/icon_lock");
                            item.lockDim.GetComponentInChildren<Text>().text = StringManager.GetString("ui_ch_lv", 25) + " " + StringManager.GetString("ui_lv_open");
                            item.uIGrade.SetGrade(-1);
                        }
                        else
                            item.lockDim.SetActive(false);
                        item.uIGrade.SetGrade(-1);


                        if (character_d != null && character_d.talent3 > 0)
                        {
                            talentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, character_d.talent3) as CharacterTalent;
                            item.uIGrade.SetGrade(talentData.talent_rank);
                        }
                        break;
                }


                if (!item.lockDim.activeSelf)
                {
                    item.main.SetActive(true);
                    item.icon_text.text = StringManager.GetString("재능") + $" {idx}";

                    if (talentData == null)
                    {
                        item.noneText.text = StringManager.GetString("ui_no_option");
                        item.typeText.gameObject.SetActive(false);
                        item.valueText.gameObject.SetActive(false);
                    }
                    else
                    {
                        item.noneText.text = "";

                        item.typeText.gameObject.SetActive(true);
                        item.valueText.gameObject.SetActive(true);

                        item.typeText.text = talentData.GetName();
                        item.valueText.text = talentData.GetValue();

                        if (talentData.talent_rank >= GameConfig.Instance.MAX_TALENT_RANK)
                        {
                            ColorUtility.TryParseHtmlString("#fff265", out Color color);
                            item.typeText.color = color;
                            item.valueText.color = color;
                        }
                        else
                        {
                            item.typeText.color = Color.white;
                            item.valueText.color = Color.white;
                        }
                    }

                    item.btn.onClick.RemoveAllListeners();
                    item.btn.onClick.AddListener(() =>
                    {
                        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
                        {
                            PopupCanvas.Instance.ShowFadeText("매치대기캐릭터변경불가");
                            return;
                        }
                        PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.TALENT_POPUP);
                        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.TALENT_POPUP) as TalentPopup).Init(Array.IndexOf(talentGroups, item) + 1);
                    });
                }
                idx++;
            }
        }

        equip_icon.transform.parent.gameObject.SetActive(false);
        //캐릭터 선택 시 장비 아이템 관련 UI
        if (selectedCharacter != null)
        {
            var myChar = Managers.UserData.GetMyCharacterInfo(selectedCharacter.characterid);

            if (myChar != null)
            {
                equip_icon.transform.parent.gameObject.SetActive(true);

                if (myChar.curEquip != null)
                {
                    equip_icon.gameObject.SetActive(true);
                    equip_text.gameObject.SetActive(false);
                    equip_icon.sprite = Managers.UserData.MyCharacters[selectedCharacter.characterid].curEquip.equipData.itemData.sprite;
                }
                else
                {
                    equip_icon.gameObject.SetActive(false);
                    equip_text.gameObject.SetActive(true);
                }
            }
        }
    }

    public void RefreshList()
    {
        List<UserCharacterData> list = null;
        if (uiType == STAT_TYPE.SUVIVOR)
            list = CharacterGameData.GetMySurvivorList();
        if (uiType == STAT_TYPE.CHASER)
            list = CharacterGameData.GetMyChaserList();

        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        if (targetCharcter <= 0)
            return;

        CharacterList.Clear();

        foreach (UserCharacterData data in list)
        {
            if (data.characterData.use)
            {
                ScrollUIControllerItem item = CharacterList.AddItem(data.characterData, OnCharacterSelect);
            }
            //(item as CharacterListItem).SetFocus(data.characterData.GetID() == targetCharcter);
        }

        foreach (CharacterGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character))
        {
            if ((uiType != STAT_TYPE.CHASER && data.IsChaserCharacter()) || (data.IsSuvivorCharacter() && uiType != STAT_TYPE.SUVIVOR))
                continue;

            if (Managers.UserData.MyCharacters.ContainsKey(data.GetID()))
                continue;

            if (data.use)
            {
                ScrollUIControllerItem item = CharacterList.AddItemNotMine(data, OnCharacterSelect);
            }
            //(item as CharacterListItem).SetFocus(data.characterData.GetID() == targetCharcter);
        }

        CharacterList.OnSorting();

        CharacterTypeTaps[0].sprite = TapImages[uiType == STAT_TYPE.SUVIVOR ? 0 : 2];
        CharacterTypeTaps[1].sprite = TapImages[uiType == STAT_TYPE.CHASER ? 1 : 3];
        CharacterTypeTexts[0].color = uiType == STAT_TYPE.SUVIVOR ? Color.black : Color.white;
        CharacterTypeTexts[1].color = uiType == STAT_TYPE.CHASER ? Color.black : Color.white;
        CharacterTypeTaps[0].GetComponent<Button>().interactable = uiType != STAT_TYPE.SUVIVOR;
        CharacterTypeTaps[1].GetComponent<Button>().interactable = uiType != STAT_TYPE.CHASER;

        RectTransform ScrollTransform = (CharacterList.transform as RectTransform);
        Vector2 scrollSize = ScrollTransform.sizeDelta;
        Vector2 canvasSize = (PopupCanvas.Instance.transform as RectTransform).sizeDelta;
        CanvasScalerExtension extension = GetComponentInParent<CanvasScalerExtension>();
        if (extension)
        {
            if (extension.UI.Length > 0 && extension.UI[0] != null)
            {
                canvasSize = extension.UI[0].rect.size;
            }
        }

        const int base_offset = 100;
        const int window_offset = 50;
        const int view_padding = 20;
        const int per_item_width = 198;

        int spaceX = (int)(canvasSize.x - (StatPanel.transform as RectTransform).sizeDelta.x) - (base_offset + window_offset + view_padding);
        int cellCount = spaceX / per_item_width;
        scrollSize.x = (cellCount * per_item_width) + (window_offset + view_padding);

        ScrollTransform.sizeDelta = scrollSize;
    }

    public void OnChaserUI()
    {
        if (uiType == STAT_TYPE.CHASER)
            return;
        uiType = STAT_TYPE.CHASER;
        RefreshUI();
        RefreshList();
    }

    public void OnSuvivorUI()
    {
        if (uiType == STAT_TYPE.SUVIVOR)
            return;
        uiType = STAT_TYPE.SUVIVOR;
        RefreshUI();
        RefreshList();
    }

    public void OnCharacterSelect(ScrollUIControllerItem caller)
    {
        CharacterListItem item = caller as CharacterListItem;
        if (item != null)
        {
            OnSelectCharacter(item.charData);
        }
    }

    public void OnSelectCharacter(CharacterGameData data)
    {
        if (data.IsChaserCharacter())
        {
            if (curChaser == data.GetID())
                return;
            curChaser = data.GetID();
        }
        else
        {
            if (curSurvivor == data.GetID())
                return;
            curSurvivor = data.GetID();
        }

        RefreshUI();
    }

    public void OnLevelUpUI()
    {
        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        levelUPUI.SetActive(true, targetCharcter);
    }

    public void OnMoveGacha()
    {
        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        CharacterGameData targetData = CharacterGameData.GetCharacterData(targetCharcter);
        if(targetData != null)
        {
            if(targetData.is_limited > 0)
            {
                PopupCanvas.Instance.ShowMessagePopup("limited_character_info");
                return;
            }
        }

        PopupCanvas.Instance.ShowConfirmPopup("ui_no_ch", () =>
        {
            //popup.Close();
            PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.GACHA_POPUP);
        });
    }

    public void OnEnchantUpUI()
    {
        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        enchantUPUI.SetActive(true, targetCharcter);
    }

    public void OnDefaultCharacterSetting()
    {
        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        popup.OnSelectCharacter(CharacterGameData.GetCharacterData(targetCharcter));
    }

    public void ToggleInfo()
    {
        int targetCharcter = -1;

        if (uiType == STAT_TYPE.SUVIVOR)
            targetCharcter = curSurvivor;
        if (uiType == STAT_TYPE.CHASER)
            targetCharcter = curChaser;

        if (Managers.UserData.GetMyCharacterInfo(targetCharcter) == null)
        {
            StatPanel.SetActive(true);
            PlayDataPanel.SetActive(false);
            return;
        }

        StatPanel.SetActive(!StatPanel.activeInHierarchy);
        PlayDataPanel.SetActive(!StatPanel.activeInHierarchy);

        string animName = "";
        switch (UnityEngine.Random.Range(0, 4))
        {
            case 0:
                animName = "f_action_0";
                break;
            case 1:
                animName = "f_victory_0";
                break;
            case 2:
                animName = "f_failure_0";
                break;
            case 3:
                if (uiType == STAT_TYPE.CHASER)
                    animName = "f_action_1";
                else
                    animName = "f_interecting_0";
                break;
        }

        var target = selectedCharacter.GetSkeletonGraphic();
        target.AnimationState.SetAnimation(0, animName, false);
        target.AnimationState.Complete += OnCompleteAnimation;
    }

    public void OnCompleteAnimation(Spine.TrackEntry entry)
    {
        var target = selectedCharacter.GetSkeletonGraphic();
        target.AnimationState.Complete -= OnCompleteAnimation;
        target.AnimationState.SetAnimation(0, "f_idle_0", true);
    }

    public int GetSelectedCharacterID()
    {
        return selectedCharacter.characterid;
    }

    public GameObject GetSubPopupObject()
    {
        if (levelUPUI.gameObject.activeInHierarchy)
            return levelUPUI.gameObject;
        else if (skillUPUI.gameObject.activeInHierarchy)
            return skillUPUI.gameObject;
        else if (enchantUPUI.gameObject.activeInHierarchy)
            return enchantUPUI.gameObject;

        return null;
    }
}
