using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField]
    CharacterPopup popup;

    [SerializeField]
    CharacterSelectInfo Survivor;
    [SerializeField]
    CharacterSelectInfo Chaser;
    [SerializeField]
    public ScrollUIController CharacterList;
    [SerializeField]
    Button CloseButton;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        CharacterList.SetActive(false);

        if (active)
        {
            Survivor.Init(this);
            Chaser.Init(this);
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (Managers.Scene.CurrentScene.name == "RoomScene" && CharacterList.gameObject.activeSelf)
        {
            popup.Close();
            return;
        }

        Survivor.RefreshUI();
        Chaser.RefreshUI();

        CharacterList.SetActive(false);
        CloseButton.gameObject.SetActive(true);
    }

    public void OnShowCharacterList(bool IsSurvivorInfo)
    {
        int curChaser = Managers.UserData.MyDefaultChaserCharacter;
        int curSurvivor = Managers.UserData.MyDefaultSurvivorCharacter;
        int targetCharcter = IsSurvivorInfo ? curSurvivor : curChaser;

        CharacterList.SetActive(true);
        CloseButton.gameObject.SetActive(false);

        List<UserCharacterData> list = null;
        if (IsSurvivorInfo)
            list = CharacterGameData.GetMySurvivorList();
        else
            list = CharacterGameData.GetMyChaserList();

        CharacterList.Clear();
        foreach (UserCharacterData data in list)
        {
            ScrollUIControllerItem item = CharacterList.AddItem(data.characterData, OnCharacterSelect);
            (item as CharacterListItem).SetFocus(data.characterData.GetID() == targetCharcter);
        }
    }

    public void OnCharacterSelect(ScrollUIControllerItem caller)
    {
        CharacterListItem item = caller as CharacterListItem;
        if (item != null)
        {
            popup.OnSelectCharacter(item.userData.characterData);
        }
    }
}
