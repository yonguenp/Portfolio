using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{

    public struct AnnounceData{
        public string SpriteName { get; private set; }
        public string ContentText { get; private set; }
        public eActionType ActionType { get; private set; }

        public string ActionParam { get; private set; }

        public string Url { get; private set; }

        public AnnounceData(string spriteName, string contentText, eActionType actionType, string actionParam, string _url )
        {
            SpriteName = SBFunc.Replace(spriteName);
            ContentText = SBFunc.Replace(contentText) + "\n";
            ActionType = actionType;
            ActionParam = SBFunc.Replace(actionParam);
            Url = SBFunc.Replace(_url);
        }
    }
    public class AnnouncePopup : Popup<PopupData>
    {

        [Header("Menu")]

        [SerializeField]
        private Transform menuTransform = null;
        [SerializeField]
        private GameObject menuObj = null;
        [SerializeField]
        private ScrollRect menuScrollRect = null;


        [Header("Content")]

        [SerializeField]
        private Image contentImg = null;

        [SerializeField]
        private float imgWidth = 0;

        [SerializeField]
        private Text contentText = null;


        [SerializeField]
        private ScrollRect contentScroll = null;

        [Header("bot")]
        [SerializeField]
        private GameObject btnLayerObj = null;

        [SerializeField]
        private Button contentsBtn = null;

        [SerializeField]
        private Toggle todayAlarmToggle = null;

        [SerializeField]
        SBWebViewController webviewController = null;
        

        List<AnnounceMenuObj> menuObjs = new List<AnnounceMenuObj>();
        private int currentMenuID = 0;
        int defaultKey = -1;
        private readonly string AnnounceKey = "Announcement";

        Dictionary<int, AnnounceData> AnnounceDatas = new Dictionary<int, AnnounceData>();
        public override void InitUI()
        {
            Clear();
            todayAlarmToggle.isOn = SBFunc.HasTimeValue(AnnounceKey);
            InfoRequest();
            UICanvas.Instance.StartBackgroundBlurEffect();
        }

        void Clear()
        {
            if( menuObjs==null)
                menuObjs = new List<AnnounceMenuObj>();
            else
                menuObjs.Clear();

            if (AnnounceDatas == null)
                AnnounceDatas = new Dictionary<int, AnnounceData>();
            else
                AnnounceDatas.Clear();
            contentImg.gameObject.SetActive(false);
            contentText.text = string.Empty;
            btnLayerObj.SetActive(false);
            SBFunc.RemoveAllChildrens(menuTransform);
        }

        void InfoRequest()
        {
            WWWForm param = new WWWForm();
            ////system/announcement : 기존 php 파일
            //NetworkManager.Send("system/announcementfile", param, (JObject jsonData) =>
            //{
            //    if((int)jsonData["rs"] == (int)eApiResCode.OK)
            //    {

            //        var arr = (JArray)jsonData["announcement"];

            //        for(int i =0, count =arr.Count; i<count; ++i)
            //        {
            //            var item = arr[i];
            //            int key = item["key"].Value<int>();
            //            if (defaultKey < 0)
            //                defaultKey = key;

            //            var obj = Instantiate(menuObj, menuTransform).GetComponent<AnnounceMenuObj>();
            //            obj.Init(key, item["title"].ToString(), MenuClickCallBack); // id 번호, 메뉴 이름, 콜백

            //            string url = "";
            //            if (((JObject)item).ContainsKey("url"))
            //                url = item["url"].ToString();

            //            AnnounceDatas[key] = new AnnounceData(item["image"].ToString(), item["msg"].ToString(),
            //                (eActionType)item["action"].ToObject<int>(), item["param"].ToString(), url);
            //            menuObjs.Add(obj);

            //            if (string.IsNullOrEmpty(item["image"].ToString()))
            //                continue;

            //            CDNManager.LoadBanner(SBFunc.GetResourceNameByLang(item["image"].ToString(),"announcement"));
            //        }

            //        // 한 프레임 늦게 실행해야 됨. // 모든 메뉴가 생성된 이후.
            //        Invoke("DefaultMenuOpen", 0.1f);
            //        //MenuClickCallBack(defaultKey); // 가장 위의 메뉴를 활성화 시킨다고 상상해서 넣음
            //    }
            //}, (json)=> {

            //});

            //기존버전 대응
            NetworkManager.Send("system/announcement", param, (JObject jsonData) =>
            {
                if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                {

                    var arr = (JArray)jsonData["announcement"];

                    for (int i = 0, count = arr.Count; i < count; ++i)
                    {
                        var item = arr[i];
                        int key = item["key"].Value<int>();
                        if (defaultKey < 0)
                            defaultKey = key;

                        var obj = Instantiate(menuObj, menuTransform).GetComponent<AnnounceMenuObj>();
                        obj.Init(key, item["title"].ToString(), MenuClickCallBack); // id 번호, 메뉴 이름, 콜백

                        string url = "";
                        if (((JObject)item).ContainsKey("url"))
                            url = item["url"].ToString();

                        AnnounceDatas[key] = new AnnounceData(item["image"].ToString(), item["msg"].ToString(),
                            (eActionType)item["action"].ToObject<int>(), item["param"].ToString(), url);
                        menuObjs.Add(obj);

                        if (string.IsNullOrEmpty(item["image"].ToString()))
                            continue;

                        CDNManager.LoadBanner(SBFunc.GetResourceNameByLang(item["image"].ToString(), "announcement"));
                    }

                    // 한 프레임 늦게 실행해야 됨. // 모든 메뉴가 생성된 이후.
                    Invoke("DefaultMenuOpen", 0.01f);
                    //MenuClickCallBack(defaultKey); // 가장 위의 메뉴를 활성화 시킨다고 상상해서 넣음
                }
            });
        }

        public void SetDefaultMenuId (int menuId)
        {
            defaultKey = menuId;
        }

        void DefaultMenuOpen()
        {
            MenuClickCallBack(defaultKey);

            AnnounceData defaultData = new AnnounceData();
            string defaultUrl = "";
            if (AnnounceDatas != null && AnnounceDatas.ContainsKey(defaultKey))
            {
                defaultData = AnnounceDatas[defaultKey];
                defaultUrl = defaultData.Url;
            }

            SetWebView(defaultUrl);
        }

        public void MenuClickCallBack(int id)
        {
            if (id == -1)
                return;
            foreach(var obj in menuObjs)
            {
                if(obj.SetMenuSelectSprite(id))
                {
                    menuScrollRect.FocusOnItem(obj.GetComponent<RectTransform>(), 0.3f);
                }
            }
            
            contentScroll.verticalNormalizedPosition = 1;
            currentMenuID = id;

            if (AnnounceDatas.ContainsKey(currentMenuID) == false)
                return;

            var currentMenuData = AnnounceDatas[currentMenuID];

            // 이미지 설정
            if (string.IsNullOrEmpty(currentMenuData.SpriteName))
            {
                contentImg.gameObject.SetActive(false);
            }
            else
            {
                contentImg.gameObject.SetActive(true);
                CDNManager.TrySetBannerCatchDefault(currentMenuData.SpriteName, "announcement", contentImg, () =>
                {
                    if (contentImg.sprite != null)
                    {
                        var ratio = contentImg.sprite.bounds.size.y / contentImg.sprite.bounds.size.x;
                        contentImg.GetComponent<RectTransform>().sizeDelta = new Vector2(imgWidth, imgWidth * ratio);
                    }
                }, ()=> {
                    contentImg.gameObject.SetActive(false);
                });
            }

            // 텍스트 설정
            contentText.text = currentMenuData.ContentText;

            bool isBtnActionExist = currentMenuData.ActionType != eActionType.NONE;
            btnLayerObj.SetActive(isBtnActionExist);

            SetWebView(currentMenuData.Url);
        }

        void SetWebView(string url = "")
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (webviewController != null)
                {
                    webviewController.CloseWebView();
                    webviewController.gameObject.SetActive(true);
                    webviewController.OnWebView(url);
                }
            }
            else
            {
                if (webviewController != null)
                {
                    webviewController.CloseWebView();
                    webviewController.gameObject.SetActive(false);
                }
            }
        }


        public void OnClickDetailBtn()
        {
            if (AnnounceDatas.ContainsKey(currentMenuID) == false)
                return;

            //자세히 보기 버튼 동작 설정
            if(SBFunc.InvokeCustomAction(AnnounceDatas[currentMenuID].ActionType, AnnounceDatas[currentMenuID].ActionParam))
            {
                ClosePopup();
            }
        }

        public override void ClosePopup()
        {
            if (webviewController != null)
                webviewController.CloseWebView();

            base.ClosePopup();
            defaultKey = -1;
            if (todayAlarmToggle.isOn)
            {
                SBFunc.SetTimeValue(AnnounceKey);
            }
            else
            {
                CacheUserData.DeleteKey(AnnounceKey);
            }


            UICanvas.Instance.EndBackgroundBlurEffect();
        }
    }
}

