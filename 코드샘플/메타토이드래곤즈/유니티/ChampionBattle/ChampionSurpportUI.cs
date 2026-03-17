using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct ChampionSurpportUpdate
{
    public static ChampionSurpportUpdate e;

    public static void Send()
    {
        EventManager.TriggerEvent(e);
    }
}
public class ChampionSurpportUI : MonoBehaviour, EventListener<ChampionSurpportUpdate>
{
    [SerializeField] bool useBattle = false;
    [Serializable]
    public class Bar
    {        
        [SerializeField]
        ChampionSurpportInfo.eSurpportType type;
        [SerializeField] RectTransform TotalRect;
        [SerializeField] RectTransform AngelRect_Only;
        [SerializeField] RectTransform WonderRect_Only;
        [SerializeField] RectTransform LunaRect_Only;
        [SerializeField] RectTransform AngelRect_L;
        [SerializeField] RectTransform WonderRect_L;
        [SerializeField] RectTransform WonderRect_C;
        [SerializeField] RectTransform WonderRect_R;
        [SerializeField] RectTransform LunaRect_R;

        public void RefreshUI(ChampionSurpportInfo.SurpportDetial data)
        {
            RefreshUI(new int[] { data.AngelRateValue, data.WonderRateValue, data.LunaRateValue });
        }

        public void RefreshUI(int[] data)
        {
            if (data.Length < 3)
            {                
                return;
            }

            var width = TotalRect.sizeDelta.x;
            var height = TotalRect.sizeDelta.y;
            var pos = new Vector3(TotalRect.sizeDelta.x * -0.5f, 0f, 0f);
            float w = 0f;

            int aliveServerCount = 0;
            if (data[0] > 0)
                aliveServerCount++;
            if (data[1] > 0)
                aliveServerCount++;
            if (data[2] > 0)
                aliveServerCount++;

            switch (aliveServerCount)
            {
                case 3:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);

                    AngelRect_L.gameObject.SetActive(true);
                    w = data[0] * 0.01f * width;
                    AngelRect_L.sizeDelta = new Vector2(w, height);
                    AngelRect_L.localPosition = pos;
                    pos.x += w;

                    w = data[1] * 0.01f * width;
                    WonderRect_C.gameObject.SetActive(true);
                    WonderRect_C.sizeDelta = new Vector2(w, height);
                    WonderRect_C.localPosition = pos;
                    pos.x += w;

                    w = data[2] * 0.01f * width;
                    LunaRect_R.gameObject.SetActive(true);
                    LunaRect_R.sizeDelta = new Vector2(w, height);
                    LunaRect_R.localPosition = pos;
                    pos.x += w;
                    break;
                case 2:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);

                    RectTransform L_target = null;
                    RectTransform R_target = null;
                    int l_rate = 0;
                    int r_rate = 0;
                    if (data[0] > 0)
                    {
                        AngelRect_L.gameObject.SetActive(true);
                        L_target = AngelRect_L;
                        l_rate = data[0];
                        if (data[1] > 0)
                        {
                            WonderRect_R.gameObject.SetActive(true);
                            R_target = WonderRect_R;
                            r_rate = data[1];
                        }
                    }
                    if (data[2] > 0)
                    {
                        LunaRect_R.gameObject.SetActive(true);
                        R_target = LunaRect_R;
                        r_rate = data[2];
                        if (data[1] > 0)
                        {
                            WonderRect_L.gameObject.SetActive(true);
                            L_target = WonderRect_L;
                            l_rate = data[1];
                        }
                    }
                    w = (float)l_rate / (l_rate + r_rate) * width;
                    L_target.sizeDelta = new Vector2(w, height);
                    L_target.localPosition = pos;
                    pos.x += w;

                    w = (float)r_rate / (l_rate + r_rate) * width;
                    R_target.sizeDelta = new Vector2(w, height);
                    R_target.localPosition = pos;
                    pos.x += w;
                    break;
                case 1:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);

                    if (data[0] > 0)
                    {
                        AngelRect_Only.gameObject.SetActive(true);
                    }
                    if (data[1] > 0)
                    {
                        WonderRect_Only.gameObject.SetActive(true);
                    }
                    if (data[2] > 0)
                    {
                        LunaRect_Only.gameObject.SetActive(true);
                    }
                    break;
                default:
                    AngelRect_Only.gameObject.SetActive(false);
                    WonderRect_Only.gameObject.SetActive(false);
                    LunaRect_Only.gameObject.SetActive(false);
                    AngelRect_L.gameObject.SetActive(false);
                    WonderRect_L.gameObject.SetActive(false);
                    WonderRect_C.gameObject.SetActive(false);
                    WonderRect_R.gameObject.SetActive(false);
                    LunaRect_R.gameObject.SetActive(false);
                    break;
            }
        }
    }

    public Bar[] bars;

    private void OnEnable()
    {
        EventManager.AddListener(this);
        RefreshUI();
    }

    private void OnDisable()
    {
        EventManager.RemoveListener(this);
    }

    public void OnEvent(ChampionSurpportUpdate eventType)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        ChampionSurpportInfo.eSurpportType[] types = new ChampionSurpportInfo.eSurpportType[] {
            ChampionSurpportInfo.eSurpportType.PHYS_DMG_RESIS,
            ChampionSurpportInfo.eSurpportType.ALL_ELEMENT_DMG_RESIS,
            ChampionSurpportInfo.eSurpportType.CRI_DMG_RESIS,
        };


        if (useBattle)
        {
            bars[0].RefreshUI(ChampionManager.Instance.CurLoger.PDR.ToArray());
            bars[1].RefreshUI(ChampionManager.Instance.CurLoger.AEDR.ToArray());
            bars[2].RefreshUI(ChampionManager.Instance.CurLoger.CDR.ToArray());
            return;
        }

        for(int i = 0; i < 3; i++)
        {
            bars[i].RefreshUI(ChampionManager.Instance.CurChampionInfo.SurpportInfo.GetSurpportInfo(types[i]));
        }
        
    }

    public void OnClick()
    {
        ChampionSurpportPopup.OpenPopup(ChampionSurpportInfo.eSurpportType.NONE);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
