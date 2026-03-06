using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleDragonPartChangeEaterPanel : MonoBehaviour
    {
        [SerializeField]
        SubLayer dragonEquipLayer = null;

        [SerializeField]
        GameObject partSlotNode = null;//버튼 애니메이션

        [SerializeField]
        ChampionBattlePartIconPanel partIconPanel = null;

        int partTag = -1;
        int tempdragonTag = -1;
        bool isPartChangeState = false;//교체 UI 켜짐 상태 체크
        

        public void Start()
        {
            // [3]
        }

        void Init()
        {
            InitCurrentDragonData();
        }

        void InitCurrentDragonData()
        {
            if (PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().DragonTag;
                var userDragonInfo = PopupManager.GetPopup<ChampionBattleDragonSelectPopup>().Dragon;
                if (userDragonInfo == null)
                {
                    Debug.Log("user Dragon is null");
                    return;
                }

                tempdragonTag = dragonTag;
            }
        }

        //파츠 교체시 표시 UI
        public void OnShowPartSlotEaterNode(int totalSlotCount, int currentTag)
        {
            Init();

            if (currentTag <= 0)
            {
                return;
            }

            if (partSlotNode == null)
            {
                return;
            }

            partTag = currentTag;
            isPartChangeState = true;//교체 UI 켜짐 상태 체크

            if (gameObject != null && gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }
            partIconPanel.ShowAnimationSlot(totalSlotCount);
            partIconPanel.IsPartChangeState = true;
        }

        public void OnHidePartSlotEaterNode()
        {
            Init();

            if (partSlotNode == null)
            {
                return;
            }

            partIconPanel.HideAllAnimationSlot();
            partIconPanel.IsPartChangeState = false;


            if (gameObject != null && gameObject.activeInHierarchy == true)
            {
                gameObject.SetActive(false);
            }

            isPartChangeState = false;
        }

        public void ShowTradePopup(int param)
        {
            //교체 없음
            Debug.LogError("교체?");

            //if (isPartChangeState)//교체 요청 상태일 때는 교체 팝업 요청
            //{
            //    var partLink = User.Instance.PartData.GetPartLink(partTag);

            //    //현재 세팅된 드래곤 태그 값으로 파츠 슬롯 넘버 가져오기
            //    var partList = User.Instance.DragonData.GetDragon(tempdragonTag).GetPartsList();
            //    if (partList == null || partList.Length <= 0)
            //    {
            //        return;
            //    }

            //    var clickSlot = -1;
            //    UserPart userPart = null;

            //    if (partList != null || partList.Length > 0)
            //    {
            //        for (var i = 0; i < partList.Length; i++)
            //        {
            //            var data = partList[i];
            //            if (data == null)
            //            {
            //                continue;
            //            }

            //            var tag = data.Tag;
            //            if (tag == param)
            //            {
            //                clickSlot = i;
            //                userPart = data;
            //            }
            //        }
            //    }

            //    if (clickSlot < 0)
            //    {
            //        return;
            //    }

            //    if (partLink <= 0)//다른 드래곤 착용상태 아님
            //    {
            //        ShowTradePartPopupAddCost(userPart, clickSlot);
            //    }
            //    else//코스트 팝업 출력
            //    {
            //        var addCost = 0;
            //        var dragonData = User.Instance.DragonData.GetDragon(tempdragonTag);
            //        if (dragonData != null)
            //        {
            //            var currentDragonPartTag = dragonData.Parts[clickSlot];
            //            if (currentDragonPartTag > 0)
            //            {
            //                var currentPartData = User.Instance.PartData.GetPart(currentDragonPartTag);
            //                if (currentPartData != null)
            //                {
            //                    var currentDesignData = currentPartData.GetPartDesignData();
            //                    addCost = currentDesignData.UNEQUIP_COST_NUM;
            //                }
            //            }
            //        }

            //        var partData = User.Instance.PartData.GetPart(partTag);
            //        if (partData == null)
            //        {
            //            return;
            //        }

            //        var partDesignData = partData.GetPartDesignData();
            //        if (partDesignData == null)
            //        {
            //            return;
            //        }

            //        var cost = partDesignData.UNEQUIP_COST_NUM + addCost;

            //        PricePopup.OpenPopup(StringData.GetStringByIndex(100000248), "", StringData.GetStringByIndex(100000363), cost, ePriceDataFlag.ContentBG | ePriceDataFlag.CancelBtn | ePriceDataFlag.Gold, () =>
            //        {
            //            ChangeEquipProcess(tempdragonTag, partTag, cost, clickSlot, PopupManager.GetPopup<PricePopup>());
            //        });
            //    }
            //}
        }

        void ShowTradePartPopupAddCost(UserPart userpart, int slotIndex)
        {
            if (userpart == null)
            {
                return;
            }

            //var cost = userpart.GetPartDesignData().UNEQUIP_COST_NUM;

            //PricePopup.OpenPopup(StringData.GetStringByIndex(100000248), "", StringData.GetStringByIndex(100002045), cost, ePriceDataFlag.ContentBG | ePriceDataFlag.CancelBtn | ePriceDataFlag.Gold, () =>
            //{
            //    PressOKProcess(cost, slotIndex, PopupManager.GetPopup<PricePopup>());

            //});
        }

        void PressOKProcess(int cost, int slotIndex, PricePopup popup)
        {
            var param = new WWWForm();
            param.AddField("did", tempdragonTag);//드래곤 id
            param.AddField("tag", partTag);//부속품 tag
            param.AddField("slot", slotIndex);//장착 슬롯 인덱스

            Debug.Log("equip adequate Slot index : " + slotIndex);

            //장착 msg -> item_update 및 dragon_exp_update 날라옴
            //NetworkManager.Send("dragon/equippart", param, (jsonData) =>
            //{
            //    if (jsonData.ContainsKey("rs") && (eApiResCode)(jsonData["rs"].Value<int>()) == eApiResCode.OK)
            //    {
            //        var unEquippedTag = jsonData["unequipped"].Value<int>();//장비 교체 이전 태그
            //        if (unEquippedTag > 0) 
            //        {
            //            var partData = User.Instance.PartData.GetPart(unEquippedTag);
            //            if (partData != null)
            //                partData.SetLink(-1);
            //        }

            //        if (dragonEquipLayer != null)
            //        {
            //            var equipLayer = dragonEquipLayer.GetComponent<SubLayer>();
            //            if (equipLayer != null)
            //            {
            //                OnHidePartSlotEaterNode();
            //                equipLayer.ForceUpdate();

            //                DragonPartEvent.PlayEquipPartAnim(partTag);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000621), StringData.GetStringByIndex(100000614));
            //    }
            //});

            popup.OnClickClose();
        }

        void ChangeEquipProcess(int dragonTag, int partTag, int cost, int slotIndex, PricePopup popup)
        {
            //var currentUserGold = User.Instance.GOLD;
            //if (currentUserGold < cost)
            //{
            //    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000620));
            //    popup.OnClickClose();
            //    return;
            //}
            //var param = new WWWForm();
            //param.AddField("did", dragonTag);//드래곤 id
            //param.AddField("tag", partTag);//부속품 tag
            //param.AddField("slot", slotIndex);//장착 슬롯 인덱스

            ////이전 드래곤 귀속 상태 끊어야함
            ////NetworkManager.Send("dragon/equippart", param, (jsonData) =>
            ////{
            ////    if (jsonData.ContainsKey("rs") && (eApiResCode)(jsonData["rs"].Value<int>()) == eApiResCode.OK)
            ////    {
            ////        var unEquippedTag = jsonData["unequipped"].Value<int>();//장비 교체 이전 태그
            ////        if (unEquippedTag > 0)
            ////        {
            ////            var partData = User.Instance.PartData.GetPart(unEquippedTag);
            ////            if (partData != null)
            ////                partData.SetLink(-1);
            ////        }
            ////        PopupManager.ForceUpdate<DragonManagePopup>();

            ////        DragonPartEvent.PlayEquipPartAnim(partTag);
            ////    }
            ////    else
            ////    {
            ////        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000621));
            ////    }
            ////    popup.OnClickClose();
            ////});
        }
    }
}
