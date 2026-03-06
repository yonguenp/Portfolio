using Google.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionWinnerInfo : MonoBehaviour
    {
        [Header("portrait")]
        [SerializeField] private UserArenaPortraitFrame portrait = null;

        [SerializeField] 
        private Text userNameLabel = null;
        [SerializeField] 
        private GameObject dragonPortraitSlotPrefab = null;

        [SerializeField]
        private Transform dragonPortraitParent = null;

        [SerializeField]
        private GameObject winnerLabel = null; // 크라운 이미지 + 우승자 텍스트가 포함된 오브젝트
        [SerializeField]
        private Text sessionCnt = null; // n회 우승자 텍스트
        [SerializeField]
        ServerFlagUI serverUI = null;
        [SerializeField]
        private GameObject[] noDataUI;
        [SerializeField]
        private GameObject[] hasDataUI;

        [Header("guild")]
        [SerializeField] 
        private GuildBaseInfoObject guildBaseInfoObject = null;

        private ChampionWinUserInfo currentData = null;
        public void Init(ChampionWinUserInfo ChampionInfo)
        {
            if (ChampionInfo == null)
            {
                foreach(var ui in noDataUI)
                {
                    ui.SetActive(true);
                }
                foreach (var ui in hasDataUI)
                {
                    ui.SetActive(false);
                }
                return;
            }

            foreach (var ui in noDataUI)
            {
                ui.SetActive(false);
            }
            foreach (var ui in hasDataUI)
            {
                ui.SetActive(true);
            }

            currentData = ChampionInfo;

            if (portrait != null)
                portrait.SetUserPortraitFrame(currentData.GetThumnailData(), false);

            userNameLabel.text = currentData.Nick;
            SetThumbnail();
            SetUI();
            SetGuildInfo();

            if (serverUI != null)
                serverUI.SetFlag(currentData.Server);
        }

        void SetUI()
        {
            string serverName = "";
            switch (currentData.SERVER)
            {
                case 1:
                case 2:
                case 3:
                    serverName = StringData.GetStringByStrKey("server_name:" + currentData.SERVER);
                    break;
                default:
                    serverName = StringData.GetStringByStrKey("통합");
                    break;
            }
            sessionCnt.text = StringData.GetStringFormatByStrKey("챔피언역대우승자", currentData.SEASON, serverName);
        }

        void SetThumbnail()
        {
            foreach (Transform child in dragonPortraitParent)
            {
                if (child == dragonPortraitSlotPrefab.transform)
                    continue;

                Destroy(child.gameObject);
            }

            dragonPortraitSlotPrefab.SetActive(true);
            foreach (var dragon in currentData.Dragons)
            {
                var clone = Instantiate(dragonPortraitSlotPrefab, dragonPortraitParent);
                clone.SetActive(true);
                var slot = clone.GetComponent<DragonPortraitFrame>();
                slot.SetCustomPotraitFrame(dragon, GameConfigTable.GetDragonLevelMax(), CharTranscendenceData.GetStepMax(eDragonGrade.Legend), false, false);
            }
            dragonPortraitSlotPrefab.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(dragonPortraitParent.GetComponent<RectTransform>());
        }

        void SetGuildInfo()
        {
            bool enableGuild = GuildManager.Instance.GuildWorkAble && currentData.HasGuild;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);

            if (enableGuild)
            {
                int markNo = currentData.GuildMarkNo;
                int emblemNo = currentData.GuildEmblemNo;
                string guildName = currentData.GuildName;
                int guildNo = currentData.GuildNo;
                guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
            }
        }

    }
}
