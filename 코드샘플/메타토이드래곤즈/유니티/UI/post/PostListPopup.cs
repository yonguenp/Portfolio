using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PostListPopup : Popup<PopupData>
    {

        [SerializeField] private Button readAllBtn = null;
        [SerializeField] private Button removeAllBtn = null;
        [SerializeField] private TableView tableView = null;
        [SerializeField] private Transform postContents = null;
        [SerializeField] private GameObject post = null;
        [SerializeField] private GameObject nonPostLabel = null;

        public List<MailItem> MailData { get; private set; } = new List<MailItem>();
        List<Asset> itemArray = new List<Asset>();
        List<ITableData> tableDatas;
        private int postItemLimit;
		private int notReadPostCount = 0;
        public bool IsInit { get; private set; } = false;

        // Start is called before the first frame update
        void Start()
        {
            postItemLimit = int.Parse(GameConfigTable.GetConfigValue("MAIL_APPEND_ITEM_NUM"));
            post.SetActive(false);
            
        }

        private void OnEnable()
        {
            PostForceUpdate();
        }

        public void PostForceUpdate()
        {
            
            if (IsInit==false)
            {
                tableView.OnStart();
                IsInit = true;
            }

            itemArray = new List<Asset>();
            MailData.Clear();

            #region 광고 관련 
            ShopManager.Instance.CheckRefreshGoodsByDayChange();
            var reward_gem_id = GameConfigTable.GetConfigIntValue("AD_MAIL_REWARD_GEM");            
            if (reward_gem_id > 0)
            {
                var goods = ShopManager.Instance.GetGoodsState(reward_gem_id);
                if (goods != null && goods.IS_VALIDE)
                {
                    var item = new MailItem();
                    item.idx_no = reward_gem_id * -1;
                    item.title_text = StringData.GetStringByStrKey("AD_MAIL_REWARD_GEM");
                    item.is_receive = false;
                    item.is_delete = false;
                    item.rewardItems = goods.BaseData.REWARDS;
                    item.isAdv = true;
                    MailData.Add(item);
                }
            }
            var reward_item_id = GameConfigTable.GetConfigIntValue("AD_MAIL_REWARD_ITEM");
            if (reward_item_id > 0)
            {
                var goods = ShopManager.Instance.GetGoodsState(reward_item_id);
                if (goods != null && goods.IS_VALIDE)
                {
                    var item = new MailItem();
                    item.idx_no = reward_item_id * -1;
                    item.title_text = StringData.GetStringByStrKey("AD_MAIL_REWARD_ITEM");
                    item.is_receive = false;
                    item.is_delete = false;
                    item.rewardItems = goods.BaseData.REWARDS;
                    item.isAdv = true;
                    MailData.Add(item);
                }
            }
            
            #endregion
            GetPostList();
        }

        public void GetPostList()
        {
            if (string.IsNullOrEmpty(NetworkManager.Instance.UserNo))
                return;

            NetworkManager.Send("post/mailbox", null, ((JObject jsonData) =>
            {
                List<MailItem> mails = new List<MailItem>();
                if (jsonData["mails"].HasValues)
                {
                    JArray jArrays = (JArray)jsonData["mails"];

                    foreach (var val in jArrays)
                    {
                        MailItem item = new MailItem();
                        item.Setdata(val);
                        if (!item.is_delete)
                            mails.Add(item);
                    }

                    mails.Reverse();
                }
                //data.Sort((JArray data1, JArray data2) =>  //우편 정렬  //중요도 순서대로
                //{
                //    return (int)data2[6] - (int)data1[6];
                //});
                int dataLen = mails.Count;
                nonPostLabel.SetActive(false);
                removeAllBtn.interactable = false;
                readAllBtn.interactable = false;
                ButtonExtensions.SetButtonSpriteState(readAllBtn, false);
                ButtonExtensions.SetButtonSpriteState(removeAllBtn, false);

                if (dataLen == 0 )
                {
                    if(MailData.Count == 0)
                        nonPostLabel.SetActive(true);
                    SetMail(true);
                    return;
                }
                //int i = 0;




                //if (dataLen > postList.Count)
                //{
                //    for (i = postList.Count; i < dataLen; ++i)
                //    {
                //        GameObject PostObject = Instantiate(post, postContents);
                //        PostObject.SetActive(true);
                //        postList.Add(PostObject);
                //    }
                //}
                //else if (dataLen < postList.Count)
                //{
                //    for (i = dataLen; i < postList.Count; ++i)
                //    {
                //        postList[i].SetActive(false);
                //    }
                //}

                notReadPostCount = 0;

                //for (i = 0; i < dataLen; ++i)  //전체 메일
                //            {
                //                postList[i].SetActive(true);

                //                const int dayToSec = 24 * 60 * 60;


                //                DateTime dueDate = SBFunc.TimeStampToDateTime(mailData[i].send_at).AddDays(postDateLimit / dayToSec);  //메일 도착 시간 + 유통기한
                //                TimeSpan timeCal = dueDate - DateTime.Now;
                //                string dateString = "";
                //                if (timeCal.Days > 0)
                //                {
                //                    dateString = string.Format(StringData.GetStringByIndex(100000806), timeCal.Days);
                //                }
                //                else
                //                {
                //                    dateString = SBFunc.TimeStringMinute((int)timeCal.TotalSeconds);
                //                }
                //                string PostName = StringData.GetStringByIndex(mailData[i].title_text_id);
                //                if (PostName == mailData[i].title_text_id.ToString())
                //                    PostName = MailStringData.GetStringByIndex(mailData[i].title_text_id);
                //                postList[i].GetComponent<PostFrame>().Init(mailData[i].idx_no, PostName, dateString, mailData[i].is_receive, mailData[i].rewardItems, refreshPostPopupState);
                //            }

                if (mails.Count <= 0) return;
                foreach (var elem in mails)
                {
                    if (elem.is_receive)
                    {
                        removeAllBtn.interactable = true;
                        ButtonExtensions.SetButtonSpriteState(removeAllBtn, true);
                        continue;
                    }
                    else
                    {
                        if (elem.rewardItems.Count > 0)
                        {
                            notReadPostCount++;
                        }
                        itemArray.AddRange(elem.rewardItems);
                    }
                }
                readAllBtn.interactable = notReadPostCount > 0;
                ButtonExtensions.SetButtonSpriteState(readAllBtn, notReadPostCount > 0);
                MailData.AddRange(mails);
                SetMail(true);

                UIMailIconObject.CheckReddot();
            }));
        }

        void SetMail(bool isInitPos = false)
        {
            tableDatas = new List<ITableData>(MailData);
            
            tableView.SetDelegate(new TableViewDelegate(tableDatas, (GameObject node, ITableData item) => {
                if (node == null) return;
                var slotInfo = node.GetComponent<PostFrame>();
                if (slotInfo == null) return;
                var mailDat = (MailItem)item;
                DateTime dueDate = TimeManager.GetCustomDateTime(mailDat.expired_at);
                TimeSpan timeCal = dueDate - TimeManager.GetDateTime();
                string dateString = "";
                if (timeCal.Days > 0)
                {
                    dateString = string.Format(StringData.GetStringByIndex(100000806), timeCal.Days);
                }
                else if(timeCal.TotalSeconds > 0)
                {
                    dateString = SBFunc.TimeStringMinute((int)timeCal.TotalSeconds);
                }

                string PostName = mailDat.title_text;
                slotInfo.Init(mailDat.idx_no, PostName, dateString, mailDat.is_receive, mailDat.rewardItems, mailDat.isAdv,OnClickPostGet);
            }));
            tableView.ReLoad(isInitPos);
        }

        void OnClickPostGet(int postNum)
        {
            var target = MailData.Find(items => items.idx_no == postNum);
            if (target != null)
            {
                target.is_receive = true;

                if (postNum < 0)
                    MailData.Remove(target);
            }

            RefreshPostPopupState();

        }
        public void onClickPostClearBtn()
        {
			if(MailData.Count == 0)
			{
				ToastManager.On(100002565);
				return;
			}

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000807),
                () => { postClear(); }, () => { }, () => { });
        }
        public void postClear()
        {
            /*
             * {
        "err": 0,
        "ts": 1661136687,
        "rs": 0,
        "mails": [
            [23, ... ], [24, ... ] 
            ]
             */
            NetworkManager.Send("post/maildel", null, ((JObject jsonData) =>
            {
                if (jsonData.ContainsKey("rs"))
                {
                    if ((int)jsonData["rs"] == (int)eApiResCode.OK)
                    {
                        List<int> dat = new List<int>();  //삭제된 우편의 번호를 담는 리스트
                        foreach (var elem in jsonData["mails"]) //메일리스트 탐색
                        {
                            dat.Add((int)elem["idx_no"]); // 메일 정보 jObject의 0번 값이 메일번호
                        }
                        List<MailItem> tempList = new List<MailItem>();
                        foreach (var elem in MailData) // 삭제 우편 번호 리스트와 현재 우편 리스트 비교 후 처리
                        {
                            if (dat.Contains(elem.idx_no))
                            {
                                removeAllBtn.interactable = false;
                                ButtonExtensions.SetButtonSpriteState(removeAllBtn, false);
                                tempList.Add(elem);
                            }
                        }
                        foreach (var elem in tempList)
                        {
                            MailData.Remove(elem);
                        }
                        int remainPostCount = MailData.Count;
                        nonPostLabel.SetActive(remainPostCount <= 0);

                        SetMail();
                    }
                }

                UIMailIconObject.CheckReddot();
            })
             );
        }

        public void isFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }
        public void isFullBagByPetAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000749), StringData.GetStringByIndex(100000414), "",
                () => {
                    DragonManagePopup.OpenPopup(2);
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }

        public void readAllPost()
        {
			if (notReadPostCount == 0)
			{
				ToastManager.On(100002564);
				return;
			}

			List<Asset> AllItems = new List<Asset>();
            foreach (var elem in MailData)
            {
                if (!elem.is_receive && elem.isAdv ==false)
                {
                    AllItems.AddRange(elem.rewardItems);
                }
            }
            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                isFullBagAlert();
                return;
            }
            var data = new WWWForm();
            data.AddField("all", 1);
            NetworkManager.Send("post/mailget", data, ((JObject jsonData) =>
            {
                if (jsonData.ContainsKey("rs"))
                {
                    switch ((int)jsonData["rs"])
                    {
                        case (int)eApiResCode.OK:
                            foreach (var elem in MailData)
                            {
                                if(elem.isAdv ==false)
                                    elem.is_receive = true;
                            }
                            readAllBtn.interactable = false;
                            ButtonExtensions.SetButtonSpriteState(readAllBtn, false);
                            var p2eUserItemCheckList = SBFunc.GetVisibleItemByP2E(AllItems);
                            SystemRewardPopup.OpenPopup(p2eUserItemCheckList, ()=> {

                                /*
                                 * 임시 -WJ - 2023.11.24 - 이벤트 전용 아이템을 우편을 통해 수령시 레드닷 갱신을 해줘야함. 
                                 * item_update 쪽에 각 item KIND 별로 정의를 하기엔 동작성이 나온게 없어서 일단 KIND == 2(이벤트 아이템) 만 처리해둠.
                                 */

                                var includeEventItem = AllItems.Find(element => {
                                    var itemNo = element.ItemNo;
                                    var itemData = ItemBaseData.Get(itemNo);
                                    if (itemData == null)
                                        return false;
                                    var itemKind = itemData.KIND;

                                    return itemKind == eItemKind.EVENT;
                                });

                                if (includeEventItem == null)
                                    return;
                                
                                if (EventScheduleData.GetActiveEvents(true).Count >= 0)//이벤트 기간중이면 (이벤트에 사용 되는 아이템을 받을 시에) 뱃지 갱신
                                    UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_BADGE, UIObjectEvent.eUITarget.LT);

                            }, true)?.SetText(StringData.GetStringByIndex(100000765), StringData.GetStringByIndex(100000249));
                            foreach (var post in postContents.GetComponentsInChildren<PostFrame>())
                            {
                                if (post.IsAdv)
                                    continue;
                                if (post.postNumber < 0)
                                    continue;
                                post.IsRead = true;
                                post.ReadStateSetting();
                            }
							RefreshPostPopupState();
							break;
                        case (int)eApiResCode.INVENTORY_FULL:
                        case (int)eApiResCode.PART_FULL:
                            isFullBagAlert();
                            break;
                        case (int)eApiResCode.PET_FULL:
                        case (int)eApiResCode.PET_TOO_MUCH_YOU_HAVE:
                            isFullBagByPetAlert();
                            break;
                    }
                }

                UIMailIconObject.CheckReddot();
            }));
        }
        public void RefreshPostPopupState()
        {
            notReadPostCount = MailData.Count;
            foreach (var elem in MailData)
            {
                if (elem.is_receive)
                {
                    removeAllBtn.interactable = true;
					ButtonExtensions.SetButtonSpriteState(removeAllBtn, true);
					--notReadPostCount;
                }
            }
            readAllBtn.interactable = notReadPostCount > 0;
			ButtonExtensions.SetButtonSpriteState(readAllBtn, notReadPostCount > 0);
            SetMail();
        }

        public override void InitUI()
        {
            //PostForceUpdate();
            //     StartCoroutine(UpdatePost());
            //  PostForceUpdate();
        }
        //IEnumerator UpdatePost()
        //{
        //    while (true)
        //    {
        //        PostForceUpdate();
        //        yield return SBDefine.GetWaitForSeconds(3f);
        //    }
        //}
        //public override void ClosePopup()
        //{
        //    StopCoroutine(UpdatePost());
        //    base.ClosePopup();
        //}


        public override void ClosePopup()
        {
            base.ClosePopup();
            ReddotManager.Set(eReddotEvent.POST_MAIL, notReadPostCount > 0);
        }
    }

    public class MailItem : ITableData
    {
        public int idx_no;
        public string title_text;
        public bool is_receive;
        public bool is_delete;
        public int send_at;
        public int expired_at;
        public bool isAdv =false;
        public List<Asset> rewardItems = new List<Asset>();


        public void Setdata(JToken datas)
        {
            if (datas == null)
                return;

            idx_no = datas["idx_no"].Value<int>();
            title_text = MailStringData.GetStringByIndex(datas["title_text_id"].Value<int>());
            is_receive = datas["is_receive"].Value<int>() == 1;
            is_delete = datas["is_delete"].Value<int>() == 1;
            send_at = datas["send_at"].Value<int>();
            expired_at = datas["expired_at"].Value<int>();

            foreach (var item in datas["items"])
            {
                Asset reward = new Asset((eGoodType)item["item_type"].Value<int>(), item["item_no"].Value<int>(), item["item_count"].Value<int>());
                rewardItems.Add(reward);
            }
        }

        string ITableData.GetKey()
        {
            return idx_no.ToString();
        }

        void ITableData.Init()
        {
            
        }
    }
}
