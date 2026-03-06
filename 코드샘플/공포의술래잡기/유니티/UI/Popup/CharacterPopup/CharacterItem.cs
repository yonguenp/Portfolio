using UnityEngine;
using UnityEngine.UI;

public class CharacterItem : MonoBehaviour
{
    [SerializeField]
    Text text;
    [SerializeField]
    SelectedCharacter slot;
    [SerializeField]
    GameObject ChangeButton;

    CharacterGameData curData = null;
    CharacterPopup characterPopup = null;
    bool currentSelected = false;

    public void SetData(CharacterGameData data, CharacterPopup popup, bool cur)
    {
        currentSelected = cur;

        curData = data;
        characterPopup = popup;

        UserEquipData equip = null;
        var myChar = Managers.UserData.GetMyCharacterInfo(data.GetID());
        if (myChar != null)
        {
            equip = myChar.curEquip;
        }
        slot.SetCharacter(data.GetID(), equip);

        text.text = data.GetName();

        Vector2 size = (transform as RectTransform).sizeDelta;
        size.x = 500;
        (transform as RectTransform).sizeDelta = size;

        Image bg = GetComponent<Image>();
        if (bg != null)
        {
            if (currentSelected)
            {
                bg.color = Color.yellow;

            }
            else
            {
                bg.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
        }

        ChangeButton.SetActive(false);
    }

    public void OnSelect()
    {
        if (!currentSelected)
            characterPopup.OnSelectCharacter(curData);
    }

    public CharacterGameData GetCurData()
    {
        return curData;
    }
}
