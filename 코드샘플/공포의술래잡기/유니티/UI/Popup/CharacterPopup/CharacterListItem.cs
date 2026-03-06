using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListItem : ScrollUIControllerItem
{
    [SerializeField]
    protected Image panel;
    [SerializeField]
    protected UIPortraitCharacter SelectedCharacter;
    [SerializeField]
    protected Text curLevel;
    [SerializeField]
    protected UIEnchant curEnchant;
    [SerializeField]
    protected UIGrade curGradeBG;
    [SerializeField]
    protected UIGrade curGradeIcon;
    [SerializeField]
    protected GameObject selectedIcon;
    [SerializeField]
    private Text curName;
    [SerializeField]
    private GameObject Focus;
    [SerializeField]
    private GameObject Limited;
    [SerializeField]
    private GameObject EquipIcon;

    public UserCharacterData userData { get; private set; }
    public CharacterGameData charData { get; private set; }


    private void Update()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CHARACTER_POPUP) as CharacterPopup;
        if (popup != null && popup.IsOpening())
        {
            if (charData != null && charData.GetID() == popup.GetUI().GetSelectedCharacterID())
                SetFocus(true);
            else
                SetFocus(false);
        }
    }
    public override void SetData(GameData data, ScrollItemSelectCallback cb = null)
    {
        base.SetData(data, cb);

        charData = data as CharacterGameData;
        if (charData == null)
        {
            return;
        }

        userData = Managers.UserData.GetMyCharacterInfo(((CharacterGameData)data).GetID());
        if (userData == null)
        {
            return;
        }

        SelectedCharacter.SetPortrait(data.GetID());
        curLevel.text = "Lv." + userData.lv.ToString();
        curEnchant.SetEnchant(userData.enchant);

        if (curName != null)
            curName.text = data.GetName();

        curGradeBG.SetGrade(charData.char_grade);
        curGradeIcon.SetGrade(charData.char_grade);

        selectedIcon.SetActive(Managers.UserData.MyDefaultChaserCharacter == data.GetID() || Managers.UserData.MyDefaultSurvivorCharacter == data.GetID());

        if(Focus != null)
            Focus.SetActive(false);

        if (EquipIcon != null)
            EquipIcon.SetActive(userData.curEquip != null);
    }

    public void SetDataForNotMine(GameData data, ScrollItemSelectCallback cb = null)
    {
        base.SetData(data, cb);

        charData = data as CharacterGameData;
        if (charData == null)
        {
            return;
        }

        SelectedCharacter.SetPortrait(data.GetID());

        if (curName != null)
            curName.text = data.GetName();

        curGradeBG.SetGrade(charData.char_grade);
        curGradeIcon.SetGrade(charData.char_grade);

        if (Limited != null)
            Limited.SetActive(charData.is_limited > 0);

        if (EquipIcon != null)
            EquipIcon.SetActive(false);
    }

    public void SetDataForChoiceTicket(GameData data, ScrollItemSelectCallback cb = null)
    {
        base.SetData(data, cb);

        charData = data as CharacterGameData;
        if (charData == null)
        {
            return;
        }

        SelectedCharacter.SetPortrait(data.GetID());

        if (curName != null)
            curName.text = data.GetName();

        curGradeBG.SetGrade(charData.char_grade);
        curGradeIcon.SetGrade(charData.char_grade);

        if (Limited != null)
            Limited.SetActive(charData.is_limited > 0);

        if (EquipIcon != null)
            EquipIcon.SetActive(false);

        SetFocus(false);
    }

    public void SetFocus(bool focus)
    {
        if (Focus != null)
            Focus.SetActive(focus);
        //Color color = Color.white;
        //Sprite sprite = panelSprite[0];
        //if (focus)
        //{
        //    color = SHelper.TextColor[(int)SHelper.TEXT_TYPE.ME];
        //    sprite = panelSprite[1];
        //}
        //panel.color = color;
        //panel.sprite = sprite;
    }
}
