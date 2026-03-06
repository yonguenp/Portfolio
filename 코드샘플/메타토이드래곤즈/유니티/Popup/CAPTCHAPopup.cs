using SandboxNetwork;
using UnityEngine;
using UnityEngine.UI;


public class CAPTCHAPopup : Popup<PopupData>
{
    [SerializeField]
    Text[] Question;

    [SerializeField]
    Text Answer;

    string strQuestion = string.Empty;
    System.Action cb = null;
    System.DateTime checkPoint = System.DateTime.MinValue;

    bool forceCAPTCHA = false;
    
    public static CAPTCHAPopup OpenPopup(System.Action cb)
    {   
        var popup = PopupManager.OpenPopup<CAPTCHAPopup>();
        popup.cb = cb;

        return popup;
    }

    public static void SetCAPTCHA()
    {
        var popup = PopupManager.GetPopup<CAPTCHAPopup>();
        if (popup == null)
            return;

        popup.forceCAPTCHA = true;
    }

    public static bool NeedCheck()
    {
        var popup = PopupManager.GetPopup<CAPTCHAPopup>();
        if (popup == null)
            return false;
        
        if (popup.forceCAPTCHA)
            return true;

        if (popup.checkPoint == System.DateTime.MinValue)
        {
            popup.checkPoint = System.DateTime.Now;
            return false;
        }

        if ((System.DateTime.Now - popup.checkPoint).TotalSeconds > GameConfigTable.GetConfigIntValue("CAPTCHA_TIMER", 3600))
        {
            return true;
        }

        return false;
    }
    public override void InitUI()
    {
        strQuestion = "";
        for (int i = 0; i < Question.Length; i++)
        {
            string q = Random.Range(0, 10).ToString();
            Question[i].text = q;
            strQuestion += q;
        }

        Answer.text = string.Empty;
    }

    public void OnButton(string value)
    {
        if (value == "back")
        {
            if (Answer.text.Length - 1 <= 0)
                return;

            Answer.text = Answer.text.Substring(0, Answer.text.Length - 1);
            return;
        }

        if (value == "enter")
        {
            SummitAnswer();
            return;
        }

        if(int.TryParse(value, out int r))
        {
            Answer.text += value;
        }
    }

    public void SummitAnswer()
    {
        //맞음
        if (Answer.text == strQuestion)
        {
            checkPoint = System.DateTime.Now;
            forceCAPTCHA = false;
            
            cb.Invoke();
            ClosePopup();
            return;
        }

        //틀림
        InitUI();
    }
}
