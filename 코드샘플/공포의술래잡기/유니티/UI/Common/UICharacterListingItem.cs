using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterListingItem : MonoBehaviour
{
    public Image characterFace;
    public Text characterName;
    public GameObject dim;
    public Text gridText;
    public UIGrade uIGrade;
    public UIGrade uIGradeBG;
    public Image playerRankImage;
    public Image runCharacter;
    public UIEnchant enchantUI;
    public Text lvText;

    public Color color;

    public GameData data;
    public bool isMine = false;
    public void Init(CharacterGameData data)
    {
        this.data = data;
        characterFace.sprite = data.sprite_ui_resource;
        characterName.text = this.data.GetName();

        if (uIGrade != null)
            uIGrade.SetGrade(Convert.ToInt32(data.char_grade));
        if(uIGradeBG != null)
            uIGradeBG.SetGrade(Convert.ToInt32(data.char_grade));
    }

    
}
