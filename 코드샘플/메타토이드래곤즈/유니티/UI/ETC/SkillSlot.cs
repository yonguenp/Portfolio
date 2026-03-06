using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SkillSlot : MonoBehaviour, EventListener<SkillBuffEvent>, IPointerEnterHandler, IPointerExitHandler
    {
        public IBattleCharacterData Data { get; private set; } = null;
        public IBattleData BattleData { get; private set; } = null;

        [SerializeField]
        private Transform skillBody = null;
        [SerializeField]
        private Image iconCharacter = null;
        [SerializeField]
        private GameObject iconSkillMask = null;
        [SerializeField]
        private Image iconSkill = null;
        [SerializeField]
        private Image iconCoolDown = null;
        [SerializeField]
        private Text textCoolDown = null;
        [SerializeField]
        private Image iconDeath = null;
        [SerializeField]
        private Slider hpBar = null;
        [SerializeField]
        private Slider shieldBar = null;
        [SerializeField]
        private Transform buffLayerParent = null;
        [SerializeField]
        private GameObject skillActiveEffect = null;
        [SerializeField]
        private SkillSlotGroup parent = null;

        private float startCoolDown = 0f;
        private float maxStartCoolDown = 0f;
        private float coolDown = 0f;
        private float maxCoolDown = 0f;
        private eBattleSkillType skillType = eBattleSkillType.None;
        private bool pause = false;
        private bool isUpdate = false;
        public bool Pause
        {
            get { return pause; }
            set { pause = value; }
        }


        Sequence sequence = null;

        // 버프 관련
        GameObject buffObject = null;
        Dictionary<string, GameObject> buffListDic = new Dictionary<string, GameObject>();

        private void OnEnable()
        {
            //스킬 슬롯에는 버프 미표시 요청
            //EventManager.AddListener(this);
        }

        private void OnDisable()
        {
            ResetSequence();
            StopCoroutine(nameof(RefreshOnlyHP));

            //스킬 슬롯에는 버프 미표시 요청
            //EventManager.RemoveListener(this);
        }

        public void SetData(IBattleData battleData, IBattleCharacterData data)
        {
            BattleData = battleData;
            Data = data;
        }

        public void InitUI()
        {
            if (Data == null || Data.IsEnemy)
                return;

            if (iconCharacter != null)
            {
                var curData = Data as DragonData;
                if (curData == null)
                    return;

                iconCharacter.sprite = curData.BaseData.GetThumbnail();
            }

            if (buffListDic != null && buffLayerParent != null)
            {
                buffListDic.Clear();
                SBFunc.RemoveAllChildrens(buffLayerParent.transform);
            }

            ResetSequence();
            if (Data.Skill1 == null)  //스킬없는 노멀 캐릭용
            {
                StopCoroutine(nameof(RefreshOnlyHP));
                StartCoroutine(nameof(RefreshOnlyHP));
                iconCoolDown.gameObject.SetActive(false);
                return;
            }
            if (iconSkill != null)
                iconSkill.sprite = Data.Skill1.GetIcon();

            isUpdate = true;
            startCoolDown = Data.Stat.GetSkillCoolDown(Data.Skill1.START_COOL_TIME - (Data.Skill1.START_COOL_TIME * Data.Stat.GetTotalStatusConvert(eStatusType.DEL_START_COOLTIME)));
            maxStartCoolDown = startCoolDown;
            iconCoolDown.gameObject.SetActive(coolDown > 0f);
        }

        void SetSequence()
        {
            var t = skillBody;
            if (t == null)
                t = transform;

            transform.DOKill();
            transform.DOScale(Vector3.one * 1.3f, 0.25f);

            sequence = DOTween.Sequence();
            sequence.Append(t.DOLocalRotate(new Vector3(0f, 90f, 0f), 0.25f));
            sequence.AppendCallback(() =>
            {
                if (skillActiveEffect != null)
                    skillActiveEffect.SetActive(true);
                if (iconSkillMask != null)
                    iconSkillMask.SetActive(true);
                if (hpBar != null)
                    hpBar.gameObject.SetActive(false);
                if (shieldBar != null)
                    shieldBar.gameObject.SetActive(false);
            });
            sequence.Append(t.DOLocalRotate(new Vector3(0f, 180f, 0f), 0.25f));
            sequence.AppendInterval(0.5f);
            sequence.Append(t.DOLocalRotate(new Vector3(0f, 270f, 0f), 0.25f));
            sequence.AppendCallback(() =>
            {
                if (skillActiveEffect != null)
                    skillActiveEffect.SetActive(false);
                if (iconSkillMask != null)
                    iconSkillMask.SetActive(false);
                if (hpBar != null)
                    hpBar.gameObject.SetActive(true);
                if (shieldBar != null)
                    shieldBar.gameObject.SetActive(true);
            });
            sequence.Append(t.DOLocalRotate(new Vector3(0f, 360f, 0f), 0.25f));
            sequence.AppendCallback(() =>
            {
                transform.DOKill();
                transform.DOScale(Vector3.one, 0.25f);

                t.Rotate(Vector3.zero);
                sequence = null;
            });
        }

        void ResetSequence()
        {
            if (sequence != null)
            {
                sequence.Kill();
                sequence = null;
            }

            var t = skillBody;
            if (t == null)
                t = transform;

            transform.DOKill();
            transform.localScale = Vector3.one;
            t.localRotation = Quaternion.Euler(Vector3.zero);

            if (iconSkillMask != null)
                iconSkillMask.SetActive(false);
            if (skillActiveEffect != null)
                skillActiveEffect.SetActive(false);
            if (hpBar != null)
                hpBar.gameObject.SetActive(true);
            if (shieldBar != null)
                shieldBar.gameObject.SetActive(true);
        }

        IEnumerator RefreshOnlyHP()
        {
            if (Data == null || hpBar == null)
                yield break;
            iconDeath.gameObject.SetActive(Data.HP <= 0);
            while (Data.HP > 0)
            {
                if (!Pause)
                {
                    hpBar.value = (float)Data.HP / Data.MaxHP;

                    var shieldPoint = Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                    if (shieldPoint > 0f)
                    {
                        var shieldValue = (float)shieldPoint / Data.MaxHP;
                        if (shieldValue > 1f)
                            shieldValue = 1f;

                        shieldBar.value = shieldValue;
                    }
                    else
                    {
                        shieldBar.value = 0f;
                    }
                }
                yield return null;
            }
            hpBar.value = 0f;
            shieldBar.value = 0f;
            iconDeath.gameObject.SetActive(true);
            yield break;

        }

        private void FixedUpdate()
        {
            if (Data == null || hpBar == null || iconCoolDown == null || textCoolDown == null)
                return;

            if (!isUpdate)
                return;

            iconDeath.gameObject.SetActive(Data.HP <= 0);
            if (Data.HP > 0)
            {
                if (!Pause)
                {
                    hpBar.value = (float)Data.HP / Data.MaxHP;
                    //var maxShield = Data.Stat.GetStatusInt(eStatusCategory.BASE, eStatusType.SHIELD_POINT);
                    var shield = Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                    if (shield > 0f)
                    {
                        var shieldValue = (float)shield / Data.MaxHP;
                        if (shieldValue > 1f)
                            shieldValue = 1f;

                        shieldBar.value = shieldValue;
                    }
                    else
                    {
                        shieldBar.value = 0f;
                    }

                    if (skillType != eBattleSkillType.Skill1 && Data.ActiveSkillType == eBattleSkillType.Skill1)
                    {
                        coolDown = Data.Stat.GetSkillCoolDown(Data.Skill1.COOL_TIME);
                        maxCoolDown = coolDown;
                        SetSequence();
                    }
                    CheckSelectSkill();

                    skillType = Data.ActiveSkillType;

                    coolDown -= SBGameManager.Instance.DTime;
                    startCoolDown -= SBGameManager.Instance.DTime;
                    if (startCoolDown > 0f)
                    {
                        if (Data.Skill1Delay > 0f)
                            startCoolDown = Data.Skill1Delay;
                        if (startCoolDown < 0f)
                            startCoolDown = 0f;
                        iconCoolDown.gameObject.SetActive(startCoolDown > 0f && sequence == null);
                        iconCoolDown.fillAmount = startCoolDown / maxStartCoolDown;
                        textCoolDown.text = Mathf.CeilToInt(startCoolDown).ToString("D");
                    }
                    else
                    {
                        if (Data.Skill1Delay > 0)
                            coolDown = Data.Skill1Delay;
                        if (coolDown < 0f)
                            coolDown = 0f;
                        iconCoolDown.gameObject.SetActive(coolDown > 0f && sequence == null);
                        iconCoolDown.fillAmount = coolDown / maxCoolDown;
                        textCoolDown.text = Mathf.CeilToInt(coolDown).ToString("D");
                    }
                }

                return;
            }

            transform.DOKill();
            transform.DOScale(Vector3.one, 0.25f);
            hpBar.value = 0f;
            shieldBar.value = 0f;
            iconDeath.gameObject.SetActive(true);
            iconCoolDown.gameObject.SetActive(false);

            if (buffListDic != null && buffListDic.Count > 0)
            {
                foreach (var buff in buffListDic)
                {
                    if (buff.Value == null)
                        continue;

                    Destroy(buff.Value);
                }
                buffListDic.Clear();
            }

            isUpdate = false;
        }

        public void OnEvent(SkillBuffEvent eventType)
        {
            if (Data == null)
                return;

            if (buffObject == null)
                buffObject = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "buffSlot");

            SBFunc.OnEventBuff(eventType, Data.ID, Data.Position, buffLayerParent, buffListDic, buffObject);
        }

        private void CheckSelectSkill()
        {
            if (sequence != null || BattleData == null)
                return;

            if (BattleData.SelectSkillCharacter != null && BattleData.SelectSkillCharacter == Data)
            {
                transform.DOKill();
                transform.DOScale(Vector3.one * 1.1f, 0.25f);
                return;
            }
            var skill = BattleData.GetSkill(Data, eBattleSide.OffenseSide_1);
            if (skill != null)
            {
                transform.DOKill();
                transform.DOScale(Vector3.one * 1.1f, 0.25f);
                return;
            }

            transform.DOKill();
            transform.DOScale(Vector3.one, 0.25f);
        }

        public void OnClickSkillIcon()
        {
            if (parent.IsSkillToolTipOn(gameObject))
            {
                ClearTooltip();
                return;
            }

            if (BattleData == null)
                return;

            if (!BattleData.IsAuto)
            {
                if (Data == null || Data.Skill1 == null || startCoolDown > 0f || coolDown > 0f)
                    return;

                if (BattleData.SelectSkillCharacter != Data)
                    BattleData.SetSelectSkillCharacter(Data);
                else
                {
                    BattleData.SkillQueueClear(eBattleSide.OffenseSide_1);
                    BattleData.SetSelectSkillCharacter(Data);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (BattleData.IsAuto || startCoolDown > 0f || coolDown > 0f)
            {
                OnTooltip();
                return;
            }

            Invoke("OnTooltip", 0.5f);
        }

        public void OnTooltip()
        {
            CancelInvoke("OnTooltip");
            parent.OnSkillInfo(Data.Skill1, Data.SkillLevel, gameObject);
        }

        public void OnPointerExit(PointerEventData eventDate)
        {
            ClearTooltip();
        }

        public void ClearTooltip()
        {
            CancelInvoke("OnTooltip");
            parent.ClearSkillInfo(gameObject);
        }
    }
}