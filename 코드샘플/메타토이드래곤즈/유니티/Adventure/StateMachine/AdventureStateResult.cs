using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class AdventureStateResult : AdventureState
    {
        protected bool IsNetworkWait = false;
        public override bool OnEnter()
        {
            if (base.OnEnter())
            {
                IsNetworkWait = true;
                var lives = 0;
                JObject deckObj = new JObject();
                for (int i = 0, count = Data.Characters.Count; i < count; ++i)
                {
                    if (Data.Characters[i] == null)
                        continue;

                    var stat = Data.Characters[i].Stat.Clone();
                    if(stat != null)
                    {
                        stat.ClearCategory(eStatusCategory.ADD_BUFF);
                        stat.ClearCategory(eStatusCategory.RATIO_BUFF);

                        stat.CalcStatusAll();

                        deckObj.Add(Data.Characters[i].ID.ToString(), stat.GetTotalINF());
                    }

                    if (Data.Characters[i].Death)
                        continue;

                    lives++;
                }

                var result = Data.State;
                if (result == eBattleState.None || result == eBattleState.Playing)
                    result = lives > 0 ? eBattleState.Playing : eBattleState.Lose;

                // 이펙트 커스텀 작동 정지
                EffectReceiverClearEvent.Send();

                var form = new WWWForm();
                form.AddField("holder",User.Instance.IS_HOLDER?"1":"0");
                form.AddField("tag", Data.BattleTag);
                form.AddField("result", (int)result);
                form.AddField("log", "[]");
                form.AddField("alives", lives);
                form.AddField("deckinf", deckObj.ToString());
#if UNITY_EDITOR
                //var myDat = StatisticsMananger.Instance.GetDamageInfoString(true);
                //form.AddBinaryData("my", System.Text.Encoding.Default.GetBytes(myDat));
                //form.AddField("my", myDat);
#endif
                NetworkManager.Send("adventure/result", form, (NetworkManager.SuccessCallback)((jsonData) =>
                { //성공 결과에 따라 처리
                    if (SBFunc.IsJTokenType(jsonData["rs"], JTokenType.Integer)
                    && jsonData["rs"].Value<int>() == (int)eApiResCode.OK)
                    {
                        if (SBFunc.IsJTokenType(jsonData["state"], JTokenType.Integer))
                        {
                            var state = (eBattleState)jsonData["state"].Value<int>();
                            switch (state)
                            {
                                case eBattleState.Playing:
                                    AdventureManager.Instance.SetWaveData(jsonData);
                                    if (AdventureManager.Instance.IsWaveCheck())
                                    {
                                        BossAlert();
                                        IsNetworkWait = false;
                                    }
                                    else
                                    {
                                        ShowErrorSystemPopup();
                                    }
                                    return;
                                case eBattleState.Win:
                                case eBattleState.Lose:
                                case eBattleState.TimeOver:
                                    AdventureManager.Instance.SetRewardData(jsonData);
                                    if (AdventureManager.Instance.IsRewardCheck())
                                    {
                                        IsNetworkWait = false;
                                    }
                                    else
                                    {
                                        ShowErrorSystemPopup();
                                    }

                                    return;
                                case eBattleState.None:
                                default:
                                    ShowErrorSystemPopup();
                                    break;
                            }
                        }
                    }
                    else
                    {
                        ShowErrorSystemPopup();
                    }

                    ////네트워크 통신 오류 밖으로 나가게하자
                    ////
                    //this.IsNetworkWait = false;
                }),
                (error) => //네트워크 연결 오류 밖으로 나가게하자
                {
                    IsNetworkWait = false;
                });

                Stage.PrevSpines.Clear();
                switch (result)
                {
                    case eBattleState.Win:
                    case eBattleState.Playing:
                    {
                        var it = Stage.DefenseSpines.GetEnumerator();
                        while (it.MoveNext())
                        {
                            List<BattleSpine> prevEnemys = new List<BattleSpine>();

                            var itit = it.Current.GetEnumerator();
                            while (itit.MoveNext())
                            {
                                //UnityEngine.Object.Destroy(itit.Current.gameObject);
                                prevEnemys.Add(itit.Current);
                            }

                            Stage.PrevSpines.Add(prevEnemys);
                        }
                        Stage.DefenseSpines.Clear();
                    }
                    break;
                }
                return true;
            }
            return false;
        }

        public virtual void BossAlert()
        {
            var dics = Data.DefenseDic;
            IBattleCharacterData bossData = null;
            foreach (var it in Data.DefensePos)
            {
                var listEnemy = it.Value;

                for (int y = 0, count = listEnemy.Count; y < count; ++y)
                {
                    var curPos = listEnemy[y];

                    if (!dics.ContainsKey(curPos.BattleTag))
                        continue;
                    var curData = dics[curPos.BattleTag];
                    if (curData != null && curData.IsBoss)
                        bossData = curData;
                }
            }
            if (bossData != null)
                UIBattleBossStartEvent.Send(bossData);
        }
        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Data.Time += dt;
                if (Data.Wave < Data.MaxWave)
                {
                    if (Data.CheckTimeOver())
                        return false;

                    Stage.Map.UpdateLeft(dt);

                    for (int x = 0, count = Stage.OffenseSpines.Count; x < count; ++x)
                    {
                        var list = Stage.OffenseSpines[x];
                        if (list == null)
                            continue;

                        for (int y = 0, yCount = list.Count; y < yCount; ++y)//드래곤 정위치 이동
                        {
                            var curDragon = list[y];
                            if (curDragon == null || curDragon.Data.Death)
                                continue;

                            curDragon.SetAnimation(eSpineAnimation.WALK);
                            curDragon.UpdateStatus(dt);
                            curDragon.GetComponent<Collider2D>().enabled = false;
                            curDragon.OnDust();

                        }
                    }
                }
                return IsNetworkWait;
            }
            return !IsPlaying;
        }
        protected override void ShowErrorSystemPopup()
        {
            IsNetworkWait = true;
            base.ShowErrorSystemPopup();
        }
    }
}