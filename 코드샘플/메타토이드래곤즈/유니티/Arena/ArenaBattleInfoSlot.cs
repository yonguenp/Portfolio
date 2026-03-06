using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SandboxNetwork
{
    public class ArenaBattleInfoSlot : MonoBehaviour
    {
        [Header("portrait")]
        [SerializeField] private UserArenaPortraitFrame portrait = null;

        [SerializeField] private Text userNameLabel = null;
        [SerializeField] private Text battlePointLabel = null;
        [SerializeField] private Text arenaPointLabel = null;

        [SerializeField] private GameObject dragonPortraitSlotPrefab = null;
        [SerializeField] private Transform dragonPortraitParent = null; //상대방 드래곤 리스트 뿌려주는곳

        [SerializeField] private GameObject battleStartButtonNode = null;
        [SerializeField] private Image battleWinIconNode = null;
        [SerializeField] private GameObject[] battleResultIconBack = null;
        [SerializeField] private Sprite win_icon = null;
        [SerializeField] private Sprite lose_icon = null;

        [SerializeField] private Image rightSideBack = null;

        [SerializeField] private Color winColor = Color.white;
        [SerializeField] private Color loseColor = Color.white;
        [SerializeField] private Color defaultColor = Color.white;

        [SerializeField] private List<GameObject> UserDragonPortraits = new List<GameObject>();

        [Header("guild")]
        [SerializeField] private GuildBaseInfoObject guildBaseInfoObject = null;

        private ArenaBattleInfoData currentArenaData = null;

        bool buttonLock = false;
        public void Init(ArenaBattleInfoData arenaData)
        {
            if (arenaData == null) return;
            currentArenaData = arenaData;
            SetBattleInfoSlotData();
            CancelInvoke("ButtonUnlock");
            buttonLock = true;
            Invoke("ButtonUnlock", 0.5f);
        }
        void ButtonUnlock()
        {
            CancelInvoke("ButtonUnlock");
            buttonLock = false;
        }
        void SetBattleInfoSlotData()
        {
            if(!string.IsNullOrEmpty(currentArenaData.userName) && userNameLabel != null)
                userNameLabel.text = currentArenaData.userName;

            if (portrait != null)
                portrait.SetUserPortraitFrame(currentArenaData.UserInfoData, false);

            battlePointLabel.text = currentArenaData.userBattlePoint.ToString();
            arenaPointLabel.text = currentArenaData.UserInfoData.Point.ToString();

            battleStartButtonNode.SetActive(false);
            battleWinIconNode.gameObject.SetActive(true);
            battleResultIconBack[0].SetActive(false);
            battleResultIconBack[1].SetActive(false);
            switch (currentArenaData.userBattleState)
            {
                case eMatchListState.OPEN:
                    battleStartButtonNode.SetActive(true);
                    battleWinIconNode.gameObject.SetActive(false);
                    rightSideBack.color = defaultColor;
                    break;
                case eMatchListState.OFFENSE:
                    battleResultIconBack[0].SetActive(true);
                    battleWinIconNode.sprite = win_icon;
                    rightSideBack.color = winColor;
                    break;
                case eMatchListState.DEFENSE:
                    battleResultIconBack[1].SetActive(true);
                    battleWinIconNode.sprite = lose_icon;
                    rightSideBack.color = loseColor;
                    break;
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
                slot.SetCustomPotraitFrame(dragonTagList[i].Tag, dragonTagList[i].Level, dragonTagList[i].TranscendenceStep, dragonTagList[i].Tag < 0, false);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(dragonPortraitParent.GetComponent<RectTransform>());
        }

        public void OnClickBattleButton()
        {
            if (buttonLock)
                return;
            buttonLock = true;
            Invoke("ButtonUnlock", 0.5f);
            ArenaManager.Instance.SetArenaVersusTeamData(currentArenaData.BattleListIndex, true);

            //LoadingManager.ImmediatelySceneLoad("ArenaBattleReady");//임시
            LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);
        }

        void SetGuildInfo()
        {
            bool enableGuild = GuildManager.Instance.GuildWorkAble && currentArenaData.UserInfoData.GuildNo > 0;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);
            if (enableGuild)
            {
                int markNo = currentArenaData.UserInfoData.GuildMarkNo;
                int emblemNo = currentArenaData.UserInfoData.GuildEmblemNo;
                string guildName = currentArenaData.UserInfoData.GuildName;
                int guildNo = currentArenaData.UserInfoData.GuildNo;
                guildBaseInfoObject.gameObject.SetActive(guildNo > 0);
                guildBaseInfoObject.Init(guildName, markNo, emblemNo, guildNo);
            }
        }
    }
}
