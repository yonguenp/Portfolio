using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class RecipeBaseTable : TableBase<RecipeBaseData, DBRecipe_core>
    {
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
    }

    public class RecipeMaterialTable : TableBase<RecipeMaterialData, DBRecipe_material>
    {
        Dictionary<int, List<RecipeMaterialData>> dicGroup = new Dictionary<int, List<RecipeMaterialData>>();
        public override void Init()
        {
            base.Init();
            dicGroup.Clear();
        }
        public override void DataClear()
        {
            base.DataClear();
            dicGroup.Clear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(RecipeMaterialData data)
        {
            if (base.Add(data))
            {
                var key = data.RECIPE_ID;
                if (!dicGroup.ContainsKey(key))
                    dicGroup[key] = new List<RecipeMaterialData>();

                dicGroup[key].Add(data);

                return true;
            }

            return false;
        }

        public List<RecipeMaterialData> GetDataByGroup(int _group)
        {
            if (!dicGroup.ContainsKey(_group))
                return new List<RecipeMaterialData>();

            return dicGroup[_group].ToList();
        }
    }
}