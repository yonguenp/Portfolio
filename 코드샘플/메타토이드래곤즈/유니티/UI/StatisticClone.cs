using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork {
    public struct UIStatisticEvent
    {

        public UIStatisticEvent(int damage, int casterID,int targetID, bool isSkill, bool isCasterEnemy =false)
        {
            Damage = damage;
            CasterID = casterID;
            TargetID = targetID;
            IsSkill = isSkill;
            IsCasterEnemy = isCasterEnemy;
            EventType = eUIStatisticsEventType.DamageRecord;
            LifeTime = -1;
            recieveDmg = 0;
            isShieldDmg = false;
        }
        public UIStatisticEvent (int casterID, bool isCasterEnemy,bool isSkill)
        {
            Damage = 0;
            CasterID = casterID;
            TargetID = 0;
            IsSkill = isSkill;
            IsCasterEnemy = isCasterEnemy;
            EventType = eUIStatisticsEventType.SkillCount;
            LifeTime = -1;
            recieveDmg = 0;
            isShieldDmg = false;
        }
        public UIStatisticEvent(int targetID, bool isTargetEnemy, int dmgAmount, bool isShieldedDmg)
        {
            Damage = 0;
            CasterID = targetID;
            TargetID = 0;
            IsSkill = false;
            IsCasterEnemy = isTargetEnemy;
            EventType = eUIStatisticsEventType.RecieveRecord;
            LifeTime = -1;
            recieveDmg = dmgAmount;
            isShieldDmg = isShieldedDmg;

        }
        public UIStatisticEvent (int casterID, bool isCasterEnemy, int deathTime)
        {
            Damage = 0;
            CasterID = casterID;
            TargetID = 0;
            IsSkill = false;
            IsCasterEnemy = isCasterEnemy;
            EventType = eUIStatisticsEventType.DeathRecord;
            LifeTime = deathTime;
            recieveDmg = 0;
            isShieldDmg = false;
        }
        
        public int Damage { get; private set; }
        public int CasterID { get; private set; }

        public int TargetID { get; private set; }

        public bool IsSkill { get; private set; }
        public bool IsCasterEnemy { get; private set; }
        public eUIStatisticsEventType EventType { get; private set; }

        public int LifeTime { get; private set; }
        public int recieveDmg { get; private set; }

        public bool isShieldDmg { get; private set; }

        public static void Send(int casterID, bool isCasterEnemy, int deathTime)
        {
            EventManager.TriggerEvent(new UIStatisticEvent(casterID, isCasterEnemy, deathTime));
        }

        public static void Send(int casterID, bool IsCasterEnemy, int dmgAmount , bool isShieldedDmg)
        {
            EventManager.TriggerEvent(new UIStatisticEvent(casterID, IsCasterEnemy, dmgAmount, isShieldedDmg));
        }
        public static void Send(int casterID, bool isCasterEnemy, bool isSkill) // 스킬 사용횟수 카운팅만을 위한것
        {
            EventManager.TriggerEvent(new UIStatisticEvent(casterID,isCasterEnemy, isSkill));
        }
        public static void Send(int damage, int casterID,int targetID,bool isSkill ,bool isCasterEnemy = false)
        {
            EventManager.TriggerEvent(new UIStatisticEvent(damage, casterID,targetID, isSkill, isCasterEnemy));
#if UNITY_EDITOR
            if (PlayerPrefs.HasKey("DmgLogOn")==false || PlayerPrefs.GetInt("DmgLogOn") == 0)
                return;
            var targetDat = CharBaseData.Get(targetID);
            var casterDat = CharBaseData.Get(casterID);
            string casterName = "";
            string targetName = "";
            if (casterDat == null)
            {
                casterName = "몬스터";
            }
            else
            {
                casterName = StringData.GetStringByStrKey(CharBaseData.Get(casterID)._NAME);
            }
            if (targetDat == null )
            {
                targetName = "몬스터";
            }
            else
            {
                targetName = StringData.GetStringByStrKey(CharBaseData.Get(targetID)._NAME);
            }
             
            string firstString = isCasterEnemy ? "적" : "내";
            string secondString = isCasterEnemy ? "내" : "적";
            string typeString =  isSkill ? "스킬" : "평타";
            Debug.Log(string.Format("시전자 - {0} {1} / 대상 - {2} {3} / 타입 - {4} / 데미지 - {5}", firstString, casterName, secondString, targetName, typeString,damage));
#endif
        }
    }
    public class StatisticClone : MonoBehaviour
    {
        [SerializeField]
        private GameObject defaultLayer;
        [SerializeField]
        private GameObject detailLayer;
        [SerializeField]
        private GameObject lifeInfoLayer;

        [Header("간단한 정보")]

        [SerializeField]
        private Text damageText;

        [SerializeField]
        private Text DpsText;



        [Header("데미지 정보")]

        [SerializeField]
        private Text normalDmgText;
        [SerializeField]
        private Text skillDmgText;
        [SerializeField]
        private Image portraitImg;
        [SerializeField]
        private Image portraitBG;
        [SerializeField]
        private Text normalAtkCountText;
        [SerializeField]
        private Text skillUseCountText;

        [Header("생존 정보")]
        [SerializeField]
        private Text recieveDmgText;
        [SerializeField]
        private Text shieldDmgText;
        [SerializeField]
        private Text lifeTimeText;
        int Damage = 0;
        int normalDamage = 0;
        int skillDamage = 0;
        int normalAtkCount = 0;
        int skillUseCount = 0;

        int recievedDmg = 0;
        int shieldedDmg = 0;
        int lifeTime = -1;

        eUIStatisticsContentType contentType;

        public void SetContentType(eUIStatisticsContentType type)
        {
            contentType = type;
        }

        public void SetImage(int dragonNo, bool isDead =false)
        {
            gameObject.SetActive(true);
            portraitImg.color = Color.white;
            var dragonDat = CharBaseData.Get(dragonNo);
            var thumbnail = dragonDat.THUMBNAIL;
            portraitImg.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.CharIconPath, thumbnail);
            detailLayer.SetActive(false);
            defaultLayer.SetActive(true);
            lifeInfoLayer.SetActive(false);


            var resourceString = MakeStringByGradeAndElement(dragonDat.GRADE);
            var icon = ResourceManager.GetResource<Sprite>(eResourcePath.DragonGradeTagIconPath, resourceString);
            if (icon != null)
                portraitBG.sprite = icon;
        }

        public void SetDetailLayer()
        {
            detailLayer.SetActive(true);
            defaultLayer.SetActive(false);
            if(lifeInfoLayer != null) 
                lifeInfoLayer.SetActive(false);
        }
        public void SetDefaultLayer()
        {
            detailLayer.SetActive(false);
            defaultLayer.SetActive(true);
            if (lifeInfoLayer != null)
                lifeInfoLayer.SetActive(false);
        }
        public void SetLifeInfoLayer()
        {
            detailLayer.SetActive(false);
            defaultLayer.SetActive(false);
            if (lifeInfoLayer != null)
                lifeInfoLayer.SetActive(true);
        }
        public void ClearDamage()
        {
            Damage = 0;
            
            normalDamage = 0;
            skillDamage = 0;
            normalAtkCount = 0;
            skillUseCount = 0;

            shieldedDmg = 0;
            recievedDmg = 0;
            lifeTime = -1;

            damageText.text = "0";
            DpsText.text = "0";
            normalDmgText.text = "0";
            skillDmgText.text = "0";
            skillUseCountText.text = "0";
            normalAtkCountText.text = "0";
            if(recieveDmgText !=null)
                recieveDmgText.text = "0";
            if(shieldDmgText !=null)
                shieldDmgText.text = "0";
            if(lifeTimeText  !=null)
                lifeTimeText.text = "-";

        }
        public void AddDamage(int damangeAmount, bool isSkill)
        {
            Damage += damangeAmount;
            if (isSkill) { 
                skillDamage += damangeAmount;
            }
            else
            {
                
                normalDamage += damangeAmount;
            }
            int dps = 0;
            float targetTimer = 0f;
            switch (contentType)
            {
                case eUIStatisticsContentType.Arena:
                    targetTimer = ArenaManager.Instance.ColosseumData.Time;
                    break;
                case eUIStatisticsContentType.Adventure:
                    targetTimer = AdventureManager.Instance.Data.Time;
                    break;
                case eUIStatisticsContentType.DailyDungeon:
                    targetTimer = DailyManager.Instance.Data.Time;
                    break;
                case eUIStatisticsContentType.WorldBoss:
                    targetTimer = WorldBossManager.Instance.Data.Time;
                    break;
                case eUIStatisticsContentType.ChampionBattle:
                    targetTimer = ChampionManager.Instance.ChampionData.Time;
                    break;
            }
            dps = Mathf.RoundToInt(Damage / targetTimer);
            DpsText.text = dps.ToString();
            damageText.text = Damage.ToString();

            normalDmgText.text =normalDamage.ToString();
            skillDmgText.text = skillDamage.ToString();
            
            
        }
        public void AddCount(bool isSkill)
        {
            if (isSkill)
            {
                skillUseCount += 1;
                skillUseCountText.text = (skillUseCount).ToString();
            }
            else
            {
                normalAtkCount += 1;
                normalAtkCountText.text = normalAtkCount.ToString();
            }
        }

        public void AddRecieveDmg(int dmgAmount, bool isShield)
        {
            if (isShield)
            {
                shieldedDmg += dmgAmount;
                shieldDmgText.text = shieldedDmg.ToString();
            }
            else
            {
                recievedDmg += dmgAmount;
                recieveDmgText.text = recievedDmg.ToString();
            }
        }

        public void SetDeath(int aliveTime)
        {
            portraitImg.color = Color.gray;
            lifeTime = aliveTime;
            lifeTimeText.text = SBFunc.TimeStringMinute(aliveTime);
        }
        string MakeStringByGradeAndElement(int grade)
        {
            var gradeString = GetGradeConvertString(grade);
            return SBFunc.StrBuilder("bggrade_", gradeString);
        }
        string GetGradeConvertString(int grade)
        {
            var gradeNameStrIndex = CharGradeData.Get(grade.ToString())._NAME;
            var gradeString = StringData.GetStringByIndex(gradeNameStrIndex).ToLower();
            return gradeString;
        }
    }
}
