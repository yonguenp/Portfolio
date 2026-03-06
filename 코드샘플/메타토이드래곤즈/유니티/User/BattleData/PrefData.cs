using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{

    //유저 preference Data (각 컨텐츠별 팀 배치 + 각 팀의 팀장비 태그, 현재 스테이지 진행 내역, 최근 스테이지 등등)
    public class PrefData
    {
        public AdventureFormationData AdventureFormationData { get; private set; } = new AdventureFormationData();
        public ArenaFormationData ArenaFormationData { get; private set; } = new ArenaFormationData();
        public DailyDungeonFormationData DailyDungeonFormationData { get; private set; } = new DailyDungeonFormationData();
        public WorldBossFormationData WorldBossFormationData { get; private set; } = new WorldBossFormationData();

        public AdventureBattleLine AdventureBattleLine { get; private set; } = new AdventureBattleLine();
        public DailyDungeonBattleLine DailyBattleLine { get; private set; } = new DailyDungeonBattleLine();

        public int DragonURCeilingCount { get; private set; } = 0;//드래곤 조합 성공(UR등급) 카운트 -> 천장 치면 다음 회차에 무조건 획득
        public int DragonSRCeilingCount { get; private set; } = 0;//드래곤 조합 성공(UR등급) 카운트 -> 천장 치면 다음 회차에 무조건 획득

        public string PetLock { get; private set; } = "";
        public string PartLock { get; private set; } = "";

        public void Set(JObject jsonData)//jsonData: any
        {
            if (jsonData.ContainsKey("adventure"))
            {
                SetAdventureFormation((JObject)jsonData["adventure"]);
            }

            if (jsonData.ContainsKey("arena"))
            {
                SetArenaFormation((JObject)jsonData["arena"]);
            }

            if (jsonData.ContainsKey("daily"))
            {
                SetDailyDungeonFormation((JObject)jsonData["daily"]);
            }

            if (jsonData.ContainsKey("raid") && SBFunc.IsJTokenType(jsonData["raid"], JTokenType.Array))
            {
                SetWorldBossFormation((JArray)jsonData["raid"]);
            }

            if (jsonData.ContainsKey("m_d_ur"))
                DragonURCeilingCount = jsonData["m_d_ur"].Value<int>();
            if (jsonData.ContainsKey("m_d_sr"))
                DragonSRCeilingCount = jsonData["m_d_sr"].Value<int>();

            if (jsonData.ContainsKey("pet_lock"))
                SetPetLock(jsonData["pet_lock"].ToString());

            if (jsonData.ContainsKey("part_lock"))
                SetPartLock(jsonData["part_lock"].ToString());
        }

        public void SetAdventureFormation(JObject jsonData)//탐험 데이터 세팅 각 팀별 포메이션 및 팀 장비 세팅//jsonData: any
        {
            AdventureFormationData.AllClearFormationData();
            if (jsonData.ContainsKey("team_1"))
            {
                AdventureFormationData.SetData(jsonData["team_1"]);
            }
            if (jsonData.ContainsKey("team_2"))
            {
                AdventureFormationData.SetData(jsonData["team_2"]);
            }
            if (jsonData.ContainsKey("team_3"))
            {
                AdventureFormationData.SetData(jsonData["team_3"]);
            }
            
            AdventureFormationData.SetDeckSize();
        }


        public void SetArenaFormation(JObject jsonData)//Arena 데이터 세팅 각 팀별 포메이션 및 팀 장비 세팅//jsonData: any
        {
            ArenaFormationData.AllClearATKFormationData();
            if (jsonData.ContainsKey("team_1"))
            {
                ArenaFormationData.SetAtkJsonData(jsonData["team_1"]);
            }
            if (jsonData.ContainsKey("team_2"))
            {
                ArenaFormationData.SetAtkJsonData(jsonData["team_2"]);
            }
            if (jsonData.ContainsKey("team_3"))
            {
                ArenaFormationData.SetAtkJsonData(jsonData["team_3"]);
            }
            if (jsonData.ContainsKey("team_4"))
            {
                ArenaFormationData.SetAtkJsonData(jsonData["team_4"]);
            }
            if (jsonData.ContainsKey("team_5"))
            {
                ArenaFormationData.SetAtkJsonData(jsonData["team_5"]);
            }
            ArenaFormationData.SetAtkDeckSize();
            if (jsonData.ContainsKey("def"))
            {
                ArenaFormationData.SetDefJsonData(jsonData["def"]);
            }
        }
        
        public void SetDailyDungeonFormation(JObject jsonData)
        {
            if (jsonData == null)
            {
                return;
            }

            DailyDungeonFormationData.SetData(jsonData);
        }

        public void SetWorldBossFormation(JArray jsonData)
        {
            if (jsonData == null)
            {
                return;
            }

            WorldBossFormationData.SetData(jsonData);
        }

        public void SetPartLock(string data)
        {
            PartLock = data;
        }

        public void SetPetLock(string data)
        {
            PetLock = data;
        }

        public void SetDragonCompoundCeilingCount(string key , int _value)
        {
            switch (key)
            {
                case "ur_d_merge_cnt":
                    DragonURCeilingCount = _value;
                    break;
                case "sr_d_merge_cnt":
                    DragonSRCeilingCount = _value;
                    break;
            }
        }

        public void Clear()
        {
            AdventureFormationData = new();
            ArenaFormationData = new();
            DailyDungeonFormationData = new();
            WorldBossFormationData = new();

            DragonURCeilingCount = 0;
            DragonSRCeilingCount = 0;
            PartLock = "";
            PetLock = "";
        }

        public List<int> GetAdventureFormation(int index = 0)
        {
            if (AdventureFormationData == null)
                return null;

            return AdventureFormationData?.GetFormation(index);
        }
        public List<int> GetDailyFormation(int dailyWorld)
        {
            return DailyDungeonFormationData?.GetFormaiotnData(dailyWorld);
            //return DailyDungeonFormationData?.GetFormaiotnData(SBFunc.GetDailyWorldIndexConvertIndex(dailyWorld));
        }
        public List<int> GetDailyFormation(eDailyType _type)
        {
            List<int> ret = new();
            if (_type != eDailyType.None)
            {
                // 여기에서 요일별 던전 체크로 변경해야 함

                var datas = DailyStageData.GetByDay(_type);
                foreach ( var data in datas)
                {
                    int currentTeamPresetNo = CacheUserData.GetInt("presetDailyDeck" + data.WORLD_NUM, 0);
                    var list = DailyDungeonFormationData?.GetFormaiotnData(currentTeamPresetNo);
                    foreach(var dat in list)
                    {
                        if(ret.Contains(dat)==false)
                            ret.Add(dat);
                    }
                }
            }
            return ret;
        }

        public List<int> GetArenaFomation(bool _isAtk = true, int index = -1)
        {
            if (ArenaFormationData == null)
                return null;
            int presetIndex = 0;
            if (index == -1)
            {
                presetIndex = CacheUserData.GetInt("presetArenaAtkDeck", 0);
            }
            else
            {
                presetIndex = index;
            }
            return _isAtk ? ArenaFormationData.GetArenaFormationATK(presetIndex) : ArenaFormationData.GetArenaFormationDEF(0);
        }
        
        public List<int> GetWorldBossFormation(int _deckIndex)
        {
            if (WorldBossFormationData == null)
                return null;

            return WorldBossFormationData.GetFormationData(_deckIndex);
        }
        /// <summary>
        /// 팀배치 전체(공,방)의 리스트
        /// </summary>
        /// <returns></returns>
        public List<int> GetSerializeTotalWorldBossFormation()
        {
            var count = WorldBossFormationData.TeamFormation.Count;

            List<int> tempList = new List<int>();
            var checkList = WorldBossFormationData.TeamFormation;
            if (checkList == null || checkList.Count <= 0)
                return tempList;

            foreach(var list in WorldBossFormationData.TeamFormation)
            {
                if (list != null && list.Count > 0)
                    tempList.AddRange(list);
            }

            tempList.RemoveAll(element => (element == 0));

            return tempList;
        }
    }
}