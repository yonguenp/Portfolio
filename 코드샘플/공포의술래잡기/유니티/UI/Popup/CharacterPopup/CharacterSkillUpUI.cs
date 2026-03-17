using Coffee.UIExtensions;
using SBCommonLib.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSkillUpUI : MonoBehaviour
{
    [SerializeField] Image curSkillIcon;
    [SerializeField] Text curSkillName;
    [SerializeField] Text curSkillDesc;
    [SerializeField] Text curSkillCoolTime;

    [SerializeField] Image nextSkillIcon;
    [SerializeField] Text nextSkillName;
    [SerializeField] Text nextSkillDesc;
    [SerializeField] Text nextSkillCoolTime;

    [SerializeField] UIBundleItem needItem;
    [SerializeField] Text needGoldCount;

    [SerializeField] GameObject maxInfo;
    [SerializeField] UIParticle up_effect;
    [SerializeField] Button upBtn;

    int curTarget = 0;
    CharacterSkillLevelGameData skillUpgradeData = null;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        curTarget = (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup).GetUI().GetSelectedCharacterID();
        RefreshUI();
    }

    public void SetActive(bool active, int target)
    {
        SetActive(active);
        up_effect.gameObject.SetActive(active);
        curTarget = target;
        if (active)
            RefreshUI();
    }

    public void RefreshUI()
    {
        var userCharData = Managers.UserData.GetMyCharacterInfo(curTarget);
        if (userCharData == null)
            return;

        var skillData = userCharData.GetSkillData();
        skillUpgradeData = userCharData.GetSkillNextLevelData();

        var curMajorSkillBase = skillData.GetMajorSkill(userCharData.skillLv);
        var nextMajorSkillBase = skillData.GetMajorSkill(userCharData.skillLv + 1);

        curSkillIcon.sprite = skillData.GetIcon();
        curSkillName.text = $"LV.{userCharData.skillLv} " + curMajorSkillBase.GetName();
        curSkillDesc.text = curMajorSkillBase.GetDesc();
        curSkillCoolTime.text = StringManager.GetString("ui_second", skillData.CoolTime);
        //curSkillValue.text = StringManager.GetString("ui_skill_value");

        var needItemCurrentCount = skillUpgradeData != null ? Managers.UserData.GetMyItemCount(skillUpgradeData.need_item) : 0;
        ItemGameData needItem = skillUpgradeData != null ? ItemGameData.GetItemData(skillUpgradeData.need_item) : null;

        MaxInfoCheck(false);
        if (nextMajorSkillBase != null && skillUpgradeData != null)
        {
            nextSkillIcon.sprite = skillData.GetIcon();
            nextSkillName.text = $"LV.{userCharData.skillLv + 1} " + nextMajorSkillBase.GetName();
            nextSkillDesc.text = nextMajorSkillBase.GetDesc();
            nextSkillCoolTime.text = StringManager.GetString("ui_second", skillData.CoolTime);
            // TODO : 스킬 값은 스킬 타입에 따라 불러와야 하는 테이블의 정보가 다르므로 스킬 구현 완료 후 불러올 수 있는 구문을 구현한다
            // nextSkillValue.text = StringManager.GetString("ui_skill_value");

            //needItemCount.text = $"{needItem.GetName()}\n{needItemCurrentCount}/{skillUpgradeData.need_item_count}";
            //needItemCount.color = (needItemCurrentCount < skillUpgradeData.need_item_count) ? Color.red : Color.black;

            //needItemIcon.sprite = ItemGameData.GetItemIcon(skillUpgradeData.need_item);
            this.needItem.SetNeedItem(needItem, Managers.UserData.GetMyItemCount(skillUpgradeData.need_item), skillUpgradeData.need_item_count);
            needGoldCount.text = $"{skillUpgradeData.need_gold:n0}";
            needGoldCount.color = Color.white;
            if (Managers.UserData.MyGold < skillUpgradeData.need_gold)
            {
                needGoldCount.color = Color.red;
            }
        }
        else
        {
            MaxInfoCheck(true);
            //nextSkillIcon.sprite = null;
            //nextSkillName.text = StringManager.GetString("스킬최대레벨");
            //nextSkillDesc.text = StringManager.GetString("스킬최대레벨");
            //nextSkillCoolTime.text = StringManager.GetString("스킬최대레벨");
            ////nextSkillValue.text = StringManager.GetString("스킬최대레벨");

            ////needItemIcon.sprite = null;
            ////needItemCount.text = StringManager.GetString("스킬최대레벨");
            //this.needItem.SetNeedItem(needItem, 0, 0);
            //needGoldCount.text = StringManager.GetString("스킬최대레벨");
        }


        bool enableButton = true;
        if (nextMajorSkillBase == null || userCharData.enchant < nextMajorSkillBase.Level || (Managers.UserData.GetMyItemCount(skillUpgradeData.need_item) < skillUpgradeData.need_item_count))
            enableButton = false;

        foreach (MaskableGraphic graphic in upBtn.GetComponentsInChildren<MaskableGraphic>())
        {
            if (graphic.GetComponent<Image>() != null || (graphic.GetComponent<Text>() != null && graphic.gameObject.name != "blackText"))
            {
                graphic.color = enableButton ? Color.white : Color.gray;
            }
        }
    }

    public void OnSkillLevelUpSend()
    {
        upBtn.interactable = false;
        var charData = Managers.UserData.GetMyCharacterInfo(curTarget);
        var skillData = charData.GetSkillData();
        var nextSkillData = skillData.GetMajorSkill(charData.skillLv + 1);

        if (nextSkillData == null)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("스킬최대레벨"));
            upBtn.interactable = true;
            return;
        }

        if (charData.enchant < nextSkillData.Level)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("msg_skill_nolvup"));
            upBtn.interactable = true;
            return;
        }

        if (Managers.UserData.GetMyItemCount(skillUpgradeData.need_item) < skillUpgradeData.need_item_count)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"));
            upBtn.interactable = true;
            return;
        }

        if (Managers.UserData.MyGold < skillUpgradeData.need_gold)
        {
            PopupCanvas.Instance.ShowConfirmPopup("골드부족상점이동", () =>
            {
                PopupCanvas.Instance.ShowShopPopup(GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.shop_menu, 6) as ShopMenuGameData);
            });
            upBtn.interactable = true;
            return;
        }

        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("매치대기캐릭터변경불가"));
            upBtn.interactable = true;
            return;
        }

        SBWeb.CharacterSkillUp(charData.characterData.GetID(), () =>
        {
            upBtn.interactable = true;
            RefreshUI();
            up_effect.gameObject.SetActive(true);
            up_effect.Play();
        });
    }

    public void MaxInfoCheck(bool active)
    {
        if (active)
        {
            maxInfo.gameObject.SetActive(true);
            needItem.transform.parent.gameObject.SetActive(false);
            needGoldCount.text = "0";
        }
        else
        {
            maxInfo.gameObject.SetActive(false);
            needItem.transform.parent.gameObject.SetActive(true);
        }
    }
}
