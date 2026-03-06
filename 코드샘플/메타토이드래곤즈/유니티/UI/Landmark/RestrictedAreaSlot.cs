using Newtonsoft.Json.Linq;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestrictedAreaSlot : MonoBehaviour
{
    [SerializeField] int index;
    public int Index { get { return index; } }
    [SerializeField] Image panel;
    [SerializeField] Text ShieldRemain;
    [SerializeField] GuildBaseInfoObject GuildUI;
    [SerializeField] Text StateText;
    [SerializeField] Image[] CharImage;
    [SerializeField] GameObject Nonconquest;

    [SerializeField] Color Selected;
    [SerializeField] Color Normal;

    [Header("[Dim]")]
    [SerializeField] GameObject DimObject;
    [SerializeField] Text DimText;
    
    JObject Data = null;
    public JToken Value(string key) { return  (Data != null && Data.ContainsKey(key)) ? Data[key] : null; }
    public int IntVal(string key) { return Value(key) != null ? Value(key).Value<int>() : 0; }
    public string StrVal(string key) { return Value(key) != null ? Value(key).ToString() : ""; }

    public int RemainShield { get; private set; } = -1;
    public int RemainTravel { get; private set; } = -1;

    public void Clear()
    {
        foreach (var dimg in CharImage)
        {
            dimg.sprite = null;
        }

        CancelInvoke("TimeRefresh");

        ShieldRemain.transform.parent.gameObject.SetActive(false);
        DimObject.SetActive(false);
        GuildUI.Init(null);

        RemainTravel = -1;
        StateText.text = "";
    }

    public void OnSelect(int idx, JObject data, StageDifficult diff)
    {
        foreach(var dimg in CharImage)
        {
            dimg.sprite = null;
        }

        RestrictedAreaData curData = RestrictedAreaData.GetByWorldDiff(index, diff);

        panel.color = index == idx ? Selected : Normal;
        Data = data;
        RemainShield = 0;
        ShieldRemain.transform.parent.gameObject.SetActive(false);

        if (IntVal("ctrl_guild_no") > 0)
        {
            GuildUI.Init(new GuildBaseData(IntVal("ctrl_guild_no"), StrVal("ctrl_guild_name"), IntVal("ctrl_emblem_no"), IntVal("ctrl_mark_no")));

            RemainShield = IntVal("dom_start_at_ts") + curData.PROTECT_TIME - TimeManager.GetTime();
            if (RemainShield > 0)
            {
                ShieldRemain.transform.parent.gameObject.SetActive(true);
                ShieldRemain.text = SBFunc.TimeStringMinute(RemainShield);
            }
        }
        else
        {
            Nonconquest.SetActive(curData.CONQUEST == 0);

            ShieldRemain.transform.parent.gameObject.SetActive(false);

            GuildUI.Init(null);            
        }


        DimObject.SetActive(IntVal("is_open") <= 0);
        DimText.text = StringData.GetStringFormatByStrKey("제한구역닫힘", index);

        RemainTravel = -1;
        StateText.text = "";
        if(IntVal("travel_tag") > 0)
        {
            string state = StringData.GetStringFormatByStrKey("여행중");
            if (curData != null)
            {
                RemainTravel = IntVal("travel_start_at_ts") + curData.TIME - TimeManager.GetTime();
                if(RemainTravel > 0)
                    state = StringData.GetStringFormatByStrKey("여행중") + "\n" + SBFunc.TimeStringMinute(RemainTravel);                
                else
                    state = StringData.GetStringFormatByStrKey("여행완료");
            }

            StateText.text = state;

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
        }

        if (RemainShield > 0 || RemainTravel > 0)
            Invoke("TimeRefresh", 1.0f);
    }

    void TimeRefresh()
    {
        CancelInvoke("TimeRefresh");

        if (RemainShield > 0)
        {
            RemainShield--;
            ShieldRemain.text = SBFunc.TimeStringMinute(RemainShield);
        }
        else
        {
            ShieldRemain.transform.parent.gameObject.SetActive(false);
        }

        if (RemainTravel > 0)
        {
            RemainTravel--;
            StateText.text = StringData.GetStringFormatByStrKey("여행중") + "\n" + SBFunc.TimeStringMinute(RemainTravel);
        }
        else
        {
            StateText.text = StringData.GetStringFormatByStrKey("여행완료");
        }


        if (RemainShield >= 0 || RemainTravel >= 0)
            Invoke("TimeRefresh", 1.0f);
    }
}
