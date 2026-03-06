using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    /*
     * 각 각 컴포넌트 모듈화 해서 사용
     * CollectionAchievementDataScrollview -> 업적 & 콜렉션 그려주는 스크롤뷰 (하위 각 타입에 맞게 레이어에서 생성) -> tableView로 가야함(갯수 너무 많음)
     * CollectionDataClone -> 업적과 class 분기 처리 할 생각이었지만, tableView 통합화 하면 그럴 필요 없을 것 같음.
     * 
     * CollectionAchievementEffectScrollview -> 업적 효과 (버프)
     * CollectionAchievementProgress -> 달성도
     */
    public class CollectionTabLayer : TabLayer
    {
        [SerializeField] CollectionAchievementDataScrollview dataScrollview = null;
        [SerializeField] CollectionAchievementEffectScrollview effectScrollview = null;
        [SerializeField] CollectionAchievementProgressController progressController = null;

        public override void InitUI(TabTypePopupData datas)//setTab 칠 때
        {
            base.InitUI(datas);

            var tabType = eCollectionAchievementType.COLLECTION;
            if (dataScrollview != null)
                dataScrollview.InitUI(tabType);
            if (effectScrollview != null)
                effectScrollview.InitUI(tabType);
            if (progressController != null)
                progressController.InitUI(tabType);

            //초기 세팅 - 필터 버튼UI 초기 세팅, 효과 초기 세팅, 첫번째 키가 눌렸다고 쏴야함.
            CollectionAchievementUIEvent.TouchFilterUI(eCollectionAchievementFilterType.ALL);//초기 필터 세팅
            CollectionAchievementUIEvent.SendDefaultScrollView();//디폴트 스크롤타입 세팅
            CollectionAchievementUIEvent.TouchDataAutoUI();//첫번째 데이터가 눌렸다고 가정
        }

        public override void RefreshUI()//popup ForceUpdate 
        {
            if (dataScrollview != null)
                dataScrollview.DrawScrollView();
        }
    }
}