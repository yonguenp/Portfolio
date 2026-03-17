using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCollectionListItem : ScrollUIControllerItem
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
    protected UIGrade curGrade;
    [SerializeField]
    protected GameObject selectedIcon;

    [SerializeField] GameObject _blackBG = null;

    [SerializeField] Text name = null;

    [SerializeField] UIGrade grade = null;


    public CharacterGameData Data { get; private set; }
    public void ShowBlackBG(bool isShow)
   {
       _blackBG.SetActive(!isShow);
   }

    public override void SetData(GameData data, ScrollItemSelectCallback cb = null)
    {
        Data = data as CharacterGameData; 
        if (Data == null)
        {
            return;
        }

        SelectedCharacter.SetPortrait(data.GetID());
        curGrade.SetGrade(Data.char_grade);
        //grade.SetGrade(Data.char_grade);
        name.text = Data.GetName();
        
        selectedIcon.SetActive(Managers.UserData.MyDefaultChaserCharacter == data.GetID() || Managers.UserData.MyDefaultSurvivorCharacter == data.GetID());

        ShowBlackBG(Managers.UserData.GetMyCharacterInfo(data.GetID()) != null);

        curLevel.gameObject.SetActive(false);
        curEnchant.gameObject.SetActive(false);
        OnSelectionChange(cb);
    }

    void OnSelectionChange(ScrollItemSelectCallback cb)
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                if (cb != null)
                    cb.Invoke(this);
            });
        }
    }
}
