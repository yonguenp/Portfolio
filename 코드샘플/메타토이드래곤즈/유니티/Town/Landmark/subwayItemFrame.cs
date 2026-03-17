using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SandboxNetwork { 
    public class subwayItemFrame : MonoBehaviour
    {
        [SerializeField] private GameObject emptyLayerObject;
        [SerializeField] private GameObject itemLayerObject;

        [SerializeField] private Image itemImg;
        [SerializeField] private Text itemNameText;
        [SerializeField] private Text itemNeedAmountText;
        [SerializeField] private GameObject itemInfoBubble;
        [SerializeField] private Button itemSubmitButton;
        [SerializeField] private SkeletonGraphic boxSkeleton;
        [SerializeField] private GameObject sendCheckIconObject;

        [SerializeField] private UnityEvent refreshAllSlotEvent;
        [SerializeField] private UnityEvent boxOutEvent;

        bool isSend = false;  //이미 납품했는지 여부
        public bool IsSend
        {
            get
            {
                return isSend;
            }
        }
        bool isAvailableSend = false; // 아이템 납품 가능여부
        public bool IsAvailableSend
        {
            get
            {
                return isAvailableSend;
            }
        }
        int currentPlatId;
        int currentSlotId;
        int itemID;
        public int ItemID { get { return itemID; } }

        int itemNeedAmount;
        int itemSubmitAmount;
        bool allSendState = false;
        Sequence twSequence;

        private void Start()
        {
            //itemID = 0;
            twSequence = DOTween.Sequence();
            //twSequence.SetAutoKill(false);

            //박스 애니메이션 시간
            float duration = boxSkeleton.SkeletonData.Animations.Items[0].Duration;
        }

        public void InitEmptyLayer()
        {
            emptyLayerObject?.SetActive(true);
            itemLayerObject?.SetActive(false);
            itemInfoBubble?.SetActive(false);

            GetComponent<CanvasGroup>().alpha = 1;
        }

        public void InitItemLayer(List<int> slotData,int platId, int slotId)
        {
            if (slotData == null) return;

            itemID = slotData[0];
            itemNeedAmount = slotData[1];
            itemSubmitAmount = slotData[2];
            currentPlatId = platId;
            currentSlotId = slotId;
            allSendState = false;

            emptyLayerObject?.SetActive(false);
            itemLayerObject?.SetActive(true);
            itemInfoBubble?.SetActive(false);

            GetComponent<CanvasGroup>().alpha = 1;
            itemImg.rectTransform.localPosition = new Vector3(0, 17, 0);
            twSequence.Rewind();

            InitItemData();
        }

        void InitItemData()
        {
            var itemData = ItemBaseData.Get(itemID);
            itemImg.sprite = itemData.ICON_SPRITE;
            isSend = itemNeedAmount == itemSubmitAmount;  //이미 납품했는지 여부
            if (isSend)
            {
                SetBoxState(false);
            }
            else
            {
                SetBoxState(true);
            }

            RefreshItemAmount();
            itemNameText.text = itemData.NAME;
        }

        public void RefreshItemLayer(List<int> slotData)
        {
            if (slotData == null) return;

            emptyLayerObject?.SetActive(false);
            itemLayerObject?.SetActive(true);
            itemInfoBubble?.SetActive(false);

            GetComponent<CanvasGroup>().alpha = 1;

            twSequence.Rewind();

            itemID = slotData[0];
            itemNeedAmount = slotData[1];
            itemSubmitAmount = slotData[2];

            if (IsSend == false)
            {
                if (itemNeedAmount == itemSubmitAmount)
                {
                    boxPackAnim();
                }
            }

            RefreshItemAmount();
        }

        public void RefreshItemAmount()
        {
            var itemData = User.Instance.GetItem(itemID);
            int myItemCount = 0;
            if(itemData != null)
            {
                myItemCount = itemData.Amount;
            }
            itemNeedAmountText.text = string.Format("{0}/{1}", myItemCount, itemNeedAmount);
            itemNeedAmountText.color = myItemCount < itemNeedAmount ? Color.red : Color.black;
            isAvailableSend = itemNeedAmount <= myItemCount;

            itemSubmitButton.SetInteractable(IsAvailableSend);
            itemSubmitButton.SetButtonSpriteState(IsAvailableSend);

            // 버블이 켜져있는 상태에서 다른 슬롯 납품 시 버블 처리
            if (itemInfoBubble.activeInHierarchy)
            {
                itemInfoBubble.SetActive(IsAvailableSend);
            }

            if (isSend)
            {
                itemNeedAmountText.color = Color.black;
            }
        }

        public void SetBoxState(bool isOpen)
        {
            if (isOpen)
            {
                //boxSkeleton.AnimationState.SetAnimation(0, "animation", false);
                if (boxSkeleton.AnimationState.Tracks.Items[0] != null)
                {
                    foreach (var trackItem in boxSkeleton.AnimationState.Tracks)
                    {
                        trackItem.TrackTime = 0;
                    }
                    //boxSkeleton.AnimationState.Tracks.Items[0].TrackTime = 0;
                }
                boxSkeleton.AnimationState.TimeScale = 0;
                
                //itemInfoBubble.SetActive(true);
                itemImg.gameObject.SetActive(true);
            }
            else
            {
                if (boxSkeleton.AnimationState.Tracks.Items[0] != null)
                {
                    foreach (var trackItem in boxSkeleton.AnimationState.Tracks)
                    {
                        trackItem.TrackTime = trackItem.TrackComplete;
                    }
                    //boxSkeleton.AnimationState.Tracks.Items[0].TrackTime = 0;
                }
                boxSkeleton.AnimationState.TimeScale = 0;

                itemInfoBubble.SetActive(false);
                itemImg.gameObject.SetActive(false);
            }

            sendCheckIconObject?.SetActive(!isOpen);
        }
        public void startBoxLockAnim()
        {
            boxSkeleton.AnimationState.TimeScale = 1;
            boxSkeleton.AnimationState.SetAnimation(0, "animation", false);
            boxSkeleton.AnimationState.Complete -= SpineEndEvent; //스파인 애니메이션 일단 비워보자
            boxSkeleton.AnimationState.Complete += SpineEndEvent; //스파인 애니메이션 종료 시 이벤트 추가

        }

        void SpineEndEvent(Spine.TrackEntry te)  
        {
            if (allSendState)  // 모든걸 보낼수 있는 상태고 심지어 애니메이션도 끝났다면 
            {
                //여기서 상자 올라가는 애니메이션 실행하자

                boxOutEvent.Invoke();

                allSendState = false;

                //Invoke(nameof(ResetBoxAnim), 0.5f);
            }

            sendCheckIconObject?.SetActive(true);
        }

        public void boxPackAnim()
        {
            twSequence = DOTween.Sequence();
            twSequence.Append(itemImg.rectTransform.DOLocalMoveY(50, 0.2f));
            twSequence.Append(itemImg.rectTransform.DOLocalMoveY(-26, 0.2f));
            twSequence.AppendCallback(startBoxLockAnim);

            isSend = true;

            itemInfoBubble?.SetActive(false);
            RefreshItemAmount();

            //if (isSend == false) {

            //    twSequence = DOTween.Sequence();
            //    twSequence.Append(itemImg.rectTransform.DOLocalMoveY(50, 0.2f));
            //    twSequence.Append(itemImg.rectTransform.DOLocalMoveY(-26, 0.2f));
            //    twSequence.AppendCallback(startBoxLockAnim);

            //    itemInfoBubble.SetActive(false);
            //    isSend = true;
            //}
        }

        public void OnClickSendItemButton() // 단일 납품
        {
            WWWForm param = new WWWForm();
            param.AddField("plat", currentPlatId);
            param.AddField("slot", currentSlotId);


            NetworkManager.Send("subway/input", param, (JObject jsonData) =>
              {
                  if(jsonData["rs"] != null)
                  {
                      switch ((eApiResCode)(int)jsonData["rs"])
                      {
                          case eApiResCode.OK:
                              
                              // 전체 아이템들 갯수 갱신
                              
                              if (jsonData["start"] != null && (int)jsonData["start"] == 1)  // 이 플랫폼 아이템 전부 충족으로 배달 시작 상태를 알리자
                              {
                                  allSendState = true;                                  
                              }
                              
                              boxPackAnim();
                              refreshAllSlotEvent.Invoke();

                              UITrainStateEvent.Event(allSendState);
                              break;
                          case eApiResCode.COST_SHORT:  // 아이템 갯수 부족
                              break;
                      }
                  }
              });
        }

        public void OnClickShowBubble()
        {
            if (itemInfoBubble == null) return;
            if (IsSend) // 이미 보낸상자 클릭 불가
            {
                ToastManager.On(StringData.GetStringByStrKey("subway_already_send"));
                return;
            }
            if (IsAvailableSend == false) // 갯수 부족  - submit 상태는 요구재료가 인벤갯수보다 적을때 저장되기때문에 고려할 필요 없음.
            {
                var needItemList = new List<Asset>() { new Asset(itemID, itemNeedAmount)};
                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                    OnClickSendItemButton();//구매 했으면 납품 보내기 시도
                },false,(index)=> {
                    refreshAllSlotEvent.Invoke();
                });

                //ToastManager.On(StringData.GetStringByStrKey("town_upgrade_text_07"));
                return;
            }

            itemInfoBubble.SetActive(!itemInfoBubble.activeSelf);
        }
    }
}