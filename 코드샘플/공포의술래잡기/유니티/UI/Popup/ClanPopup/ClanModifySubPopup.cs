using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanModifySubPopup : ClanCreateSubPopup
{
    [SerializeField]
    Text ClanName;

    ClanPopup ClanPopup;
    protected override void InitUI()
    {
        ClanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;

        if (ClanPopup == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if(ClanPopup.ClanInfo == null)
        {
            gameObject.SetActive(false);
            return;
        }

        JObject ClanData = (JObject)ClanPopup.ClanInfo["my"];


        ClanName.text = ClanData["name"].Value<string>();

        autoJoin = ClanData["option"].Value<int>() == 1;
        AutoJoinOption[0].SetActive(autoJoin);
        AutoJoinOption[1].SetActive(!autoJoin);

        ClanDescInput.SetActive(true);
        ClanDescInput.GetComponent<InputField>().text = ClanData["desc"].Value<string>();
        OnEmblemChanged(ClanData["icon"].Value<int>());
    }

    public void OnModifyClan()
    {
        ClanPopup.ClanRequestModify(ClanDescInput.GetComponent<InputField>().text, autoJoin, curEmblem);
    }
}
