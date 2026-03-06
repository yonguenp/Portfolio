using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChampionPracticeDragonListView : ChampionBattleDragonListView
{
    [SerializeField]
    GameObject[] FilterUI;

    ChampionPracticeMode parent = null;
    ChampionPracticeBattleLine battleLine = null;
    protected override bool IsInTeamDragon(int tag)
    {
        return battleLine.IsContainDragon(tag);
    }

    protected override List<ChampionDragon> GetSelectableDragons()
    {
        return parent.GetPracticeDragons(battleLine.TeamSIde);
    }

    public void OnShow(ChampionPracticeMode p, ChampionPracticeBattleLine b, ClickCallBack regist_cb, ClickCallBack release_cb)
    {
        parent = p;
        battleLine = b;
        OnShowList();

        Init(GetSelectableDragons().Select(data => data.Tag).ToArray());
        
        clickRegistCallBack = regist_cb;
        clickReleaseCallback = release_cb;

        foreach(var ui in FilterUI)
            ui.SetActive(false);
    }
    public void OnClose()
    {
        clickRegistCallBack = null;
        clickReleaseCallback = null;

        OnHideList();

        foreach (var ui in FilterUI)
            ui.SetActive(true);
    }

    public override void ScrollDelegate(GameObject itemNode, ITableData item)
    {
        if (itemNode == null || item == null)
        {
            return;
        }
        var frame = itemNode.GetComponent<ChampionBattleDragonPortraitFrame>();
        if (frame == null)
        {
            return;
        }
        var dragonData = (UserDragon)item;
        var dragonTableData = CharBaseData.Get(dragonData.Tag);
        if (dragonTableData == null)
            return;

        frame.SetCustomPotraitFrameForSelect(dragonData.Tag, battleLine.IsContainDragon(dragonData.Tag));
        frame.setCallback((param) =>
        {
            int tag = int.Parse(param);
            bool check = IsInTeamDragon(tag);
            if (check)
            {
                if (clickReleaseCallback != null)
                {
                    clickReleaseCallback(dragonData.Tag);
                    RefreshDragonCountLabel(battleLine.DeckCount, 5);
                }
            }
            else
            {
                if (clickRegistCallBack != null)
                {
                    clickRegistCallBack(tag);
                    RefreshDragonCountLabel(battleLine.DeckCount, 5);
                }
            }
        });
    }

    public void LoadPreset(int type)
    {
        LoadPreset(type, false);
    }

    public void LoadPreset(int type, bool repeater)
    {
        ChampionLeagueTable.ROUND_INDEX prvIndex = ChampionLeagueTable.ROUND_INDEX.NONE;
        ChampionLeagueTable.ROUND_INDEX curIndex = ChampionLeagueTable.ROUND_INDEX.NONE;
        ChampionMatchData curData = null;
        ChampionMatchData prvData = null;
        foreach (var match in ChampionManager.Instance.CurChampionInfo.MatchData)
        {
            if ((match.Value.A_SIDE != null && match.Value.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO) || (match.Value.B_SIDE != null && match.Value.B_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO))
            {
                if (curIndex < match.Key)
                {
                    if (prvIndex < curIndex)
                    {
                        prvIndex = curIndex;
                        prvData = curData;
                    }

                    curIndex = match.Key;
                    curData = match.Value;
                }
                else if (prvIndex < match.Key)
                {
                    prvIndex = match.Key;
                    prvData = match.Value;
                }
            }
        }

        if (curData == null)
        {
            ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터없음"));
            return;
        }

        ChampionDragon[] line = null;
        switch (type)
        {
            case 0://상대방어                
                if (ChampionManager.Instance.CurChampionInfo.CurState == curData.round && ChampionManager.Instance.CurChampionInfo.CurStep >= ChampionInfo.ROUND_STEP.MATCH_DEFENSE_OPEN)
                {
                    if(curData.Detail == null || curData.Detail.UserADragons == null || curData.Detail.UserBDragons == null)
                    {
                        if (repeater)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터오류"));
                            return;
                        }

                        ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(curData.round, curData.match_slot, () =>
                        {
                            LoadPreset(type, true);
                        });
                        return;
                    }
                    else
                    {
                        if(curData.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO)
                        {
                            line = curData.Detail.UserBDragons.DefenceTeam;
                        }
                        else
                        {
                            line = curData.Detail.UserADragons.DefenceTeam;
                        }
                    }
                }
                else
                {
                    if(prvData == null)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터없음"));
                        return;
                    }
                    else
                    {
                        if (prvData.Detail == null || prvData.Detail.UserADragons == null || prvData.Detail.UserBDragons == null)
                        {
                            if (repeater)
                            {
                                ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터오류"));
                                return;
                            }

                            ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(prvData.round, prvData.match_slot, () =>
                            {
                                LoadPreset(type, true);
                            });
                            return;
                        }
                        else
                        {
                            ToastManager.On(StringData.GetStringByStrKey("덱공개전이전데이터로드"));

                            if (prvData.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO)
                            {
                                line = prvData.Detail.UserBDragons.DefenceTeam;
                            }
                            else
                            {
                                line = prvData.Detail.UserADragons.DefenceTeam;
                            }
                        }
                    }
                }
                break;
            case 1://상대공격
                if (ChampionManager.Instance.CurChampionInfo.CurState == curData.round && ChampionManager.Instance.CurChampionInfo.CurStep >= ChampionInfo.ROUND_STEP.MATCH_ATTACK_OPEN)
                {
                    if (curData.Detail == null || curData.Detail.UserADragons == null || curData.Detail.UserBDragons == null)
                    {
                        if (repeater)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터오류"));
                            return;
                        }

                        ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(curData.round, curData.match_slot, () =>
                        {
                            LoadPreset(type, true);
                        });
                        return;
                    }
                    else
                    {
                        if (curData.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO)
                        {
                            line = curData.Detail.UserBDragons.OffenceTeam;
                        }
                        else
                        {
                            line = curData.Detail.UserADragons.OffenceTeam;
                        }
                    }
                }
                else
                {
                    if (prvData == null)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터없음"));
                        return;
                    }
                    else
                    {
                        if (prvData.Detail == null || prvData.Detail.UserADragons == null || prvData.Detail.UserBDragons == null)
                        {
                            if (repeater)
                            {
                                ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터오류"));
                                return;
                            }

                            ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(prvData.round, prvData.match_slot, () =>
                            {
                                LoadPreset(type, true);
                            });
                            return;
                        }
                        else
                        {
                            ToastManager.On(StringData.GetStringByStrKey("덱공개전이전데이터로드"));

                            if (prvData.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO)
                            {
                                line = prvData.Detail.UserBDragons.OffenceTeam;
                            }
                            else
                            {
                                line = prvData.Detail.UserADragons.OffenceTeam;
                            }
                        }
                    }
                }
                break;
            case 2://상대히든
                if (ChampionManager.Instance.CurChampionInfo.CurState == curData.round && ChampionManager.Instance.CurChampionInfo.CurStep >= ChampionInfo.ROUND_STEP.MATCH)
                {
                    if (curData.Detail == null || curData.Detail.UserADragons == null || curData.Detail.UserBDragons == null)
                    {
                        if (repeater)
                        {
                            ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터오류"));
                            return;
                        }

                        ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(curData.round, curData.match_slot, () =>
                        {
                            LoadPreset(type, true);
                        });
                        return;
                    }
                    else
                    {
                        if (curData.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO)
                        {
                            line = curData.Detail.UserBDragons.HiddenTeam;
                        }
                        else
                        {
                            line = curData.Detail.UserADragons.HiddenTeam;
                        }
                    }
                }
                else
                {
                    if (prvData == null)
                    {
                        ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터없음"));
                        return;
                    }
                    else
                    {
                        if (prvData.Detail == null || prvData.Detail.UserADragons == null || prvData.Detail.UserBDragons == null)
                        {
                            if (repeater)
                            {
                                ToastManager.On(StringData.GetStringByStrKey("연습경기로드데이터오류"));
                                return;
                            }

                            ChampionManager.Instance.CurChampionInfo.ReqMatchInfo(prvData.round, prvData.match_slot, () =>
                            {
                                LoadPreset(type, true);
                            });
                            return;
                        }
                        else
                        {
                            ToastManager.On(StringData.GetStringByStrKey("덱공개전이전데이터로드"));

                            if (prvData.A_SIDE.USER_NO == ChampionManager.Instance.MyInfo.USER_NO)
                            {
                                line = prvData.Detail.UserBDragons.HiddenTeam;
                            }
                            else
                            {
                                line = prvData.Detail.UserADragons.HiddenTeam;
                            }
                        }
                    }
                }
                break;
            case 3://내방어
                var d = ChampionManager.Instance.CurChampionInfo.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.DEFFENCE).GetArray();
                line = new ChampionDragon[6];
                for(int i = 0; i < d.Length; i++)
                {
                    if(d[i] > 0)
                        line[i] = (ChampionDragon)ChampionManager.Instance.CurChampionInfo.MyInfo.ChampionDragons.GetDragon(d[i]);
                }
                break;
            case 4://내공격
                var a = ChampionManager.Instance.CurChampionInfo.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.ATTACK).GetArray();
                line = new ChampionDragon[6];
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] > 0)
                        line[i] = (ChampionDragon)ChampionManager.Instance.CurChampionInfo.MyInfo.ChampionDragons.GetDragon(a[i]);
                }
                break;
            case 5://내히든
                var h = ChampionManager.Instance.CurChampionInfo.MyInfo.GetChampionBattleFomation(ParticipantData.eTournamentTeamType.HIDDEN).GetArray();
                line = new ChampionDragon[6];
                for (int i = 0; i < h.Length; i++)
                {
                    if (h[i] > 0)
                        line[i] = (ChampionDragon)ChampionManager.Instance.CurChampionInfo.MyInfo.ChampionDragons.GetDragon(h[i]);
                }
                break;
        }

        LoadDeck(line);
    }

    public void LoadDeck(ChampionDragon[] dragons)
    {
        List<int> nos = new List<int>();
        for (int i = 0; i < dragons.Length; i++)
        {
            int no = 0;
            if(dragons[i] != null)
            {
                no = dragons[i].Tag;
                var dragon = battleLine.GetPracticeDragon(no);
                dragon.Clone(dragons[i]);
            }

            nos.Add(no);
        }

        if (nos.Count == 0)
            ToastManager.On(StringData.GetStringByStrKey("덱세팅없음안내"));

        battleLine.SetLine(nos);
        battleLine.SaveDeck();

        PracticeDragonChangedEvent.Refresh();

        OnClose();
    }
}
