using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 월드 보스 추천덱 구성
/// 관통되는 규칙이 있지만, 기획기반(러프주의) 구성
/// 얼마나 바뀔지 모르는 로직이라 클래스로 따로 구성함.
/// </summary>

/*
 * 공격파티 구성 로직
 * 공격력 순으로 6마리 배치
 * 수치 동일할 경우 (스나->어쌔신->봄버->워리어->탱커->서포터) 순서로 체크
 * 직업도 동일할 경우 높은 티어 체크
 * 티어도 같으면 캐릭터 넘버 빠른 쪽
 */

/*
 * 방어 파티 구성 로직
 * 방어력*10+체력 순으로 6마리 배치
 * 수치 동일할 경우 (탱커->워리어->어쌔신->봄버->스나->서포터) 순서로 체크
 * 직업도 동일할 경우 높은 티어 체크
 * 티어도 같으면 캐릭터 넘버 빠른 쪽
 */

/*
 * 전체 로직
 * 위 규칙에 따라 공격 파티1부터 순서대로 배치
 */

//
namespace SandboxNetwork
{
    public static class Extensions
    {
        public static List<List<T>> partition<T>(this List<T> values, int chunkSize)
        {
            return values.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }

    //공/방에 따라 직업 채용 우선순위가 다름(오름차순 체크)
    public enum eAtkJobPriority
    {
        NONE = 0,
        SNIPER,
        ASSASSIN,
        BOMBER,
        WARRIOR,
        TANKER, 
        SUPPORTER,
        MAX
    }
    public enum eDefJobPriority
    {
        NONE = 0,
        TANKER,
        WARRIOR,
        ASSASSIN,
        BOMBER,
        SNIPER,
        SUPPORTER,
        MAX
    }

    public class WorldBossAutoDeckLogic
    {
        const int DECK_FULL_COUNT = 6;
        const int ATK_DECK_TOTAL_COUNT = 2;
        const int DEF_DECK_TOTAL_COUNT = 2;
        const int DEF_MULTIPLY_FACTOR = 10;

        List<UserDragon> userDragonList = new List<UserDragon>();

        //실험적 기능 테스트용
        List<UserDragon> atkDragonList = new List<UserDragon>();
        List<UserDragon> defDragonList = new List<UserDragon>();

        float GetAtkClassRetouchFactor(eJobType _type)
        {
            return _type switch
            {
                eJobType.TANKER => 0.7f,
                eJobType.WARRIOR => 0.7f,
                eJobType.ASSASSIN => 2,
                eJobType.BOMBER => 2,
                eJobType.SNIPER => 2,
                eJobType.SUPPORTER => 2,
                _ => 1
            };
        }

        float GetDefClassRetouchFactor(eJobType _type)
        {
            return _type switch
            {
                eJobType.TANKER => 2,
                eJobType.WARRIOR => 2,
                eJobType.ASSASSIN => 0.5f,
                eJobType.BOMBER => 0.5f,
                eJobType.SNIPER => 0.5f,
                eJobType.SUPPORTER => 0.5f,
                _ => 1
            };
        }

        eAtkJobPriority GetAtkPriorityConvertByJobType(eJobType _type)
        {
            return _type switch
            {
                eJobType.TANKER => eAtkJobPriority.TANKER,
                eJobType.WARRIOR => eAtkJobPriority.WARRIOR,
                eJobType.ASSASSIN => eAtkJobPriority.ASSASSIN,
                eJobType.BOMBER => eAtkJobPriority.BOMBER,
                eJobType.SNIPER => eAtkJobPriority.SNIPER,
                eJobType.SUPPORTER => eAtkJobPriority.SUPPORTER,
                _ => eAtkJobPriority.SUPPORTER
            };
        }

        eDefJobPriority GetDefPriorityConvertByJobType(eJobType _type)
        {
            return _type switch
            {
                eJobType.TANKER => eDefJobPriority.TANKER,
                eJobType.WARRIOR => eDefJobPriority.WARRIOR,
                eJobType.ASSASSIN => eDefJobPriority.ASSASSIN,
                eJobType.BOMBER => eDefJobPriority.BOMBER,
                eJobType.SNIPER => eDefJobPriority.SNIPER,
                eJobType.SUPPORTER => eDefJobPriority.SUPPORTER,
                _ => eDefJobPriority.SUPPORTER
            };
        }

        void SetUserDragonList()
        {
            userDragonList = User.Instance.DragonData.GetAllUserDragons();
        }

        #region 공격 세팅 로직
        //전투력 내림차순
        private int SortBattlePointDescend(UserDragon a, UserDragon b)
        {
            float factorA = a.GetTotalINF() * GetAtkClassRetouchFactor((eJobType)a.JOB());
            float factorB = b.GetTotalINF() * GetAtkClassRetouchFactor((eJobType)b.JOB());

            return factorB.CompareTo(factorA);
        }
        //직업 오름차순
        private int SortAtkJobPriorityAscend(UserDragon a, UserDragon b)
        {
            var priorityA = GetAtkPriorityConvertByJobType((eJobType)a.JOB());
            var priotityB = GetAtkPriorityConvertByJobType((eJobType)b.JOB());
            return priorityA.CompareTo(priotityB);
        }
        #endregion
        #region 방어 세팅 로직
        //특수 옵션 로직 (방어력*10+체력)내림차순
        private int SortCustomStatPointDescend(UserDragon a, UserDragon b)
        {
            var statA = a.GetALLStatus();
            var dragonDefA = statA.GetTotalStatus(eStatusType.DEF);
            var dragonHpA = statA.GetTotalStatus(eStatusType.HP);

            var statB = b.GetALLStatus();
            var dragonDefB = statB.GetTotalStatus(eStatusType.DEF);
            var dragonHpB = statB.GetTotalStatus(eStatusType.HP);

            var resultStatA = dragonDefA * DEF_MULTIPLY_FACTOR + dragonHpA * GetDefClassRetouchFactor((eJobType)a.JOB());
            var resultStatB = dragonDefB * DEF_MULTIPLY_FACTOR + dragonHpB * GetDefClassRetouchFactor((eJobType)b.JOB());

            return resultStatB.CompareTo(resultStatA);
        }

        //직업 오름차순
        private int SortDefJobPriorityAscend(UserDragon a, UserDragon b)
        {
            var priorityA = GetDefPriorityConvertByJobType((eJobType)a.JOB());
            var priotityB = GetDefPriorityConvertByJobType((eJobType)b.JOB());
            return priorityA.CompareTo(priotityB);
        }
        #endregion
        #region 공통 정렬 로직
        //등급 내림차순
        private int SortGradeDescend(UserDragon a, UserDragon b)
        {
            return b.Grade().CompareTo(a.Grade());
        }
        //태그(키 값) 오름 차순
        private int SortTagAscend(UserDragon a, UserDragon b)
        {
            return a.Tag.CompareTo(b.Tag);
        }
        #endregion

        private int AtkSort(UserDragon a, UserDragon b)
        {
            var checker = SortBattlePointDescend(a, b);

            if (checker == 0)
            {
                checker = SortAtkJobPriorityAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        return SortTagAscend(a, b);
                    }
                }
            }

            return checker;
        }

        private int DefSort(UserDragon a, UserDragon b)
        {
            var checker = SortCustomStatPointDescend(a, b);

            if (checker == 0)
            {
                checker = SortDefJobPriorityAscend(a, b);
                if (checker == 0)
                {
                    checker = SortGradeDescend(a, b);
                    if (checker == 0)
                    {
                        return SortTagAscend(a, b);
                    }
                }
            }

            return checker;
        }


        /// <summary>
        /// 일단 통 데이터(유저드래곤)를 한번에 정제해서 잘라서 리턴 해줌.
        /// 0번 인덱스는 공1 덱
        /// 1번 인덱스는 공2 덱
        /// 2번 인덱스는 방1 덱
        /// 3번 인덱스는 방2 덱
        /// -> 일단 12개 기준으로 밀어넣고, 공1/2 덱이 전부 찼다면, 방어 로직으로 변경해서 추가
        /// </summary>
        /// <returns></returns>
        public List<List<int>> GetTotalAutoMerge()
        {
            SetUserDragonList();

            var ret = new List<List<int>>();

            if (userDragonList == null || userDragonList.Count <= 0)
                return ret;

            var tempTotalList = userDragonList.ToList();
            tempTotalList.Sort((a, b) => AtkSort(a, b));//공격기준 소팅

            var splitAtkList = ListSplit(tempTotalList, DECK_FULL_COUNT);//공격 기준소팅 6개씩 자름
            var splitCount = splitAtkList.Count;

            int elementCount = 0;
            for(int i = 0; i< ATK_DECK_TOTAL_COUNT; i++)
            {
                if (i < splitCount)
                {
                    ret.Add(splitAtkList[i]);
                    elementCount += splitAtkList[i].Count;
                }
            }

            tempTotalList.RemoveRange(0, elementCount);//공격덱 세팅 삭제

            tempTotalList.Sort((a, b) => DefSort(a, b));//방어 기준 소팅

            var splitDefList = ListSplit(tempTotalList, DECK_FULL_COUNT);//공격 기준소팅 6개씩 자름
            splitCount = splitDefList.Count;
            for (int i = 0; i < DEF_DECK_TOTAL_COUNT; i++)
            {
                if (i < splitCount)
                    ret.Add(splitDefList[i]);
            }

            return ret;
        }

        List<List<int>> ListSplit(List<UserDragon> _dragonList, int _splitCount)
        {
            List<int> tagList = new List<int>();
            foreach (var data in _dragonList)
                if (data != null && data.Tag > 0)
                    tagList.Add(data.Tag);

            return tagList.partition(_splitCount);
        }

        /// <summary>
        /// 새로운 추천 조합 방식 - 공/방/공/방 순서대로 1마리씩 밀어넣음. 리스트를 미리 생성해놓음.
        /// 0 공 1 공 2 방 3 방
        /// 기본 로직 - 공 소팅 / 방 소팅 미리 세팅해두고 공격 넣을 때 방어에서 지우고 반복실행
        /// </summary>
        /// <returns></returns>
        public List<List<int>> GetTotalAutoMergeNewVersion()
        {
            if (atkDragonList == null)
                atkDragonList = new List<UserDragon>();
            if (defDragonList == null)
                defDragonList = new List<UserDragon>();

            atkDragonList = User.Instance.DragonData.GetAllUserDragons();
            defDragonList = User.Instance.DragonData.GetAllUserDragons();

            var ret = new List<List<int>>();
            
            if (atkDragonList == null || atkDragonList.Count <= 0)
                return ret;

            //각 타입별로 미리 소팅해둠.
            atkDragonList.Sort((a, b) => AtkSort(a, b));//공격기준 소팅
            defDragonList.Sort((a, b) => DefSort(a, b));//방어기준 소팅

            var userDragonCount = atkDragonList.Count;//현재 소지 카운트
            var totalDragonCount = DECK_FULL_COUNT * ATK_DECK_TOTAL_COUNT * DEF_DECK_TOTAL_COUNT;//총 24마리
            var totalDeckCount = ATK_DECK_TOTAL_COUNT + DEF_DECK_TOTAL_COUNT;

            for (int i = 0; i < totalDeckCount; i++)
            {
                ret.Add(new List<int>(){});//빈덱 미리 세팅
            }

            //공[0] 공[1] 방[2] 방[3] 
            //0 - [0],1 - [2],2 - [1],3 - [3],4 - [0],5,6
            for (int i = 0; i < userDragonCount; i++)
            {
                if (i >= totalDragonCount)//최대 24번
                    break;

                var modulerIndex = i % totalDeckCount;
                var checkDragonID = GetDragonIndex(modulerIndex);
                if (checkDragonID < 0)//end
                    break;

                int retIndex = 0;
                switch(modulerIndex)
                {
                    case 0:
                        retIndex = 0;
                        break;
                    case 1:
                        retIndex = 2;
                        break;
                    case 2:
                        retIndex = 1;
                        break;
                    case 3:
                        retIndex = 3;
                        break;
                }
                ret[retIndex].Add(checkDragonID);
            }

            for (int i = 0; i < totalDeckCount; i++)
            {
                var listCheck = ret[i];
                if(listCheck.Count < DECK_FULL_COUNT)
                    ret[i].AddRange(Enumerable.Repeat(0, DECK_FULL_COUNT - listCheck.Count).ToList());
            }

            return ret;
        }

        //0 공 1 방 2 공 3 방 ... 핑퐁
        int GetDragonIndex(int _modulerIndex)
        {
            int resultDragonID = -1;
            switch(_modulerIndex)
            {
                case 0:
                case 2:

                    if(atkDragonList.Count > 0 && defDragonList.Count > 0)
                    {
                        resultDragonID = atkDragonList[0].Tag;

                        var defDragonData = defDragonList.Find(element => element.Tag == resultDragonID);
                        if (defDragonData != null)
                            defDragonList.Remove(defDragonData);

                        atkDragonList.Remove(atkDragonList[0]);
                    }

                    break;
                case 1:
                case 3:
                    if (atkDragonList.Count > 0 && defDragonList.Count > 0)
                    {
                        resultDragonID = defDragonList[0].Tag;

                        var getSameDragonData = atkDragonList.Find(element => element.Tag == resultDragonID);
                        if (getSameDragonData != null)
                            atkDragonList.Remove(getSameDragonData);

                        defDragonList.Remove(defDragonList[0]);
                    }
                    break;
            }

            return resultDragonID;
        }





        /// <summary>
        /// 특정 덱에 대한 자동 세팅
        /// 현재 덱 자체에 대한 구분이 따로 없어서 하드코딩
        /// slotIndex 0~1 : 공격 // 2~3 : 방어
        /// </summary>
        /// <param name="_isAtk"></param>
        /// <returns></returns>
        public List<int> GetAutoMerge(int _curDeckSlot)
        {
            SetUserDragonList();

            //나머지 덱은 전체 리스트에서 제외시키기
            var exceptList = new List<UserDragon>();
            var exceptIndexList = new List<int>();
            var totalDeckCount = ATK_DECK_TOTAL_COUNT + DEF_DECK_TOTAL_COUNT;
            for(int i = 0; i< totalDeckCount; i++)
            {
                if (i == _curDeckSlot)
                    continue;

                var idList = User.Instance.PrefData.WorldBossFormationData.GetTemporaryFormation(i);
                exceptIndexList.AddRange(idList);
            }

            exceptIndexList.RemoveAll(element => element == 0);

            foreach(var id in exceptIndexList)
            {
                var dragonData = userDragonList.Find(element => element.Tag == id);
                if (dragonData != null)
                    exceptList.Add(dragonData);
            }

            var modifyList = userDragonList.Except(exceptList).ToList();//다른 덱에 등록된 드래곤 제외

            bool isAtk = _curDeckSlot >= 0 && _curDeckSlot <= 1 ? true : false;

            if(isAtk)
                modifyList.Sort((a, b) => AtkSort(a, b));//공격기준 소팅
            else
                modifyList.Sort((a, b) => DefSort(a, b));//방어기준 소팅

            var splitList = ListSplit(modifyList, DECK_FULL_COUNT).ToList();//공격 기준소팅 6개씩 자름

            if (splitList.Count > 0)
                return splitList[0];
            else
                return new List<int>() { 0,0,0,0,0,0};
        }
    }
}

