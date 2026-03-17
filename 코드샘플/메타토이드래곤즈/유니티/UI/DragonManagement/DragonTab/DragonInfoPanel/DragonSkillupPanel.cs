using Newtonsoft.Json.Linq;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    class MaterialInfo : ITableData//tableView 그리는 재료
    {
        public bool isCash;
        public int cashType;
        public int amount;
        public int itemID;

        public void Init() { }
        public string GetKey() { return ""; }
    }
    public class DragonSkillupPanel : DragonManageSubPanel
    {
        const int UI_SLOT_SCROLLVIEW_COUNT = 3;

        [SerializeField]
        GameObject skillLocked = null;

        [SerializeField]
        Button skillLevelUpBtn = null;

        [SerializeField]
        GameObject spineNodeParent = null;
        
        [SerializeField]
        GameObject itemPlusPrefab = null;
        [SerializeField]
        GameObject targetParent = null;
        [SerializeField]
        GameObject targetPrefab = null;

        [SerializeField]
        GameObject targetDescSlotParent = null;
        [SerializeField]
        GameObject targetDescSlotPrefab = null;
        [SerializeField]
        ScrollRect descScroll = null;
        
        [Space(10)]
        [Header("UI")]
        [SerializeField]
        Image sprMySkillIcon = null;
        [SerializeField]
        Text labelMySkillLevel = null;
        [SerializeField]
        Text labelMySkillName = null;
        [SerializeField]
        Text labelMySkillDesc = null;
        [SerializeField]
        Text labelNewSkillLevel = null;
        [SerializeField]
        Text labelSkillLevel = null;
        [SerializeField]
        GameObject maxSkillLabelNode = null;

        [Space(10)]
        [Header("Buttons")]
        [SerializeField]
        GameObject btnBundleObj = null;
        [SerializeField]
        Button btnBefSkill = null;
        [SerializeField]
        Button btnNextSkill = null;
        [SerializeField]
        Button btnMaxSkill = null;

        private SkeletonGraphic spine = null;
        private UserDragon dragon = null;

        private int curLevel = 0;
        private int uiNextLevel = 0;
        private int nextLevel = 0;
        private int maxLevel = 0;
        private int maxSkillLevel = 0;
        private Dictionary<string, int> reqItems = new Dictionary<string, int>();
        private Dictionary<string, int> reqCash = new Dictionary<string, int>();

        private SkillCharData myskilldata = null;

        private bool isClickLevelup = false;

        List<MaterialInfo> materialList = new List<MaterialInfo>();
        private bool isSufficientAmount = false;//재료 상태 체크

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

            if (materialList == null)
                materialList = new List<MaterialInfo>();
            materialList.Clear();

            Refresh();
        }

        void Refresh()
        {
            if (dragonTag <= 0)
            {
                Debug.Log("dragon Info is null");
                return;
            }

            isClickLevelup = false;

            if (spineNodeParent != null)
            {
                SBFunc.RemoveAllChildrens(spineNodeParent.transform);

                spine = null;
            }

            dragon = User.Instance.DragonData.GetDragon(dragonTag);
            dragonBase = CharBaseData.Get(dragon.Tag);//CharBaseTable.GetDragonData(dragon.Tag);

            myskilldata = dragonBase.SKILL1;
            if (myskilldata == null)
            {
                if (skillLocked != null)
                    skillLocked.SetActive(true);

                if (skillLevelUpBtn != null)
                    skillLevelUpBtn.SetButtonSpriteState(false);
                return;
            }

            if (skillLocked != null)
                skillLocked.SetActive(false);
            if (skillLevelUpBtn != null)
                skillLevelUpBtn.SetButtonSpriteState(true);
            if (maxSkillLabelNode != null)
                maxSkillLabelNode.SetActive(false);

            curLevel = dragon.SLevel;
            nextLevel = dragon.SLevel + 1;
            maxLevel = dragon.Level;
            maxSkillLevel = GameConfigTable.GetSkillLevelMax();
            SetMaxLevel();

            labelMySkillLevel.text = string.Format("Lv.{0}", dragon.SLevel);
            labelMySkillName.text = StringData.GetStringByStrKey(myskilldata.NAME);
            labelMySkillDesc.text = StringData.GetStringByStrKey(myskilldata.DESC) + string.Format(" (쿨타임 {0}초)", myskilldata.COOL_TIME);
            sprMySkillIcon.sprite = myskilldata.GetIcon();

            if (maxSkillLevel == curLevel)//스킬 만렙
            {
                btnBundleObj.SetActive(false);
                btnBefSkill.SetInteractable(false);
                btnNextSkill.SetInteractable(false);
                btnMaxSkill.SetInteractable(false);
                labelNewSkillLevel.text = string.Format("Lv.{0}", dragon.SLevel);
                labelSkillLevel.text = string.Format("Lv.{0}", dragon.SLevel);

                if (maxSkillLabelNode != null)
                    maxSkillLabelNode.SetActive(true);
                if (skillLevelUpBtn != null)
                    skillLevelUpBtn.SetButtonSpriteState(false);

                materialList.Clear();
                DrawScrollView();
                DrawSkillDescScrollView();
            }
            else
                RefreshSkillUI();
        }

        void RefreshSkillUI()
        {
            btnBundleObj.SetActive(true);
            SetNeedMaterial();//필요 재료 세팅
            SetButtonInteract();//버튼 세팅

            DrawScrollView();
            DrawSkillDescScrollView();

            RefreshLevelUpButton();//레벨업 버튼 상태 갱신
        }
        void SetButtonInteract()
        {
            // +2 일경우 이전 버튼을 활성
            btnBefSkill.SetInteractable(curLevel + 1 < nextLevel && curLevel != maxSkillLevel);
            btnMaxSkill.SetInteractable(nextLevel < maxLevel && nextLevel != maxSkillLevel);
            btnNextSkill.SetInteractable(nextLevel < maxSkillLevel);
        }

        void SetNeedMaterial()
        {
            reqItems = new Dictionary<string, int>();
            reqItems.Clear();

            reqCash = new Dictionary<string, int>();
            reqCash.Clear();

            materialList.Clear();

            uiNextLevel = curLevel;

            //최종 선택 레벨, 재료 정보
            while (IsCalculateNextLevel())
                if (maxSkillLevel <= uiNextLevel) break;

            labelNewSkillLevel.text = string.Format("Lv.{0}", nextLevel);
            labelSkillLevel.text = string.Format("Lv.{0}", nextLevel);

            List<string> keys = new List<string>(reqCash.Keys);//재료 아이템이 복수개로 늘었을 경우를 대비해 키 검사 및 재료 아이템 클론 생성
            keys.ForEach((element) => {
                var type = eGoodType.NONE;

                switch (element)
                {
                    case "CASH":
                        type = eGoodType.CASH;
                        break;

                    case "GOLD":
                        type = eGoodType.GOLD;
                        break;

                    case "GEMSTONE":
                        type = eGoodType.GEMSTONE;
                        break;
                }

                MaterialInfo tempInfo = new MaterialInfo();
                tempInfo.cashType = (int)type;
                tempInfo.isCash = true;
                tempInfo.amount = reqCash[element];
                tempInfo.itemID = 0;

                materialList.Add(tempInfo);
            });

            keys = new List<string>(reqItems.Keys);
            keys.ForEach((element) =>
            {
                MaterialInfo tempInfo = new MaterialInfo();
                tempInfo.cashType = 0;
                tempInfo.isCash = false;
                tempInfo.amount = reqItems[element];
                tempInfo.itemID = int.Parse(element);

                materialList.Add(tempInfo);
            });
        }

        void RefreshLevelUpButton()//드래곤 레벨보다 스킬렙이 낮고 & 스킬 맥렙보다 낮고 & 재료가 충분하고
        {
            if (skillLevelUpBtn != null)
                skillLevelUpBtn.SetButtonSpriteState(IsAvailableLevelupLevel() && !IsDragonMaxSkill() && IsSufficientMaterial());
        }

        void DrawScrollView()
        {
            SBFunc.RemoveAllChildrens(targetParent.transform);

            if (materialList == null || materialList.Count <= 0)
                return;

            isSufficientAmount = true;
            for (int i = 0; i< materialList.Count; i++)
            {
                
                GameObject plusObj = null;
                if (i > 0)
                    plusObj = Instantiate(itemPlusPrefab, targetParent.transform);
                
                var data = materialList[i];
                var isCash = data.isCash;
                var amount = data.amount;
                var cashType = data.cashType;
                var itemID = data.itemID;

                var clone = Instantiate(targetPrefab, targetParent.transform);
                var frame = clone.GetComponent<ItemFrame>();
                if (frame == null)
                {
                    Destroy(clone);
                    Destroy(plusObj);
                    continue;
                }

                if (isCash)
                {
                    frame.setFrameCashInfo(cashType, amount, true, true);
                    isSufficientAmount &= frame.IsSufficientAmount;
                }
                else
                {
                    frame.setFrameRecipeInfo(itemID, amount, true);
                    isSufficientAmount &= frame.IsSufficientAmount;
                }
            }
        }
        bool IsCalculateNextLevel()
        {
            if (uiNextLevel >= nextLevel)
                return false;

            var data = SkillLevelData.GetDataByJobAndLevel(dragon.BaseData.JOB, uiNextLevel);
            if (data == null)
                return false;

            int resultNum = data.ITEM;
            int resultCount = data.ITEM_NUM;
            if (resultNum < 0 || resultCount < 0)
                return false;

            if (resultCount > 0)
            {
                string index = resultNum.ToString();

                if (reqItems.ContainsKey(index))
                    reqItems[index] += resultCount;
                else
                    reqItems.Add(index, resultCount);
            }

            uiNextLevel += 1;

            return true;
        }

        void SetMaxLevel()
        {
            maxLevel = SkillLevelData.CalculateMaxLevel(dragon);
        }

        public void OnClickBefLevel()
        {
            nextLevel -= 1;
            RefreshSkillUI();
        }

        public void OnClickNewLevel()
        {
            if (dragon.Level > nextLevel)
            {
                nextLevel += 1;
                RefreshSkillUI();
            }
        }

        public void OnClickMaxLevel()
        {
            nextLevel = maxLevel;
            RefreshSkillUI();
        }

        bool IsAvailableLevelupLevel()//드래곤 레벨 >= 드래곤 스킬레벨 일 때 가능
        {
            return dragon.Level >= nextLevel;
        }
        bool IsDragonMaxSkill()//최대렙 달성했는지?
        {
            return dragon.SLevel >= maxSkillLevel;
        }

        bool IsSufficientMaterial()//재료 다있는지?
        {
            return isSufficientAmount;
        }

        public void OnClickLevelUp()
        {
            if (!IsAvailableLevelupLevel())//현재 드래곤 레벨보다 더 높게 레벨업 할 시
            {
                ToastManager.On(100000331);
                return;
            }
            if (IsDragonMaxSkill())
            {
                ToastManager.On(StringData.GetStringByStrKey("최대레벨도달"));
                return;
            }
            if (!IsSufficientMaterial())//재료 불충분
            {
                ToastManager.On(100000619);//재료 비용 부족
                return;
            }

            if (isClickLevelup)
                return;

            isClickLevelup = true;

            var param = new WWWForm();
            param.AddField("did", dragon.BaseData.KEY.ToString());
            param.AddField("targetlvl", nextLevel);

            NetworkManager.Send("dragon/skill", param, (jsonObj) =>
            {
                if (jsonObj.ContainsKey("rs"))
                {
                    var response = jsonObj["rs"].Value<int>();
                    if ((eApiResCode)response == eApiResCode.OK)
                    {
                        ForceUpdate();
                        CreateSpineAnimation();
                        isClickLevelup = false;

                        if (successCallback != null)
                            successCallback();
                    }
                    else
                        isClickLevelup = false;
                }
            });
        }

        public override void ForceUpdate()
        {
            Refresh();
        }
        void CreateSpineAnimation()
        {
            if (spine == null)
                CreateSpineNode();

            if (spine != null)
                spine.AnimationState.SetAnimation(0, "start", false);
        }

        void CreateSpineNode()
        {
            if (spine == null)
            {
                var spineClonePrefab = ResourceManager.GetResource<GameObject>(eResourcePath.EffectPrefabPath, "dragon_skill_levelup");
                if (spineClonePrefab != null && spineNodeParent != null)
                {
                    var clone = Instantiate(spineClonePrefab, spineNodeParent.transform);

                    spine = clone.GetComponentInChildren<SkeletonGraphic>();
                    spine.AnimationState.SetAnimation(0, "start", false);
                }
            }
        }

        void DrawSkillDescScrollView()
        {
            SBFunc.RemoveAllChildrens(targetDescSlotParent.transform);

            var originDesc = StringData.GetStringByStrKey(myskilldata.DESC);
            var summonData = SkillSummonData.Get(myskilldata.SUMMON_KEY);

            //혹시 모를 안전장치
            var count = 0;
            List<SkillEffectData> group = new();
            while(summonData != null && count < 100)
            {
                count++;                
                var groupData = SkillEffectData.GetGroup(summonData.EFFECT_GROUP_KEY);

                summonData = SkillSummonData.Get(summonData.NEXT_SUMMON);

                if (groupData == null || groupData.Count <= 0)
                    continue;

                group.AddRange(groupData);                
            }

            SetConvertStatStr(originDesc, group, curLevel, nextLevel);
        }
        void SetConvertStatStr(string _originDesc, List<SkillEffectData> _list, int _currentSlevel, int _nextLevel)
        {
            var splitList = GetSplitTypeComponent(_originDesc);//치환 해야하는 키값 - 이걸 들고 skillEffectData 에 해당하는 수치값 가져오기

            List<string> replaceList = new List<string>(); //치환 키워드 기준으로 desc 변경 값 생성하기

            var instantiateCount = 0;
            string effectString = "";
            float value = 0f;
            float nextValue = 0f;
            bool isPercent = false;
            bool isValueZeroFlag = false;//현재와 미래 예측치가 둘 다 0이면 만들필요없음 + 1019 WJ - 증감이 0 일때(증감이 바꾸지 않을때도 표시 안함)

            foreach (var splitStr in splitList)
            {
                if (string.IsNullOrEmpty(splitStr))
                    continue;

                var splitData = splitStr.Split('/');

                if (splitData.Length <= 1)
                    continue;

                var type = SBFunc.ConvertSkillEffectType(splitData[0]);
                if (splitData.Length == 2)
                {
                    var findEffectData = _list.Find(element => element.TYPE == type);
                    if (findEffectData == null)
                        continue;

                    var levelStatus = findEffectData.GetEffectStat(_currentSlevel);
                    var targetLevelStatus = findEffectData.GetEffectStat(_nextLevel);

                    value = levelStatus.GetStatByStr(splitData[1]);//split 값 그대로 씀
                    nextValue = targetLevelStatus.GetStatByStr(splitData[1]);
                    isPercent = findEffectData.VALUE_TYPE == eStatusValueType.PERCENT;

                    var modifyStr = SBFunc.StrBuilder("@", splitStr, "#");//이게 원본
                    effectString = StringData.GetStringByStrKey(modifyStr);//효과 이름
                    isValueZeroFlag = (nextValue - value == 0);
                }
                else if (splitData.Length == 3)//3개는 (Buff / Debuff/ Shield) -> stat 의 value값 고정
                {
                    var statType = SBFunc.ConvertStatusType(splitData[1]);
                    var valueType = SBFunc.ConvertValueType(splitData[2]);

                    var findEffectData = _list.Find(element => element.TYPE == type && element.STAT_TYPE == statType && valueType == element.VALUE_TYPE);
                    if (findEffectData != null)
                    {
                        var levelStatus = findEffectData.GetEffectStat(_currentSlevel);
                        var targetLevelStatus = findEffectData.GetEffectStat(_nextLevel);
                        if (levelStatus != null && targetLevelStatus != null)
                        {
                            value = levelStatus.VALUE;
                            nextValue = targetLevelStatus.VALUE;
                            isPercent = valueType == eStatusValueType.PERCENT;
                            
                            var modifyStr = SBFunc.StrBuilder("@", splitStr, "#");//이게 원본
                            effectString = StringData.GetStringByStrKey(modifyStr);//효과 이름
                            isValueZeroFlag = (nextValue - value == 0);
                        }
                    }
                }

                if(!string.IsNullOrEmpty(effectString) && !isValueZeroFlag)
                {
                    var slotData = Instantiate(targetDescSlotPrefab, targetDescSlotParent.transform);
                    var slotComp = slotData.GetComponent<DragonSkillupDescSlot>();
                    if (slotComp == null)
                    {
                        Destroy(slotData);
                        continue;
                    }

                    slotComp.SetData(effectString, value, nextValue, isPercent);
                    instantiateCount++;
                }
            }

            SetScrollviewSize(instantiateCount++);
        }
        List<string> GetSplitTypeComponent(string _originDesc)//DESC에 정의된 특문 기준으로 잘라서 배열화 -> skill Effect Data row의 키조합 str
        {
            if (string.IsNullOrEmpty(_originDesc))
                return new List<string>() {""};

            string beforeWord = "#";
            string afterWord = "@";

            List<string> splitList = new List<string>();

            string[] splitDatas = _originDesc.Split(new string[] { beforeWord }, StringSplitOptions.None);
            foreach (string s in splitDatas)
            {
                var keyWordArr = s.Split(new string[] { afterWord }, StringSplitOptions.None);
                if (keyWordArr.Length > 1)
                    splitList.Add(keyWordArr[1]);
            }

            return splitList;
        }

        void SetScrollviewSize(int _instantiateCount)
        {
            if (_instantiateCount >= UI_SLOT_SCROLLVIEW_COUNT)
            {
                targetDescSlotParent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
                targetDescSlotParent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                targetDescSlotParent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
                targetDescSlotParent.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            }
            else
            {
                targetDescSlotParent.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                targetDescSlotParent.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                targetDescSlotParent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                targetDescSlotParent.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            }

            if (descScroll != null)
                descScroll.verticalNormalizedPosition = 0f;
        }
    }
}

