using Coffee.UIExtensions;
using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewCharacterPopup : Popup
{
    [Header("[공용 UI]")]
    [SerializeField] Image bg_Image;
    [SerializeField] Color bg_Color_chaser;
    [SerializeField] Color bg_Color_survivor;
    [SerializeField] SkeletonGraphic character_bg;
    [SerializeField] SkeletonGraphic character_spine;
    [SerializeField] SkeletonDataAsset chaser_bg_eff;
    [SerializeField] SkeletonDataAsset survivor_bg_eff;

    [Header("[신규 연출 화면Type1]")]
    [SerializeField] GameObject mainType1;
    [SerializeField] Text chatText;
    [SerializeField] UIGrade rank_Image;
    [SerializeField] Image factionbadge_Image;
    [SerializeField] Text faction_text;
    [SerializeField] Text characterName_text;


    [Header("[강화 연출화면Type2]")]
    [SerializeField] GameObject mainType2;
    [SerializeField] UIGrade rank_Image2;
    [SerializeField] Image factionbadge_Image2;
    [SerializeField] Text faction_text2;
    [SerializeField] Text characterName_text2;

    [SerializeField] Text max_level_up_text;
    [SerializeField] Text curlevel_text;
    [SerializeField] Text prevlevel_text;
    [SerializeField] Text skill_name_text;
    [SerializeField] Text cur_skilllevel_text;
    [SerializeField] Text prev_skilllevel_text;
    [SerializeField] UIEnchant uIEnchant;
    [SerializeField] List<UIParticle> starReflex_effect = new List<UIParticle>();

    [SerializeField] UIParticle[] fx_enchant_star;
    [SerializeField] UIParticle fx_enchant_last;



    public void SetData(CharacterGameData characterData)
    {
        Managers.Sound.Play("effect/EF_GACHA_NEW", Sound.Effect);

        mainType1.SetActive(true);
        mainType2.SetActive(false);

        bg_Image.color = Color.white;
        // 1 추격자
        if (characterData.char_type == 1)
        {
            bg_Image.DOColor(bg_Color_chaser, 0.2f);
            character_bg.skeletonDataAsset = chaser_bg_eff;
            factionbadge_Image.sprite = Resources.Load<Sprite>("Texture/UI/gacha/gacha_result_badge_02");
            faction_text.text = StringManager.GetString("char_info_chaser");
        }
        // 2 생존자
        else
        {
            bg_Image.DOColor(bg_Color_survivor, 0.2f);
            character_bg.skeletonDataAsset = survivor_bg_eff;
            factionbadge_Image.sprite = Resources.Load<Sprite>("Texture/UI/gacha/gacha_result_badge_01");
            faction_text.text = StringManager.GetString("char_info_suvivor");

        }
        //공통
        character_spine.skeletonDataAsset = characterData.spine_resource;
        chatText.text = string.Empty;
        chatText.DOKill();
        chatText.DOText(characterData.GetScript(characterData.GetID()), 1.5f);
        characterName_text.text = characterData.GetName();
        rank_Image.SetGrade(characterData.char_grade);
        character_bg.startingAnimation = "f_play_0";
        character_bg.startingLoop = true;
        character_bg.Initialize(true);

        character_spine.startingAnimation = "f_idle_0";
        character_spine.startingLoop = true;
        character_spine.Initialize(true);
        uIEnchant.gameObject.SetActive(false);
    }
    public void SetData(CharacterReinforceGameData curData, CharacterReinforceGameData nextData, CharacterGameData characterData)
    {
        mainType1.SetActive(false);
        mainType2.SetActive(true);
        uIEnchant.gameObject.SetActive(true);

        if (characterData.char_type == 1)
        {
            bg_Image.color = bg_Color_chaser;
            character_bg.skeletonDataAsset = chaser_bg_eff;
            factionbadge_Image2.sprite = Resources.Load<Sprite>("Texture/UI/gacha/gacha_result_badge_02");
            faction_text2.text = StringManager.GetString("char_info_chaser");
        }
        // 2 생존자
        else
        {
            bg_Image.color = bg_Color_survivor;
            character_bg.skeletonDataAsset = survivor_bg_eff;
            factionbadge_Image2.sprite = Resources.Load<Sprite>("Texture/UI/gacha/gacha_result_badge_01");
            faction_text2.text = StringManager.GetString("char_info_suvivor");

        }
        //공통
        character_spine.skeletonDataAsset = characterData.spine_resource;
        characterName_text2.text = characterData.GetName();
        rank_Image2.SetGrade(characterData.char_grade);

        character_bg.startingAnimation = "f_play_0";
        character_bg.startingLoop = true;
        character_bg.Initialize(true);

        character_spine.startingAnimation = "f_idle_0";
        character_spine.startingLoop = true;
        character_spine.Initialize(true);

        max_level_up_text.text = StringManager.GetString("ui_reinforce_lvup");
        curlevel_text.text = "LV." + ((curData == null ? 1 : curData.reinforce_grade) * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL).ToString();
        prevlevel_text.text = "LV." + (nextData.reinforce_grade * GameConfig.Instance.REINFORCE_PER_MAX_LEVEL).ToString();
        skill_name_text.text = StringManager.GetString("ui_reinforce_skillup");
        cur_skilllevel_text.text = "LV" + (curData == null ? 1 : curData.reinforce_grade).ToString();
        prev_skilllevel_text.text = "LV" + nextData.reinforce_grade.ToString();

        uIEnchant.SetEnchant(nextData.reinforce_grade);
    }

    public void OnStarReflexEffect(int grade)
    {
        for (int i = 0; i < grade; i++)
        {
            starReflex_effect[i].Play();
        }
    }

    public void EnchantAnim(CharacterReinforceGameData data)
    {
        fx_enchant_last.transform.position = fx_enchant_star[data.reinforce_grade - 1].transform.position;
        Sequence sq = DOTween.Sequence();

        sq.AppendInterval(0.5f);
        for (int i = 0; i < data.reinforce_grade; i++)
        {
            sq.AppendCallback(() => { fx_enchant_star[i].Play(); }).SetDelay(0.1f * i);
        }
        sq.AppendCallback(() => { fx_enchant_last.Play(); });
    }
}
