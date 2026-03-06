using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class CollectionTable : TableBase<CollectionData, DBCollection_info>
    {
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();

            Dictionary<string, List<CollectionData>> dic = new Dictionary<string, List<CollectionData>>();
            foreach(var data in datas.Values)
            {
                if (!dic.ContainsKey(data.collection_name))
                    dic.Add(data.collection_name, new List<CollectionData>());
                
                dic[data.collection_name].Add(data);
            }

            foreach(var list in dic.Values)
            {
                if (list.Count > 1)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].SetNameTag(i);
                    }
                }
            }
        }

        public int GetCollectionTotalCount()
        {
            return datas.Values.Count;
        }
    }

    public class CollectionGroupTable : TableBase<CollectionGroupData, DBCollection_group>
    {
        Dictionary<int, List<int>> groupDic = null;//key : groupID , value : dragonID List
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

            SortGroup();
        }
        protected override bool Add(CollectionGroupData data)
        {
            if (base.Add(data))
            {
                if (!groupDic.ContainsKey(data.GROUP_ID))
                    groupDic.Add(data.GROUP_ID, new());

                groupDic[data.GROUP_ID].Add(data.DRAGON_KEY);
                return true;
            }
            return false;
        }
        public List<int> GetIDListByGroup(int group)
        {
            if (groupDic == null || !groupDic.ContainsKey(group))
                return new();

            return groupDic[group].ToList();
        }

        void SortGroup()
        {
            foreach (var key in groupDic.Keys.ToList())
            {
                groupDic[key] = groupDic[key].OrderByDescending(x =>
                                                                    {
                                                                        if (x >= 15000) return 5;
                                                                        else if (x >= 14000) return 4;
                                                                        else if (x >= 13000) return 3;
                                                                        else if (x >= 12000) return 2;
                                                                        else return 1;
                                                                    })
                                                                    //.ThenBy(x => x) 의도적인 컬렉션 리더가 퇴색됨
                                                                    .ToList();
            }
        }
    }
}
