using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 부스터 아이템 슬롯 세팅 부분 - 딱히 몇개 없어보여서 tableview 안쓰고 6개 정도 미리 생성
    /// </summary>
    public class MineBoostInfoSlot : MonoBehaviour
    {
        [SerializeField] ItemFrame itemFrame = null;
        [SerializeField] Text itemNameText = null;
        [SerializeField] Text durationTimeText = null;
        [SerializeField] Text mineBoostValueText = null;
        [SerializeField] Text limitTimeText = null;
        [SerializeField] RectTransform mineBoosterRect = null;// contentsSizeFitter영향주는 노드라 갱신 한번 해주는 용도

        [SerializeField] TimeEnable durationTimeEnable = null; // 유지시간은 제한시간과 동일한 성질 (시간이 0이면 아이템 사용 못함.)
        [SerializeField] TimeEnable limitTimeEnable = null;

        [SerializeField] Button useButton = null;
        public bool IsVisible { get; private set; } = false;
        public MineBoosterItem BoosterItem { get; private set; } = null;

        MineBoosterData itemTableData = null;//일단 키값이 itembaseData의 키와 같음.
        Asset currentItemData = null;
        Action timeFinishCallback = null;

        //서버쪽에서 부스트 전용 아이템 리스트를 줄지, 인벤 데이터에 있는걸 가져다쓰는건지 아직 미정
        public void InitUI(MineBoosterItem _boosterItemData , Action _timeFinishCallback = null)
        {
            if(_boosterItemData == null || _boosterItemData.Amount <= 0)//갯수 0개면 꺼도 될듯.
            {
                SetVisible(false);
                return;
            }

            SetData(_boosterItemData);

            if(currentItemData == null || itemTableData == null)
            {
                SetVisible(false);
                return;
            }

            BoosterItem = _boosterItemData;
            timeFinishCallback = _timeFinishCallback;

            SetVisible(true);
            SetUI();
            RefreshUseBoostButton();
        }

        void SetData(MineBoosterItem _itemData)
        {
            currentItemData = new Asset(eGoodType.ITEM, _itemData.ItemNo, _itemData.Amount);
            itemTableData = MineBoosterData.Get(_itemData.ItemNo);
        }

        public void SetVisible(bool _isVisible)
        {
            gameObject.SetActive(_isVisible);
            IsVisible = _isVisible;
        }

        void SetUI()
        {
            var itemDesingData = itemTableData.GetItemDesignData();
            if (itemNameText != null)
                itemNameText.text = itemDesingData.NAME;//아이템 이름

            if (mineBoostValueText != null)
                mineBoostValueText.text = itemTableData.VALUE_DESC;//부스트 효과량

            if (itemFrame != null)
                itemFrame.SetFrameItem(currentItemData);//아이템프레임

            SetTimeEnable();//시간 세팅

            if (mineBoosterRect != null)
                RefreshContentFitter(mineBoosterRect);

            var backGround = SBFunc.GetChildrensByName(this.gameObject.transform,new string []{"itemInfoNode", "BG"});
            if(backGround != null)
            {
                var targetImage = backGround.GetComponent<Image>();
                if (targetImage != null)
                    targetImage.color = itemTableData.IS_LIMIT_TIME_TYPE ? Color.green : Color.white;
            }
        }

        /// <summary>
        /// 조건 - 기간 무제한 아이템은 시간감소X (유지시간 - 켜고 고정 & 제한 시간 - 끄기)
        /// 유통기한 걸려있는 아이템은 둘다 감소
        /// </summary>
        void SetTimeEnable()
        {
            var isItemTimeLimit = itemTableData.IS_LIMIT_TIME_TYPE;

            if (limitTimeEnable != null)//제한 시간 timeobject
                limitTimeEnable.gameObject.SetActive(isItemTimeLimit);
            if (limitTimeText != null)//제한 시간 표시 라벨
                limitTimeText.gameObject.SetActive(isItemTimeLimit);
            if (isItemTimeLimit)//제한시간 limit 타입 조건이면 refresh 등록
            {
                limitTimeEnable.Refresh = RefreshLimitTime;
            }
            else
                limitTimeEnable.Refresh = null;

            if (durationTimeEnable != null)//유지 시간 refresh 등록
            {
                if (isItemTimeLimit)//유통 기한 걸려있으면 시간 감소 같이 세팅
                {
                    int time = BoosterItem.ExpireTime;
                    var timeInterval = TimeManager.GetTimeCompare(time);
                    if (timeInterval > 0)
                    {
                        durationTimeEnable.Refresh = RefreshRemainTime;//일단 만료 시간이 미래면 refresh 돌림
                    }
                    else
                    {
                        durationTimeEnable.Refresh = null;
                        MiningPopupEvent.RequestUseBoosterItem(BoosterItem, null);//유지 시간이 끝나면 클라쪽에서 선처리
                    }
                }
                else
                    durationTimeText.text = SBFunc.TimeString(itemTableData.BOOST_TIME);//기간제 아닌 아이템은 유지시간 고정
            }
        }

        private void RefreshRemainTime()
        {
            if (durationTimeText == null)
                return;

            int time = BoosterItem.ExpireTime;
            var boostTime = BoosterItem.BoostTableData.BOOST_TIME;
            var timeSpan = TimeManager.GetTimeCompareFromNow(time);//현재시간에서 만료시간을 뺌
            if (boostTime < timeSpan)//사이시간에 대한 처리 -> 실제 만료 기간보다 내가 일찍 수령했을 경우
            {
                durationTimeText.text = TimeText(boostTime);
                return;
            }
            
            var timeInterval = TimeManager.GetTimeCompare(time);
            if (timeInterval > 0)
                durationTimeText.text = TimeText(timeInterval);
            else
            {
                durationTimeText.text = TimeText(0);
                if (timeFinishCallback != null)
                    timeFinishCallback();

                MiningPopupEvent.RequestUseBoosterItem(BoosterItem, null);//유지 시간이 끝나면 클라쪽에서 선처리
            }
        }
        private void RefreshLimitTime()
        {
            if (limitTimeText == null)
                return;

            int time = BoosterItem.ExpireTime;//expire_time 가져올곳?
            var timeInterval = TimeManager.GetTimeCompare(time);
            if (timeInterval > 0)
            {
                var resultString = StringData.GetStringFormatByStrKey("남음", SBFunc.TimeCustomString(timeInterval, 1, true));
                limitTimeText.text = resultString;
            }
            else
            {
                limitTimeText.text = TimeText(0);

                //현재 시스템은 유지기간과 제한시간이 같아서 RefreshRemainTime 쪽에서만 시간 처리 해주면 되서, 제거함.
                //다른 케이스 (유지기간과 제한시간이 다른 경우)에는 콜백을 찢어야함.
                //if (timeFinishCallback != null)
                //    timeFinishCallback();
                //MiningPopupEvent.RequestUseBoosterItem(BoosterItem, null);
            }
        }

        private string TimeText(int time)
        {
            return SBFunc.TimeString(time);
        }

        /// <summary>
        /// 부스터 아이템 사용 -> MineBoostInfoLayerController에 이벤트 및 아이템 사용 로직 추가 
        /// 아이템 사용 팝업 호출 추가
        /// </summary>
        public void OnClickUseBoosterItem()
        {
            if (BoosterItem == null)
                return;

            if(MiningManager.Instance.GetProductData() == null)
            {
                ToastManager.On(StringData.GetStringByStrKey("광산토스트7"));
                return;
            }


            //아이템 사용 팝업 제어 조건
            if(!IsVisible || BoosterItem.Amount <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("광산토스트1"));
                return;
            }

            PopupManager.OpenPopup<MiningUseBoosterItemPopup>(new MineBoostItemUsePopupData(BoosterItem));
        }

        void RefreshUseBoostButton()
        {
            if (useButton != null)
                useButton.SetButtonSpriteState(MiningManager.Instance.GetProductData() != null);
        }

        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
            {
                return;
            }

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}

