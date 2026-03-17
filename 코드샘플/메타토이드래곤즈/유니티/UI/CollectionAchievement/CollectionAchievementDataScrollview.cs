using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 콜렉션 및 업적 테이블 + 컨디션(진행상태) 보여주는 스크롤 뷰.
    /// </summary>
    public class CollectionAchievementDataScrollview : MonoBehaviour, EventListener<CollectionAchievementUIEvent>
    {
        [SerializeField] TableView tableView = null;
        [SerializeField] Text emptyLabelText = null;
        [SerializeField] Text searchCountLabel = null;//검색 카운트 갯수 라벨 표시

        private bool isTableInit = false;

        eCollectionAchievementType tabType = eCollectionAchievementType.NONE;//업적 & 콜렉션 구분
        eCollectionAchievementFilterType filterType = eCollectionAchievementFilterType.NONE;//필터 타입
        eStatusType statType = eStatusType.NONE;//스탯효과 선택 타입
        eStatusValueType statValueType = eStatusValueType.VALUE;
        int currentSelectKey = -1;//현재 선택된 데이터 클론의 키값

        List<CollectionAchievement> originData = null;
        List<CollectionAchievement> viewData = null;

        private void OnEnable()
        {
            EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public void OnEvent(CollectionAchievementUIEvent eventType)
        {
            switch(eventType.Event)
            {
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.SEND_DEFAULT_SCROLLIVEW:
                    statType = eStatusType.NONE;                    
                    filterType = eCollectionAchievementFilterType.ALL;
                    break;
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.SEND_FILTER_SCROLLVIEW:
                    if (eventType.filterType == eCollectionAchievementFilterType.NONE)
                    {
                        statType = eventType.statType;
                        statValueType = eventType.statValueType;
                    }
                    else if ((eventType.filterType != eCollectionAchievementFilterType.NONE)
                        && (eventType.filterType != eCollectionAchievementFilterType.MAX))//전체, 완료, 미완료 필터
                        filterType = eventType.filterType;

                    SortData();//데이터 정렬 - 결과 데이터의 0번을 key로 던짐(눌렸다고 가정).

                    if(viewData.Count <= 0)//결과 데이터가 없을 때 처리 - detail 초기화 
                    {
                        currentSelectKey = -1;
                        DrawScrollView();//빈껍데기 그리기
                        CollectionAchievementUIEvent.InitDetailScrollView();
                        CollectionAchievementUIEvent.RefreshReddot();//유저가 입력을 할 상황에 갱신 요청
                    }
                    else
                        CollectionAchievementUIEvent.TouchDataUI(viewData[0].KEY);

                    break;
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.TOUCH_DATA_AUTO:
                    SortData();//데이터 정렬
                    if (viewData != null && viewData.Count > 0)
                        CollectionAchievementUIEvent.TouchDataUI(viewData[0].KEY, true);

                    break;
                case CollectionAchievementUIEvent.CollectionAchievementUIEventEnum.TOUCH_DATA://데이터 클릭하면 드로우
                    currentSelectKey = eventType.Key;
                    if(!eventType.isAutoSort)
                        CollectionAchievementUIEvent.RefreshReddot();//유저가 입력을 할 상황에 갱신 요청

                    DrawScrollView(eventType.isAutoSort);//그리기
                    break;
            }
        }

        public void InitUI(eCollectionAchievementType _type)
        {
            tabType = _type;
            InitScrollView();
            SetData();
        }

        void InitScrollView()
        {
            if (tableView != null && !isTableInit)
            {
                isTableInit = true;
                tableView.OnStart();
            }
        }

        void SetData()//데이터 세팅 (진행상황 + 테이블 데이터)
        {
            originData = CollectionAchievementManager.Instance.GetTotalDataByType(tabType);
        }

        public void DrawScrollView(bool initPos = true)
        {
            List<ITableData> tableViewItemList = new List<ITableData>();
            tableViewItemList.Clear();
            if (viewData != null && viewData.Count > 0)
            {
                for (var i = 0; i < viewData.Count; i++)
                {
                    var data = viewData[i];
                    if (data == null)
                        continue;

                    tableViewItemList.Add(data);
                }
            }

            tableView.SetDelegate(new TableViewDelegate(tableViewItemList, (GameObject node, ITableData item) => {
                if (node == null)
                    return;

                var frame = node.GetComponent<CollectionAchievementDataClone>();
                if (frame == null)
                    return;

                var data = (CollectionAchievement)item;
                frame.InitUI(tabType,data);
                frame.SetVisibleSelectNode(data.KEY == currentSelectKey);
            }));

            tableView.ReLoad(initPos);
        }

        /// <summary>
        /// 현재의 filter(전체, 완성, 미완성) 와 statTypeList(스탯 체크) 섞어서 적용
        /// filter가 전체 일 때는 
        /// 1. 미확인 달성 업적
        /// 2. 미완성 업적 (key 값 순)
        /// 3. 달성 업적(key 값 순서)
        /// </summary>
        void SortData()
        {
            GetStatFilterData();//스탯효과 필터 데이터 선 세팅

            //to do - 미확인 달성 업적 - local 저장해서 키 값으로 선 체크 해서 뽑아오기

            var tempPrevCompleteList = new List<CollectionAchievement>();//레드닷 완성 상태
            var tempIncompleteList = new List<CollectionAchievement>();//미완성
            var tempCompleteList = new List<CollectionAchievement>();//완성

            foreach(var data in viewData)//완성, 미완성 데이터 분류
            {
                if (data == null)
                    continue;

                List<CollectionAchievement> targetList;

                if (IsContainReddotList(data.KEY))//최근 완료한 데이터가 있다면
                    tempPrevCompleteList.Add(data);
                else
                {
                    targetList = IsCompleteUserData(data.KEY) ? tempCompleteList : tempIncompleteList;
                    targetList.Add(data);
                }
            }
            tempIncompleteList.Sort((a,b) => a.KEY - b.KEY);//key값 오름차순
            tempCompleteList.Sort((a,b) => a.KEY - b.KEY);//key값 오름차순
            
            switch(filterType)//현재 필터의 상태를 가지고 세팅
            {
                case eCollectionAchievementFilterType.ALL:
                    tempPrevCompleteList.AddRange(tempIncompleteList);
                    tempPrevCompleteList.AddRange(tempCompleteList);
                    viewData = tempPrevCompleteList.ToList();

                    CollectionAchievementManager.Instance.ClearReddotList(tabType);//타입(컬렉션 & 업적)에 따라 레드닷 리스트 삭제
                    break;
                case eCollectionAchievementFilterType.INCOMPLETE:
                    viewData = tempIncompleteList.ToList();
                    break;
                case eCollectionAchievementFilterType.COMPLETE:
                    viewData = tempCompleteList.ToList();
                    break;
            }

            SetEmptyText();
            SetCountLabelUI();
        }

        bool IsCompleteUserData(int _key)
        {
            return CollectionAchievementManager.Instance.IsCompleteUserData(tabType, _key);
        }

        void SetEmptyText()
        {
            if (emptyLabelText != null)
                emptyLabelText.gameObject.SetActive(viewData.Count <= 0);
        }

        void SetCountLabelUI()
        {
            if (searchCountLabel != null)
                searchCountLabel.text = StringData.GetStringFormatByStrKey("collection_research", viewData.Count);//검색 항목:{0}
        }

        List<CollectionAchievement> GetStatFilterData()
        {
            viewData = originData.ToList();
            if (statType == eStatusType.NONE)
                return viewData;
            else
            {
                return viewData = viewData.FindAll(element => element.StatType == statType && element.StatValueType == statValueType).ToList();
            }
        }

        bool IsContainReddotList(int _key)
        {
            return CollectionAchievementManager.Instance.IsContainReddotList(tabType,_key);
        }
    }
}
