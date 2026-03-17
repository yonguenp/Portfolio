using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class WorldCursor
    {
        public WorldCursor(int targetWorld, int targetStage, int targetDiff = 1)
        {
            SetData(targetWorld, targetStage, targetDiff);
        }
        public int World { get; private set; } = -1;
        public int Diff { get; private set; } = -1;
        public int Stage { get; private set; } = -1;

        public void SetData(int targetWorld, int targetStage, int targetDiff = 1)
        {
            World = targetWorld;
            Diff = targetDiff;
            Stage = targetStage;
        }
    }
    public class WorldProgress
    {
        public int World { get; private set; } = -1;
        public int Diff { get; private set; } = -1;
        public List<int> Stages { get; private set; } = null;
        public List<int> Rewarded { get; private set; } = null;
        public WorldProgress(JObject data)
        {
            SetData(data);
        }
        public void SetRewarded(List<int> data)
        {
            Rewarded = data;
        }

        public void SetData(JObject data)
        {
            if (data == null)
                return;

            World = SBFunc.IsJTokenCheck(data["world"]) ? data["world"].Value<int>() : -1;
            Diff = SBFunc.IsJTokenCheck(data["diff"]) ? data["diff"].Value<int>() : 1;
            if (SBFunc.IsJArray(data["stages"]))
            {
                var array = (JArray)data["stages"];
                Stages = array.ToObject<List<int>>();
            }

            if(SBFunc.IsJArray(data["rewarded"]))
            {
                var array = (JArray)data["rewarded"];
                SetRewarded(array.ToObject<List<int>>());
            }
        }

        public bool IsClearStage(int stage)
        {
            if (Stages == null)
                return false;

            if (Stages.Count < stage)
                return false;

            return Stages[stage] > 0;
        }

        public int GetAllStar()
        {
            if (Stages == null)
                return 0;

            int star = 0;
            for(int i = 0, count = Stages.Count; i < count; ++i)
                star += Stages[i];

            return star;
        }

        public int GetLastestClearStage()
        {
            if (Stages == null)
                return 0;
            if(Stages.Count == 0)
                return 0;
            for(int i =0, count = Stages.Count;i < count; ++i)
            {
                if (Stages[i] == 0)
                    return i;
            }
            return Stages.Count;
        }

        public bool IsWorldClear()
        {
            return IsClearStage(Stages.Count - 1);
        }
    }
    public class AdventureProgressData
    {
        //월드 입장 가능 리스트 - stageInfo들고있음. 
        /**
         * diff : stage 난이도
         * rewarded[3] : 월드 획득 별갯수 0 미해금 1 해금 2 획득완료
         * stages[8] : 스테이지 진행 별 갯수 0은 미해금 1~3 별갯수
         * world : 월드 인덱스
         */
        public Dictionary<int, WorldProgress> WorldList { get; private set; } = new Dictionary<int, WorldProgress>();

        //커서 (array [world, stage, mode] - 0번 인덱스 부터 순서대로 - 마지막 클리어 월드 , 스테이지, 난이도)
        public WorldCursor WorldCuror { get; private set; } = null;
        
        public void SetData(JObject jsonData)
        {
            if (jsonData == null)
                return;

            JToken worlds = jsonData["worlds"];
            //worlds 데이터(key - world string , value - 1)와 , cursor 데이터(array 형태) 들어옴
            if (SBFunc.IsJTokenType(worlds, JTokenType.Array))
                SetWorldData((JArray)worlds);

            JToken cursor = jsonData["cursor"];
            if (SBFunc.IsJTokenType(cursor, JTokenType.Array))
                SetCursorData((JArray)cursor);
        }

        /**
         * key : 0101 // 01(월드인덱스) 01(난이도)
         */
        int MakeKey(int worldIndex, int diff)
        {
            string firstKey = worldIndex.ToString("D2");
            string secondKey = diff.ToString("D2");

            string key = firstKey + secondKey;
            return int.Parse(key);
        }
        void SetWorldData(JArray jArray)
        {
            if (jArray == null) { return; }

            var worldCount = jArray.Count;

            WorldList.Clear();
            if (worldCount > 0)
            {
                for (var i = 0; i < worldCount; i++)
                {
                    var data = jArray[i];
                    if (!SBFunc.IsJTokenCheck(data))
                        continue;

                    int key = MakeKey(data["world"].Value<int>(), data["diff"].Value<int>());

                    WorldList.Add(key, new WorldProgress((JObject)data));
                }
            }
        }

        void SetCursorData(JArray jArray)
        {
            var CursorArr = jArray.ToObject<List<int>>();
            if (CursorArr == null || CursorArr.Count < 3)
                return;

            if (WorldCuror == null)
                WorldCuror = new(CursorArr[0], CursorArr[1], CursorArr[2]);
            else
                WorldCuror.SetData(CursorArr[0], CursorArr[1], CursorArr[2]);
        }

        public void SetCursorDataIndex(int world, int stage, int diff = 1)
        {

            if (WorldCuror == null)
                WorldCuror = new(world, stage, diff);
            else
                WorldCuror.SetData(world, stage, diff);
        }

        /**
         * 가장 마지막 월드 가져오기 world 인덱스 제일 큰 것으로 비교
         */
        public int GetLatestWorld(int diff = 1)
        {
            if (WorldList == null || WorldList.Count < 1)
            {
                return 1;
            }

            int lastWorld = -1;

            foreach (var world in WorldList)
            {
                if (diff != world.Value.Diff)
                    continue;

                if (lastWorld < 0 || lastWorld < world.Value.World)
                {
                    lastWorld = world.Value.World;
                }
            }

            if (GameConfigTable.GetLastWorld() <= lastWorld)
            {
                lastWorld = GameConfigTable.GetLastWorld();
            }

            return lastWorld;
        }

        public void SetWorldInfoData(int worldNumber, int diff, JObject info)
        {
            var key = MakeKey(worldNumber, diff);
            if (WorldList.ContainsKey(key))
            {
                if (WorldList[key] == null)
                    WorldList[key] = new WorldProgress(info);
                else
                    WorldList[key].SetData(info);
            }
            else
                WorldList.Add(key, new WorldProgress(info));
        }

        /**
         * 월드 데이터 가져오기
         * @param worldNumber : 가져올 월드 인덱스 (ex 1 월드 1 = 1)
         * @param diff : 월드 난이도 (1 : 노멀 , 2 : 하드)
         * @returns 
         */
        public WorldProgress GetWorldInfoData(int worldNumber, int diff = 1)
        {
            if (WorldList == null || WorldList.Count <= 0)
            {
                return null;
            }

            var key = MakeKey(worldNumber, diff);
            if (WorldList.ContainsKey(key) == false)
            {
                return null;
            }

            if (WorldList.ContainsKey(key))
            {
                return WorldList[key];
            }
            return null;
        }

        public bool IsClearedStage(int world, int stage, int diff = 1)
        {
            if (WorldList == null || WorldList.Count <= 0)
            {
                return false;
            }

            var worldInfo = GetWorldInfoData(world, diff);
            if (worldInfo == null)
                return false;

            var stageIndex = (stage - 1) < 0 ? 0 : stage - 1;
            return worldInfo.IsClearStage(stageIndex);
        }

        public bool IsLastStageClear(int world, int stage, int diff = 1)
        {
            var worldInfo = GetWorldInfoData(world, diff);
            if (worldInfo == null)
                return false;

            var worldIndex = worldInfo.World;
            var worldDiff = worldInfo.Diff;
            var stageCount = worldInfo.Stages.Count;
            if (worldIndex == world && worldDiff == diff)
            {
                bool isLastStage = stageCount == stage;
                if (isLastStage)
                {
                    var nextWorld = GetWorldInfoData(world + 1, diff);
                    if (nextWorld == null)
                        return false;

                    if (!nextWorld.IsClearStage(0))
                    {
                        return true;
                    }
                }
                else
                {
                    int stageIndex = stage - 1 < 0 ? 0 : stage - 1;
                    if (!worldInfo.IsClearStage(stageIndex))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        List<int> GetWorldStarReward(int worldIndex, int diff = 1)
        {
            var worldInfo = GetWorldInfoData(worldIndex);
            if (worldInfo == null)
                return new List<int>();

            return worldInfo.Rewarded;
        }

        void SetWorldStarReward(int worldIndex, List<int> starArray)
        {
            var worldInfo = GetWorldInfoData(worldIndex);
            if (worldInfo == null)
                return;

            worldInfo.SetRewarded(starArray);
        }

        public List<int> GetWorldStages(int worldIndex, int diff = 1)
        {
            var worldInfo = GetWorldInfoData(worldIndex, diff);
            if (worldInfo == null)
                return new List<int>();

            return worldInfo.Stages;
        }

        Dictionary<int, WorldProgress> GetTotalWorldInfoData()
        {
            return WorldList;
        }

        /**
         * 입장 가능한 월드 인지 체크
         * @param worldIndex 월드 인덱스 (world1 은 1)
         * @returns 
         */
        public bool isAvailableWorld(int worldIndex, int diff = 1)
        {
            if (WorldList == null || WorldList.Count <= 0)
                return false;

            if (!WorldList.ContainsKey(MakeKey(worldIndex, diff)))
                return false;

            return true;
        }

        int GetTotalWorldCount()
        {
            if (WorldList == null)
                return 0;

            return WorldList.Keys.Count;
        }

        /**
         * 서버에 기록된 가장 마지막 전투 위치
         */
        int GetCurrentLastWorldCursor(int diff = 1)
        {
            return WorldCuror != null ? this.WorldCuror.World : 1;
        }
        List<int> GetUserAvailableWorldAndStage()//유저가 입장 가능한 월드 및 스테이지 찾기
        {
            List<int> worldStageList = new List<int>();
            worldStageList.Add(1);
            worldStageList.Add(1);
            var keys = new List<int>(WorldList.Keys);
            if (keys == null || keys.Count <= 0)
                return worldStageList;

            List<int> convertKeyNumbers = new List<int>();
            keys.ForEach(element => {
                convertKeyNumbers.Add(element);
            });

            convertKeyNumbers.Sort((a, b) => a.CompareTo(b));//키 오름차순 정렬 - 월드별 정렬 / 나중에 난이도 제거 해야함

            var worldIndex = -1;
            var stageIndex = -1;
            var findCheck = false;

            convertKeyNumbers.ForEach(element =>
            {
                var worldInfo = WorldList[element];
                if (worldInfo == null)
                    return;

                var stageList = worldInfo.Stages;//이 또한 배열
                var worldCheck = element / 100;

                if (stageList == null || stageList.Count <= 0)
                    return;

                if (findCheck)
                    return;

                for (var i = 0; i < stageList.Count; i++)
                {
                    var starCount = stageList[i];
                    if (int.Parse(starCount.ToString()) <= 0)
                    {
                        worldIndex = worldCheck;
                        stageIndex = (i + 1);
                        findCheck = true;
                        break;
                    }
                }
            });

            if (worldIndex < 0 || stageIndex < 0)//둘중 하나도 값을 못찾았으면(다깼단 의미), 마지막월드, 스테이지로 넘김
            {
                var lastKey = convertKeyNumbers[convertKeyNumbers.Count - 1];
                var worldInfo = WorldList[lastKey];
                if(worldInfo != null)
                {
                    var worldCheckIndex = worldInfo.World;
                    var stageLength = worldInfo.Stages.Count;

                    worldStageList[0] = int.Parse(worldCheckIndex.ToString());
                    worldStageList[1] = stageLength;
                }
            }
            else
            {
                worldStageList[0] = worldIndex;
                worldStageList[1] = stageIndex;
            }
            return worldStageList;
        }

        //현재 클리어한 월드 리스트 인덱스 가져오기
        List<int> GetAvailableWorldList(int diff = 1)
        {
            List<int> worldIndexList = new List<int>();

            if (WorldList == null)
                return worldIndexList;

            var it = WorldList.GetEnumerator();
            while (it.MoveNext())
            {
                var worldData = it.Current.Value;
                if (worldData == null)
                    continue;

                var stageCount = worldData.Stages.Count;
                if (stageCount <= 0)
                    continue;

                var worldIndex = worldData.World;
                var isLastStageClear = worldData.Stages[stageCount - 1] > 0;
                if (isLastStageClear)
                    worldIndexList.Add(worldIndex);
            }

            worldIndexList.Sort((a, b) => a.CompareTo(b));
            return worldIndexList;
        }
    }
}