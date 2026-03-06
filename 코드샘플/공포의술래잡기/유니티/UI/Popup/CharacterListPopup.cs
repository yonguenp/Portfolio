using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterListPopup : Popup
{
    public UICharacterListingItem sampleObject;
    public Transform groupA;
    public Transform groupB;

    public List<GameData> gameDatas = new List<GameData>();
    public override void Open(CloseCallback cb = null)
    {
        gameDatas = Managers.Data.GetData(GameDataManager.DATA_TYPE.character);
        sampleObject.gameObject.SetActive(true);

        base.Open(cb);
    }
    public override void Close()
    {
        Clear();
        base.Close();
    }

    public override void RefreshUI()
    {
        Clear();
        ListSort();
        var datas = Managers.UserData.MyCharacters;

        foreach (CharacterGameData data in gameDatas)
        {
            if (data.GetID() > 2)
            {
                Transform parent;
                if (data.IsSuvivorCharacter())
                    parent = groupA;
                else
                    parent = groupB;


                UICharacterListingItem obj = Instantiate(sampleObject, parent);
                obj.transform.localScale = Vector3.one;
                obj.Init(data as CharacterGameData);


                if (datas.ContainsKey(data.GetID()))
                {
                    obj.dim.SetActive(false);
                    obj.gridText.text = "잠금 해제 완료!";
                }
                else
                {
                    obj.dim.SetActive(true);
                    obj.gridText.text = "뽑기를 통해\n잠금해제";
                }
            }
        }
        sampleObject.gameObject.SetActive(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(groupA.parent.GetComponent<RectTransform>());
    }
    public void ListSort()
    {
        gameDatas.Sort(CharacterSort);

    }
    public int CharacterSort(GameData A, GameData B)
    {
        if (Managers.UserData.MyCharacters.ContainsKey(A.GetID()))
            return -1;
         if (Managers.UserData.MyCharacters.ContainsKey(B.GetID()))
            return 1;

         if (Managers.UserData.MyCharacters.ContainsKey(A.GetID()) && Managers.UserData.MyCharacters.ContainsKey(B.GetID()))
        {
            if (Convert.ToInt32(A.GetValue("char_grade")) > Convert.ToInt32(B.GetValue("char_grade")))
                return -1;
            else
                return 1;
        }

        else
        {
            if (Convert.ToInt32(A.GetValue("char_grade")) > Convert.ToInt32(B.GetValue("char_grade")))
                return -1;
            else
                return 1;

        }
    }



    public void Clear()
    {
        foreach (Transform item in groupA)
        {
            if (item != sampleObject.transform)
            {
                Destroy(item.gameObject);
            }
        }
        foreach (Transform item in groupB)
        {
            Destroy(item.gameObject);
        }
    }
}
