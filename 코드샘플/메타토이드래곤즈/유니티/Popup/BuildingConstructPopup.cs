using DG.Tweening;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class BuildingConstructPopup : Popup<BuildingPopupData>
    {
        public GameObject emptyReceipeItemLayerObject = null;
        public ScrollRect receipeScrollRect = null;
        public Button constructButton = null;

        [SerializeField] private GameObject buildingSpineParent = null;

        public Image buildingImage = null;
        public Text buildingNameText = null;
        public Text buildingDescText = null;

        public GameObject priceNode = null;
        public Image priceTypeImage = null;
        public Text priceText = null;

        public Text timeText = null;

        [SerializeField] private Image ItemBackImg = null;

        [SerializeField] private Color noneItemNeedBackColor = Color.white;

        bool isSufficientItem = true;
        bool isSufficientPrice = true;

        bool isNetworkState = false;
        
        public override void ForceUpdate(BuildingPopupData data)
        {
            base.DataRefresh(data);
            Refresh();
        }

        public override void InitUI()
        {
            UICanvas.Instance.StartBackgroundBlurEffect();
            InitPopup();
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            UICanvas.Instance.EndBackgroundBlurEffect();
        }
        protected override IEnumerator OpenAnimation()
        {
            dimClose = false;
            InitUI();
            dimClose = true;
            yield break;
        }
        protected override IEnumerator CloseAnimation()
        {
            SetActive(false);
            yield break;
        }

        public void OnClickConstructButton()
        {
            // 재료 체크
            if(!isSufficientPrice)
            {
                ToastManager.On(100002520);
                return;
            }

            // 외형레벨 조건 체크
            if (User.Instance.ExteriorData.ExteriorLevel < Data.OpenData.OPEN_LEVEL)
            {
                ToastManager.On(100000059, Data.OpenData.OPEN_LEVEL);
                return;
            }

            if (!isSufficientItem)
            {
                var needItemList = SBFunc.GetNeedItemList(Data.LevelData.NEED_ITEM);
                if (needItemList.Count <= 0)
                {
                    ToastManager.On(100002520);
                    return;
                }

                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                    if (Data.BaseData.IS_LANDMARK == false) {
                        
                        if (PopupManager.IsExistPopup(typeof(BuildingConstructListPopup)))
                        {
                            PopupManager.GetPopup<BuildingConstructListPopup>().InitUI();
                        }
                    }
                     
                    RefreshItemUI();
                });
                return;
            }

            if(Data.BaseData.IS_LANDMARK)//코인도저, 여행사, 지하철 이면
            {
				WWWForm data = new WWWForm();
                data.AddField("tag", Data.BuildInfo.Tag);
                if (isNetworkState)
                {
                    return;
                }
                isNetworkState = true;
				NetworkManager.Send("building/construct", data, (jsonData) =>
                {
                    isNetworkState = false;
                    PopupManager.ForceUpdate<LandMarkPopup>();
                    ClosePopup();
                    Town.Instance.RefreshMap();
                },
                (string arg) =>
                {
                    isNetworkState = false;
                });
                return;
            }

            Camera.main.targetTexture = null;
            Town.Instance.SetSubCamState(false);

            if (Data.TargetCell != Vector2Int.zero)
            {
                WWWForm paramData = new WWWForm();
                paramData.AddField("tag", Data.OpenData.INSTALL_TAG);
                paramData.AddField("x", Data.TargetCell.y);
                paramData.AddField("y", Data.TargetCell.x);
                if (isNetworkState)
                {
                    return;
                }
                isNetworkState = true;
                NetworkManager.Send("building/construct", paramData, (NetworkManager.SuccessCallback)((jsonData) =>
                {
                    isNetworkState = false;
                    if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                    {
                        switch (jsonData["rs"].Value<int>())
                        {
                            case (int)eApiResCode.OK:

                                Town.Instance.RefreshMap();
                                Town.Instance.SetConstructModeState((bool)false);
                                
                                UIManager.Instance.RefreshCurrentUI();

                                break;
                        }
                    }
                }), (string arg) =>
                {
                    isNetworkState = false;
                });
            }
            else
            {
                Town.Instance.ConstructTag = Data.OpenData.INSTALL_TAG;
                Town.Instance.SetConstructModeState(true);
            }

            PopupManager.AllClosePopup();
        }

        public void OnClickCloseButton()
        {
            PopupManager.ClosePopup<BuildingConstructPopup>();
        }

        void InitPopup()
        {
            if (Data == null) { return; }
            if (Data.LevelData == null) { return; }

            isNetworkState = false;
            // 기본 UI 데이터 세팅
            buildingNameText.text = StringData.GetStringByStrKey(Data.BaseData._NAME);
            buildingDescText.text = StringData.GetStringByStrKey(Data.BaseData._DESC);
            SetBuildingSpine();//spine으로 적용

            if (Data.LevelData.UPGRADE_TIME <= 0)
                timeText.text = StringData.GetStringByStrKey("town_completion");
            else
                timeText.text = SBFunc.TimeString(Data.LevelData.UPGRADE_TIME);

            if(priceNode != null)
                priceNode.SetActive(Data.LevelData.COST_TYPE != "NONE");
            priceTypeImage.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, Data.LevelData.COST_TYPE);
            priceText.text = SBFunc.CommaFromNumber(Data.LevelData.COST_NUM);

            RefreshItemUI();
        }

        void RefreshItemUI()
        {
            SBFunc.RemoveAllChildrens(receipeScrollRect.content);
            isSufficientItem = true;
            // 재료 관련 레이어 세팅
            foreach (var needItem in Data.LevelData.NEED_ITEM)
            {
                GameObject newItem = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"));
                newItem.transform.SetParent(receipeScrollRect.content.transform);
                newItem.transform.localScale = Vector3.one;

                ItemFrame itemframe = newItem.GetComponent<ItemFrame>();
                if (itemframe != null)
                {
                    itemframe.setFrameRecipeInfo(needItem.ItemNo, needItem.Amount);

                    isSufficientItem = isSufficientItem && itemframe.IsSufficientAmount;
                }
            }

            emptyReceipeItemLayerObject.SetActive(Data.LevelData.NEED_ITEM.Count <= 0);
            ItemBackImg.color = Data.LevelData.NEED_ITEM.Count > 0 ? Color.white : noneItemNeedBackColor;
            // 보유 재화 확인
            switch (Data.LevelData.COST_TYPE)
            {
                case "GOLD":
                    isSufficientPrice = User.Instance.GOLD >= Data.LevelData.COST_NUM;
                    break;
                case "GEMSTONE":
                    isSufficientPrice = User.Instance.GEMSTONE >= Data.LevelData.COST_NUM;
                    break;
            }

            // 버튼 상태 갱신
            if (constructButton != null)
            {
                constructButton.SetButtonSpriteState(isSufficientItem && isSufficientPrice);
            }

            if (priceText != null)
                priceText.color = isSufficientPrice ? Color.white : Color.red;
        }

        void SetBuildingSpine()
        {
            SBFunc.RemoveAllChildrens(buildingSpineParent.transform);
            var buildingUISpinePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.BuildingUIClonePath, Data.BaseData.KEY.ToString());
            if (buildingUISpinePrefab != null)
            {
                var spineObj = Instantiate(buildingUISpinePrefab, buildingSpineParent.transform);
                var spineSkeleton = spineObj.GetComponentInChildren<SkeletonGraphic>();
                if (spineSkeleton != null)
                {
                    spineSkeleton.Clear();

                    if (GetAnimation(spineSkeleton, "play"))
                        spineSkeleton.AnimationState.SetAnimation(0, "play", false);
                    else if (GetAnimation(spineSkeleton, "idle"))//의뢰소만 idle로 나온건지 확인 후에 처리해야함. - 일단 터지지않게 수정
                        spineSkeleton.AnimationState.SetAnimation(0, "idle", false);

                    spineSkeleton.timeScale = 0f;
                }
            }
        }

        bool GetAnimation(SkeletonGraphic spineData, string _animationName)
        {
            if (spineData.SkeletonData.FindAnimation(_animationName) != null)
                return true;
            else
                return false;
        }

        void Refresh() { }

    }
}
