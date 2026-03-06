using Coffee.UIExtensions;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TalentPopup : Popup
{
    [SerializeField] Text talent_cur_id_text;
    [SerializeField] Text talent_cur_detail_text;

    [SerializeField] Text talent_id_text;
    [SerializeField] Text talent_detail_text;

    [SerializeField] Image need_item_icon_1;
    [SerializeField] Text need_item_amount_1;
    [SerializeField] Image need_item_icon_2;
    [SerializeField] Text need_item_amount_2;

    [SerializeField] GameObject talent_lists;
    [SerializeField] GameObject uiRoot;
    [SerializeField] GameObject talentSampleItem;

    [SerializeField] Button tryButton;
    [SerializeField] Button trySpecialButton;
    [SerializeField] Image SpecialIcon;
    [SerializeField] Text SpecialAmount;
    [SerializeField] Text SpecialText;
    [SerializeField] Button resetButton;
    [SerializeField] Button applyButton;

    [SerializeField] List<TalentArray> talentArrays = new List<TalentArray>();
    [SerializeField] UIParticle fx_talant00;
    [SerializeField] UIGrade curGrade;
    [SerializeField] UIGrade preGrade;

    private UserCharacterData characterData;
    private int slot_idx;
    private int pick_index = 0;
    public class TalentArray
    {
        public CharacterTalent data;
        public int min;
        public int max;
    }
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
        SubPopupSetactive(false);
    }
    public override void Close()
    {
        if (pick_index > 0)
        {
            PopupCanvas.Instance.ShowFadeText("선택필요");
            return;
        }

        base.Close();
    }
    public override void RefreshUI()
    {
        base.RefreshUI();

        if (slot_idx == 0 || characterData == null)
            return;

        talent_detail_text.transform.localScale = Vector3.one;
        talent_cur_detail_text.transform.localScale = Vector3.one;

        talent_id_text.text = StringManager.GetString("ui_option_num", slot_idx);
        talent_cur_id_text.text = StringManager.GetString("ui_option_num", slot_idx);

        int need_soul = 0;
        int need_gold = 0;
        CharacterTalent curTalentData = null;
        switch (slot_idx)
        {
            case 1:
                if (characterData.talent1 > 0)
                {
                    curTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, characterData.talent1) as CharacterTalent;
                }

                need_soul = GameConfig.Instance.TALENT1_SOUL;
                need_gold = GameConfig.Instance.TALENT1_GOLD;
                break;
            case 2:
                if (characterData.talent2 > 0)
                {
                    curTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, characterData.talent2) as CharacterTalent;
                }

                need_soul = GameConfig.Instance.TALENT2_SOUL;
                need_gold = GameConfig.Instance.TALENT2_GOLD;
                break;
            case 3:
                if (characterData.talent3 > 0)
                {
                    curTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, characterData.talent3) as CharacterTalent;
                }

                need_soul = GameConfig.Instance.TALENT3_SOUL;
                need_gold = GameConfig.Instance.TALENT3_GOLD;
                break;
        }

        if (pick_index > 0)
        {
            need_soul = 0;
            need_gold = 0;
        }

        need_item_amount_1.text = need_soul.ToString();
        need_item_amount_2.text = need_gold.ToString();


        bool enable = true;
        if (Managers.UserData.GetMyItemCount(4) < need_soul)
        {
            enable = false;
            need_item_amount_1.color = Color.red;
        }
        else
            need_item_amount_1.color = Color.white;
        if (Managers.UserData.MyGold < need_gold)
        {
            enable = false;
            need_item_amount_2.color = Color.red;
        }
        else
            need_item_amount_2.color = Color.white;

        tryButton.interactable = enable;

        RefreshSpecialButton();

        Color textColor = Color.white;
        talent_cur_detail_text.transform.parent.transform.Find("num").gameObject.SetActive(true);
        if (curTalentData == null)
        {
            talent_cur_detail_text.text = StringManager.GetString("재능미발견");
            talent_cur_detail_text.transform.parent.transform.Find("num").gameObject.SetActive(false);
            talent_cur_detail_text.transform.parent.transform.Find("num").GetComponent<Text>().text = string.Empty;
            curGrade.gameObject.SetActive(false);
        }
        else
        {
            talent_cur_detail_text.text = curTalentData.GetName();
            talent_cur_detail_text.transform.parent.transform.Find("num").GetComponent<Text>().text = curTalentData.GetValue();
            curGrade.gameObject.SetActive(true);
            curGrade.SetGrade(curTalentData.talent_rank);

            if (curTalentData.talent_rank >= GameConfig.Instance.MAX_TALENT_RANK)
            {
                ColorUtility.TryParseHtmlString("#fff265", out textColor);
            }
        }

        talent_cur_detail_text.color = textColor;
        talent_cur_detail_text.transform.parent.transform.Find("num").GetComponent<Text>().color = textColor;

        if (pick_index > 0)
        {
            var candidateTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, pick_index) as CharacterTalent;
            talent_detail_text.text = candidateTalentData.GetName();
            talent_detail_text.transform.Find("num").GetComponent<Text>().text = candidateTalentData.GetValue();
            preGrade.SetGrade(candidateTalentData.talent_rank);
            preGrade.gameObject.SetActive(true);

            Color color = Color.white;
            if (candidateTalentData.talent_rank >= GameConfig.Instance.MAX_TALENT_RANK)
            {
                ColorUtility.TryParseHtmlString("#fff265", out color);
            }
            else
            {
                if (curTalentData == null)
                {
                    ColorUtility.TryParseHtmlString("#89FF66", out color);
                }
                else if (curTalentData.talent_rank > candidateTalentData.talent_rank)
                {
                    ColorUtility.TryParseHtmlString("#FF667D", out color);
                }
                else if (curTalentData.talent_rank < candidateTalentData.talent_rank)
                {
                    ColorUtility.TryParseHtmlString("#89FF66", out color);
                }
            }

            talent_detail_text.color = color;
            talent_detail_text.transform.Find("num").GetComponent<Text>().color = color;

        }
        else
        {
            talent_detail_text.color = Color.white;
            talent_detail_text.transform.Find("num").GetComponent<Text>().color = Color.white;

            talent_detail_text.text = "";
            talent_detail_text.transform.Find("num").GetComponent<Text>().text = "";
            preGrade.gameObject.SetActive(false);
        }
    }

    void RefreshSpecialButton()
    {
        bool active = tryButton.gameObject.activeSelf && GameConfig.Instance.SPECIAL_TALENT_ITEM_NO > 0;
        trySpecialButton.gameObject.SetActive(active);

        if (active)
        {
            int specialItemCount = Managers.UserData.GetMyItemCount(GameConfig.Instance.SPECIAL_TALENT_ITEM_NO);
            trySpecialButton.interactable = (specialItemCount >= slot_idx);
            SpecialAmount.text = StringManager.GetString("ui_count", slot_idx);
            SpecialIcon.sprite = ItemGameData.GetItemIcon(GameConfig.Instance.SPECIAL_TALENT_ITEM_NO);
            if (specialItemCount >= slot_idx)
            {
                SpecialAmount.color = Color.black;
                SpecialText.color = Color.black;
            }
            else
            {
                SpecialAmount.color = Color.red;
                SpecialText.color = Color.red;
            }
        }
    }

    public void Init(int idx)
    {
        pick_index = 0;
        slot_idx = idx;
        var characterid = (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup).GetUI().GetSelectedCharacterID();
        characterData = Managers.UserData.GetMyCharacterInfo(characterid);

        tryButton.gameObject.SetActive(true);        
        resetButton.gameObject.SetActive(false);
        applyButton.gameObject.SetActive(false);

        RefreshUI();
    }

    public void TryTalent()
    {
        tryButton.gameObject.SetActive(false);
        trySpecialButton.gameObject.SetActive(false);

        SBWeb.TryCharacterTalent(characterData.characterData.GetID(), slot_idx, (response) =>
        {
            Anim();
            pick_index = response["pick"].Value<int>();

            RefreshUI();
        });
    }

    public void TrySpecialTalent()
    {
        tryButton.gameObject.SetActive(false);
        trySpecialButton.gameObject.SetActive(false);

        SBWeb.TryCharacterTalent(characterData.characterData.GetID(), slot_idx, (response) =>
        {
            Anim();
            pick_index = response["pick"].Value<int>();

            RefreshUI();
        }, true);
    }

    public void Anim()
    {
        var clip = Managers.Resource.LoadAssetsBundle<AudioClip>("AssetsBundle/Sounds/effect/EF_SLOT");
        if (clip != null)
            Managers.Sound.Play(clip, Sound.Effect);

        StartCoroutine(TalentAnim());
    }
    IEnumerator TalentAnim()
    {
        applyButton.interactable = false;
        resetButton.interactable = false;
        applyButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
        float time = 1;
        var talent_d = Managers.Data.GetData(GameDataManager.DATA_TYPE.character_talent);
        int idx = 0;

        CharacterTalent curTalentData = null;
        switch (slot_idx)
        {
            case 1:
                if (characterData.talent1 > 0)
                {
                    curTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, characterData.talent1) as CharacterTalent;
                }
                break;
            case 2:
                if (characterData.talent2 > 0)
                {
                    curTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, characterData.talent2) as CharacterTalent;
                }
                break;
            case 3:
                if (characterData.talent3 > 0)
                {
                    curTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, characterData.talent3) as CharacterTalent;
                }
                break;
        }
        preGrade.gameObject.SetActive(true);
        while (time > 0)
        {
            idx = Random.Range(0, talent_d.Count);
            if (talent_d[idx] != null)
            {
                CharacterTalent candidateTalentData = (talent_d[idx] as CharacterTalent);
                talent_detail_text.text = candidateTalentData.GetName();
                talent_detail_text.transform.Find("num").GetComponent<Text>().text = (talent_d[idx] as CharacterTalent).GetValue();

                Color color = Color.white;
                if (candidateTalentData.talent_rank >= GameConfig.Instance.MAX_TALENT_RANK)
                {
                    ColorUtility.TryParseHtmlString("#fff265", out color);
                }
                else
                {
                    if (curTalentData == null)
                    {
                        ColorUtility.TryParseHtmlString("#89FF66", out color);
                    }
                    else if (curTalentData.talent_rank > candidateTalentData.talent_rank)
                    {
                        ColorUtility.TryParseHtmlString("#FF667D", out color);
                    }
                    else if (curTalentData.talent_rank < candidateTalentData.talent_rank)
                    {
                        ColorUtility.TryParseHtmlString("#89FF66", out color);
                    }
                }

                talent_detail_text.color = color;
                talent_detail_text.transform.Find("num").GetComponent<Text>().color = color;
                preGrade.SetGrade(candidateTalentData.talent_rank);
            }
            time -= 0.05f;
            yield return new WaitForSeconds(0.02f);
        }
        if (pick_index > 0)
        {
            CharacterTalent candidateTalentData = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.character_talent, pick_index) as CharacterTalent;
            talent_detail_text.text = candidateTalentData.GetName();
            talent_detail_text.transform.Find("num").GetComponent<Text>().text = candidateTalentData.GetValue();

            Color color = Color.white;
            talent_detail_text.color = color;
            talent_detail_text.transform.Find("num").GetComponent<Text>().color = color;

            preGrade.gameObject.SetActive(false);
            talent_detail_text.transform.localScale = Vector3.one;
            talent_detail_text.transform.DOScale(1.8f, 0.1f).OnComplete(() =>
            {
                Color color = Color.white;
                if (candidateTalentData.talent_rank >= GameConfig.Instance.MAX_TALENT_RANK)
                {
                    ColorUtility.TryParseHtmlString("#fff265", out color);
                }
                else
                {
                    if (curTalentData == null)
                    {
                        ColorUtility.TryParseHtmlString("#89FF66", out color);
                    }
                    else if (curTalentData.talent_rank > candidateTalentData.talent_rank)
                    {
                        ColorUtility.TryParseHtmlString("#FF667D", out color);
                    }
                    else if (curTalentData != null && curTalentData.talent_rank < candidateTalentData.talent_rank)
                    {
                        ColorUtility.TryParseHtmlString("#89FF66", out color);
                    }
                }

                talent_detail_text.color = color;
                talent_detail_text.transform.Find("num").GetComponent<Text>().color = color;
                preGrade.SetGrade(candidateTalentData.talent_rank);
                preGrade.gameObject.SetActive(true);
                talent_detail_text.transform.DOScale(1f, 0.05f);
            });
        }

        yield return new WaitForSeconds(0.1f);

        applyButton.interactable = true;
        resetButton.interactable = curTalentData != null;
        applyButton.gameObject.SetActive(true);
        resetButton.gameObject.SetActive(true);
    }

    public void ApplyTalent()
    {
        resetButton.gameObject.SetActive(false);
        applyButton.gameObject.SetActive(false);

        SBWeb.ApplyCharacterTalent(characterData.characterData.GetID(), slot_idx, pick_index, (response) =>
        {
            talent_cur_detail_text.transform.DOScale(2f, 0.1f).OnComplete(() =>
            {

                talent_cur_detail_text.transform.DOScale(1f, 0.05f);
            });

            fx_talant00.Play();
            tryButton.gameObject.SetActive(true);
            RefreshSpecialButton();

            Init(slot_idx);
        });
    }


    public void ResetTalent()
    {
        Init(slot_idx);
    }

    public void SubPopupSetactive(bool enable)
    {
        if (enable)
        {
            uiRoot.SetActive(false);
            SubPopupRefreshUI();
        }
        else
        {
            uiRoot.SetActive(true);
        }

        talent_lists.SetActive(enable);
    }
    public void SubPopupRefreshUI()
    {
        TalentListClear();
        var talent_d = Managers.Data.GetData(GameDataManager.DATA_TYPE.character_talent);

        foreach (CharacterTalent item in talent_d)
        {
            TalentArray ary = new TalentArray();
            ary.data = item;
            ary.min = item.talent_value;
            ary.max = item.talent_value;

            talentArrays.Add(ary);

            foreach (TalentArray talent in talentArrays)
            {
                if (ary == talent)
                    continue;

                if (talent.data.talent_group == ary.data.talent_group)
                {
                    if (talent.min > ary.min)
                        talent.min = ary.min;
                    if (talent.max < ary.max)
                        talent.max = ary.max;
                    talentArrays.Remove(ary);
                    break;
                }
            }

        }

        talentSampleItem.gameObject.SetActive(true);
        foreach (TalentArray item in talentArrays)
        {
            if (item.data.talent_use_type != characterData.characterData.char_type || slot_idx != item.data.talent_grade)
                continue;

            var _obj = GameObject.Instantiate(talentSampleItem, talentSampleItem.transform.parent);
            if (item.min == item.max)
            {
                if (item.data.ui_text_type == 1) //그냥 정수 표기
                    _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min}";
                else if (item.data.ui_text_type == 2) // * 0.1f 곱 표기
                    _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min * 0.1f}%";
                else if (item.data.ui_text_type == 3) // * 0.001f 곱 표기 정수
                    _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min * 0.001f}";

            }
            else if (item.data.ui_text_type == 1) // 그냥 정수 표기
                _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min} ~ {item.max}";
            else if (item.data.ui_text_type == 2) // * 0.1f 곱 표기
                _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min * 0.1f}% ~ {item.max * 0.1f}%";
            else if (item.data.ui_text_type == 3) // * 0.001f 곱 표기 정수
                _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min * 0.001f} ~ {item.max * 0.001f}";

            else //표기되면 문제가 있는것
                _obj.transform.GetComponentInChildren<Text>().text = item.data.GetName() + $" {item.min} ~ {item.max}";
        }
        talentSampleItem.gameObject.SetActive(false);
    }

    public void TalentListClear()
    {
        foreach (Transform item in talentSampleItem.transform.parent)
        {
            if (item == talentSampleItem.transform)
                continue;
            Destroy(item.gameObject);
        }

        talentArrays.Clear();
    }

    public int SetTalentGrade(int slotId)
    {
        var rowData = Managers.Data.GetData(GameDataManager.DATA_TYPE.character_talent, slotId) as CharacterTalent;

        return rowData.talent_rank;
    }
}

public class CharacterTalent : GameData
{
    public int uid { get; private set; }
    public int talent_group { get; private set; }
    public int talent_grade { get; private set; }
    public int talent_rank { get; private set; }
    public int talent_use_type { get; private set; }
    public int talent_type { get; private set; }
    public float talent_calc { get; private set; }
    public int talent_value { get; private set; }
    public int ui_text_type { get; private set; }

    public override void SetValue(Dictionary<string, string> tmp)
    {
        base.SetValue(tmp);

        uid = Int(data["uid"]);
        talent_group = Int(data["talent_group"]);
        talent_grade = Int(data["talent_grade"]);
        talent_rank = Int(data["talent_rank"]);
        talent_use_type = Int(data["talent_use_type"]);
        talent_type = Int(data["talent_type"]);
        talent_calc = float.Parse(data["talent_calc"]);
        talent_value = Int(data["talent_value"]);
        ui_text_type = Int(data["ui_text_type"]);
    }
    public override string GetName()
    {
        return StringManager.Instance.GetName(dataType, talent_group);
    }

    public string GetValue()
    {
        switch (ui_text_type)
        {
            case 1:
                return (talent_value).ToString();
            case 2:
                return (talent_value * 0.1f).ToString() + "%";
            case 3:
                return (talent_value * 0.001f).ToString();
        }

        return "";
    }
    public string GetString()
    {
        return GetName() + " " + GetValue();
    }
}

