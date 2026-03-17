using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIOnOffButton : MonoBehaviour
{
    [SerializeField] Toggle toggle;

    [SerializeField] Text onText;
    [SerializeField] Text offText;

    [SerializeField] GameObject onButton;
    [SerializeField] GameObject offButton;


    public void SetUI()
    {
        onText.color = toggle.isOn == true ? Color.black : Color.white;
        offText.color = toggle.isOn == true ? Color.white: Color.black;

        onButton.SetActive(toggle.isOn);
        offButton.SetActive(!toggle.isOn);
    }
}
