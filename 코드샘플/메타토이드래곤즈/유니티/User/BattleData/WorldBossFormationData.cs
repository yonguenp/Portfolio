using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossFormationData
    {
        public const int MAX_PARTY_COUNT = 4;
        public const int MAX_DRAGON_COUNT = 6;
        public List<List<int>> TeamFormation { get; private set; } = new List<List<int>>();
        public List<List<int>> TeamPart { get; private set; } = new List<List<int>>();

        /// <summary>
        /// 월드 보스 팀 세팅 시 임시저장소
        /// </summary>
        public List<List<int>> TeamTemporarystorage { get; private set; } = new List<List<int>>();

        #region worldTeamSetting Test
        //팀배치 데이터
        //public class FormationParty
        //{
        //    public List<List<int>> TeamFormation { get; private set; } = new List<List<int>>();
        //    public List<List<int>> TeamPart { get; private set; } = new List<List<int>>();

        //    public void Clear()
        //    {
        //        TeamFormation.Clear();
        //        TeamPart.Clear();
        //    }

        //    public void Set(List<int> deck, List<int> item)
        //    {
        //        TeamFormation.Add(deck);
        //        TeamPart.Add(item);
        //    }
        //}

        //private FormationParty[] Party = new FormationParty[MAX_PARTY_COUNT];

        //public void SetData(JObject jsonData)//jsonData: any
        //{
        //    if (jsonData == null) { return; }

        //    foreach(var party in Party)
        //    {
        //        party.Clear();
        //    }

        //    var properties = jsonData.Properties();
        //    foreach (var obj in properties.Select((value, i) => (value, i)))
        //    {
        //        string key = obj.value.Name;
        //        int index = obj.i;

        //        int[][] deckArray = new int[MAX_PARTY_COUNT][];
        //        for(int i = 0; i < MAX_PARTY_COUNT; i++)
        //        {
        //            deckArray[i] = new int[MAX_DRAGON_COUNT] { 0, 0, 0, 0, 0, 0 };
        //        }

        //        JArray jsonDeck = JArray.Parse(jsonData[key]["deck"].Value<string>());
        //        if (jsonDeck.Count > 0)
        //        {
        //            for (var i = 0; i < MAX_PARTY_COUNT * MAX_DRAGON_COUNT; i++)
        //            {
        //                int party = i / MAX_DRAGON_COUNT;
        //                int member = i % MAX_DRAGON_COUNT;
        //                var checkIndex = 0;
        //                var data = jsonDeck[i].Value<string>();
        //                var isInt = int.TryParse(data, out checkIndex);
        //                if (isInt)
        //                {
        //                    deckArray[party][member] = checkIndex;
        //                }
        //            }
        //        }

        //        List<int> itemList = new List<int>();
        //        JToken jsonItem = JArray.Parse(jsonData[key]["items"].Value<string>());
        //        itemList = jsonItem.ToObject<List<int>>();

        //        for (int i = 0; i < MAX_PARTY_COUNT; i++)
        //        {
        //            Party[i].Set(deckArray[i].ToList(), itemList.GetRange(MAX_DRAGON_COUNT * i, MAX_DRAGON_COUNT * (i + 1)));
        //        }
        //    }
        //}

        //public void AllClearFormationData()
        //{
        //    foreach (var party in Party)
        //    {
        //        party.Clear();
        //    }
        //}

        //public void ClearFomationData(int party, int tag)
        //{
        //    Party[party].TeamFormation[tag].Clear();
        //}

        //public void SetFormationData(int party, int tag, List<int> dragonList)
        //{
        //    if (Party[party].TeamFormation.Count > tag)
        //        Party[party].TeamFormation[tag] = dragonList;
        //    else
        //        Party[party].TeamFormation[0] = dragonList;
        //}
        //public List<int> GetFormaiotnData(int party, int tag)
        //{
        //    if (0 <= tag && Party[party].TeamFormation.Count > tag)
        //        return Party[party].TeamFormation[tag];

        //    return null;
        //}
        #endregion

        public void SetData(JArray jsonData)//jsonData: any
        {
            if (jsonData == null) { return; }

            TeamFormation.Clear();
            TeamPart.Clear();

            foreach(var teamFormation in jsonData)
            {
                var dragonAndItem = (JArray)teamFormation;
                var tempDragonList = dragonAndItem[0];
                var tempItemList = dragonAndItem[1];
                var dragon = tempDragonList.ToObject<List<int>>();
                var itemList = tempItemList.ToObject<List<int>>();

                if (dragon.Count < MAX_DRAGON_COUNT)
                {
                    var remainCount = MAX_DRAGON_COUNT - dragon.Count;
                    for (int i = 0; i < remainCount; i++)
                        dragon.Add(0);
                }

                TeamFormation.Add(dragon);
                TeamPart.Add(itemList);
            }

            //var properties = jsonData.Properties();
            //foreach (var obj in properties.Select((value, i) => (value, i)))
            //{
            //    string key = obj.value.Name;
            //    int index = obj.i;
            //    List<int> deckList = new List<int>();
            //    JArray jsonDeck = JArray.Parse(jsonData[key]["deck"].Value<string>());
            //    if (jsonDeck.Count > 0)
            //    {
            //        for (var i = 0; i < jsonDeck.Count; i++)
            //        {
            //            var checkIndex = 0;
            //            var data = jsonDeck[i].Value<string>();
            //            var isInt = int.TryParse(data, out checkIndex);
            //            if (isInt)
            //            {
            //                deckList.Add(checkIndex);
            //            }
            //            else
            //            {
            //                deckList.Add(0);
            //            }
            //        }
            //    }
            //    TeamFormation.Add(deckList);
            //    List<int> itemList = new List<int>();
            //    JToken jsonItem = JArray.Parse(jsonData[key]["items"].Value<string>());
            //    itemList = jsonItem.ToObject<List<int>>();
            //    TeamPart.Add(itemList);
            //}
        }

        public void AllClearFormationData(bool _sizeClear = true)
        {
            if(_sizeClear)
                TeamFormation.Clear();
            else
            {
                for(int i = 0; i < TeamFormation.Count; i++)
                {
                    var list = TeamFormation[i];
                    if (list != null)
                        list.Clear();
                }
            }
        }

        public void ClearFomationData(int tag)
        {
            for (int i = 0; i < MAX_PARTY_COUNT; i++)
            {
                if (TeamFormation.Count > i)
                    continue;

                TeamFormation.Add(new List<int>());
            }

            if (TeamFormation.Count > tag)
            {
                TeamFormation[tag].Clear();
            }
        }

        public void SetFormationData(int tag, List<int> dragonList)
        {
            for (int i = 0; i < MAX_PARTY_COUNT; i++)
            {
                if (TeamFormation.Count > i)
                    continue;

                TeamFormation.Add(new List<int>());
            }

            if (TeamFormation.Count > tag)
                TeamFormation[tag] = dragonList.ToList();
        }
        public List<int> GetFormationData(int tag)
        {
            if (0 <= tag && TeamFormation.Count > tag)
                return TeamFormation[tag];

            return null;
        }

        public bool RemoveSpecificDragon(int _DeckIndex, int _dragonTag)
        {
            if (_dragonTag <= 0)
                return false;

            if (TeamFormation.Count <= _DeckIndex || TeamFormation.Count < 0)
                return false;

            var list = TeamFormation[_DeckIndex];
            var check = list.FindIndex(element => element == _dragonTag);
            if (check >= 0)
            {
                list[check] = 0;
                return true;
            }
            else
                return false;
        }

        public bool HasFormation()
        {
            if (TeamFormation != null)
            {
                foreach (var t in TeamFormation)
                {
                    if (t != null)
                    {
                        if (t.Count > 0)
                        {
                            foreach (var f in t)
                            {
                                if (f > 0)
                                    return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool IsEmpty(List<int> _formation)
        {
            if (_formation == null || _formation.Count <= 0)
                return true;

            foreach(var id in _formation)
            {
                if (id > 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 요청하려는 드래곤 덱 id 리스트 
        /// size 6 고정(그것보다 작을 경우 0 넣어서 사이즈 맞출 것.)
        /// </summary>
        /// <param name="_totalDragonList"></param>
        /// <returns></returns>
        public JObject GetWorldBossFormation(List<List<int>> _totalDragonList)
        {
            JObject totalParam = new JObject();
            for (int i = 0; i < _totalDragonList.Count; i++)
            {
                var dragonIDList = _totalDragonList[i];
                if (dragonIDList.Count < 6)
                {
                    var remainCount = 6 - dragonIDList.Count;
                    if (remainCount > 0)
                    {
                        for (int k = 0; k < remainCount; k++)
                            dragonIDList.Add(0);
                    }
                }

                JArray innerParam = new JArray();
                innerParam.Add(new JArray(dragonIDList));
                innerParam.Add(new JArray(new int[]{0,0,0}));

                totalParam.Add(i.ToString(), innerParam);
            }

            return totalParam;
        }

        public JObject GetWorldBossFormation(Dictionary<int, List<int>> _totalDragonDic)
        {
            if (_totalDragonDic == null || _totalDragonDic.Count <= 0)
                return null;

            JObject totalParam = new JObject();

            var keys = _totalDragonDic.Keys.ToList();
            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var dragonIDList = _totalDragonDic[key];
                if (dragonIDList.Count < 6)
                {
                    var remainCount = 6 - dragonIDList.Count;
                    if (remainCount > 0)
                    {
                        for (int k = 0; k < remainCount; k++)
                            dragonIDList.Add(0);
                    }
                }

                JArray innerParam = new JArray();
                innerParam.Add(new JArray(dragonIDList));
                innerParam.Add(new JArray(new int[] { 0, 0, 0 }));

                totalParam.Add(key.ToString(), innerParam);
            }

            return totalParam;
        }

        //빈덱 상태 만들기 - 안씀 삭제 예정
        public JObject GetWorldBossEmptyFormation()
        {
            JObject totalParam = new JObject();
            for (int i = 0; i < MAX_PARTY_COUNT; i++)
            {
                JArray innerParam = new JArray();
                innerParam.Add(new JArray(new int[] { 0, 0, 0, 0, 0, 0 }));
                innerParam.Add(new JArray(new int[] { 0, 0, 0 }));

                totalParam.Add(i.ToString(), innerParam);
            }

            return totalParam;
        }


        public void SetAutoFormationData(Dictionary<int, List<int>> _dic)
        {
            if (_dic == null || _dic.Count <= 0)
            {
                Debug.LogError("SetAutoFormationData size error  target size : " + _dic.Count + "default size : " + MAX_PARTY_COUNT);
                return;
            }

            var keys = _dic.Keys.ToList();

            foreach(var key in keys)
            {
                var dicData = _dic[key];
                if (dicData == null || dicData.Count <= 0)
                    continue;

                ClearFomationData(key);
                SetFormationData(key, _dic[key].ToList());
            }
        }

        public int GetTotalFormationDragonCount()
        {
            int totalCount = 0;
            int formationCount = TeamFormation.Count;
            for (int i = 0; i< MAX_PARTY_COUNT; i++)
            {
                if(formationCount > i)
                {
                    var curList = TeamFormation[i].ToList();
                    var dragonElementList = curList.FindAll(element => element > 0);
                    totalCount += dragonElementList != null ? dragonElementList.Count : 0;
                }
            }

            return totalCount;
        }

        #region dirtyFlag 용도의 임시저장 변수

        public void InitTemporaryFormation(bool _useServerCopy = true)
        {
            if (TeamTemporarystorage == null)
                TeamTemporarystorage = new List<List<int>>();

            TeamTemporarystorage.Clear();

            if(_useServerCopy)
            {
                foreach(var formation in TeamFormation)
                {
                    if (formation == null)
                        continue;

                    TeamTemporarystorage.Add(formation.ToList());
                }
            }
        }

        public bool isEqualFormationFromServerData(int _index , List<int>battleLine)
        {
            if (TeamTemporarystorage.Count != TeamFormation.Count)
                return false;

            for(int i = 0; i < TeamFormation.Count; i++)
            {
                var tempList = TeamTemporarystorage[i];
                var serverDeck = TeamFormation[i];

                if (_index == i)
                    tempList = battleLine;

                if (!ArrayEqual(tempList, serverDeck))
                    return false;
            }
            return true;
        }

        public List<bool> GetEqualCheckListFromServerData(int _index, List<int> battleLine)
        {
            List<bool> checkList = new List<bool>();
            if (TeamTemporarystorage.Count != TeamFormation.Count)
                return checkList;

            for (int i = 0; i < TeamFormation.Count; i++)
            {
                var tempList = TeamTemporarystorage[i];
                var serverDeck = TeamFormation[i];

                if (_index == i)
                    tempList = battleLine;

                checkList.Add(ArrayEqual(tempList, serverDeck));
            }
            return checkList;
        }

        bool ArrayEqual(List<int> list1, List<int> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            for(int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                    return false;
            }

            return true;
        }

        public void SetTemporaryFormation(List<List<int>> _totalList)
        {
            if(_totalList.Count != MAX_PARTY_COUNT)
            {
                Debug.LogError("teamTemporary count is different 4 set value : " + _totalList.Count);
                return;
            }

            TeamTemporarystorage = _totalList.ToList();
        }
        public List<int> GetTemporaryFormation(int tag)
        {
            if (0 <= tag && TeamTemporarystorage.Count > tag)
                return TeamTemporarystorage[tag];

            return null;
        }
        public void SetTemporaryFormation(int _index, List<int> _List)
        {
            if (_index >= MAX_PARTY_COUNT || _index < 0)
            {
                Debug.LogError("teamTemporary count index not include boundary : " + _index);
                return;
            }

            TeamTemporarystorage[_index] = _List.ToList();
        }
        public bool RemoveTemporarySpecificDragon(int _DeckIndex, int _dragonTag)
        {
            if (_dragonTag <= 0)
                return false;

            if (TeamTemporarystorage.Count <= _DeckIndex || TeamTemporarystorage.Count < 0)
                return false;

            var list = TeamTemporarystorage[_DeckIndex];
            var check = list.FindIndex(element => element == _dragonTag);
            if (check >= 0)
            {
                list[check] = 0;
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// param == -1 -> 전체 복사
        /// </summary>
        /// <param name="_specificIndex"></param>
        public void SaveToTeamFormation(int _specificIndex = -1)
        {
            if(TeamTemporarystorage.Count != TeamFormation.Count)
            {
                Debug.LogError("team save count is different");
                return;
            }

            if(TeamFormation.Count <= _specificIndex)
            {
                Debug.LogError("team save count bigger than TeamFormation count");
                return;
            }

            if(_specificIndex < 0)//전체 복사
            {
                TeamFormation = TeamTemporarystorage.ToList();
            }
            else
            {
                TeamFormation[_specificIndex] = TeamTemporarystorage[_specificIndex].ToList();
            }
        }

        public int GetDragonCountInTemporaryDeck(int _index)
        {
            if (TeamTemporarystorage.Count <= _index || _index < 0)
                return -1;

            var deck = TeamTemporarystorage[_index];
            if (deck == null || deck.Count <= 0)
                return -1;

            int checkCount = 0;
            for(int i = 0; i < deck.Count; i++)
            {
                var dragonID = deck[i];
                if (dragonID > 0)
                    checkCount++;
            }

            return checkCount;
        }
        //전체 덱을 검사해서 빈덱이 하나라도 있는지 체크
        public bool IsTotalDeckEmptyCheck(int _index = -1 , bool _include = true)
        {
            for(int i = 0; i< TeamTemporarystorage.Count; i++)
            {
                if (_index >= 0 && _include == false && i == _index)
                    continue;

                var dragonCount = GetDragonCountInTemporaryDeck(i);
                if (dragonCount <= 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 현재 클라와 서버에서 준 데이터 값 비교한 리스트 가지고 빈덱이 아닌 덱 만 골라서 저장 요청
        /// </summary>
        /// <param name="_diffList"></param>
        /// <returns></returns>
        public JObject GetWorldBossTemporaryFormationSpecificDeck(List<bool> _diffList)
        {
            JObject totalParam = new JObject();

            for (int i = 0; i < _diffList.Count; i++)
            {
                var isDiff = _diffList[i];
                if (isDiff)
                    continue;

                var dragonIDList = GetTemporaryFormation(i);
                var dragonCount = GetDragonCountInTemporaryDeck(i);
                if (dragonCount <= 0)
                    continue;

                JArray innerParam = new JArray();
                innerParam.Add(new JArray(dragonIDList));
                innerParam.Add(new JArray(new int[] { 0, 0, 0 }));

                totalParam.Add(i.ToString(), innerParam);
            }

            return totalParam;
        }
        #endregion
    }
}

