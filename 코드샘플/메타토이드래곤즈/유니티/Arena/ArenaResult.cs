using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SandboxNetwork {
    public class ArenaResult : MonoBehaviour
    {

        [SerializeField]
        private Image bgImg = null;
        [SerializeField]
        private Sprite[] bgSprites = null;

        [SerializeField]
        private Text battleTimeLabel = null;
        
        [SerializeField]
        private GameObject arenaTrophyIcon = null;
        [SerializeField]
        private Text arenaTrophyLabel = null;
        [SerializeField]
        private GameObject arenaPointIcon = null;
        [SerializeField]
        private Text arenaPointLabel = null;
        [SerializeField]
        private GameObject accountPointIcon = null;
        [SerializeField]
        private Text accountPointLabel = null;
        [SerializeField]
        private GameObject guildExpIcon = null;
        [SerializeField]
        private Text guildExpLabel = null;
        [SerializeField]
        private GameObject winLayerNode = null;
        [SerializeField]
        private Color winnerLvColor;
        [SerializeField]
        private GameObject loseLayerNode = null;
        [SerializeField]
        private Color loseLvColor;

        [SerializeField]
        private GameObject[] dragonListNode;
        [SerializeField]
        private Text[] dragonLvTexts;

        [SerializeField] Animator resultAnimator;
        [SerializeField] float fontStartDelay;

        [SerializeField] GameObject winAnim = null; // 임시
        [SerializeField] GameObject loseAnim = null; // 임시

        

        [Header("friend change objects")]
        [SerializeField] Button townButton = null;
        [SerializeField] GameObject rewardNode = null;
        [SerializeField] RectTransform winParticleRect = null;
        [SerializeField] RectTransform loseParticleRect = null;
        [SerializeField] RectTransform dragonSpineRect = null;
        [SerializeField] RectTransform titleRect = null;


        ArenaBattleData arenaResultData = null;
        bool isWin = false;

        bool isRequestArenaData = false;
        void Start()
        {
            arenaResultData = ArenaManager.Instance.ColosseumData;

            UIManager.Instance.InitUI(eUIType.None);
            var charDatas = arenaResultData.OffenseDic;
            if (charDatas != null)
            {
                int charCount = 0;
                if (arenaResultData.WinType == eArenaWinType.Open)
                {
                    winLayerNode.SetActive(true);
                    loseLayerNode.SetActive(false);
                    StartCoroutine(animCoroutine(true));
                    isWin = true;
                }
                else
                {
                    winLayerNode.SetActive(false);
                    loseLayerNode.SetActive(true);
                    StartCoroutine(animCoroutine(false));
                }
                winAnim.SetActive(isWin);
                loseAnim.SetActive(!isWin);
                foreach (var it in arenaResultData.OffenseDic)
                {
                    var elem = it.Value;
                    int tag = elem.ID;
                    var data = elem.BaseData as CharBaseData;
                    if (data != null)
                    {
                        GameObject dragonClone = Instantiate(data.GetUIPrefab(), dragonListNode[charCount].transform);
                        dragonClone.transform.SetAsFirstSibling();
                        UIDragonSpine spine = dragonClone.GetComponent<UIDragonSpine>();
                        if (spine == null)
                            spine = dragonClone.AddComponent<UIDragonSpine>();

                        var myDragonData = User.Instance.DragonData.GetDragon(tag);
                        spine.SetData(myDragonData);
                        spine.InitComplete = SpineInitCallback;

                        var image = dragonListNode[charCount].GetComponent<Image>();
                        if (image != null)
                        {
                            image.color = new Color(0, 0, 0, 0);
                        }

                        dragonClone.transform.localScale = new Vector3(-2, 2, 1);
                        int dragonLv = myDragonData.Level;
                        dragonLvTexts[charCount].text = string.Format("Lv. {0}", dragonLv);
                        ++charCount;

                        if (isWin) { 
                            if (elem.Death)
                            {
                                spine.SetAnimation(eSpineAnimation.LOSE);
                            }
                            else
                            {
                                spine.SetAnimation(eSpineAnimation.WIN);
                            }
                        }
                        else
                        {
                            spine.SetAnimation(eSpineAnimation.LOSE);
                        }
                        
                    }
                }


                bgImg.sprite = isWin ? bgSprites[0] : bgSprites[1];

                for (int i = charCount; i < dragonListNode.Length; ++i)
                {
                    dragonListNode[i].SetActive(false);
                }
                if (arenaTrophyLabel != null)
                {
                    string pointString = arenaResultData.RewardTrophy.ToString("+#;-#;0");
                    arenaTrophyLabel.text = pointString;

                    arenaTrophyLabel.gameObject.SetActive(arenaResultData.RewardTrophy != 0);
                }

                if (arenaTrophyIcon != null)
                {
                    arenaTrophyIcon.SetActive(arenaResultData.RewardTrophy != 0);
                }

                if (arenaPointLabel != null)
                {
                    string pointString = arenaResultData.RewardPoint.ToString("+#;-#;0");
                    arenaPointLabel.text = pointString;

                    arenaPointLabel.gameObject.SetActive(arenaResultData.RewardPoint != 0);
                }

                if(arenaPointIcon != null)
                {
                    arenaPointIcon.SetActive(arenaResultData.RewardPoint != 0);
                }

                if (accountPointLabel != null)
                {
                    string pointString = arenaResultData.RewardAccountExp.ToString("+#;-#;0");
                    accountPointLabel.text = pointString;

                    accountPointLabel.gameObject.SetActive(arenaResultData.RewardAccountExp != 0);
                }

                if (accountPointIcon != null)
                {
                    accountPointIcon.SetActive(arenaResultData.RewardAccountExp != 0);
                }

                if (guildExpLabel != null)
                {
                    string pointString = arenaResultData.RewardGuildExp.ToString("+#;-#;0");
                    guildExpLabel.text = pointString;

                    guildExpLabel.gameObject.SetActive(arenaResultData.RewardGuildExp < 0);
                }

                if (guildExpIcon != null)
                {
                    guildExpIcon.SetActive(arenaResultData.RewardGuildExp < 0);
                }

                SetModifyByFriendData();//친선전 일 경우 세팅


                SoundManager.Instance.PushBGM(isWin ? "BGM_BATTLE_VICTORY" : "BGM_BATTLE_DEFEAT", true, false);
                battleTimeLabel.text = SBFunc.TimeString(arenaResultData.Time);
            }
        }

        //친선전일 경우에 수정해야하는 오브젝트들
        void SetModifyByFriendData()
        {
            bool isFriend = ArenaManager.Instance.IsFriendFightDataFlag;
            bool isRestrictedBattle = ArenaManager.Instance.IsRestrictedFightDataFlag;
            //친구 대전일 경우 보상 내용 다끔
            if (isFriend || isRestrictedBattle)
            {
                if (accountPointIcon != null)
                    accountPointIcon.SetActive(false);
                if (accountPointLabel != null)
                    accountPointLabel.gameObject.SetActive(false);
                if (arenaPointIcon != null)
                    arenaPointIcon.SetActive(false);
                if (arenaPointLabel != null)
                    arenaPointLabel.gameObject.SetActive(false);
                if (arenaTrophyLabel != null)
                    arenaTrophyLabel.gameObject.SetActive(false);
                if (arenaTrophyIcon != null)
                    arenaTrophyIcon.SetActive(false);
                if (guildExpIcon != null)
                    guildExpIcon.SetActive(false);
            }

            if (townButton != null)//타운 친선일 때 활성화
                townButton.gameObject.SetActive(isFriend);

            if (rewardNode != null)//보상 친선일 때 끔
                rewardNode.gameObject.SetActive(!isFriend && !isRestrictedBattle);

            winParticleRect.anchoredPosition = isFriend || isRestrictedBattle ? new Vector2(0, 296) : new Vector2(0, 426);
            loseParticleRect.anchoredPosition = isFriend || isRestrictedBattle ? new Vector2(0, 296) : new Vector2(0, 426);
            dragonSpineRect.anchoredPosition = isFriend || isRestrictedBattle ? new Vector2(0, -132) : new Vector2(0, -2);
            titleRect.anchoredPosition = isFriend || isRestrictedBattle ? new Vector2(0, 302) : new Vector2(0, 432);
        }

        IEnumerator animCoroutine(bool isWin)
        {
            resultAnimator.SetBool("win", isWin);
            yield return SBDefine.GetWaitForSeconds(fontStartDelay);
            resultAnimator.SetBool("layer1End", true);
        }

        void SpineInitCallback(UIDragonSpine spineData)
        {
            spineData.SetShadow(true);
        }

        void CharacterDestroy()
        {
            if (dragonListNode != null)
            {
                foreach(GameObject elem in dragonListNode)
                {
                    if (elem == null) continue;
                    SBFunc.RemoveAllChildrens(elem.transform);
                }
            }
        }
        public void onClickToLobby()
        {
            if (isRequestArenaData)
            {
                return;
            }

            isRequestArenaData = true;

            //친구 대전 일 때
            if(ArenaManager.Instance.IsFriendFightDataFlag)
            {
                isRequestArenaData = false;
                CharacterDestroy();

                //var userChatInfo = ArenaManager.Instance.GetFriendFightDataSet();
                //ChatUserData chatUserData = new ChatUserData(userChatInfo.user_no, userChatInfo.name, userChatInfo.icon_name, userChatInfo.level, userChatInfo.portraitData);

                ArenaManager.Instance.ClearFriendTeamDataSet();

                LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town),
                        SBFunc.CallBackCoroutine(() => { PopupManager.OpenPopup<ChattingPopup>().SetDirectFriendListLayer(); }
                    /*PopupManager.OpenPopup<FriendPopup>()*/));
                return;
            }

            if (ArenaManager.Instance.IsRestrictedFightDataFlag)
            {
                int world = ArenaManager.Instance.RestrictedDataSet.SlotID;
                int tag = ArenaManager.Instance.RestrictedDataSet.CombatID;
                StageDifficult diff = ArenaManager.Instance.RestrictedDataSet.Difficult;
                isRequestArenaData = false;
                CharacterDestroy();

                //var userChatInfo = ArenaManager.Instance.GetFriendFightDataSet();
                //ChatUserData chatUserData = new ChatUserData(userChatInfo.user_no, userChatInfo.name, userChatInfo.icon_name, userChatInfo.level, userChatInfo.portraitData);

                ArenaManager.Instance.ClearRestrictedTeamDataSet();

                WWWForm param = new WWWForm();
                param.AddField("world", world);
                param.AddField("travel_tag", tag);
                param.AddField("result", isWin ? 1 : 0);
                param.AddField("diff", (int)diff);

                NetworkManager.Send("travelinst/result", param, (data) =>
                {
                    LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, UIManager.RefreshUICoroutine(eUIType.Town),
                        SBFunc.CallBackCoroutine(() => { ((TravelLayer)LandMarkPopup.OpenPopup(eLandmarkType.Travel).GetCurLayer()).OnRetrunToRestrictedArea(world); }));
                });
                return;
            }

            //입장전 미리 arena 데이터 요청
            ArenaManager.Instance.ReqArenaData(() =>
            {
                LoadingManager.Instance.EffectiveSceneLoad("ArenaLobby", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => { 
                    CharacterDestroy();
                    //if (ArenaManager.Instance.IsRankUp())
                    //{
                    //    CheckRankUp();
                    //}
                        
                    ArenaManager.Instance.battleInfoTabIdx = (arenaResultData.IsRevenge) ? 2: 1;
                }));
                isRequestArenaData = false;
            }, () =>
            {
                ToastManager.On(100002516);
                isRequestArenaData = false;
            });
        }
        public void onClickToVillage()
        {
            //CharacterDestroy();
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => { 
                CharacterDestroy();
                //if (ArenaManager.Instance.IsRankUp())
                //{
                //    CheckRankUp();
                //}

                ArenaManager.Instance.battleInfoTabIdx = 1;
            }));
        }
        public void onClickStatistics()
        {
            var popup = PopupManager.OpenPopup<ArenaDamageStatisticPopup>(new ArenaResultPopupData(arenaResultData.Stats));
            popup.basicInfoSetting(isWin, (int)arenaResultData.Time);
        }
        
        public void OnClickStatisticInfo()
        {
            //PopupManager.OpenPopup<StatisticInfoPopup>(new StatisticPopupData(true));
            PopupManager.OpenPopup<ArenaStatisticPopup>(new ArenaStatisticPopupData(isWin, (int)arenaResultData.Time));
        }

        //public void CheckRankUp()
        //{
        //    var lastUserGrade = ArenaManager.Instance.UserArenaData.lastUserGrade;
        //    var currentGrade = ArenaManager.Instance.UserArenaData.season_grade;
        //    PopupManager.OpenPopup<ArenaRankChangePopup>(new ArenaRankChangePopupData((int)currentGrade, (int)lastUserGrade)).Init();           
        //}
    }
}