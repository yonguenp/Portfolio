using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonPartDecomposePanel : MonoBehaviour
    {
        const int Constraint_Part_Level_Condition = 9;//9강 이상 분해 재료에 포함되어있는지 체크 해야함

        [SerializeField]
        List<ItemFrame> materialItemList = new List<ItemFrame>();

        [Header("materials")]
        [SerializeField]
        Text emptyLabelText = null;
        [SerializeField]
        GameObject partSlotNode = null;
        [SerializeField]
        GameObject arrowNode = null;
        [SerializeField]
        PartSlotFrame partFrame = null;
        [SerializeField]
        Text partNameText = null;
        [SerializeField]
        Image tagImageTarget = null;
        [SerializeField]
        List<Sprite> tagImageList = new List<Sprite>();


        [Header("buttons")]
        [SerializeField]
        Button decomposeButton = null;
        [SerializeField]
        Button selectAllButton = null;
        [SerializeField]
        Button deselectAllButton = null;

        List<int> decomposeTagList = new List<int>();//분해 모드 선택 시 다중 체크 담기
        public List<int> DecomposeTagList { get { return decomposeTagList; } }

        List<int> materialList = new List<int>();//분해할 재료 태그 리스트
        List<ItemFrame> resultMaterialList = new List<ItemFrame>();//분해 결과 프리펩 리스트
        List<Asset> decomposedItemList = new List<Asset>(); // 분해 이후 얻을 아이템을 가방에서 체크하기 위한 용도

        int currentListCount = 0;//현재 리스트의 크기를 세팅(DragonPartListPanel에서 사이즈 드로우 할때 세팅) - 전체 해제 버튼 제어용
        public int CurrentListCount {set { currentListCount = value; } }

        bool isOpen = false;
        public bool IsOpen { get { return isOpen; } }

        void OnDisable()
        {
            isOpen = false;
        }
        public void ShowDecomposePanel()
        {
            gameObject.SetActive(true);
            isOpen = true;
            Init();
        }

        public void HideDecomposePanel()
        {
            gameObject.SetActive(false);
        }
        public void Init()
        {
            initTagData();
            ClearMaterialTag();
            InitDecomposeMaterial();
            initDecomposeButtonUI();
            RefreshSelectButtonUI();
            RefreshMaterialUIList(-1);
        }

        void initTagData()//장비 태그값 강제 비우기
        {
            PopupManager.GetPopup<DragonManagePopup>().CurPartTag = 0;
        }

        void PushMaterialTag(int tag)
        {
            var index = materialList.IndexOf(tag);
            if (index < 0)
                materialList.Add(tag);
        }

        void PopMaterialTag(int tag)
        {
            var index = materialList.IndexOf(tag);
            if (index > -1)
                materialList.RemoveAt(index);
        }

        void ClearMaterialTag()
        {
            if (materialList == null)
                materialList = new List<int>();

            materialList.Clear();
        }

        void InitDecomposeMaterial()//분해 이후 획득 가능한 재료 예상치
        {
            if (decomposeTagList == null)
                decomposeTagList = new List<int>();

            decomposeTagList.Clear();

            var possibleResultItemList = PartDecomposeData.GetTotalResultItemList();
            if (possibleResultItemList == null || possibleResultItemList.Count < 0)
                return;

            if (resultMaterialList == null)
                resultMaterialList = new List<ItemFrame>();

            resultMaterialList.Clear();

            for (var i = 0; i < possibleResultItemList.Count; i++)
            {
                var itemID = possibleResultItemList[i];

                if(materialItemList != null && materialItemList.Count > i)
                {
                    var targetItemFrame = materialItemList[i];
                    if (itemID > 0)
                    {
                        targetItemFrame.SetFrameItemInfo(itemID, 0, -1);//0으로 초기화
                        resultMaterialList.Add(targetItemFrame);
                    }
                }
            }
        }

        public void RefreshCompoundMaterial()//분해 이후 획득 가능한 재료 예상치 갱신 - 일단 전부 외부에서 갱신으로
        {
            RefreshDecomposeButtonUI();//분해 버튼 활성 / 비활성(현재 등록한 분해 재료 사이즈에 따라)
            RefreshSelectButtonUI();//전체 선택 / 전체 해제 갱신

            if (materialList == null || materialList.Count <= 0)
            {
                SetresultMaterialList(true);
                return;
            }

            Dictionary<int, int> tempItemCount = new Dictionary<int, int>();
            tempItemCount.Clear();
            decomposedItemList.Clear();
            for (var i = 0; i < materialList.Count; i++)
            {
                var tag = materialList[i];
                if (tag < 0)
                {
                    continue;
                }

                var partData = User.Instance.PartData.GetPart(tag);
                var partGrade = partData.Grade();
                var partLevel = partData.Reinforce;

                var decomposeData = PartDecomposeData.GetDecomposeDataByGradeAndPartLevel(partGrade, partLevel);
                if (decomposeData == null)
                {
                    continue;
                }

                var item = decomposeData.ITEM;
                var itemCount = decomposeData.ITEM_NUM;

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

            SetresultMaterialList(false, tempItemCount);
        }

        void SetresultMaterialList(bool isInit, Dictionary<int, int> itemCountData = null)
        {
            for (var i = 0; i < resultMaterialList.Count; i++)
            {
                var item = resultMaterialList[i];
                if (item == null)
                    continue;

                var itemName = item.GetItemID();

                var itemCount = 0;
                if (!isInit)
                {
                    if (itemCountData.ContainsKey(itemName))
                        itemCount = itemCountData[itemName];
                    else
                        itemCount = 0;
                }

                item.SetFrameItemInfo(itemName, itemCount, -1);
            }
        }

        void initDecomposeButtonUI()
        {
            if (decomposeButton != null)
                decomposeButton.SetButtonSpriteState(false);
        }

        void RefreshDecomposeButtonUI()
        {
            if (decomposeButton != null)
            {
                var isEmpty = (materialList == null || materialList.Count <= 0);
                decomposeButton.SetButtonSpriteState(!isEmpty);
            }
        }

        public void OnClickAllReleased()//모두 해제 버튼
        {
            if(IsEmpty())
            {
                ToastManager.On(StringData.GetStringByStrKey("장비분해모두해제오류"));
                return;
            }

            ReleaseCheckPartSlot();
        }

        public void OnClickAllTouched()//모두 선택 버튼
        {
            if (IsAllSelect())
            {
                ToastManager.On(StringData.GetStringByStrKey("장비분해모두선택오류"));
                return;
            }

            AllCheckPartSlot();
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

        public void OnClickDecompose()//분해 요청
        {
            if(materialList == null || materialList.Count <= 0)
            {
                ToastManager.On(100001129);
                return;
            }

            if (User.Instance.CheckInventoryGetItem(decomposedItemList)) // 가방 체크
            {
                InventoryFullAlert();
                return;
            }


            var hasHigherPartCheck = HasHigherPartLevelCheck();

            if (hasHigherPartCheck)
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100001205), StringData.GetStringByIndex(100001204),
                    () => {
                        //ok
                        RequestDecomposeMessage();
                    },
                    () => {
                        //cancle
                    },
                    () => {
                        //x
                    });
                return;
            }
            else
                RequestDecomposeMessage();
        }

        bool HasHigherPartLevelCheck()//9강 이상 장비 분해 요청시 팝업이 떠야한다고 함
        {
            bool hasHigherPart = false;
            if (materialList == null || materialList.Count <= 0)
                return hasHigherPart;

            for (var i = 0; i < materialList.Count; i++)
            {
                var tag = materialList[i];
                if (tag <= 0)
                    continue;

                var partData = User.Instance.PartData.GetPart(tag);
                if (partData == null)
                    continue;

                var partLevel = partData.Reinforce;

                if (partLevel >= Constraint_Part_Level_Condition)
                {
                    hasHigherPart = true;
                    return hasHigherPart;
                }
            }

            return hasHigherPart;
        }


        void RequestDecomposeMessage()
        {
            var param = new WWWForm();//{ materials: materialList};
            param.AddField("materials", JsonConvert.SerializeObject(materialList.ToArray()));
            NetworkManager.Send("part/decompose", param, (jsonObj) =>
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
                            //보상 아이템 통째로 넘겨서 팝업 생성
                            var rewardItemList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["reward"]));
                            var popup = PopupManager.OpenPopup<RewardResultPopup>(new RewardPopupData(rewardItemList));
                            Init();
                            DragonPartEvent.RefreshList();
                        }
                    }
                    break;
                    case eApiResCode.PART_INVALID_TAG_TO_DECOMPOUND:
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

        public void PushDecompoundList(int tag)
        {
            decomposeTagList.Add(tag);
            PushMaterialTag(tag);
        }

        public void PopDecompoundList(int tag)
        {
            var index = decomposeTagList.IndexOf(tag);
            if (index > -1)
                decomposeTagList.RemoveAt(index);

            PopMaterialTag(tag);
        }
        void ReleaseCheckPartSlot()//전체 해제
        {
            ClearMaterialTag();
            decomposeTagList.Clear();
            RefreshMaterialUIList(-1);
            DragonPartEvent.RefreshList();
            RefreshCompoundMaterial();
        }

        void AllCheckPartSlot()//전체 선택
        {
            decomposeTagList.Clear();
            DragonPartEvent.DecomposeAllSelect();//안쪽에서 리스트 세팅까지 같이함
            RefreshMaterialUIAuto();
            RefreshCompoundMaterial();
        }

        public void RefreshSelectButtonUI()
        {
            if (selectAllButton != null)
                selectAllButton.SetButtonSpriteState(!IsAllSelect());
            if (deselectAllButton != null)
                deselectAllButton.SetButtonSpriteState(!IsEmpty());
        }

        bool IsEmpty()
        {
            return decomposeTagList.Count <= 0;
        }

        bool IsAllSelect()//전부 선택했는지
        {
            return decomposeTagList.Count == currentListCount;
        }

        public void RefreshMaterialUIAuto()
        {
            RefreshMaterialUIList(materialList.Count > 0 ? materialList[materialList.Count - 1] : -1);
        }

        void RefreshMaterialUIList(int tag = -1)//분해요청 목록에 대한 리스트
        {
            if(materialList == null || materialList.Count <= 0)
            {
                emptyLabelText.gameObject.SetActive(true);
                partSlotNode.SetActive(false);
                return;
            }

            emptyLabelText.gameObject.SetActive(false);
            partSlotNode.SetActive(true);

            if(tag <= 0 && materialList.Count > 0)
                tag = materialList[materialList.Count - 1];

            var partData = User.Instance.PartData.GetPart(tag);
            if (partData == null)
            {
                Debug.Log("장비 데이터 없음 tag : " + tag);
                return;
            }

            if (arrowNode != null)
                arrowNode.SetActive(materialList.Count > 1);

            var designData = partData.GetItemDesignData();
            var partGrade = partData.Grade();
            var gradeModifyIndex = partGrade - 1;
            if (partFrame != null)
                partFrame.SetPartSlotFrame(tag, partData.Reinforce,false);
            if (partNameText != null)
                partNameText.text = designData.NAME;
            if (tagImageTarget != null && tagImageList != null && tagImageList.Count > gradeModifyIndex && gradeModifyIndex >= 0)
                tagImageTarget.sprite = tagImageList[gradeModifyIndex];
        }

        public void OnClickLeft()
        {
            if (materialList == null || materialList.Count <= 0)
                return;

            var currentTag = partFrame.PartTag;
            var index = materialList.IndexOf(currentTag);
            if(index < 0)
            {
                Debug.Log("장비 태그오류 tag : " + currentTag);
                return;
            }

            if(index <= 0)
                currentTag = materialList[materialList.Count - 1];
            else
                currentTag = materialList[index - 1];

            RefreshMaterialUIList(currentTag);
        }

        public void OnClickRight()
        {
            if (materialList == null || materialList.Count <= 0)
                return;

            var currentTag = partFrame.PartTag;
            var index = materialList.IndexOf(currentTag);
            if (index < 0)
            {
                Debug.Log("장비 태그오류 tag : " + currentTag);
                return;
            }

            if (index >= materialList.Count - 1)
                currentTag = materialList[0];
            else
                currentTag = materialList[index + 1];

            RefreshMaterialUIList(currentTag);
        }
    }
}
