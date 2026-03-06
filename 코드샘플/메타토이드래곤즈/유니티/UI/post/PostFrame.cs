using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PostFrame : MonoBehaviour
    {
        [SerializeField] private Image postStateImg;
        [SerializeField] private Sprite postReadImg;
        [SerializeField] private Sprite postNotReadImg;
        [SerializeField] private Text postNameLabel;
        [SerializeField] private Text expiryDateLabel;

        [SerializeField] private GameObject[] itemList = new GameObject[4];

        [SerializeField] private Button recieveBtn;
        [SerializeField] private Button advertiseBtn;
        [SerializeField] private Image bgNode;

        [SerializeField] private Color readColor;
        [SerializeField] private Color notReadColor;


        public delegate void Callback(int no);
        Callback itemGetCallBack = null;

        public int postNumber { get; private set; }
        public List<Asset> itemArray { get; private set; }
        public bool isItemEnclosed { get; private set; } = false;
        private bool isRead = false;
        public bool IsRead
        {
            set { isRead = value; }
            get { return isRead; }
        }
        public bool IsAdv { get; private set; } = false;
        public void Init(int postNum, string postName, string expiryDate, bool isRead, List<Asset> ItemArr,bool isAdv, Callback itemGetCallback=null)
        {
            //if (postNum == postNumber) return; // 언어별 스트링 변경, 각종 상태 변경을 고려해서 뺏음
            this.isRead = isRead;
            IsAdv = isAdv;
            isItemEnclosed = (ItemArr.Count > 0);
            postNumber = postNum;
            postNameLabel.text = postName;
            expiryDateLabel.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(expiryDate));
            expiryDateLabel.text = expiryDate;
            itemArray = ItemArr;

            int itemLimit = int.Parse(GameConfigTable.GetConfigValue("MAIL_APPEND_ITEM_NUM"));

            for (int j = 0; j < itemLimit; ++j)
            {
                itemList[j].SetActive(false);
            }
            int i = 0;
            foreach (var item in itemArray)
            {
                var userP2E = GameConfigTable.WEB3_MENU_OPEN_ON_KOREAN || User.Instance.ENABLE_P2E;
                if (item.BaseData != null && item.BaseData.ENABLE_P2E && !userP2E)
                    continue;

                itemList[i].SetActive(true);
                itemList[i].GetComponent<ItemFrame>().SetFrameItem(item.ItemNo, item.Amount, (int)item.GoodType);
                ++i;
            }
            ReadStateSetting();
            if (itemGetCallback != null)
            {
                itemGetCallBack = itemGetCallback;
            }
        }
        public void ReadStateSetting()
        {
            foreach (GameObject obj in itemList)
            {
                var postItem = obj.GetComponent<ItemFrame>();
                postItem.setFrameCheck(IsRead);
            }
            bgNode.GetComponent<Image>().color = isRead ? readColor : notReadColor;

            recieveBtn.gameObject.SetActive(false);
            advertiseBtn.gameObject.SetActive(false);
            Button btn = postNumber > 0 ? recieveBtn : advertiseBtn;
            btn.gameObject.SetActive(true);
            
            bool btState =(!isRead) && isItemEnclosed;
            btn.interactable = btState;
            btn.SetButtonSpriteState(btState); 


            postStateImg.sprite = isRead ? postReadImg : postNotReadImg;
        }
        public void clickItemGet()
        {
            if(postNumber < 0)
            {
                List<Asset> AllItems = new List<Asset>();
                int itemNum = ShopGoodsData.Get(Mathf.Abs(postNumber)).REWARD_ID;
                foreach (var dat in ItemGroupData.Get(itemNum))
                {
                    AllItems.Add(dat.Reward);
                }
                if (User.Instance.CheckInventoryGetItem(itemArray))
                {
                    isFullBagAlert();
                    return;
                }
                AdvertiseManager.Instance.TryADWithPopup((log) =>
                {
                    WWWForm param = new WWWForm();
                    param.AddField("prod", Mathf.Abs(postNumber));
                    param.AddField("ad_log", log);
                    param.AddField("count", 1);
                    NetworkManager.Send("shop/buy", param, (response) => {
                        var data = response;
                        var isSuccess = (data["err"].Value<int>() == 0);
                        if (data.ContainsKey("rs"))
                        {
                            var rs = (eApiResCode)data["rs"].Value<int>();
                            switch (rs)
                            {
                                case eApiResCode.OK:
                                {
                                    if (isSuccess)
                                    {
                                        if (data.ContainsKey("rewards"))
                                        {
                                            SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(data["rewards"]), true));
                                            if (itemGetCallBack != null)
                                            {
                                                itemGetCallBack(postNumber);
                                            }

                                            UIMailIconObject.CheckReddot();
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    });
                }, () => { ToastManager.On(StringData.GetStringByStrKey("ad_empty_alert")); });
                return;
            }
            if (User.Instance.CheckInventoryGetItem(itemArray))
            {
                isFullBagAlert();
                return;
            }
            var data = new WWWForm();
            data.AddField("idx_no", postNumber);

            NetworkManager.Send("post/mailget", data, (NetworkManager.SuccessCallback)((JObject jsonData) =>
            {
                if (jsonData.ContainsKey("rs"))
                {
                    switch ((int)jsonData["rs"])
                    {
                        case (int)eApiResCode.OK:
                            IsRead = true;

                            var p2eUserItemCheckList = SBFunc.GetVisibleItemByP2E(itemArray);
                            SystemRewardPopup.OpenPopup(p2eUserItemCheckList, ()=> {
                                /*
                                 * 임시 -WJ - 2023.11.24 - 이벤트 전용 아이템을 우편을 통해 수령시 레드닷 갱신을 해줘야함. 
                                 * item_update 쪽에 각 item KIND 별로 정의를 하기엔 동작성이 나온게 없어서 일단 KIND == 2(이벤트 아이템) 만 처리해둠.
                                 */

                                var includeEventItem = itemArray.Find(element => {
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
                            ReadStateSetting();
                            if(itemGetCallBack != null){
                                itemGetCallBack(postNumber);
                            }
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
        public void isFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
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
    }
}