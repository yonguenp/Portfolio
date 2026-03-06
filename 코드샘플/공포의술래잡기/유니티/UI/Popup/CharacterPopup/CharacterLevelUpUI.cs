using Coffee.UIExtensions;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLevelUpUI : MonoBehaviour
{
    enum EXP_ITEM
    {
        EXP_SMALL = 1,
        EXP_MEDIUM,
        EXP_LARGE,
    }
    [Serializable]
    public class StatUI
    {
        public Text statName;
        public Text curStat;
        public Text nextStat;
        public Text PlusStat;
    }

    [SerializeField]
    Text level;
    [SerializeField]
    Text nextlevel;

    [SerializeField]
    UIPortraitCharacter characterPortrait;
    [SerializeField]
    StatUI[] statUI;
    [SerializeField]
    Image curExpBar;
    [SerializeField]
    Image nextExpBar;
    [SerializeField]
    Text needExp;
    [SerializeField]
    Text needGold;
    [SerializeField]
    UIGrade gradeUI;

    [SerializeField]
    Button LevelUPButton;

    [SerializeField]
    Text[] useItemCountText;

    [SerializeField]
    GameObject[] downGameObject;
    [SerializeField]
    Image[] exp_itemIcon;

    [SerializeField]
    Transform AnimationPanel;

    [SerializeField]
    List<UIParticle> UIParticles = new List<UIParticle>();

    UserCharacterData targetUserData;
    int curCharacterID = 0;
    int[] itemUseCount = new int[3];
    int viewLevel = 0;
    int PreviewLevel { get { return GetPreviewLevel(); } }
    int PreviewExp { get { return GetPreviewExp(); } }
    Sequence seq = null;
    bool IsExpAnimation = false;
    public void SetActive(bool active)
    {
        if (IsExpAnimation && !active)
            return;

        gameObject.SetActive(active);

        if (active)
        {
            AnimationPanel.DOKill();
            AnimationPanel.localScale = Vector3.zero;
            AnimationPanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        else
            (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup).HideParticle();

    }

    int GetPreviewExp()
    {
        int curExp = 0;
        if (targetUserData != null)
            curExp = targetUserData.exp;

        return (curExp + (itemUseCount[2] * 1000) + (itemUseCount[1] * 100) + (itemUseCount[0] * 10));
    }

    int GetPreviewLevel()
    {
        if (targetUserData == null)
        {
            return 0;
        }

        int lv = targetUserData.lv;

        CharacterLevelGameData levelData = targetUserData.characterData.levelData[lv];
        if (levelData == null)
        {
            return 0;
        }

        while (levelData.need_exp <= PreviewExp && GetCurMaxLevel() > lv)
        {
            levelData = targetUserData.characterData.levelData[lv + 1];
            lv = lv + 1;
        }

        return lv;
    }

    int GetCurMaxLevel()
    {
        if (targetUserData == null)
        {
            return 0;
        }

        return targetUserData.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL;
    }

    public void SetActive(bool active, int target)
    {
        SetActive(active);
        curCharacterID = target;
        targetUserData = Managers.UserData.GetMyCharacterInfo(curCharacterID);
        RefreshUI();
    }

    public void RefreshUI()
    {
        CancelInvoke("RefreshUI");

        itemUseCount[0] = 0;
        itemUseCount[1] = 0;
        itemUseCount[2] = 0;
        viewLevel = targetUserData.lv;
        nextExpBar.fillAmount = 0.0f;

        gradeUI.SetGrade(targetUserData.characterData.char_grade);
        characterPortrait.SetPortrait(curCharacterID);

        SetLevelUI();
        OnPreviewUI();

        for (int i = 0; i < useItemCountText.Length; i++)
        {
            int itemNo = 0;
            switch (i)
            {
                case 0:
                    itemNo = 1;
                    break;
                case 1:
                    itemNo = 2;
                    break;
                case 2:
                    itemNo = 3;
                    break;
            }

            useItemCountText[i].text = itemUseCount[i].ToString() + "/" + Managers.UserData.GetMyItemCount(itemNo).ToString();

            if (Managers.UserData.GetMyItemCount(itemNo) == 0)
                useItemCountText[i].color = Color.red;
            else
            {
                if (itemUseCount[i] > 0)
                {
                    ColorUtility.TryParseHtmlString("#71FE55", out Color color);
                    useItemCountText[i].color = color;
                }
                else
                    useItemCountText[i].color = Color.white;
            }
        }

        LevelButtonSync(false);
        IsExpAnimation = false;
    }

    public void SetLevelUI()
    {
        int previewLevel = viewLevel;
        UserCharacterData data = targetUserData;
        level.text = "LV " + data.lv.ToString();
        nextlevel.text = "LV " + previewLevel.ToString();

        List<StatItem.STAT_TYPE> displayStat = new List<StatItem.STAT_TYPE>();


        if (!data.characterData.IsChaserCharacter())
        {
            displayStat.Add(StatItem.STAT_TYPE.HP);
            displayStat.Add(StatItem.STAT_TYPE.MOVE_SPEED);
            displayStat.Add(StatItem.STAT_TYPE.MAX_BATTERY);
        }
        else
        {
            displayStat.Add(StatItem.STAT_TYPE.ATK);
            displayStat.Add(StatItem.STAT_TYPE.MOVE_SPEED);
            displayStat.Add(StatItem.STAT_TYPE.ATTACK_COOL_TIME);
        }

        CharacterLevelGameData curLevelData = data.characterData.levelData[data.lv];
        CharacterLevelGameData nextLevelData = null;
        if (data.characterData.levelData.Length > previewLevel)
        {
            nextLevelData = data.characterData.levelData[previewLevel];
        }
        else
        {
            nextlevel.text = "MAX";
        }

        for (int i = 0; i < statUI.Length; i++)
        {
            if (displayStat.Count > i)
            {
                switch (displayStat[i])
                {
                    case StatItem.STAT_TYPE.MOVE_SPEED:
                        statUI[i].statName.text = StringManager.GetString("이동속도");
                        statUI[i].curStat.text = Mathf.RoundToInt((curLevelData.move_speed)).ToString();

                        if (nextLevelData != null)
                            statUI[i].PlusStat.text = "+ " + Mathf.RoundToInt(((nextLevelData.move_speed - curLevelData.move_speed))).ToString();
                        else
                            statUI[i].PlusStat.text = "";

                        if (nextLevelData != null)
                            statUI[i].nextStat.text = Mathf.RoundToInt((nextLevelData.move_speed)).ToString();
                        else
                            statUI[i].nextStat.text = StringManager.GetString("최고레벨");
                        break;
                    case StatItem.STAT_TYPE.HP:
                        statUI[i].statName.text = StringManager.GetString("체력");
                        statUI[i].curStat.text = Mathf.Floor(curLevelData.hp * 0.001f).ToString();

                        if (nextLevelData != null)
                            statUI[i].PlusStat.text = "+ " + Mathf.Floor((nextLevelData.hp - curLevelData.hp) * 0.001f < 1 ? 0 : (nextLevelData.hp - curLevelData.hp) * 0.001f).ToString();
                        else
                            statUI[i].PlusStat.text = "";

                        if (nextLevelData != null)
                            statUI[i].nextStat.text = Mathf.Floor(nextLevelData.hp * 0.001f).ToString();
                        else
                            statUI[i].nextStat.text = StringManager.GetString("최고레벨");
                        break;
                    case StatItem.STAT_TYPE.QUEST_SPEED:
                        statUI[i].statName.text = StringManager.GetString("수행력");
                        statUI[i].curStat.text = curLevelData.quest_speed.ToString();

                        if (nextLevelData != null)
                            statUI[i].PlusStat.text = "+ " + (nextLevelData.quest_speed - curLevelData.quest_speed).ToString();
                        else
                            statUI[i].PlusStat.text = "";

                        if (nextLevelData != null)
                            statUI[i].nextStat.text = nextLevelData.quest_speed.ToString();
                        else
                            statUI[i].nextStat.text = StringManager.GetString("최고레벨");
                        break;
                    case StatItem.STAT_TYPE.ATK:
                        statUI[i].statName.text = StringManager.GetString("공격력");
                        statUI[i].curStat.text = Mathf.Floor(curLevelData.atk_point * 0.001f).ToString();

                        if (nextLevelData != null)
                            statUI[i].PlusStat.text = "+ " + Mathf.Floor((nextLevelData.atk_point - curLevelData.atk_point) * 0.001f < 1 ? 0 : (nextLevelData.atk_point - curLevelData.atk_point) * 0.001f).ToString();
                        else
                            statUI[i].PlusStat.text = "";

                        if (nextLevelData != null)
                            statUI[i].nextStat.text = Mathf.Floor(nextLevelData.atk_point * 0.001f).ToString();
                        else
                            statUI[i].nextStat.text = StringManager.GetString("최고레벨");
                        break;
                    case StatItem.STAT_TYPE.ATTACK_COOL_TIME:
                        statUI[i].statName.text = StringManager.GetString("공격속도");
                        statUI[i].curStat.text = (curLevelData.attack_cool_time).ToString();

                        if (nextLevelData != null)
                            statUI[i].PlusStat.text = "+ " + Mathf.Floor((nextLevelData.attack_cool_time - curLevelData.attack_cool_time) < 1 ? 0 : (nextLevelData.attack_cool_time - curLevelData.attack_cool_time)).ToString();
                        else
                            statUI[i].PlusStat.text = "";

                        if (nextLevelData != null)
                            statUI[i].nextStat.text = (nextLevelData.attack_cool_time).ToString();
                        else
                            statUI[i].nextStat.text = StringManager.GetString("최고레벨");
                        break;
                    case StatItem.STAT_TYPE.MAX_BATTERY:
                        statUI[i].statName.text = StringManager.GetString("최대소지량");
                        statUI[i].curStat.text = (curLevelData.max_battary * 0.001f).ToString();

                        if (nextLevelData != null)
                            statUI[i].PlusStat.text = "+ " + Mathf.Floor((nextLevelData.max_battary - curLevelData.max_battary) * 0.001f < 1 ? 0 : (nextLevelData.max_battary - curLevelData.max_battary) * 0.001f).ToString();
                        else
                            statUI[i].PlusStat.text = "";

                        if (nextLevelData != null)
                            statUI[i].nextStat.text = (nextLevelData.max_battary * 0.001f).ToString();
                        else
                            statUI[i].nextStat.text = StringManager.GetString("최고레벨");
                        break;

                }
            }
            else
            {
                statUI[i].statName.text = "";
                statUI[i].curStat.text = "";
                statUI[i].nextStat.text = "";
                statUI[i].PlusStat.text = "";
            }
        }

        Vector2 size = (curExpBar.transform.parent as RectTransform).sizeDelta;
        size.y = (curExpBar.transform as RectTransform).sizeDelta.y;

        if (GetCurMaxLevel() == data.lv)
        {
            //size max
        }
        else if (data.lv == previewLevel)
        {
            int prevLv = curLevelData.level - 1;
            int prevLvExp = 0;
            if (prevLv > 0 && data.characterData.levelData.Length > prevLv)
            {
                prevLvExp = data.characterData.levelData[prevLv].need_exp;
            }

            size.x = size.x * ((float)(data.exp - prevLvExp) / (curLevelData.need_exp - prevLvExp));
        }
        else
        {
            size.x = 0;
        }

        (curExpBar.transform as RectTransform).sizeDelta = size;

        if (GetCurMaxLevel() == previewLevel)
        {
            needExp.text = "MAX";
            needExp.color = new Color(0.4431373f, 0.9960784f, 0.3333333f);
        }
        else
        {
            needExp.text = (curLevelData.need_exp - data.exp).ToString();
            needExp.color = Color.white;
        }

        exp_itemIcon[0].sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/item_exp_small");
        exp_itemIcon[1].sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/item_exp_medium");
        exp_itemIcon[2].sprite = Managers.Resource.LoadAssetsBundle<Sprite>("AssetsBundle/Texture/Icon/item_exp_large");
    }

    private void OnPreviewUI()
    {
        UserCharacterData data = targetUserData;
        int lv = data.lv;

        int previewExp = PreviewExp;
        if (PreviewLevel == GetCurMaxLevel())
        {
            if (previewExp > data.characterData.levelData[PreviewLevel].need_exp)
                previewExp = data.characterData.levelData[PreviewLevel].need_exp;
        }
        CharacterLevelGameData previewLevelData = data.characterData.levelData[PreviewLevel];

        Vector2 size = (nextExpBar.transform.parent as RectTransform).sizeDelta;
        size.y = (nextExpBar.transform as RectTransform).sizeDelta.y;
        if (PreviewLevel != GetCurMaxLevel())
        {
            int prevLv = previewLevelData.level - 1;
            int prevLvExp = 0;
            if (prevLv > 0 && data.characterData.levelData.Length > prevLv)
            {
                prevLvExp = data.characterData.levelData[prevLv].need_exp;
            }

            size.x = size.x * ((float)(previewExp - prevLvExp) / (previewLevelData.need_exp - prevLvExp));
            size.x = Mathf.Min(size.x, (nextExpBar.transform.parent as RectTransform).sizeDelta.x);
        }

        nextExpBar.transform.DOKill();

        if (UIParticles.Find(_ => _.name == "fx_gaugebar_00") != null)
            UIParticles.Find(_ => _.name == "fx_gaugebar_00").Stop();

        if (viewLevel < PreviewLevel)
        {
            int loopCount = PreviewLevel - viewLevel;

            if (seq != null)
                seq.Kill();

            seq = DOTween.Sequence();
            for (int i = 0; i < loopCount; i++)
            {
                seq.Append((nextExpBar.transform as RectTransform).DOSizeDelta(new Vector2((nextExpBar.transform.parent as RectTransform).sizeDelta.x, size.y), 0.1f));
                seq.AppendCallback(() =>
                {
                    (nextExpBar.transform as RectTransform).sizeDelta = new Vector2(0, size.y);
                    viewLevel++;
                    SetLevelUI();
                });
            }

            seq.Append((nextExpBar.transform as RectTransform).DOSizeDelta(size, 0.1f));
            seq.AppendCallback(() =>
            {
                viewLevel = PreviewLevel;
                SetLevelUI();
            });

        }
        else if ((nextExpBar.transform as RectTransform).sizeDelta.x < size.x)
        {
            (nextExpBar.transform as RectTransform).DOSizeDelta(size, 0.2f).OnComplete(() =>
           {
           });
            viewLevel = PreviewLevel;
            SetLevelUI();
        }
        else
        {
            (nextExpBar.transform as RectTransform).sizeDelta = size;
            viewLevel = PreviewLevel;
            SetLevelUI();
        }

        if (PreviewLevel == GetCurMaxLevel())
        {
            needExp.text = "MAX";
            needExp.color = new Color(0.4431373f, 0.9960784f, 0.3333333f);
            UIParticles.Find(_ => _.name == "fx_gaugebar_00").Play();
        }
        else
        {
            needExp.text = (previewLevelData.need_exp - previewExp).ToString();
            needExp.color = Color.white;
        }

        int needgold = 0;
        for (int i = 0; i < useItemCountText.Length; i++)
        {
            int itemNo = 0;
            switch (i)
            {
                case 0:
                    itemNo = 1;
                    needgold += GameConfig.Instance.SMALL_EXP_ITEM_GOLD * itemUseCount[i];
                    break;
                case 1:
                    itemNo = 2;
                    needgold += GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD * itemUseCount[i];
                    break;
                case 2:
                    itemNo = 3;
                    needgold += GameConfig.Instance.LARGE_EXP_ITEM_GOLD * itemUseCount[i];
                    break;
            }

            useItemCountText[i].text = itemUseCount[i].ToString() + "/" + Managers.UserData.GetMyItemCount(itemNo).ToString();
            if (Managers.UserData.GetMyItemCount(itemNo) == 0)
                useItemCountText[i].color = Color.red;
            else
            {
                if (itemUseCount[i] > 0)
                {
                    ColorUtility.TryParseHtmlString("#71FE55", out Color color);
                    useItemCountText[i].color = color;
                }
                else
                    useItemCountText[i].color = Color.white;
            }
            downGameObject[i].SetActive(itemUseCount[i] > 0);
        }

        if (needGold != null)
        {
            if (targetUserData.lv == GetCurMaxLevel() || targetUserData.lv >= GameConfig.Instance.MAX_CHARACTER_LEVEL)
            {
                LevelButtonSync(false);
                needGold.color = Color.grey;

                if (targetUserData.lv >= GameConfig.Instance.MAX_CHARACTER_LEVEL)
                    needGold.text = StringManager.GetString("max_level");
                else
                    needGold.text = StringManager.GetString("need_enchant");
            }
            else if (needgold <= 0)
            {
                LevelUPButton.interactable = false;
                needGold.color = Color.grey;

                needGold.text = "0";
            }
            else
            {
                needGold.text = needgold.ToString();

                if (needgold > Managers.UserData.MyGold)
                {
                    needGold.color = Color.red;
                    LevelButtonSync(false);
                }
                else
                {
                    needGold.color = Color.white;
                    LevelButtonSync(true);
                }
            }
        }


    }

    public void OnSmallExpItem(int repeat)
    {
        OnUseExpItem(EXP_ITEM.EXP_SMALL, repeat);
    }
    public void OnMiddleExpItem(int repeat)
    {
        OnUseExpItem(EXP_ITEM.EXP_MEDIUM, repeat);
    }
    public void OnLargeExpItem(int repeat)
    {
        OnUseExpItem(EXP_ITEM.EXP_LARGE, repeat);
    }

    public void CancelSmallExpItem(int repeat)
    {
        OnCancelExpItem(EXP_ITEM.EXP_SMALL, repeat);
    }
    public void CancelMiddleExpItem(int repeat)
    {
        OnCancelExpItem(EXP_ITEM.EXP_MEDIUM, repeat);
    }
    public void CancelLargeExpItem(int repeat)
    {
        OnCancelExpItem(EXP_ITEM.EXP_LARGE, repeat);
    }

    void OnUseExpItem(EXP_ITEM type, int amount = 1)
    {
        if (IsExpAnimation)
            return;

        int previewLevel = PreviewLevel;
        int itemNo = 0;
        UserCharacterData data = targetUserData;

        switch (type)
        {
            case EXP_ITEM.EXP_SMALL:
                itemNo = 1;
                if (Managers.UserData.GetMyItemCount(itemNo) <= itemUseCount[0])
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"));
                    break;
                }

                if (previewLevel >= data.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL)
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_reinforce_short"));
                    return;
                }

                if (previewLevel < GameConfig.Instance.MAX_CHARACTER_LEVEL)
                {
                    itemUseCount[0] += amount;
                    if (itemUseCount[0] > Managers.UserData.GetMyItemCount(itemNo))
                        itemUseCount[0] = Managers.UserData.GetMyItemCount(itemNo);
                }
                else
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템 부족"));
                    return;
                }
                break;
            case EXP_ITEM.EXP_MEDIUM:
                itemNo = 2;
                if (Managers.UserData.GetMyItemCount(itemNo) <= itemUseCount[1])
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"));
                    break;
                }

                if (previewLevel >= data.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL)
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_reinforce_short"));
                    return;
                }

                if (previewLevel < GameConfig.Instance.MAX_CHARACTER_LEVEL)
                {
                    itemUseCount[1] += amount;
                    if (itemUseCount[1] > Managers.UserData.GetMyItemCount(itemNo))
                        itemUseCount[1] = Managers.UserData.GetMyItemCount(itemNo);
                }
                else
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템 부족"));
                    return;
                }
                break;
            case EXP_ITEM.EXP_LARGE:
                itemNo = 3;
                if (Managers.UserData.GetMyItemCount(itemNo) <= itemUseCount[2])
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"));
                    break;
                }

                if (previewLevel >= data.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL)
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_reinforce_short"));
                    return;
                }

                if (previewLevel < GameConfig.Instance.MAX_CHARACTER_LEVEL)
                {
                    itemUseCount[2] += amount;
                    if (itemUseCount[2] > Managers.UserData.GetMyItemCount(itemNo))
                        itemUseCount[2] = Managers.UserData.GetMyItemCount(itemNo);
                }
                else
                {
                    PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템 부족"));
                    return;
                }
                break;
            default:
                return;
        }
        Managers.Sound.Play("effect/EF_LEVELUP_BAR", Sound.Effect);
        OnPreviewUI();

        if (PreviewLevel >= data.enchant * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL)
        {
            CharacterLevelGameData levelData = data.characterData.levelData[PreviewLevel - 1];
            while (levelData.need_exp < (data.exp + (itemUseCount[2] * 1000) + (itemUseCount[1] * 100) + (itemUseCount[0] * 10)))
            {
                int diff = (data.exp + (itemUseCount[2] * 1000) + (itemUseCount[1] * 100) + (itemUseCount[0] * 10)) - levelData.need_exp;
                if (diff > 1000 && itemUseCount[2] > 0)
                {
                    itemUseCount[2]--;
                }
                else if (diff > 100 && itemUseCount[1] > 0)
                {
                    itemUseCount[1]--;
                }
                else if (diff > 10 && itemUseCount[0] > 0)
                {
                    itemUseCount[0]--;
                }
                else // 경험치를 다 채우지 못하고 아이템이 모자랄 때
                {
                    break;
                }
            }

            OnPreviewUI();
        }


    }

    void OnCancelExpItem(EXP_ITEM type, int amount = 1)
    {
        if (IsExpAnimation)
            return;

        switch (type)
        {
            case EXP_ITEM.EXP_SMALL:
                if (itemUseCount[0] <= 0)
                    return;
                itemUseCount[0] -= amount;
                if (itemUseCount[0] < 0)
                    itemUseCount[0] = 0;
                break;
            case EXP_ITEM.EXP_MEDIUM:
                if (itemUseCount[1] <= 0)
                    return;
                itemUseCount[1] -= amount;
                if (itemUseCount[1] < 0)
                    itemUseCount[1] = 0;
                break;
            case EXP_ITEM.EXP_LARGE:
                if (itemUseCount[2] <= 0)
                    return;
                itemUseCount[2] -= amount;
                if (itemUseCount[2] < 0)
                    itemUseCount[2] = 0;
                break;
            default:
                return;
        }

        OnPreviewUI();
    }

    public void OnMax()
    {
        if (IsExpAnimation)
            return;

        Managers.Sound.Play("effect/EF_LEVELUP_BAR", Sound.Effect);
        RefreshUI();

        int myGold = Managers.UserData.MyGold;

        UserCharacterData data = targetUserData;
        if (data.lv >= GameConfig.Instance.MAX_CHARACTER_LEVEL)
        {
            return;
        }
        if (data.lv >= GetCurMaxLevel())
        {
            return;
        }

        int[] items = { 1, 2, 3 };
        int[] itemCount = {
            Managers.UserData.GetMyItemCount(items[0]),
            Managers.UserData.GetMyItemCount(items[1]),
            Managers.UserData.GetMyItemCount(items[2]),
        };
        int[] itemValue =
        {
            ItemGameData.GetItemData(items[0]).value,
            ItemGameData.GetItemData(items[1]).value,
            ItemGameData.GetItemData(items[2]).value,
        };

        int exp = data.exp;

        CharacterLevelGameData maxLevelData = data.characterData.levelData[GetCurMaxLevel() - 1];
        while (maxLevelData.need_exp >= exp && myGold > 0)
        {
            int diff = maxLevelData.need_exp - exp;
            if (diff >= itemValue[2] && itemCount[2] > itemUseCount[2])
            {
                itemUseCount[2]++;
                int add = itemValue[2];
                exp += add;
                myGold -= GameConfig.Instance.LARGE_EXP_ITEM_GOLD;
            }
            else if (diff >= itemValue[1] && itemCount[1] > itemUseCount[1])
            {
                itemUseCount[1]++;
                int add = itemValue[1];
                exp += add;
                myGold -= GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD;
            }
            else if (diff >= itemValue[0] && itemCount[0] > itemUseCount[0])
            {
                itemUseCount[0]++;
                int add = itemValue[0];
                exp += add;
                myGold -= GameConfig.Instance.SMALL_EXP_ITEM_GOLD;
            }
            else // 경험치를 다 채우지 못하고 아이템이 모자랄 때
            {
                if (itemCount[0] > itemUseCount[0])
                {
                    itemUseCount[0]++;
                    int add = itemValue[0];
                    exp += add;
                    myGold -= GameConfig.Instance.SMALL_EXP_ITEM_GOLD;
                }
                else if (itemCount[1] > itemUseCount[1])
                {
                    itemUseCount[1]++;
                    int add = itemValue[1];
                    exp += add;
                    myGold -= GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD;
                }
                else if (itemCount[2] > itemUseCount[2])
                {
                    itemUseCount[2]++;
                    int add = itemValue[2];
                    exp += add;
                    myGold -= GameConfig.Instance.LARGE_EXP_ITEM_GOLD;
                }
                else
                {
                    break;
                }
            }
        }

        if (myGold <= 0)
        {
            while (myGold < 0)
            {
                if (itemUseCount[2] > 0 && Mathf.Abs(myGold) >= GameConfig.Instance.LARGE_EXP_ITEM_GOLD)
                {
                    itemUseCount[2]--;
                    exp -= itemValue[2];
                    myGold += GameConfig.Instance.LARGE_EXP_ITEM_GOLD;
                }
                else if (itemUseCount[1] > 0 && Mathf.Abs(myGold) >= GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD)
                {
                    itemUseCount[1]--;
                    exp -= itemValue[1];
                    myGold += GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD;
                }
                else if (itemUseCount[0] > 0 && Mathf.Abs(myGold) >= GameConfig.Instance.SMALL_EXP_ITEM_GOLD)
                {
                    itemUseCount[0]--;
                    exp -= itemValue[0];
                    myGold += GameConfig.Instance.SMALL_EXP_ITEM_GOLD;
                }
                else
                {
                    if (itemUseCount[0] > 0)
                    {
                        itemUseCount[0]--;
                        exp -= itemValue[0];
                        myGold += GameConfig.Instance.SMALL_EXP_ITEM_GOLD;
                    }
                    else if (itemUseCount[1] > 0)
                    {
                        itemUseCount[1]--;
                        exp -= itemValue[1];
                        myGold += GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD;
                    }
                    else if (itemUseCount[2] > 0)
                    {
                        itemUseCount[2]--;
                        exp -= itemValue[2];
                        myGold += GameConfig.Instance.LARGE_EXP_ITEM_GOLD;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            PopupCanvas.Instance.ShowConfirmPopup("골드부족상점이동", () =>
            {
                PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
            });
        }

        if (maxLevelData.need_exp < exp)
        {
            while (maxLevelData.need_exp < exp)
            {
                if (itemUseCount[2] > 0 && maxLevelData.need_exp <= (exp - itemValue[2]))
                {
                    itemUseCount[2]--;
                    exp -= itemValue[2];
                }
                else if (itemUseCount[1] > 0 && maxLevelData.need_exp <= (exp - itemValue[1]))
                {
                    itemUseCount[1]--;
                    exp -= itemValue[1];
                }
                else if (itemUseCount[0] > 0 && maxLevelData.need_exp <= (exp - itemValue[0]))
                {
                    itemUseCount[0]--;
                    exp -= itemValue[0];
                }
                else
                {
                    break;
                }
            }
        }

        OnPreviewUI();
    }

    public void CheckGold()
    {
        int needgold = 0;
        for (int i = 0; i < itemUseCount.Length; i++)
        {
            int itemNo = 0;
            switch (i)
            {
                case 0:
                    needgold += GameConfig.Instance.SMALL_EXP_ITEM_GOLD * itemUseCount[i];
                    break;
                case 1:
                    needgold += GameConfig.Instance.MEDIUM_EXP_ITEM_GOLD * itemUseCount[i];
                    break;
                case 2:
                    needgold += GameConfig.Instance.LARGE_EXP_ITEM_GOLD * itemUseCount[i];
                    break;
            }
        }

        if (needgold > Managers.UserData.MyGold)
        {
            PopupCanvas.Instance.ShowConfirmPopup("골드부족상점이동", () =>
            {
                PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
            });
        }
    }

    public void OnClear()
    {
        if (IsExpAnimation)
            return;

        RefreshUI();
    }

    public void OnLevelUpSend()
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("매치대기캐릭터변경불가"));
            return;
        }

        LevelButtonSync(false);
        IsExpAnimation = true;
        int curLevel = Managers.UserData.GetMyCharacterInfo(curCharacterID).lv;
        SBWeb.CharacterExpUp(curCharacterID, itemUseCount[0], itemUseCount[1], itemUseCount[2],
            () =>
            {
                bool isLevelUp = Managers.UserData.GetMyCharacterInfo(curCharacterID).lv != curLevel;
                for (int i = 0; i < 3; i++)
                {
                    if (itemUseCount[i] > 0)
                    {
                        Managers.Sound.Play("effect/EF_LEVELUP_EX_UP", Sound.Effect);
                        Sequence seq = DOTween.Sequence();

                        seq.AppendInterval(1.2f);
                        seq.AppendCallback(() =>
                        {
                            UIParticles.Find(_ => _.name == "fx_gaugebar_00").Invoke("Stop", 1.0f);

                            if (isLevelUp)
                            {
                                VibrateManager.OnVibrate(0.5f, 100);
                                UIParticles.Find(_ => _.name == "fx_firework00").Play();

                                level.transform.DOScale(Vector3.one * 1.2f, 0.1f).SetDelay(0.5f).OnComplete(() =>
                                {
                                    level.transform.DOScale(Vector3.one, 0.1f);
                                });
                                Managers.Sound.Play("effect/EF_LEVELUP", Sound.Effect);
                            }
                        });
                        UIParticles.Find(_ => _.name == "fx_gaugebar_00").Play();
                        UIParticles.Find(_ => _.name == "fx_arrow00").Play();
                        UIParticles.Find(_ => _.name == "fx_cha_box00").Play();
                    }
                }
                seq.Kill();
                //SetLevelUI();
                RefreshUI();
                //Invoke("RefreshUI", 1.7f);

                //if (isLevelUp)
                //{
                //    (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup).OnParticleEffect(level.transform, 1.7f);
                //}
            }, () =>
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("게임서버오류"));
                RefreshUI();
            });
    }

    void LevelButtonSync(bool able)
    {
        foreach (MaskableGraphic graphic in LevelUPButton.GetComponentsInChildren<MaskableGraphic>())
        {
            if (graphic.GetComponent<Image>() != null || (graphic.GetComponent<Text>() != null && graphic.gameObject.name == "amount"))
            {
                if (able)
                    graphic.color = LevelUPButton.colors.normalColor;
                else
                    graphic.color = LevelUPButton.colors.disabledColor;
            }
        }

        LevelUPButton.interactable = able;
    }
}
