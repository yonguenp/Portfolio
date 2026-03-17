using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClanPopup : Popup
{
    /// <summary>
    /// 서브 팝업 타입 enum 값
    /// </summary>

    public delegate void SuccessCallback(JToken body);

    public enum SubPopupType
    {
        CREATE_CLAN = 0,
        CREATE_CLAN_INFO,
        CLAN_MANAGEMENT,
        CLAN_EMBLEM,
        CLAN_MODIFY,
        LENGTH
    }

    /// <summary>
    /// 클랜 페이지 enum 값
    /// </summary>
    public enum ClanPage
    {
        CLAN_LIST = 0,
        CLAN_INFO,
    }

    [SerializeField] ClanList clanListPage;
    [SerializeField] List<GameObject> subPopupList = new List<GameObject>();


    public ClanInfo clanInfoPage;
    public ClanPage curPage = ClanPage.CLAN_LIST;

    public int prelv = -1;
    public JObject ClanInfo { get; private set; } = null;
    public override void Close()
    {
        base.Close();
    }
    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);
    }
    public override void RefreshUI()
    {
        for (int i = 0; i < (int)SubPopupType.LENGTH; i++)
        {
            ClanSubPopupSetActive((SubPopupType)i, false);
        }

        base.RefreshUI();

        clanListPage.gameObject.SetActive(false);
        clanInfoPage.gameObject.SetActive(false);

        SBWeb.SendPost("clan/clan", null, (response) =>
        {
            SBWeb.OnResponseCheck(response, () =>
            {
                if (SBWeb.IsResultOK(response))
                {
                    JToken res = SBWeb.GetResultData(response);
                    ClanInfo = (JObject)res;

                    if (ClanInfo == null)
                    {
                        Close();
                        return;
                    }

                    if (ClanInfo.ContainsKey("list"))
                    {
                        ShowStateClan(ClanPage.CLAN_LIST);
                    }
                    else
                    {
                        ShowStateClan(ClanPage.CLAN_INFO);
                    }

                    Managers.UserData.SetMyUserInfo(res["user"]);
                }
            });
        });
    }

    public void UpDateClanInfo()
    {
        SBWeb.SendPost("clan/clan", null, (response) =>
        {
            SBWeb.OnResponseCheck(response, () =>
            {
                if (SBWeb.IsResultOK(response))
                {
                    JToken res = SBWeb.GetResultData(response);
                    ClanInfo = (JObject)res;

                    if (ClanInfo == null)
                    {
                        Close();
                        return;
                    }

                    clanInfoPage.RefreshUI();
                }
            });
        });
    }

    public void ShowStateClan(ClanPage page)
    {
        switch (page)
        {
            case ClanPage.CLAN_LIST:
                clanListPage.gameObject.SetActive(true);
                clanListPage.RefreshUI();
                break;
            case ClanPage.CLAN_INFO:
                clanInfoPage.gameObject.SetActive(true);
                //clanInfoPage.RefreshUI();
                break;
            default:
                break;
        }
    }

    public void ClanSubPopupSetActive(SubPopupType type, bool value = false)
    {
        subPopupList[(int)type].SetActive(value);
    }
    public bool EscSubPopup()
    {
        foreach (var item in subPopupList)
        {
            if (item.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    public void OnClanCreateButton()
    {
        ClanSubPopupSetActive(SubPopupType.CREATE_CLAN, true);
    }

    public void OnClanModifyButton()
    {
        bool leader = false;
        if (ClanInfo.ContainsKey("my"))
        {
            JObject my = (JObject)ClanInfo["my"];
            if (my["status"].Value<int>() == 4)
            {
                leader = true;
            }
        }

        if (!leader)
        {
            return;
        }

        ClanSubPopupSetActive(SubPopupType.CLAN_MODIFY, true);
    }

    public void ClanRequestJoin(JObject data)
    {
        if (ClanInfo.ContainsKey("my"))
        {
            JObject my = (JObject)ClanInfo["my"];
            if (my != null && my.ContainsKey("status") && my["status"].Value<int>() == 1)
            {
                PopupCanvas.Instance.ShowFadeText("이미신청");
                return;
            }
        }

        if (data == null)
            return;

        if (data["headcount"].Value<int>() >= GameConfig.Instance.DEFAULT_CLAN_HEADCOUNT)
        {
            PopupCanvas.Instance.ShowFadeText("클랜원수초과");
            return;
        }

        WWWForm param = new WWWForm();
        param.AddField("clan_no", data["no"].Value<int>());

        SBWeb.SendPost("clan/clan_request", param, (response) =>
        {
            if (SBWeb.IsResultOK(response))
            {
                PopupCanvas.Instance.ShowFadeText("신청완료");
                RefreshUI();
                return;
            }

            JObject res = (JObject)response;
            if (res.ContainsKey("rs"))
            {
                int rs = res["rs"].Value<int>();
                if (rs != 0)
                {
                    switch (rs)
                    {
                        case 302:
                            PopupCanvas.Instance.ShowFadeText("클랜신청원수초과");
                            break;
                        default:
                            PopupCanvas.Instance.ShowFadeText("클랜오류발생");
                            break;
                    }
                    return;
                }
            }

            PopupCanvas.Instance.ShowFadeText("클랜오류발생");
        });
    }

    public void ClanRequestCancel()
    {
        string msg = "탈퇴완료";
        JObject my = (JObject)ClanInfo["my"];
        if (my != null && my.ContainsKey("status") && my["status"].Value<int>() < 3)
        {
            msg = "클랜신청취소완료";
        }

        SBWeb.SendPost("clan/clan_exit", null, (response) =>
        {
            if (SBWeb.IsResultOK(response))
            {
                Managers.Network.SendCSClanInfoUpdateNotify();

                PopupCanvas.Instance.ShowFadeText(msg);
                RefreshUI();

                Managers.UserData.ClearClanRank();

                return;
            }

            PopupCanvas.Instance.ShowFadeText("클랜오류발생");
        });
    }

    public void ClanRequestInfo(Action action = null)
    {
        SBWeb.SendPost("clan/clan", null, (response) =>
        {
            SBWeb.OnResponseCheck(response, () =>
            {
                if (SBWeb.IsResultOK(response))
                {
                    JToken res = SBWeb.GetResultData(response);
                    ClanInfo = (JObject)res;

                    if (ClanInfo == null)
                    {
                        Close();
                        PopupCanvas.Instance.ShowFadeText("클랜정보오류");
                        action?.Invoke();
                        return;
                    }

                    if (ClanInfo.ContainsKey("list"))
                    {
                        ShowStateClan(ClanPage.CLAN_LIST);
                        PopupCanvas.Instance.ShowFadeText("클랜정보새로고침");
                    }
                    else
                    {
                        Close();
                        PopupCanvas.Instance.ShowFadeText("클랜목록오류");
                    }
                }
                action?.Invoke();
            });
        });

    }

    public void ClanRequestCreate(string name, string desc, bool auto, int emblem)
    {
        WWWForm param = new WWWForm();
        param.AddField("clan_name", name);
        param.AddField("clan_desc", desc);
        param.AddField("auto_join", auto ? 1 : 0);
        param.AddField("emblem", emblem);

        SBWeb.SendPost("clan/clan_make", param, (response) =>
        {
            JObject res = (JObject)response;
            if (res.ContainsKey("rs"))
            {
                int rs = res["rs"].Value<int>();
                if (rs != 0)
                {
                    switch (rs)
                    {
                        case 204:
                        case 205:
                            PopupCanvas.Instance.ShowFadeText("클랜명부적절");
                            break;
                        case 301:
                            PopupCanvas.Instance.ShowFadeText("클랜생성재화부족");
                            break;
                        case 203:
                            PopupCanvas.Instance.ShowFadeText("클랜명중복");
                            break;
                        default:
                            PopupCanvas.Instance.ShowFadeText("클랜생성오류");
                            break;
                    }
                    return;
                }
            }

            SBWeb.OnResponseCheck(response, () =>
            {
                if (SBWeb.IsResultOK(response))
                {
                    if (res["rs"].Value<int>() == 0)
                    {
                        JObject res = (JObject)SBWeb.GetResultData(response);
                        Managers.UserData.SetMyUserInfo(res["user"]);

                        PopupCanvas.Instance.ShowFadeText("생성완료");

                        RefreshUI();

                        return;
                    }
                }

                PopupCanvas.Instance.ShowFadeText("클랜오류발생");
            });
        });
    }

    public void ClanRequestModify(string desc, bool auto, int emblem)
    {
        WWWForm param = new WWWForm();
        param.AddField("clan_desc", desc);
        param.AddField("auto_join", auto ? 1 : 0);
        param.AddField("emblem", emblem);

        SBWeb.SendPost("clan/clan_modify", param, (response) =>
        {
            JObject res = (JObject)response;
            if (res.ContainsKey("rs"))
            {
                int rs = res["rs"].Value<int>();
                if (rs != 0)
                {
                    switch (rs)
                    {
                        case 204:
                        case 205:
                            PopupCanvas.Instance.ShowFadeText("클랜명부적절");
                            break;
                        case 301:
                            PopupCanvas.Instance.ShowFadeText("클랜생성재화부족");
                            break;
                        case 203:
                            PopupCanvas.Instance.ShowFadeText("클랜명중복");
                            break;
                        default:
                            PopupCanvas.Instance.ShowFadeText("클랜생성오류");
                            break;
                    }
                    return;
                }
            }

            PopupCanvas.Instance.ShowFadeText("클랜수정완료");

            RefreshUI();
        });
    }

    public void ClanRequestUserList(int clan_no, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("clan_no", clan_no);

        SBWeb.SendPost("clan/clan_request_list", param, (response) =>
        {
            JObject res = (JObject)response;
            if (res.ContainsKey("rs"))
            {
                if (res["rs"].Value<int>() == 0)
                {
                    cb?.Invoke(res);
                    return;
                }
            }

            PopupCanvas.Instance.ShowFadeText("클랜오류발생");
        });
    }

    public void ClanRequestMissionClear(int quest_index, SuccessCallback cb = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("quest_index", quest_index);

        SBWeb.SendPost("clan/clan_quest", param, (response) =>
        {
            JObject res = (JObject)response;
            if (res.ContainsKey("rs"))
            {
                if (res["rs"].Value<int>() == 0)
                {
                    cb?.Invoke(res);
                    RefreshUI();
                    return;
                }
            }

            PopupCanvas.Instance.ShowFadeText("클랜오류발생");
        });
    }

    public void ApprovalRequest(long target_user_no, int accept, int _delegate, SuccessCallback action = null)
    {
        WWWForm param = new WWWForm();
        param.AddField("target_user_no", target_user_no.ToString());
        param.AddField("accept", accept);
        param.AddField("delegate", _delegate);

        SBWeb.SendPost("clan/clan_manage", param, (response) =>
        {
            JObject res = (JObject)response;
            if (res.ContainsKey("rs"))
            {
                if (res["rs"].Value<int>() == 0)
                {
                    action?.Invoke(response);
                    return;
                }
            }

            PopupCanvas.Instance.ShowFadeText("클랜오류발생");
        });
    }

    public void SetCandidateDuo(IList<SBSocketSharedLib.ClanMemberInfo> infos)
    {
        clanInfoPage.SetCandidateDuo(infos);
    }

    public void DuoClear()
    {
        clanInfoPage.DuoClear();
    }

    public void SetDataSubPopup(JObject data)
    {
        subPopupList[(int)SubPopupType.CLAN_MANAGEMENT].GetComponent<ClanMemberManagement>().SetPopup(data);
    }
}