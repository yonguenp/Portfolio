using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ConstructInfoData : ITableData
    {
        public BuildingBaseData buildingBase { get; private set; } = null;
        public BuildingOpenData openData { get; private set; } = null;
        public eBuildingState eBuildingState { get; private set; } = eBuildingState.NONE;
        public string Name { get { return StringData.GetStringByStrKey(buildingBase._NAME); } }
        public string KEY { get { return buildingBase.KEY; } }
        public void Init() { }
        public string GetKey() { if (buildingBase == null) return ""; return buildingBase.GetKey(); }

        public ConstructInfoData(string key)
        {
            buildingBase = BuildingBaseData.Get(key);
            openData = BuildingOpenData.GetAvailTotalBuilding(buildingBase.KEY);
            if (openData == null)
            {
                eBuildingState = eBuildingState.NONE;
                return;
            }

            if (User.Instance.GetAreaLevel() >= openData.OPEN_LEVEL)
            {
                BuildInfo userBuilding = User.Instance.GetUserBuildingInfoByTag(openData.INSTALL_TAG);
                if (userBuilding != null)
                {
                    eBuildingState = userBuilding.State;
                }
                else
                {
                    eBuildingState = eBuildingState.NOT_BUILT;
                }
            }
            else
            {
                eBuildingState = eBuildingState.LOCKED;
            }
        }
    }

    public class BuildingConstructListPopup : Popup<BuildingConstructListData>
    {
        List<BuildingBaseData> BuildingList { get { return BuildingBaseData.GetProductBuildingList(); } }        // 전체 건물 정보 리스트

        [SerializeField]
        ScrollRect scrollView = null;
        public Transform CardParent { get { return scrollView.content; } }
        [SerializeField]
        GameObject buildingCard = null;
        [SerializeField]
        Text toggleText = null;
        [SerializeField]
        Image btnImage = null;
        [SerializeField]
        Sprite btnToggleOn = null;
        [SerializeField]
        Sprite btnToggleOff = null;
        List<ConstructInfoData> viewCards = new List<ConstructInfoData>();//정렬 데이터

        private bool viewDirty = true;

        bool isTutorial = false;
        bool viewAll = false;

        List<BuildingCard> cards = new List<BuildingCard>();
        #region OpenPopup
        public static BuildingConstructListPopup OpenPopup(BuildingConstructListData data)
        {
            return PopupManager.OpenPopup<BuildingConstructListPopup>(data);
        }
        #endregion
        public override void InitUI()
        {
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
            SetSubCamTexture();
            SetBuildingCardData();
            isTutorial = TutorialManager.tutorialManagement.IsPlayingTutorial;

            DrawScrollView();
        }

        void SetSubCamTexture()
        {
            Town.Instance.SetSubCamState(true);
            UICanvas.Instance.StartBackgroundBlurEffect();
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
        public override void ClosePopup()
        {
            Town.Instance.SetSubCamState(false);
            UICanvas.Instance.EndBackgroundBlurEffect();
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
            base.ClosePopup();
        }

        void RefreshBuildingLayer()
        {
            viewDirty = true;

            DrawScrollView();
        }

        // 빌딩 카드 목록을 생성
        void SetBuildingCardData()
        {
            if (BuildingList == null || BuildingList.Count <= 0) { return; }

            if (viewCards == null)
            {
                viewCards = new List<ConstructInfoData>();
            }

            viewCards.Clear();
            viewCards = Data.ConstructList;

            //현재 나의 건설 레벨 이하(건설 가능한 것)만 표시
            viewCards = viewCards.FindAll(element => element.openData != null && element.openData.OPEN_LEVEL <= User.Instance.TownInfo.AreaLevel).ToList();
            viewCards = viewCards.OrderByDescending(building => building.eBuildingState == eBuildingState.NOT_BUILT)
                 .ThenByDescending(building => building.eBuildingState == eBuildingState.CONSTRUCT_FINISHED)
                 .ThenByDescending(building => building.eBuildingState == eBuildingState.LOCKED)
                 .ThenByDescending(building => building.eBuildingState == eBuildingState.NORMAL)
                 .ThenBy(building => building.KEY == "exp_battery")//베터리는 최후방에
                 .ToList();

            viewDirty = true;
        }

        public void DrawScrollView()
        {
            cards.Clear();
            SBFunc.RemoveAllChildrens(scrollView.content.transform);

            if (!viewDirty || viewCards == null)
            {
                return;
            }

            if (isTutorial)
            {
                int buildKey = TutorialManager.tutorialManagement.GetCurTutoPrivateKey();
                viewCards.RemoveAll(dat => dat.openData.INSTALL_TAG != buildKey);
            }

            toggleText.transform.parent.gameObject.SetActive(!isTutorial);

            foreach (var viewItem in viewCards)
            {
                var buildingCardObj = Instantiate(buildingCard, scrollView.content);
                var buildingCardData = buildingCardObj.GetComponent<BuildingCard>();
                if(buildingCardData != null)
                {
                    buildingCardData.InitBuildingCard(Data, viewItem, ()=> { ClosePopup(); });
                    cards.Add(buildingCardData);
                }
                else
                {
                    Destroy(buildingCardObj);
                }
            }

            if(!isTutorial)
            {
                var list = BuildingBaseData.GetProductBuildingList();
                foreach (var baseData in list)
                {
                    bool contain = false;
                    foreach(var viewItem in viewCards)
                    {
                        if(viewItem.buildingBase == baseData)
                        {
                            contain = true;
                            break;
                        }
                    }

                    if (contain)
                        continue;

                    var buildingCardObj = Instantiate(buildingCard, scrollView.content);
                    var buildingCardData = buildingCardObj.GetComponent<BuildingCard>();
                    if (buildingCardData != null)
                    {
                        buildingCardData.InitDisableBuildingCard(Data, baseData);
                        cards.Add(buildingCardData);
                    }
                    else
                    {
                        Destroy(buildingCardObj);
                    }
                }
            }

            scrollView.horizontalNormalizedPosition = 0f;
            scrollView.enabled = true;

            viewDirty = false;

            OnViewSettring(false);
        }

        public void OnToggleView()
        {
            OnViewSettring(!viewAll);
        }

        void OnViewSettring(bool all)
        {
            viewAll = all;

            btnImage.gameObject.SetActive(!isTutorial);
            if (viewCards.Count == 0)
            {
                viewAll = true;
                if(!all)
                {
                    ToastManager.On(StringData.GetStringByStrKey("건설가능건물없음"));
                    btnImage.gameObject.SetActive(false);
                }
            }

            toggleText.text = viewAll ? StringData.GetStringByStrKey("건설가능보기") : StringData.GetStringByStrKey("전체건설보기");
            btnImage.sprite = viewAll ? btnToggleOff : btnToggleOn;
            if (viewAll)
            {
                foreach(var card in cards)
                {
                    card.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (var card in cards)
                {
                    if(card.curConstructData == null)
                        card.gameObject.SetActive(false);
                }
            }
        }


        //const int MIDDLE_ALIGN_COUNT = 3;
        //void SetMiddleAlignByItemCount()//데이터 크기에 따라서 중앙 정렬 요청 - 중앙 정렬을 기본으로 해놓고 initUI에서 시작시 맨왼쪽으로 미는걸로
        //{
        //    bool isAlign = viewCards.Count >= MIDDLE_ALIGN_COUNT;
        //    var pos = Vector2.zero;
        //    var minVec = isAlign ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0.5f);
        //    var maxVec = isAlign ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0.5f);
        //    var pivot = isAlign ? new Vector2(0, 0.5f) : new Vector2(0.5f, 0.5f);

        //    scrollView.content.GetComponent<RectTransform>().anchorMin = minVec;
        //    scrollView.content.GetComponent<RectTransform>().anchorMax = maxVec;
        //    scrollView.content.GetComponent<RectTransform>().pivot = pivot;
        //    scrollView.content.GetComponent<RectTransform>().anchoredPosition = pos;
        //}
    }
}
