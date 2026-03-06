using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주사위 보드 게임 레이어
/// </summary>
namespace SandboxNetwork
{
    public class DiceBoardSubLayer : SubLayer
    {
        const int MAX_BOARD_COLUMN_COUNT = 10;
        const int MAX_BOARD_ROW_COUNT = 6;
        const int OUTBOUND_WIDTH = 10;
        const int OUTBOUND_HEIGHT = 10;


        [SerializeField] GridLayoutGroup grid = null;
        [SerializeField] DiceBoardController boardController = null;
        [SerializeField] RectTransform descBg = null;

        public override bool backBtnCall() { return base.backBtnCall(); } //백 버튼 콜백이 없으면 false 를 출력
        public override void ForceUpdate() { }

        bool isFirstInit = false;

        public override void Init() 
        {
            if(!isFirstInit)
            {
                //RefreshBoardSize();//고정 해상도로 가서 필요없음
                SetBoardSlotData();

                isFirstInit = true;
            }
            else
                RefreshBoard();
        }

        void SetBoardSlotData()//한번만
        {
            if (boardController != null)
                boardController.SetBoardSlotData();
        }

        void RefreshBoard()
        {
            if (boardController != null)
                boardController.RefreshBoard();
        }

        void RefreshBoardSize()//해상도 - 입장시 최초 한번만 하면 될듯
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
            var curUISize = gameObject.GetComponent<RectTransform>().rect;

            if (grid == null)
                return;

            var offsetSize = grid.spacing;
            var slotWSize = (int) (curUISize.width - OUTBOUND_WIDTH * 2 - offsetSize.x * (MAX_BOARD_COLUMN_COUNT - 1)) / MAX_BOARD_COLUMN_COUNT;
            var slotHSize = (int) (curUISize.height - OUTBOUND_HEIGHT * 2 - offsetSize.y * (MAX_BOARD_ROW_COUNT - 1)) / MAX_BOARD_ROW_COUNT;

            var modifySize = slotHSize > slotWSize ? slotWSize : slotHSize;

            grid.cellSize = new Vector2(modifySize, modifySize);

            if(descBg != null)//안쪽 영역 사이즈 정의하기
            {
                var innerOffsetW = offsetSize.x * (MAX_BOARD_COLUMN_COUNT - 3);
                var innerOffsetH = offsetSize.y * (MAX_BOARD_ROW_COUNT - 3);

                descBg.sizeDelta = new Vector2(modifySize * (MAX_BOARD_COLUMN_COUNT - 2) + innerOffsetW - OUTBOUND_WIDTH * 2, modifySize * (MAX_BOARD_ROW_COUNT - 2) + innerOffsetH - OUTBOUND_HEIGHT * 2);
            }

            RefreshContentFitter(grid.GetComponent<RectTransform>());
        }

        private void RefreshContentFitter(RectTransform transform)
        {
            if (transform == null || !transform.gameObject.activeSelf)
                return;

            if (transform.gameObject.GetComponent<ParticleSystem>() != null)
                return;

            foreach (RectTransform child in transform)
            {
                RefreshContentFitter(child);
            }

            var layoutGroup = transform.GetComponent<LayoutGroup>();
            var contentSizeFitter = transform.GetComponent<ContentSizeFitter>();
            if (layoutGroup != null)
            {
                layoutGroup.SetLayoutHorizontal();
                layoutGroup.SetLayoutVertical();
            }

            if (contentSizeFitter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }
        }
    }
}
