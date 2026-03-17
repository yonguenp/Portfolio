using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class UserDragon : ITableData
    {
        const int MAX_PART_SLOT_COUNT = 6;
        public int Tag { get; private set; } = -1;
        public eDragonState State { get; private set; } = eDragonState.Normal;
        public int Exp { get; private set; } = -1;
        public int Level { get; private set; } = -1;
        public int SLevel { get; private set; } = 1;
        public int Obtain { get; private set; } = -1;
        //part
        public int[] Parts { get; private set; } = new int[MAX_PART_SLOT_COUNT];
        /**
         * @return 장착한 part의 세트 효과
         */
        public int[] PartsSetList { get; private set; } = new int[3];
        //
        //pet
        public int Pet { get; private set; } = -1;
        //
        public TranscendenceData TranscendenceData { get; private set; } = new();
        public int TranscendenceStep { get => TranscendenceData.Step; }
        public int PassiveSkillSlot { get => TranscendenceData.PassiveSlot; }
        public List<int> PassiveSkills { get => TranscendenceData.PassiveSkills; }
        public GameObject MapDragon { get; private set; } = null;
        public int Fatigue { get; private set; } = 0;
        public int FatigueTimeStamp { get; private set; } = 0;


        private CharBaseData baseData = null;
        public CharBaseData BaseData
        {
            get
            {
                if(baseData == null)
                    baseData = CharBaseData.Get(Tag);
                return baseData;
            }
        }
        public CharacterStatus Status { get; protected set; } = null;

        public virtual void Init() { }
        public string GetKey() { return Tag.ToString(); }

        /// <summary> 드래곤 데이터를 업데이트 받거나 세팅하는건 이곳에서 </summary>
        /// <param name="tag"> Tag는 밖에서 체크하고 오기 </param>
        /// <param name="data"> 데이터 변동때 수정하기 용이하기 위함 </param>
        public void SetJsonData(int tag, JObject data)
        {
            // BaseData
            var exp = SBFunc.IsJTokenType(data["exp"], JTokenType.Integer) ? data["exp"].Value<int>() : -1;
            var level = SBFunc.IsJTokenType(data["level"], JTokenType.Integer) ? data["level"].Value<int>() : -1;
            var state = SBFunc.IsJTokenType(data["state"], JTokenType.Integer) ? data["state"].Value<int>() : -1;
            var sLevel = SBFunc.IsJTokenType(data["skill_level"], JTokenType.Integer) ? data["skill_level"].Value<int>() : 1;
            var obtain = SBFunc.IsJTokenType(data["obtain_at"], JTokenType.Integer) ? data["obtain_at"].Value<int>() : 1;
            Fatigue = SBFunc.IsJTokenType(data["fatigue"], JTokenType.Integer) ? data["fatigue"].Value<int>() : 0;
            if (SBFunc.IsJTokenType(data["fatigue_timestamp"], JTokenType.Integer))
                FatigueTimeStamp = data["fatigue_timestamp"].Value<int>();
            else if (SBFunc.IsJTokenType(data["fatigue_ts"], JTokenType.Integer))
                FatigueTimeStamp = data["fatigue_ts"].Value<int>();
            else
                FatigueTimeStamp = 0;

            SetBaseData(tag, (eDragonState)state, exp, level, sLevel, obtain);
            //

            //PartData
            if(data["parts"] != null && data["parts"].Type == JTokenType.Array)
            {
                SetPartData(data["parts"].ToObject<int[]>());
                Array.ForEach(Parts, (element) =>
                {
                    var partTag = element;
                    if (partTag > 0)
                        User.Instance.PartData.SetPartLink(partTag, tag);
                });
                SetPartSetEffectOption();
            }
            //

            //PetData
            if (SBFunc.IsJTokenType(data["pet"], JTokenType.Integer))
            {
                SetPetTag(data["pet"].Value<int>());
                User.Instance.PetData.SetPetLink(Pet, tag);
            }
            //

            // TranscendencdData
            TranscendenceData.SetData(data);
            //

            RefreshALLStatus();
        }
        /// <summary>
        /// 밖에서 깡통 드래곤 세팅 용도로 사용됨
        /// </summary>
        public void SetBaseData(int tag, eDragonState state, int exp, int level, int sLevel = 1, int obtain = -1)
        {
            Tag = tag;
            SetDragonState(state);
            SetExp(exp);
            SetLevel(level);
            SetSkillLevel(sLevel);
            Obtain = obtain;
        }
        /// <summary>
        /// 깡통 초월 드래곤이 필요하다면 사용
        /// </summary>
        /// <param name="transcendendceStep">초월 단계</param>
        /// <param name="passiveSkillSlot">패시브 슬롯</param>
        /// <param name="passiveSkills">패시브 스킬번호</param>
        public void SetTranscendenceData(int transcendendceStep, int passiveSkillSlot = 0, List<int> passiveSkills = null)
        {
            TranscendenceData.SetData(transcendendceStep, passiveSkillSlot, passiveSkills);
        }
        public void SetExp(int exp)
        {
            Exp = exp;
        }
        public void SetLevel(int level)
        {
            Level = level;
        }
        public void SetSkillLevel(int sLevel)
        {
            SLevel = sLevel;
        }
        public void SetDragonState(eDragonState state)
        {
            State |= state;
        }
        public void RemoveDragonState(eDragonState state)
        {
            State &= ~state;
        }
        public void SetPetTag(int pet)
        {
            Pet = pet;
        }
        public void SetMapDragon(GameObject mapDragon)
        {
            MapDragon = mapDragon;
        }
        public void SetPartData(int[] part)
        {
            if (part.Length < MAX_PART_SLOT_COUNT)
                Array.Resize(ref part, MAX_PART_SLOT_COUNT);

            Parts = part;
        }
        /**
         * 고정 길이 6
         *@return 드래곤이 장착한 파츠 리스트를 리턴
         */
        public virtual UserPart[] GetPartsList()
        {
            UserPart[] arrParts = new UserPart[6];

            foreach (var element in Parts.Select((value, index) => (value, index)))
            {
                var value = element.value;
                var index = element.index;

                arrParts[index] = User.Instance.PartData.GetPart(value);

            }

            return arrParts;
        }
        /**
         * @return 장비를 장착하기 위해서 가장 앞의 인덱스를 보냄
         */
        public int GetEmptySlotIndex()
        {
            var tempSlotIndex = -1;
            var partList = GetPartsList();
            if (partList == null || partList.Count() <= 0)
            {
                return tempSlotIndex;
            }

            for (var i = 0; i < partList.Count(); i++)
            {
                var partTag = partList[i];
                if (partTag == null)
                {
                    tempSlotIndex = i;
                    break;
                }
            }

            if (tempSlotIndex == -1)//전부 차있다.
            {
                tempSlotIndex = GetCurrentSlotOpenCount();
            }

            var currentSlotMaxCount = GetCurrentSlotOpenCount() - 1;//해금된 최대 슬롯 인덱스

            if (tempSlotIndex > currentSlotMaxCount)
            {
                tempSlotIndex = -1;//가득 차있음
            }
            return tempSlotIndex;
        }

        /**
         * @returns 현재 드래곤 레벨 기준으로 해금되는 장비 슬롯 갯수
         */
        public int GetCurrentSlotOpenCount()
        {
            return CharExpData.GetSlotCountByDragonLevel(BaseData.GRADE, Level);
        }
        
        public int Element()
        {
            if (BaseData == null)
            {
                return -1;
            }

            return BaseData.ELEMENT;
        }

        public string Image()
        {
            if (BaseData == null)
            {
                return "";
            }

            return BaseData.IMAGE;
        }

        public int Grade()
        {
            if (BaseData == null)
            {
                return -1;
            }
            return BaseData.GRADE;
        }

        public int JOB()
        {
            if (BaseData == null)
            {
                return -1;
            }
            return (int)BaseData.JOB;
        }
        public string Name()
        {
            if (BaseData == null)
            {
                return "";
            }
            return StringData.GetStringByStrKey(BaseData._NAME);
        }

        public string Desc()
        {
            if (BaseData == null)
            {
                return "";
            }

            return StringData.GetStringByStrKey(BaseData._DESC);
        }

        public eJoinedContentFilter JoinedContent()
        {
            eJoinedContentFilter joinContent = eJoinedContentFilter.None;
            //Tag
            if (User.Instance.PrefData.GetAdventureFormation().Contains(Tag))
            {
                joinContent |= eJoinedContentFilter.Adventure;
            }
            if (User.Instance.PrefData.GetArenaFomation().Contains(Tag))
            {
                joinContent |= eJoinedContentFilter.Arena_Atk;
            }
            if (User.Instance.PrefData.GetArenaFomation(false).Contains(Tag))
            {
                joinContent |= eJoinedContentFilter.Arena_Def;
            }
            if (User.Instance.PrefData.GetDailyFormation(DailyManager.Instance.GetDaily()).Contains(Tag))
            {
                joinContent |= eJoinedContentFilter.Daily_Dungeon;
            }
            for (int i = 0; i < 4; i++)
            {
                if (User.Instance.PrefData.GetWorldBossFormation(i).Contains(Tag))
                {
                    joinContent |= eJoinedContentFilter.World_boss;
                    break;
                }
            }

            return joinContent;
        }

        public int Position()
        {
            if (BaseData == null)
            {
                return -1;
            }

            return BaseData.POSITION;
        }

        public CharacterStatus GetDragonBaseStatus(int tag, int level)//: { HP: number, ATK: number, DEF: number, CRI: number, INF: number }
        {
            if (level <= 0)
                return null;

            var baseData = CharBaseData.Get(tag);
            if (baseData != null)
            {
                //2024-02-26 초월 추가스텟 변경 (자기속성 % 증폭 -> 레벨 스텟 보너스)
                //var stat = SBFunc.BaseCharStatus(level, baseData, StatFactorData.Get(baseData.FACTOR));
                if(TranscendenceStep > 0)
                {
                    /** BaseStat에 초월 스텟 추가. */
                    var transcendenceData = CharTranscendenceData.Get((eDragonGrade)Grade(), TranscendenceStep);
                    if (transcendenceData != null)
                    {
                        level += transcendenceData.ADD_STAT;
                        //switch (BaseData.ELEMENT_TYPE)
                        //{
                        //    case eElementType.FIRE:
                        //    {
                        //        stat.IncreaseStatus(eStatusCategory.RATIO, eStatusType.FIRE_DMG, transcendenceData.ADD_STAT);
                        //    } break;
                        //    case eElementType.WATER:
                        //    {
                        //        stat.IncreaseStatus(eStatusCategory.RATIO, eStatusType.WATER_DMG, transcendenceData.ADD_STAT);
                        //    } break;
                        //    case eElementType.EARTH:
                        //    {
                        //        stat.IncreaseStatus(eStatusCategory.RATIO, eStatusType.EARTH_DMG, transcendenceData.ADD_STAT);
                        //    } break;
                        //    case eElementType.WIND:
                        //    {
                        //        stat.IncreaseStatus(eStatusCategory.RATIO, eStatusType.WIND_DMG, transcendenceData.ADD_STAT);
                        //    } break;
                        //    case eElementType.LIGHT:
                        //    {
                        //        stat.IncreaseStatus(eStatusCategory.RATIO, eStatusType.LIGHT_DMG, transcendenceData.ADD_STAT);
                        //    } break;
                        //    case eElementType.DARK:
                        //    {
                        //        stat.IncreaseStatus(eStatusCategory.RATIO, eStatusType.DARK_DMG, transcendenceData.ADD_STAT);
                        //    } break;
                        //    default: break;
                        //}
                    }
                    //
                }

                return SBFunc.BaseCharStatus(level, baseData, StatFactorData.Get(baseData.FACTOR));
            }

            return null;
        }
        /**
         * return param
         * HP: number, ATK: number, DEF: number, CRI: number, INF: number,    //기본 스탯
         * HP_ADD: number, ATK_ADD: number, DEF_ADD: number, CRI_ADD: number   //추가 스탯 
         */
        public virtual CharacterStatus GetALLStatus(int customLevel = -1)
        {
            if (customLevel <= 0)
                customLevel = Level;

            var status = GetDragonBaseStatus(Tag, customLevel);
            if (status == null)
                return null;

            var addedStatus = new List<UnitStatus>();
            var skillData = BaseData.SKILL1;

            List<UserPart> equipedParts = new List<UserPart>();
            foreach (var element in Parts.Select((value, index) => (value, index)))
            {
                equipedParts.Add(User.Instance.PartData.GetPart(element.value));
            }

            equipedParts.ForEach((element) =>
            {
                if (element == null)
                    return;

                addedStatus.Add(element.GetALLStat());
            });

            //장착 세트 효과 계산
            if (PartsSetList != null && PartsSetList.Count() > 0)
                addedStatus.Add(SBFunc.GetPartSetEffectOption(PartsSetList));
            //

            var petData = User.Instance.PetData.GetPet(Pet);
            //펫 장착 시 스킬 효과 추가
            if (Pet > 0 && petData != null)
            {
                addedStatus.Add(petData.GetALLStat());

                var petBaseData = petData.GetPetDesignData();
                if (petBaseData != null)
                {
                    var rainforceData = PetReinforceData.GetDataByGradeAndStep(petBaseData.GRADE, petData.Reinforce);
                    if (rainforceData != null && petData.Element() == Element())//현재 드래곤과 펫의 속성이 같다면 보너스 수치 추가
                    {
                        if (rainforceData.ELEMENT_BUFF > 0)
                            status.IncreaseStatus(eStatusCategory.RATIO, petBaseData.ELEMENT_BUFF_TYPE, rainforceData.ELEMENT_BUFF);
                    }
                }
            }
            //
            //추가되는 스텟 반영
            for (int i = 0, count = addedStatus.Count; i < count; ++i)
            {
                if (addedStatus[i] == null)
                    continue;

                status.IncreaseStatus(addedStatus[i]);
            }
            //

            //업적 & 콜렉션 버프 추가
            if (User.Instance.UserData.ExtraStatBuff != null)
                status.IncreaseStatus(User.Instance.UserData.ExtraStatBuff.GetUserBuff());
            //

            //길드 버프
            if (!GuildManager.Instance.IsNoneGuild)
            {
                var guildStat = GuildManager.Instance.MyGuildInfo.GuildStatus;
                if(guildStat != null)
                    status.IncreaseStatus(guildStat);
            }

            CalcPassiveSkill(status);


            //스킬레벨 전투력 반영
            status.SetSkillLevel(SLevel);
            //계산
            status.CalcStatusAll();
            //
            return status;
        }
        /// <summary> 초월 드래곤 페시브 스킬 적용 추가(기본 스텟은 GetDragonBaseStatus에 있음) </summary>
        protected void CalcPassiveSkill(CharacterStatus status)
        {
            float ratio_passive_effect = status.GetStatus(eStatusCategory.RATIO, eStatusType.RATIO_PASSIVE_EFFECT) * SBDefine.CONVERT_FLOAT;
            if (TranscendenceData.Step > 0 && TranscendenceData.PassiveSlot > 0)
            {
                for (int i = 1, count = TranscendenceData.PassiveSlot; i <= count; ++i)
                {
                    var curPassive = TranscendenceData.GetPassiveData(i);
                    if (null == curPassive)
                        continue;

                    //StartType이 Always인 경우에만 기본 스텟에 바로반영
                    if (eSkillPassiveStartType.ALWAYS == curPassive.START_TYPE)
                    {
                        var category = eStatusCategory.ADD;
                        if (eStatusValueType.PERCENT == curPassive.EFFECT_VALUE)
                            category = eStatusCategory.RATIO;

                        switch (curPassive.PASSIVE_EFFECT)
                        {
                            case eSkillPassiveEffect.STAT:
                            {
                                status.IncreaseStatus(category, curPassive.STAT, curPassive.VALUE + (curPassive.VALUE * ratio_passive_effect));
                            } break;
                            case eSkillPassiveEffect.STAT_MAIN_ELEMENT:
                            {
                                status.IncreaseStatus(category, BaseData.ELEMENT_TYPE.StatDMG(), curPassive.VALUE + (curPassive.VALUE * ratio_passive_effect));
                            } break;
                            default: continue;
                        }
                    }
                    //
                }
            }
        }
        public void RefreshALLStatus(int customLevel = -1)
        {
            if (customLevel <= 0)
                customLevel = Level;

            Status = GetALLStatus(customLevel);
        }
        public CharacterStatus GetDragonCustomStat(int customLevel, int customSkillLevel, List<UserPart> equipedParts, int[] _partsSetList, UserPet petData)
        {
            if (customLevel <= 0)
                customLevel = Level;

            var status = GetDragonBaseStatus(Tag, customLevel);
            if (status == null)
                return null;

            var addedStatus = new List<UnitStatus>();
            var skillData = BaseData.SKILL1;

            equipedParts.ForEach((element) =>
            {
                if (element == null)
                    return;

                var stat = element.GetALLStat();
                if (stat == null)
                    return;

                addedStatus.Add(stat);
            });

            //장착 세트 효과 계산
            if (_partsSetList != null && _partsSetList.Count() > 0)
                addedStatus.Add(SBFunc.GetPartSetEffectOption(_partsSetList));

            //펫 장착 시 스킬 효과 추가
            if (petData != null)
            {
                addedStatus.Add(petData.GetALLStat());

                var petBaseData = petData.GetPetDesignData();
                if (petBaseData != null)
                {
                    var rainforceData = PetReinforceData.GetDataByGradeAndStep(petBaseData.GRADE, petData.Reinforce);
                    if (rainforceData != null && petData.Element() == Element())//현재 드래곤과 펫의 속성이 같다면 보너스 수치 추가
                    {
                        if (rainforceData.ELEMENT_BUFF > 0)
                            status.IncreaseStatus(eStatusCategory.RATIO, petBaseData.ELEMENT_BUFF_TYPE, rainforceData.ELEMENT_BUFF);
                    }
                }
            }

            for (int i = 0, count = addedStatus.Count; i < count; ++i)
            {
                if (addedStatus[i] == null)
                    continue;

                status.IncreaseStatus(addedStatus[i]);
            }
            //업적 & 콜렉션 버프 추가
            status.IncreaseStatus(User.Instance.UserData.ExtraStatBuff.GetUserBuff());

            status.SetSkillLevel(customSkillLevel);
            status.CalcStatusAll();


            return status;
        }

        public void SetPartSetEffectOption()//part List(tag List) 기준으로 세트 효과 만들기
        {
            PartsSetList = new int[3];
            PartsSetList = GetPartEffectOption(GetPartsList());
        }

        public int[] GetPartEffectOption(UserPart[] userPartList)
        {
            Dictionary<int, int> partSetDic = new Dictionary<int, int>();//key : part_set 테이블의 group값 , value : count 갯수

            foreach(var part in userPartList)
            {
                if (part == null)
                    continue;
                var designData = part.GetPartDesignData();
                if (designData == null)
                    continue;
                var setGroup = designData.SET_GROUP;

                if (partSetDic.ContainsKey(setGroup))
                {
                    var count = partSetDic[setGroup];
                    partSetDic[setGroup] = count + 1;
                }
                else
                    partSetDic.Add(setGroup, 1);
            }

            List<int> setTempList = new List<int>();//part_set Table의 key List
            
            foreach(var dicElement in partSetDic)
            {
                var key = dicElement.Key;//그룹값
                var count = dicElement.Value;//갯수

                var setDataList = PartSetData.GetOptionByGroupAndCount(key, count);
                if (setDataList != null && setDataList.Count > 0)
                {
                    foreach(var setData in setDataList)
                    {
                        setTempList.Add(int.Parse(setData.KEY));
                    }
                }
            }

            return setTempList.ToArray();
        }

        public float GetMoveSpeed()
        {
            if (BaseData == null)
            {
                return 1.0f;
            }

            return BaseData.MOVE_SPEED;
        }

        public int GetTotalINF()
        {
            if (Status == null)
                return 0;

            return Status.GetTotalINF();
        }
        public bool IsTranscendenceAble()
        {
            int requireLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_LEVEL", 50);
            int requireSkillLv = GameConfigTable.GetConfigIntValue("TRANSCENDENCE_MINIMUM_SKILL_LEVEL", 50);
            int requireGrade = CharTranscendenceData.GetStepMax((eDragonGrade)Grade());
            if (Level >= requireLv && SLevel>= requireSkillLv && requireGrade > 0)
            {
                return true;
            }
            return false;
        }
    }
}