using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class ChampionBattleCharacterSlotFrame : CharacterSlotFrame
    {
        [SerializeField]
        protected GameObject ReddotAnimNode = null;

        public ChampionDragon CurDragon { get; private set; } = null; 
        protected override UserDragon GetDragon(int tag)
        {
            if (CurDragon != null)
                return CurDragon;

            return ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(tag);
        }
        public void ShowReddotAnimNode()
        {
            if (ReddotAnimNode != null && ReddotAnimNode.activeInHierarchy == false)
            {
                ReddotAnimNode.gameObject.SetActive(true);
            }
        }

        public void HideReddotAnimNode()
        {
            if (ReddotAnimNode != null && ReddotAnimNode.activeInHierarchy == true)
            {
                ReddotAnimNode.gameObject.SetActive(false);
            }
        }

        public void onClickDragonFrame()
        {
            if (clickCallback != null && clickActive)
            {
                SoundManager.Instance.PlaySFX("FX_BUTTON_OK");
                clickCallback(DragonTag.ToString());
            }
            else if(CurDragon != null)
            {
                bool settingEnable = false;

                var dragonData = new UserDragonData();
                ChampionBattleLine line = curBattleLine as ChampionBattleLine;
                if(line != null)
                {
                    int remain = 0;
                    switch (line.Teamtype)
                    {
                        case ParticipantData.eTournamentTeamType.DEFFENCE:
                            remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.DEF_TEAM_SET));
                            break;
                        case ParticipantData.eTournamentTeamType.ATTACK:
                            remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.ATK_TEAM_SET));
                            break;
                        case ParticipantData.eTournamentTeamType.HIDDEN:
                            remain = TimeManager.GetTimeCompare(ChampionManager.Instance.CurChampionInfo.GetContentsTime(ChampionInfo.CONTENTS_TIME.HIDDEN_TEAM_SET));
                            break;

                        default:
                            remain = -1;
                            break;
                    }

                    settingEnable = remain > 0;

                    foreach (var dragonTag in line.GetList())
                    {
                        if (dragonTag <= 0)
                            continue;

                        dragonData.AddUserDragon(dragonTag, ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(dragonTag));
                    }
                }
                else
                {
                    ChampionPracticeBattleLine practiceLine = curBattleLine as ChampionPracticeBattleLine;
                    if(practiceLine != null)
                    {
                        settingEnable = true;

                        foreach (var dragonTag in practiceLine.GetList())
                        {
                            if (dragonTag <= 0)
                                continue;

                            dragonData.AddUserDragon(dragonTag, practiceLine.GetPracticeDragon(dragonTag));
                        }
                    }
                }
                

                if (settingEnable)
                {
                    ChampionBattleDragonSelectPopup.OpenDragonSetting(CurDragon, dragonData);
                }
                else
                {
                    ChampionDragonDetailPopup.OpenPopup(CurDragon, dragonData);
                }
            }
            else if (DragonTag > 0)
            {
                var dragonData = new UserDragonData();
                ChampionBattleLine line = curBattleLine as ChampionBattleLine;
                if (line != null)
                {
                    foreach (var dragonTag in line.GetList())
                    {
                        if (dragonTag <= 0)
                            continue;

                        dragonData.AddUserDragon(dragonTag, ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(dragonTag));
                    }
                }
                else
                {
                    ChampionPracticeBattleLine practiceLine = curBattleLine as ChampionPracticeBattleLine;
                    if (practiceLine != null)
                    {
                        foreach (var dragonTag in practiceLine.GetList())
                        {
                            if (dragonTag <= 0)
                                continue;

                            dragonData.AddUserDragon(dragonTag, practiceLine.GetPracticeDragon(dragonTag));
                        }
                    }
                }

                ChampionBattleDragonSelectPopup.OpenDragonSetting(GetDragon(DragonTag) as ChampionDragon, dragonData);
            }
        }

        public override void SetDragonData(int dragonTag, bool isShowLevel = false, bool shadowState = true, BattleLine battleLine = null, bool dragEnable = true)
        {
            CurDragon = null;
            base.SetDragonData(dragonTag, true, false, battleLine);
        }

        public void SetDragonData(ChampionDragon dragon, BattleLine line)
        {
            CurDragon = dragon;
            int tag = 0;
            if (dragon != null)
                tag = dragon.Tag;
            HideReddotAnimNode();
            base.SetDragonData(tag, false, false, line);            
        }

        public override void SetClear(bool shadowState = false)
        {
            HideReddotAnimNode();
            base.SetClear(shadowState);

            CurDragon = null;
        }

        public override void ShowAnimArrowNode(bool shadowState = true)
        {
            HideReddotAnimNode();
            base.ShowAnimArrowNode(shadowState);
        }
    }
}

