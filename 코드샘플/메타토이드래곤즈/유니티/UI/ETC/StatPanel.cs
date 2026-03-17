using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    [System.Serializable]
    public class Stat
    {
        [SerializeField]
        public eStatusType Type = eStatusType.NONE;
        [SerializeField]
        public Image Icon = null;
        [SerializeField]
        public Text Label = null;
        [SerializeField]
        public Text AddedLabel = null;
    }
    public class StatPanel : MonoBehaviour
    {
        [SerializeField]
        protected CanvasGroup group = null;
        [SerializeField]
        protected Text battleLabel = null;
        [SerializeField]
        protected List<Stat> statObj = null;
        public int PrevINF { get; set; } = 0;
        public int INF { get; protected set; } = 0;
        public CharacterStatus PrevStat { get; protected set; } = null;
        public CharacterStatus NextStat { get; protected set; } = null;

        /// <summary> 초기화 및 시작 </summary>
        /// <param name="prevStat">변경 전 스텟</param>
        /// <param name="nextStat">변경 후 스텟</param>
        public void Initialize(int inf, CharacterStatus prevStat, CharacterStatus nextStat)
        {
            INF = inf;
            PrevStat = prevStat;
            NextStat = nextStat;

            Refresh();
        }
        /// <summary> 갱신 방법 or 그 외 로직 자체가 변경되야할 경우 하위구현으로 변경 </summary>
        protected virtual void Refresh()
        {
            RefreshETC();
            RefreshStat();
        }
        /// <summary> 추가적으로 전투력 이외에 적용되야 할 사항이 있으면 하위구현으로 추가 </summary>
        protected virtual void RefreshETC()
        {
            /** 전투력 표기 구간 */
            if (battleLabel != null)
            {
                battleLabel.text = Mathf.FloorToInt(PrevINF).ToString();
                battleLabel.DOCounter(PrevINF, INF, 1f).SetEase(Ease.InOutQuad);
            }
            //
        }
        /// <summary> 만들어 둔 Stat Obj를 통해 스텟 표시 </summary>
        protected virtual void RefreshStat()
        {
            /** 표시 스텟 결정 및 사용 구간 */
            if (statObj == null)
                return;

            for (int i = 0, count = statObj.Count; i < count; ++i)
            {
                RefreshStat(statObj[i]);
            }
            //
        }
        protected virtual void RefreshStat(Stat stat)
        {
            if (stat == null)
                return;

            var prevStat = PrevStat.GetTotalStatus(stat.Type);
            var nextStat = NextStat.GetTotalStatus(stat.Type);
            switch (stat.Type)
            {
                case eStatusType.FIRE_DMG: case eStatusType.FIRE_DMG_RESIS:
                case eStatusType.WATER_DMG: case eStatusType.WATER_DMG_RESIS:
                case eStatusType.EARTH_DMG: case eStatusType.EARTH_DMG_RESIS:
                case eStatusType.WIND_DMG: case eStatusType.WIND_DMG_RESIS:
                case eStatusType.DARK_DMG: case eStatusType.DARK_DMG_RESIS:
                case eStatusType.LIGHT_DMG: case eStatusType.LIGHT_DMG_RESIS:
                {
                    /// 스텟 상승이 없는 경우 저항이나 속성쪽은 하나만 사용하기 때문에 표시방식이 혼자 다름
                    /// 만약 동작이 달라져야한다면 하위에서 구현을 수정해야함.
                    if (prevStat == nextStat)
                        return;

                    if (stat.Icon != null)
                    {
                        stat.Icon.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ElementIconPath, SBFunc.StrBuilder("type_", SBDefine.ConvertToElementString((int)SBFunc.ConvertStatToElement(stat.Type))));
                    }
                } break;
                default: break;
            }

            /// 이미 현재 값(이전 값 + 상승된 값)을 표시
            if(stat.Label != null)
            {
                stat.Label.text = Mathf.FloorToInt(nextStat).ToString();
            }
            //
            /// 상승된 값을 표시
            if (stat.AddedLabel != null)
            {
                var modStat = Mathf.FloorToInt(nextStat - prevStat);
                var operatorText = modStat > 0 ? "+" : modStat < 0 ? "-" : string.Empty;
                if (operatorText == string.Empty)
                    stat.AddedLabel.text = "";
                else
                    stat.AddedLabel.text = SBFunc.StrBuilder(operatorText, modStat);
            }
            //
        }
        public CanvasGroup GetCanvasGroup()
        {
            return group;
        }
    }
}
