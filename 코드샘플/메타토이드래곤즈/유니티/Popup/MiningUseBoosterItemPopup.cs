using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class MiningUseBoosterItemPopup : Popup<MineBoostItemUsePopupData>
    {
        [SerializeField] MineBoostInfoSlot itemInfo = null;
        [SerializeField] Text descText = null;
        [SerializeField] Text warningText = null;

        bool isNetwork = false;
        public override void InitUI()
        {
            isNetwork = false;

            var targetItemInfo = Data.boostItem;
            if (targetItemInfo == null || targetItemInfo.Amount <= 0)
                return;//여기 타면 안되긴함.

            SetTextByMiningState();

            if (itemInfo != null)
                itemInfo.InitUI(targetItemInfo, () => {
                    isNetwork = false;
                    ClosePopup();
                    ToastManager.On(StringData.GetStringByStrKey("광산토스트11"));
                    MiningPopupEvent.RequestUseBoosterItem(itemInfo.BoosterItem, null);
                });
        }

        void SetTextByMiningState()
        {
            var isMining = MiningManager.Instance.UserMiningData.IsMiningState();

            if(descText != null)
                descText.text = isMining ? StringData.GetStringByStrKey("부스터사용팝업내용1") : StringData.GetStringByStrKey("부스터사용팝업내용3");
            if (warningText != null)
                warningText.text = isMining ? StringData.GetStringByStrKey("부스터사용팝업내용2") : StringData.GetStringByStrKey("부스터사용팝업내용4");
        }


        public void OnClickUseItem()
        {
            if (itemInfo == null)
                return;

            var targetItem = itemInfo.BoosterItem;
            if (targetItem == null)
                return;

            var baseData = targetItem.BaseData;
            if (baseData == null)
                return;

            var boosterData = targetItem.BoostTableData;
            if (boosterData == null)
                return;

            var isLimitTime = boosterData.IS_LIMIT_TIME_TYPE;
            if(isLimitTime)
            {
                var expireTime = targetItem.ExpireTime;
                var timeInterval = TimeManager.GetTimeCompare(expireTime);

                if(timeInterval < 0)
                {
                    ToastManager.On(StringData.GetStringByStrKey("광산토스트10"));
                    return;
                }
            }

            if (isNetwork)
                return;

            isNetwork = true;
            WWWForm data = new WWWForm();
            data.AddField("item_no", targetItem.BaseData.KEY);
            NetworkManager.Send("mine/changebooster", data, (jsonData) =>
            {
                isNetwork = false;
                JToken resultResponse = jsonData["rs"];
                if (resultResponse != null && resultResponse.Type == JTokenType.Integer)
                {
                    int rs = resultResponse.Value<int>();
                    switch ((eApiResCode)rs)
                    {
                        case eApiResCode.OK:
                            //아이템이 push로 오는지, rs로 오는지 확인할 것.

                            jsonData.Add("item_no", targetItem.BaseData.KEY);

                            MiningPopupEvent.RequestUseBoosterItem(null, jsonData);

                            ClosePopup();
                            break;
                    }
                }
            },(failString)=> {
                isNetwork = false;
                ToastManager.On(StringData.GetStringByStrKey("서버요청실패"));
            });
        }
    }
}

