using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BuildingCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("[ UI ]")]
        [SerializeField] private Text titleText = null;
        [SerializeField] private Text availBuildCountText = null;
        [SerializeField] private Text timeText = null;        
        [SerializeField] private GameObject buildingSpineParent = null;
        [SerializeField] private Button buildButton = null;
        [SerializeField] private GameObject materialParent = null;
        [SerializeField] private Text priceText = null;
        [SerializeField] private GameObject timeIcon = null;
        
        [Space(20)]

        [Header("[ Common ]")]
        [SerializeField] private GameObject blockObject = null;
        [SerializeField] private Text blockGuideText = null;
        [SerializeField] private Image buildingBG = null;
        [SerializeField] private Image Frame = null;

        BuildingConstructListData listData = null;
        public ConstructInfoData curConstructData { get; private set; } = null;
        BuildingLevelData buildingLevelData = null;
        SkeletonGraphic buildingSpine = null;

        List<BuildingOpenData> totalBuildingOpenList = new List<BuildingOpenData>();        // 건설 가능한 전체 빌딩 목록 - 랜드마크 타입(1번) 제외
        bool availBuilding = false;        // 건설 가능한 빌딩 목록
        
        int currentBuildingCount = 0;       // 현재 지어진 건물 갯수
        int buildingRemainCount = 0;        // 중복 건물 잔여 갯수로 사용

        bool isSufficientItem = true;//재료 수량 체크
        bool isSufficientPrice = true;//골드 여분 체크

        public delegate void Func();
        public Func clickCallback = null;
        string BuildingKey = "";
        public void InitBuildingCard(BuildingConstructListData data, ConstructInfoData buildingData, Func _clickCallback = null)
        {
            if (buildingData == null) 
            { 
                return; 
            }
            
            listData = data;

            // 데이터 세팅
            curConstructData = buildingData;
            BuildingKey = curConstructData.KEY;
            totalBuildingOpenList = BuildingOpenData.GetByBuildingGroup(BuildingKey);
            buildingLevelData = BuildingLevelData.GetDataByGroupAndLevel(BuildingKey, 0);
            if (_clickCallback != null)
                clickCallback = _clickCallback;
            Refresh();
        }

        public void InitDisableBuildingCard(BuildingConstructListData data, BuildingBaseData baseData)
        {
            if (baseData == null)
            {
                return;
            }

            listData = data;

            // 데이터 세팅
            BuildingKey = baseData.KEY;
            curConstructData = null;
            totalBuildingOpenList = BuildingOpenData.GetByBuildingGroup(BuildingKey);
            buildingLevelData = BuildingLevelData.GetDataByGroupAndLevel(BuildingKey, 0);
            Refresh();
        }

        public void Refresh()
        {
            ClearBuildingData();

            int buildingCount = BuildingOpenData.GetAvailTotalBuildingList(BuildingKey).Count;
            availBuilding = buildingCount > 0;

            // 현재 건설 가능 갯수 계산
            BuildInfo userBuilding = null;
            int needLevel = 0;      // 외형 레벨 부족 가이드 텍스트 표기를 위한 값 저장
            foreach (BuildingOpenData openData in totalBuildingOpenList)
            {
                if (User.Instance.GetAreaLevel() >= openData.OPEN_LEVEL)
                {
                    userBuilding = User.Instance.GetUserBuildingInfoByTag(openData.INSTALL_TAG);
                    if (userBuilding != null)
                    {
                        currentBuildingCount++;
                    }
                    else
                    {
                        buildingRemainCount++;
                    }
                }
                else
                {
                    if (needLevel == 0)
                    {
                        needLevel = openData.OPEN_LEVEL;
                    }
                }
            }


            /* UI 관련 세팅 */

            titleText.text = StringData.GetStringByStrKey(BuildingBaseData.Get(BuildingKey)._NAME);
            availBuildCountText.text = string.Format("({0}/{1})", currentBuildingCount, buildingCount);
            if (curConstructData == null)
            {
                titleText.color = Color.gray;
                Frame.color = Color.gray;
                buildingBG.color = new Color(0.25f, 0.25f, 0.25f, 1.0f);
                availBuildCountText.color = Color.gray;
            }
            else
            {
                titleText.color = Color.white;
                Frame.color = Color.white;
                buildingBG.color = Color.white;
                availBuildCountText.color = new Color(1.0f, 0.8941176470588235f, 0.0f);
            }
            

            SetBuildingSpine();//건물 스파인 세팅

            blockObject.SetActive(false);

            //최대 수량 제어나 건설 가능 수량은 앞 단(buildingLayer)에서 처리 하므로 주석
            //SetBlockObject(needLevel);

            if(buildingLevelData != null)
            {
                SetCurrencyEfficient();//골드 수량 체크
                SetCostLabel();//골드 표시
            }

            if(IsBuildable())
            {
                int time = BuildingLevelData.GetDataByGroupAndLevel(BuildingKey, 0).UPGRADE_TIME;
                timeText.text = SBFunc.TimeString(time);//건설 시간
                timeText.color = Color.white;
                timeIcon.SetActive(true);
                if (buildingSpine != null)
                {
                    buildingSpine.color = Color.white;
                    buildingSpine.AnimationState.SetAnimation(0, "play", true);
                }
            }
            else
            {
                if (curConstructData == null)
                {
                    if (BuildingOpenData.GetTotalBuildingList(BuildingKey).Count <= currentBuildingCount)
                    {
                        timeText.color = Color.yellow;
                        timeText.text = StringData.GetStringByStrKey("모든건설완료");
                    }
                    else
                    {
                        timeText.color = Color.gray;
                        timeText.text = StringData.GetStringByStrKey("조건부족");
                    }
                    

                    if (buildingSpine != null)
                    {
                        buildingSpine.color = new Color(0.25f,0.25f,0.25f);
                        buildingSpine.AnimationState.SetAnimation(0, "off", true);
                    }
                }
                else
                {
                    timeText.text = StringData.GetStringByStrKey("재료부족");
                    timeText.color = Color.red;

                    if (buildingSpine != null)
                    {
                        buildingSpine.color = Color.gray;
                        buildingSpine.AnimationState.SetAnimation(0, "off", true);
                    }
                }

                timeIcon.SetActive(false);                
            }

            // 건설이 불가능할 경우 처리
            buildButton.SetButtonSpriteState(isSufficientPrice && isSufficientItem);
            availBuildCountText.gameObject.SetActive(availBuilding);
        }

        bool IsBuildable()
        {
            if (curConstructData == null)
                return false;

            bool ret = true;
            switch (buildingLevelData.COST_TYPE)
            {
                case "GOLD":
                    ret = User.Instance.GOLD >= buildingLevelData.COST_NUM;
                    break;
                case "GEMSTONE":
                    ret = User.Instance.GEMSTONE >= buildingLevelData.COST_NUM;
                    break;
            }

            if (ret)
            {
                foreach (var needItem in buildingLevelData.NEED_ITEM)
                {
                    if (User.Instance.GetItemCount(needItem.ItemNo) < needItem.Amount)
                    {
                        ret = false;
                        break;
                    }
                }
            }

            return ret;
        }

        void SetBuildingSpine()
        {
            SBFunc.RemoveAllChildrens(buildingSpineParent.transform);
            var buildingUISpinePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.BuildingUIClonePath, BuildingKey.ToString());
            if(buildingUISpinePrefab != null)
            {
                var spineObj = Instantiate(buildingUISpinePrefab, buildingSpineParent.transform);
                buildingSpine = spineObj.GetComponentInChildren<SkeletonGraphic>();
                if (buildingSpine == null)
                    Destroy(spineObj);
                else
                {
                    buildingSpine.Clear();
                    buildingSpine.timeScale = 1f;
                }
            }
        }

        void SetCostLabel()
        {
            if (priceText != null)
            {
                priceText.text = SBFunc.CommaFromNumber(buildingLevelData.COST_NUM);
                priceText.color = isSufficientPrice ? Color.white : Color.red;
            }
        }
        void SetItemEfficient()//화면 앞단에서는 재료 체크 안하는 것으로 기획 변경
        {
            SBFunc.RemoveAllChildrens(materialParent.transform);

            // 재료 관련 레이어 세팅
            foreach (var needItem in buildingLevelData.NEED_ITEM)
            {
                GameObject newItem = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "item"), materialParent.transform);
                newItem.transform.localScale = Vector3.one;

                ItemFrame itemframe = newItem.GetComponent<ItemFrame>();
                if (itemframe != null)
                {
                    itemframe.setFrameRecipeInfo(needItem.ItemNo, needItem.Amount);

                    isSufficientItem = isSufficientItem && itemframe.IsSufficientAmount;
                }
                else
                {
                    Destroy(newItem);
                }
            }
        }

        void SetCurrencyEfficient()
        {
            switch (buildingLevelData.COST_TYPE)
            {
                case "GOLD":
                    isSufficientPrice = User.Instance.GOLD >= buildingLevelData.COST_NUM;
                    break;
                case "GEMSTONE":
                    isSufficientPrice = User.Instance.GEMSTONE >= buildingLevelData.COST_NUM;
                    break;
            }
        }

        void SetBlockObject(int _needLevel)
        {
            if (totalBuildingOpenList.Count == currentBuildingCount)
            {
                blockObject.SetActive(true);
                blockGuideText.text = StringData.GetStringByIndex(100000941);
            }
            else if (_needLevel > User.Instance.GetAreaLevel() && buildingRemainCount == 0)
            {
                blockObject.SetActive(true);
                blockGuideText.text = string.Format(StringData.GetStringByIndex(100000059), _needLevel);
            }
        }

        public void OnClickConstructButton()
        {
            if (curConstructData == null)
                return;

            OnScaleClear();
                        
            if (IsEmptyGrid())
            {
                Vector2Int target = Vector2Int.zero;
                if (listData != null)
                    target = listData.TargetCell;
                BuildingPopupData popupData = new BuildingPopupData(curConstructData, target);
                PopupManager.OpenPopup<BuildingConstructPopup>(popupData);

                //if (clickCallback != null)
                //    clickCallback();
                //DirectConstructProcess();//바로 설치로 이동
            }
            else
            {
                ToastManager.On(100002567);
            }
        }

        void DirectConstructProcess()
        {
            var buildingOpenData = curConstructData.openData;
            if (buildingOpenData == null) { return; }

            Town.Instance.ConstructTag = buildingOpenData.INSTALL_TAG;
            Town.Instance.SetConstructModeState(true);

            PopupManager.AllClosePopup();
        }

        void ClearBuildingData()
        {
            currentBuildingCount = 0;
            buildingRemainCount = 0;
        }

        bool IsEmptyGrid()//현재 유저 grid Data에 0(공백 - 설치 가능 빈자리)이 있는지
        {
            var userData = User.Instance.GetGridData();
            if(userData == null)
            {
                return false;
            }

            var keyList = new List<int>(userData.Keys);
            if(keyList == null || keyList.Count <= 0)
            {
                return false;
            }

            for(int i = 0; i < keyList.Count; i++)
            {
                var key = keyList[i];
                if(key < 0)//지하철은 패스
                {
                    continue;
                }

                var floorData = userData[key];
                if(floorData != null && floorData.Count > 0)
                {
                    var floorKeyList = new List<int>(floorData.Keys);

                    if(floorKeyList == null || floorKeyList.Count <= 0)
                    {
                        continue;
                    }

                    for(int k = 0; k < floorKeyList.Count; k++)
                    {
                        var floorKey = floorKeyList[k];

                        var tagValue = floorData[floorKey];

                        if(tagValue <= 0)
                        {
                            return true;
                        }
                    }
                }

            }
            return false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (curConstructData == null)
                return;

            foreach (Transform bro in transform.parent.transform)
            {
                bro.localScale = Vector3.one;
            }
            transform.localScale = Vector3.one * 1.1f;
        }

        public void OnPointerExit(PointerEventData eventDate)
        {
            OnScaleClear();
        }

        public void OnScaleClear()
        {
            transform.localScale = Vector3.one;
        }
    }
}