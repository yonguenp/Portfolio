using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRateItem : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text rateText;
    [SerializeField] UIGrade uIGradeBG;
    [SerializeField] UIGrade uIGradeIcon;
    [SerializeField] UIPortraitCharacter character;
    
    public int grade;

    public void Init(CharacterGameData data, double rate)
    {
        nameText.text = data.GetName();
        rateText.text = string.Format("{0:0.###}%", rate); 
        character.SetPortrait(data.GetID());
        grade = Convert.ToInt32(data.char_grade);
        uIGradeBG.SetGrade(grade);
        uIGradeIcon.SetGrade(grade);
    }

    public void Init(ItemGameData data, double rate)
    {
        nameText.text = data.GetName();
        rateText.text = string.Format("{0:0.###}%", rate);
        character.SetItem(data.GetID());
        grade = Convert.ToInt32(data.grade);
        uIGradeBG.SetGrade(grade);
        uIGradeIcon.SetGrade(grade);
    }
}
