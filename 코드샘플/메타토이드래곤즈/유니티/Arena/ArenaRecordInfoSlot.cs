using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SandboxNetwork { 
    public class ArenaRecordInfoSlot : MonoBehaviour
    {
        [Header("portrait")]
        [SerializeField] private UserArenaPortraitFrame portrait = null;

        [SerializeField] private Text userNameLabel = null;
        [SerializeField] private Text arenaTimeLogLabel = null;
        [SerializeField] private Text battlePointLabel = null;
        [SerializeField] private Text arenaPointLabel = null;

        [SerializeField] private GameObject dragonPortraitSlotPrefab = null;
        [SerializeField] private Transform dragonPortraitParent = null; //상대방 드래곤 리스트 뿌려주는곳

        [SerializeField] private GameObject battleStartButtonNode = null;
        [SerializeField] private Image battleWinIconNode = null;
        [SerializeField] private Sprite win_icon = null;
        [SerializeField] private Sprite lose_icon = null;

        [Header("default info")]
        [SerializeField] private Image colorBgTarget = null;
        [SerializeField] private Text recordResultLabel = null;
        [SerializeField] private Text recordArenaPointLabel = null;
        
        [Header("color Info")]
        [SerializeField] private Color winBgColor;
        [SerializeField] private Color loseBgColor;

        [Header("guild")]
        [SerializeField] private GuildBaseInfoObject guildBaseInfoObject = null;


        private ArenaRecordInfoData currentArenaData = null;
        private List<GameObject> UserDragonPortraits = new List<GameObject>();
        bool buttonLock = false;
        public void Init(ArenaRecordInfoData arenaData)
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

            if (!string.IsNullOrEmpty(currentArenaData.userName) && userNameLabel != null)
                userNameLabel.text = currentArenaData.userName;

            if (portrait != null)
                portrait.SetUserPortraitFrame(currentArenaData.UserData, false);

            battlePointLabel.text = currentArenaData.userBattlePoint.ToString();
            arenaPointLabel.text = currentArenaData.UserData.Point.ToString();
            
            string timeString = StringData.GetStringByIndex(100001189);
            if (currentArenaData.recordDay > 0)
            {
                timeString = string.Format(StringData.GetStringByIndex(100001192), currentArenaData.recordDay, currentArenaData.recordHour - currentArenaData.recordDay * 24);
            }
            else if (currentArenaData.recordHour > 0){
                timeString = string.Format(StringData.GetStringByIndex(100001191), currentArenaData.recordHour, currentArenaData.recordMinutes);
            }
            else if (currentArenaData.recordMinutes > 0)
            {
                timeString = string.Format(StringData.GetStringByIndex(100001190), currentArenaData.recordMinutes);
            }
            arenaTimeLogLabel.text = timeString;
            RefreshDefaultInfoUIByState();
            SetOtherTeamThumbnail();
            SetGuildInfo();
        }

        void RefreshDefaultInfoUIByState()
        {
            battleStartButtonNode.SetActive(currentArenaData.userBattleState==eMatchListState.OFFENSE);
            battleWinIconNode.gameObject.SetActive(currentArenaData.userBattleState != eMatchListState.OFFENSE && currentArenaData.userBattleState != eMatchListState.OPEN);
            switch (currentArenaData.userBattleState)
            {
                case eMatchListState.OFFENSE:
                case eMatchListState.OPEN:
                case eMatchListState.REV_FAIL:
                case eMatchListState.REV_SUCCESS:  //방어 실패
                    colorBgTarget.color = loseBgColor;
                    recordResultLabel.text = StringData.GetStringByIndex(100001188);
                    battleWinIconNode.sprite = currentArenaData.userBattleState == eMatchListState.REV_SUCCESS? win_icon : lose_icon;
                    break;
                case eMatchListState.DEFENSE: //방어 성공
                    colorBgTarget.color = winBgColor;
                    recordResultLabel.text = StringData.GetStringByIndex(100001187);
                    battleWinIconNode.sprite = win_icon;
                    break;
                case eMatchListState.ERROR:
                    recordResultLabel.text = "eMatchListState is error";
                    break;
             // case eMatchListState.draw:
            }
            recordArenaPointLabel.text = currentArenaData.resultArenaPoint.ToString("+#;-#;0");
        }

        void SetOtherTeamThumbnail()
        {
            var dragonTagList = currentArenaData.userATKTeamList;
            if (dragonTagList == null || dragonTagList.Count <= 0) return;
            SBFunc.RemoveAllChildrens(dragonPortraitParent);
            for (int i = 0; i < dragonTagList.Count; ++i)
            {
                DragonInfo dragonData = dragonTagList[i];
                var clone = Instantiate(dragonPortraitSlotPrefab, dragonPortraitParent);
                clone.SetActive(true);
                if (dragonData == null) continue;

                var dragonTag = dragonData.Tag;
                var dragonLevel = dragonData.Level;
                var dragonTranscend = dragonData.TranscendenceStep;
                var checkDragonTag = dragonTag == 0;
                if (checkDragonTag) continue;

                DragonPortraitFrame slot = clone.GetComponent<DragonPortraitFrame>();
                slot.SetCustomPotraitFrame(dragonTag, dragonLevel, dragonTranscend, (dragonTag < 0), false);
            }
        }

        public void onClickRevengeBtn()
        {
            if (buttonLock)
                return;
            buttonLock = true;
            Invoke("ButtonUnlock", 0.5f);
            ArenaManager.Instance.SetArenaVersusTeamData(currentArenaData.battleListIndex, false);

            //LoadingManager.ImmediatelySceneLoad("ArenaBattleReady");//임시
            LoadingManager.Instance.EffectiveSceneLoad("ArenaBattleReady", eSceneEffectType.CloudAnimation);
        }

        void SetGuildInfo()
        {
            bool enableGuild = GuildManager.Instance.GuildWorkAble && currentArenaData.UserData.GuildNo > 0;
            guildBaseInfoObject.gameObject.SetActive(enableGuild);
            if (enableGuild)
            {
                int guildMark = currentArenaData.UserData.GuildMarkNo;
                int emblemNo = currentArenaData.UserData.GuildEmblemNo;
                string guildName = currentArenaData.UserData.GuildName;
                int guildNo = currentArenaData.UserData.GuildNo;
                guildBaseInfoObject.gameObject.SetActive(guildNo > 0);
                guildBaseInfoObject.Init(guildName, guildMark, emblemNo, guildNo);
            }
        }
    }
}