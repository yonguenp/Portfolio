using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ArenaTimeBuff
{
    public static ArenaTimeBuff e;

    public static void Send()
    {
        EventManager.TriggerEvent(e);
    }
}

public class ElemBuffInfoUI : MonoBehaviour, EventListener<ArenaTimeBuff>
{
    public enum ElemBuffType
    {
        NONE = -1,
        ATK,
        DEF,
        HP
    }
    [Serializable]
    public class BuffItemUI
    {
        [SerializeField] Image Icon = null;
        [SerializeField] Image ActivateBack = null;
        [SerializeField] ElemBuffType type = ElemBuffType.NONE;

        public void Set(ArenaSeasonData target)
        {
            eElementType elem = eElementType.None;
            switch (type)
            {
                case ElemBuffType.ATK:
                    elem = target.ATK_ELEM;
                    break;
                case ElemBuffType.DEF:
                    elem = target.DEF_ELEM;
                    break;
                case ElemBuffType.HP:
                    elem = target.HP_ELEM;
                    break;
            }

            Icon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("icon_property_{0}_160", SBDefine.ConvertToElementString((int)elem)));
            SetVisible(true);
        }

        public void Set(eElementType elem)
        {
            Icon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("icon_property_{0}_160", SBDefine.ConvertToElementString((int)elem)));
            SetVisible(true);
        }

        public void SetVisible(bool value)
        {
            if (Icon == null)
                return;
            if(ActivateBack != null)
                ActivateBack.gameObject.SetActive(false);
            Icon.transform.parent.gameObject.SetActive(value);
        }

        public void SetActivateEffect(bool value, eElementType type) // 타입별 이미지가 다를 수도 있을까봐 혹시나 받아옴
        {
            if(ActivateBack != null)
                ActivateBack.gameObject.SetActive(value);
        }
    }
    [Serializable]
    public class BuffArenaUI
    {
        [SerializeField] Image Icon = null;
        [SerializeField] Text textCount = null;
        int buffCount = 0;

        public void Init()
        {
            SetVisible(false);
            buffCount = 0;
            if (textCount != null)
            {
                textCount.gameObject.SetActive(false);
                textCount.text = "";
            }
        }
        public void Set()
        {
            SetVisible(true);
            buffCount++;
            if(textCount != null)
            {
                textCount.gameObject.SetActive(buffCount > 0);
                textCount.text = buffCount.ToString();
            }
        }
        private void SetVisible(bool value)
        {
            if (Icon == null)
                return;

            Icon.gameObject.SetActive(value);
        }
    }

    
    [SerializeField] BuffItemUI ATK = null;
    [SerializeField] BuffItemUI DEF = null;
    [SerializeField] BuffItemUI HP = null;
    [SerializeField] BuffArenaUI TIME_BUFF = null;

    public void Init()
    {
        UIObject parent = GetComponentInParent<UIObject>();
        if (parent != null)
        {
            if (parent.IsCurSceneType(eUIType.Battle_ChampionBattle))
            {
                ATK?.SetVisible(false);
                DEF?.SetVisible(false);
                HP?.SetVisible(false);

                if (TIME_BUFF != null)
                    TIME_BUFF.Init();

                EventManager.AddListener<ArenaTimeBuff>(this);

                return;
            }
        }

        if (ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.RegularSeason)
        {
            ATK?.Set(ArenaManager.Instance.UserArenaData.ATK_ELEM);
            DEF?.Set(ArenaManager.Instance.UserArenaData.DEF_ELEM);
            HP?.Set(ArenaManager.Instance.UserArenaData.HP_ELEM);
        }
        else
        {
            ATK?.SetVisible(false);
            DEF?.SetVisible(false);
            HP?.SetVisible(false);
        }

        if (TIME_BUFF != null)
            TIME_BUFF.Init();
    }
    private void OnEnable()
    {
        Init();

        EventManager.AddListener<ArenaTimeBuff>(this);
    }
    public void SetEffect(params eElementType[] eElements)
    {
        if (ArenaManager.Instance.UserArenaData.season_type == ArenaBaseData.SeasonType.RegularSeason) {
            ATK?.SetActivateEffect(false, ArenaManager.Instance.UserArenaData.ATK_ELEM);
            DEF?.SetActivateEffect(false, ArenaManager.Instance.UserArenaData.DEF_ELEM);
            HP?.SetActivateEffect(false, ArenaManager.Instance.UserArenaData.HP_ELEM);
            foreach (var eElement in eElements)
            {
                if (ArenaManager.Instance.UserArenaData.ATK_ELEM == eElement)
                {
                    ATK?.SetActivateEffect(true, ArenaManager.Instance.UserArenaData.ATK_ELEM);
                }
                if (ArenaManager.Instance.UserArenaData.DEF_ELEM == eElement)
                {
                    DEF?.SetActivateEffect(true, ArenaManager.Instance.UserArenaData.DEF_ELEM);
                }
                if (ArenaManager.Instance.UserArenaData.HP_ELEM == eElement)
                {
                    HP?.SetActivateEffect(true, ArenaManager.Instance.UserArenaData.HP_ELEM);
                }
            }
        }
    }
    private void OnDisable()
    {
        EventManager.RemoveListener<ArenaTimeBuff>(this);
    }

    public void OnClick()
    {
        UIObject parent = GetComponentInParent<UIObject>();
        if (parent != null)
        {
            if (parent.IsCurSceneType(eUIType.Battle_ChampionBattle))
            {
                return;
            }
        }

        PopupManager.OpenPopup<ElemBuffInfoPopup>();
    }

    public void OnEvent(ArenaTimeBuff eventType)
    {
        TIME_BUFF.Set();
    }

}
