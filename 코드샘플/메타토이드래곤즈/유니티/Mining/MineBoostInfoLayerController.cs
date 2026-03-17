using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 광산 UI 오른쪽 데이터 및 UI 컨트롤러, 
    /// 부스터 아이템 세팅 및 사용시 refresh 처리 부분 이벤트
    /// </summary>
    public class MineBoostInfoLayerController : MonoBehaviour, EventListener<MiningPopupEvent>
    {
        [SerializeField] List<MineBoostInfoSlot> mineBoostInfoSlotList = new List<MineBoostInfoSlot>();//기본 6개 정도 들고있고, 수량 증감에 따른 visible세팅은 알아서.

        [SerializeField] Text emptyItemText = null;

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        /// <summary>
        /// 부스터 아이템 사용 요청 시 data 아이템 데이터 갱신용 이벤트 추가 - mineBooster item update가 push 나 rs 로 올거 대응용도.
        /// 상태 정리
        /// 1. eventType.targetItem 이 null 이 아닌 경우에는 클라쪽에서 차감 처리 하려고 하는 목적. (현재 걸리는 케이스로는 기간제 아이템 밖에 없음)
        /// 2. 기간 무제한 아이템은 아이템 갯수에 영향을 받는 것들이 아니다보니, eventType.targetItem == null 조건이 절대 올 수 없긴한데, 확인해봐야함.
        /// 3. 부스터 아이템 사용시 push 데이터를 통해 들어오는 아이템 (변경점만 올 지, 전체로 올지는 확인해봐야함.) 갱신.
        /// </summary>
        /// <param name="eventType"></param>
        public void OnEvent(MiningPopupEvent eventType)
        {
            switch (eventType.Event)
            {
                case MiningPopupEvent.MiningPopupEventEnum.USE_BOOSTER_ITEM:
                    var data = eventType.responseData;
                    var targetItem = eventType.targetItem;

                    if (data == null && targetItem == null)
                        return;

                    if(targetItem != null)//클라 요청 처리
                    {
                        var itemData = MiningManager.Instance.GetItem(targetItem.ItemNo);

                        MineBoosterItem tempItem = new MineBoosterItem(itemData.ItemNo, itemData.Amount - 1, itemData.ExpireTime);//클라쪽에서 차감처리
                        RefreshItemSlot(tempItem);//해당 아이템 세팅 해서 넘기기
                        return;
                    }

                    if (data != null)//서버쪽 요청으로 인한 처리 - mine_item_update로 인한 아이템 사용 업데이트가 됐다고 가정.
                    {
                        var itemNo = 0;
                        if (data.ContainsKey("item_no"))
                            itemNo = data["item_no"].Value<int>();

                        if (itemNo > 0)
                        {
                            var itemData = MiningManager.Instance.GetItem(itemNo);
                            if(itemData != null)
                                RefreshItemSlot(itemData);//해당 아이템 세팅 해서 넘기기
                            else
                            {
                                var currentBoostItem = MiningManager.Instance.GetAllBoosterItem();
                                if (currentBoostItem.Count <= 0)//현재 인벤토리에 아무것도 없음.
                                    SetSlotUI();
                            }
                        }
                    }
                    break;
            }
        }
        public void InitController()
        {
            SetSlotUI();
        }

        /// <summary>
        /// 부스터 슬롯 아이템 세팅
        /// </summary>
        void SetSlotUI()
        {
            if (mineBoostInfoSlotList == null || mineBoostInfoSlotList.Count <= 0)
                return;

            var boosterItemList = MiningManager.Instance.GetAllBoosterItem();//현재 아이템 갯수 가져오기
            var sortResultList = SortData(boosterItemList);//정렬 한번 돌림.
            var boosterItemCount = sortResultList.Count;
            var currentSlotCount = mineBoostInfoSlotList.Count;

            if (boosterItemCount > currentSlotCount)//보여줄 아이템 숫자가 많으면 미리 생성
            {
                var instanceTarget = mineBoostInfoSlotList[0];
                var parent = instanceTarget.transform.parent;
                var remainCount = boosterItemCount - currentSlotCount;

                for (int i = 0; i < remainCount; i++)
                {
                    var instanceObj = Instantiate(instanceTarget, parent);
                    mineBoostInfoSlotList.Add(instanceObj);
                }
            }
            
            for (int i = 0; i < mineBoostInfoSlotList.Count; i++)
            {
                if (boosterItemCount <= i)
                    mineBoostInfoSlotList[i].SetVisible(false);
                else
                {
                    mineBoostInfoSlotList[i].InitUI(sortResultList[i]);
                }
            }

            RefreshEmptyLabel();
        }

        /// <summary>
        /// KIND == 19 (부스터 아이템 타입 일때) // SORT 써서 오름차순 하자고 논의 완료.
        /// </summary>
        /// <param name="_originList"></param>
        /// <returns></returns>
        List<MineBoosterItem> SortData(List<MineBoosterItem> _originList)
        {
            List<MineBoosterItem> tempList = new List<MineBoosterItem>();
            if (_originList == null || _originList.Count <= 0)
                return tempList;

            return _originList.OrderBy((a)=>a.BaseData.SORT).ToList();
        }

        /// <summary>
        /// 변경점 아이템 데이터 UI 갱신
        /// </summary>
        /// <param name="_item"></param>
        void RefreshItemSlot(MineBoosterItem _item)
        {
            if (mineBoostInfoSlotList == null || mineBoostInfoSlotList.Count <= 0)
                return;

            foreach(var slotData in mineBoostInfoSlotList)
            {
                var isVisible = slotData.IsVisible;
                var itemData = slotData.BoosterItem;

                if (!isVisible || itemData == null)//꺼져있거나 빈슬롯이면
                    continue;

                if(itemData.ItemNo == _item.ItemNo)
                {
                    slotData.InitUI(_item);
                    break;
                }
            }

            RefreshEmptyLabel();
        }

        void RefreshEmptyLabel()
        {
            if (mineBoostInfoSlotList == null || mineBoostInfoSlotList.Count <= 0)
                return;

            if (emptyItemText == null)
                return;

            int checkVisibleCount = 0;
            foreach (var slotData in mineBoostInfoSlotList)
            {
                var isVisible = slotData.IsVisible;
                var itemData = slotData.BoosterItem;

                if (!isVisible || itemData == null)//꺼져있거나 빈슬롯이면
                    continue;

                checkVisibleCount++;
            }

            emptyItemText.gameObject.SetActive(checkVisibleCount == 0);
        }
    }
}
