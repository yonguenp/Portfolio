using Coffee.UIEffects;
using Google.Impl;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionWinnerPopup : Popup<PopupBase>
    {
        [SerializeField]
        private ChampionWinnerInfo winnerInfo = null;

        [SerializeField]
        private ChampionWinnerLayer winnerLayer = null;

        List<ChampionWinUserInfo> Champs = new List<ChampionWinUserInfo>();
        public override void InitUI()
        {
            DataLoad();
            if(Champs.Count > 0)
                winnerInfo.Init(Champs[0]);
            else
                winnerInfo.Init(null);

            winnerLayer.Init(Champs);
        }

        void DataLoad()
        {
            Champs.Clear();

            Dictionary<int, List<ChampionWinUserInfo>> datas = new Dictionary<int, List<ChampionWinUserInfo>>();
            foreach (var user in ChampionManager.Instance.CurChampionInfo.HallOfFameData)
            {
                foreach (var server in user)
                {
                    foreach (var u in server)
                    {
                        if (u.Type == JTokenType.Object)
                        {
                            var winner = new ChampionWinUserInfo((JObject)u, ((JProperty)user).Name);
                            if (winner.UID > 0)
                            {
                                if (!datas.ContainsKey(winner.SERVER))
                                    datas.Add(winner.SERVER, new List<ChampionWinUserInfo>());

                                datas[winner.SERVER].Add(winner);
                            }
                        }
                    }
                }
            }

            //최신순 정렬
            if(datas.ContainsKey(0))
            {
                if (datas[0].Count > 0)
                {
                    datas[0].Sort((a, b) => { return b.SEASON.CompareTo(a.SEASON); });
                    Champs.AddRange(datas[0]);
                }
            }

            int maxLength = 0;
            if (datas.ContainsKey(1))
            {
                if (datas[1].Count > 0)
                {
                    datas[1].Sort((a, b) => { return b.SEASON.CompareTo(a.SEASON); });
                    if (maxLength < datas[1].Count)
                        maxLength = datas[1].Count;
                }
            }
            if (datas.ContainsKey(2))
            {
                if (datas[2].Count > 0)
                {
                    datas[2].Sort((a, b) => { return b.SEASON.CompareTo(a.SEASON); });
                    if (maxLength < datas[2].Count)
                        maxLength = datas[2].Count;
                }
            }
            if (datas.ContainsKey(3))
            {
                if (datas[3].Count > 0)
                {
                    datas[3].Sort((a, b) => { return b.SEASON.CompareTo(a.SEASON); });
                    if (maxLength < datas[3].Count)
                        maxLength = datas[3].Count;
                }
            }

            for (int i = 0; i < maxLength; i++)
            {
                if (datas.ContainsKey(1))
                {
                    if (datas[1].Count > i)
                        Champs.Add(datas[1][i]);
                }
                if (datas.ContainsKey(2))
                {
                    if (datas[2].Count > i)
                        Champs.Add(datas[2][i]);
                }
                if (datas.ContainsKey(3))
                {
                    if (datas[3].Count > i)
                        Champs.Add(datas[3][i]);
                }
            }
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
        }

        public void OnClickWinnerSlot(ChampionWinUserInfo data)
        {
            if(data != null)
                winnerInfo.Init(data);
        }
    }
}

