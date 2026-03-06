using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class WorldBossStateResult : WorldBossState
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

                //result 필드 값 정의 : 클라에서 3분을 버티면 - timeover or  전멸 lose
                //이탈 or 일시정지 탈주 일 때 : (abort) 5 -> eBattleState.Abort // worldbossManager에서 pausePopup에서 연결 처리
                //wave가 1 (단판) 이기 때문에 result 바로 수정해서 날려도 괜찮을 것 같음.
                var result = Data.State;
                if (result == eBattleState.None || result == eBattleState.Playing)
                    result = lives > 0 ? eBattleState.TimeOver : eBattleState.Lose;
                else
                    result = Data.State;

                // 이펙트 커스텀 작동 정지
                EffectReceiverClearEvent.Send();

                WorldBossManager.Instance.RequestBattleResult(result, (jsonData) => {
                    if (WorldBossManager.Instance.IsRewardCheck())
                        IsNetworkWait = false;
                    else
                        ShowErrorSystemPopup();
                }, (failString) =>
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

        public override bool Update(float dt)
        {
            if (base.Update(dt))
            {
                Data.Time += dt;
                if (Data.Wave < Data.MaxWave)
                {
                    if (Data.CheckTimeOver())
                        return false;

                    //Stage.Map.UpdateLeft(dt);

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