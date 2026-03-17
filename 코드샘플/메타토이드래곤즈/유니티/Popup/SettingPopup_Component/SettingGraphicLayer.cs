using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class SettingGraphicLayer : MonoBehaviour
    {
        // 그래픽 관련
        [Space(10)]
        [Header("Graphic")]
        [SerializeField]
        Toggle ToyToggle = null;
        [SerializeField]
        Slider ToySlider = null;

        public void Init()
        {
            // 드래곤 수량 관련 Init
            if(PlayerPrefs.HasKey("Setting_Toy"))
            {
                var toyData = JsonConvert.DeserializeObject<JObject>(PlayerPrefs.GetString("Setting_Toy"));
                if (toyData != null)
                {
                    ToySlider.value = toyData["value"].Value<float>(); 
                    ToyToggle.isOn = toyData["isOn"].Value<bool>();
                }
                else
                {
                    ToyToggle.isOn = true;
                    ToySlider.value = 1.0f;
                }
            }
            else
            {
                ToyToggle.isOn = true;
                ToySlider.value = 1.0f;
            }

            Refresh();
        }

        void Refresh()
        {
            if (ToyToggle.isOn)
            {
                ToySlider.interactable = false;
                ToySlider.fillRect.GetComponent<Image>().color = Color.gray;
            }
            else
            {
                ToySlider.interactable = true;
                ToySlider.fillRect.GetComponent<Image>().color = new Color(0.7960785f, 0.9529412f, 0.2588235f);
            }
        }

        public void OnClickToyToggle()
        {
            this.SaveToyLocalData();
            Refresh();

            if (ToyToggle.isOn)
            {
                ToastManager.On(StringData.GetStringByStrKey("타운드래곤무제한설정"));
            }
            else
            {
                ToastManager.On(StringData.GetStringByStrKey("타운드래곤제한설정"));
            }
        }

        public void OnToyValueChanged()
        {
            this.SaveToyLocalData();
        }

        void SaveToyLocalData()
        {
            if (this.ToyToggle == null || this.ToySlider == null) { return; }

            JObject toyData = new JObject();
            toyData.Add("isOn", ToyToggle.isOn);
            toyData.Add("value", ToySlider.value);

            var DataString = JsonConvert.SerializeObject(toyData);
            PlayerPrefs.SetString("Setting_Toy", DataString);

            CancelInvoke("ApplyTown");
            Invoke("ApplyTown", 1.0f);
        }

        void ApplyTown()
        {
            //Town.Instance.SettingDragonVisible();
        }
    }
}
