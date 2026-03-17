using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestrictedLogSlot : MonoBehaviour
{
    [SerializeField] GuildBaseInfoObject GuildUI;
    [SerializeField] UserPortraitFrame Portrait;
    [SerializeField] Text Nick;

    [SerializeField] Image[] CharImage;

    [SerializeField] GameObject Income;
    [SerializeField] GameObject Battle;
    
    [SerializeField] Text Time;
    [SerializeField] Text IncomeAmount;

    JObject Data = null;
    public JToken Value(string key) { return  (Data != null && Data.ContainsKey(key)) ? Data[key] : null; }
    public int IntVal(string key) { return Value(key) != null ? Value(key).Value<int>() : 0; }
    public string StrVal(string key) { return Value(key) != null ? Value(key).ToString() : ""; }

    public void SetData(JObject data, int tax)
    {
        Data = data;

        foreach (var dimg in CharImage)
        {
            dimg.sprite = null;
        }

        GuildUI.Init(new GuildBaseData(IntVal("travel_user_guild_no"), StrVal("travel_user_guild_name"), IntVal("travel_user_guild_emblem"), IntVal("travel_user_guild_mark")));
        Portrait.SetUserPortraitFrame(StrVal("travel_user_icon"), IntVal("travel_user_level"), false, new PortraitEtcInfoData(Value("travel_user_portrait")));
        Nick.text = StrVal("travel_user_nick");

        if (IntVal("travel_type") == 3)//전투
        {
            Battle.SetActive(true);
            Income.SetActive(false);
        }
        else if (IntVal("travel_type") == 2)
        {
            Battle.SetActive(false);
            Income.SetActive(true);

            IncomeAmount.text = SBFunc.CommaFromNumber(tax);
        }
        else if (IntVal("travel_type") == 1)
        {
            Battle.SetActive(false);
            Income.SetActive(true);

            IncomeAmount.text = StringData.GetStringByStrKey("제한구역길드면세");
        }

        var deck = Value("travel_deck");
        if (deck != null && deck.Type == JTokenType.Array)
        {
            int index = 0;
            var deckarray = (JArray)deck;
            foreach (var d in deckarray)
            {
                CharBaseData cd = CharBaseData.Get(d.Value<int>());
                if (cd != null)
                {
                    CharImage[index++].sprite = cd.GetThumbnail();
                }
            }
        }

        Time.text = SBFunc.TimeStampToDateTime(IntVal("travel_at")).ToString();
    }
}
