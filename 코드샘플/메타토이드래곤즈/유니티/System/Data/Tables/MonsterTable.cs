using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class MonsterBaseTable : TableBase<MonsterBaseData, DBMonster_base>
    {
        protected Dictionary<string, MonsterBaseData> imageData;

        public override void Init()
        {
            base.Init();
            if (imageData == null)
                imageData = new Dictionary<string, MonsterBaseData>();
            else
                imageData.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            if (imageData == null)
                imageData = new Dictionary<string, MonsterBaseData>();
            else
                imageData.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(MonsterBaseData data)
        {
            if (base.Add(data))
            {
                imageData.TryAdd(data.IMAGE, data);
                return true;
            }
            return false;
        }

        public MonsterBaseData GetMonsterDataByImageName(string _skinName)
        {
            if (imageData.ContainsKey(_skinName))
            {
                return imageData[_skinName];
            }

            return null;
        }
    }

    public class MonsterSpawnTable : TableBase<MonsterSpawnData, DBMonster_spawn>
    {
        protected Dictionary<int, List<MonsterSpawnData>> spawnGroupDatas;
        public override void Init()
        {
            base.Init();
            spawnGroupDatas = new();
        }
        public override void DataClear()
        {
            base.DataClear();
			if (spawnGroupDatas == null)
				spawnGroupDatas = new();
			else
				spawnGroupDatas.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(MonsterSpawnData data)
        {
            if (base.Add(data))
            {
                if (!spawnGroupDatas.ContainsKey(data.SPAWN_GROUP))
                    spawnGroupDatas[data.SPAWN_GROUP] = new();

                spawnGroupDatas[data.SPAWN_GROUP].Add(data);
                return true;
            }
            return false;
        }
        public List<MonsterSpawnData> GetBySpawnGroup(int spawnGroup)
        {
            if (spawnGroupDatas.ContainsKey(spawnGroup))
                return spawnGroupDatas[spawnGroup];

            return null;
        }
    }
}