using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SandboxNetwork
{
    /// <summary>
    /// 광산 부스터 전용 아이템에 대한 정의
    /// </summary>
    public class MineBoosterData : TableData<DBMine_booster>
    {
        static private MineBoosterTable table = null;
        static public MineBoosterData Get(string key)
        {
            if (table == null)
                table = TableManager.GetTable<MineBoosterTable>();

            return table.Get(key);
        }

        static public MineBoosterData Get(int key)
        {
            return Get(key.ToString());
        }

        /// <summary>
        /// item 기본 테이블 데이터 가져오기  - itemBase쪽에다가 넣기엔 '굳이'의 영역이라 서버쪽에서 itemList를 주는 형태가 되지 않을까 싶음.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ItemBaseData GetItemDesignData()
        {
            var itemTable = TableManager.GetTable<ItemBaseTable>();
            if (itemTable == null)
                return null;

            return itemTable.Get(KEY);
        }

        public int KEY { get { return Int(UNIQUE_KEY); } }
        public int GROUP => Data.GROUP;//아이템 분류를 위한 그룹
        public int VALUE_TYPE => Data.VALUE_TYPE;//부스터 그룹(1=증폭 부스터, 2=추가 부스터)
        public float VALUE => Data.VALUE;//그룹값이 1 일때 10000 기준으로 % 연산 (ex. 10000 -> 100%)
        public bool IS_LIMIT_TIME_TYPE { get; private set; } = false;//부스터 아이템 유지 시간 체크(안쪽에서 1이면 영구유지, 0이면 제한 시간 적용)
        public int BOOST_TIME => Data.BOOST_TIME;//(초) 부스트 유지 시간
        public DateTime EXPIRE_AT { get; private set; } = DateTime.MinValue;//아이템 소멸 시간 관리 (서버에서 따로 할당 해줄듯.)


        //채굴량 붙여야되는지, 뒤에 '증폭', '증가' 붙어야되는지 확인해볼 것.
        public string VALUE_DESC
        {
            get
            {
                return string.Format("+{0}{1}", VALUE, IS_PERCENT ? "%" : "");
            }
        }

        public string GetValueDesc(bool _addPlusStr = true)
        {
            string value_str = IS_PERCENT ? MathF.Round(VALUE / 100f, 2).ToString(GamePreference.Instance.Culture) : VALUE.ToString();
            return string.Format(_addPlusStr ? "+{0}{1}" : "{0}{1}", value_str, VALUE_TYPE == 1 ? "%" : "");
        }

        public bool IS_PERCENT { get { return VALUE_TYPE == 1; } }

        public override void SetData(DBMine_booster _data)
        {
            base.SetData(_data);

            IS_LIMIT_TIME_TYPE = Data.TYPE != 1;//1이면 무제한, 0이면 제한 시간 있음
            if (!IS_LIMIT_TIME_TYPE && !string.IsNullOrEmpty(Data.EXPIRE_AT))
                if (DateTime.TryParse(Data.EXPIRE_AT, GamePreference.Instance.Culture, System.Globalization.DateTimeStyles.None, out DateTime _result))
                    EXPIRE_AT = _result;
        }
    }
}
