using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//장비 클릭 시 표시 되는 정보 패널
namespace SandboxNetwork
{
    public class DragonPartInfoPanel : MonoBehaviour
    {
        const int MAX_SET_PART_EFFECT = 6;


        [SerializeField]
        GameObject dragonEquipLayer = null;//장비 레이어
        [SerializeField]
        PartSlotFrame partFrame = null;

        [SerializeField]
        Text partNameLabel = null;
        [SerializeField]
        Image statTypeIcon = null;
        [SerializeField]
        Text statTypeLabel = null;
        [SerializeField]
        Text statAmountLabel = null;
        [SerializeField]
        Text reinforceStepLabel = null;

        [SerializeField]
        List<Sprite> partGradeTagList = new List<Sprite>();
        [SerializeField]
        Image gradeTagImageTarget = null;

        [SerializeField]
        GameObject equipButtonNode = null;
        [SerializeField]
        GameObject unequipButtonNode = null;

        [SerializeField]
        GameObject[] partOptionNodeList = null;

        [SerializeField]
        DragonPartChangeEaterPanel partSlotEater = null;

        [SerializeField]
        GameObject leftParentNode = null;

        [SerializeField]
        GameObject rightParentNode = null;

        [SerializeField]
        List<GameObject> setOptionDescNode = new List<GameObject>();

        [SerializeField]
        Button equipButton = null;
        [SerializeField]
        Button unEquipButton = null;
        [SerializeField]
        Button decomposeButton = null;
        [SerializeField]
        Button reinforceButton = null;
        [SerializeField]
        Button dappButton = null;

        [SerializeField]
        Toggle partLockToggle = null;
        [SerializeField]
        GameObject partUnlckObj =null;
        [SerializeField]
        Text FusionDesc;

        int partTag = 0; //파츠 태그 정보
        int tempdragonTag = 0;//클릭 할 때의 드래곤 태그
        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        public delegate void func();

        private func closeCallback;

        public func CloseCallback
        {
            set
            {
                if (value != null)
                {
                    closeCallback = value;
                }
            }
        }

        public void InitCurrentDragonData()
        {
            if (PopupManager.GetPopup<DragonManagePopup>().CurDragonTag != 0)//드래곤 태그값
            {
                var dragonTag = PopupManager.GetPopup<DragonManagePopup>().CurDragonTag;
                var dragonData = User.Instance.DragonData;
                if (dragonData == null)
                {
                    Debug.Log("user's dragon Data is null");
                    return;
                }

                var userDragonInfo = dragonData.GetDragon(dragonTag);
                if (userDragonInfo == null)
                {
                    Debug.Log("user Dragon is null");
                    return;
                }

                tempdragonTag = dragonTag;
            }
        }

        /**
         * @param param //파츠 태그가 param으로 들어옴
         */

        public void ShowDetailInfo(int param, bool isRight = true, func CloseCallback = null)
        {
            CloseCallback = null;

            if(isRight)
            {
                if (rightParentNode != null)
                {
                    gameObject.transform.parent = rightParentNode.transform;
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                }
            }
            else
            {
                if (leftParentNode != null)
                {
                    gameObject.transform.parent = leftParentNode.transform;
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                }
            }

            if (param < 0)
            {
                return;
            }

            if (gameObject.activeInHierarchy == false)
            {
                gameObject.SetActive(true);
            }

            InitCurrentDragonData();
            partTag = param;

            RefreshUI();

            if (CloseCallback != null)
            {
                closeCallback = CloseCallback;
            }
            isOpen = true;
        }

        public void HideDetailInfo()
        {
            DragonPartEvent.HideInfoPanel();

            if (closeCallback != null)
            {
                closeCallback();
            }

            if (gameObject.activeInHierarchy == true)
            {
                gameObject.SetActive(false);
            }

            OnHidePartSlotEaterNode();//장비 슬롯 등록 버튼UI 도 끄기   
            isOpen = false;
        }

        //장비 태그를 기준으로 링크 상태 판단, -1은 연결되지 않음
        void RefreshPartEquipButtonUI()
        {
            var isPartLink = IsPartLink();
            if (isPartLink)//귀속이 됨
            {
                var isSamebelonging = (tempdragonTag == GetPartLinkDragonTag());//클릭한 파츠의 귀속 드래곤이 현재 같은지
                if(unequipButtonNode != null)
                unequipButtonNode.SetActive(isSamebelonging);
                if (equipButtonNode != null)
                    equipButtonNode.SetActive(!isSamebelonging);
            }
            else//귀속 안됨
            {
                if (unequipButtonNode != null)
                    unequipButtonNode.SetActive(isPartLink);
                if (equipButtonNode != null)
                    equipButtonNode.SetActive(!isPartLink);
            }

            var isLock = User.Instance.Lock.IsLockPart(partTag);
            if (reinforceButton != null)
                reinforceButton.SetButtonSpriteState(!IsPartMaxLevel() && !isLock);//맥렙이 아니고 잠기지 않았을 때

            if (decomposeButton != null)
                decomposeButton.SetButtonSpriteState(!isPartLink && !isLock);//귀속된 장비는 분해 안됨
        }

        bool IsPartMaxLevel()
        {
            var partData = User.Instance.PartData.GetPart(partTag);
            var partLevel = partData.Reinforce;
            var partMaxLevel = PartReinforceData.GetMaxReinforceStep(partData.Grade());

            return partLevel >= partMaxLevel;
        }

        bool IsPartLink()
        {
            var partLink = User.Instance.PartData.GetPartLink(partTag);
            return partLink > 0;
        }

        int GetPartLinkDragonTag()
        {
            return User.Instance.PartData.GetPartLink(partTag);
        }

        void RefreshUI()
        {
            if (partTag <= 0)
            {
                return;
            }

            RefreshPartEquipButtonUI();//장착 / 해제 버튼 귀속 상태에 따른 refresh
            RefreshPartDetailUI();//아이콘 및 장비 이름 강화 레벨 같은 UI 연결
        }

        void RefreshPartDetailUI()
        {
            var partData = User.Instance.PartData.GetPart(partTag);//파츠 원본 데이터

            if (partData == null)
            {
                return;
            }
            var partItemDesignData = partData.GetItemDesignData();//파츠 아이템 테이블 데이터
            var partDesignData = partData.GetPartDesignData();//파츠 테이블 데이터

            if (partItemDesignData == null || partDesignData == null)
            {
                Debug.Log("designData is null");
                return;
            }

            var partName = partItemDesignData.NAME;//파츠(장비) 이름
            var partLevel = partData.Reinforce;//강화 레벨
            var partOptionList = partData.SubOptionList;//key와 value

            var partMainoptionType_str = partDesignData.STAT_TYPE;//파츠 타입 string
            var partMainoption_Amount = partData.GetValue();
            var partMainType = partDesignData.VALUE_TYPE;//per 인지 value인지

            partNameLabel.text = partName;

            if (partFrame != null)
                partFrame.SetPartSlotFrame(partTag, partLevel, false);

            if (gradeTagImageTarget != null)
                gradeTagImageTarget.sprite = partGradeTagList[partData.Grade()];

            if (dappButton != null)
                dappButton.gameObject.SetActive(partItemDesignData.ENABLE_NFT && User.Instance.ENABLE_P2E);//nft 가능하면 버튼 켜기

            statTypeIcon.sprite = GetBaseGemSprite(partDesignData);
            statTypeLabel.text = GetStringByType(partMainoptionType_str, partMainType == "PERCENT");

            SetLabelByMainType(partMainType, statAmountLabel, partMainoption_Amount);

            var maxReinforceCount = PartReinforceData.GetMaxReinforceStep(partDesignData.GRADE);//현재 최대 강화 단계
            if (reinforceStepLabel != null)
                reinforceStepLabel.text = SBFunc.StrBuilder(StringData.GetStringByIndex(100001128), " ", partLevel, "/", maxReinforceCount);

            if (partLockToggle != null)
            {
                partLockToggle.gameObject.SetActive(true);
                bool lockState = User.Instance.Lock.IsLockPart(partTag);
                partLockToggle.isOn = lockState;
                partUnlckObj.SetActive(!lockState);
            }
            //강화 단계별 리스트 데이터 갱신
            RefreshPartOptionUI(partLevel, partDesignData.KEY, partOptionList);
            RefreshSetDescLabel();//부옵 표시 UI 갱신

            if(FusionDesc != null)
            {
                FusionDesc.transform.parent.gameObject.SetActive(partData.IsFusion);
                if(partData.IsFusion)
                {
                    var data = PartFusionData.Get(partData.FusionStatKey);
                    if (data != null)
                    {
                        FusionDesc.text = StringData.GetStringFormatByStrKey(data._DESC, partData.FusionStatValue);
                    }
                }
            }
        }

        void SetLabelByMainType(string type, Text targetLabel, float text)
        {
            switch (type.ToUpper())
            {
                case "PERCENT":
                {
                    targetLabel.text = '+' + text.ToString() + '%';
                }
                break;
                case "VALUE":
                {
                    targetLabel.text = '+' + text.ToString();
                }
                break;
            }
        }

        void RefreshPartOptionUI(int partLevel, int ID, List<KeyValuePair<int, float>> partOptionList)
        {
            if (partOptionNodeList != null && partOptionNodeList.Length > 0)
            {
                var listCount = partOptionNodeList.Length;
                var partOptionListLength = partOptionList.Count;
                var availableSlot = PartBaseData.GetMaxReinforceSlotCount(ID);//현재 오픈 가능한 최대 슬롯 갯수
                var checkType = "";
                for (var i = 0; i < listCount; i++)//각 단계별 부옵 리스트 갱신 (강화 단계로 인한 잠금상태, 단순 미강화 잠금, 부옵 표시)
                {
                    var CheckNode = partOptionNodeList[i];
                    int optionKey = 0;
                    float optionValue = 0;
                    if (partOptionListLength > 0 && partOptionListLength > i)//부옵이 존재함. (open)
                    {
                        optionKey = partOptionList[i].Key;
                        optionValue = partOptionList[i].Value;
                        checkType = "open";
                    }
                    else
                    {
                        if (availableSlot > i)//단순 미강화 잠금(lock)
                        {
                            checkType = "lock";
                        }
                        else//최대 강화 수치(ex. 저급은 옵션 1개만 뚫리고 나머지 2개 슬롯 처리)보다 벗어난 잠금(none)
                        {
                            checkType = "none";
                        }
                    }
                    RefreshPartOptionUIByType(checkType, CheckNode, optionKey, optionValue);
                }
            }
        }

        void RefreshPartOptionUIByType(string checkType, GameObject targetNode, int optionKey = 0, float optionValue = 0)//켜야될 노드 세팅
        {
            var childCount = targetNode.transform.childCount;
            if (childCount <= 0)
            {
                return;
            }

            var childNodeList = SBFunc.GetChildren(targetNode);

            GameObject visibleNode = null;
            for (var i = 0; i < childNodeList.Length; i++)
            {
                var node = childNodeList[i];
                var isRightNodeName = node.name == checkType;

                if (isRightNodeName)
                {
                    visibleNode = node;
                }

                node.SetActive(isRightNodeName);
            }

            if (visibleNode != null)//타입에 따라 세부 내용 세팅
            {
                switch (checkType)
                {
                    case "open":
                    {
                        if (optionKey <= 0 || optionValue <= 0)
                        {
                            return;
                        }
                        var data = SubOptionData.Get(optionKey);
                        var dataType = data.STAT_TYPE;
                        var valueType = data.VALUE_TYPE;
                        var typeStr = GetStringByType(dataType, valueType == "PERCENT");

                        var LabelNode = visibleNode.GetComponentInChildren<Text>();
                        if (LabelNode != null)
                        {
                            optionValue = (float)Math.Round((double)optionValue, 2);
                            var str = SBFunc.StrBuilder(typeStr, " +", optionValue);
                            if (valueType == "PERCENT")
                            {
                                str += "%";
                            }

                            LabelNode.text = str;
                        }
                    }
                    break;
                }
            }
        }

        string GetStringByType(string statTypeStr, bool per)
        {
            if (statTypeStr == "ATK_DMG_RESIS" && per == true)
            {
                return StringData.GetStringByStrKey("BASE_DMG_RESIS_PERCENT");
            }

            return StatTypeData.GetDescStringByStatType(statTypeStr, per);
        }

        Sprite GetBaseGemSprite(PartBaseData part)
        {
            var basePart = PartBaseData.GetBasePartFromStatType(part.STAT_TYPE);
            if (basePart != null)
            {
                return basePart.ITEM.ICON_SPRITE;
            }

            return null;
        }

        void RefreshSetDescLabel()//세트 효과 표시
        {
            var partData = User.Instance.PartData.GetPart(partTag);
            if(partData == null)
            {
                foreach(var obj in setOptionDescNode)
                {
                    if (obj == null)
                        continue;
                    obj.SetActive(false);
                }
                return;
            }

            var partSetList = PartSetData.GetAllOptionByGroup(partData.GetPartDesignData().SET_GROUP);
            bool isOnlySixPartSet = partSetList.Count == 1 && partSetList[0].NUM == ePartSetNum.SET_6;//6셋만 있을 때
            for(int i = 0; i< setOptionDescNode.Count; i++)
            {
                var obj = setOptionDescNode[i];
                if (obj == null)
                    continue;

                var descLabel = SBFunc.GetChildrensByName(obj.transform, "desc");
                var valueLabel = SBFunc.GetChildrensByName(obj.transform, "label");

                PartSetData data = null;

                bool isAvailable;
                if (partSetList == null || partSetList.Count < 1)
                {
                    isAvailable = false;
                }
                else if(partSetList.Count <= 1)
                {
                    if (isOnlySixPartSet)
                    {
                        isAvailable = !(i == 0);
                        data = i == 0 ? null : partSetList[0];
                    }
                    else
                    {
                        isAvailable = (i == 0);
                        data = i == 0 ? partSetList[0] : null;
                    }
                }
                else
                {
                    isAvailable = partSetList.Count > i;
                    data = partSetList[i];
                }

                obj.SetActive(true);
                descLabel.gameObject.SetActive(isAvailable);
                SetValueLabel(isAvailable, valueLabel.gameObject, data);
            }
        }

        void SetValueLabel(bool isAvailable,GameObject tartgetObj, PartSetData _setData)
        {
            if (!isAvailable)
                tartgetObj.GetComponent<Text>().text = "-";
            else
            {
                string preFix;
                string postFix;

                switch (_setData.VALUE_TYPE)
                {
                    case "PERCENT":
                        preFix = "";
                        postFix = "%";
                        break;
                    default:
                        preFix = "+";
                        postFix = "";
                        break;
                }

                tartgetObj.GetComponent<Text>().text = preFix + StatTypeData.GetDescStringByStatType(_setData.STAT_TYPE, _setData.VALUE_TYPE == "PERCENT") + " " + _setData.VALUE + postFix;
            }
        }

        /**
         * 1. 장착 비용 없음
         * 2. 해제 비용 있음 (장비 테이블 unequip_cost_num)
         * 3. 기존 장비에 또다른 장비를 씌울때 비용 없음(기존 장비 삭제 , 신규 장비 등록)
         * 4. 현재 드래곤에서(빈슬롯O) 다른 드래곤이 끼고 있는것 착용시 (다른 드래곤 장비 해제 비용 발생)
         * 5. 현재 드래곤에서 (풀슬롯) 다른 드래곤이 끼고 있는것 착용시 (2가지 중 뭐가 맞는지)
         * =>3번 조건 실행, 다른 드래곤 장비 해제 비용 발생)
         */
        public void OnClickEquipButton()
        {
            var partLink = User.Instance.PartData.GetPartLink(partTag);
            var isPartLink = partLink > 0;

            var dragonInfo = User.Instance.DragonData.GetDragon(tempdragonTag);
            if (dragonInfo == null)
            {
                Debug.Log("dragonData is null");
                return;
            }
            var inputSlot = dragonInfo.GetEmptySlotIndex();

            if (isPartLink)//귀속이 됨
            {
                var isSamebelonging = (tempdragonTag == partLink);//클릭한 파츠의 귀속 드래곤 체크 - 같으면 여기로 아예 안옴
                if (!isSamebelonging)//교체 코스트 발생
                {
                    if (inputSlot < 0)//장착 슬롯이 가득 차 있을 경우
                    {
                        var slotOpenTotalCount = dragonInfo.GetCurrentSlotOpenCount();
                        FullSlotState(slotOpenTotalCount);
                    }
                    else//풀슬롯이 아니면 바로 팝업 띄우기
                    {
                        var addCost = 0;
                        var dragonData = User.Instance.DragonData.GetDragon(tempdragonTag);
                        if (dragonData != null)
                        {
                            var currentDragonPartTag = dragonData.Parts[inputSlot];
                            if (currentDragonPartTag > 0)
                            {
                                var currentPartData = User.Instance.PartData.GetPart(currentDragonPartTag);
                                if (currentPartData != null)
                                {
                                    var currentDesignData = currentPartData.GetPartDesignData();
                                    addCost = currentDesignData.UNEQUIP_COST_NUM;
                                }
                            }
                        }

                        var partData = User.Instance.PartData.GetPart(partTag);
                        if (partData == null)
                            return;

                        var partDesignData = partData.GetPartDesignData();
                        if (partDesignData == null)
                            return;

                        var cost = partDesignData.UNEQUIP_COST_NUM + addCost;

                        PricePopup.OpenPopup(StringData.GetStringByIndex(100000248), "", StringData.GetStringByIndex(100000363), cost, ePriceDataFlag.ContentBG | ePriceDataFlag.CancelBtn | ePriceDataFlag.Gold, () =>
                        {
                            ChangeEquipProcess(tempdragonTag, partTag, cost, inputSlot, PopupManager.GetPopup<PricePopup>());
                        });
                    }
                }
            }
            else//귀속 안됨
            {
                if (inputSlot < 0)//장착 슬롯이 가득 차 있을 경우 - 교체 팝업 띄우기
                {
                    var slotOpenTotalCount = dragonInfo.GetCurrentSlotOpenCount();
                    FullSlotState(slotOpenTotalCount);
                }
                else
                {
                    RemainSlotState(inputSlot);
                }
            }
        }

        void RemainSlotState(int inputSlot)
        {
            var param = new WWWForm();
            param.AddField("did", tempdragonTag);//드래곤 id
            param.AddField("tag", partTag);//부속품 tag
            param.AddField("slot", inputSlot);

            Debug.Log("equip adequate Slot index : " + inputSlot);

            //장착 msg -> item_update 및 dragon_exp_update 날라옴
            NetworkManager.Send("dragon/equippart", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (eApiResCode)(jsonData["rs"].Value<int>()) == eApiResCode.OK)
                {
                    if (dragonEquipLayer != null)
                    {

                        var equipLayer = dragonEquipLayer.GetComponent<SubLayer>();
                        if (equipLayer != null)
                        {
                            equipLayer.ForceUpdate();

                            DragonPartEvent.PlayEquipPartAnim(partTag);
                        }
                    }
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000621), StringData.GetStringByIndex(100000614));
                }
            });
        }

        //장착 슬롯이 가득 찼을 경우 유저에게 슬롯 갈아 끼울거냐고 제공
        void FullSlotState(int totalSlotCount)
        {
            OnShowPartSlotEaterNode(totalSlotCount);//파츠 교체시 표시 UI
        }

        //해제 버튼
        public void OnClickUnEquipButton()
        {
            var partData = User.Instance.PartData.GetPart(partTag);
            if (partData == null)
            {
                return;
            }

            var partDesignData = partData.GetPartDesignData();
            if (partDesignData == null)
            {
                return;
            }

            var cost = partDesignData.UNEQUIP_COST_NUM;

            PricePopup.OpenPopup(StringData.GetStringByIndex(100000248), "", StringData.GetStringByIndex(100000362), cost, ePriceDataFlag.ContentBG | ePriceDataFlag.CancelBtn | ePriceDataFlag.Gold, () => 
            { 
                UnEquipProcess(tempdragonTag, partTag, cost, PopupManager.GetPopup<PricePopup>()); 
            });
        }

        //파츠 교체시 표시 UI
        void OnShowPartSlotEaterNode(int totalSlotCount)
        {
            DragonPartEvent.HideInfoPanel();

            if (partSlotEater == null)
            {
                return;
            }

            partSlotEater.OnShowPartSlotEaterNode(totalSlotCount, partTag);

            if (gameObject.activeInHierarchy == true)//파츠 교체 UI 팝업 시 상세창 닫음
            {
                gameObject.SetActive(false);
            }
        }

        void OnHidePartSlotEaterNode()
        {
            if (partSlotEater == null)
            {
                return;
            }

            partSlotEater.OnHidePartSlotEaterNode();
        }

        public void OnClickReinforce()
        {
            if (IsPartMaxLevel())
            {
                ToastManager.On(StringData.GetStringByStrKey("gem_info_text_04"));
                return;
            }

            if (User.Instance.Lock.IsLockPart(partTag))
            {
                ToastManager.On(StringData.GetStringByStrKey("잠금해제요청"));
                return;
            }

            if(User.Instance.Inventory.GetEmptySlotCount() < 1)//최소 1슬롯 이상의 여유분 필요함.
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => {
                        PopupManager.AllClosePopup();
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }

            PopupManager.GetPopup<DragonManagePopup>().CurPartTag = partTag;
            DragonPartEvent.ShowReinforcePanel();
        }

        public void OnClickDecompose()//분해 요청 버튼
        {
            if(IsPartLink())
            {
                ToastManager.On(StringData.GetStringByStrKey("장비분해착용오류"));
                return;
            }

            if (User.Instance.Lock.IsLockPart(partTag))
            {
                ToastManager.On(StringData.GetStringByStrKey("잠금해제요청"));
                return;
            }

            var partData = User.Instance.PartData.GetPart(partTag);
            var resultData = PartDecomposeData.GetDecomposeDataByGradeAndPartLevel(partData.Grade(), partData.Reinforce);
            List<Asset> rewards = new List<Asset>() { new Asset(eGoodType.ITEM, resultData.ITEM, resultData.ITEM_NUM) };
            if (User.Instance.CheckInventoryGetItem(rewards))
            {
                InventoryFullAlert();
                return;
            }

            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100001205), StringData.GetStringByIndex(100001255),
                () => {
                    //ok
                    SendDecomposeMessage();
                },
                () => {
                    //cancle
                },
                () => {
                    //x
                }
            );
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

        public void OnLockToggle()
        {
            partLockToggle.isOn = !partLockToggle.isOn;
            if (partLockToggle.isOn)
            {
                User.Instance.Lock.SetPartLock(partTag, ()=> {
                    partUnlckObj.SetActive(false);
                    RefreshPartEquipButtonUI();//잠금시에는 강화 버튼 비활성화처리
                });                
            }
            else
            {
                User.Instance.Lock.SetPartUnlock(partTag, ()=> {
                    partUnlckObj.SetActive(true);
                    RefreshPartEquipButtonUI();//잠금시에는 강화 버튼 비활성화처리
                });                
            }

            RefreshPartEquipButtonUI();//잠금시에는 강화 버튼 비활성화처리
        }

        void SendDecomposeMessage()
        {
            var data = new WWWForm();
            var arrStr = JsonConvert.SerializeObject(new int[] { partTag });
            data.AddField("materials", arrStr);
            NetworkManager.Send("part/decompose", data, (jsonObj) =>
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
                            //필터 refresh
                            DragonPartEvent.RefreshLayer();

                            //보상 아이템 통째로 넘겨서 팝업 생성
                            var rewardItemList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["reward"]));
                            var popup = PopupManager.OpenPopup<RewardResultPopup>(new RewardPopupData(rewardItemList));
                        }
                    }
                    break;
                    case eApiResCode.PART_INVALID_TAG_TO_DECOMPOUND:
                    {
                        ToastManager.On(100002550);
                    }
                    break;
                }
            });
        }

        /**
         * //업데이트 기대해달라는 문구
         */
        public void OnClickExpectGameAlphaUpdate()
        {
            ToastManager.On(100000326);
        }



        //교체 및 해제 ok 콜백시 프로세스
        void UnEquipProcess(int dragonTag, int partTag, int cost, PricePopup popup)
        {
            var currentUserGold = User.Instance.GOLD;
            if (currentUserGold < cost)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000620));
                popup.OnClickClose();
                return;
            }

            var param = new WWWForm();
            param.AddField("did", dragonTag);
            param.AddField("tag", partTag);

            //장착 msg -> item_update 및 dragon_exp_update 날라옴
            NetworkManager.Send("dragon/unequippart", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (eApiResCode)(jsonData["rs"].Value<int>()) == eApiResCode.OK)
                {
                    var partTagArr = (JArray)jsonData["partTag"];//특정 tag값을 던지면 무조건 사이즈 1의 arr가 날아옴
                    foreach(JToken token in partTagArr)
                    {
                        var curPartTag = token.Value<int>();
                        var partData = User.Instance.PartData.GetPart(curPartTag);
                        if (partData != null)
                            partData.SetLink(-1);
                    }

                    PopupManager.ForceUpdate<DragonManagePopup>();
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000621));
                }

                popup.OnClickClose();
            });
        }

        void ChangeEquipProcess(int dragonTag, int partTag, int cost, int slotIndex, PricePopup popup)
        {
            var currentUserGold = User.Instance.GOLD;
            if (currentUserGold < cost)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000620));
                popup.OnClickClose();
                return;
            }
            var param = new WWWForm();
            param.AddField("did", dragonTag);//드래곤 id
            param.AddField("tag", partTag);//부속품 tag
            param.AddField("slot", slotIndex);//장착 슬롯 인덱스

            //이전 드래곤 귀속 상태 끊어야함
            NetworkManager.Send("dragon/equippart", param, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (eApiResCode)(jsonData["rs"].Value<int>()) == eApiResCode.OK)
                {
                    var unEquippedTag = jsonData["unequipped"].Value<int>();//장비 교체 이전 태그
                    if (unEquippedTag > 0)
                    {
                        var partData = User.Instance.PartData.GetPart(unEquippedTag);
                        if (partData != null)
                            partData.SetLink(-1);
                    }

                    PopupManager.ForceUpdate<DragonManagePopup>();

                    DragonPartEvent.PlayEquipPartAnim(partTag);
                }
                else
                {
                    SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000621));
                }
                popup.OnClickClose();
            });
        }

        /// <summary>
        /// dapp 호출 연결
        /// </summary>
        public void OnClickDAppLink()
        {
            DAppManager.Instance.OpenDAppWithEquipment(partTag, ()=> {
                DragonPartEvent.RefreshLayer();
            });
        }
    }
}

