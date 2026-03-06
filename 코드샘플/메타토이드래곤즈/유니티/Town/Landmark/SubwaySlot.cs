using DG.Tweening;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SubwaySlot : MonoBehaviour
    {
        [SerializeField] UnityEvent refreshAllSlotEvent;

        [SerializeField] SubwayLayer parentLayer = null;

        [Header("lock layer")] //슬롯 잠김
        [Space(10)]
        [SerializeField] GameObject lockLayer = null;
        [SerializeField] Text lockOffConditionLabel = null;

        [Header("unlock layer")] // 슬롯 오픈 가능
        [Space(10)]
        [SerializeField] GameObject unlockLayer = null;
        [SerializeField] Button unlockButton = null;
        [SerializeField] Text unlockMoneyLabel = null;

        [Header("ready layer")]  // 납품 가능
        [Space(10)]
        [SerializeField] GameObject readyLayer = null;
        [SerializeField] Button readyAllBtn = null; // 모두 납품 버튼
        [SerializeField] subwayItemFrame[] subwayItems = null;
        [Header("delivering layer")]  // 납품 중
        [Space(10)]
        [SerializeField] GameObject deliveringLayer = null;
        [SerializeField] Button deliveringAccButton = null;
        [SerializeField] TimeObject deliveringTimeObj = null;
        [SerializeField] Text deliveringTimeLabel = null;

        [Header("delivering finish layer")]  // 납품 완료
        [Space(10)]
        [SerializeField] GameObject deliveringFinishLayer = null;
        [SerializeField] Transform rewardContainer = null;

        [Header("subway")]  // 지하철
        [SerializeField] trainMoveUI train = null;
        [SerializeField] GameObject lightOnLayer = null;     // 배경 조명 on
        [SerializeField] GameObject lightOffLayer = null;   // 배경 조명 off
        [SerializeField] Text platformText = null;

        [Header("Bubble")]  // 필요재화 버블
        [SerializeField] DOTweenAnimation bubbleAnimObj = null;

        [Header("NPC")]
        [SerializeField] Spine.Unity.SkeletonGraphic npc;

        SubwayPlatformData currentTableData = null;
        LandmarkSubwayPlantData currentPlatformData = null;
        public LandmarkSubwayPlantState LastState { get; private set; } = LandmarkSubwayPlantState.NONE;
        int currentPlatID = 0;

        int delieveringTime = 0;
        int delieveringTimeSum = 0;

        public void Init(int platformID)
        {
            currentPlatID = platformID;
            currentPlatformData = User.Instance.GetLandmarkData<LandmarkSubway>().PlatsData[currentPlatID];
            currentTableData = SubwayPlatformData.GetByFlatform(currentPlatformData.ID);
            platformText.text = StringData.GetStringFormatByStrKey("지하철번호", currentPlatID + 1);

            deliveringAccButton.interactable = true;

            UpdateLayerState();
        }
        public void AllLayerOff()
        {
            lockLayer.SetActive(false);
            unlockLayer.SetActive(false);
            readyLayer.SetActive(false);
            deliveringLayer.SetActive(false);
            deliveringFinishLayer.SetActive(false);
        }
        void UpdateLayerState()  // 현재 플랫폼 상태에 따른 layer 끄고 켜기 && 아이템 정보 세팅
        {
            if (currentPlatformData == null) return;

            AllLayerOff();

            npc.transform.DOKill();
            npc.transform.DOLocalMoveY(-360, 1.0f);

            switch (currentPlatformData.State)
            {
                case LandmarkSubwayPlantState.LOCKED:
                    lockOffConditionLabel.text = string.Format(StringData.GetStringByIndex(100000059), currentTableData.OPEN_LEVEL);
                    lockLayer.SetActive(true);
                    
                    break;
                case LandmarkSubwayPlantState.CAN_UNLOCK:
                    unlockLayer.SetActive(true);

                    bool isAvailUnlock = User.Instance.GOLD >= currentTableData.COST_NUM;

                    unlockMoneyLabel.text = SBFunc.CommaFromNumber(currentTableData.COST_NUM);

                    unlockButton.SetInteractable(isAvailUnlock);
                    unlockButton.SetButtonSpriteState(isAvailUnlock);
                    SetBubbleNodeEffect(isAvailUnlock);

                    unlockMoneyLabel.color = isAvailUnlock ? Color.white : Color.red;

                    break;
                case LandmarkSubwayPlantState.READY:
                    readyLayer.SetActive(true);
                    SetItemData();
                    SetReadyBtnState(true);

                    if (train != null)
                    {
                        if (LastState != currentPlatformData.State)
                        {
                            if (LastState != LandmarkSubwayPlantState.DELIVER_COMPLETE)
                            {
                                train.MoveToSpecific(eTrainState.In, eTrainState.DoorOpenIdle);
                            }
                            else
                            {
                                train.MoveToSpecific(eTrainState.DoorOpen, eTrainState.DoorOpenIdle);
                            }
                        }
                        else
                        {
                            train.MoveToSpecific(eTrainState.DoorOpen, eTrainState.DoorOpenIdle);
                        }
                    }
                    break;
                case LandmarkSubwayPlantState.DELIVERING:
                    deliveringLayer.SetActive(true);
                    GetAllDeliveringTimeSum();
                    SetDeliveringData();
                    SetReadyBtnState(false);

                    if (train != null)
                    {
                        if (LastState != currentPlatformData.State)
                        {
                            if (LastState == LandmarkSubwayPlantState.READY)
                            {
                                train.MoveToSpecific(eTrainState.DoorClose, eTrainState.Out);
                            }
                            else
                            {
                                train.trainSetPos(0);
                            }
                        }
                        else
                        {
                            train.trainSetPos(0);
                        }
                    }

                    npc.transform.DOKill();
                    npc.transform.DOLocalMoveY(-180, 1.0f);
                    break;
                case LandmarkSubwayPlantState.DELIVER_COMPLETE:
                    deliveringFinishLayer.SetActive(true);
                    SetDeliverFinishData();
                    SetReadyBtnState(false);

                    train.curTrainState = eTrainState.In;
                    if (train != null)
                    {
                        if (LastState != currentPlatformData.State)
                        {
                            train.MoveToSpecific(eTrainState.In, eTrainState.DoorOpenIdle);
                        }
                        else
                        {
                            train.MoveToSpecific(eTrainState.DoorOpen, eTrainState.DoorOpenIdle);
                        }
                    }


                    npc.transform.DOKill();
                    npc.transform.DOLocalMoveY(-180, 1.0f);
                    break;
            }

            LastState = currentPlatformData.State;

            // bg 조명 설정
            bool lightState = currentPlatformData.State == LandmarkSubwayPlantState.LOCKED || currentPlatformData.State == LandmarkSubwayPlantState.CAN_UNLOCK;
            lightOnLayer?.SetActive(!lightState);
            lightOffLayer?.SetActive(lightState);
        }

        void SetItemData()
        {
            foreach (var item in subwayItems)
            {
                item.gameObject.SetActive(false);
            }
            //readyAllBtn.SetInteractable(false);

            if (currentPlatformData.Slots.Count > subwayItems.Length) return;

            for (int i = 0; i < subwayItems.Length; ++i)
            {
                if (currentPlatformData.Slots.Count <= i)
                {
                    subwayItems[i].gameObject.SetActive(true);
                    subwayItems[i].InitEmptyLayer();
                }
                else if (currentPlatformData.Slots[i] != null)
                {
                    subwayItems[i].gameObject.SetActive(true);
                    subwayItems[i].InitItemLayer(currentPlatformData.Slots[i], currentPlatformData.ID, i);
                    //if (subwayItems[i].IsAvailableSend && !subwayItems[i].IsSend)
                    //{
                    //    readyAllBtn.SetInteractable(true);
                    //}
                }
            }
        }

        void RefreshItemData()
        {
            foreach (var item in subwayItems)
            {
                item.gameObject.SetActive(false);
            }
            //readyAllBtn.SetInteractable(false);

            if (currentPlatformData.Slots.Count > subwayItems.Length) return;

            for (int i = 0; i < subwayItems.Length; ++i)
            {
                if (currentPlatformData.Slots.Count <= i)
                {
                    subwayItems[i].gameObject.SetActive(true);
                    subwayItems[i].InitEmptyLayer();
                }
                else if (currentPlatformData.Slots[i] != null)
                {
                    subwayItems[i].gameObject.SetActive(true);
                    subwayItems[i].RefreshItemLayer(currentPlatformData.Slots[i]);
                    //if (subwayItems[i].IsAvailableSend && !subwayItems[i].IsSend)
                    //{
                    //    readyAllBtn.SetInteractable(true);
                    //}
                }
            }
        }
        /// <summary>
        /// SBFunc 로 호출 따로 
        /// </summary>
        /// <returns></returns>
        List<Asset> GetNeedItemList()
        {
            List<Asset> needItemDataList = new List<Asset>();
            for (int i = 0; i < subwayItems.Length; ++i)
            {
                if (currentPlatformData.Slots.Count <= i)
                    continue;

                if (currentPlatformData.Slots[i] != null)
                {
                    var slotData = currentPlatformData.Slots[i];
                    var itemID = slotData[0];
                    var itemNeedAmount = slotData[1];
                    //var itemSubmitAmount = slotData[2];//필요없을듯

                    if (subwayItems[i].IsSend)//이미 보냄
                        continue;

                    needItemDataList.Add(new Asset(itemID, itemNeedAmount));
                }
            }

            return needItemDataList;
        }


        public bool IsAllSendCondition()
        {
            int checkCount = 0;
            for (int i = 0; i < currentPlatformData.Slots.Count; ++i)
            {
                if (subwayItems[i].IsAvailableSend || subwayItems[i].IsSend)//이미 풀 제출 or 인벤에서 납품 가능할 경우
                    checkCount++;
            }

            return checkCount == currentPlatformData.Slots.Count;
        }

        public void SetReadyBtnState(bool state)
        {
            if (readyAllBtn == null) return;

            //readyAllBtn.SetInteractable(state);
            readyAllBtn.SetButtonSpriteState(state);
        }

        public void SendAllCompleteCallBack()
        {
            Sequence twSequence = DOTween.Sequence();

            for (int i = 0; i < currentPlatformData.Slots.Count; ++i)
            {
                //float yPos = subwayItems[i].transform.position.y;
                twSequence.Append(subwayItems[i].transform.DOLocalMoveY(30f, 0.2f).SetEase(Ease.OutQuad));
                twSequence.Join(subwayItems[i].GetComponent<CanvasGroup>().DOFade(0, 0.2f).SetEase(Ease.OutQuad));

            }
            twSequence.AppendCallback(UpdateLayerState);
        }

        public void SendAllBtn()//전체 발송을 할수 있는 상태인지 컨디션 체크 먼저
        {
            if(!IsAllSendCondition())
            {
                RequestShowBuyNowPopup();
                return;
            }

            SetReadyBtnState(false);
            WWWForm param = new WWWForm();
            param.AddField("plat", currentPlatformData.ID);
            param.AddField("all", 1);
            NetworkManager.Send("subway/input", param, (JObject jsonData) =>
            {
                SetReadyBtnState(true);
                if (jsonData["rs"] != null && (int)jsonData["rs"] == 0)
                {
                    if ((int)jsonData["start"] == 1) //전부 보내기
                    {
                        foreach (var item in subwayItems)
                        {
                            if (item.IsSend == false)
                            {
                                item.boxPackAnim();
                            }
                            item.RefreshItemAmount();
                        }
                        Invoke("SendAllCompleteCallBack", 2);

                        refreshAllSlotEvent.Invoke();

                        UITrainStateEvent.Event(true);
                    }
                    else  //일부 아이템만 보내기 - 부족 분에 한해서 즉시구매 팝업 출력
                    {
                        RefreshItemData();

                        refreshAllSlotEvent.Invoke();
                        
                        UITrainStateEvent.Event(false);

                        RequestShowBuyNowPopup();
                    }
                }
            });
        }

        void RequestShowBuyNowPopup()
        {
            var needItemList = GetNeedItemList();
            if (needItemList.Count > 0)
                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                    RefreshItemData();
                    refreshAllSlotEvent.Invoke();

                    SendAllBtn();
                }, false, (index) => {

                    var itemID = index;
                    foreach (var item in subwayItems)
                    {
                        if (item.ItemID == itemID)
                            item.OnClickSendItemButton();
                    }
                });
        }

        // 납품 중 타이머 처리
        void SetDeliveringData()
        {
            if (deliveringTimeObj == null || deliveringTimeLabel == null) return;
            deliveringTimeObj.Time = TimeManager.GetTime();
            deliveringTimeObj.Refresh = delegate
            {
                delieveringTime = TimeManager.GetTimeCompare(currentPlatformData.Expire);
                deliveringTimeLabel.text = SBFunc.TimeString(delieveringTime);
                if (delieveringTime <= 0)
                {
                    deliveringTimeObj.Refresh = null;

                    NetworkManager.Send("subway/state", null, (JObject jsonData) =>
                    {
                        if (jsonData["rs"] != null && (int)jsonData["rs"] == (int)eApiResCode.OK)
                        {
                            UpdateLayerState();
                            UITrainStateEvent.Event();
                        }
                    });
                }
            };
        }

        void GetAllDeliveringTimeSum()
        {
            delieveringTimeSum = SubwayDeliveryTable.GetTotalDeliveringTime(currentPlatformData);
        }

        // 납품 끝나고 보상 세팅
        void SetDeliverFinishData()
        {
            if (rewardContainer == null) return;
            SBFunc.RemoveAllChildrens(rewardContainer);
            GameObject item = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab");
            foreach (var reward in currentPlatformData.Reward)
            {
                if (reward == null) continue;
                var newItemFrame = Instantiate(item, rewardContainer);
                newItemFrame.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                newItemFrame.GetComponent<ItemFrame>().SetFrameItem((int)reward[1], (int)reward[2], (int)reward[0]);
            }
        }


        // 보상 수령 버튼 클릭시
        public void OnClickRecieveItemButton()
        {
            List<Asset> itemArr = new List<Asset>();
            foreach (var itemData in currentPlatformData.Reward)
            {
                itemArr.Add(new Asset((eGoodType)(int)itemData[0], (int)itemData[1], (int)itemData[2]));
            }

            if (User.Instance.CheckInventoryGetItem(itemArr))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => { PopupManager.OpenPopup<InventoryPopup>(); }, () => { }, () => { });
                return;
            }

            WWWForm param = new WWWForm();
            param.AddField("plat", currentPlatformData.ID);
            NetworkManager.Send("subway/finish", param, (JObject jsonData) =>
             {
                 if (jsonData["rs"] != null)
                 {
                     if ((int)jsonData["rs"] == (int)eApiResCode.INVENTORY_FULL)
                     {
                         SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                         () => { PopupManager.OpenPopup<InventoryPopup>(); }, () => { }, () => { });
                         return;
                     }
                     if ((int)jsonData["rs"] == (int)eApiResCode.OK && jsonData["rewards"] != null)
                     {
                         UpdateLayerState();

                         SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["rewards"])));

                         // 빨콩 갱신 
                         //PopupManager.ForceUpdate<MainPopup>();
                     }
                 }

                 UITrainStateEvent.Event();
             });

        }

        // 잠긴 슬롯 개방 버튼 클릭시
        public void OnClickUnlockSlotBtn()
        {
            WWWForm param = new WWWForm();
            param.AddField("plat", currentTableData.PLATFORM);
            ePriceDataFlag priceFlag = ePriceDataFlag.CloseBtn | ePriceDataFlag.Gold | ePriceDataFlag.ContentBG | ePriceDataFlag.SubTitleLayer;

            PricePopup.OpenPopup(StringData.GetStringByStrKey("플랫폼추가"), StringData.GetStringFormatByStrKey("플랫폼확장", currentTableData.PLATFORM),
                StringData.GetStringByStrKey("플랫폼추가설명"),
                currentTableData.COST_NUM, priceFlag, () =>
                {
                    NetworkManager.Send("subway/unlock", param, (JObject jsonData) =>
                    {
                        if (jsonData["rs"] != null && (int)jsonData["rs"] == 0)
                        {
                            UpdateLayerState();
                            PopupManager.ClosePopup<PricePopup>();


                            if (currentTableData.PLATFORM == 1)
                            {
                                TutorialCheck();
                            }

                            UITrainStateEvent.Event();
                        }
                    });
                });
        }


        void TutorialCheck()
        {
            if (TutorialManager.tutorialManagement.IsFinishedTutorial(TutorialDefine.Subway)==false)
            {
                TutorialManager.tutorialManagement.StartTutorial((int)TutorialDefine.Subway);
            }
        }

        public void OnClickAccelBtn()
        {
            parentLayer.SetDeliveryButton(false);

            var data = new AccelerationMainData(eAccelerationType.JOB, (int)eLandmarkType.SUBWAY, delieveringTimeSum, currentPlatformData.Expire, () =>
            {
                // PopupManager.ForceUpdate<MainPopup>();
            });
            data.platform = currentPlatformData.ID;
            var accPopup = AccelerationMainPopup.OpenPopup(data);
            accPopup.SetExitCallback(() =>
            {
                Invoke(nameof(BackAccButton), 1.0f);
            });
        }

        void BackAccButton()
        {
            parentLayer.SetDeliveryButton(true);
        }

        public void SetButtonState(bool isOn)
        {
            deliveringAccButton.interactable = isOn;
        }

        public void RefreshItemSlot()
        {
            if (subwayItems == null) return;

            foreach (var item in subwayItems)
            {
                item.RefreshItemAmount();
            }
        }

        void SetBubbleNodeEffect(bool _isNormal)
        {
            if (bubbleAnimObj != null)
            {
                if (_isNormal)
                    bubbleAnimObj.DOPlay();
                else
                    bubbleAnimObj.DOPause();
            }
        }
    }
}