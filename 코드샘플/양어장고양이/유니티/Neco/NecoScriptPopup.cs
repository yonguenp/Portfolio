using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NecoScriptPopup : MonoBehaviour
{
    public delegate void Callback();
    enum STATE { 
        SHOW_EFFECT,
        WAIT,
        HIDE_EFFECT,
    };

    public Text ScriptUIText;
    public Image ScriptUIBackground;

    Callback scriptDoneCallback = null;
    List<string> scripts;
    STATE state = STATE.SHOW_EFFECT;
    bool backgroundAction = false;

    public void OnShowScript(string _script, Callback doneCallback = null)
    {
        scriptDoneCallback = doneCallback;
        scripts = new List<string>();
        scripts.Add(_script);

        ScriptUIText.text = "";

        backgroundAction = true;
        OnShowScript();
    }

    public void OnShowScript(List<string> _scripts, Callback doneCallback = null)
    {
        scriptDoneCallback = doneCallback;
        scripts = _scripts;

        ScriptUIText.text = "";
        backgroundAction = true;
        OnShowScript();
    }

    private void OnShowScript()
    {
        state = STATE.SHOW_EFFECT;
        ScriptUIText.text = scripts[0];

        StopAllCoroutines();
        StartCoroutine(ScriptShowAction());
    }

    private void OnWaitScript()
    {
        state = STATE.WAIT;
        StopAllCoroutines();
        ScriptUIText.color = Color.white;
        ScriptUIBackground.color = new Color(0f, 0f, 0f, 0.509804f);
        //StartCoroutine(ScriptWaitAction());
    }

    private void OnHideScript()
    {
        state = STATE.HIDE_EFFECT;
        StopAllCoroutines();
        StartCoroutine(ScriptHideAction());
    }

    private IEnumerator ScriptShowAction()
    {
        Color color = Color.white;
        color.a = 0;

        Color bgColor = new Color(0f, 0f, 0f, 0.509804f);
        Color bg = bgColor;

        if (backgroundAction)
        {
            bg.a = bg.a * color.a;
            ScriptUIBackground.color = bg;
        }

        while (color.a < 1.0f)
        {
            color.a += Time.deltaTime * 2.0f;
            ScriptUIText.color = color;

            if (backgroundAction)
            {
                bg = bgColor;
                bg.a = bg.a * color.a;
                ScriptUIBackground.color = bg;
            }

            yield return new WaitForEndOfFrame();
        }

        ScriptUIText.color = Color.white;

        OnNext();
    }

    private IEnumerator ScriptWaitAction()
    {
        yield return new WaitForSeconds(1.0f);

    }

    private IEnumerator ScriptHideAction()
    {
        Color color = Color.white;

        Color bgColor = new Color(0f, 0f, 0f, 0.509804f);
        Color bg = bgColor;
        if (backgroundAction)
        {
            bg.a = bg.a * color.a;
            ScriptUIBackground.color = bg;
        }

        while (color.a > 0.0f)
        {
            color.a -= Time.deltaTime * 2.0f;
            ScriptUIText.color = color;

            if (backgroundAction)
            {
                bg = bgColor;
                bg.a = bg.a * color.a;
                ScriptUIBackground.color = bg;
            }

            yield return new WaitForEndOfFrame();
        }

        color.a = 0;
        ScriptUIText.color = color;

        OnNext();
    }

    private void OnNext()
    {
        backgroundAction = false;

        switch (state)
        {
            case STATE.SHOW_EFFECT:
                OnWaitScript();
                break;
            case STATE.WAIT:
                if (scripts.Count <= 1)
                    backgroundAction = true;

                OnHideScript();
                break;
            case STATE.HIDE_EFFECT:
                if (scripts.Count <= 1)
                {
                    OnPopupClose();
                }
                else
                {
                    scripts.RemoveAt(0);
                    OnShowScript();
                }
                break;
        }
    }

    private void OnPopupClose()
    {
        NecoCanvas.GetPopupCanvas().OnPopupClose(NecoPopupCanvas.POPUP_TYPE.SCRIPT_POPUP);
        Callback tmpCurCallback = scriptDoneCallback;
        scriptDoneCallback?.Invoke();
        if(tmpCurCallback == scriptDoneCallback)
            scriptDoneCallback = null;
    }

    public void OnTouch()
    {
        OnNext();
    }
}
