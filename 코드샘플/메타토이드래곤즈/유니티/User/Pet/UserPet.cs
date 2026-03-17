using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserPetStat
    {
        public int Key { get; private set; }
        public bool IsStatus1 { get; private set; }

        public UserPetStat(JArray array)
        {
            SetData(array);
        }
        public void SetData(JArray array)
        {
            if (array == null || array.Count < 2)
                return;

            Key = array[0].Value<int>();
            IsStatus1 = array[1].Value<int>() == 1;
        }

        public UserPetStat(PetStatData stat, bool geno)
        {
            SetData(stat, geno);
        }
        public void SetData(PetStatData stat, bool geno)
        {
            if (stat == null)
                return;

            Key = stat.KEY;
            IsStatus1 = geno;
        }
    }
    public class UserPet : ITableData
    {
        public int Tag { get; private set; } = -1;
        public int ID { get; private set; } = -1;
        public int Exp { get; private set; } = -1;
        public int Level { get; private set; } = -1;
        //강화 레벨
        public int Reinforce { get; private set; } = -1;
        //고유 스킬 번호 pet_skill_unique Key
        public int UniqueSkillID { get; private set; } = -1;
        //고유 스킬 번호 pet_skill_normal KeyArray
        public int[] SkillsID { get; private set; } = null;
        //고유 스킬 번호 pet_stat KeyArray
        public List<UserPetStat> Stats { get; private set; } = null;
        //부가 옵션 리스트
        public List<KeyValuePair<int, float>> SubOptionList { get; private set; } = new();
        //획득 시간
        public int Obtain { get; private set; } = -1;
        //장착했다면 장비한 드래곤 태그
        public int LinkDragonTag { get; private set; } = -1;

        public int ReinforceFalseCount { get; private set; } = -1;

        public UserPet(int tag, int id, int level, int exp, int reinForce = 0, int obtain = 0, int reinforceFalseCnt = 0)
        {
            if (Stats == null)
                Stats = new();
            else
                Stats.Clear();
            if (SubOptionList == null)
                SubOptionList = new();
            else
                SubOptionList.Clear();

            SetBaseData(tag, id, level, exp, reinForce, obtain, reinforceFalseCnt);
        }

        public void SetBaseData(int tag, int id, int level, int exp, int reinForce = 0, int obtain = 0, int reinforceFalseCnt = 0)
        {
            Tag = tag;
            ID = id;
            SetLevel(level);
            SetExp(exp);
            SetReinforce(reinForce);
            Obtain = obtain;
            ReinforceFalseCount = reinforceFalseCnt;
        }

        public void SetLevel(int value)
        {
            Level = value;
        }
        public void SetExp(int value)
        {
            Exp = value;
        }
        public void SetReinforce(int value)
        {
            Reinforce = value;
        }
        public void SetUniqueSkillID(int value)
        {
            UniqueSkillID = value;
        }
        public void SetStats(JArray[] value, JArray subValue = null) // 현재 서버에서 주는 건 신규 스텟이 아니라 전체 스텟을 줌 따라서 클리어 후에 다시 넣어줌
        {                                    // 추후 서버 구조 변경시 바꿀 것
            Stats.Clear();
            for(int i = 0, count = value.Length; i < count; ++i)
            {
                Stats.Add(new UserPetStat(value[i]));
            }
            SetPartOptionList(subValue);
        }

        public void SetPartOptionList(JToken partList)
        {
            if(partList == null)
            {
                SubOptionList = new();
                return;
            }

            if (SubOptionList == null)
                SubOptionList = new();
            else
                SubOptionList.Clear();

            var array = JArray.FromObject(partList);
            for (var i = 0; i < array.Count; i++)
            {
                var subOptionData = array[i];
                var optionData = JArray.FromObject(subOptionData);
                if (optionData == null)
                    continue;

                var key = optionData[0].Value<int>();
                var value = optionData[1].Value<float>();

                SubOptionList.Add(new(key, value));
            }
        }

        public void SetPetOption(KeyValuePair<int, float> option, int index)//simulator-add
        {
            if (SubOptionList == null)
                SubOptionList = new();

            if (SubOptionList.Count <= index)
            {
                var goalSize = index - SubOptionList.Count + 1;

                for (var i = 0; i < goalSize; i++)
                {
                    SubOptionList.Add(default);
                }

                SubOptionList[index] = option;
            }
            else
            {
                SubOptionList[index] = option;
            }
        }

        public void SetSkillsID(int[] value)// 추후 전환 완료 후 정리 HYEON
        {
            SkillsID = value;
        }
        public void SetLinkDragonTag(int value)
        {
            LinkDragonTag = value;
        }

        private PetBaseData petDesignData = null;
        public PetBaseData GetPetDesignData()
        {
            if (petDesignData == null)
                petDesignData = PetBaseData.Get(ID);

            return petDesignData;
        }

        public string Image()
        {
            if (GetPetDesignData() == null)
                return "";

            return GetPetDesignData().IMAGE;
        }

        public int Grade()
        {
            if (GetPetDesignData() == null)
                return -1;

            return GetPetDesignData().GRADE;
        }

        public int Element()
        {
            if (GetPetDesignData() == null)
                return -1;

            return GetPetDesignData().ELEMENT;
        }

        public string Name()
        {
            if (GetPetDesignData() == null)
                return "";

            return StringData.GetStringByStrKey(GetPetDesignData()._NAME);
        }

        public string Desc()
        {
            if (GetPetDesignData() == null)
                return "";

            return StringData.GetStringByStrKey(GetPetDesignData()._DESC);
        }

        public PetStatus GetALLStat()
        {
            PetStatus status = new PetStatus();
            status.Initialze();

            if (Stats.Count > 0)
            {
                for (int i = 0, count = Stats.Count; i < count; ++i)
                {
                    var petStat = Stats[i];
                    if (petStat == null)
                        continue;

                    var statData = PetStatData.Get(petStat.Key.ToString());
                    if (statData == null)
                        continue;

                    status.IncreaseStatus(statData.STAT_TYPE, statData.VALUE_TYPE, PetStatData.GetStatValue(petStat.Key.ToString(), Level, Reinforce, petStat.IsStatus1));
                }
            }

            if (SubOptionList.Count > 0)
                status.IncreaseStatus(SBFunc.GetSubOption(SubOptionList));

            return status;
        }

        public void Init() { }
        public string GetKey() { return Tag.ToString(); }
    }
}