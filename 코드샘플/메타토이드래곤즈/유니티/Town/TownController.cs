using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SandboxNetwork
{
    public class TownController : MonoBehaviour, EventListener<BuildCompleteEvent>
    {
        [SerializeField]
        Town parentTown = null;
        [SerializeField]
        GameObject townBase = null;
        
        public Button townZoomFinishLayer = null; // 건설 완료 연출이후에 가능한 건설완료를 누를수 밖에 없도록 되어 있지만
                                                // 극히 일부 기종에서는 옆 건물를 터치 가능, 그런 기종을 고려하면 다른 기종에서는 이상하게 보여서 옆 레이어 터치 막기 위한 용도
                                                  // 또한 여러 줌 상태를 종료 하기 위한 용도      

        /* 건설 관련 */
        bool isConstructMode = false;

        Vector2Int? constructFloorCellVec2 = null;  // 월드 좌표로 얻은 floor/cell 정보를 담은 vec2

        GameObject newBuilding = null;          // 미리 표시되는 건물 오브젝트
        GameObject newControlUI = null;         // 건설 진행 여부(O/X) UI 오브젝트

        public int CurTag { get; private set; } = 0;                 // 현재 건설하려는 빌딩의 태그
        bool isActiveCell = false;         //현재 클릭한 층이 활성화 Cell인가
        bool isLockedFloor = false;         //현재 클릭한 층이 잠겨있나?

        /* 건물 위치 변경 관련 */
        bool isBuildingSwitchMode = false;
        bool isSelectBuilding = false;

        Vector2Int? originFloorCellVec2 = null;     // 이동 대상 건물 좌표
        Vector2Int? destFloorCellVec2 = null;       // 이동 목적지 건물 좌표

        GameObject selectedMarkerUI = null;              // 건물 선택 표시 UI 오브젝트

        List<GameObject> markerSpaceList = new List<GameObject>();            // 건설 가능 표시 UI 오브젝트

        /* 공용 */
        Vector3 currentInputPos = Vector3.zero;             // 최초 클릭 시 마우스 좌표
        Vector3 currentInputWorldPos = Vector3.zero;        // 마우스 좌표의 월드 좌표

        EventSystem _eventSystem = null;
        private bool isNetworkState = false;
        private void Start()
        {
            ClearData();
            EventManager.AddListener(this);
            if (_eventSystem == null)
                _eventSystem = EventSystem.current;
            isNetworkState = false;
        }
        private void OnDestroy()
        {
            EventManager.RemoveListener(this);
            ClearData();
        }

        private void Update()
        {
            if (isConstructMode)
                ConstructModeInputCheck();
            else if (!isConstructMode && !isBuildingSwitchMode)
                ConstructionTargetClick();

            if (isBuildingSwitchMode)
                BuildingSwitchModeInputCheck();
        }

        #region 건물 건설 관련
        public bool IsConstructModeState()
        {
            return isConstructMode;
        }
        public void SetConstructModeState(bool state)
        {
            if (parentTown.IsCamDragonFollow)
            {
                parentTown.CameraUnFollowObject();
            }
            isConstructMode = state;

            if (isConstructMode)
            {
                UIManager.Instance.UIEditTown.SetEditMessege(StringData.GetStringByIndex(100000735));
                //UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.LB);
                CameraZoomForBuildingEdit();
                StartConstructMode();
            }
            else
            {
				UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
				EndConstructMode();
            }
        }

        void StartConstructMode()
        {
            if (isConstructMode == false) { return; }
            UIManager.Instance.InitUI(eUIType.TownEdit);
            CreateWhiteMarkerObject();
            //ToastManager.Set(StringTable.GetString(100000735));

            CurTag = Town.Instance.ConstructTag;
        }

        void EndConstructMode()
        {
            if (isConstructMode) { return; }
            ClearMarkerObject();
            ClearData();
            CameraZoomClear();

            UIManager.Instance.InitUI(eUIType.Town);
        }

        void ConstructionTargetClick()
        {
            bool uitarget = _eventSystem.currentSelectedGameObject == null;

            if (!uitarget)
            {
                return;
            }

            var panCam = Camera.main.GetComponent<ProCamera2DPanAndZoomCustom>();
            if(panCam == null)
            {
                return;
            }

            if(parentTown == null)
            {
                return;
            }

            if (PopupManager.OpenPopupCount > 0)
            {
                return;
            }

            if (Town.Instance.IsIntroMode)
            {
                return;
            }


            if (panCam.IsPanning)
            {
                CurTag = 0;
                isActiveCell = false;
                isLockedFloor = false;

                Town.Instance.ClearBuildingProductEffect();
            }

            if(Input.touchCount >= 2)
            {
                CurTag = 0;
                isActiveCell = false;
                isLockedFloor = false;
            }

            //drag 시작한 순간부터 panCam.IsPanning true로 바뀜
            if (Input.GetMouseButtonDown(0) && !panCam.IsPanning)
            {
                currentInputPos = Input.mousePosition;
                currentInputWorldPos = Camera.main.ScreenToWorldPoint(currentInputPos);

                Vector2Int newCellPos = TownMap.GetCellPosByInputPosition(currentInputWorldPos);

                var floor = newCellPos.x;
                var cell = newCellPos.y;

                //잠긴 층인지 선체크
                var isGemdungeonCell = parentTown.IsGemDungeonCell(floor, cell);
                var isGuildCell = parentTown.IsGuildPos(cell, currentInputWorldPos);

                isActiveCell = parentTown.isActiveCell(floor, cell) || isGemdungeonCell || isGuildCell;
                isLockedFloor = parentTown.isLockedFloor(floor, cell);

                if (isActiveCell)
                {
                    SoundManager.Instance.PlaySFX("sfx_btn_1");

                    var gridData = User.Instance.ExteriorData.ClickGrid;

					if (gridData.ContainsKey(floor))
					{
						var cellTagSet = gridData[floor];

						if (cellTagSet.ContainsKey(cell))
						{
							CurTag = cellTagSet[cell];

                            //Debug.Log("buildingDetect tag : " + tag + "  x : " + floor + "   y :  " + cell);
                            var building = parentTown.GetBuilding(CurTag);
                            if (building != null)
                                building.OnTouchAction();
                        }
                        else
                        {
                            CurTag = -1;
                            if(isLockedFloor)
                            {
                                Town.Instance.OnFoorTouchAction(floor);
                            }
                        }
					}
                    else if(isGemdungeonCell)
                    {
                        CurTag = (int)eLandmarkType.GEMDUNGEON;
                    }
                    else if (isGuildCell)
                    {
                        CurTag = (int)eLandmarkType.GUILD;
                    }
				}
            }

            if(Input.GetMouseButtonUp(0))
            {
                if (isActiveCell)
                {
                    if (CurTag > 0)
                    {
                        var building = parentTown.GetBuilding(CurTag);
                        if (building != null)
                        {
                            eHarvestType harvestType = building.BName == "exp_battery" ? eHarvestType.GET_BUILDING_TYPE : eHarvestType.GET_BUILDING;    // 23.5.12 - 타운맵에서 배터리 수거는 일괄획득이 디폴트
                            if (building.TryHarvest(harvestType))
                            {
                                CurTag = 0;
                                return;
                            }
                        }

                        Vector2Int newCellPos = TownMap.GetCellPosByInputPosition(currentInputWorldPos);

                        var floor = newCellPos.x;

                        switch (CurTag)
                        {
                            case (int)eLandmarkType.Dozer:
                                parentTown.dozer.OnClickLandmark();
                                break;
                            case (int)eLandmarkType.Travel:
                                parentTown.travel.OnClickLandmark();
                                break;
                            case (int)eLandmarkType.SUBWAY:
                                //베타 기준 버튼 클릭 막음
                                parentTown.train.OnClickLandmark();
                                break;
                            case (int)eLandmarkType.GEMDUNGEON:
                                if(parentTown.Gemdungeon.ContainsKey(floor))
                                {
                                    parentTown.Gemdungeon[floor].OnClickLandmark();
                                }
                                break;
                            case (int)eLandmarkType.EXCHANGE:
                                parentTown.exchangeCenter.OnClickLandmark();
                                break;
                            case (int)eLandmarkType.GUILD:
                                if (parentTown.guildBuilding != null)
                                    parentTown.guildBuilding?.OnClickLandmark();
                                break;
                            default:
                                if (building != null)
                                {
                                    building.OnClickBuilding();
                                }
                                break;
                        }

                        CurTag = 0;
                    }
                    else if (CurTag < 0)
                    {
                        Vector2Int cellPos = TownMap.GetCellPosByInputPosition(currentInputWorldPos);

                        var openBuildingData = new BuildingConstructListData(cellPos);
                        var isAvailableOpen = openBuildingData.IsAvailableOpen();
                        if (isAvailableOpen)
                            BuildingConstructListPopup.OpenPopup(openBuildingData);
                        else//불가능 팝업 연결 - 해당레벨에 건물이 다 지음 & 그래도 빈 땅이 있을 경우
                        {
                            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002193));
                        }
                        CurTag = 0;
                    }
                }
                else if(!isActiveCell && isLockedFloor)
                {
                    var currentUserExterior = User.Instance.ExteriorData;
                    if(currentUserExterior != null)
                    {
                        var currentState = currentUserExterior.ExteriorState;
                        if(currentState == eBuildingState.CONSTRUCTING || currentState == eBuildingState.CONSTRUCT_FINISHED)//건설 중일 때나 완료 상태 일때
                        {
                            ToastManager.On(100002531);
                        }
                        else
                        {
                            var mainpop = PopupManager.OpenPopup<TownManagePopup>();// 외형탭으로 이동
                            if (mainpop != null)
                            {
                                mainpop.OnClickFloorExtensionButton();
                            }
                        }
                        isActiveCell = false;
                        isLockedFloor = false;
                    }
                }
            }
        }

        void ConstructModeInputCheck()
        {
            bool uitarget = _eventSystem.currentSelectedGameObject == null;

            if (!uitarget)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                currentInputPos = Input.mousePosition;
                currentInputWorldPos = Camera.main.ScreenToWorldPoint(currentInputPos);

                //constructFloorCellVec2 = TownMap.GetCellPosByInput(currentInputWorldPos);
                Vector2Int newCellPos = TownMap.GetCellPosByInputPosition(currentInputWorldPos);

                // 중복 지역 클릭 체크 (최초 빈 셀 클릭 이거나, 현재와 다른 셀을 클릭한 경우)
                if ((constructFloorCellVec2.HasValue == false) || (constructFloorCellVec2.HasValue && constructFloorCellVec2.Value != newCellPos))
                {
                    constructFloorCellVec2 = newCellPos;
                    CreatePreConstructImage();
                }
                else
                {
                    return;
                }
            }

            // temp.. 임시 취소처리

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(1))
            {
                SetConstructModeState(false);
            }
#endif
        }

        void CreatePreConstructImage()
        {
            if (townBase == null || parentTown == null) { return; }
            if (constructFloorCellVec2.HasValue == false) { return; }

            // 확장하지 않은 층에 건설을 시도할 경우 처리
            if (constructFloorCellVec2.Value.x >= User.Instance.ExteriorData.ExteriorFloor)
            {
                ToastManager.On(100000742);
                return;
            }

            bool isActiveCell = parentTown.isActiveCell(constructFloorCellVec2.Value.x, constructFloorCellVec2.Value.y);
            if(!isActiveCell)
            {
                ToastManager.On(100000742);
                return;
            }

            // 해당위치에 이미 건물이 있는 경우 처리
            int buildingTag = User.Instance.GetBuildingTagInGridData(constructFloorCellVec2.Value.x, constructFloorCellVec2.Value.y);
            if (buildingTag > 1)    
            {
                ToastManager.On(100000741);
                return;
            }
            else if(buildingTag < 0 || buildingTag == 1)
            {
                ToastManager.On(100000742);
                return;
            }

            ClearObject();
            CreateWhiteMarkerObject(true);

            BuildingOpenData openData = BuildingOpenData.GetWithTag(CurTag);

            Vector2 cellPos = TownMap.GetBuildingPos(constructFloorCellVec2.Value.y, constructFloorCellVec2.Value.x);

            if (openData != null)
            {
                GameObject buildingPrefab = TownFactory.GetBuilding(openData.BUILDING);
                bool isTutorial = TutorialManager.tutorialManagement.IsPlayingTutorial;
                GameObject constructionClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "ConstructionClone");

                if (buildingPrefab != null && constructionClonePrefab != null)
                {
                    // 빌딩 생성
                    newBuilding = Instantiate(buildingPrefab, townBase.transform);
                    newBuilding.transform.localPosition = cellPos;

                    // 빌딩 건설 제어 UI 생성
                    newControlUI = Instantiate(constructionClonePrefab, townBase.transform);
                    newControlUI.transform.localPosition = cellPos;

                    ConstructionClone cloneScript = newControlUI.GetComponent<ConstructionClone>();
                    if (cloneScript != null)
                    {
                        cloneScript.InitClone(CurTag, constructFloorCellVec2.Value.x, constructFloorCellVec2.Value.y,()=> {
                            BuildingConstructListPopup.OpenPopup(new BuildingConstructListData());
                        }, isTutorial);

                        ////하얀색 건설 가능 이펙트 끄기 - 현재는 없지만 취소 버튼 누르면 다시 건설모드 돌입 및 하얀색 이펙트 켜줘야 할수도 있으니 setActive false 함
                        //for (int i = 0, cnt = markerSpaceList.Count; i < cnt; ++i)
                        //{
                        //    markerSpaceList[i].SetActive(false);
                        //}
                    }
                }
            }
        }

        public void CreatePreConstruction(int buildingTag, int x, int y)
        {
            if (townBase == null || parentTown == null) { return; }
            ClearObject();
            //CreateWhiteMarkerObject(true);

            BuildingOpenData openData = BuildingOpenData.GetWithTag(buildingTag);

            Vector2 cellPos = TownMap.GetBuildingPos(x, y);

            if (openData != null)
            {
                GameObject buildingPrefab = TownFactory.GetBuilding(openData.BUILDING);
                bool isTutorial = TutorialManager.tutorialManagement.IsPlayingTutorial;
                GameObject constructionClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "ConstructionClone");

                if (buildingPrefab != null && constructionClonePrefab != null)
                {
                    // 빌딩 생성
                    newBuilding = Instantiate(buildingPrefab, townBase.transform);
                    newBuilding.transform.localPosition = cellPos;

                    // 빌딩 건설 제어 UI 생성
                    newControlUI = Instantiate(constructionClonePrefab, townBase.transform);
                    newControlUI.transform.localPosition = cellPos;

                    ConstructionClone cloneScript = newControlUI.GetComponent<ConstructionClone>();
                    if (cloneScript != null)
                    {
                        cloneScript.InitClone(buildingTag, x, y, () => {
                            BuildingConstructListPopup.OpenPopup(new BuildingConstructListData());
                        }, isTutorial);

                    }
                }
            }
        }



        #endregion
        #region 건설완료 연출
        public void OnEvent(BuildCompleteEvent eventType)
        {
            if (this == null) return;

            var cam = Camera.main.GetComponent<ProCamera2D>();

            switch (eventType.eType)
            {
                case eBuildingState.CONSTRUCT_FINISHED:
                    PopupTopUIRefreshEvent.Hide();
                    townZoomFinishLayer.transform.parent.SetAsLastSibling();
                    townZoomFinishLayer.gameObject.SetActive(true);
                    townZoomFinishLayer.onClick.RemoveAllListeners();
                    //townZoomFinishLayer.onClick.AddListener(eventType.building.OnClickFinishButton);
                    UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_HIDE, UIObjectEvent.eUITarget.ALL);
                    
                    if (eventType.building != null)
                    {
                        eventType.building.ClearProduct();

                        if (eventType.building.BTag == (int)eLandmarkType.SUBWAY)
                        {
                            parentTown.ZoomToTarget(eventType.building.transform, 0.2f, 1 / 5f, Vector3.up * SBDefine.FloorSpancing * 1f, () => {
                                eventType.building.OnClickFinishButton();
                                //                        parentTown.ZoomEndProcess(false);
                            }, 0.1f);
                        }
                        else
                        {
                            parentTown.ZoomToTarget(eventType.building.transform, 0.2f, 1 / 3f, Vector3.up * SBDefine.FloorSpancing * 0.6f, () => {
                                eventType.building.OnClickFinishButton();
                                //                        parentTown.ZoomEndProcess(false);
                            }, 0.1f);
                        }
                    }

                    break;
                case eBuildingState.NORMAL:
                     townZoomFinishLayer.gameObject.SetActive(false);
                //    targetBuildingPos = Vector3Int.zero;
                //    parentTown.ZoomEndProcess(false);
                //    UIEffectEvent.Event(UIEffectEvent.eEvent.EFFECT_HIDE, UIEffectEvent.eUIEffect.FIRE_WORK);
                     break;
            }
        }

        
        

        //IEnumerator CamMoveAndEffectOrigin(Transform camTr, Vector3 buildingPos)
        //{
        //    float fixedY = SBDefine.FloorSpancing * 0.6f;
        //    Vector3 TargetPos = new Vector3(buildingPos.x, buildingPos.y + fixedY, -10);
        //    yield return CamMove(camTr, TargetPos, .3f);
        //    UIEffectEvent.Event(UIEffectEvent.eEvent.EFFECT_SHOW, UIEffectEvent.eUIEffect.FIRE_WORK);
        //}

      
        // 카메라 이동 파트
        //IEnumerator CamMove(Transform camTr, Vector3 TargetPos,float time)
        //{
        //    float elapsed_time = 0.0f;
        //    Vector3 startPosition = camTr.position;
        //    while (elapsed_time < time)
        //    {
        //        elapsed_time += Time.deltaTime;
        //        float progress = Mathf.Clamp01(elapsed_time / time);
        //        camTr.position = Vector3.Lerp(startPosition, TargetPos, progress);
        //        yield return null;
        //    }
        //    camTr.position = TargetPos;
        //    yield return null;
        //}

        #endregion
        #region 건물 위치 변경 관련

        public bool IsBuildingEditModeState()
        {
            return isBuildingSwitchMode;
        }

        public void SetBuildingEditModeState(bool state)
        {
            isBuildingSwitchMode = state;

            if (isBuildingSwitchMode)
                StartSwitchMode();
            else
                EndSwitchMode();
            
        }

        void StartSwitchMode()
        {
            if (isBuildingSwitchMode == false) 
                return;
            if (parentTown.IsCamDragonFollow)
                parentTown.CameraUnFollowObject();
            
            UIManager.Instance.InitUI(eUIType.TownEdit);
            UIManager.Instance.UIEditTown.SetEditMessege(StringData.GetStringByIndex(100002678));
            CameraZoomForBuildingEdit();
            CreateWhiteMarkerObject(false);            
        }

        void EndSwitchMode()
        {
            if (isBuildingSwitchMode) 
                return;

            ClearData();
            ClearMarkerObject();
            CameraZoomClear();

            UIManager.Instance.InitUI(eUIType.Town);
            UIObjectEvent.Event(UIObjectEvent.eEvent.EVENT_SHOW, UIObjectEvent.eUITarget.ALL);
        }
        void CameraZoomForBuildingEdit()
        {
            //
            var grid = User.Instance.GetGridData();
            int cellInFloorCount = grid[0].Count;
            Vector2 cellPosLB = TownMap.GetCellPos(1, 0);
            Vector2 cellPosRT = TownMap.GetCellPos(grid.Count - 1, cellInFloorCount - 1);
            var buildingCellCount = (grid.Count - 1) * cellInFloorCount;
            var yPos = (cellPosLB.y + cellPosRT.y) / 2f;
            var cam = Camera.main.GetComponent<ProCamera2D>();
            var panCamTarget = cam.CameraTargets[0];
            panCamTarget.TargetTransform.position = new Vector3(0, yPos, -10);
            
            cam.UpdateScreenSize(Mathf.Clamp(buildingCellCount / 2f, ((float)Screen.height / Screen.width) * 6.0f, ((float)Screen.width / Screen.height) * 6.0f));
        }

        void CameraZoomClear()
        {
            Camera.main.GetComponent<ProCamera2D>().ResetSize();
        }

        void BuildingSwitchModeInputCheck()
        {
            if (parentTown == null)
                return;

            
            if (_eventSystem.currentSelectedGameObject != null)
                return;

            if (Camera.main.GetComponent<ProCamera2DPanAndZoomCustom>() == null)
                return;

            if (PopupManager.OpenPopupCount > 0)
                return;

            if (Input.touchCount >= 2)
                return;
            
            if (Input.GetMouseButtonUp(0))
            {
                currentInputPos = Input.mousePosition;
                currentInputWorldPos = Camera.main.ScreenToWorldPoint(currentInputPos);

                if (isSelectBuilding == false)
                {
                    originFloorCellVec2 = TownMap.GetCellPosByInputPosition(currentInputWorldPos);
                    if (originFloorCellVec2.Value.x < 0)
                    {
                        if (originFloorCellVec2.Value.x == -1)  // 지하철인 경우
                            ToastManager.On(StringData.GetStringByStrKey("building_edit_error_1"));
                        else
                            ToastManager.On(100000645);
                        

                        return;
                    }
                    if (CheckBuildingCell(originFloorCellVec2))
                    {
                        isSelectBuilding = true;

                        CreateSelectedMarkerObject();
                    }
                    else
                    {
                        originFloorCellVec2 = null;
                    }
                }
                else
                {
                    destFloorCellVec2 = TownMap.GetCellPosByInputPosition(currentInputWorldPos);
                    if (destFloorCellVec2.Value.x < 0)
                    {
                        if (destFloorCellVec2.Value.x == -1)  // 지하철인 경우
                        {
                            ToastManager.On(StringData.GetStringByStrKey("building_edit_error_1"));
                        }
                        else
                        {
                            ToastManager.On(100000645);
                        }

                        return;
                    }
                    if (CheckBuildingCell(destFloorCellVec2))
                    {
                        SwitchBuilding();
                    }
                    else
                    {
                        destFloorCellVec2 = null;
                    }
                }
            }
        }

        // 해당 위치에 이동이 가능한지 체크
        bool CheckBuildingCell(Vector2Int? cellPos)
        {
            if (cellPos.HasValue)
            {
                // 확장하지 않은 층에 이동을 시도할 경우 처리
                if (cellPos.Value.x >= User.Instance.ExteriorData.ExteriorFloor)
                {
                    ToastManager.On(isSelectBuilding ? 100000645 : 100000742);
                    return false;
                }

                // 이동가능한 셀 인지 체크
                int buildingTag = User.Instance.GetBuildingTagInGridData(cellPos.Value.x, cellPos.Value.y);
                //bool landmarkCheck = /*buildingTag == (int)eLandmarkType.COINDOZER*/ /*|| buildingTag == (int)eLandmarkType.WORLDTRIP*/ buildingTag == (int)eLandmarkType.SUBWAY;
                if (isSelectBuilding == false)  // 첫 건물 선택
                {
                    if (buildingTag <= 1)   // 비어있는 셀을 선택한 경우
                    {
                        ToastManager.On(100000645);
                        return false;
                    }
                    //else if (landmarkCheck) // 랜드마크를 선택한 경우
                    //{
                    //    ToastManager.On(StringData.GetStringByStrKey("building_edit_error_1"));
                    //    return false;
                    //}
                    else
                    {
                        return true;
                    }
                }
                else  // 이동할 위치 선택
                {
                    //if (landmarkCheck) // 랜드마크를 선택한 경우
                    //{
                    //    ToastManager.On(StringData.GetStringByStrKey("building_edit_error_1"));
                    //    return false;
                    //}
                    //else
                    //{
                    //    if (buildingTag < 0 || buildingTag == 1)
                    //    {
                    //        ToastManager.On(100000742);//스위칭 불가지역
                    //        return false;
                    //    }

                    //    return true;
                    //}

                    if (buildingTag < 0 /*|| buildingTag == 1*/)
                    {
                        ToastManager.On(100000742);//스위칭 불가지역
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }
        void CreateWhiteMarkerObject(bool isConstruct = true) 
        {
            ClearMarkerObject();

            GameObject markerClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "BuildingConstructMarkerClone");
            if (markerClonePrefab == null)
                return;

            var grid = User.Instance.ExteriorData.ExteriorGrid;
            int floorCount = TownMap.Height;
            int cellCount = TownMap.Width;

            Vector2 selectedPos = Vector2.negativeInfinity;
            bool isSwitchable = false;
            if (!isConstruct && isSelectBuilding && originFloorCellVec2.HasValue)
            {
                selectedPos = TownMap.GetBuildingPos(originFloorCellVec2.Value.y, originFloorCellVec2.Value.x);
            }
            else if (isConstruct && constructFloorCellVec2.HasValue)
            {
                selectedPos = TownMap.GetBuildingPos(constructFloorCellVec2.Value.y, constructFloorCellVec2.Value.x);
                isSwitchable = true;
            }

            for (int i = 0; i < floorCount; ++i) //지하철 제외
            {
                for(int j =0; j< cellCount; ++j)
                {
                    Vector2 cellPos = TownMap.GetBuildingPos(j, i);
                    bool isSelectable = true;
                    
                    if (!isConstruct)
                    {
                        if (isSelectBuilding)
                        {
                            if (selectedPos == cellPos)
                                continue;
                        }
                        else
                        {
                            if (!grid.ContainsKey(i) || !grid[i].ContainsKey(j) || grid[i][j] == 0)
                                isSelectable = false;
                        }
                    }
                    else
                    {
                        if (selectedPos == cellPos)
                            continue;

                        if (!grid.ContainsKey(i) || !grid[i].ContainsKey(j) || grid[i][j] != 0)
                            isSelectable = false;
                    }




                    var obj = Instantiate(markerClonePrefab, townBase.transform);
                    BuildingConstructMarker marker = obj.GetComponent<BuildingConstructMarker>();
                    if (marker != null)
                        marker.SetSelectable(isSelectable, isSwitchable);

                    obj.transform.localPosition = cellPos;
                    markerSpaceList.Add(obj);
                }
            }
        }
        void ClearMarkerObject()
        {
            foreach(var obj in markerSpaceList)
            {
                Destroy(obj);
            }

            markerSpaceList.Clear();

            if (selectedMarkerUI != null)
            {
                Destroy(selectedMarkerUI);
                selectedMarkerUI = null;
            }
        }

        void CreateSelectedMarkerObject()
        {
            if (originFloorCellVec2.HasValue == false) 
                return;

            CreateWhiteMarkerObject(false);

            Vector2 cellPos = TownMap.GetBuildingPos(originFloorCellVec2.Value.y, originFloorCellVec2.Value.x);

            GameObject markerClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "BuildingSwitchMarkerClone");
            if (markerClonePrefab != null)
            {
                selectedMarkerUI = Instantiate(markerClonePrefab, townBase.transform);
                selectedMarkerUI.transform.localPosition = cellPos;
            }
        }

        void SwitchBuilding()
        {
            if (isNetworkState)
                return;
            isNetworkState = true;
            if (originFloorCellVec2.HasValue == false || destFloorCellVec2.HasValue == false)  
                return; 

            WWWForm paramData = new WWWForm();
            
            // 서버 데이터와 맞게끔 x+1
            paramData.AddField("posA", JsonConvert.SerializeObject(new int[] { originFloorCellVec2.Value.x + 1, originFloorCellVec2.Value.y }));
            paramData.AddField("posB", JsonConvert.SerializeObject(new int[] { destFloorCellVec2.Value.x + 1, destFloorCellVec2.Value.y }));
            
            NetworkManager.Send("building/replace", paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            SoundManager.Instance.PlaySFX("sfx_build_set");

                            Town.Instance.RefreshMap();
                            ClearSwitchData();
                            break;
                    }
                }
            },
            (string arg) =>
            {
                isNetworkState = false;
            });
        }

        #endregion



       

        void ClearData()
        {
            isConstructMode = false;

            isBuildingSwitchMode = false;
            isSelectBuilding = false;

            if (newBuilding != null)
            {
                Destroy(newBuilding);
                newBuilding = null;
            }

            if (newControlUI != null)
            {
                Destroy(newControlUI);
                newControlUI = null;
            }

            ClearMarkerObject();

            constructFloorCellVec2 = null;
            CurTag = 0;

            originFloorCellVec2 = null;
            destFloorCellVec2 = null;
        }

        void ClearObject()
        {
            if (newBuilding != null)
            {
                Destroy(newBuilding);
                newBuilding = null;
            }

            if (newControlUI != null)
            {
                Destroy(newControlUI);
                newControlUI = null;
            }

            ClearMarkerObject();
        }

        void ClearSwitchData()
        {
            isSelectBuilding = false;

            originFloorCellVec2 = null;
            destFloorCellVec2 = null;

            ClearMarkerObject();

            CreateWhiteMarkerObject(false);
        }
    }
}
