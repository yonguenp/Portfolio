using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class FriendsUIObject : MonoBehaviour
{
    public FriendPopup.FriendsUI myEnum;
    public FriendItem cloneTargetItem;
    public Transform itemContainer;
    public FriendPopup FriendPanel;

    public void OnFriendsStateChange(FriendPopup.FriendsUI ui)
    {
        gameObject.SetActive(myEnum == ui);

        if (myEnum == ui)
        {
            RefreshUI();
        }
        else
        {
            HideHI();
        }
    }

    virtual public void ClearItems()
    {
        foreach(Transform obj in itemContainer)
        {
            if(cloneTargetItem.transform != obj)
            {
                Destroy(obj.gameObject);
            }
        }

        cloneTargetItem.gameObject.SetActive(false);
    }

    abstract public void RefreshUI();
    abstract public void HideHI();
    abstract public bool IsNewFlag();
    abstract public void NewFlagDone();

}
