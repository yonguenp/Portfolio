using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class WorldBossResultDragonSlotClone : MonoBehaviour
    {
        [SerializeField]
        DragonPortraitFrame dragonFrame = null;

        [SerializeField]
        private Color firstPointColor = Color.white;
        [SerializeField]
        private Color normalPointColor = Color.white;

        [SerializeField]
        private Color firstTimeColor = Color.white;
        [SerializeField]
        private Color normalTimeColor = Color.white;


        [SerializeField]
        private Text totalDmgText;
        [SerializeField]
        private Text LifeTimeText;

        [SerializeField] Image targetBG = null;
        [SerializeField] Sprite highlightBG = null;
        [SerializeField] Sprite normalBG = null;
        [SerializeField] int highLightSize = 40;
        [SerializeField] int normalSize = 35;

        public void Init(bool _isFirst, int dragonNo, StatisticsInfo info/*, StatisticsInfo bestStatistic*/)
        {
            //SetTextState(totalDmgText, bestStatistic.BossDmg == info.BossDmg && info.BossDmg != 0);
            SetPortrait(dragonNo);
            SetTextState(_isFirst, info);
            SetBG(_isFirst);
        }
        void SetTextState(Text text, bool isBest)
        {
            text.color = (isBest) ? firstPointColor : normalPointColor;
            text.fontStyle = (isBest) ? FontStyle.Bold : FontStyle.Normal;
        }

        void SetPortrait(int _dragonNo)
        {
            var dragonData = User.Instance.DragonData.GetDragon(_dragonNo);
            if (dragonFrame != null && dragonData != null)
                dragonFrame.SetDragonPortraitFrame(dragonData);
        }

        void SetTextState(bool _isFirst, StatisticsInfo _info)
        {
            if (LifeTimeText != null)
            {
                LifeTimeText.color = _isFirst ? firstTimeColor : normalTimeColor;
                LifeTimeText.text = _info.AliveTime == -1 ? "-" : SBFunc.TimeStringMinute(_info.AliveTime);
            }
            if(totalDmgText != null)
            {
                totalDmgText.text = SBFunc.CommaFromNumber(_info.BossDmg);
                totalDmgText.color = _isFirst ? firstPointColor : normalPointColor;
                totalDmgText.fontSize = _isFirst ? highLightSize : normalSize;
                totalDmgText.resizeTextMaxSize = _isFirst ? highLightSize : normalSize;
            }
        }

        void SetBG(bool _isFirst)
        {
            if (targetBG == null)
                return;

            if (highlightBG == null || normalBG == null)
                return;

            targetBG.sprite = _isFirst ? highlightBG : normalBG;
        }
    }
}

