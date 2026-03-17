using Crosstales;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ProfileIconSettingPopup : MonoBehaviour
{
    uint curTitleID;

    public ProfileIconButton profileIconBtn;

    [Header("Common - Properties")]
    public GameObject subPopupobj;
    public GameObject confPopupobj;
    public Sprite OnSprite;
    public Sprite OffSprite;

    [Header("Common - On")]
    public Color OnColor;
    public Color OnTextColor;
    public Color OnIDColor;

    [Header("Common - Off")]
    public Color OffColor;
    public Color OffTextColor;
    public Color OffIDColor;

    [Header("Common - Button Color")]
    public Color BtnOnColor;
    public Color BtnOffColor;

    [Header("Dropdown Scroll")]
    public GameObject TitleClone;
    public Transform contentTf;
    public GameObject scroll;
    public Button btnCommit;
    public Image imgCommit;

    [Header("Debug Values")]
    public uint selTitleID;
    Sprite[] imgSheet;
    List<GameObject> clones;

    public void OnCloseIconPopup()
    {
        //OnChangeTitleBtn();
        subPopupobj.SetActive(false);
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.MYTITLE_POPUP);
    }

    public void OpenDropDownScroll()
    {
        StartCoroutine(DropScroll());
    }

    IEnumerator DropScroll()
    {
        scroll.SetActive(!scroll.active);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTf as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.transform as RectTransform);
        yield return new WaitForFixedUpdate();
        float height = ((contentTf as RectTransform).sizeDelta.y < 100 ? 1478 : (contentTf as RectTransform).sizeDelta.y) - scroll.GetComponent<ScrollRect>().viewport.sizeDelta.y;
        float targetHeight = ((selTitleID - 1) * 5) + (selTitleID - 1) * (TitleClone.transform as RectTransform).sizeDelta.y;
        scroll.GetComponent<ScrollRect>().verticalNormalizedPosition = (height - targetHeight) / height;
        yield return null;
    }

    public void OnChangeTitleBtn()
    {
        btnCommit.interactable = selTitleID != curTitleID;
        if ( selTitleID != curTitleID)
        {
            //버튼 색상 변경
            imgCommit.color = BtnOnColor;
            return;
        }
        imgCommit.color = BtnOffColor;
    }

    public void OnClickTitleBtn(uint id)
    {
        selTitleID = id;
        gameObject.CTFind("CurImg").GetComponent<Image>().sprite = imgSheet[id-1];
        gameObject.CTFind("CurTitle").GetComponent<Text>().text = LocalizeData.GetText(string.Format("USERICON_{0}", id-1));
        OnChangeTitleBtn();
        OpenDropDownScroll();
    }

    private void OnEnable()
    {
        object profileList;
        
        if (GameDataManager.Instance.GetUserData().data.TryGetValue("profileList", out profileList) && ((int[])profileList).Length > 0)
        {
            init((uint[])profileList);
            subPopupobj.SetActive(true);
        }
        else
        {
            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_330"), LocalizeData.GetText("SERVER_ERROR"), ()=> { NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.MYTITLE_POPUP); });
        }        
    }

    public void SetTitleTarget()
    {
        //WWW 통신
        WWWForm data = new WWWForm();
        data.AddField("uri", "user");
        data.AddField("op", 10);
        data.AddField("profile", selTitleID.ToString());

        NetworkManager.GetInstance().SendApiRequest("user", 10, data, (response) =>
        {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == "user")
                {
                    JToken opCode = row["op"];
                    JToken resultCode = row["rs"];
                    string msgTitle = LocalizeData.GetText("LOCALIZE_330");

                    if (resultCode != null && resultCode.Type == JTokenType.Integer && opCode != null && opCode.Type == JTokenType.Integer && opCode.Value<uint>() == 10)
                    {
                        int rs = resultCode.Value<int>();
                        JObject rewardObj = (JObject)row["rew"];
                        
                        if(rs == 0)
                        {
                            Debug.Log("쿠폰 아이템 받음");
                            GameDataManager.Instance.GetUserData().data["profileId"] = selTitleID;
                            //NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("프로필아이콘변경"));
                            ShowCustomConfirmPopup();
                            profileIconBtn.RefreshProfileIcon();
                            curTitleID = selTitleID;
                            OnChangeTitleBtn();
                            return;
                        }                                
                    }
                    NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(msgTitle, LocalizeData.GetText("SERVER_ERROR"));
                }
            }
        });
    }

    void init(uint[] profileList)
    {
        TitleClone.SetActive(false);
        //현재 타이틀 사용중인것으로 불러오기
        users user = GameDataManager.Instance.GetUserData();
        if (user != null)
        {
            object obj;
            if (user.data.TryGetValue("profileId", out obj))
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Neco/Ui/Idcard");
                uint index = (uint)obj;
                if (index > 0)
                {
                    if (sprites.Length > index - 1)
                    {
                        curTitleID = index;
                        gameObject.CTFind("CurImg").GetComponent<Image>().sprite = sprites[index - 1];
                        gameObject.CTFind("CurTitle").GetComponent<Text>().text = LocalizeData.GetText(string.Format("USERICON_{0}", index - 1));
                    }
                    else
                    {
                        gameObject.CTFind("CurImg").GetComponent<Image>().sprite = sprites[0];
                        gameObject.CTFind("CurTitle").GetComponent<Text>().text = LocalizeData.GetText("USERICON_0");
                    }
                }
            }
        }
        scroll.SetActive(true);
        scroll.SetActive(false);

        imgSheet = Resources.LoadAll<Sprite>("Sprites/Neco/Ui/Idcard");
        if(clones == null)
        {
            clones = new List<GameObject>();
            for (int i = 0; i < imgSheet.Length; i++)
            {
                GameObject obj = Instantiate(TitleClone, contentTf);
                Image idcard = obj.CTFind("idcard").GetComponent<Image>();
                Text title = obj.CTFind("text").GetComponent<Text>();

                idcard.sprite = imgSheet[i];
                title.text = LocalizeData.GetText(string.Format("USERICON_{0}", i));
                obj.SetActive(true);
                clones.Add(obj);

                if (profileList.Contains((uint)i+1))
                {
                    //타이틀 소지함
                    obj.GetComponent<Image>().sprite = OnSprite;
                    obj.GetComponent<Image>().color = OnColor;

                    idcard.color = OnIDColor;
                    title.color = OnTextColor;

                    obj.GetComponent<Button>().interactable = true;
                }
                else
                {
                    obj.GetComponent<Image>().sprite = OffSprite;
                    obj.GetComponent<Image>().color = OffColor;

                    idcard.color = OffIDColor;
                    title.color = OffTextColor;

                    obj.GetComponent<Button>().interactable = false;
                }
                uint temp = (uint)i + 1;
                obj.GetComponent<Button>().onClick.AddListener(() => { OnClickTitleBtn(temp); });
            }
        }
        else
        {
            for (int i = 0; i < imgSheet.Length; i++)
            {
                GameObject obj = clones[i];
                Image idcard = obj.CTFind("idcard").GetComponent<Image>();
                Text title = obj.CTFind("text").GetComponent<Text>();

                if (profileList.Contains((uint)i + 1))
                {
                    //타이틀 소지함
                    obj.GetComponent<Image>().sprite = OnSprite;
                    obj.GetComponent<Image>().color = OnColor;

                    idcard.color = OnIDColor;
                    title.color = OnTextColor;

                    obj.GetComponent<Button>().interactable = true;
                }
                else
                {
                    obj.GetComponent<Image>().sprite = OffSprite;
                    obj.GetComponent<Image>().color = OffColor;

                    idcard.color = OffIDColor;
                    title.color = OffTextColor;

                    obj.GetComponent<Button>().interactable = false;
                }
            }
        }

        selTitleID = curTitleID;
        OnChangeTitleBtn();
        confPopupobj.SetActive(false);
    }

    void ShowCustomConfirmPopup()
    {
        subPopupobj.SetActive(false);
        confPopupobj.SetActive(true);
    }
}
