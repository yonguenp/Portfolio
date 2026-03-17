using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonLevelupPanel : DragonManageSubPanel
    {
        [SerializeField]
        Button levelUpButton = null;
        [SerializeField]
        Text emptySelectLabel = null;
        [SerializeField]
        Text emptyInvenLabel = null;

        [SerializeField]
        TableView invenTableview = null;

        [SerializeField]
        TableView selectedTableview = null;

        int DRAGON_MAX_LEVEL = -1;//테이블에 정의된 드래곤 MAX 레벨
        int currentDragonLevel = 0;//현재 선택된 드래곤 레벨
        int currentDragonExp = 0;//현재 선택된 드래곤 경험치의 잔여 경험치 (총경험치 아님!)
        int currentDragonGrade = 0;//현재 선택된 드래곤 경험치의 잔여 경험치 (총경험치 아님!)

        [SerializeField]
        Slider currentLevelSlider = null;
        [SerializeField]
        Text currentExpLabel = null;
        [SerializeField]
        Slider levelUpSlider = null;
        [SerializeField]
        Text currentLevelLabel = null;
        [SerializeField]
        Text currentBattlePointLabel = null;
        [SerializeField]
        Text nextLevelLabel = null;
        [SerializeField]
        Text nextBattlePointLabel = null;

        bool tempMaxLevelCheckFlag = false;//드래곤 레벨 최대치 체크
        int tempPredictableLevel = 0;//다음 예상 레벨 임시 변수 - 레벨 라벨 표시용도
        int tempLevelCheck = -1;//레벨업 연출 할때 다음예상 레벨 몇인지 임시 변수

        [SerializeField]
        GameObject tempTweenNode = null;

        //auto levelup comp
        [SerializeField]
        GameObject autoClickNode = null;
        [SerializeField]
        Button autoLeftButton = null;
        [SerializeField]
        Button autoRightButton = null;
        [SerializeField]
        Button autoMaxButton = null;
        [SerializeField]
        Text autoTargetLevelLabel = null;

        Dictionary<int, List<int>> autoLevelClickData = new Dictionary<int, List<int>>();//key : predictableLevel , value : battery index List
        List<int> remainList = new List<int>();

        Sequence tweenProgressAnim = null;
        Sequence tweenLabelAnim = null;

        Dictionary<int, batteryItem> invenDataList = new Dictionary<int, batteryItem>();//현재 소유한 배터리
        Dictionary<int, batteryItem> selectedDataList = new Dictionary<int, batteryItem>();//현재 선택한 배터리

        private bool isInvenTableInit = false;
        private bool isSelectTableInit = false;

        private bool emptyBatteryFlag = false;
        private bool maxBatteryFlag = false;

        public override void ShowPanel(VoidDelegate _successCallback = null)
        {
            base.ShowPanel(_successCallback);
        }

        public override void HidePanel()
        {
            base.HidePanel();
        }

        public override void Init()
        {
            base.Init();

            if (tweenProgressAnim != null)
            {
                tweenProgressAnim.Kill();
                tweenProgressAnim = null;
            }

            if (DRAGON_MAX_LEVEL < 0)
                DRAGON_MAX_LEVEL = GameConfigTable.GetDragonLevelMax();

            tempMaxLevelCheckFlag = false;
            tempLevelCheck = -1;

            //draw tableview
            InitInvenTableview();
            InitSelectedTableview();

            SetInvenBatteryData();

            DrawAllTableView();


            RefreshDragonInfo();

            //auto click init
            InitAutoLevelUpData();
            HideAutoClickNode();
        }

        public override void ForceUpdate()
        {
            base.ForceUpdate();
        }

        void InitInvenTableview()
        {
            if (invenTableview != null && !isInvenTableInit)
            {
                invenTableview.OnStart();
                isInvenTableInit = true;
            }

            if (invenDataList != null)
                invenDataList.Clear();
            else
                invenDataList = new Dictionary<int, batteryItem>();
        }

        List<batteryItem> GetUserAllBatteryItems()
        {
            var batteryTypeItems = User.Instance.Inventory.GetKindItems(eItemKind.EXP);
            List<batteryItem> batteryTypeList = new List<batteryItem>();

            foreach (var battery in batteryTypeItems)
            {
                if (battery == null)
                    continue;

                batteryItem tempInfo = new batteryItem(battery.ItemNo, battery.Amount);
                batteryTypeList.Add(tempInfo);
            }
            return batteryTypeList;
        }


        void SetInvenBatteryData()//배터리 key 순 정렬 및 앞에서 부터 slot count 갯수 대로 최대치 만들어서 표시
        {
            var batteryTypeList = GetUserAllBatteryItems();

            invenDataList.Clear();

            if (batteryTypeList != null && batteryTypeList.Count > 0)
            {
                batteryTypeList.Sort((a, b) => a.ItemNo.CompareTo(b.ItemNo));

                for (var k = 0; k < batteryTypeList.Count; k++)
                {
                    var itemNo = batteryTypeList[k].ItemNo;
                    var itemCount = batteryTypeList[k].Amount;
                    var itemInfo = batteryTypeList[k].BaseData;

                    if (itemInfo == null || itemInfo.SLOT_USE == null)
                    {
                        Debug.Log("item Data is error : " + itemNo);
                    }

                    var itemRemain = itemCount;
                    var slotCount = 0;


                    while (itemRemain != 0)
                    {
                        if (itemRemain > itemInfo.MERGE)
                        {
                            slotCount = itemInfo.MERGE;
                            itemRemain -= itemInfo.MERGE;
                        }
                        else
                        {
                            slotCount = itemRemain;
                            itemRemain -= slotCount;
                        }

                        if (invenDataList.ContainsKey(itemNo))
                        {
                            invenDataList[itemNo].AddCount(slotCount);
                        }
                        else
                        {
                            batteryItem battery = new batteryItem(itemNo, slotCount);
                            invenDataList.Add(itemNo, battery);
                        }
                    }
                }
            }
        }
        bool IsEmptyInvenBattery()
        {
            if (invenDataList == null || invenDataList.Count <= 0)
                return true;
            return false;
        }
        void RemoveInvenDataItem(batteryItem data)
        {
            if (IsEmptyInvenBattery())
                return;

            if(invenDataList.ContainsKey(data.ItemNo))
            {
                if(invenDataList[data.ItemNo].Amount >= data.Amount)
                {
                    invenDataList[data.ItemNo].AddCount(data.Amount * -1);
                }

                if(invenDataList[data.ItemNo].Amount <= 0)
                {
                    invenDataList.Remove(data.ItemNo);
                }            
            }            
        }

        void ClearInvenBatteryData()
        {
            invenDataList.Clear();
        }

        void DrawInvenBattery(bool initPos = true)
        {
            if (invenDataList == null)
                return;

            var keyList = invenDataList.Keys.ToList();
            keyList.Sort();
            //itemID 기준으로 오름차순 정렬해야함

            List<ITableData> tempList = new List<ITableData>();
            tempList.Clear();

            foreach(int key in keyList)
            {
                tempList.Add(invenDataList[key]);
            }

            invenTableview.SetDelegate(new TableViewDelegate(tempList, (GameObject node, ITableData item) => {
                if (node == null)
                    return;

                var frame = node.GetComponent<ItemFrame>();
                if (frame == null)
                    return;

                var batteryData = (batteryItem)item;

                frame.GetComponent<ItemFrame>().SetFrameItemExpInfo(batteryData.ItemNo, batteryData.Amount, batteryData.GetEXP());
                frame.GetComponent<ItemFrame>().setCallback((param) =>
                {
                    OnClickInvenFrame(param);

                    DrawAllTableView(false);
                });
            }));

            invenTableview.ReLoad(initPos);

        }
        void InitSelectedTableview()
        {
            if (selectedTableview != null && !isSelectTableInit)
            {
                selectedTableview.OnStart();
                isSelectTableInit = true;
            }

            if (selectedDataList != null)
                selectedDataList.Clear();
            else
                selectedDataList = new Dictionary<int, batteryItem>();
        }
        bool IsEmptySelectBattery()
        {
            if (selectedDataList == null || selectedDataList.Count <= 0)
                return true;
            return false;
        }
        void ClearSelectedBatteryData()
        {
            selectedDataList.Clear();
        }

        void RemoveSelectedDataItem(batteryItem data)
        {
            if (IsEmptySelectBattery())
                return;

            if (selectedDataList.ContainsKey(data.ItemNo))
            {
                if (selectedDataList[data.ItemNo].Amount >= data.Amount)
                {
                    selectedDataList[data.ItemNo].AddCount(data.Amount * -1);
                }

                if (selectedDataList[data.ItemNo].Amount <= 0)
                {
                    selectedDataList.Remove(data.ItemNo);
                }
            }
        }

        void DrawSelectedBattery(bool initPos = true)
        {
            if (selectedDataList == null)
                return;

            var keyList = selectedDataList.Keys.ToList();
            keyList.Sort();
            //itemID 기준으로 오름차순 정렬해야함

            List<ITableData> tempList = new List<ITableData>();
            tempList.Clear();

            foreach(var key in keyList)
            {
                var selectedItem = selectedDataList[key];
                var amount = selectedItem.Amount;
                if (amount <= 0)
                    continue;

                tempList.Add(selectedDataList[key]);
            }

            selectedTableview.SetDelegate(new TableViewDelegate(tempList, (GameObject node, ITableData item) => {
                if (node == null)
                    return;

                var frame = node.GetComponent<ItemFrame>();
                if (frame == null)
                    return;

                var batteryData = (batteryItem)item;

                frame.GetComponent<ItemFrame>().SetFrameItemInfo(batteryData.ItemNo, batteryData.Amount);
                frame.GetComponent<ItemFrame>().setCallback((param) => {
                    int batteryIndex = int.Parse(param);
                    SelectNodeParamProcess(batteryIndex);

                    DrawAllTableView(false);
                });
            }));

            selectedTableview.ReLoad(initPos);
        }

        void DrawAllTableView(bool invenInitPos = true, bool selectInitPos = true)
        {
            DrawInvenBattery(invenInitPos);
            DrawSelectedBattery(selectInitPos);
        }

        void OnClickInvenFrame(string selectIdx)//선택된 인벤토리에 선택한 노드가 없으면 생성, 있으면 카운트만 리프레시
        {
            if (tempMaxLevelCheckFlag)
            {
                Debug.Log("level Max Already");
                ToastManager.On(100000210);
                return;
            }

            SetSelectItemFrameByIndex(int.Parse(selectIdx));
            RefreshUIProcess();
        }

        void CustomClickInvenFrame(string selectIdx)
        {
            if (tempMaxLevelCheckFlag)
            {
                Debug.Log("level Max Already");
                ToastManager.On(100000210);
                return;
            }

            SetSelectItemFrameByIndex(int.Parse(selectIdx));
            RefreshUIProcess(false);
        }

        void SetSelectItemFrameByIndex(int selectIdx)//선택 건전지 스크롤 뷰에 세팅
        {
            var checkNode = GetBatterInfoByItemID(selectIdx, false);
            if (checkNode == null)//생성 인벤슬롯이 없음
            {
                if (selectedDataList.ContainsKey(selectIdx))
                {
                    selectedDataList[selectIdx].AddCount(1);
                }
                else
                {
                    selectedDataList.Add(selectIdx, new batteryItem(selectIdx, 1));
                }
            }
            else//인벤 슬롯이 null이 아님 (있음)
            {
                checkNode.AddCount(1);
            }
            //현재 선택 인벤 노드의 아이템 갯수 줄이기
            var invenInfo = GetBatterInfoByItemID(selectIdx, true);
            if (invenInfo != null)
            {
                if (SelectInvenCountOne(invenInfo))
                    invenInfo.AddCount(-1);
                else
                    RemoveInvenDataItem(invenInfo);
            }
        }

        void SelectNodeParamProcess(int batteryID, int count = 1)
        {
            var selectInfo = GetBatterInfoByItemID(batteryID, false);
            var invenInfo = GetBatterInfoByItemID(batteryID, true);

            if (SelectInvenCountOne(selectInfo))
                selectInfo.AddCount(count * -1);
            else
                RemoveSelectedDataItem(selectInfo);

            if (invenInfo == null)//노드 전부 삭제시 프레임 재생성
            {
                if (invenDataList.ContainsKey(batteryID))
                {
                    invenDataList[batteryID].AddCount(count);
                }
                else
                {
                    invenDataList.Add(batteryID, new batteryItem(batteryID, count));
                }
            }
            else
                invenInfo.AddCount(count);

            RefreshUIProcess();
        }

        batteryItem GetBatterInfoByItemID(int itemID, bool isInvenTable)//true면 inven에서, false 면 selected에서 검색
        {
            if (isInvenTable)
            {
                if (IsEmptyInvenBattery())
                    return null;

                if (invenDataList.ContainsKey(itemID))
                    return invenDataList[itemID];
            }
            else
            {
                if (IsEmptySelectBattery())
                    return null;

                if(selectedDataList.ContainsKey(itemID))
                    return selectedDataList[itemID];
            }
            return null;
        }

        //1보다 작으면 - 0이되면 해당 노드 삭제
        bool SelectInvenCountOne(batteryItem itemNode)
        {
            var itemAmount = itemNode.Amount;
            bool isUpperOne = itemAmount > 1;
            return isUpperOne;
        }

        void InitCurrentDragonData()
        {
            if (dragonTag > 0)//드래곤 태그값
            {
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

                currentDragonGrade = userDragonInfo.BaseData.GRADE;
                currentDragonLevel = userDragonInfo.Level;
                currentDragonExp = CalcModifyReduceEXP(userDragonInfo.Exp);//누적 경험치로옴
            }
        }
        int CalcModifyReduceEXP(int sendtotalExp)
        {
            var requireTotalEXP = CharExpData.GetCurrentAccumulateGradeAndLevelExp(currentDragonGrade, currentDragonLevel);//현재 레벨 요구 경험치
            return sendtotalExp - requireTotalEXP;
        }

        void RefreshDragonInfo()
        {
            InitCurrentDragonData();//현재 드래곤 레벨, 경험치 세팅
            InitLevelUPLabel();
            InitProgressBar();
            InitLevelLabel();

            RefreshUIProcess();
        }

        void RefreshUIProcess(bool _isRefreshUI = true)//레벨업 버튼 갱신, 프로그레스 바 갱신, 배터리 라벨 갱신
        {
            RefreshPredictData();
            
            if(_isRefreshUI)
            {
                RefreshDragonProgressBar();
                RefreshLevelUpButton();
                RefreshSelectEmptyLabel();
                RefreshInvenEmptyLabel();
            }
        }

        void RefreshPredictData()//현재 선택된 배터리 기반 예상 레벨 계산
        {
            var resultData = GetExpectLevelBySelectedBattery();
            var level = resultData.FinalLevel;//계산 완료 후 결과 레벨
            var maxLevelFlag = IsMaxLevel(level);

            tempPredictableLevel = level;
            tempMaxLevelCheckFlag = maxLevelFlag;
        }

        void RefreshDragonProgressBar()//현재 경험치 총량 계산 - 경험치 아이템 클릭시 들어옴
        {
            var result = GetExpectLevelBySelectedBattery();
            RefreshLevelUPProgressBar(tempPredictableLevel != currentDragonLevel, tempPredictableLevel, result.ReduceExp);
            RefreshLevelLabel(tempPredictableLevel);
            RefreshLevelUPLabel(IsMaxLevel(tempPredictableLevel), tempPredictableLevel, result.ReduceExp);
        }

        CharLevelExpData GetExpectLevelBySelectedBattery()//현재 선택된 배터리 기반으로 예상 레벨 및 경험치 계산
        {
            var calcTotalExp = GetTotalExpByTargetList(selectedDataList.Values.ToList());
            return CharExpData.GetGradeAndLevelAddExp(currentDragonGrade, currentDragonLevel, currentDragonExp, calcTotalExp);
        }

        /**
        * 
        * @param isLevelChangeFlag // 레벨 변경 플래그
        * @param level //변경 후 레벨
        * @param reduceExp //잔여 경험치
        * @returns 
        */

        void RefreshLevelUPLabel(bool maxLevelFlag, int level, int reduceExp)
        {
            if (currentExpLabel == null)
            {
                return;
            }

            if (tweenLabelAnim != null)
            {
                tweenLabelAnim.Kill();
                tweenLabelAnim = null;
            }
            if (maxLevelFlag)
            {
                currentExpLabel.text = StringData.GetStringByIndex(100000113);
            }
            else
            {
                tweenLabelAnim = DOTween.Sequence();

                var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, level);//현재 레벨 요구 경험치
                if (level == currentDragonLevel)
                {
                    tweenLabelAnim.Append(tempTweenNode.GetComponent<RectTransform>().DOAnchorPos(new Vector3(reduceExp + currentDragonExp, 0, 0), 0.2f)).OnUpdate(() =>
                    {
                        currentExpLabel.text = (((int)tempTweenNode.GetComponent<RectTransform>().anchoredPosition.x).ToString() + " / " + totalExp);
                    });

                }
                else
                {
                    tweenLabelAnim.Append(tempTweenNode.GetComponent<RectTransform>().DOAnchorPos3D(new Vector3(0, reduceExp, totalExp), 0.2f)).OnUpdate(() =>
                    {
                        var position = tempTweenNode.GetComponent<RectTransform>().anchoredPosition3D;
                        var posX = (int)position.x;
                        var posY = (int)position.y;
                        var posZ = (int)position.z;

                        currentExpLabel.text = posY.ToString() + " / " + posZ.ToString();
                    });
                }

                tweenLabelAnim.Play();
            }
        }

        void InitLevelUPLabel()
        {
            if (currentExpLabel == null)
            {
                return;
            }
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, currentDragonLevel);//현재 레벨 요구 경험치
            currentExpLabel.text = SBFunc.StrBuilder(currentDragonExp, " / ", totalExp);
            tempTweenNode.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(currentDragonExp, 0, 0);
        }

        void InitProgressBar()//현재 레벨 및 경험치 기반으로 세팅
        {
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, currentDragonLevel);//현재 레벨 요구 경험치

            if (totalExp <= 0)
                totalExp = 1;

            var mod = currentDragonExp / totalExp;
            if (levelUpSlider != null && currentLevelSlider)
            {
                levelUpSlider.value = mod;
                currentLevelSlider.value = mod;
            }
        }

        /**
         * @param levelChangeFlag //현재의 드래곤 레벨과 다른지 - 단순 동렙 체크용
         * @param level //들어온 레벨
         * @param reduceExp //잔여 경험치
         * @param isAutoClick //자동 선택버튼을 눌렀는지
         */
        void RefreshLevelUPProgressBar(bool levelChangeFlag, int level, int reduceExp)//레벨업 노티
        {
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, level);//현재 레벨 요구 경험치
            var isDragonMaxLevel = IsMaxLevel(level);//맥렙 체크
            var isDragonPrevLastLevel = (DRAGON_MAX_LEVEL - 1 == level);//현재 들어온 레벨이 맥렙 바로 전인가 - 추후에 한계 레벨도 분류해야함
            float mod = 0;

            bool levelup = false;
            bool leveldown = false;
            if (tempLevelCheck < 0)
            {
                tempLevelCheck = level;
            }
            if (tempLevelCheck < level)//레벨이 증가하려함
            {
                leveldown = false;
                levelup = true;
            }
            else if (tempLevelCheck > level)//레벨이 떨어지려함
            {
                leveldown = true;
                levelup = false;
            }
            else
            {
                leveldown = false;
                levelup = false;
            }
            tempLevelCheck = level;


            if (totalExp != 0)
            {
                mod = (float)reduceExp / (float)totalExp;
            }

            if (isDragonMaxLevel)//맥렙일때 - 맥렙이 된 경우
            {
                if (IsMaxLevel(currentDragonLevel))//이미 드래곤이 맥스레벨
                {
                    currentLevelSlider.gameObject.SetActive(false);
                    levelUpSlider.value = 1;
                    levelUpSlider.gameObject.SetActive(true);
                    return;
                }
                if (leveldown == false && levelup == false)
                {
                    return;
                }
                PlayTweenAllMotion(mod, 5, levelChangeFlag);
            }
            else
            {
                if (levelChangeFlag)
                {
                    if (levelUpSlider != null && currentLevelSlider)
                    {
                        if (levelup == true && leveldown == false)
                        {
                            PlayTweenAllMotion(mod, 2, levelChangeFlag);
                        }
                        else if (levelup == false && leveldown == true)
                        {
                            if (isDragonPrevLastLevel)
                            {
                                PlayTweenAllMotion(mod, 4, levelChangeFlag);
                            }
                            else
                            {
                                PlayTweenAllMotion(mod, 3, levelChangeFlag);
                            }
                        }
                        else
                        {
                            PlayTweenAllMotion(mod, 1, levelChangeFlag);
                        }
                    }
                }
                else
                {
                    if (levelUpSlider != null && currentLevelSlider)
                    {

                        mod = (float)(reduceExp + currentDragonExp) / (float)totalExp;
                        var currentExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, currentDragonLevel);//현재 레벨 요구 경험치
                        var currentMode = (float)currentDragonExp / currentExp;
                        currentLevelSlider.value = currentMode;
                        if (leveldown)
                        {
                            if (isDragonPrevLastLevel)//맥렙 바로 전일때
                            {
                                PlayTweenAllMotion(mod, 4, levelChangeFlag);
                            }
                            else
                            {
                                PlayTweenAllMotion(mod, 3, levelChangeFlag);
                            }
                        }
                        else
                        {
                            PlayTweenAllMotion(mod, 1, levelChangeFlag);
                        }
                    }
                }
            }
        }

        void PlayTweenAllMotion(float modifyAmount, int caseIndex, bool isChangeCurrentLevel)
        {
            levelUpSlider.gameObject.SetActive(true);

            if (tweenProgressAnim != null)
            {
                tweenProgressAnim.Kill();
            }
            tweenProgressAnim = DOTween.Sequence();

            switch (caseIndex)
            {
                case 1:
                    currentLevelSlider.gameObject.SetActive(!isChangeCurrentLevel);
                    tweenProgressAnim.Append(levelUpSlider.DOValue(modifyAmount, 0.2f));
                    break;
                case 2://레벨 업 평상렙
                    tweenProgressAnim.Append(levelUpSlider.DOValue(1, 0.3f))
                        .AppendCallback(() => {
                            currentLevelSlider.gameObject.SetActive(false);
                            levelUpSlider.value = 0f;
                        })
                        .Append(levelUpSlider.DOValue(modifyAmount, 0.3f));
                    break;
                case 3://레벨 다운 평상렙
                    tweenProgressAnim.Append(levelUpSlider.DOValue(0, 0.3f)).Append(levelUpSlider.DOValue(1, 0f)).AppendCallback(() => {
                        if (isChangeCurrentLevel == false)
                        {
                            currentLevelSlider.gameObject.SetActive(true);
                        }
                    }).Append(levelUpSlider.DOValue(modifyAmount, 0.3f));
                    break;
                case 4://레벨 다운(맥렙에서 맥렙 바로전)
                    tweenProgressAnim.Append(levelUpSlider.DOValue(1, 0f)).OnComplete(() => {
                        levelUpSlider.gameObject.SetActive(true);
                        if (!isChangeCurrentLevel)
                        {
                            currentLevelSlider.gameObject.SetActive(true);
                        }
                    }).Append(levelUpSlider.DOValue(modifyAmount, 0.3f));

                    break;
                case 5: //맥렙으로 레벨업
                    tweenProgressAnim.Append(levelUpSlider.DOValue(1, 0.2f)).OnComplete(() => {
                        currentLevelSlider.gameObject.SetActive(false);
                        levelUpSlider.value = 1;
                        levelUpSlider.gameObject.SetActive(true);
                    }).Append(levelUpSlider.DOValue(1, 0.2f));
                    break;
            }

            if (tweenProgressAnim != null)
            {
                tweenProgressAnim.Play();
            }
        }

        void InitLevelLabel()
        {
            if (currentLevelLabel != null && nextLevelLabel != null)
            {
                var levelMod = string.Format("{0:D2}", currentDragonLevel);
                currentLevelLabel.text = SBFunc.StrBuilder("Lv. ", levelMod);
                nextLevelLabel.text = SBFunc.StrBuilder("Lv. ", levelMod);
                tempPredictableLevel = currentDragonLevel;
            }
            if (currentBattlePointLabel != null && nextBattlePointLabel != null)
            {
                var dragonINF = GetCurrentDragonINF();

                currentBattlePointLabel.text = dragonINF.ToString();
                nextBattlePointLabel.text = dragonINF.ToString();
            }
        }

        void RefreshLevelLabel(int nextLevel)
        {
            if (currentLevelLabel != null && nextLevelLabel != null)
            {
                var levelMod = string.Format("{0:D2}", nextLevel);
                nextLevelLabel.text = SBFunc.StrBuilder("Lv. ", levelMod);
            }

            if (currentBattlePointLabel != null && nextBattlePointLabel != null)
            {
                var dragonINF = GetIncrementDragonTotalINF(nextLevel);
                nextBattlePointLabel.text = dragonINF.ToString();
            }
        }
        //현재 레벨의 총합 전투력
        int GetCurrentDragonINF()
        {
            var dragonInfo = User.Instance.DragonData.GetDragon(dragonTag);
            if (dragonInfo == null)
                return 0;

            if (dragonInfo.Status == null)
                dragonInfo.RefreshALLStatus();

            return dragonInfo.Status.GetTotalINF();
        }

        //증가분 투력치(장비 및 스킬 계산 값)
        int GetIncrementDragonTotalINF(int nextLevel)
        {
            var dragonInfo = User.Instance.DragonData.GetDragon(dragonTag);
            if (dragonInfo == null)
                return 0;

            return dragonInfo.GetALLStatus(nextLevel).GetTotalINF();
        }

        public void OnClickLevelUpButton()
        {
            if (IsMaxLevel(currentDragonLevel))
            {
                ToastManager.On(StringData.GetStringByStrKey("최대레벨도달"));
                return;
            }

            var checkStr = SetSendItemListByString();
            if (checkStr == "")
            {
                Debug.Log("not select battery");
                ToastManager.On(100000207);
                return;
            }

            var currentStat = User.Instance.DragonData.GetDragon(dragonTag).Status;

            var data = new WWWForm();
            data.AddField("did", dragonTag);
            data.AddField("item", checkStr);

            NetworkManager.Send("dragon/battery", data, (jsonData) => {
                Debug.Log("levelup success callback");

                DragonLevelPopupData popupData = new DragonLevelPopupData(currentDragonLevel, tempPredictableLevel, currentStat);

                if (tempPredictableLevel > currentDragonLevel)
                {
                    PopupManager.OpenPopup<DragonLevelUpPopup>(popupData);
                }

                ForceUpdate();//화면 갱신
                
                if (successCallback != null)//levelupSuccessCallback 추가
                    successCallback();
            });
        }

        string SetSendItemListByString()
        {
            if (IsEmptySelectBattery())
                return "";

            List<string> totalStr = new List<string>();
            selectedDataList.Values.ToList().ForEach((element) => {
                var itemID = element.ItemNo;
                var Amount = element.Amount;

                if (Amount <= 0)
                    return;

                totalStr.Add(itemID.ToString() + ":" + Amount);
            });

            return string.Join(",", totalStr);

        }

        /**
         * 자동 선택 버튼 +1 레벨업 기준으로 자동 건전지 등록
         */

        public void OnClickAutoSelectButton()
        {
            if (IsEmptyInvenBattery())
            {
                ToastManager.On(100000208);
                Debug.Log("보유 중인 건전지가 없습니다.");
                return;
            }

            var reduceExp = currentDragonExp;
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, tempPredictableLevel);//현재 레벨 요구 경험치
            if (IsMaxLevel(currentDragonLevel))
            {
                ToastManager.On(100000210);
                Debug.Log("더 이상 선택할 수 없습니다.");
                return;
            }
            else
            {
                var currentSelectedEXP = GetTotalExpByTargetList(selectedDataList.Values.ToList());//현재 걸려있는 경험치총량
                var currentDragonExp = User.Instance.DragonData.GetDragon(dragonTag).Exp;//현재 드래곤 경험치 총량
                var predictDragonExp = (currentSelectedEXP + currentDragonExp);//예상 총 경험치

                var predictData = CharExpData.GetLevelAndExpByTotalExp(currentDragonGrade, predictDragonExp);
                var predictReduce = predictData.ReduceExp;

                reduceExp = predictReduce;
            }

            var requireExp = totalExp - reduceExp;

            invenDataList.Values.ToList().ForEach((element) => {
                if (requireExp <= 0)
                    return;

                var itemID = element.ItemNo;
                var itemAmount = element.Amount;
                var exp = element.GetEXP();

                while (itemAmount > 0)
                {
                    OnClickInvenFrame(itemID.ToString());
                    itemAmount = element.Amount;
                    requireExp -= exp;
                    if (requireExp <= 0)
                        break;
                }
            });

            DrawAllTableView();
        }


        /**
        * //[보유 중인 경험치 아이템] 중 현재 [선택해야 하는 경험치]와 제일 차이가 적은 경험치 아이템 선택
        * //onClickAutoSelectButton <-- 이 함수는 가장 왼쪽(가장 적은 경험치)기준으로 채워넣음
        */
        public void OnClickAutoSelectButton_effectiveUse()
        {
            if (IsEmptyInvenBattery())
            {
                ToastManager.On(100000208);
                Debug.Log("보유 중인 건전지가 없습니다.");
                return;
            }

            var reduceExp = currentDragonExp;
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, tempPredictableLevel);//현재 레벨 요구 경험치

            if (IsMaxLevel(currentDragonLevel))
            {
                ToastManager.On(100000210);
                Debug.Log("더 이상 선택할 수 없습니다.");
                return;
            }
            else
            {
                var currentSelectedEXP = GetTotalExpByTargetList(selectedDataList.Values.ToList());//현재 걸려있는 경험치총량
                var currentDragonExp = User.Instance.DragonData.GetDragon(dragonTag).Exp;//현재 드래곤 경험치 총량
                var predictDragonExp = (currentSelectedEXP + currentDragonExp);//예상 총 경험치

                var predictData = CharExpData.GetLevelAndExpByTotalExp(currentDragonGrade, predictDragonExp);
                var predictReduce = predictData.ReduceExp;

                reduceExp = predictReduce;
            }

            var requireExp = totalExp - reduceExp;//레벨업에 필요한 요구 경험치 - calcTotalExp

            while (requireExp > 0)
            {
                var getCheckItemID = CalcEffectiveDiffBattery(requireExp);
                if (getCheckItemID < 0)
                {
                    break;
                }
                CustomClickInvenFrame(getCheckItemID.ToString());
                ItemBaseData itemInfo = ItemBaseData.Get(getCheckItemID);
                var itemValue = itemInfo.VALUE;
                requireExp -= itemValue;
            }
            RefreshUIProcess();
            DrawAllTableView();
        }

        /**
         * 남은 경험치 (reduceExp)기준으로 현재 인벤토리에서 가장 차이가 적은 아이템 태그를 반환해줌
         */
        int CalcEffectiveDiffBattery(int reduceExp)
        {
            if (IsEmptyInvenBattery())
            {
                return -1;
            }

            var diff = CharExpData.GetDragonMaxTotalExp(currentDragonGrade);
            var tempItemID = -1;

            foreach(var data in invenDataList.Reverse())
            {
                var element = data.Value;

                var itemValue = element.BaseData.VALUE;
                var itemAmount = element.Amount;

                if (itemAmount <= 0)
                    continue;

                var calcDiff = Math.Abs(reduceExp - itemValue);
                if (calcDiff < diff)
                {
                    diff = calcDiff;
                    tempItemID = element.ItemNo;
                    break;
                }
            }

            return tempItemID;
        }

        public void OnClickSetVisibleAutoClickNode()
        {
            if (autoClickNode.activeInHierarchy)
            {
                //기획 변경으로 인한 켜져있을 때 자동 선택을 누르면 레벨업 되게 변경
                //hideAutoClickNode();

                if (tweenProgressAnim.active)
                    return;

                OnClickAutoRightButton();
            }
            else
                ShowAutoClickNode();
        }

        void ShowAutoClickNode()
        {
            if (IsMaxLevel(currentDragonLevel))
            {
                ToastManager.On(100000210);
                Debug.Log("최대 레벨입니다.");
                return;
            }

            autoClickNode.SetActive(true);

            emptyBatteryFlag = false;
            maxBatteryFlag = false;

            if (!IsEqualDataSelectedNode())//다르면 밀고 다시 그려
            {
                if (tweenProgressAnim != null)
                {
                    tweenProgressAnim.Kill();
                }
                tempMaxLevelCheckFlag = false;
                tempLevelCheck = -1;

                //Set & Draw TableView
                ClearSelectedBatteryData();
                ClearInvenBatteryData();
                SetInvenBatteryData();
                DrawAllTableView();


                RefreshDragonInfo();
                InitAutoLevelUpData();
            }

            if (currentDragonLevel == tempPredictableLevel)//최초 입장 시엔 자동 선택 한번 태움
            {
                OnClickAutoRightButton();
            }
        }

        public void HideAutoClickNode()
        {
            autoClickNode.SetActive(false);
        }

        void RefreshAutoClickNode()
        {
            if (autoTargetLevelLabel != null)
                autoTargetLevelLabel.text = SBFunc.StrBuilder("Lv. ", tempPredictableLevel);

            bool leftButtonFlag = true;
            if (currentDragonLevel == tempPredictableLevel)
            {
                if (autoLevelClickData.ContainsKey(currentDragonLevel))
                {
                    var data = autoLevelClickData[currentDragonLevel];
                    if (data == null || data.Count <= 0)
                        leftButtonFlag = false;
                    else
                        leftButtonFlag = true;
                }
                else
                    leftButtonFlag = false;
            }

            autoLeftButton.SetInteractable(leftButtonFlag);

            var isRightButtonInteractable = (tempPredictableLevel < DRAGON_MAX_LEVEL);
            autoRightButton.SetInteractable(isRightButtonInteractable);
            autoMaxButton.SetInteractable(isRightButtonInteractable);
        }

        public void OnClickAutoRightButton()
        {
            AutoIncrementSelect();
            RefreshAutoClickNode();
        }

        public void OnClickAutoLeftButton()//경험치 배터리 저장된 상태에서 빼는 기능 제작
        {
            AutoDecrementSelect();
            RefreshAutoClickNode();
        }

        public void OnClickMaxButton()//현재 배터리 기준으로 딱 맞는 레벨맞게
        {
            var count = DRAGON_MAX_LEVEL - tempPredictableLevel;

            while (count > 0)
            {
                AutoMaxLevel();
                count--;
            }

            emptyBatteryFlag = false;
            maxBatteryFlag = false;

            DrawAllTableView();
            RefreshUIProcess();
            RefreshAutoClickNode();
        }

        void InitAutoLevelUpData()
        {
            autoLevelClickData.Clear();
        }

        void AutoIncrementSelect()
        {
            if (IsEmptyInvenBattery())
            {
                ToastManager.On(100000208);
                Debug.Log("보유 중인 건전지가 없습니다.");
                return;
            }

            var reduceExp = currentDragonExp;
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, tempPredictableLevel);//현재 레벨 요구 경험치

            if (IsMaxLevel(currentDragonLevel) || IsMaxLevel(tempPredictableLevel))
            {
                ToastManager.On(100000210);
                Debug.Log("더 이상 선택할 수 없습니다.");
                return;
            }
            else
            {
                var currentSelectedEXP = GetTotalExpByTargetList(selectedDataList.Values.ToList());//현재 걸려있는 경험치총량
                var currentDragonExp = User.Instance.DragonData.GetDragon(dragonTag).Exp;//현재 드래곤 경험치 총량
                var predictDragonExp = (currentSelectedEXP + currentDragonExp);//예상 총 경험치

                var predictData = CharExpData.GetLevelAndExpByTotalExp(currentDragonGrade, predictDragonExp);
                var predictReduce = predictData.ReduceExp;

                reduceExp = predictReduce;
            }

            var requireExp = totalExp - reduceExp;//레벨업에 필요한 요구 경험치 - calcTotalExp

            List<int> tempList = new List<int>();
            tempList.Clear();

            var checkLevelChanged = tempPredictableLevel;//등록 이전 레벨
            while (requireExp > 0)
            {
                var getCheckItemID = CalcEffectiveDiffBattery(requireExp);
                if (getCheckItemID < 0)
                    break;

                CustomClickInvenFrame(getCheckItemID.ToString());
                ItemBaseData itemInfo = ItemBaseData.Get(getCheckItemID);
                var itemValue = itemInfo.VALUE;
                requireExp -= itemValue;

                tempList.Add(getCheckItemID);
            }

            if (checkLevelChanged == tempPredictableLevel && tempList.Count > 0)//계산 결과 이후(tempPredictableLevel 값이 바뀜) 레벨
            {
                remainList.Clear();
                remainList = tempList.ToList();
            }
            else
            {
                if (autoLevelClickData.ContainsKey(tempPredictableLevel))
                {
                    autoLevelClickData[tempPredictableLevel].Clear();
                    autoLevelClickData[tempPredictableLevel] = tempList.ToList();
                }
                else
                    autoLevelClickData.Add(tempPredictableLevel, tempList);
            }
            RefreshUIProcess();
            DrawAllTableView();
        }

        void AutoDecrementSelect()//현재 레벨을 만들기 위해 저장되어있는 배터리 태그 값을 일괄 제거해줌
        {
            //여분 체크
            List<int> levelBatteryData = new List<int>();

            var remainData = remainList;
            if (remainData != null && remainData.Count > 0)
            {
                levelBatteryData = remainData.ToList();
                remainList.Clear();
            }
            else
            {
                levelBatteryData = autoLevelClickData[tempPredictableLevel].ToList();
                autoLevelClickData[tempPredictableLevel].Clear();
            }

            if (levelBatteryData == null || levelBatteryData.Count <= 0)
                return;

            Dictionary<int, int> batteryList = new Dictionary<int, int>();
            for (var i = 0; i < levelBatteryData.Count; i++)
            {
                if (!batteryList.ContainsKey(levelBatteryData[i]))
                    batteryList.Add(levelBatteryData[i], 0);

                batteryList[levelBatteryData[i]] += 1;
            }


            foreach(var data in batteryList)
            {
                SelectNodeParamProcess(data.Key, data.Value);
            }

            DrawAllTableView();
        }

        bool IsEqualDataSelectedNode()//현재 선택된 배터리와 (selectedNode 상의, 자동 선택된 노드와 같은지 체크 -> 다르면 강제 초기화)
        {
            if (autoLevelClickData == null)
                return false;

            Dictionary<int, int> currentAmountCount = new Dictionary<int, int>();
            currentAmountCount.Clear();

            if (IsEmptySelectBattery())
                return true;

            //현재 걸려있는 아이템 갯수 전체 파악
            selectedDataList.Values.ToList().ForEach((element) => {
                var itemID = element.ItemNo;
                var Amount = element.Amount;

                if (currentAmountCount.ContainsKey(itemID))
                    currentAmountCount.Add(itemID, Amount);
                else
                    currentAmountCount[itemID] = Amount;
            });

            Dictionary<int, int> autoClickData = new Dictionary<int, int>();
            autoClickData.Clear();

            List<int> autoClickKeys = new List<int>(autoLevelClickData.Keys);//각 레벨에 따른 배터리 리스트
            if (autoClickKeys == null || autoClickKeys.Count <= 0)
                return false;

            for (var i = 0; i < autoClickKeys.Count; i++)
            {
                var BatteryData = autoLevelClickData[autoClickKeys[i]];

                if (BatteryData == null || BatteryData.Count <= 0)
                    continue;

                for (var k = 0; k < BatteryData.Count; k++)
                {
                    var batteryID = BatteryData[k];

                    if (autoClickData.ContainsKey(batteryID))
                    {
                        var currentCount = autoClickData[batteryID];
                        autoClickData[batteryID] = currentCount + 1;
                    }
                    else
                        autoClickData.Add(batteryID, 1);
                }
            }

            List<int> baseKeys = new List<int>(autoClickData.Keys);
            if (baseKeys == null || baseKeys.Count <= 0)
                return false;

            var equalCount = 0;
            for (var i = 0; i < baseKeys.Count; i++)
            {
                var key = baseKeys[i];
                if (!autoClickData.ContainsKey(key) || !currentAmountCount.ContainsKey(key))
                    continue;

                var baseData = autoClickData[key];
                var selectData = currentAmountCount[key];

                if (baseData == selectData)
                    equalCount++;
            }

            return equalCount == baseKeys.Count && currentAmountCount.Count == equalCount;
        }

        void AutoMaxLevel()
        {
            if (IsEmptyInvenBattery())
            {
                if(!emptyBatteryFlag)
                {
                    ToastManager.On(100000208);
                    Debug.Log("보유 중인 건전지가 없습니다.");
                    emptyBatteryFlag = true;
                }
                
                return;
            }

            var reduceExp = currentDragonExp;
            var totalExp = CharExpData.GetCurrentRequireLevelExp(currentDragonGrade, tempPredictableLevel);//현재 레벨 요구 경험치

            if (IsMaxLevel(currentDragonLevel))
            {
                if(!maxBatteryFlag)
                {
                    ToastManager.On(100000210);
                    Debug.Log("더 이상 선택할 수 없습니다.");
                    maxBatteryFlag = true;
                }
                
                return;
            }
            else
            {
                var baseData = User.Instance.DragonData.GetDragon(dragonTag);
                var currentSelectedEXP = GetTotalExpByTargetList(selectedDataList.Values.ToList());//현재 걸려있는 경험치총량
                var currentDragonExp = baseData.Exp;//현재 드래곤 경험치 총량
                var predictDragonExp = (currentSelectedEXP + currentDragonExp);//예상 총 경험치

                var predictData = CharExpData.GetLevelAndExpByTotalExp(baseData.BaseData.GRADE, predictDragonExp);
                var predictReduce = predictData.ReduceExp;

                reduceExp = predictReduce;
            }

            var requireExp = totalExp - reduceExp;//레벨업에 필요한 요구 경험치 - calcTotalExp
            var reduceInventoryAmount = GetTotalExpByTargetList(invenDataList.Values.ToList());//현재 인벤토리 남은 배터리량
            if (requireExp > reduceInventoryAmount)
                return;

            List<int> tempList = new List<int>();
            tempList.Clear();

            var checkLevelChanged = tempPredictableLevel;
            while (requireExp > 0)
            {
                var getCheckItemID = CalcEffectiveDiffBattery(requireExp);
                if (getCheckItemID < 0)
                    break;

                SetSelectItemFrameByIndex(getCheckItemID);
                ItemBaseData itemInfo = ItemBaseData.Get(getCheckItemID);
                var itemValue = itemInfo.VALUE;
                requireExp -= itemValue;

                tempList.Add(getCheckItemID);
            }

            RefreshUIProcess(false);

            if (checkLevelChanged == tempPredictableLevel)
            {
                if (IsMaxLevel(checkLevelChanged))
                    return;

                remainList.Clear();
                remainList = tempList.ToList();
            }
            else
            {
                if (autoLevelClickData.ContainsKey(tempPredictableLevel))
                {
                    autoLevelClickData[tempPredictableLevel].Clear();
                    autoLevelClickData[tempPredictableLevel] = tempList.ToList();
                }
                else
                    autoLevelClickData.Add(tempPredictableLevel, tempList);
            }
        }

        int GetTotalExpByTargetList(List<batteryItem> _list)
        {
            int totalExp = 0;
            if (_list == null || _list.Count <= 0)
                return totalExp;

            _list.ForEach((element) => {
                var exp = element.GetEXP();
                var amount = element.Amount;

                if (amount < 0)
                    return;

                totalExp += (exp * amount);
            });
            return totalExp;
        }

        bool IsMaxLevel(int _targetLevel)
        {
            return DRAGON_MAX_LEVEL <= _targetLevel;
        }
        void RefreshLevelUpButton()
        {
            if (levelUpButton != null)
                levelUpButton.SetButtonSpriteState(!IsEmptySelectBattery());
        }
        void RefreshSelectEmptyLabel()
        {
            if (emptySelectLabel != null)
                emptySelectLabel.gameObject.SetActive(IsEmptySelectBattery());
        }
        void RefreshInvenEmptyLabel()
        {
            if (emptyInvenLabel != null)
                emptyInvenLabel.gameObject.SetActive(IsEmptyInvenBattery());
        }
    }
}