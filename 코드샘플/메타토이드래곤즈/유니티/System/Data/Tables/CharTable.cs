using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace SandboxNetwork
{
    public class CharBaseTable : TableBase<CharBaseData, DBChar_base>
    {
        public static Sprite DefaultThumbnail { get; private set; } = null;
        public static Sprite DefaultClassIcon { get; private set; } = null;
        public static Sprite DefaultBackGround { get; private set; } = null;

        private Dictionary<int, List<CharBaseData>> dicGrade = new Dictionary<int, List<CharBaseData>>();
        public override void Init()
        {
            base.Init();
            dicGrade.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGrade.Clear();
        }

        protected override bool Add(CharBaseData data)
        {
            if (base.Add(data))
            {
                if (!dicGrade.ContainsKey(data.GRADE))
                    dicGrade.Add(data.GRADE, new List<CharBaseData>());

                if(data.IS_USE)
                    dicGrade[data.GRADE].Add(data);

                return true;
            }
            return false;
        }

        public List<CharBaseData> GetGradeAll(int grade)
        {
            if (dicGrade.ContainsKey(grade))
                return dicGrade[grade];

            return new List<CharBaseData>();
        }
        public override void Preload()
        {
            DefaultThumbnail = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, "enemy_0");
            DefaultClassIcon = ResourceManager.GetResource<Sprite>(eResourcePath.ClassIconPath, "icon_class_tanker");
            DefaultBackGround = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, "light_n_bg");
            Init();
            base.Preload();
            /// 전체 드래곤 수량을 보는게 있기 때문에 전부 로드
            LoadAll();
        }
        public CharBaseData GetDataBySkin(string SkinName)
        {
            foreach(var data in datas)
            {
                if (data.Value.SKIN == SkinName)
                    return data.Value;
            }

            return null;
        }
        public int GetTotalDragonCount(bool _includeInUse = false)
        {
            if (!_includeInUse)
                return datas.Values.ToList().FindAll(element => element.IS_USE).Count;
            else
                return datas.Values.ToList().Count;
        }
    }

    public class CharGradeTable : TableBase<CharGradeData, DBChar_grade>
    {
    }

    public class CharExpTable : TableBase<CharExpData, DBChar_exp>
    {
        Dictionary<int, List<CharExpData>> gradeDic = null;
        Dictionary<KeyValuePair<int, int>, CharExpData> gradeLevelDic = null;
        Dictionary<KeyValuePair<int, int>, CharExpData> dicOpenEquipSlot = null;
        public override void Init()
        {
            base.Init();
            if (gradeDic == null)
                gradeDic = new();
            else
                gradeDic.Clear();
            if (gradeLevelDic == null)
                gradeLevelDic = new();
            else
                gradeLevelDic.Clear();
            if (dicOpenEquipSlot == null)
                dicOpenEquipSlot = new();
            else
                dicOpenEquipSlot.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (gradeDic == null)
                gradeDic = new();
            else
                gradeDic.Clear();
            if (gradeLevelDic == null)
                gradeLevelDic = new();
            else
                gradeLevelDic.Clear();
            if (dicOpenEquipSlot == null)
                dicOpenEquipSlot = new();
            else
                dicOpenEquipSlot.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(CharExpData data)
        {
            if (base.Add(data))
            {
                gradeLevelDic.Add(new(data.GRADE, data.LEVEL), data);
                //아마도 다음 슬롯 최소 오픈 레벨을 알고 싶었던것 같음.
                var openKey = new KeyValuePair<int, int>(data.GRADE, data.OPEN_EQUIP_SLOT);
                if (!dicOpenEquipSlot.ContainsKey(openKey))
                    dicOpenEquipSlot.Add(openKey, data);
                else if (data.LEVEL < dicOpenEquipSlot[openKey].LEVEL)
                    dicOpenEquipSlot[openKey] = data;

                if (gradeDic.ContainsKey(data.GRADE))
                {
                    var expData = gradeDic[data.GRADE];
                    if (expData == null)
                        gradeDic[data.GRADE] = new List<CharExpData>();

                    if (!gradeDic[data.GRADE].Contains(data))
                        gradeDic[data.GRADE].Add(data);
                }
                else
                    gradeDic.Add(data.GRADE, new List<CharExpData>() { data });
                return true;
            }
            return false;
        }
        public CharExpData Get(int grade, int level)
        {
            return Get(new (grade, level));
        }
        public CharExpData Get(KeyValuePair<int, int> gradeLevelKey)
        {
            if (gradeLevelDic.TryGetValue(gradeLevelKey, out CharExpData value))
                return value;

            return null;
        }

        /**
         * @param currentLevel //현재 레벨
         * @param currentExp // 현재 드래곤 경험치
         * @param obtainExp // 획득 총 경험치
         * @param finallevel // 결과 레벨
         * @param reduceExp // 최종 레벨 도달 이후 잔여 경험치
         */

        public CharLevelExpData GetGradeAndLevelAddExp(int currentGrade, int currentLevel, int currentExp, int obtainExp)
        {
            CharExpData levelData = Get(currentGrade, currentLevel);
            int requireExp = levelData.EXP;//다음 렙업을 위한 총 경치량

            //최대값이 외부에서 제어될 경우를 대비해서 세팅(game_config로 최대 레벨 수치 제어)
            if (currentLevel == GameConfigTable.GetDragonLevelMax())
            {
                requireExp = 0;
            }

            var currentLevelRequireExp = requireExp - currentExp;// 현재 레벨에서 렙업을 위한 경치량

            if (requireExp == 0)//최대값 도달
            {
                var maxLevel = GameConfigTable.GetDragonLevelMax();
                var reduceExp = Get(currentGrade, maxLevel).EXP;

                var returnData = new CharLevelExpData();
                returnData.FinalLevel = maxLevel;
                returnData.ReduceExp = reduceExp;

                return returnData;
            }

            if (obtainExp >= currentLevelRequireExp)
            {
                return GetGradeAndLevelAddExp(currentGrade, currentLevel + 1, 0, obtainExp - currentLevelRequireExp);
            }
            else
            {
                var returnData = new CharLevelExpData();
                returnData.FinalLevel = currentLevel;
                returnData.ReduceExp = obtainExp;

                return returnData;
            }
        }

        //경험치를 가지고 현재 레벨과 잔여 경험치 구하기
        public CharLevelExpData GetLevelAndExpByGradeAndTotalExp(int grade, int inComeTotalExp)
        {
            int checkIndex = 0;
            if(gradeDic.TryGetValue(grade, out var gData))
            {
                foreach (var element in gData)
                {
                    int totalEXP = element.TOTAL_EXP;
                    int level = element.LEVEL;
                    if (totalEXP > inComeTotalExp)
                    {
                        checkIndex = level;
                        break;
                    }
                }

                int expectLevel = checkIndex - 1;
                if (expectLevel < 0)//만렙일 경우
                {
                    var maxLevelData = new CharLevelExpData();
                    maxLevelData.FinalLevel = GameConfigTable.GetDragonLevelMax();
                    maxLevelData.ReduceExp = 0;

                    return maxLevelData;
                }

                int expectEXP = inComeTotalExp - Get(grade, expectLevel).TOTAL_EXP;

                var returnData = new CharLevelExpData();
                returnData.FinalLevel = expectLevel;
                returnData.ReduceExp = expectEXP;

                return returnData;
            }
            return null;

        }
    
        //슬롯 인덱스에 따른 해금 레벨 - 슬롯 인덱스 자체는 외부에서 제어 최소 1이상
        public int GetUnLockLevelByGradeAndSlotIndex(int grade, int slotIndex)
        {
            var key = new KeyValuePair<int, int>(grade, slotIndex);
            if (!dicOpenEquipSlot.TryGetValue(key, out CharExpData data))
                return -1;

            int mininumLv = data.LEVEL;
            if (mininumLv == int.MaxValue) return -1;

            return mininumLv; //최소 요구 레벨 반환
        }
    }

    public class CharMergeBaseTable : TableBase<CharMergeBaseData, DBChar_merge_base>
    {
    }

    public class CharMergeListTable : TableBase<CharMergeListData, DBChar_merge_list>
    {
        Dictionary<int, List<CharMergeListData>> groupDic = null;

        public override void Init()
        {
            base.Init();
            groupDic = new();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (groupDic == null)
                groupDic = new();
            else
                groupDic.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(CharMergeListData data)
        {
            if (base.Add(data))
            {
                if (!groupDic.ContainsKey(data.GROUP))
                    groupDic[data.GROUP] = new();

                groupDic[data.GROUP].Add(data);
                return true;
            }
            return false;
        }

        public virtual List<CharMergeListData> GetByGroup(int group)
        {
            if (groupDic == null || !groupDic.ContainsKey(group))
                return null;

            return groupDic[group];
        }
    }
}