
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArenaSeasonOpeningPopup : Popup<PopupData>
{

    [SerializeField] Image bgImgRect = null;
    [SerializeField] Image dimImg = null;


    [SerializeField] Text title = null;
    [SerializeField] Text dateText = null;

    [SerializeField] GameObject nextSeasonAlertTextObj = null;
    public override void InitUI()
    {
        bool isPreSeason = ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.PreSeason;
        if (isPreSeason)
        {
            title.text = StringData.GetStringByStrKey("pvp_season_next_buff_name");
            dateText.text = string.Empty;
            nextSeasonAlertTextObj.SetActive(true);
        }
        else
        {
            title.text = string.Format(StringData.GetStringByIndex(100001141), ArenaManager.Instance.UserArenaData.season_id.ToString());
            dateText.text = string.Format("{0} ~ {1}", TimeManager.GetCustomDateTime(ArenaManager.Instance.UserArenaData.season_start).ToString(StringData.GetStringByStrKey("아레나시즌기간표기")), TimeManager.GetCustomDateTime(ArenaManager.Instance.UserArenaData.season_remain_time).ToString(StringData.GetStringByStrKey("아레나시즌기간표기")));
            nextSeasonAlertTextObj.SetActive(false);
        }
        
        SetBgSize();
    }


    void SetBgSize()
    {
        var ratio = bgImgRect.sprite.bounds.size.y / bgImgRect.sprite.bounds.size.x;
        var screenWidth = UICanvas.Instance.GetCanvasRectTransform().sizeDelta.x;
        bgImgRect.GetComponent<RectTransform>().sizeDelta = new Vector2(screenWidth, screenWidth * ratio);
    }

}
