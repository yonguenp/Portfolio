using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElemBuffInfoPopup : Popup<PopupData>
{
    public enum ElemBuffType { 
        NONE = -1,
        ATK,
        DEF,
        HP
    }
    [Serializable]
    public class BuffItemUI
    {
        [SerializeField] Image Icon = null;
        [SerializeField] Text Name = null;
        [SerializeField] Text Desc = null;
        [SerializeField] ElemBuffType type = ElemBuffType.NONE;
        public void Set(ArenaSeasonData target)
        {
            eElementType elem = eElementType.None;
            switch(type)
            {
                case ElemBuffType.ATK:
                    elem = target.ATK_ELEM;
                    Desc.text = StringData.GetStringByStrKey("시즌속성공격력문구");
                    break;
                case ElemBuffType.DEF:
                    elem = target.DEF_ELEM;
                    Desc.text = StringData.GetStringByStrKey("시즌속성방어력문구");
                    break;
                case ElemBuffType.HP:
                    elem = target.HP_ELEM;
                    Desc.text = StringData.GetStringByStrKey("시즌속성체력문구");
                    break;
            }
            string elemString = StringData.GetStringByStrKey(string.Format("dragon_property_0{0}", (int)elem));
            Name.text = StringData.GetStringFormatByStrKey("시즌속성",elemString);
            Icon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("icon_property_{0}_160", SBDefine.ConvertToElementString((int)elem)));            
        }

        public void Set(eElementType elem)
        {
            switch (type)
            {
                case ElemBuffType.ATK:
                    Desc.text = StringData.GetStringByStrKey("시즌속성공격력문구");
                    break;
                case ElemBuffType.DEF:
                    Desc.text = StringData.GetStringByStrKey("시즌속성방어력문구");
                    break;
                case ElemBuffType.HP:
                    Desc.text = StringData.GetStringByStrKey("시즌속성체력문구");
                    break;
            }
            string elemString = StringData.GetStringByStrKey(string.Format("dragon_property_0{0}", (int)elem));
            Name.text = StringData.GetStringFormatByStrKey("시즌속성", elemString);
            Icon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, string.Format("icon_property_{0}_160", SBDefine.ConvertToElementString((int)elem)));
        }
    }

    [SerializeField] Text Title = null;

    [SerializeField] BuffItemUI ATK = null;
    [SerializeField] BuffItemUI DEF = null;
    [SerializeField] BuffItemUI HP = null;
    public override void InitUI()
    {
        switch (ArenaManager.Instance.UserArenaData.season_type)
        {
            case ArenaBaseData.SeasonType.PreSeason:
                Title.text = StringData.GetStringByStrKey("다음시즌속성버프");
                break;
            case ArenaBaseData.SeasonType.RegularSeason:
                Title.text = StringData.GetStringByStrKey("시즌속성버프");
                break;
            default:
#if DEBUG
                Debug.LogError("여기 들어오면 안됨");
#endif
                break;
        }

        ATK.Set(ArenaManager.Instance.UserArenaData.ATK_ELEM);
        DEF.Set(ArenaManager.Instance.UserArenaData.DEF_ELEM);
        HP.Set(ArenaManager.Instance.UserArenaData.HP_ELEM);
    }
}
