using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuoPlayerUI : MonoBehaviour
{
    [SerializeField] Image RankIcon;
    [SerializeField] Text NickName;
    [SerializeField] GameObject ReadyFrame;
    public void Clear()
    {
        NickName.text = "";
        RankIcon.sprite = null;
        RankIcon.gameObject.SetActive(false);
    }

    public void SetUI(string nick, int point, bool ready)
    {
        NickName.text = nick;

        RankType rankData = RankType.GetRankFromPoint(point);
        if (rankData != null)
        {
            RankIcon.gameObject.SetActive(true);
            RankIcon.sprite = rankData.rank_resource;
        }

        //ReadyFrame.SetActive(ready);        
    }

}
