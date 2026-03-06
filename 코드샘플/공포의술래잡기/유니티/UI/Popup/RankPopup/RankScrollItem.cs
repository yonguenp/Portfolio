using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankScrollItem : ScrollUIControllerItem
{
    [SerializeField]
    Text UserText;
    public override void SetData(GameData data, ScrollItemSelectCallback cb = null)
    {
        base.SetData(data, cb);
        RankData rankData = data as RankData;

        UserText.text = $"{rankData.userNick} : {rankData.Point}";
    }
}
