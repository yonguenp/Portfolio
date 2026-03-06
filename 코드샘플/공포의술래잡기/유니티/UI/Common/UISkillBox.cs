using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillBox : MonoBehaviour
{
    CharacterGameData targetCharacterData = null;
    
    [SerializeField]
    Text txtSkillLv = null;
    [SerializeField]
    Image imgSkillIcon = null;
    [SerializeField]
    Text txtSkillName = null;
    [SerializeField]
    Text txtSkillDesc = null;
    [SerializeField]
    Text txtCooltimeValue = null;

    public int Level{get;set;} = 1;

     public void SetCharacter(CharacterGameData characterData, int level = 1)
    {
        targetCharacterData = characterData;
        Level = level;
        RefreshUI();
    }

    void RefreshUI()
    {
        var skillGameData = targetCharacterData.GetSkillData();
        var skillBase = skillGameData.GetMajorSkill(Level);
        txtSkillLv.text = "Lv." + skillBase.Level.ToString();
        txtSkillName.text = skillGameData.GetName();
        imgSkillIcon.sprite = skillGameData.GetIcon();
        txtSkillDesc.text = skillBase.GetDesc();
        txtCooltimeValue.text = targetCharacterData.GetSkillData().CoolTime.ToString() + StringManager.GetString("초");
    }

    public void OnSetNextLevel()
    {
        Level++;
        var maxLevel = GameConfig.Instance.MAX_CHARACTER_SKILL_LEVEL;
        if(Level > maxLevel)
        {
            Level = maxLevel;
            return;   
        }

        RefreshUI();
    }

    public void OnSetPrevLevel()
    {
        Level--;
        var minLevel = 1;
        if(Level < minLevel)
        {
            Level = minLevel;
            return;   
        }

        RefreshUI();
    }
}
