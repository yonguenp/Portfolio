using DG.Tweening;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossHpUI : BossHpUIObject, EventListener<SkillBuffEvent>
    {
        [SerializeField]
        private Text totalScore = null;
        [SerializeField]
        private Slider advBossShieldbar = null;
        [SerializeField]
        private Transform buffParent = null;
        [SerializeField]
        private GameObject buffObject = null;
        private Dictionary<string, GameObject> buffListDic = new Dictionary<string, GameObject>();
        [SerializeField]
        SkillSlotGroup[] DragonSkillUI = new SkillSlotGroup[4];
        [SerializeField]
        GameObject bossLevelup = null;

        private int lastLevel = -1;
        private void OnEnable()
        {
            //HPBar 버프 표시 요청
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            //HPBar 버프 표시 요청
            EventManager.RemoveListener(this);
        }

        public void InitWorldBoss()
        {
            Clear();

            List<IBattleCharacterData>[] list = new List<IBattleCharacterData>[4];
            for(int i = 0; i < DragonSkillUI.Length; i++)
            {
                list[i] = new List<IBattleCharacterData>();
            }
            foreach (var it in WorldBossManager.Instance.Data.OffenseDic)
            {
                var bTag = it.Key;
                var elem = it.Value;
                if (elem == null)
                    continue;

                var tag = it.Value.ID;
                if (it.Value.BaseData is CharBaseData data)
                {
                    var pt = bTag - 1;
                    list[pt / WorldBossFormationData.MAX_DRAGON_COUNT].Add(it.Value);
                }
            }

            for (int i = 0; i < DragonSkillUI.Length; i++)
            {
                DragonSkillUI[i].gameObject.SetActive(true);
                DragonSkillUI[i].SetSlot(WorldBossManager.Instance.Data, list[i]);
            }
        }


        public override void Clear()
        {
            base.Clear();

            foreach(Transform child in buffParent)
            {
                Destroy(child.gameObject);
            }

            advBossShieldbar.value = 1.0f;
            advBossHpbar.value = 1.0f;
            advBossShieldbar.gameObject.SetActive(false);
            lastShieldBoss = 0.0f;
            nameText.text = "";

            foreach(var ui in DragonSkillUI)
            {
                ui.gameObject.SetActive(false);
            }

            if (bossLevelup != null)
                bossLevelup.SetActive(false);
        }

        public override void RefreshBossHpBar()
        {
            if (advBossHpbar == null || boss == null || !advBossHpbar.IsActive())
                return;

            if(lastLevel != boss.Level)
            {
                SetTargetBoss(boss);
            }

            var shieldPoint = boss.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
            float shieldValue = 0.0f;
            if (shieldPoint > 0f)
            {
                shieldValue = (float)shieldPoint / boss.MaxShield;
                if (shieldValue > 1f)
                    shieldValue = 1f;
            }

            advBossShieldbar.value = shieldValue;

            if (boss.HP > 0)
                advBossHpbar.value = Mathf.Max(0.0f, (float)boss.HP / boss.MaxHP);
            else
                advBossHpbar.value = 1.0f;

            DeffBossHpBarChange(shieldPoint);
        }
        public override void SetTargetBoss(IBattleCharacterData bossData)
        {
            boss = bossData;
            if (boss == null)
                return;

            gameObject.SetActive(true);
            bossSequence = DOTween.Sequence();

            nameText.text = "Lv." + boss.Level;
            lastLevel = boss.Level;

            nameText.DOKill();
            nameText.transform.DOKill();

            nameText.color = Color.yellow;
            nameText.DOColor(Color.white, 1.0f);

            nameText.transform.localScale = Vector3.one * 2.0f;
            nameText.transform.DOScale(1.0f, 1.0f);

            if (boss.Level > 1 && bossLevelup != null && !bossLevelup.activeSelf)
            {
                bossLevelup.SetActive(true);
                Invoke("HideLevelUI", 1.0f);
            }
        }

        void HideLevelUI()
        {
            if (bossLevelup != null)
            {
                bossLevelup.SetActive(false);
            }
        }

        public override void DeffBossHpBarChange(int shieldPoint)
        {
            if (boss == null)
                return;

            bossSequence.Kill();
            advLostBossHp.sizeDelta += new Vector2(1080 * Mathf.Min(lastHpBoss - advBossHpbar.value, 1.0f), 0);

            bossSequence.Append(advLostBossHp.DOSizeDelta(Vector2.zero, 1.0f));

            if(totalScore != null)
            {
                totalScore.text = SBFunc.CommaFromNumber(((BattleWorldBossData)boss).SCORE);
            }

            if (hpText != null)
            {
                string curText = SBFunc.CommaFromNumber(((BattleWorldBossData)boss).HP) + "/" + SBFunc.CommaFromNumber(((BattleWorldBossData)boss).MaxHP);

                if (shieldPoint > 0)
                {
                    curText = curText + "(" + shieldPoint.ToString() + "/" + boss.MaxShield + ")";
                }

                hpText.text = curText;
            }

            advBossShieldbar.gameObject.SetActive(shieldPoint > 0);
            if (shieldPoint > 0)
            {
                (advBossShieldbar.image.transform as RectTransform).sizeDelta += new Vector2(1080 * Mathf.Min(lastShieldBoss - advBossShieldbar.value, 1.0f), 0);
                bossSequence.Append((advBossShieldbar.image.transform as RectTransform).DOSizeDelta(Vector2.zero, 1.0f));
            }


            if (lastHpBoss == advBossHpbar.value && lastShieldBoss == advBossShieldbar.value)
                return;

            lastShieldBoss = advBossShieldbar.value;
            lastHpBoss = advBossHpbar.value;

            if (lastHpBoss == advBossHpbar.maxValue && lastShieldBoss == advBossShieldbar.minValue)
                return;

            if (advBossHpbar.value <= 0.0f)
            {
                advBossHpbar.value = 0f;

                if (profileBackground != null)
                {
                    profileBackground.color = bgColor;
                }

                if (profileImage != null)
                {
                    profileImage.transform.localPosition = Vector3.zero;
                }
            }
            else
            {

                if (profileBackground != null)
                {
                    profileBackground.DOKill();
                    profileBackground.DOColor(new Color(0.8392157f, 0.3882353f, 0.3882353f), 0.1f).OnComplete(() => {
                        profileBackground.DOColor(bgColor, 0.5f);
                    });
                }

                if (profileImage != null)
                {
                    profileImage.transform.DOKill();
                    profileImage.transform.localPosition = Vector3.zero;
                    profileImage.transform.DOShakePosition(1.0f);
                }
            }
        }
        public void OnEvent(SkillBuffEvent eventType)
        {
            if (boss == null)
                return;

            SBFunc.OnEventBuff(eventType, boss.ID, boss.Position, buffParent, buffListDic, buffObject);
        }
    }
}