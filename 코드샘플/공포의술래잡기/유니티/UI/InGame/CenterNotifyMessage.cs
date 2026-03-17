using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CenterNotifyMessage : GameLogNotifyMessage
{
    [SerializeField]
    UIPortraitCharacter uiPortrait;
    [SerializeField]
    Text title;
    [SerializeField]
    Text desc;
    [SerializeField]
    Text userName;
    public void SetData(int uid, string id)
    {
        var data = Managers.Data.GetData(GameDataManager.DATA_TYPE.reward_point, uid) as RewardGameData;
        if (data == null)
            return;

        CharacterObject charObject = Managers.Object.FindCharacterById(id);
        if (charObject == null)
        {
            SBDebug.LogError("오류!");
            return;
        }

        uiPortrait.SetPortrait(charObject.CharacterType);

        userName.text = charObject.UserName;
        title.text = data.GetName();
        desc.text = data.GetDesc();

        gameObject.SetActive(true);
    }
}
