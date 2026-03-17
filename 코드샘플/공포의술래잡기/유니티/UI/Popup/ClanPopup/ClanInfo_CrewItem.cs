using Newtonsoft.Json.Linq;
using SBCommonLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanInfo_CrewItem : MonoBehaviour
{
    [SerializeField] Text crewName;
    [SerializeField] Image crewRankImage;
    [SerializeField] GameObject offDim;
    [SerializeField] Text[] crewLastUpdate;
    [SerializeField] Button duoBtn;

    [SerializeField] Button approvalBtn;
    [SerializeField] Button refusalBtn;

    public JObject userData { get; private set; } = null;
    public void Init(JObject jo)
    {
        userData = jo;
        DateTime lastlogout = DateTime.MinValue;
        DateTime lastlogin = DateTime.MinValue;

        crewName.text = jo["user_nick"].Value<string>();

        if (jo.ContainsKey("point"))
            crewRankImage.sprite = RankType.GetRankFromPoint(jo["point"].Value<int>()).rank_resource;

        if (jo.ContainsKey("last_login"))
            lastlogin = DateTime.Parse(jo["last_login"].Value<string>());

        if (jo.ContainsKey("last_logout"))
            lastlogout = DateTime.Parse(jo["last_logout"].Value<string>());

        if (duoBtn != null)
        {
            if (jo.ContainsKey("user_no"))
            {
                duoBtn.gameObject.SetActive(jo["user_no"].Value<long>() != Managers.UserData.MyUserID);
            }
        }

        SetDuoEnable(false);

        bool login = lastlogin > lastlogout;

        if (jo["user_no"].Value<long>() == Managers.UserData.MyUserID)
            login = true;

        if (offDim != null)
            offDim.SetActive(!login);

        if (crewLastUpdate != null && crewLastUpdate.Length > 0)
        {
            crewLastUpdate[0].gameObject.SetActive(login);
            crewLastUpdate[1].gameObject.SetActive(!login);

            const int MAX_VIEW_DAY = 60;
            string ret = string.Empty;
            if (!login)
            {
                TimeSpan gapTime = SBUtil.KoreanTime - lastlogout;

                // 60일이 넘어서면
                if (gapTime.Days > MAX_VIEW_DAY)
                    ret = StringManager.GetString("ui_frlist_time_day", MAX_VIEW_DAY);
                else
                {
                    //1일 이상 접속 안하면
                    if (gapTime.Days >= 1)
                    {
                        ret = StringManager.GetString("ui_frlist_time_day", gapTime.Days.ToString());
                    }
                    else
                    {
                        //1시간 이상일때
                        if (gapTime.Hours >= 1)
                        {
                            ret = StringManager.GetString("ui_frlist_time_hours", gapTime.Hours.ToString()); ;
                        }
                        else
                        {
                            if (gapTime.Minutes >= 1)
                                ret = StringManager.GetString("ui_frlist_time_min", gapTime.Minutes.ToString());
                            else
                                ret = StringManager.GetString("ui_frlist_time_min", 1);
                        }
                    }
                }

                crewLastUpdate[1].text = ret;
            }
        }


        if (approvalBtn != null && refusalBtn != null)
        {
            approvalBtn.onClick.RemoveAllListeners();
            approvalBtn.onClick.AddListener(() =>
            {
                ApprovalBtn(jo["user_no"].Value<long>(), 1);
            });
            refusalBtn.onClick.RemoveAllListeners();
            refusalBtn.onClick.AddListener(() =>
            {
                ApprovalBtn(jo["user_no"].Value<long>(), 0);
            });
        }
    }

    public void SetDuoEnable(bool active)
    {
        if (duoBtn == null)
            return;

        if (duoBtn != null)
            duoBtn.interactable = active;

        if(active == false)
        {
            foreach (Transform item in duoBtn.transform)
            {
                if(item.GetComponent<Graphic>() != null)
                {
                    ColorUtility.TryParseHtmlString("#7B7B7B", out Color color);
                    item.GetComponent<Graphic>().color = color;
                }
            }
        }
        else
        {
            foreach (Transform item in duoBtn.transform)
            {
                if (item.GetComponent<Graphic>() != null)
                {
                    item.GetComponent<Graphic>().color = Color.white;
                }
            }

        }
    }

    public void DuoInvite()
    {
        Managers.FriendData.DUO.SendDuoRequest(userData["user_no"].Value<long>(), SBSocketSharedLib.DuoType.ClanDuo);
        (PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup).SetCandidateDuo(null);
    }

    public void ApprovalBtn(long target_user_no, int accept)
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        if (popup.IsOpening())
        {
            popup.ApprovalRequest(target_user_no, accept, 0, (response) =>
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

                 Destroy(this.gameObject);

                 if (accept == 1)
                     PopupCanvas.Instance.ShowFadeText("가입승인");
                 else
                     PopupCanvas.Instance.ShowFadeText("승인거부");

                 var clanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
                 clanPopup.UpDateClanInfo();
             });
        }
    }


    public void OnSubPopupManagement()
    {
        var popup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;
        if (popup.clanInfoPage.cur_auth != ClanInfo.INFO_AUTH.Master || userData["user_no"].Value<long>() == Managers.UserData.MyUserID)
            return;
        popup.ClanSubPopupSetActive(ClanPopup.SubPopupType.CLAN_MANAGEMENT, true);
        popup.SetDataSubPopup(userData);
    }
}
