using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionSurpportCheckPopup : Popup<PopupBase>
    {
        [SerializeField] Text Title = null;
        [SerializeField] Text Value = null;


        ChampionSurpportInfo.eSurpportType curType;
        int value;
        public static void Open(ChampionSurpportInfo.eSurpportType type, int value)
        {
            var popup = PopupManager.OpenPopup<ChampionSurpportCheckPopup>();
            popup.SetData(type, value);
        }

        public void SetData(ChampionSurpportInfo.eSurpportType type, int v)
        {
            curType = type;
            value = v;
            Title.text = StringData.GetStringByStrKey("마그넷입금");
            Value.text = SBFunc.CommaFromNumber(value);
        }

        public override void InitUI()
        {

        }

        public void OK()
        {
            var param = new WWWForm();
            switch(curType)
            {
                case ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS:
                    param.AddField("type", 1);
                    break;
                case ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS:
                    param.AddField("type", 2);
                    break;
                case ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS:
                    param.AddField("type", 3);
                    break;
            }
            
            param.AddField("magnite", value);

            NetworkManager.Send("unifiedtournament/support", param, (data) => {
                PopupManager.ClosePopup<ChampionSurpportCheckPopup>();
                PopupManager.ClosePopup<ChampionSurpportServerPopup>();

                if (data.ContainsKey("support"))
                    ChampionManager.Instance.CurChampionInfo.SurpportInfo.SetData((JObject)data["support"]);
            });
        }
    }
}