using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PracticeMatchingPopup : Popup
{
    [Header("로비팝업")]
    [SerializeField] GameObject lobbyMatchingPopup;
    [SerializeField] GameObject joinPopup;
    [SerializeField] GameObject mapSelectPopup;

    [Header("참가 팝업")]
    [SerializeField] Text titleText;
    [SerializeField] Text subTitleText;
    [SerializeField] InputField inputField;
    [SerializeField] Text popupBtnText;
    [SerializeField] Button popupBtn;

    [Header("맵선택 팝업")]
    [SerializeField] Transform mapContainer;
    [SerializeField] GameObject mapCloneTarget;
    [SerializeField] Button mapSelectBtn;
    string nick = "";
    int selectedMap = -1;

    public override void Open(CloseCallback cb = null)
    {
        base.Open(cb);

        UIClear();
        lobbyMatchingPopup.SetActive(true);
    }
    public override void Close()
    {
        UIClear();

        base.Close();
    }

    void UIClear()
    {
        lobbyMatchingPopup.SetActive(true);
        joinPopup.SetActive(false);
        mapSelectPopup.SetActive(false);

        if (!PopupCanvas.Instance.IsOpeningPopup(PopupCanvas.POPUP_TYPE.MATCH_INFO_POPUP))
        {
            var lobby = Managers.Scene.CurrentScene as LobbyScene;
            if (lobby != null)
            {
                lobby.SetEnableMatch(true);
            }
        }
    }


    public override void RefreshUI()
    {
        base.RefreshUI();
    }


    public void MatchingPopupOpen()
    {
        lobbyMatchingPopup.SetActive(true);
        joinPopup.SetActive(false);
        mapSelectPopup.SetActive(false);
    }

    /// <summary>
    /// </summary>
    /// <param name="type"> 0. 공개방 1. 비공개 방 만들기 버튼 2. 참가하기 버튼 3. 비밀번호입력</param>
    public void SetJoinPopup(int type)
    {
        popupBtn.onClick.RemoveAllListeners();

        switch (type)
        {
            case 0:
                OnMapSelectPopup(() =>
                {
                    if (selectedMap < 0)
                        return;
                    mapSelectPopup.SetActive(false);
                    inputField.text = "";
                    OnCreateGame();
                });
                break;
            case 1:
                OnMapSelectPopup(() =>
                {
                    if (selectedMap < 0)
                        return;

                    mapSelectPopup.SetActive(false);
                    titleText.text = StringManager.GetString("비공개방");
                    subTitleText.text = StringManager.GetString("비공개방안내");
                    popupBtnText.text = StringManager.GetString("button_check");

                    joinPopup.SetActive(true);
                    inputField.text = "";
                    inputField.placeholder.GetComponent<Text>().text = StringManager.GetString("비밀번호입력");
                    inputField.contentType = InputField.ContentType.DecimalNumber;
                    popupBtn.onClick.AddListener(OnCreateGameWithPassword);
                });
                break;
            case 2:
                nick = "";
                titleText.text = StringManager.GetString("참가하기");
                subTitleText.text = StringManager.GetString("참가하기닉네임입력");
                popupBtnText.text = StringManager.GetString("button_check");

                inputField.text = "";
                inputField.placeholder.GetComponent<Text>().text = StringManager.GetString("ui_nickname_enter");
                inputField.contentType = InputField.ContentType.Standard;
                popupBtn.onClick.AddListener(OnJoinGame);
                break;
            case 3:
                titleText.text = StringManager.GetString("비공개방");
                subTitleText.text = StringManager.GetString("비공개방안내");
                popupBtnText.text = StringManager.GetString("button_check");

                inputField.text = "";
                inputField.placeholder.GetComponent<Text>().text = StringManager.GetString("비밀번호입력");
                inputField.contentType = InputField.ContentType.DecimalNumber;
                popupBtn.onClick.AddListener(OnPasswordGame);
                break;
            default:
                break;
        }
    }

    public void JoinButton(int btnType)
    {
        lobbyMatchingPopup.SetActive(false);
        joinPopup.SetActive(true);
        mapSelectPopup.SetActive(false);

        SetJoinPopup(btnType);
    }
    public void ClosePopupButton(GameObject obj)
    {
        obj?.SetActive(false);

        lobbyMatchingPopup.SetActive(true);
    }

    void OnCreateGame()
    {
        //임시
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            lobby.OnCreatePracticeGame(string.Empty, selectedMap);
        }

        Close();
    }

    void OnCreateGameWithPassword()
    {
        string password = inputField.text;
        //todo 서버로 방생성 send 
        if (string.IsNullOrEmpty(password))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("비공개방안내"));
            return;
        }

        if (password.Length > 6)
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("비공개방안내"));
            return;
        }

        //임시
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            lobby.OnCreatePracticeGame(password, selectedMap);
        }

        Close();
    }

    void OnJoinGame()
    {
        nick = inputField.text;
        if (string.IsNullOrEmpty(nick))
            return;

        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            lobby.OnJoinPracticeGame(nick);
        }

        Close();
    }

    void OnPasswordGame()
    {
        string password = inputField.text;
        var lobby = Managers.Scene.CurrentScene as LobbyScene;
        if (lobby != null)
        {
            lobby.OnPasswordPracticeGame(nick, password);
            Close();
        }
    }

    void OnMapSelectPopup(UnityEngine.Events.UnityAction cb)
    {
        lobbyMatchingPopup.SetActive(false);
        joinPopup.SetActive(false);
        mapSelectPopup.SetActive(true);

        mapSelectBtn.onClick.RemoveAllListeners();
        mapSelectBtn.onClick.AddListener(cb);

        foreach (Transform child in mapContainer)
        {
            if (child == mapCloneTarget.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        mapCloneTarget.SetActive(true);

        foreach (MapTypeGameData data in GameDataManager.Instance.GetData(GameDataManager.DATA_TYPE.map_type))
        {
            if (!data.use_custom)
                continue;
            GameObject listItem = Instantiate(mapCloneTarget);
            listItem.transform.SetParent(mapContainer);
            RectTransform rt = listItem.GetComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.localScale = Vector3.one;

            rt.GetComponent<Image>().sprite = data.map_icon;
            rt.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnMapFocus(rt, data);
            });

            rt.Find("Cursor").gameObject.SetActive(false);
            rt.Find("MapName").GetComponent<Text>().text = data.GetName();
        }
        mapCloneTarget.SetActive(false);
        selectedMap = -1;
    }

    void OnMapFocus(Transform focusTransform, MapTypeGameData data)
    {
        foreach (Transform map in mapContainer)
        {
            if (map == mapCloneTarget.transform)
            {
                continue;
            }

            map.Find("Cursor").gameObject.SetActive(map == focusTransform);
        }

        selectedMap = data.GetID();
    }
}
