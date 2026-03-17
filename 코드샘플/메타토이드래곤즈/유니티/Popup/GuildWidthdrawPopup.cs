using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GuildWidthdrawPopup : Popup<PopupBase>
    {
        [SerializeField] Text Title = null;

        [SerializeField] Text SubTitle = null;

        [SerializeField] GameObject Magnet = null;
        [SerializeField] GameObject Magnite = null;

        [SerializeField] Text Total = null;
        [SerializeField] Text Target = null;

        [SerializeField] Slider slider = null;

        GuildAssetType type = GuildAssetType.MAGNET_DEPOSIT;
        GuildUserData userData = null;
        public static void Open(GuildAssetType type, GuildUserData data)
        {
            var popup = PopupManager.OpenPopup<GuildWidthdrawPopup>();
            popup.SetData(type, data);
        }

        public void SetData(GuildAssetType t, GuildUserData data) 
        {
            type = t;
            userData = data;

            slider.value = 0;
            slider.minValue = 0;
            switch (type)
            {
                case GuildAssetType.MAGNET_DEPOSIT:
                    SubTitle.text = GuildManager.Instance.MyBaseData.GuildName;
                    Title.text = StringData.GetStringByStrKey("마그넷입금");
                    Magnet.SetActive(true);
                    Magnite.SetActive(false);
                    Total.text = SBFunc.CommaFromNumber(User.Instance.UserData.Magnet);
                    slider.maxValue = User.Instance.UserData.Magnet;
                    break;
                case GuildAssetType.MAGNET_WITHDRAW:
                    SubTitle.text = data.Nick;
                    Title.text = StringData.GetStringByStrKey("마그넷출금");
                    Magnet.SetActive(true);
                    Magnite.SetActive(false);
                    Total.text = SBFunc.CommaFromNumber(GuildManager.Instance.MyGuildInfo.GuildMagnet);
                    slider.maxValue = GuildManager.Instance.MyGuildInfo.GuildMagnet;
                    break;
                case GuildAssetType.MAGNITE_DEPOSIT:
                    SubTitle.text = GuildManager.Instance.MyBaseData.GuildName;
                    Title.text = StringData.GetStringByStrKey("마그나이트입금");
                    Magnet.SetActive(false);
                    Magnite.SetActive(true);
                    Total.text = SBFunc.CommaFromNumber(User.Instance.UserData.Magnite);
                    slider.maxValue = User.Instance.UserData.Magnite;
                    break;
                case GuildAssetType.MAGNITE_WITHDRAW:
                    SubTitle.text = data.Nick;
                    Title.text = StringData.GetStringByStrKey("마그나이트출금");
                    Magnet.SetActive(false);
                    Magnite.SetActive(true);
                    Total.text = SBFunc.CommaFromNumber(GuildManager.Instance.MyGuildInfo.GuildMagnite);
                    slider.maxValue = GuildManager.Instance.MyGuildInfo.GuildMagnite;
                    break;
            }

            UpdateValue();
        }

        public override void InitUI()
        {
            
        }

        public void UpdateValue()
        {
            if (slider.value > slider.maxValue)
                slider.value = slider.maxValue;
            if (slider.value < slider.minValue)
                slider.value = slider.minValue;

            Target.text = slider.value.ToString();
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

            GuildWidthdrawCheckPopup.Open(type, userData, (int)slider.value);
        }
    }
}