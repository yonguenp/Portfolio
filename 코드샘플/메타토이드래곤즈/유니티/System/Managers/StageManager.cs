using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class StageManager
    {
        private static StageManager instance = null;

        public static StageManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StageManager();
                    instance.DailyDungeonProgressData.Init();
                }
                return instance;
            }
        }


        private AccumPlayData accumPlayData = new AccumPlayData();

        public int World { get; private set; } = 1;
        public int Stage { get; private set; } = 1;
        //난이도라고 합니다. 1 -> 노말 2 -> 하드
        public int Diff { get; private set; } = 1;

        //준형 :: 데일리 던전에 관련된 정보가 없어 임시 초기값으로 넣었습니다. 
        public int Daily_World { get; private set; } = 1;
        public int Daily_Stage { get; private set; } = 1;
        public int Daily_Diff { get; private set; } = 1;

        public int LastEnterWorld { get; private set; } = 0;

        public AdventureProgressData AdventureProgressData { get; private set; } = new AdventureProgressData();
        public DailyDungeonProgressData DailyDungeonProgressData { get; private set; } = new DailyDungeonProgressData();

        public void SetWorld(int world) { World = world; }
        public void SetStage(int stage) { Stage = stage; }
        public void SetDiff(int diff) { Diff = diff; }

        public void SetAll(int world, int stage, int diff)
        {
            World = world;
            Stage = stage;
            Diff = diff;
        }
        public void SetDailyAll(int dWorld, int dStage, int dDiff)
        {
            Daily_World = dWorld;
            Daily_Stage = dStage;
            Daily_Diff = dDiff;
        }

        public void SetLastEnterWorld(int world)
        {
            LastEnterWorld = world;
        }
        // 퀘스트 바로가기 스테이지 진입 시 사용
        public int Quest_World { get; set; } = 0;
        public int Quest_Stage { get; set; } = 0;
        public int Quest_Diff { get; set; } = 0;
         

        static public Dictionary<int, Dictionary<int, Asset>> AccumRewards { get { return Instance.accumPlayData.Rewards; } }
        static public int AccumCount { get { return Instance.accumPlayData.Count; } }
        static public int AccumTotalCount { get { return Instance.accumPlayData.TotalCount; } }

        static public void ClearAccumData()
        {
            Instance.accumPlayData.SetAccumData(-1, -1);
        }
        static public void InitAccumData(int count, int total)
        {
            Instance.accumPlayData.SetAccumData(count, total);
        }
        static public void StageCompleteAccum()
        {
            Instance.accumPlayData.StageCompleteAccum();
        }
        static public void StageStartAccum()
        {
            Instance.accumPlayData.StageStartAccum();
        }

        public void SetAdventureWorldProgress(JObject jsonData)//월드 진행 상태 저장//jsonData: any
        {
            AdventureProgressData.SetData(jsonData);
        }
        public void SetDailyDungeonProgress(JArray jsonData)//요일던전 진행 상태
        {
            if (jsonData == null || jsonData.Count <= 0)
            {
                return;
            }

            DailyDungeonProgressData.SetData(jsonData);
        }

    }
}
