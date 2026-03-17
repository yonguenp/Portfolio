using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

namespace SandboxNetwork {   
    public class PetDecomposeLayer : SubLayer, EventListener<PetDataEvent>
    {
        const int PET_DECOMPOSE_RESULT_ITEM_KEY = 140000004;

        const int PetMaxDecomposeNum = 99;

        [SerializeField]
        PetTabLayer petTabLayer = null;

        


        [Space(10)]
        [Header("pet List scroll Info")]
        [SerializeField]
        PetListPanel petList = null;

        [Space(10)]
        [Header("material pet Info")]
        [SerializeField]
        PetPortraitFrame petFrame = null;
        [SerializeField]
        Text petNameText =  null;
        [SerializeField]
        GameObject[] petRank = null;
        //[SerializeField]
        //GameObject arrowNode = null;

        [Space(10)]
        [Header("decompose Info")]
        [SerializeField]
        Button decomposeButton = null;
        [SerializeField]
        ItemFrame resultItem = null;//분해 시 획득 가능 강화 재료

        [SerializeField]
        Button backBtn = null;

        [SerializeField]
        GameObject alertObj = null;

        int currentDecomposeListIndex = 0;
        List<int> currentDecomposeList = new List<int>();
        List<Asset> decomposedItemList = new List<Asset>(); // 분해 이후 얻을 아이템
        void OnEnable()
        {
            EventManager.AddListener<PetDataEvent>(this);
        }

        void OnDisable()
        {
            EventManager.RemoveListener<PetDataEvent>(this);
        }
        public override void Init()
        {
            SetPetSubList();
            HideWillDecomposePet();
            decomposedItemList.Clear();
            alertObj.SetActive(true);
            InitResultItemSlot();
            RefreshDecomposeButton();
        }
        void SetPetSubList()
        {
            if (currentDecomposeList == null)
            {
                currentDecomposeList = new List<int>();
            }
            currentDecomposeList.Clear();//펫 재료 리스트 초기화
            currentDecomposeListIndex = 0;
            if (petList != null)
            {
                petList.ClickRegistCallback = RegistMaterialPetTag;
                petList.ClickReleaseCallback = ReleaseMaterialPetTag;
                petList.ClickAutoRegistCallback = AutoRegistMaterialList;
                petList.ClickReleaseAllCallback = ReleaseAllMaterialPet;
                petList.Init(ePetPopupState.Decompose);
            }
        }

        void RegistMaterialPetTag(string param)//리스트에서 재료 넣을 때
        {
            var tag = int.Parse(param);
            if (tag <= 0)
            {
                return;
            }

            if (currentDecomposeList.Count>= PetMaxDecomposeNum) // 제한 수량
            {
                ToastManager.On(100000210);
                return;
            }
            else
            {
                petList.SetDetailButtonState(true);
                PushMaterialList(tag);
            }

            RefreshDecomposeButton();
        }

        void ReleaseMaterialPetTag(string param)//리스트에서 재료 뺄 때
        {
            PopMaterialList(int.Parse(param));

            RefreshDecomposeButton();
        }

        void ReleaseAllMaterialPet()
        {
            currentDecomposeList.Clear();
            HideWillDecomposePet();
            RefreshDecomposeItemList();
            RefreshDecomposeButton();
        }

        void PushMaterialList(int tag)
        {
            if(currentDecomposeList != null && currentDecomposeList.Contains(tag)==false)
            {
                if(currentDecomposeList.Count< PetMaxDecomposeNum)
                {
                    currentDecomposeList.Add(tag);
                    //arrowNode?.SetActive(currentDecomposeList.Count > 1);// 중간 펫 UI On
                    ShowWillDecomposePet(currentDecomposeList.Count - 1);
                    RefreshDecomposeItemList();
                }
            }
        }

        void PopMaterialList(int tag)
        {
            if (currentDecomposeList != null && currentDecomposeList.Contains(tag))
            {
                var index = currentDecomposeList.IndexOf(tag);
                currentDecomposeList.Remove(tag);
                if (currentDecomposeList.Count == 0)
                {
                    HideWillDecomposePet();// 중간 펫 UI Off
                }
                else if (index <= currentDecomposeListIndex)
                {
                    //arrowNode?.SetActive(currentDecomposeList.Count > 1);
                    ShowWillDecomposePet(currentDecomposeListIndex - 1);
                }
                
                RefreshDecomposeItemList();
            }
        }

        void AutoRegistMaterialList(List<int> materialList)//자동 등록 프로세스
        {
            foreach (var tag in materialList)
            {
                if(currentDecomposeList.Count >= PetMaxDecomposeNum)
                {
                    break;
                }
                if (currentDecomposeList.Contains(tag))
                {
                    continue;
                }
                PushMaterialList(tag);
            }

            RefreshDecomposeButton();
        }

        void RefreshDecomposeItemList()
        {
            Dictionary<int, int> tempItemCount = new Dictionary<int, int>();
            tempItemCount.Clear();
            decomposedItemList.Clear();
            for (var i = 0; i < currentDecomposeList.Count; i++)
            {
                var tag = currentDecomposeList[i];
                if (tag < 0)
                {
                    continue;
                }
                var petData = User.Instance.PetData.GetPet(tag);
                var petGrade = petData.Grade();
                var petReinforce = petData.Reinforce;
                var decomposeData = PetDecomposeData.GetResultItemData(petGrade, petReinforce);
                if (decomposeData == null)
                {
                    continue;
                }
                var item = decomposeData.ItemNo;
                var itemCount = decomposeData.Amount;

                // 추후 분해를 통해 얻을 수 있는 아이템 종류가 많아 질 걸 대비해ㅏㄴ 코드
                if (tempItemCount.ContainsKey(item))
                {
                    var reduceValue = tempItemCount[item];
                    tempItemCount[item] = reduceValue + itemCount;
                }
                else
                {
                    tempItemCount.Add(item, itemCount);
                }

                decomposedItemList.Add(new Asset(eGoodType.ITEM, item, itemCount));
            }

            //현재는 분해를 통해 얻을 수 있는 아이템 종류가 1종류 밖에 없어서 놓은 임시 코드
            var itemKey = tempItemCount.Keys.FirstOrDefault();
            if(itemKey == default)
                InitResultItemSlot();
            else
            {
                resultItem.gameObject.SetActive(true);
                resultItem.SetFrameItem(itemKey, tempItemCount[itemKey]);
                resultItem.SetItemBgOff();
            }
        }

        void InitResultItemSlot()
        {
            resultItem.SetFrameItemInfo(PET_DECOMPOSE_RESULT_ITEM_KEY, 0, -1);
        }
        
        public void RefreshDecomposeButton()
        {
            if (decomposeButton != null)
                decomposeButton.SetButtonSpriteState(currentDecomposeList.Count > 0);
        }

        void ShowWillDecomposePet(int index)
        {
            if(currentDecomposeList == null || currentDecomposeList.Count <= index)
            {
                return;
            }
            alertObj.SetActive(false);
            currentDecomposeListIndex = (index < 0)? 0: index;
            petFrame.gameObject.SetActive(true);
            var petInfo = User.Instance.PetData.GetPet(currentDecomposeList[currentDecomposeListIndex]);
            petFrame.SetPetPortraitFrame(petInfo);
            petNameText.text = petInfo.Name();
            HideAllPetRank();
            petRank[petInfo.Grade()-1].SetActive(true);
        }

        void HideWillDecomposePet()
        {
            alertObj.SetActive(true);
            //arrowNode?.SetActive(false);
            petNameText.text = StringData.GetStringByIndex(100000344);
            HideAllPetRank();
            petFrame.gameObject.SetActive(false);
        }

        void HideAllPetRank()
        {
            foreach(var item in petRank)
            {
                item.gameObject.SetActive(false);
            }
        }

        public void OnClickRightArrow()
        {
            if(currentDecomposeList.Count <= currentDecomposeListIndex + 1)
            {
                ShowWillDecomposePet(0);
            }
            else
            {
                ShowWillDecomposePet(currentDecomposeListIndex+1);
            }
        }

        public void OnClickLeftArrow()
        {
            if (0 > currentDecomposeListIndex - 1)
            {
                ShowWillDecomposePet(currentDecomposeList.Count -1);
            }
            else
            {
                ShowWillDecomposePet(currentDecomposeListIndex - 1);
            }

        }

        public void OnClickDecompose()//분해 요청
        {
            if(currentDecomposeList.Count <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("펫선택"));
                return;
            }

            if (User.Instance.CheckInventoryGetItem(decomposedItemList)) // 가방 체크
            {
                InventoryFullAlert();
                return;
            }
            SendDecomposeMessage();
        }

        void SendDecomposeMessage()
        {
            var param = new WWWForm();//{ materials: materialList};
            param.AddField("materials", JsonConvert.SerializeObject(currentDecomposeList.ToArray()));   
            NetworkManager.Send("pet/decompose", param, (jsonObj) =>  
            {
                var data = jsonObj;
                var isSuccess = (data["err"].Value<int>() == 0);
                var rs = (eApiResCode)data["rs"].Value<int>();
                switch (rs)
                {
                    case eApiResCode.OK:
                    {
                        if (isSuccess)//결과 확인 팝업 출력
                        {

                            if (currentDecomposeList.Contains(PopupManager.GetPopup<DragonManagePopup>().CurPetTag)) //선택된 펫이 갈리는 경우
                            {
                                PopupManager.GetPopup<DragonManagePopup>().CurPetTag = 0;
                            }
                            //보상 아이템 통째로 넘겨서 팝업 생성
                            var rewardItemList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["reward"]));
                            PopupManager.OpenPopup<RewardResultPopup>(new RewardPopupData(rewardItemList));
                            petList.Init(ePetPopupState.Decompose);
                            currentDecomposeList.Clear();//펫 재료 리스트 초기화
                            currentDecomposeListIndex = 0;
                            InitResultItemSlot();
                            HideWillDecomposePet();
                            RefreshDecomposeButton();
                        }
                    }
                    break;
                    case eApiResCode.PART_INVALID_TAG_TO_DECOMPOUND:  // 바꿔야 됨
                    {
                        Init();
                        ToastManager.On(100002550);
                    }
                    break;
                    case eApiResCode.INVENTORY_FULL:
                    {
                        InventoryFullAlert();
                    }
                    break;
                }
            });
        }
        void InventoryFullAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                () => {
                    PopupManager.OpenPopup<InventoryPopup>();
                },
                () => {   //나가기
                },
                () => {  //나가기
                }
            );
        }

        public void OnClickChangeLayer()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonManagePopup = PopupManager.GetPopup<DragonManagePopup>();
                if (dragonManagePopup != null)
                {
                    dragonManagePopup.moveTab(new DragonTabTypePopupData(0, 1));
                }
            }
            else
            {
                petTabLayer.moveLayer(4);
            }
        }
        public void OnClickBackButton()
        {
            petTabLayer.onClickChangeLayer("1");
        }
        public override bool backBtnCall()
        {
            if (backBtn != null)
            {
                backBtn.onClick.Invoke();
                return true;
            }
            return false;
        }

        public void OnEvent(PetDataEvent eventData)
        {
            switch (eventData.type)
            {
                case PetDataEvent.PetEvent.FOCUS_FRAME:
                    var index = currentDecomposeList.IndexOf(eventData.target_tag);
                    if (index >= 0 && currentDecomposeList.Count > index)
                        ShowWillDecomposePet(index);
                    break;
            }
        }
    }
}