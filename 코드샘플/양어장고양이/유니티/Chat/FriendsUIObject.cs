using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class FriendsUIObject : MonoBehaviour
{
    public FriendsPanel.FriendsUI myEnum;
    public FriendItem cloneTargetItem;
    public Transform itemContainer;
    public FriendsPanel FriendPanel;

    public void OnFriendsStateChange(FriendsPanel.FriendsUI ui)
    {
        gameObject.SetActive(myEnum == ui);

        if (myEnum == ui)
        {
            RefreshUI();
        }
    }

    public void ClearItems()
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

    abstract public bool IsNewFlag();
    abstract public void NewFlagDone();
}
