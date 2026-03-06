using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ChampionSurpportServerPopup : Popup<PopupBase>
    {
        [SerializeField] Text Total = null;
        [SerializeField] Text Target = null;

        [SerializeField] Slider slider = null;

        int Measure = 10;
        ChampionSurpportInfo.eSurpportType curType;
        public static void Open(ChampionSurpportInfo.eSurpportType type)
        {
            var popup = PopupManager.OpenPopup<ChampionSurpportServerPopup>();
            popup.SetSurpportType(type);
        }


        public override void InitUI()
        {
            Measure = GameConfigTable.GetConfigIntValue("MINIMUM_CHAMP_SURPPORT_MAGNITE", 10);
            slider.value = 0;
            slider.minValue = 0;
            slider.maxValue = User.Instance.UserData.Magnite / Measure;

            Total.text = SBFunc.CommaFromNumber(User.Instance.UserData.Magnite);

            UpdateValue();
        }

        void SetSurpportType(ChampionSurpportInfo.eSurpportType type)
        {
            curType = type;
        }

        public void UpdateValue()
        {
            if (slider.value > slider.maxValue)
                slider.value = slider.maxValue;
            if (slider.value < slider.minValue)
                slider.value = slider.minValue;

            Target.text = (slider.value * Measure).ToString();
        }

        public void Plus()
        {
            slider.value++;
            UpdateValue();
        }

        public void Minus()
        {
            slider.value--;
            UpdateValue();
        }

        public void Max()
        {
            slider.value = slider.maxValue;
            UpdateValue();
        }

        public void OK()
        {
            if (slider.value <= 0)
                return;

            ChampionSurpportCheckPopup.Open(curType, (int)slider.value * Measure);
        }
    }
}