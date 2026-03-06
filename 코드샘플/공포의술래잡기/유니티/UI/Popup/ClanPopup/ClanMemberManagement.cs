using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanMemberManagement : MonoBehaviour
{
    [SerializeField] Text crew_name;
    [SerializeField] Image crew_Rank;
    [SerializeField] Text authText;

    JObject user;
    ClanInfo.INFO_AUTH authType = ClanInfo.INFO_AUTH.Master;
    public void SetPopup(JObject user_data)
    {
        user = user_data;
        crew_name.text = user_data["user_nick"].Value<string>();
        if (user_data.ContainsKey("point"))
            crew_Rank.sprite = RankType.GetRankFromPoint(user_data["point"].Value<int>()).rank_resource;

        authText.text = StringManager.GetString($"{authType}");
    }

    public void ClanAppointmentBtn()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        if (popup.IsOpening())
        {
            popup.ApprovalRequest(user["user_no"].Value<long>(), 0, 1, (response) =>
            {
                JObject res = (JObject)response;
                if (res.ContainsKey("rs"))
                {
                    int rs = res["rs"].Value<int>();
                    if (rs != 0)
                    {
                        PopupCanvas.Instance.ShowFadeText("클랜오류");
                        return;
                    }
                }

                PopupCanvas.Instance.ShowFadeText("임명완료");
                Close();

                popup.RefreshUI();
            });
        }
    }

    public void ClanExpulsionBtn()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        if (popup.IsOpening())
        {
            popup.ApprovalRequest(user["user_no"].Value<long>(), 0, 0, (response) =>
            {
                JObject res = (JObject)response;
                if (res.ContainsKey("rs"))
                {
                    int rs = res["rs"].Value<int>();
                    if (rs != 0)
                    {
                        PopupCanvas.Instance.ShowFadeText("클랜오류");
                        return;
                    }

                }
                PopupCanvas.Instance.ShowFadeText("추방완료");
                Close();

                popup.RefreshUI();
            });
        }
    }

    public void LeftSideBtn()
    {
        authType -= 1;
        if (authType < ClanInfo.INFO_AUTH.Crew)
            authType = ClanInfo.INFO_AUTH.Master;

        authText.text = StringManager.GetString($"{authType}");
    }
    public void RightSideBtn()
    {
        authType += 1;
        if (authType > ClanInfo.INFO_AUTH.Master)
            authType = ClanInfo.INFO_AUTH.Crew;

        authText.text = StringManager.GetString($"{authType}");
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
