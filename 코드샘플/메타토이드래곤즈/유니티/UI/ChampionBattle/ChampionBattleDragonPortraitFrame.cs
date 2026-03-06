using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EasyMobile.ConsentDialog;

namespace SandboxNetwork
{
    public class ChampionBattleDragonPortraitFrame : DragonPortraitFrame
    {
        [SerializeField]
        GameObject detailsNode = null;
        [SerializeField]
        GameObject ATK;
        [SerializeField]
        GameObject DEF;
        [SerializeField]
        GameObject HID;

        private func detailsClickCallback = null;
        private CharBaseData charData = null;

        void ClearTeamIcon()
        {
            if (ATK != null)
                ATK.SetActive(false);
            if (DEF != null)
                DEF.SetActive(false);
            if (HID != null)
                HID.SetActive(false);
        }

        void SetTeamType(ParticipantData.eTournamentTeamType teamType)
        {
            ClearTeamIcon();

            if (ATK != null)
                ATK.SetActive(teamType == ParticipantData.eTournamentTeamType.ATTACK);
            if (DEF != null)
                DEF.SetActive(teamType == ParticipantData.eTournamentTeamType.DEFFENCE);
            if (HID != null)
                HID.SetActive(teamType == ParticipantData.eTournamentTeamType.HIDDEN);
        }
        public override void SetDragonPortraitFrame(UserDragon _dragonData, bool isSelectCheck = false, bool clickEnable = true, bool isSpineOn = true)
        {
            ClearTeamIcon();
            dragonData = _dragonData;
            var dragonTag = dragonData.Tag;
            dragonID = dragonTag;

            var dragonInfo = CharBaseData.Get(dragonTag.ToString());

            SetSelectCheckNode(isSelectCheck);
            SetLevelNode(0);
            SetBG(dragonInfo, true, isSpineOn);
        }

        public void SetDragonPortraitFrame(UserDragon _dragonData, ParticipantData.eTournamentTeamType teamType)
        {
            dragonData = _dragonData;
            var dragonTag = dragonData.Tag;
            dragonID = dragonTag;

            var dragonInfo = CharBaseData.Get(dragonTag.ToString());

            SetSelectCheckNode(false);
            SetLevelNode(0);
            SetBG(dragonInfo, true);
            SetTeamType(teamType);
        }

        public void SetCustomPotraitFrameForSelect(int dragonTag, bool isSelectCheck)
        {
            ClearTeamIcon();
            dragonID = dragonTag;

            var dragonInfo = CharBaseData.Get(dragonTag.ToString());
            SetLevelNode(0);
            SetSelectCheckNode(isSelectCheck);
            SetBG(dragonInfo);
        }
        public void setDetailClickCallback(func ok_cb)
        {
            if (ok_cb != null)
            {
                detailsClickCallback = ok_cb;
            }
        }
        public void onClickDetailsNode()
        {
            if (detailsClickCallback != null)
            {
                detailsClickCallback(dragonID.ToString());
            }
        }
    }
}

