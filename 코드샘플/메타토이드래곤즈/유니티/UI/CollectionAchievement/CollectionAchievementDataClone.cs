using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// CollectionAchievementDataScrollview <-- 안에 다가 그릴 UI 구성에 필요한 객체.
    /// 콜렉션과 업적을 분리해서 세팅하려다가, tableView 통합화 될 것같아, 외부에서 type 체크해서 그리기로 변경.
    /// </summary>
    public class CollectionAchievementDataClone : MonoBehaviour
    {
        [Header("default UI")]
        [SerializeField] Text titleText = null;
        [SerializeField] Text rewardText = null;//스탯 타입, 수치값
        [SerializeField] Text detailText = null;//업적 상세 표시 라벨
        [SerializeField] GameObject selectNode = null;//선택 표시 노드

        [Header("Condition UI")]
        [SerializeField] GameObject completeBG = null;
        [SerializeField] GameObject incompleteBG = null;

        [SerializeField] GameObject completeNode = null;//완료 체크 아이콘 노드
        [SerializeField] GameObject conditionNode = null;//진행도 표시 라벨 노드
        [SerializeField] Text conditionText = null;//진행도 2/10

        [SerializeField] Color collectionTextColor = new Color();
        [SerializeField] Color collectionLineColor = new Color();

        [SerializeField] Color achievementTextColor = new Color();
        [SerializeField] Color achievementLineColor = new Color();


        eCollectionAchievementType tabType = eCollectionAchievementType.NONE;//업적 & 콜렉션 구분
        CollectionAchievement data = null;

        int key = -1;
        public int KEY { get { return key; } }
        public void InitUI(eCollectionAchievementType _type, CollectionAchievement _data)
        {
            if (_data == null)
                return;

            SetData(_type, _data);
            SetColor();
            SetUI();
        }

        void SetColor()
        {
            var isCollectionTab = tabType == eCollectionAchievementType.COLLECTION;
            if (selectNode != null)
                selectNode.GetComponent<Image>().color = isCollectionTab ? collectionLineColor : achievementLineColor;

            if (rewardText != null)
                rewardText.color = isCollectionTab ? collectionTextColor : achievementTextColor;
        }

        void SetData(eCollectionAchievementType _type, CollectionAchievement _data)//콜렉션인지 업적인지 구분
        {
            tabType = _type;
            data = _data;
            key = _data.KEY;
        }

        void SetUI()
        {
            SetSubjectText();
            SetRewardText();
            SetConditionUI();
        }
        void SetSubjectText()
        {
            titleText.text = StringData.GetStringByStrKey(data.NameKey); 
        }
        void SetRewardText()
        {
            rewardText.text = data.GetRewardValueToString();
        }

        void SetConditionUI()//완료 상태를 판단해서 라벨 표시 또는 완료 아이콘 표시
        {
            var isComplete = CollectionAchievementManager.Instance.IsCompleteUserData(tabType, data.KEY);
            completeNode.SetActive(isComplete);
            conditionNode.SetActive(!isComplete);

            incompleteBG.SetActive(!isComplete);
            completeBG.SetActive(isComplete);

            if(!isComplete)
                conditionText.text = data.GetCurrentConditionToString();
        }

        public void OnClickData()//클릭하면 상세 정보 이벤트
        {
            //if (tabType == eCollectionAchievementType.ACHIEVEMENT)
            //    return;

            CollectionAchievementUIEvent.TouchDataUI(data.KEY);
        }
        public void SetVisibleSelectNode(bool _isVisible)
        {
            if (selectNode != null)
                selectNode.SetActive(_isVisible);
        }
    }
}
