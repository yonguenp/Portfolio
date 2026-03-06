using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SandboxNetwork
{
    public class MagicShowcaseTable : TableBase<MagicShowcaseData, DBMagic_showcase_info>
    {
        Dictionary<int, List<MagicShowcaseData>> dicGroup = new Dictionary<int, List<MagicShowcaseData>>();
        public override void Init()
        {
            base.Init();
        }
        public override void DataClear()
        {
            dicGroup.Clear();
            base.DataClear();
        }
        public override void Preload()
        {
            Init();
            base.Preload();
            LoadAll();
        }
        protected override bool Add(MagicShowcaseData data)
        {
            if (base.Add(data))
            {
                var key = (int)data.GROUP;
                if (!dicGroup.ContainsKey(key))
                    dicGroup[key] = new List<MagicShowcaseData>();

                dicGroup[key].Add(data);
                return true;
            }
            return false;
        }
        public List<MagicShowcaseData> GetDataByGroup(int _group)
        {
            if (!dicGroup.ContainsKey(_group))
                return new List<MagicShowcaseData>();
            
            return dicGroup[_group].ToList();
        }

        public MagicShowcaseData GetDataByGroupAndLevel(int _group , int _level)
        {
            var list = GetDataByGroup(_group);
            if (list == null || list.Count <= 0)
                return null;

            return list.Find(element => element.LEVEL == _level);
        }
        public int GetMaxLevelByGroup(int _group)
        {
            var list = GetDataByGroup(_group);
            if (list == null || list.Count <= 0)
                return -1;

            return list.Select(x => x.LEVEL).Max();
        }
        /// <summary>
        /// 타입과 레벨을 기준으로 1렙부터 누적된 리스트 가져오기
        /// </summary>
        /// <param name="_group"></param>
        /// <param name="_level"></param>
        /// <param name="_isInclude"></param>//_level param을 포함시킬건지? 
        /// <returns></returns>
        public List<MagicShowcaseData> GetAccumulateDataByLevel(int _group, int _level, bool _isInclude = true)
        {
            var list = GetDataByGroup(_group);
            if (list == null || list.Count <= 0)
                return new List<MagicShowcaseData>();

            if (_isInclude)
                return list.FindAll(element => element.LEVEL <= _level).ToList();
            else
                return list.FindAll(element => element.LEVEL < _level).ToList();
        }
        /// <summary>
        /// _level 기준으로 위의 데이터를 가져옴
        /// </summary>
        /// <param name="_group"></param>
        /// <param name="_level"></param>
        /// <param name="_isInclude"></param>
        /// <returns></returns>
        public List<MagicShowcaseData> GetNextTotalDataByLevel(int _group, int _level, bool _isInclude = false)
        {
            var list = GetDataByGroup(_group);
            if (list == null || list.Count <= 0)
                return new List<MagicShowcaseData>();

            if (_isInclude)
                return list.FindAll(element => element.LEVEL >= _level).ToList();
            else
                return list.FindAll(element => element.LEVEL > _level).ToList();
        }
    }
}