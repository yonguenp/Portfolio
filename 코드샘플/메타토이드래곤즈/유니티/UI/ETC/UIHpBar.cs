using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIHpBar : MonoBehaviour, EventListener<SkillBuffEvent>
    {
        private IBattleCharacterData data;
        public IBattleCharacterData Data
        {
            get { return data; }
            private set { data = value; }
        }
        [SerializeField]
        private Slider hpBar = null;
        [SerializeField]
        private Slider shieldBar = null;
        [SerializeField]
        private CanvasGroup group = null;
        [SerializeField]
        private Transform buffParent = null;
        [SerializeField]
        private Text hpText = null;


        private GameObject buffObject = null;
        private Dictionary<string, GameObject> buffListDic = new Dictionary<string, GameObject>();
        private Transform effectTr = null;
        private BattleStage battleStage = null;
        float offset = 0.0f;
        
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

        public void SetData(BattleStage stage, IBattleCharacterData data)
        {
            battleStage = stage;

            if (data == null)
                return;
#if UNITY_EDITOR
            var show = PlayerPrefs.GetInt("DmgShowOn",1);
            if(show == 1)
                hpText.gameObject.SetActive(true);
            else
                hpText.gameObject.SetActive(false);
#else
            hpText.gameObject.SetActive(false);
#endif
            Data = data;
            var e = Data.Transform.GetComponent<statusEffect>();
            if (e != null)
            {
                effectTr = e.EffectTr;
                //var spine = Data.GetSpine();
                //if (spine != null)
                //{
                //    var follow = effectTr.GetComponent<FollowPosition>();
                //    if (follow == null)
                //        follow = effectTr.gameObject.AddComponent<FollowPosition>();

                //    follow.Set(spine.SpineTransform, spine.transform, effectTr.localPosition, Data.IsLeft, false);
                //}
            }

            if (effectTr != null)
            {
                offset = 4f;
            }
            else
            {
                offset = 8f;
            }

            CharBaseData charData = Data.BaseData as CharBaseData;
            if (charData != null)
            {
                offset += (charData.OFFSET) * 72;
            }

            StopCoroutine("Refresh");
            StartCoroutine("Refresh");
        }


        IEnumerator Refresh()
        {
            if (hpBar == null || Data == null)
                yield break;

            while(Data.HP > 0f)
            {
                if(Data.Transform == null)
                    break;
                else
                {
                    if (effectTr != null)
                        transform.localPosition = new Vector2(effectTr.position.x * 72, effectTr.position.y * 72 + offset);
                    else
                        transform.localPosition = new Vector2(Data.Transform.position.x * 72, Data.Transform.position.y * 72 - offset);

                    var offHP = Data.HP;
                    hpBar.value = offHP > 0 ? (float)offHP / Data.MaxHP : 0f;
                    if(shieldBar != null)
                    {
                        //var maxShield = Data.Stat.GetStatusInt(eStatusCategory.BASE, eStatusType.SHIELD_POINT);
                        var shield = Data.Stat.GetStatusInt(eStatusCategory.ADD_BUFF, eStatusType.SHIELD_POINT);
                        if (shield > 0f)
                        {
                            var shieldValue = (float)shield / Data.MaxHP;
                            if (shieldValue > 1f)
                                shieldValue = 1f;
                            hpText.text = string.Format("{0}<color=#6df6ea>({1})</color>", offHP,shield);
                            shieldBar.value = shieldValue;
                        }
                        else
                        {
                            shieldBar.value = 0f;
                            hpText.text = offHP.ToString();
                        }
                    }
                    
                    
                    group.alpha = 1f;
                }
                
                yield return null;
            }
            hpText.text = "0";

            if (buffListDic != null && buffParent != null)
            {
                buffListDic.Clear();
                SBFunc.RemoveAllChildrens(buffParent.transform);
            }
            DestroyObject();
            yield break;
        }

        public void DestroyObject()
        {
            Destroy(gameObject);

            if (battleStage != null)
            {
                battleStage.RemoveHpBar(Data);
            }
        }

        public void OnEvent(SkillBuffEvent eventType)
        {
            if (Data == null)
                return;

            if (buffObject == null)
                buffObject = ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "buffSlot");

            SBFunc.OnEventBuff(eventType, Data.ID, Data.Position, buffParent, buffListDic, buffObject);
        }
    }
}