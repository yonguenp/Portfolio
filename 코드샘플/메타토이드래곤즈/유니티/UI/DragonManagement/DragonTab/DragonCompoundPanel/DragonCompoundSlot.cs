using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DragonCompoundSlot : MonoBehaviour
    {
        [SerializeField]
        protected GameObject ItemTemplate = null;
        [SerializeField]
        protected GameObject[] mergeNodes = null;

        [SerializeField]
        protected Image arrowImage = null;
        [SerializeField]
        protected Color defaultColor = new Color();
        [SerializeField]
        protected List<Color> arrowColorList = new List<Color>();

        [SerializeField]
        protected GameObject emptyResult = null;
        [SerializeField]
        protected GameObject completeResult = null;
        [SerializeField]
        protected Image completeBg = null;
        [SerializeField]
        protected List<Sprite> completeSpriteList = new List<Sprite>();
        [SerializeField]
        protected SlotFrameController completeFrame = null;
        [SerializeField]
        protected GameObject eventIcon = null;

        private List<UserDragonCard> currentList = new List<UserDragonCard>();
        public void Init(UserDragonSelectCardSlot _slotData, DragonCardFrame.func _func)
        {
            if (_slotData == null || _slotData.list == null)
                return;

            if(_slotData.list.Count <= 0)
            {
                RemoveAllFrame();
                InitResultCard();
                if (currentList == null)
                    currentList = new List<UserDragonCard>();
                currentList.Clear();
                return;
            }

            var list = _slotData.list;
            if (IsEqualList(currentList.ToList(), list.ToList()))
                return;

            currentList = list.ToList();
            var mergeCount = mergeNodes.Length;
            for (var i = 0; i < mergeCount; i++)
            {
                if (mergeNodes[i] == null)
                    continue;

                SBFunc.RemoveAllChildrens(mergeNodes[i].transform);
            }

            RefreshEnableSlot(list.Count > 0 ? list[0] : null);

            var selectCount = list.Count;
            if (selectCount <= mergeCount)
            {                
                for (var i = 0; i < selectCount; i++)
                {
                    if (mergeNodes[i] == null || list[i] == null)
                        continue;

                    var cNode = Instantiate(ItemTemplate, mergeNodes[i].transform);
                    if (cNode == null)
                        continue;

                    cNode.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                    cNode.SetActive(true);

                    var card = cNode.GetComponent<DragonCardFrame>();
                    if (card == null)
                        continue;

                    card.InitCardFrame(list[i], false, true);
                    card.ClickCallBack = _func;
                    card.SetVisibleMinusIcon(true);
                }
            }

            int maxcount = 0;
            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(list[0].CardGrade);
            if (mergeInfo != null)
                maxcount = mergeInfo.NEED_COUNT;

            if (selectCount >= maxcount && IsEqualGradeList(list))//결과 드래곤 세팅 & 화살표 컬러 변경
            {
                var data = GetMergeData(list);
                if (data == null)
                    return;

                SetResultCard(data.MATERIAL1_GRADE);
            }
            else//초기화
            {
                InitResultCard();
            }
        }

        void RefreshEnableSlot(UserDragonCard card)
        {
            int grade = 1;
            if (card != null)
                grade = card.CardGrade;

            int needCount = 4;
            var gradeData = CharMergeBaseData.GetMergeDataByGrade(grade);
            if(gradeData != null)
            {
                needCount = gradeData.NEED_COUNT;
            }

            for(int i = 0; i < 4; i++)
            {
                mergeNodes[i].transform.parent.gameObject.SetActive(i < needCount);
            }
        }

        bool IsEqualGradeList(List<UserDragonCard> _list)//리스트내부의 등급이 전부 동일한지
        {
            if (_list.Count <= 0)//들어가지 않았다면 넘기기
                return true;

            int maxcount = 0;
            var mergeInfo = CharMergeBaseData.GetMergeDataByGrade(_list[0].CardGrade);
            if (mergeInfo != null)
                maxcount = mergeInfo.NEED_COUNT;

            if (_list.Count != maxcount)
                return false;

            var listCount = _list.Select(x=>x.CardGrade).Distinct().ToList();//등급이 모두 같다는 가정하에, 중복제거를 하면 size 1
            return listCount.Count == 1;
        }

        private CharMergeBaseData GetMergeData(List<UserDragonCard> _list)
        {
            if (_list == null)
            {
                return null;
            }

            if (_list.Count > 0)
                return CharMergeBaseData.GetMergeDataByGrade(_list[0].CardGrade);
            
            return null;
        }

        bool IsEqualList(List<UserDragonCard> list1, List<UserDragonCard> list2)
        {
            list1.Sort(SortCardTagDescend);
            list2.Sort(SortCardTagDescend);

            if (list1.Count != list2.Count)
                return false;

            var isListsEqual = true;
            for (var i = 0; i < list1.Count; i++)
            {
                if (list2[i].CardTag != list1[i].CardTag)
                {
                    isListsEqual = false;
                }
            }
            return isListsEqual;
        }
        protected int SortCardTagDescend(UserDragonCard param_a, UserDragonCard param_b)
        {
            return param_b.CardTag - param_a.CardTag;
        }

        void RemoveAllFrame()
        {
            var mergeCount = mergeNodes.Length;
            for (var i = 0; i < mergeCount; i++)
            {
                if (mergeNodes[i] == null)
                    continue;

                SBFunc.RemoveAllChildrens(mergeNodes[i].transform);
            }

            RefreshEnableSlot(null);
        }

        void InitResultCard()
        {
            if (emptyResult != null)
                emptyResult.SetActive(true);
            if (completeResult != null)
                completeResult.SetActive(false);
            if (arrowImage != null)
                arrowImage.color = defaultColor;
        }

        void SetResultCard(int _grade)
        {
            if (emptyResult != null)
                emptyResult.SetActive(false);
            if (completeResult != null)
                completeResult.SetActive(true);

            var modifyIndex = _grade - 1;
            if (arrowImage != null && modifyIndex >= 0 && arrowColorList.Count > modifyIndex)
                arrowImage.color = arrowColorList[modifyIndex];
            if (completeBg != null && modifyIndex >= 0 && completeSpriteList.Count > modifyIndex)
                completeBg.sprite = completeSpriteList[modifyIndex];
            if (completeFrame != null)
                completeFrame.SetColor(_grade + 1);//상위 등급 프레임 색상으로 변경

            if (eventIcon != null)
            {
                eventIcon.SetActive(false);
                var data = CharMergeBaseData.GetMergeDataByGrade(_grade);
                if (data != null)
                {
                    int defualt = 0;
                    switch(_grade)
                    {
                        case 1:
                            defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_C", 400000);
                            break;
                        case 2:
                            defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_UC", 300000);
                            break;
                        case 3:
                            defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_R", 150000);
                            break;
                        case 4:
                            defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_U", 100000);
                            break;
                        case 5:
                            defualt = GameConfigTable.GetConfigIntValue("DEFUALT_MERGE_SUCCESS_RATE_L", 1000000);
                            break;
                    }
                    if(data.MERGE_SUCCESS_RATE > defualt)
                    {
                        eventIcon.SetActive(true);
                    }
                }
            }
        }
    }
}
