using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestrictedAreaDragonListView : BattleDragonListView
{
    [SerializeField] RestrictedAreaReadyPopup popup;
    public override List<UserDragon> GetAbleDragons()
    {
        var usingDragons = popup.UsingDragons;
        List<UserDragon> ret = new List<UserDragon>();
        foreach(var dragon in User.Instance.DragonData.GetAllUserDragons())
        {
            if (usingDragons.Contains(dragon.Tag))
            {
                continue;
            }

            ret.Add(dragon);
        }

        return ret;
    }
}
