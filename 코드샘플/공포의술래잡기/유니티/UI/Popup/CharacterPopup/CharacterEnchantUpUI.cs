using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEnchantUpUI : MonoBehaviour
{
    [SerializeField]
    UIEnchant uiEnchant = null;

    //[SerializeField]
    //Text txtEnchant = null;

    int characterIndex = -1;
    UserCharacterData charData = null;
    CharacterReinforceGameData curEnchantData = null;
    CharacterReinforceGameData nextEnchantData = null;


    [Header("[현재 캐릭터 상태]")]
    [SerializeField] Text maxCurCharacterLV_text;
    [SerializeField] Text maxCurCharacterSkillLV_text;

    [Header("[업그레이드 캐릭터 상태]")]
    [SerializeField] Text maxUpCharacterLV_text;
    [SerializeField] Text maxUpCharacterSkillLV_text;

    [SerializeField] GameObject maxInfo;

    [Header("[기타 UI]")]
    [SerializeField] UIBundleItem needSkillUpItem_01;
    [SerializeField] Button reinforceBtn;
    [SerializeField] Transform AnimationPanel;
    [SerializeField] GameObject EnchantUpParticle;

    [Header("[UI 이펙트]")]
    [SerializeField] UIParticle fx_rwrd_scene;

    [SerializeField] GameObject cover;

    SkillGameData skillData = null;


    public void SetActive(bool active)
    {
        gameObject.SetActive(active);

        if (active)
        {
            EnchantUpParticle.SetActive(false);
            AnimationPanel.DOKill();
            AnimationPanel.localScale = Vector3.zero;
            AnimationPanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            cover.SetActive(false);
        }
    }
    public void SetActive(bool active, int targetCharacter)
    {
        SetActive(active);
        OnSelectCharacterData(targetCharacter);
    }


    public void OnSelectCharacterData(int targetCharacter)
    {
        characterIndex = targetCharacter;
        charData = Managers.UserData.GetMyCharacterInfo(targetCharacter);

        if (charData == null)
        {
            SetActive(false);
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("ui_ch_error"));
            return;
        }

        curEnchantData = charData.characterData.GetEnchantData(charData.enchant);

        //if (curEnchantData == null)
        //{
        //    SetActive(false);
        //    PopupCanvats.Instance.ShowFadeText("승급데이터오류");
        //    return;
        //}

        nextEnchantData = charData.characterData.GetEnchantData(charData.enchant + 1);

        skillData = charData.GetSkillData();

        RefreshUI();
    }

    public void RefreshUI()
    {
        SetSkillCharacterUI();
        SetNextEnchant();
        SetMaterial();
        SetNextEnchantDesc();
    }

    public void SetAbleReinforce()
    {
        bool able = true;
        if (nextEnchantData == null)
            able = false;
        else if (Managers.UserData.GetMyItemCount(nextEnchantData.need_item_reinforce) < nextEnchantData.reinforce_item_count)
            able = false;

        reinforceBtn.interactable = able;

        if (reinforceBtn.interactable)
        {
            reinforceBtn.transform.Find("Image").GetComponent<Image>().color = reinforceBtn.colors.normalColor;
            reinforceBtn.transform.Find("Text").GetComponent<Text>().color = reinforceBtn.colors.normalColor;

        }
        else
        {
            reinforceBtn.transform.Find("Image").GetComponent<Image>().color = reinforceBtn.colors.disabledColor;
            reinforceBtn.transform.Find("Text").GetComponent<Text>().color = reinforceBtn.colors.disabledColor;
        }
    }
    void SetSkillCharacterUI()
    {
        maxCurCharacterLV_text.text = "LV." + ((curEnchantData == null ? 1 : curEnchantData.reinforce_grade) * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL).ToString();
        maxCurCharacterSkillLV_text.text = "LV." + ((curEnchantData == null ? 1 : curEnchantData.reinforce_grade)).ToString();
        string desc = skillData.GetMajorSkill(charData.skillLv).GetDesc();
        desc = desc.Replace("#18F12A", "#FFD86E");


        if (nextEnchantData != null)
        {
            maxUpCharacterLV_text.text = "LV." + (nextEnchantData.reinforce_grade * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL).ToString();
            maxUpCharacterSkillLV_text.text = "LV." + nextEnchantData.reinforce_grade.ToString();
        }
        MaxReinforceAble(nextEnchantData == null);
        SetAbleReinforce();
    }
    void MaxReinforceAble(bool able)
    {
        maxInfo.SetActive(able);
        reinforceBtn.gameObject.SetActive(!able);
    }
    void SetNextEnchant()
    {
        if (nextEnchantData == null)
            uiEnchant.SetEnchant(curEnchantData.reinforce_grade);
        else
            uiEnchant.SetNextEnchant(nextEnchantData.reinforce_grade);
    }

    void SetMaterial()
    {
        int needItem_1 = 0;

        string strNeedItemCount_01 = "";

        if (nextEnchantData == null)//만랩
        {
            needItem_1 = curEnchantData.need_item_reinforce;// todo: 아이템 가지고오기
            strNeedItemCount_01 = curEnchantData.reinforce_item_count.ToString();

        }
        else
        {
            needItem_1 = nextEnchantData.need_item_reinforce;// todo: 아이템 가지고오기
            strNeedItemCount_01 = nextEnchantData.reinforce_item_count.ToString();
        }

        needSkillUpItem_01.SetNeedItem(ItemGameData.GetItemData(needItem_1),
            Managers.UserData.GetMyItemCount(needItem_1), int.Parse(strNeedItemCount_01));
    }

    void SetNextEnchantDesc()
    {
        int curSkillLv = charData.skillLv;
        string skillName = charData.GetSkillData().GetName();

        if (nextEnchantData != null)
        {
            int curMaxLevel = 0;
            if (curEnchantData != null)
                curMaxLevel = curEnchantData.reinforce_grade;
            //txtEnchant.text = $"[{skillName}]{curSkillLv}/{curMaxLevel} -> {skillName}]{curSkillLv}/{nextEnchantData.reinforce_grade}";
        }
        else
        {
            //txtEnchant.text = StringManager.GetString("최대강화알림");
        }
    }

    public void OnEnchant()
    {
        if (PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("매치대기캐릭터변경불가"));
            return;
        }

        if (nextEnchantData != null && charData.enchant <= GameConfig.Instance.MAX_CHARACTER_REINFORCE)
        {
            //if (Managers.UserData.GetMyItemCount(nextEnchantData.need_item_reinforce) < nextEnchantData.reinforce_item_count)
            if (!reinforceBtn.interactable)
            {
                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("아이템부족"));
                return;
            }

            SBWeb.UpgradeEnchantCharacter(charData.characterData.GetID(), () =>
            {
                reinforceBtn.enabled = false;
                OnEnchantAnimation();
                var openAudio = Managers.Sound.Play("effect/EF_REINFORCE", Sound.Effect);

                fx_rwrd_scene.Play();
                cover.SetActive(true);
                cover.GetComponent<Image>().DOKill();
                cover.GetComponent<Image>().DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f).From();

                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.3f).OnComplete(() =>
                {
                    var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.NEW_CHARACTER_POPUP) as NewCharacterPopup;
                    popup.SetData(curEnchantData, nextEnchantData, charData.characterData);
                    PopupCanvas.Instance.ShowPopup(PopupCanvas.POPUP_TYPE.NEW_CHARACTER_POPUP);
                    popup.OnStarReflexEffect(nextEnchantData.reinforce_grade);
                    Managers.Sound.Play("effect/EF_REINFORCE_END", Sound.Effect);
                    OnSelectCharacterData(characterIndex);

                    cover.SetActive(false);
                });
                sequence.AppendInterval(0.3f);
                sequence.AppendCallback(() =>
                {
                    reinforceBtn.enabled = true;
                });

                VibrateManager.OnVibrate(0.5f, 200);
            });
        }
        else
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("최대강화알림"));
        }
    }

    void OnEnchantAnimation()
    {
        EnchantUpParticle.SetActive(true);
        EnchantUpParticle.transform.position = uiEnchant.EnchantIcon[charData.enchant - 1].transform.position;
        EnchantUpParticle.GetComponent<Coffee.UIExtensions.UIParticle>().Play();
        uiEnchant.EnchantIcon[charData.enchant - 1].GetComponent<Animation>().Play();

        //(PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup).OnParticleEffect(uiEnchant.EnchantIcon[charData.enchant - 1].transform, 0.0f);
    }
}
