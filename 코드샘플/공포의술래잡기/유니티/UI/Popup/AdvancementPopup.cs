using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AdvancementPopup : Popup
{
    [SerializeField] Image gradeImage;
    [SerializeField] Text gradeText;
    [SerializeField] Text pointText;
    [SerializeField] GameObject star;
    [SerializeField] Transform parent;

    [SerializeField] float radius;

    [SerializeField] GameObject MainPopup;
    [SerializeField] GameObject SubPopup;
    [SerializeField] bool isTrigger;

    List<GameObject> starLists = new List<GameObject>();
    int starCount = 0;


    //rankUP Play
    [SerializeField]
    RankUpPlay rankUpPlay = null;
    
    public override void Open(CloseCallback cb = null)
    {
        CacheUserData.SetInt("saved_rank", Managers.UserData.MyRank.GetID());
        base.Open(cb);
    }
    public override void Close()
    {
        base.Close();

        StopAllCoroutines();
        foreach (var item in starLists)
        {
            Destroy(item);
        }

        starLists.Clear();
    }

    public void Init(bool trigger)
    {
        MainPopup.SetActive(trigger);
        SubPopup.SetActive(!trigger);

        if (trigger)
        {
            starCount = 10;
            gradeImage.sprite = Managers.UserData.MyRank.rank_resource;
            gradeText.text = Managers.UserData.MyRank.GetName();
            pointText.text = Managers.UserData.MyPoint.ToString();
            CreateStar();

            InitAndPlay(Managers.UserData.MyRank);

            VibrateManager.OnVibrate(1.0f, 250);

            switch(Managers.UserData.MyRank.GetID())
            {
                case 2:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("nlrbbt");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 5:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("rl6xc6");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 8:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("s74wm8");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
                case 11:
                    {
                        com.adjust.sdk.AdjustEvent ae = new com.adjust.sdk.AdjustEvent("i0n8yh");
                        com.adjust.sdk.Adjust.trackEvent(ae);
                    }
                    break;
            }
        }

        rankUpPlay.gameObject.SetActive(trigger);

        //RankReward 갱신을 위해 호출
        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANKREWARD_POPUP) as RankRewardPopup).RefreshRewardEnableFlag();
    }

    public void InitAndPlay(RankType targetRankType)
    {   
        if(rankUpPlay == null) 
            SBDebug.LogError("rankUpPlay is NULL");

        //RankType prevRankType = RankType.GetRankDataFromRank(targetRankType.GetID() - 1);
        //rankUpPlay.SetRankSprite(prevRankType.rank_resource);
        //rankUpPlay.nextRankSprite = targetRankType.rank_resource;
        rankUpPlay.SetRankSprite(targetRankType.rank_resource);

        //RankReward 갱신을 위해 호출
        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.RANKREWARD_POPUP) as RankRewardPopup).RefreshRewardEnableFlag();
    }

    public void CreateStar()
    {
        for (int i = 0; i < starCount; i++)
        {
            var obj = Instantiate(star, parent);
            obj.SetActive(true);
            starLists.Add(obj);
            StartCoroutine(OnAnim(obj, (360 / starCount) * i));
        }
    }
    IEnumerator OnAnim(GameObject obj, float angle)
    {
        var pos = Vector2.zero;
        var rad = 0;

        while (rad < radius)
        {
            pos.x = rad * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.y = rad * Mathf.Sin(angle * Mathf.Deg2Rad);

            obj.GetComponent<RectTransform>().anchoredPosition = pos;

            rad += 5;
            yield return null;
        }

        while (true)
        {
            yield return null;

            pos.x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            pos.y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

            obj.GetComponent<RectTransform>().anchoredPosition = pos;
            angle -= 3f;
        }
    }
}
