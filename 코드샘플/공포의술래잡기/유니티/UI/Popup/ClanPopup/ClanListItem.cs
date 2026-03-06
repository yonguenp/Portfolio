using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanListItem : MonoBehaviour
{
    enum STATE_BUTTON_TYPE
    {
        JOIN,
        WAIT,
        CANCEL,
    };
    [SerializeField]
    UIClanEmblem ClanEmblem;
    [SerializeField]
    Image ClanIcon;
    [SerializeField]
    Text ClanName;
    [SerializeField]
    Text ClanLevel;
    [SerializeField]
    Text ClanLeaderName;
    [SerializeField]
    Text ClanHeadCount;
    [SerializeField]
    GameObject[] StatusButton;

    JObject Data;
    public void SetData(JObject data)
    {
        Data = data;

        RefreshUI();
    }

    void RefreshUI()
    {
        gameObject.SetActive(false);

        if (Data == null)
        {
            return;
        }

        if (!Data.ContainsKey("no"))
        {
            return;
        }

        if (Data["no"].Value<int>() == 0)
        {
            return;
        }

        gameObject.SetActive(true);

        ClanName.text = Data["name"].Value<string>();
        ClanLevel.text = "LV." + Data["level"].Value<string>();
        ClanLeaderName.text = Data["leader_nick"].Value<string>();
        ClanHeadCount.text = Data["headcount"].Value<int>().ToString() + "/" + GameConfig.Instance.DEFAULT_CLAN_HEADCOUNT;
        ClanEmblem.Init(Data["icon"].Value<int>());
        int status = 0;
        if(Data.ContainsKey("status"))
        {
            status = Data["status"].Value<int>();
        }
            
        foreach(GameObject btn in StatusButton)
        {
            btn.SetActive(false);
        }

        switch(status)
        {
            case 0:
                StatusButton[(int)STATE_BUTTON_TYPE.JOIN].SetActive(true);
                break;
            case 1:
                StatusButton[(int)STATE_BUTTON_TYPE.CANCEL].SetActive(true);
                break;
            case 2:
                gameObject.SetActive(false);
                return;
            default:
                StatusButton[(int)STATE_BUTTON_TYPE.JOIN].SetActive(true);
                break;
        }
    }

    public void OnStatusButton()
    {
        ClanPopup ClanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;

        int status = 0;
        if (Data.ContainsKey("status"))
        {
            status = Data["status"].Value<int>();
        }

        switch (status)
        {
            case 0:
                ClanPopup.ClanRequestJoin(Data);
                break;
            case 1:
                ClanPopup.ClanRequestCancel();
                break;
            case 2:
                return;
            default:
                ClanPopup.ClanRequestJoin(Data);
                break;
        }
    }
}
