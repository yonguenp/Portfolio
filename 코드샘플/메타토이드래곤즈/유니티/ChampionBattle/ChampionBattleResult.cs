using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using System;

namespace SandboxNetwork {
    public class ChampionBattleResult : MonoBehaviour
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


        ChampionBattleBattleData championBattleResultData = null;
        bool isWin = false;

        bool isRequestArenaData = false;
        void Start()
        {
            championBattleResultData = ChampionManager.Instance.ChampionData;

            UIManager.Instance.InitUI(eUIType.None);
            var charDatas = championBattleResultData.OffenseDic;
            if (charDatas != null)
            {
                int charCount = 0;
                if (championBattleResultData.WinType == eChampionWinType.Open)
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
                foreach (var it in championBattleResultData.OffenseDic)
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

                        //todo : change championData
                        var myDragonData = ChampionManager.Instance.MyInfo.ChampionDragons.GetDragon(tag);
                        if (myDragonData != null)
                        {
                            spine.SetData(myDragonData);
                            spine.InitComplete = SpineInitCallback;
                        }

                        var image = dragonListNode[charCount].GetComponent<Image>();
                        if (image != null)
                        {
                            image.color = new Color(0, 0, 0, 0);
                        }

                        dragonClone.transform.localScale = new Vector3(-2, 2, 1);

                        //todo : change championData
                        //int dragonLv = myDragonData.Level;
                        //dragonLvTexts[charCount].text = string.Format("Lv. {0}", dragonLv);
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
                    //string pointString = championBattleResultData.RewardTrophy.ToString("+#;-#;0");
                    //arenaTrophyLabel.text = pointString;

                    //arenaTrophyLabel.gameObject.SetActive(championBattleResultData.RewardTrophy != 0);
                }

                if (arenaTrophyIcon != null)
                {
                    //arenaTrophyIcon.SetActive(championBattleResultData.RewardTrophy != 0);
                }

                if (arenaPointLabel != null)
                {
                    //string pointString = championBattleResultData.RewardPoint.ToString("+#;-#;0");
                    //arenaPointLabel.text = pointString;

                    //arenaPointLabel.gameObject.SetActive(championBattleResultData.RewardPoint != 0);
                }

                if(arenaPointIcon != null)
                {
                    //arenaPointIcon.SetActive(championBattleResultData.RewardPoint != 0);
                }

                if (accountPointLabel != null)
                {
                    //string pointString = championBattleResultData.RewardAccountExp.ToString("+#;-#;0");
                    //accountPointLabel.text = pointString;

                    //accountPointLabel.gameObject.SetActive(championBattleResultData.RewardAccountExp != 0);
                }

                if (accountPointIcon != null)
                {
                    //accountPointIcon.SetActive(championBattleResultData.RewardAccountExp != 0);
                }

                SoundManager.Instance.PushBGM(isWin ? "BGM_BATTLE_VICTORY" : "BGM_BATTLE_DEFEAT", true, false);
                battleTimeLabel.text = SBFunc.TimeString(championBattleResultData.Time);
            }
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

            LoadingManager.Instance.EffectiveSceneLoad("ChampionBattleLobby", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => { 
                CharacterDestroy();
                //MatchInfoPopup.OpenPopup();
            }));
        }
        public void onClickToVillage()
        {
            LoadingManager.Instance.EffectiveSceneLoad("Town", eSceneEffectType.CloudAnimation, SBFunc.CallBackCoroutine(() => { 
                CharacterDestroy();
            }));
        }
        public void OnClickStatisticInfo()
        {
            //PopupManager.OpenPopup<ChampionBattleStatisticPopup>(new ChampionBattleStatisticPopupData(isWin, (int)championBattleResultData.Time));
        }
    }
}