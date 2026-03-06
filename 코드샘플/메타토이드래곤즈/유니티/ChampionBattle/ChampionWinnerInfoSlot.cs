using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Google.Impl;

namespace SandboxNetwork
{
    public class ChampionWinnerInfoSlot : MonoBehaviour
    {
        [Header("portrait")]
        [SerializeField] private UserArenaPortraitFrame portrait = null;

        [SerializeField] private Text userNameLabel = null;
        [SerializeField] private Text sessionCnt = null;
        [SerializeField] private Text battlePointLabel = null;
        [SerializeField] private Text arenaPointLabel = null;
        [SerializeField] private GameObject dragonPortraitSlotPrefab = null;
        [SerializeField] private Transform dragonPortraitParent = null; 
        [SerializeField] private Image rankIcon = null;
        [SerializeField] private Text rankLabel = null;
        [SerializeField] private Image backGroundImg = null;
        [SerializeField] private ServerFlagUI serverFlag = null;

        [Header("guild")]
        [SerializeField] private GuildBaseInfoObject guildBaseInfoObject = null;
        [SerializeField] private List<GameObject> DragonPortraits = new List<GameObject>();
        [Header("parent")]
        [SerializeField]
        private ChampionWinnerPopup parent = null;

        ChampionWinUserInfo data = null;


        public void Init(ChampionWinUserInfo winner)
        {
            data = winner;
            SetRankingInfoSlotData();
        }

        void SetRankingInfoSlotData()
        {
            if (portrait != null)
                portrait.SetUserPortraitFrame(data.GetThumnailData(), false);

            userNameLabel.text = data.Nick;
            //battlePointLabel.text = "";
            //arenaPointLabel.text = "";
            string serverName = "";
            switch(data.SERVER)
            {
                case 1:
                case 2:
                case 3:
                    serverName = StringData.GetStringByStrKey("server_name:" + data.SERVER);
                    break;
                default:
                    serverName = StringData.GetStringByStrKey("통합");
                    break;
            }
            sessionCnt.text = StringData.GetStringFormatByStrKey("챔피언역대우승자", data.SEASON, serverName);
            SetOtherTeamThumbnail();
            SetGuildInfo();

            if (serverFlag != null)
                serverFlag.SetFlag(data.Server);
        }

        public void OnClickWinner()
        {
            if(data != null)
                parent.OnClickWinnerSlot(data);
        }

        void SetOtherTeamThumbnail()
        {
            foreach (Transform child in dragonPortraitParent)
            {
                if (child == dragonPortraitSlotPrefab.transform)
                    continue;

                Destroy(child.gameObject);
            }

            dragonPortraitSlotPrefab.SetActive(true);
            foreach (var dragon in data.Dragons)
            {
                var clone = Instantiate(dragonPortraitSlotPrefab, dragonPortraitParent);
                clone.SetActive(true);
                var slot = clone.GetComponent<DragonPortraitFrame>();
                slot.SetCustomPotraitFrame(dragon, GameConfigTable.GetDragonLevelMax(), CharTranscendenceData.GetStepMax(eDragonGrade.Legend), false, false);
            }
            dragonPortraitSlotPrefab.SetActive(false);
        }

        void SetGuildInfo()
        {
            bool enableGuild = GuildManager.Instance.GuildWorkAble && data.HasGuild;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);

            if (enableGuild)
            {
                int markNo = data.GuildMarkNo;
                int emblemNo = data.GuildEmblemNo;
                string guildName = data.GuildName;
                int guildNo = data.GuildNo;
                guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
            }
        }
        
    }
}