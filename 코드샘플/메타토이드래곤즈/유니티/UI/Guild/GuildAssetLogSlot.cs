using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 

    public class GuildAssetLogSlot : MonoBehaviour
    {
        [SerializeField]
        Text Nick;
        [SerializeField]
        GameObject Magnite;
        [SerializeField]
        GameObject Magnet;
        [SerializeField]
        GameObject Deposit;
        [SerializeField]
        GameObject Withdraw;
        [SerializeField]
        Text Value;
        [SerializeField]
        Text Time;
        [SerializeField]
        Text Sender;

        public class GuildAssetLog : ITableData
        {            
            public int index { get; private set; } = 0;
            public long user_no { get; private set; }
            public string nick { get; private set; }
            public GuildAssetType type { get; private set; }
            public long value { get; private set; }
            public string time { get; private set; }
            public string sender { get; private set; }

            public GuildAssetLog(int idx, JObject data)
            {
                index = idx;
                if (data.ContainsKey("to_user_no"))
                {
                    user_no = data["to_user_no"].Value<long>();
                }

                if (data.ContainsKey("to_user_nick"))
                {
                    nick = data["to_user_nick"].Value<string>();
                }
                if (string.IsNullOrEmpty(nick))
                    nick = "-";

                if (data.ContainsKey("asset_code") && data.ContainsKey("change_amount"))
                {
                    eGoodType code = (eGoodType)data["asset_code"].Value<int>();
                    int order = data["order_type"].Value<int>();

                    switch (code)
                    {
                        case eGoodType.MAGNET:
                            if(order == 1)
                            {
                                type = GuildAssetType.MAGNET_DEPOSIT;
                            }
                            else
                            {
                                type = GuildAssetType.MAGNET_WITHDRAW;
                            }
                            break;
                        case eGoodType.MAGNITE:
                            if (order == 1)
                            {
                                type = GuildAssetType.MAGNITE_DEPOSIT;
                            }
                            else
                            {
                                type = GuildAssetType.MAGNITE_WITHDRAW;
                            }
                            break;
                    }
                }
                if (data.ContainsKey("change_amount"))
                {
                    value = data["change_amount"].Value<long>();
                }
                if (data.ContainsKey("regist_at"))
                {
                    time = SBFunc.TimeStampToDateTime(data["regist_at"].Value<long>()).ToShortDateString() + "\n" + SBFunc.TimeStampToDateTime(data["regist_at"].Value<long>()).ToShortTimeString();
                }
                if (data.ContainsKey("nick"))
                {
                    sender = data["nick"].Value<string>();
                }
            }
            public string GetKey()
            {
                return index.ToString();
            }

            public void Init()
            {
                
            }
        }
        public void Init(GuildAssetLog data)
        {
            Nick.text = data.nick;
            switch(data.type)
            {
                case GuildAssetType.MAGNET_DEPOSIT:
                    Magnet.SetActive(true);
                    Magnite.SetActive(false);
                    Deposit.SetActive(true);
                    Withdraw.SetActive(false);
                    break;
                case GuildAssetType.MAGNET_WITHDRAW:
                    Magnet.SetActive(true);
                    Magnite.SetActive(false);
                    Deposit.SetActive(false);
                    Withdraw.SetActive(true);
                    break;
                case GuildAssetType.MAGNITE_DEPOSIT:
                    Magnet.SetActive(false);
                    Magnite.SetActive(true);
                    Deposit.SetActive(true);
                    Withdraw.SetActive(false);
                    break;
                case GuildAssetType.MAGNITE_WITHDRAW:
                    Magnet.SetActive(false);
                    Magnite.SetActive(true);
                    Deposit.SetActive(false);
                    Withdraw.SetActive(true);
                    break;
            }

            Value.text = SBFunc.CommaFromNumber(data.value);
            Time.text = data.time;
            Sender.text = data.sender;
        }
    }
}