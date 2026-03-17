using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// MagicShowcasePopup의 하위 탭을 구성하는 기본 데이터 객체
/// 레벨, 레벨업을 위한 각 재료의 투입 상태 정보 저장, 
/// 테이블 데이터까지 연결해둘 필요는 없을 것같음 - MagicShowcaseManager 에서 가져오는 걸로.
/// </summary>
namespace SandboxNetwork
{
    public class MagicShowcaseInfoData
    {
        const int STOCK_MAX_COUNT = 3;
        public int LEVEL { get; private set; } = -1;//현재 진열장 레벨
        public eShowcaseGroupType TYPE { get; private set; } = eShowcaseGroupType.NONE;//탭속성 타입

        public Asset[] STOCK { get; private set; } = null;

        public MagicShowcaseData TABLE_DATA//현재 레벨에서 업그레이드 하기위한 테이블 데이터
        {
            get
            {
                return MagicShowcaseData.GetDataByGroupAndLevel(TYPE, LEVEL + 1);
            }
        }

        public bool IsUpgradeCondition//업그레이드가 가능한 상태인가 - 현재 요구재료랑 투입량이랑 같으면
        {
            get
            {
                if (TABLE_DATA == null)
                    return false;

                int checkCount = 0;
                var upgradeList = TABLE_DATA.MATERIAL_LIST;
                foreach(var itemData in upgradeList)
                {
                    if (itemData == null)
                        continue;

                    var itemNo = itemData.ItemNo;
                    var itemCount = itemData.Amount;

                    var stockItem = GetInputAmount(itemNo);

                    if (stockItem == itemCount)
                        checkCount++;
                }

                return checkCount == upgradeList.Count;
            }
        }

        public MagicShowcaseInfoData(eShowcaseGroupType _type, int _level, List<Asset> _list)
        {
            TYPE = _type;
            LEVEL = _level;

            if (_list.Count == STOCK_MAX_COUNT)
                STOCK = _list.ToArray();
        }

        public List<Asset> GetStockList()
        {
            if (STOCK == null || STOCK.Length <= 0)
                return new List<Asset>() { };

            return STOCK.ToList();
        }

        public void UpdateInfoData(int _level , List<Asset> _list)
        {
            LEVEL = _level;
            STOCK = _list.ToArray();
        }

        public int GetInputAmount(int _itemNo)
        {
            if (STOCK == null || STOCK.Length != STOCK_MAX_COUNT)
                return 0;

            var isData = GetStockList().Find(element => element.ItemNo == _itemNo);
            if (isData != null)
                return isData.Amount;
            return 0;
        }
    }
}

