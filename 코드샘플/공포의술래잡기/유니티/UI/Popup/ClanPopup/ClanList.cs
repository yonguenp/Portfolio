using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClanList : MonoBehaviour
{
    [SerializeField] ClanPopup ClanPopup = null;
    public Transform content;
    public GameObject clanSampleItem;
    public int curState = 0;        // 탭 버튼에 따름 1:: 클랜목록, 2:: 신청목록
    public GameObject search;
    public GameObject joinnotiy;

    [SerializeField] InputField inputField;
    [Header("[UI Text]")]
    public Text text;

    [Header("[UI Btn]")]
    [SerializeField] Button refreshBtn;
    [SerializeField] Button searchBtn;

    public List<Button> tabBtn = new List<Button>();
    public JObject SearchInfo = null;

    public void RefreshUI()
    {
        Clear();

        search.SetActive(curState == 0);
        joinnotiy.SetActive(curState == 1);


        if (ClanPopup.ClanInfo == null)
            return;

        if (!ClanPopup.ClanInfo.ContainsKey("list"))
        {
            return;
        }

        List<int> clans = new List<int>();
        if (ClanPopup.ClanInfo.ContainsKey("my"))
        {
            JObject my = (JObject)ClanPopup.ClanInfo["my"];
            if (my != null && my.ContainsKey("status") && my["status"].Value<int>() == 1)
            {
                var obj = Instantiate(clanSampleItem.gameObject, clanSampleItem.transform.parent);
                obj.gameObject.SetActive(true);

                obj.GetComponent<ClanListItem>().SetData((JObject)ClanPopup.ClanInfo["my"]);

                clans.Add(my["no"].Value<int>());
            }
        }

        bool searchOk = false;

        if (SearchInfo != null)
        {
            foreach (JObject clan in SearchInfo["list"])
            {
                if (clans.Contains(clan["no"].Value<int>()))
                    continue;

                var obj = Instantiate(clanSampleItem.gameObject, clanSampleItem.transform.parent);
                obj.gameObject.SetActive(true);

                obj.GetComponent<ClanListItem>().SetData(clan);

                clans.Add(clan["no"].Value<int>());
                searchOk = true;
            }

            SearchInfo = null;

            if (!searchOk)
            {                
                PopupCanvas.Instance.ShowFadeText("클랜검색결과없음");
            }
        }

        if (!searchOk)
        {
            foreach (JObject clan in ClanPopup.ClanInfo["list"])
            {
                if (clans.Contains(clan["no"].Value<int>()))
                    continue;

                var obj = Instantiate(clanSampleItem.gameObject, clanSampleItem.transform.parent);
                obj.gameObject.SetActive(true);

                obj.GetComponent<ClanListItem>().SetData(clan);

                clans.Add(clan["no"].Value<int>());
            }
        }
    }

    void Clear()
    {
        foreach (Transform item in content)
        {
            if (item == clanSampleItem.transform)
                continue;

            Destroy(item.gameObject);
        }

        clanSampleItem.SetActive(false);
    }

    public void TabButton(int value)
    {
        if (curState == value)
            return;

        curState = value;

        foreach (var item in tabBtn)
        {
            item.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_02");
        }

        tabBtn[curState].GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_01");
        RefreshUI();
    }

    public void OnRefreshButton()
    {
        ClanPopup.RefreshUI();
    }

    public void CreateClanBtn()
    {
        ClanPopup.ClanSubPopupSetActive(ClanPopup.SubPopupType.CREATE_CLAN, true);
    }
    public void ListRefreshBtn()
    {
        refreshBtn.interactable = false;
        ClanPopup.ClanRequestInfo(() =>
        {
            refreshBtn.interactable = true;
        });
    }

    public void ClanSearchBtn()
    {
        var searchTarget = inputField.text;
        inputField.text = "";

        if (string.IsNullOrEmpty(searchTarget))
            return;

        WWWForm param = new WWWForm();
        param.AddField("name", searchTarget);

        SBWeb.SendPost("clan/clan", param, (response) =>
        {
            SBWeb.OnResponseCheck(response, () =>
            {
                if (SBWeb.IsResultOK(response))
                {
                    JToken res = SBWeb.GetResultData(response);
                    JObject searchResult = (JObject)res;

                    if (searchResult == null)
                    {
                        SearchInfo = null;
                    }
                    else if (searchResult.ContainsKey("list"))
                    {
                        if (searchResult.Type == JTokenType.Array)
                            SearchInfo = (JObject)(searchResult["list"][0]);
                        else
                            SearchInfo = (JObject)searchResult;
                    }

                    if (SearchInfo == null)
                    {
                        PopupCanvas.Instance.ShowFadeText("클랜검색결과없음");
                    }

                    RefreshUI();
                }
            });
        });
    }
}
