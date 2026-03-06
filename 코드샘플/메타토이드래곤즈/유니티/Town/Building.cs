using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Newtonsoft.Json.Linq;
using DG.Tweening;
using Com.LuisPedroFonseca.ProCamera2D;

namespace SandboxNetwork
{
    public struct BuildingUIEvent
    {
        public enum eBuildingUIEventEnum
        {
            RefreshSpine,//생산 및 랜드마크 UI 에서 스파인 갱신용 - building 상태 변경에 따라 UI도 바뀌게
        }

        public eBuildingUIEventEnum Event;
        static BuildingUIEvent e;

        public BuildingUIEvent(eBuildingUIEventEnum _Event)
        {
            Event = _Event;
        }

        public static void RefreshSpine()
        {
            e.Event = eBuildingUIEventEnum.RefreshSpine;
            EventManager.TriggerEvent(e);
        }
    }

    public class Building : MonoBehaviour
    {
        public enum ProductState
        {
            UNKNOWN,
            RUNNING,
            QUEUE_EMPTY,            
            COMPLETED_ALL
        }


        [SerializeField]
        protected SkeletonAnimation spine = null;
        [SerializeField]
        protected SkeletonAnimation cloudSpine = null;
        [SerializeField]
        protected SkeletonAnimation shutterSpine = null;
        [SerializeField]
        protected SkeletonAnimation npcSpine = null;
        [SerializeField]
        protected GameObject shutterEffectObj = null;
        [SerializeField]
        protected GameObject fireWorkObj = null;
        [SerializeField]
        protected Text timeText = null;
        [SerializeField]
        protected GameObject finishLayerObject = null;
        [SerializeField]
        protected GameObject tutorialRect = null;

        protected ProductState curState = ProductState.UNKNOWN;

        protected BuildingProductUI BuildingProductUI = null;

        protected bool isBuildingCompleteClicked = false;

        protected int curProductCount = 0;
        public int Cell
        {
            get;
            private set;
        } = 0;
        
        public int Floor
        {
            get;
            private set;
        } = 0;

        public BuildInfo Data
        {
            get;
            private set;
        } = null;

        public int BTag
        {
            get;
            private set;
        } = -1;

        public string BName
        {
            get;
            private set;
        } = "";

        protected float curSec = 0.0f;
        protected float waitSec = 0.5f;
        protected bool isUpdate = false;

        private bool isNetworkState = false;

        protected virtual void Start()
        {
            fireWorkObj.SetActive(false);
            ClearLayerState();

            UpdateOn();
            ActiveAction();
            isNetworkState = false;
        }

        protected virtual void Update()
        {
            if (!isUpdate)
                return;

            curSec -= Time.deltaTime;
            if (curSec > 0)
                return;

            curSec = waitSec;
            if (ActiveAction())
            {
                BuildingAction();
            }
        }

        public void Init(int cell, int floor)
        {
            Cell = cell;
            Floor = floor;
        }

        public void Init(int cell, int floor, BuildInfo buildingInstance, int bTag, string bName)
        {
            Cell = cell;
            Floor = floor;
            Data = buildingInstance;
            BTag = bTag;
            BName = bName;
        }

        public void UpdateOn()
        {
            isUpdate = true;
        }

        public void UpdateOff()
        {
            isUpdate = false;
        }

        protected virtual void OnDestroy()
        {
            UpdateOff();
        }

        public virtual void RefreshBuildingAction()
        {
            BuildingAction();
        }

        protected virtual void BuildingAction()
        {
            if (Data == null || spine == null)
                return;

            var queueList = User.Instance.GetProduces(Data.Tag);
            if (queueList == null || queueList.Items == null || queueList.Items.Count < 1)
            {
                if (0 != curProductCount)
                {
                    curProductCount = 0;
                    CheckProductAlarm();
                }

                SetProductState(ProductState.QUEUE_EMPTY);
                return;
            }

            ProductState animState = ProductState.QUEUE_EMPTY;
            int count = 0;
            int curExp = 0;

            var itemCount = queueList.Items.Count;
            if (itemCount > 0)
            {
                for (var i = 0; i < itemCount; ++i)
                {
                    var curItem = queueList.Items[i];
                    if (curItem == null)
                        continue;

                    if (curItem.ProductionExp > 0)
                    {
                        curExp += curItem.ProductionExp;
                    }
                    else
                    {
                        BuildingOpenData buildingOpendData = BuildingOpenData.GetWithTag(Data.Tag);
                        
                        ProductData itemReceipe = ProductData.GetProductDataByGroupAndKey(buildingOpendData.BUILDING, curItem.RecipeID);
                        if(itemReceipe != null)
                            curExp += itemReceipe.PRODUCT_TIME;
                    }

                    if (TimeManager.GetTimeCompare(curExp) > 0)
                    {
                        animState = ProductState.RUNNING;
                        break;
                    }
                    else
                    {
                        count++;
                    }
                }

                if (count != curProductCount)
                {
                    curProductCount = count;
                    CheckProductAlarm();
                }

                if(count == itemCount)
                {
                    animState = ProductState.COMPLETED_ALL;
                }
            }

            SetProductState(animState);
        }

        protected Spine.TrackEntry SetAnimation(int trackIndex, string animationName, bool loop)
        {
            Spine.TrackEntry ret = spine.AnimationState.SetAnimation(trackIndex, animationName, loop);
            
            return ret;
        }
        
        public void ShutterForceOff()
        {
            shutterSpine.gameObject.SetActive(false);
        }
        public virtual bool ActiveAction()
        {
            if (cloudSpine == null || finishLayerObject == null || timeText == null || shutterSpine == null) { return false; }

            bool result = false;
            
            if (Data != null)
            {
                SetLockIcon(Data.State);
                switch (Data.State)
                {
                    case eBuildingState.NONE:
                        result = false;
                        break;
                    case eBuildingState.LOCKED:                        
                        result = false;
                        break;
                    case eBuildingState.NOT_BUILT:
                        result = false;
                        break;
                    case eBuildingState.CONSTRUCTING:
                        int time = TimeManager.GetTimeCompare(Data.ActiveTime);
                        if(time <= 0)
                        {
                            time = 0;
                            Data.SetState(eBuildingState.CONSTRUCT_FINISHED);
                            if(shutterSpine != null)
                            {
                                shutterSpine.gameObject.SetActive(true);
                                shutterSpine.AnimationState.SetAnimation(0, "closed", false);
                            }
                        }
                        
                        timeText.text = SBFunc.TimeString(time);

                        result =  false;
                        break;
                    case eBuildingState.CONSTRUCT_FINISHED:
                        if(shutterSpine != null)
                        {
                            shutterSpine.gameObject.SetActive(true);
                            shutterSpine.AnimationState.SetAnimation(0, "closed", false);
                        }
                        result =  false;
                        break;
                    case eBuildingState.NORMAL:
                        npcSpine?.gameObject.SetActive(true);
                        result = true;
                        break;
                    default:
                        result =  false;
                        break;
                }

                if(spine != null)
                    spine.gameObject.SetActive(Data.State != eBuildingState.CONSTRUCTING);

                cloudSpine.gameObject.SetActive(Data.State == eBuildingState.CONSTRUCTING);
                finishLayerObject.SetActive(false);
                
                timeText.gameObject.SetActive(Data.State == eBuildingState.CONSTRUCTING);

                if(Data.State == eBuildingState.CONSTRUCT_FINISHED)
                {
                    if (!shutterEffectObj.activeSelf)
                    {
                        shutterEffectObj.SetActive(true);
                        shutterEffectObj.transform.localPosition = new Vector3(-5f, -35f, 0f);
                    }
                }
                else
                {
                    if (shutterEffectObj.activeSelf)
                        shutterEffectObj.SetActive(false);
                }
                
            }

            return result;
        }

        protected virtual void SetLockIcon(eBuildingState state)
        {
            //일반건물을 LockIcon 없음 
            //현재는 랜드마크만
        }

        void ShutterOpenEvent (Spine.TrackEntry te)
        {
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(ProductState.UNKNOWN);
            }
            BuildingCompletePopup.OpenPopup(null, () =>
            {
                Town.Instance.ZoomEndProcess(false);
                Camera.main.GetComponent<ProCamera2D>().ResetSize();
                BuildCompleteEvent.Send(this, eBuildingState.NORMAL);

                if (BuildingProductUI != null)
                {
                    BuildingProductUI.SetState(ProductState.QUEUE_EMPTY);
                    UpdateOn();
                }
            });
            isBuildingCompleteClicked = false;
            
            if(fireWorkObj != null)
            {
                fireWorkObj.SetActive(false);
            }
            if(shutterSpine != null)
            {
                shutterSpine.gameObject.SetActive(false);
                shutterSpine.AnimationState.Complete -= ShutterOpenEvent;
            }
            
        }

        public void OnClickFinishButton()
        {
            if (isBuildingCompleteClicked)
                return;

            isBuildingCompleteClicked = true;

            WWWForm paramData = new WWWForm();
            paramData.AddField("tag", BTag);
            if (isNetworkState) return;
            isNetworkState = true;
			finishLayerObject.GetComponent<Button>().interactable = false;
            
			NetworkManager.Send("building/complete", paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            SoundManager.Instance.PlaySFX("sfx_build_complete_effect");

                            if (BuildingProductUI != null)
                            {
                                BuildingProductUI.SetState(ProductState.UNKNOWN);
                                UpdateOff();
                            }
                            shutterEffectObj.SetActive(false);
                            if(shutterSpine != null)
                            {
                                shutterSpine.AnimationState.SetAnimation(0, "open", false);
                                if (fireWorkObj != null)
                                {
                                    fireWorkObj.SetActive(true);
                                }

                                shutterSpine.AnimationState.Complete += ShutterOpenEvent;
                            }
                            else
                            {
                                ShutterOpenEvent(new Spine.TrackEntry());
                            }
                            


                            if(BTag == (int)eLandmarkType.SUBWAY)
                            {
                                // 지하철 완성하면 지하철 돌아다녀야징
                                Town.Instance.RefreshMap();
                            }

                            if (BTag == (int)eLandmarkType.GEMDUNGEON)
                            {
                                BuildingCompletePopup.OpenPopup(null, () =>
                                {
                                    Town.Instance.ZoomEndProcess(false);
                                    Camera.main.GetComponent<ProCamera2D>().ResetSize();
                                    BuildCompleteEvent.Send(this, eBuildingState.NORMAL);

                                    if (BuildingProductUI != null)
                                    {
                                        BuildingProductUI.SetState(ProductState.QUEUE_EMPTY);
                                        UpdateOn();
                                    }
                                });
                            }
                            break;

						default:
							finishLayerObject.GetComponent<Button>().interactable = true;
                            isBuildingCompleteClicked = false;
                            break;
					}
                }
            },(string arg) =>
            {
                isNetworkState = false;
            });
        }

        public void OnClickBuilding()
        {
            if (TutorialManager.tutorialManagement.IsPlayingTutorial) // 건물 클릭시 튜토리얼 전용 팝업을 뛰어야 하는 경우도 있으니 제외
                return;

            BuildingOpenData openData = BuildingOpenData.GetWithTag(BTag);

            if (openData == null)
                return;

            if (Data != null)
            {
                switch (Data.State)
                {
                    case eBuildingState.CONSTRUCTING://가속팝업
                        if(Data.Level == 0)
                        {
                            AccelerationMainPopup.OpenPopup(eAccelerationType.CONSTRUCT, new BuildingPopupData(openData.INSTALL_TAG));
                        }
                        else
                        {
                            AccelerationMainPopup.OpenPopup(eAccelerationType.LEVELUP, new BuildingPopupData(openData.INSTALL_TAG));
                        }
                        return;
                    case eBuildingState.CONSTRUCT_FINISHED:
                        BuildCompleteEvent.Send(this, Data.State);
                        return;
                    case eBuildingState.NONE:
                    case eBuildingState.LOCKED:
                    case eBuildingState.NOT_BUILT:
                    case eBuildingState.NORMAL:
                    default:
                        break;
                }
            }

            ProductPopup.OpenPopup(openData.INSTALL_TAG);
            return;
        }

        public virtual void OnClickLandmark()
        {
            switch (BTag) 
            {
                case (int)eLandmarkType.Dozer:
                    break;
                case (int)eLandmarkType.Travel:
                    break;
                case (int)eLandmarkType.SUBWAY:
                    break;
            }

            // 지하철 임시 처리
            //if (BTag == (int)eLandmarkType.SUBWAY)
            //{
            //    LandMarkPopup.OpenPopup(BTag);
            //    return;
            //}

            if (Data != null)
            {
                switch (Data.State)
                {
                    case eBuildingState.CONSTRUCTING://가속팝업
                        
                        if (Data.Level == 0)
                        {
                            AccelerationMainPopup.OpenPopup(eAccelerationType.CONSTRUCT, new BuildingPopupData(BTag));
                        }
                        else
                        {
                            AccelerationMainPopup.OpenPopup(eAccelerationType.LEVELUP, new BuildingPopupData(BTag));
                        }
                        return;
                    case eBuildingState.CONSTRUCT_FINISHED:
                        BuildCompleteEvent.Send(this, Data.State);
                        return;
                    case eBuildingState.LOCKED:
                        switch(BTag)
                        {
                            case (int)eLandmarkType.Dozer:
                                ToastManager.On(100002520);
                                break;
                            case (int)eLandmarkType.Travel:
                                ToastManager.On(100001242);
                                break;
                            case (int)eLandmarkType.SUBWAY:
                                int TownLvToSubwayOpen = 0;
                                var buildingData = BuildingOpenData.GetByInstallTag((int)eLandmarkType.SUBWAY);

                                if (buildingData != null)
                                    TownLvToSubwayOpen = buildingData.OPEN_LEVEL;

                                ToastManager.On(string.Format(StringData.GetStringByIndex(100000059), TownLvToSubwayOpen));
                                break;
                            case (int)eLandmarkType.EXCHANGE://의뢰소 해금 제약 조건
                                ToastManager.On(StringData.GetStringByIndex(100002834));
                                break;
                        }
                        return;
                    case eBuildingState.NONE:
                        return;
                    case eBuildingState.NOT_BUILT:
                        PopupManager.OpenPopup<BuildingConstructPopup>(new BuildingPopupData(BTag));
                        return;
                    case eBuildingState.NORMAL:
                    default:
                        //if(BTag == (int)eLandmarkType.EXCHANGE)
                        //    PopupManager.OpenPopup<ExchangePopup>();
                        //else
                        //{
                            LandMarkPopup.OpenPopup(BTag);
                            if(BTag ==(int) eLandmarkType.Travel)
                            {
                                PopupManager.Instance.Top.SetStaminaUI(true);
                            }
                        //}
                            
                        break;
                }
            }
        }

        public SkeletonAnimation GetSpine()
        {
            return spine;
        }

        void ClearLayerState()
        {
            spine?.gameObject.SetActive(true);
            cloudSpine?.gameObject.SetActive(false);
            shutterSpine?.gameObject.SetActive(false);
            npcSpine?.gameObject.SetActive(false);
            finishLayerObject?.SetActive(false);
            timeText?.gameObject.SetActive(false); 
            AddBuildProductUI();
        }


        public void ClearProduct()
        {
            if(BuildingProductUI != null)
                BuildingProductUI.Clear();
        }

        public virtual void CheckProductAlarm()
        {
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(curState);
            }

            if (Data == null || Data.State != eBuildingState.NORMAL)//기본적으로 NORMAL이 아닌경우에는 알람이 뜰 수 없음
                return;

            ProducesBuilding produces = User.Instance.GetProduces(Data.Tag);
            if (produces == null || produces.Items.Count == 0)
                return;
            
            BuildingOpenData buildingOpendData = BuildingOpenData.GetWithTag(Data.Tag);
            if (buildingOpendData == null)
                return;

            BuildingBaseData buildingInfo = buildingOpendData.BaseData;
            if (buildingInfo == null)
                return;

            List<ProductReward> products = new List<ProductReward>();
            if (buildingInfo.TYPE == 2)
            {
                ProductAutoData autoProductInfo = ProductAutoData.GetProductDataByGropuAndLevel(buildingInfo.KEY, Data.Level);
                ItemBaseData itemInfo = autoProductInfo.ProductItem.BaseData;

                int count = 0;
                int curExp = 0;
                var itemCount = produces.Items.Count;
                if (itemCount > 0)
                {
                    ProducesRecipe curItem = null;
                    foreach (var item in produces.Items)
                    {
                        count++;
                        if (item.State == eProducesState.Complete)
                        {
                            products.Add(new ProductReward(autoProductInfo.ProductItem));
                        }
                        else
                        {
                            curItem = item;
                            break;
                        }
                    }
                    
                    if (curItem != null)
                    {
                        if (curItem.ProductionExp > 0)
                        {
                            curExp = curItem.ProductionExp;
                        }

                        var datas = ProductAutoData.GetListByGroupAndLevel(BName, Data.Level);
                        if (datas != null)
                        {
                            var autoProductData = datas[0];
                            if (autoProductData != null)
                            {
                                if (TimeManager.GetTimeCompare(curExp) <= 0)
                                {
                                    while (autoProductData != null && produces.Slot > count && curExp > 0)
                                    {
                                        count++;
                                        products.Add(new ProductReward(autoProductInfo.ProductItem));

                                        curExp += Mathf.FloorToInt(autoProductData.MAX_TIME / produces.Slot);
                                        if (TimeManager.GetTimeCompare(curExp) > 0)
                                        {   
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                List<ProducesRecipe> itemList = produces.Items;
                if (itemList.Count > 0)
                {
                    int ProductionExp = 0;
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        ProducesRecipe produceReceipe = itemList[i];
                        if (produceReceipe == null) 
                        { 
                            continue; 
                        }

                        ProductionExp += produceReceipe.ProductionExp;

                        ProductData itemReceipe = ProductData.GetProductDataByGroupAndKey(buildingOpendData.BUILDING, produceReceipe.RecipeID);
                        if (itemReceipe == null)
                            continue;

                        if ((ProductionExp + i * itemReceipe.PRODUCT_TIME) <= TimeManager.GetTime())
                        {
                            products.Add(new ProductReward(itemReceipe.ProductItem));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetProducts(products);
            }
        }

        public void BlurCompleteUI()
        {
            if (BuildingProductUI != null)
            {
                BuildingProductUI.TouchEffectClear();
            }
        }

        public virtual void OnHarvest(eHarvestType harvestType)
        {
            List<Asset> ItemList = new List<Asset>();

            ProducesBuilding building = User.Instance.GetProduces(Data.Tag);            
            string index = BuildingOpenData.GetWithTag(Data.Tag).BUILDING;
            List<ProducesRecipe> queueList = building.Items;

            if (queueList != null)
            {
                List<int> item = new List<int>();
                for (int i = 0; i < queueList.Count; ++i)
                {
                    item.Clear();
                    if ((int)queueList[i].State == 3)
                    {
                        ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(index, queueList[i].RecipeID);
                        if (itemInfo == null)//인벤 체크 배터리 타입일 때 처리 따로 해야함
                            continue;

                        ItemList.Add(new Asset(itemInfo.ProductItem));
                    }
                }
            }

            if (User.Instance.CheckInventoryGetItem(ItemList))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => {
                        //PopupManager.GetPopup<MainPopup>().changeTab(4);
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }

            WWWForm data = new WWWForm();
            data.AddField("tag", BTag);
            data.AddField("auto", (int)harvestType);    //해당 건물에서만 생산된 물품 받기 플래그

            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.SendWithCAPTCHA("produce/harvest", data, (jsonObj) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonObj["rs"]))
                {
                    switch (jsonObj["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:                            
                            if ((eApiResCode)jsonObj["rs"].Value<int>() == eApiResCode.OK)
                            {
                                if (jsonObj.ContainsKey("rewards"))
                                {
                                    var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));
                                    InventoryIncomeEvent.Send(rewardList, BTag, harvestType);
                                    //SystemRewardPopup.OpenPopup(rewardList.ToList());
                                }
                            }

                            CheckProductAlarm();
                            break;

                        case (int)eApiResCode.INVENTORY_FULL:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                            () =>
                            {
                                //LoadingManager.ImmediatelySceneLoad("Town");
                                PopupManager.OpenPopup<InventoryPopup>();
                            },
                            () =>
                            {
                            },
                            () =>
                            {
                            });
                            //SBLog("가방이 가득 참")
                            //let popup = PopupManager.OpenPopup("SystemPopupOKCANCEL") as SystemPopup;
                            //popup.setMessage(StringTable.GetString(100000248, "알림"), StringTable.GetString(100002077, "아이템 획득에 필요한 가방 슬롯이 부족합니다.\n아이템을 획득하기 위해선\n가방에 빈 슬롯이 필요합니다.\n가방 메뉴로 이동 하시겠습니까?"));
                            //let okLabelNode = GetChild(popup.node,['body', 'Bot_bg', 'btnOk', 'layout', 'labelMsg']);
                            //okLabelNode.getComponent(Label).string = StringTable.GetString(100000414, "알림")
                            //            popup.setCallback(() => {
                            //            PopupManager.OpenPopup("MainPopup", true, { index: 4});
                            //popup.ClosePopup();
                            //},
                            //            () => {   //나가기
                            //                popup.ClosePopup();
                            //            },
                            //            () => {  //나가기
                            //                popup.ClosePopup();
                            //            });
                            break;

                        case (int)eApiResCode.NOTHING_TO_HARVEST:
                            ToastManager.On(100000812);
                            break;
                    }
                }
            },(string arg) =>
            {
                isNetworkState = false;
            });
        }

        // 모두 수확하기 (23.6.21 - 타운에서 획득 시 모두 수령)
        public void OnHarvestAll()
        {
            List<Asset> ItemList = new List<Asset>();
            List<ProducesBuilding> buildingList = User.Instance.GetAllProducesList(true);
            Dictionary<int, List<Asset>> buildingRewardDic = new();

            // 수령 가능한 생산품목 리스트 확인
            foreach (ProducesBuilding building in buildingList)
            {
                if (building.Items == null || building.Items.Count <= 0) continue;

                string buildingGroup = BuildingOpenData.GetWithTag(building.Tag).BUILDING;

                foreach (ProducesRecipe productItem in building.Items)
                {
                    if (productItem.State == eProducesState.Complete ||
                        (productItem.State == eProducesState.Ing && productItem.ProductionExp <= TimeManager.GetTime()))
                    {
                        ProductData itemInfo = ProductData.GetProductDataByGroupAndKey(buildingGroup, productItem.RecipeID);
                        ItemList.Add(new Asset(itemInfo.ProductItem));

                        if (buildingRewardDic.ContainsKey(building.Tag) == false)
                        {
                            buildingRewardDic.Add(building.Tag, new List<Asset>());
                        }
                        buildingRewardDic[building.Tag].Add(new Asset(itemInfo.ProductItem));
                    }
                }
            }

            // 수령 가능한 아이템이 없을 경우
            if (ItemList.Count <= 0)
            {
                ToastManager.On(100000812);
                return;
            }

            // 수령 가능한 생산품목 대비 인벤토리 체크
            if (User.Instance.CheckInventoryGetItem(ItemList))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                    () => {
                        //PopupManager.GetPopup<MainPopup>().changeTab(4);
                        PopupManager.OpenPopup<InventoryPopup>();
                    }
                );
                return;
            }
            
            // 생산 품목 일괄 수령
            WWWForm data = new WWWForm();
            data.AddField("tag", 0);    // 모두받기 태그 : 0
            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.SendWithCAPTCHA("produce/harvest", data, (jsonObj) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonObj["rs"]))
                {
                    switch (jsonObj["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            if (jsonObj.ContainsKey("rewards"))
                            {
                                var rewardList = SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonObj["rewards"]));

                                SendInventoryIncomeEvent(buildingRewardDic);
                                //InventoryIncomeEvent.Send(rewardList, Data.BuildingTag);
                                //SystemRewardPopup.OpenPopup(rewardList);        // 보상팝업 사용할 경우 활성화

                                // 레드닷 갱신
                                UIManager.Instance.MainPopupUI.RequestUpdateProductReddot();
                            }
                            break;

                        case (int)eApiResCode.INVENTORY_FULL:
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                                () =>
                                {
                                    PopupManager.OpenPopup<InventoryPopup>();
                                }
                            );
                            break;
                        case (int)eApiResCode.NOTHING_TO_HARVEST:
                            ToastManager.On(100000812);
                            break;
                    }
                }
            },
            (string arg) =>
            {
                isNetworkState = false;
            });
        }

        void SendInventoryIncomeEvent(Dictionary<int, List<Asset>> rewardDic)
        {
            foreach (var rewardInfo in rewardDic)
            {
                InventoryIncomeEvent.Send(rewardInfo.Value, rewardInfo.Key);
            }
        }

        protected virtual void SetProductState(ProductState state)
        {
            if (curState == state)
                return;

            curState = state;
            if (BuildingProductUI != null)
            {
                BuildingProductUI.SetState(curState);
            }
                        
            float timeScale = 1.0f;
            string anim = "off";
            bool loop = false;
            Color color = Color.white;
            switch(curState)
            {
                case ProductState.QUEUE_EMPTY:
                    anim = "off";
                    loop = true;
                    timeScale = 1.0f;
                    color = Color.gray;
                    break;
                case ProductState.COMPLETED_ALL:
                    if (spine.Skeleton.Data.FindAnimation("full") != null)
                        anim = "full";
                    else
                        anim = "off";
                    loop = true;
                    timeScale = 1.0f;
                    color = Color.gray;
                    UIObjectEvent.Event(UIObjectEvent.eEvent.PRODUCT_DONE, UIObjectEvent.eUITarget.LB);
                    break;
                case ProductState.RUNNING:
                    anim = "play";
                    loop = true;
                    timeScale = 1.0f;
                    color = Color.white;
                    break;

                case ProductState.UNKNOWN:                
                default:
                    anim = "off";
                    loop = false;
                    timeScale = 0.0f;
                    color = Color.gray;
                    break;
            }

            spine.timeScale = timeScale;
            //spine.Skeleton.SetColor(color);
            SetAnimation(0, anim, loop);

            BuildingUIEvent.RefreshSpine();//생산 및 랜드마크 스파인 갱신
        }
        protected void AddBuildProductUI()
        {
            if (BuildingProductUI == null)
            {
                GameObject ui = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "BuildingProductUI"), finishLayerObject.transform.parent);
                if (ui != null)
                {
                    BuildingProductUI = ui.GetComponent<BuildingProductUI>();
                    BuildingProductUI.SetParentBuilding(this);
                }
            }
        }

        public virtual bool TryHarvest(eHarvestType harvestType)
        {
            if (BuildingProductUI != null)
            {
                return BuildingProductUI.TryHarvest(harvestType);
            }

            return false;
        }

        public BuildingProductUI GetProductUI()
        {
            return BuildingProductUI;
        }

        public virtual void OnTouchAction()
        {
            if (spine != null)
            {
                if (DOTween.IsTweening(spine))
                    return;

                Color origin = spine.Skeleton.GetColor();
                spine.Skeleton.SetColor(Color.gray);
                DOTween.To(spine.Skeleton.GetColor, spine.Skeleton.SetColor, origin, 1f);
            }
        }
    }
}