using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SandboxNetwork
{
    public class ArenaRankingInfoSlot : MonoBehaviour
    {
        [Header("portrait")]
        [SerializeField] private UserArenaPortraitFrame portrait = null;

        [SerializeField] private Text userNameLabel = null;
        [SerializeField] private Text battlePointLabel = null;
        [SerializeField] private Text arenaPointLabel = null;
        [SerializeField] private GameObject dragonPortraitSlotPrefab = null;
        [SerializeField] private Transform dragonPortraitParent = null; //상대방 드래곤 리스트 뿌려주는곳
        [SerializeField] private Sprite[] rankIconSpriteArr = null;// 랭킹 아이콘 (1~3등)
        [SerializeField] private Image rankIcon = null;
        [SerializeField] private Text rankLabel = null;
        [SerializeField] private Image backGroundImg = null;
        [SerializeField] private Color[] backGroundColors = null;
        [Header("guild")]
        [SerializeField] private GuildBaseInfoObject guildBaseInfoObject = null;


        private ArenaRankingInfoData currentArenaData = null;
        private List<GameObject> UserDragonPortraits = new List<GameObject>();
        public void Init(ArenaRankingInfoData arenaData)
        {
            if (arenaData == null) return;
            currentArenaData = arenaData;
            SetRankingInfoSlotData();
        }
        void SetRankingInfoSlotData()
        {
            if (!string.IsNullOrEmpty(currentArenaData.userName) && userNameLabel != null)
                userNameLabel.text = currentArenaData.userName;

            if (portrait != null)
                portrait.SetUserPortraitFrame(currentArenaData.UserData, false);

            userNameLabel.text = currentArenaData.userName;
            battlePointLabel.text = currentArenaData.userBattlePoint.ToString();
            arenaPointLabel.text = currentArenaData.UserData.Point.ToString();

            int rank = currentArenaData.userRank;
            if (rank > 0 && rank <= 3)
            {
                rankIcon.gameObject.SetActive(true);
                rankLabel.gameObject.SetActive(false);
                rankIcon.sprite = rankIconSpriteArr[rank - 1];
                if (backGroundImg !=null) 
                    backGroundImg.color = backGroundColors[rank - 1];
            }
            else
            {
                rankIcon.gameObject.SetActive(false);
                rankLabel.gameObject.SetActive(true);
                rankLabel.text = rank < 0 ? "-" : rank.ToString();
                if (backGroundImg != null)
                    backGroundImg.color = backGroundColors[3];
            }
            SetOtherTeamThumbnail();
            SetGuildInfo();
        }

        void SetOtherTeamThumbnail()
        {
            var dragonTagList = currentArenaData.userDefenceTeamList;
            if (dragonTagList == null || dragonTagList.Count <= 0) return;
            if (UserDragonPortraits.Count < dragonTagList.Count)
            {
                for (int i = UserDragonPortraits.Count; i < dragonTagList.Count; ++i)
                {
                    var clone = Instantiate(dragonPortraitSlotPrefab, dragonPortraitParent);
                    clone.SetActive(true);
                    UserDragonPortraits.Add(clone);
                }

            }
            else if (UserDragonPortraits.Count > dragonTagList.Count)
            {
                for (int i = dragonTagList.Count; i < UserDragonPortraits.Count; ++i)
                {
                    UserDragonPortraits[i].SetActive(false);
                }
            }
            for (int i = 0; i < dragonTagList.Count; ++i)
            {
                
                if (dragonTagList[i].Tag == 0)
                {
                    UserDragonPortraits[i].SetActive(false);
                    continue;
                }
                UserDragonPortraits[i].SetActive(true);
                var slot = UserDragonPortraits[i].GetComponent<DragonPortraitFrame>();
                slot.SetCustomPotraitFrame(dragonTagList[i].Tag, dragonTagList[i].Level, dragonTagList[i].TranscendenceStep ,dragonTagList[i].Tag < 0, false);
            }
        }

        void SetGuildInfo()
        {
            bool enableGuild = GuildManager.Instance.GuildWorkAble && currentArenaData.UserData.GuildNo > 0;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);
            if (enableGuild)
            {
                int markNo = currentArenaData.UserData.GuildMarkNo;
                int emblemNo = currentArenaData.UserData.GuildEmblemNo;
                string guildName = currentArenaData.UserData.GuildName;
                int guildNo = currentArenaData.UserData.GuildNo;
                guildBaseInfoObject.gameObject.SetActive(guildNo > 0);
                guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
            }
        }
        
    }
}