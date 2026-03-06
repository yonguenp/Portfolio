using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectInfo : MonoBehaviour
{
    [SerializeField]
    bool IsSurvivorInfo;
    [SerializeField]
    SelectedCharacter selectedCharacter;
    [SerializeField]
    Text characterName;
    [SerializeField]
    UIEnchant enchant;
    [SerializeField]
    UIGrade grade;
    [SerializeField]
    Text level;
    [SerializeField]
    Text skillLevel;
    [SerializeField]
    Image skillImage;
    [SerializeField]
    Text skillName;
    [SerializeField]
    Text skillDesc;
    [SerializeField]
    Text skillCoolDesc;

    CharacterSelectUI parentUI = null;

    public void Init(CharacterSelectUI parent)
    {
        parentUI = parent;
    }

    public void RefreshUI()
    {
        int curChaser = Managers.UserData.MyDefaultChaserCharacter;
        int curSurvivor = Managers.UserData.MyDefaultSurvivorCharacter;
        int targetCharcter = IsSurvivorInfo ? curSurvivor : curChaser;
        UserCharacterData targetCharacterData = Managers.UserData.GetMyCharacterInfo(targetCharcter);

        selectedCharacter.SetCharacter(targetCharacterData.characterData.GetID(), targetCharacterData.curEquip);
        characterName.SetText(targetCharacterData.characterData.GetName(), IsSurvivorInfo ? SHelper.TEXT_TYPE.SURVIVOR_CHARACTER : SHelper.TEXT_TYPE.CHASER_CHARACTER);
        grade.SetGrade(targetCharacterData.characterData.char_grade);
        enchant.SetEnchant(targetCharacterData.enchant);
        level.text = "Lv." + targetCharacterData.lv.ToString();
        skillLevel.text = "Lv." + targetCharacterData.skillLv.ToString();
        skillImage.sprite = targetCharacterData.GetSkillData().GetIcon();
        skillName.text = targetCharacterData.GetSkillData().GetName();
        skillDesc.text = targetCharacterData.GetSkillBaseData().GetDesc();
        skillCoolDesc.text = StringManager.GetString("cooltimedesc", targetCharacterData.GetSkillData().CoolTime);
    }

    public void OnShowCharacterList()
    {
        if (parentUI != null)
            parentUI.OnShowCharacterList(IsSurvivorInfo);
    }
}
