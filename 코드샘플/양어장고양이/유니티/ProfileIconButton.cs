using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconButton : MonoBehaviour
{
    public void RefreshProfileIcon()
    {
        Sprite[] imgSheet = Resources.LoadAll<Sprite>("Sprites/Neco/Ui/Idcard");
        users user = GameDataManager.Instance.GetUserData();
        Image img = GetComponent<Image>();
        object obj;
        user.data.TryGetValue("profileId", out obj);
        uint index = (uint)obj;
        if(imgSheet.Length >= index)
        {
            img.sprite = imgSheet[index - 1];
        }
        else
        {
            img.sprite = imgSheet[0];
        }
    }
}
