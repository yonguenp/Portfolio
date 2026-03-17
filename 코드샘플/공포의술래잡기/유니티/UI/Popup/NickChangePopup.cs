using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NickChangePopup : Popup
{
    [SerializeField]
    InputField NickField;
    
    public void OnChangeNick()
    {
        if (string.IsNullOrWhiteSpace(NickField.text))
            return;

        if (Crosstales.BWF.BWFManager.Instance.Contains(NickField.text))
        {
            PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구4"));
            return;
        }

        PopupCanvas.Instance.ShowConfirmPopup("닉네임변경안내문구", () => { 
            WWWForm param = new WWWForm();
            param.AddField("nick", NickField.text);

            SBWeb.SendPost("user/use_nickchange", param, (response) =>
            {
                JObject res = (JObject)response;
                if (res.ContainsKey("rs"))
                {
                    int rs = res["rs"].Value<int>();
                    if (rs == 0)
                    {
                        GameManager.Instance.ClearSingleton();
                        Managers.Instance.ClearSingleton();

                        UnityEngine.SceneManagement.SceneManager.LoadScene("Start");
                    }
                    else
                    {
                        switch (rs)
                        {
                            case 203:
                                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구2"));
                                break;
                            case 204:
                                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구3"));
                                break;
                            case 205:
                                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임_문구4"));
                                break;
                            default:
                                PopupCanvas.Instance.ShowFadeText(StringManager.GetString("닉네임변경오류"));
                                break;
                        }
                    }
                }
            });
        });
    }
}
