using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SandboxNetwork
{
    public class UIPageViewController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public bool isPrefabLoadMode = false;       // contents 내용물을 프리팹 로드방식으로 할 것인지 체크

        public GameObject pageViewContent = null;
        public ToggleGroup pageToggleGroup = null;
        public Button prevPageButton = null;
        public Button nextPageButton = null;

        public UIPageController pageControl = null;

        public float animationDuration = 0.3f;

        float key1InTangent = 0;
        float key1OutTangent = 1.0f;
        float key2InTangent = 1.0f;
        float key2OutTangent = 0;

        bool isAnimating = false;                       // 애니메이션 재생 중임을 나타내는 플래그
        Vector2 destPosition = Vector2.zero;            // 최종 스크롤 위치
        Vector2 initialPosition = Vector2.zero;         // 자동 스크롤 시작 시 스크롤 위치
        AnimationCurve animationCurve;                  // 자동 스크롤에 관련된 애니메이션 커브


        public int CurrentPageIndex { get; private set; } = 0;               // 현재 페이지 인덱스
        int prevPageIndex = 0;                  // 이전 페이지 인덱스
        float  currentPageWidth = 0;

        Rect currentViewRect;                   // 스크롤 뷰 사각형 크기

        RectTransform pageViewTr = null;
        ScrollRect scrollRect = null;

        GridLayoutGroup gridLayout = null;

        GameObject contentPrefab = null;         // contents에 생성할 오브젝트

        Action pageChangedCallBack = null;

        private void Start()
        {
            // 프리팹 로드 모드가 아닌경우 자동 초기화
            if (isPrefabLoadMode == true) { return; }

            UpdatePadding();

            UpdatePageControl();
        }

        public void InitPageView()
        {
            pageViewTr = GetComponent<RectTransform>();
            scrollRect = GetComponent<ScrollRect>();

            gridLayout = scrollRect.content.GetComponent<GridLayoutGroup>();
        }

        public void SetPageChangedCallback(Action callback)
        {
            pageChangedCallBack = callback;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isAnimating = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // 현재 동작중인 스크롤 멈춤
            scrollRect.StopMovement();

            // 한 페이지 폭 계산
            currentPageWidth = -(gridLayout.cellSize.x + gridLayout.spacing.x);

            // 맞출 페이지의 인덱스 계산
            CurrentPageIndex = Mathf.RoundToInt((scrollRect.content.anchoredPosition.x) / currentPageWidth);

            if (CurrentPageIndex == prevPageIndex && Mathf.Abs(eventData.delta.x) >= 4)
            {
                // 일정 속도 이상으로 드래그할 경우 해당 방향으로 페이지 이동
                scrollRect.content.anchoredPosition += new Vector2(eventData.delta.x, 0);
                CurrentPageIndex += (int)Mathf.Sign(-eventData.delta.x);
            }

            UpdatePageView();
        }

        private void Update()
        {
            if (pageViewTr == null || currentViewRect == null)
                return;

            if (pageViewTr.rect.width != currentViewRect.width || pageViewTr.rect.height != currentViewRect.height)
            {
                UpdatePadding();
            }
        }

        private void LateUpdate()
        {
            if (isAnimating)
            {
                if (Time.time >= animationCurve.keys[animationCurve.length - 1].time)
                {
                    // 애니메이션 커브의 마지막 키프레임을 지나가면 애니메이션 종료
                    scrollRect.StopMovement();
                    scrollRect.content.anchoredPosition = destPosition;
                    isAnimating = false;
                    return;
                }

                // 애니메이션 커브를 사용하여 현재 스크롤 위치 계산 후 스크롤 뷰 이동
                Vector2 newPosition = initialPosition + (destPosition - initialPosition) * animationCurve.Evaluate(Time.time);
                scrollRect.content.anchoredPosition = newPosition;
            }
        }

        // 로드할 프리팹을 인스턴싱하고 내부의 자식들을 모두 Contents 자식으로 재편성
        public void SetPageViewController(GameObject contents)
        {
            if (isPrefabLoadMode == false) { return; }
            if (scrollRect == null) { return; }

            // 컨텐츠 내용 프리팹 생성
            contentPrefab = contents;
            contentPrefab.transform.SetParent(scrollRect.content.transform);
            contentPrefab.transform.localScale = Vector3.one;

            // 생성한 프리팹의 자식개체들을 모두 contents 자식으로 재편성
            while (contentPrefab.transform.childCount > 0)
            {
                Transform child = contentPrefab.transform.GetChild(0);
                child.SetParent(scrollRect.content.transform);
            }

            // 생성한 프리팹 (부모만 남은상태) off
            contentPrefab.SetActive(false);

            UpdatePageView();

            UpdatePadding();

            // 인디케이터 관련 설정
            UpdatePageControl();
        }

        public void MovePageByNumer(int number)
        {
            // 현재 동작중인 스크롤 멈춤
            scrollRect.StopMovement();

            // 한 페이지 폭 계산
            currentPageWidth = -(gridLayout.cellSize.x + gridLayout.spacing.x);

            CurrentPageIndex = number;

            UpdatePageView();
        }

        public void MoveNextPage()
        {
            // 현재 동작중인 스크롤 멈춤
            scrollRect.StopMovement();

            // 한 페이지 폭 계산
            currentPageWidth = -(gridLayout.cellSize.x + gridLayout.spacing.x);

            // 맞출 페이지의 인덱스 계산
            CurrentPageIndex = Mathf.RoundToInt((scrollRect.content.anchoredPosition.x) / currentPageWidth);

            CurrentPageIndex++;

            UpdatePageView();
        }

        public void MovePrevPage()
        {
            // 현재 동작중인 스크롤 멈춤
            scrollRect.StopMovement();

            // 한 페이지 폭 계산
            currentPageWidth = -(gridLayout.cellSize.x + gridLayout.spacing.x);

            // 맞출 페이지의 인덱스 계산
            CurrentPageIndex = Mathf.RoundToInt((scrollRect.content.anchoredPosition.x) / currentPageWidth);

            CurrentPageIndex--;

            UpdatePageView();
        }

        public int GetCurrentPageIndex()
        {
            return CurrentPageIndex;
        }

        void UpdatePageView()
        {
            // 첫 페이지 또는 끝페이지일 경우 처리
            if (CurrentPageIndex < 0)
            {
                CurrentPageIndex = 0;
            }
            else if (CurrentPageIndex > SBFunc.GetActiveChildCount(gridLayout.transform) - 1)
            {
                CurrentPageIndex = SBFunc.GetActiveChildCount(gridLayout.transform) - 1;
            }

            prevPageIndex = CurrentPageIndex;      // 현재 페이지의 인덱스 유지

            // 최종 스크롤 위치 계산
            float destX = CurrentPageIndex * currentPageWidth;
            destPosition = new Vector2(destX, scrollRect.content.anchoredPosition.y);

            // 시작할 때 스크롤 위치 저장
            initialPosition = scrollRect.content.anchoredPosition;

            // 애니메이션 커브 작성
            Keyframe keyFrame1 = new Keyframe(Time.time, 0, key1InTangent, key1OutTangent);
            Keyframe keyFrame2 = new Keyframe(Time.time + animationDuration, 1.0f, key2InTangent, key2OutTangent);
            animationCurve = new AnimationCurve(keyFrame1, keyFrame2);

            isAnimating = true;

            // 페이지 컨트롤 표시
            pageControl?.SetCurrentPage(CurrentPageIndex);

            // 버튼 상태 업데이트
            UpdateButtonState();

            // 콜백 실행
            pageChangedCallBack?.Invoke();
        }

        void UpdatePageControl()
        {
            if (pageControl != null)
            {
                if (pageViewContent != null)
                {
                    int activeChildCount = SBFunc.GetActiveChildCount(pageViewContent.transform);
                    pageControl.SetNumberOfPages(activeChildCount);
                }

                pageControl.SetCurrentPage(0);
            }
        }

        void UpdatePadding()
        {
            if (gridLayout == null || pageViewTr == null) { return; }

            // 스크롤 뷰의 사각형 크기 보존
            currentViewRect = pageViewTr.rect;

            int paddingH = Mathf.RoundToInt((currentViewRect.width - gridLayout.cellSize.x) / 2.0f);
            int paddingV = Mathf.RoundToInt((currentViewRect.height - gridLayout.cellSize.y) / 2.0f);
            gridLayout.padding = new RectOffset(paddingH, paddingH, paddingV, paddingV);
        }

        void UpdateButtonState()
        {
            if (gridLayout == null) { return; }
            if (prevPageButton == null || nextPageButton == null) { return; }

            prevPageButton.SetInteractable(CurrentPageIndex > 0);
            nextPageButton.SetInteractable(CurrentPageIndex < SBFunc.GetActiveChildCount(gridLayout.transform) - 1);
        }
    }
}