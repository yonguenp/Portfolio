using Coffee.UIExtensions;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClanInfo : MonoBehaviour
{
    public enum INFO_AUTH
    {
        Crew = 3,
        Master = 4,
    }

    [SerializeField] ClanPopup clanPopup;
    [SerializeField] GameObject clanInfo;
    [SerializeField] GameObject clanChat;

    [Header("[클랜정보]")]
    [SerializeField] Text text_clan_name;
    [SerializeField] Text text_clan_desc;
    [SerializeField] Text text_clan_Lv;
    [SerializeField] Text text_clan_MasterName;
    //[SerializeField] Text text_clan_headcount;
    [SerializeField] Text text_clan_exp;
    [SerializeField] UIClanEmblem image_clan_icon;
    [SerializeField] Slider expSlider;
    [SerializeField] GameObject[] clan_option;
    [SerializeField] UIBuffItem buffInfo;

    [SerializeField] Text text_extiBtn;

    public ClanInfo_Chat info_Chat;
    public List<ClanInfoMenu> clanMenu = new List<ClanInfoMenu>();

    public INFO_AUTH cur_auth;
    public ClanInfoMenu.CLAN_MENU cur_menu;
    JObject ClanData;
    public JArray ClanPeopleData;

    public List<UIParticle> uIParticles = new List<UIParticle>();

    private void OnEnable()
    {
        if (clanPopup == null)
            clanPopup = PopupCanvas.Instance.GetPopup(PopupCanvas.POPUP_TYPE.CLAN_POPUP) as ClanPopup;

        cur_menu = ClanInfoMenu.CLAN_MENU.None;

        foreach (ClanInfoMenu item in clanMenu)
        {
            item.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_01");

            item.GetComponent<Button>().onClick.RemoveAllListeners();
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                ClanMenuBtn(item);
            });
        }

        InitUI();
    }

    public void InitUI()
    {
        RefreshUI();

        ClanMenuBtn(clanMenu.Find(_ => _.menuType == ClanInfoMenu.CLAN_MENU.Clan_CrewList));

        clanInfo.SetActive(true);
        clanChat.GetComponent<ClanInfo_Chat>().SetToggleChatSize(!clanInfo.activeInHierarchy);
    }
    public void RefreshUI()
    {
        if (clanPopup == null)
            return;
        if (clanPopup.ClanInfo == null || !clanPopup.ClanInfo.ContainsKey("my"))
            return;

        //SBDebug.Log((JObject)clanPopup.ClanInfo);

        ClanData = (JObject)clanPopup.ClanInfo["my"];
        ClanPeopleData = (JArray)clanPopup.ClanInfo["users"];

        if (ClanData["leader"].Value<long>() == ClanData["user_no"].Value<long>())
        {
            cur_auth = INFO_AUTH.Master;

            clanMenu.Find(_ => _.menuType == ClanInfoMenu.CLAN_MENU.CLAN__MEMBERSHIPAUTH).gameObject.SetActive(true);
            text_extiBtn.text = StringManager.GetString("ui_crew_breakup");
        }
        else
        {
            cur_auth = INFO_AUTH.Crew;

            clanMenu.Find(_ => _.menuType == ClanInfoMenu.CLAN_MENU.CLAN__MEMBERSHIPAUTH).gameObject.SetActive(false);
            text_extiBtn.text = StringManager.GetString("button_crew_exit");
        }

        // 클랜정보 세팅
        text_clan_name.text = ClanData["name"].Value<string>() + " " + $"({ClanData["headcount"].Value<int>()} / {GameConfig.Instance.DEFAULT_CLAN_HEADCOUNT})";
        text_clan_desc.text = ClanData["desc"].Value<string>();

        text_clan_Lv.text = ClanData["level"].Value<int>().ToString();
        //클랜랩업 시 
        if (clanPopup.prelv != -1 && (clanPopup.prelv < ClanData["level"].Value<int>()))
        {
            var particle = uIParticles.Find(_ => _.name == "fx_clan_levelup00");
            if(particle != null)
            {
                particle.Clear();
                particle.CancelInvoke("Play");
                particle.Invoke("Play", 0.1f);
                clanPopup.prelv = -1;
            }
        }
        text_clan_MasterName.text = ClanData["leader_nick"].Value<string>();

        if (text_clan_exp != null)
            text_clan_exp.text = "";
        if (expSlider != null)
            expSlider.value = 0.0f;

        int curExp = ClanData["exp"].Value<int>();
        ClanLevelData curLevelData = ClanLevelData.GetLevelDataFromExp(curExp);
        if (curLevelData != null)
        {
            int prevExp = 0;
            ClanLevelData prev = ClanLevelData.GetPrevLevelData(curLevelData);
            if (prev != null)
            {
                prevExp = prev.exp;
            }

            int exp = curExp - prevExp;
            int need_exp = curLevelData.exp - prevExp;

            if (text_clan_exp != null)
                text_clan_exp.text = exp.ToString() + " / " + need_exp.ToString();
            if (expSlider != null)
                expSlider.value = (float)exp / need_exp;


            if (buffInfo != null)
                buffInfo.SetInfo(StringManager.GetString("클랜골드버프", curLevelData.level), StringManager.GetString("클랜골드버프설명", curLevelData.level));
        }


        clan_option[0].SetActive(ClanData["option"].Value<int>() == 1);
        clan_option[1].SetActive(ClanData["option"].Value<int>() == 0);

        image_clan_icon.Init(ClanData["icon"].Value<int>());

        //클랜정보창 채팅 초기화 
        info_Chat.Init();
    }

    public void SetCandidateDuo(IList<SBSocketSharedLib.ClanMemberInfo> infos)
    {
        foreach (ClanInfoMenu item in clanMenu)
        {
            if (item.gameObject.activeInHierarchy && item.menuType == ClanInfoMenu.CLAN_MENU.Clan_CrewList)
            {
                item.SetCandidateDuo(infos);
            }
        }
    }

    public void DuoClear()
    {
        foreach (ClanInfoMenu item in clanMenu)
        {
            if (item.gameObject.activeInHierarchy && item.menuType == ClanInfoMenu.CLAN_MENU.Clan_CrewList)
            {
                item.RefreshMemberstate();
            }
        }
    }

    public void ClanMenuBtn(ClanInfoMenu menu)
    {
        if (cur_menu == menu.menuType)
            return;
        if (menu.menuType == ClanInfoMenu.CLAN_MENU.CLAN_SHOP)
        {
            PopupCanvas.Instance.ShowFadeText("클랜업데이트예정");
            return;
        }

        cur_menu = menu.menuType;

        foreach (var item in clanMenu)
        {
            item.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_02");
            ColorUtility.TryParseHtmlString($"#b9b9b9", out Color color);
            item.transform.Find("Text").GetComponent<Text>().color = color;
            item.RefreshPage();
        }
        menu.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_01");
        menu.transform.Find("Text").GetComponent<Text>().color = Color.black;

    }

    public void LeaveClanBtn()
    {
        if (ClanData == null)
            return;

        string msg = "클랜탈퇴";
        if (ClanData["status"].Value<int>() == 4)
        {
            if (ClanData["headcount"].Value<int>() > 1)
            {
                PopupCanvas.Instance.ShowFadeText("클랜원존재탈퇴불가");
                return;
            }

            msg = "클랜해체";
        }

        PopupCanvas.Instance.ShowConfirmPopup(msg, () =>
        {
            clanPopup.ClanRequestCancel();
        });
    }

    public void ToggleClanInfo()
    {
        clanInfo.SetActive(!clanInfo.activeSelf);
        clanChat.GetComponent<ClanInfo_Chat>().SetToggleChatSize(!clanInfo.activeInHierarchy);
    }
}

public class ClanLevelData : GameData
{
    public int level { get; private set; } = 0;
    public int exp { get; private set; } = 0;

    public override void SetValue(Dictionary<string, string> data)
    {
        base.SetValue(data);

        level = Int(data["clan_level"]);
        exp = Int(data["clan_exp"]);
    }

    public static ClanLevelData GetLevelDataFromExp(int exp)
    {
        ClanLevelData ret = null;

        int maxExp = int.MaxValue;

        List<GameData> levelDatas = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_level);
        foreach (ClanLevelData data in levelDatas)
        {
            if (data.exp > exp && data.exp < maxExp)
            {
                maxExp = data.exp;
                ret = data;
            }
        }

        return ret;
    }

    public static ClanLevelData GetPrevLevelData(ClanLevelData cur)
    {
        int prevLevel = cur.level - 1;
        if (prevLevel <= 0)
            return null;

        List<GameData> levelDatas = GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.clan_level);
        foreach (ClanLevelData data in levelDatas)
        {
            if (data.level == prevLevel)
            {
                return data;
            }
        }

        return null;
    }
}

