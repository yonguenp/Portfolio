using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 내가 보유한 선물 상자 레이어 -> 상자 뽑기의 연장선
    /// </summary>
    public class GiftBoxMineSubLayer : SubLayer , EventListener<DiceUIEvent>
    {
        [SerializeField] List<GiftBoxContentSlot> giftBoxList = new List<GiftBoxContentSlot>();

        [SerializeField] Text lottoWinnerText = null;

        EventDiceBaseData holidayData = null;

        bool isBoxOpen = false;

        public override void ForceUpdate() { }
        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력
        public override void Init() 
        {
            if (giftBoxList == null || giftBoxList.Count <= 0)
                return;

            if (holidayData == null)
                holidayData = DiceEventPopup.GetHolidayData();

            isBoxOpen = false;

            RefreshBox();
        }

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }

        public void RefreshBox()
        {
            if (holidayData == null)
                return;

            for(int i = 0; i < giftBoxList.Count; i++)
            {
                var item = GetItem(i);
                if (item == null)
                    continue;

                RefreshBoxSlot(i, item);
            }

            DiceUIEvent.RefreshTabReddot();//변경된 상자 기준 레드닷 갱신

            RefreshWinnerText();//당첨자 갱신
        }

        public void RefreshBoxSlot(int _slotIndex, InventoryItem _item)
        {
            if (_slotIndex < 0 || _slotIndex >= giftBoxList.Count)
                return;

            giftBoxList[_slotIndex].SetData(_slotIndex, GetGachaTypeKeyByIndex(_slotIndex) , _item);
        }

        InventoryItem GetItem(int _index)
        {
            return holidayData?.GetBoxItemByIndex(_index + 1);
        }

        int GetGachaTypeKeyByIndex(int _index)
        {
            if (holidayData == null)
                return -1;
            else
                return holidayData.GetGachaTypeKeyByIndex(_index + 1);
        }

        public void OnClickBuyGift(int _index)//상자 구매하기 - 상자 탭으로 이동
        {
            if (holidayData == null)
                return;

            bool isPeriod = holidayData.IsEventPeriod(false);
            if(!isPeriod)
            {
                ToastManager.On(StringData.GetStringByStrKey("이벤트종료안내"));
                return;
            }

            DiceEventPopup.MoveTabForce(new TabTypePopupData(1, 1));
        }

        public void OnClickOpenGiftBox(int _index)
        {
            OpenBoxApi(_index, 1);
        }

        public void OnClickOpenGiftBox(int _index, int count)//상자 오픈하기
        {
            if (holidayData == null)
            {
                Debug.LogError("holidayData is null");
                return;
            }

            if (_index < 0 || _index >= giftBoxList.Count)
            {
                //ToastManager.On("유효하지않은 선물상자");
                return;
            }

            var data = giftBoxList[_index].Item;
            if(data.Amount <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("보유상자없음"));
                return;
            }

            if (isBoxOpen)
                return;

            isBoxOpen = true;
            OpenBoxApi(_index, count);
        }
        void OpenBoxApi(int _index, int count)
        {
            switch(_index)
            {
                case 0:
                    holidayData.OpenBox1((jsonData)=> {
                        SuccessOpenApiProcess(_index, jsonData);
                    },(failString)=> {
                        isBoxOpen = false;
                    }, giftBoxList[_index].GetAvailableOpenCount(count));
                    break;
                case 1:
                    holidayData.OpenBox2((jsonData) => {
                        SuccessOpenApiProcess(_index, jsonData);
                    }, (failString) => {
                        isBoxOpen = false;
                    }, giftBoxList[_index].GetAvailableOpenCount(count));
                    break;
                case 2:
                    holidayData.OpenBox3((jsonData) => {
                        SuccessOpenApiProcess(_index, jsonData);
                    }, (failString) => {
                        isBoxOpen = false;
                    }, giftBoxList[_index].GetAvailableOpenCount(count));
                    break;
            }
        }

        void SuccessOpenApiProcess(int _slotIndex , JObject _jsonData)
        {
            RefreshBox();//백판 갱신 - 갱신된 itemData 받아옴.

            if (_slotIndex < 0 && _slotIndex >= giftBoxList.Count)
            {
                isBoxOpen = false;
                return;
            }

            var item = giftBoxList[_slotIndex].Item;
            List<Asset> rewardList = new List<Asset>();

            if (_jsonData.ContainsKey("reward"))//보상
                rewardList = SBFunc.ConvertSystemRewardDataList((JArray)_jsonData["reward"]);

            var isAllRewardMailSended = holidayData.IsMailSended(_jsonData) && rewardList.Count <= 0;//총보상이 우편으로 전부 간 경우
            if(isAllRewardMailSended)
            {
                var boxOpenPopup = PopupManager.GetPopup<DiceEventGiftOpenPopup>();
                var isOpenBoxPopup = PopupManager.IsPopupOpening(boxOpenPopup);
                if (isOpenBoxPopup)//열려있다.
                    boxOpenPopup.ClosePopup();

                isBoxOpen = false;
                return;
            }

            if (rewardList != null && rewardList.Count > 0)
            {
                var boxOpenPopup = PopupManager.GetPopup<DiceEventGiftOpenPopup>();
                var isOpenBoxPopup = PopupManager.IsPopupOpening(boxOpenPopup);
                if (isOpenBoxPopup)//열려있다.
                    boxOpenPopup.ForceUpdate(new GiftBoxOpenPopupData(_slotIndex, item, rewardList));
                else
                    DiceEventGiftOpenPopup.OpenPopup(_slotIndex, item, rewardList);

                isBoxOpen = false;
            }
        }

        public void OnEvent(DiceUIEvent eventType)
        {
            switch(eventType.type)
            {
                case DiceUIEvent.eDiceUI.OPEN_BOX:
                    var slotIndex = eventType.boxSlotIndex;                    
                    OnClickOpenGiftBox(slotIndex, eventType.boxCount);
                    break;
            }
        }

        void RefreshWinnerText()
        {
            if(holidayData == null)
            {
                lottoWinnerText.text = StringData.GetStringByStrKey("이벤트종료안내2");
                return;
            }

            if (lottoWinnerText != null)
            {
                bool isPeriod = holidayData.IsEventPeriod(false);
                if(!isPeriod)
                {
                    lottoWinnerText.text = StringData.GetStringByStrKey("이벤트종료안내2");
                    return;
                }

                var winnerInfo = holidayData.LottoWinnerInfo;
                var nick = winnerInfo.nick;
                var timeStamp = winnerInfo.update_time;//다음날로 넘어가는 케이스 비교처리해야함 - 이월이 될 경우 stringKey값을 바꿔서 해달라고함.

                bool isTodayWinner = false;
                if (timeStamp > 0)
                {
                    var winnerTime = TimeManager.GetCustomDateTime(timeStamp);//로또 맞은 시각
                    var curTime = TimeManager.GetDateTime();//지금시각

                    if(winnerTime.Day == curTime.Day)//오늘 당첨자인가?
                        isTodayWinner = true;
                }
                string resultKey;
                if (!string.IsNullOrEmpty(nick))//당첨자가 존재함
                {
                    if (isTodayWinner)//현재
                        resultKey = StringData.GetStringFormatByStrKey("2023_EVENT_MENU2_1_LOTTO_WINNER", nick);
                    else//과거 - 이월 횟수가 붙음
                    {
                        var winnerTime = TimeManager.GetCustomDateTime(timeStamp);//과거에 로또 맞은 시각
                        resultKey = GetCarryOverCountString(winnerTime, true);
                    }
                }
                else//당첨자 없음 - 이벤트 시작일 기준으로 이월 횟수 계산
                {
                    var eventScheduleData = holidayData.GetScheduleData();
                    var eventStartDate = eventScheduleData.START_TIME;//이벤트 시작 시간 - 시작 날에 대한 횟수 체크를 위해서 정각으로 만듦
                    resultKey = GetCarryOverCountString(eventStartDate, false);
                }

                lottoWinnerText.text = resultKey;
            }
        }
        /// <summary>
        /// 이월 카운트 + 결과 스트링 연산
        /// </summary>
        /// <returns></returns>
        string GetCarryOverCountString(DateTime _targetTime, bool _isWinner)
        {
            var targetHour = _targetTime.Hour;
            var targetMin = _targetTime.Minute;
            var targetSec = _targetTime.Second;
            var modifyTime = _targetTime.AddHours(-1 * targetHour).AddMinutes(-1 * targetMin).AddSeconds(-1 * targetSec);//타겟의 정각 시각

            var curTime = TimeManager.GetDateTime();//현재 시각
            var modifyCurTime = TimeManager.GetDateTime(0,curTime.Hour * -1 , curTime.Minute * -1 , curTime.Second * -1);//오늘은 이월 횟수에 포함 안되기 때문에, 지금 시각을 정각으로 만듦.
            var span = modifyCurTime.Subtract(modifyTime);//시간 차이
            var spanTotalSec = (int)span.TotalSeconds;

            int spanDayCount = spanTotalSec / SBDefine.Day;//이월된 횟수

            if (_isWinner)//로또 당첨자가 나온 날을 기준으로 1일을 빼야함 (ex. 당첨자가 나온날 기준 다음날은 이월 횟수 0)
                spanDayCount -= 1;

            //0보다 작다 (이월 된 '날' 이 24시간 이하면 하루가 안지남.
            return spanDayCount <= 0 ? StringData.GetStringByStrKey("2023_EVENT_MENU2_1_LOTTO") : StringData.GetStringFormatByStrKey("2023_EVENT_MENU2_1_LOTTO_DELAY", spanDayCount);
        }
    }
}


