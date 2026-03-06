using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupControl : MonoBehaviour
{
    enum POPUP_TYPE
    { 
        POPUP_RESULT,
        POPUP_REWARD,
        POPUP_LEVELUP,
        POPUP_SERVERWAIT,
        POPUP_MESSAGE,
        POPUP_TOAST,
        POPUP_MAX,
    };

    public delegate void Callback();
    Callback popupCloseCallback = null;
    Callback popupCancelCallback = null;
    public GameObject BackGround;
    public GameObject[] popupObject = new GameObject[(int)POPUP_TYPE.POPUP_MAX];

    private Coroutine ToastFadeCoroutine = null;
    private Coroutine CoroutineBackGroundAction = null;
    // Start is called before the first frame update
    void Start()
    {
        //NetworkManager.GetInstance().SetPopupControl(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Clear()
    {
        gameObject.SetActive(true);

        if (BackGround.activeSelf)
        {
            if (CoroutineBackGroundAction != null)
                StopCoroutine(CoroutineBackGroundAction);

            CoroutineBackGroundAction = StartCoroutine(RunAlphaAction(BackGround.GetComponent<Image>(), 0.5f, 0.0f));
            Invoke("OnCompleteTweenAnimation", 0.5f);
        }

        foreach (GameObject popup in popupObject)
        {
            popup.SetActive(false);
        }

        popupCloseCallback = null;
    }

    public void OnCompleteTweenAnimation()
    {
        BackGround.SetActive(false);
    }

    public void OnBackgroundActive()
    {
        BackGround.SetActive(true);

        if (CoroutineBackGroundAction != null)
            StopCoroutine(CoroutineBackGroundAction);

        CoroutineBackGroundAction = StartCoroutine(RunAlphaAction(BackGround.GetComponent<Image>(), 0, 0.5f));
        
        CancelInvoke("OnCompleteTweenAnimation");
    }

    public IEnumerator RunAlphaAction(Image target, float start, float end)
    {
        float startAlpha = start;
        float maxAlpha = end;
        float curTime = 0.0f;
        float actionTime = 1.0f;
        Color color = target.color;
        color.a = startAlpha;
        target.color = color;
        
        while (curTime < actionTime)
        {
            float delta = Time.deltaTime;
            curTime += delta;
            if (start > end)
                color.a -= delta * maxAlpha;
            else
                color.a += delta * maxAlpha;

            target.color = color;
            yield return new WaitForEndOfFrame();
        }

        color.a = maxAlpha;
        target.color = color;
    }

    public void OnShowUnlockPopup(Callback callback = null)
    {
        Clear();

        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_RESULT];

        Text text = target.transform.Find("Text").GetComponent<Text>();
        text.text = "고양이가 찾아왔습니다";

        target.SetActive(true);

        popupCloseCallback = callback;
    }

    public void OnShowCollectionPopup(card_collection cc, Callback callback = null)
    {
        Clear();

        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_REWARD];

        Text text = target.transform.Find("Text").GetComponent<Text>();
        text.text = "콜렉션을 추가하여 아이템을 획득하였습니다.";

        target.SetActive(true);

        popupCloseCallback = callback;
    }

    public void OnShowRewardPopup(int money, Callback callback = null)
    {
        Clear();

        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_REWARD];

        Text text = target.transform.Find("Text").GetComponent<Text>();
        text.text = "획득 경험치는 " + money + "입니다.";

        target.SetActive(true);

        popupCloseCallback = callback;
    }

    public void OnShowLevelUPPopup(uint level, Callback callback)
    {
        Clear();

        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_LEVELUP];

        Text text = target.transform.Find("Text").GetComponent<Text>();
        text.text = "레벨 업하여 " + level + "레벨을 달성했습니다.";

        target.SetActive(true);

        popupCloseCallback = callback;
    }

    public void OnShowServerWait()
    {
        Clear();
        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_SERVERWAIT];
        target.SetActive(true);

        foreach(Image img in target.GetComponentsInChildren<Image>())
        {
            StartCoroutine(RunAlphaAction(img, 0.0f, 0.5f));
        }

        target.transform.Find("Loading_CatFoot").GetComponent<Animation>().Play();        
        target.transform.Find("Button").gameObject.SetActive(false);
    }

    public void OnServerRetry(string uri,
        WWWForm param,
        NetworkManager.SuccessCallback onSuccess,
        NetworkManager.FailCallback onFail = null)
    {
        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_SERVERWAIT];
        GameObject button = target.transform.Find("Button").gameObject;
        button.gameObject.SetActive(true);
        Button btn = button.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            StartCoroutine(NetworkManager.GetInstance().SendPostCorutine(uri, param, onSuccess, onFail));
        });
    }

    public void OnServerWaitDone()
    {
        BackGround.SetActive(false);

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_SERVERWAIT];
        target.SetActive(false);
    }

    public void OnPopupMessage(string textMsg, Callback callback = null)
    {
        Clear();

        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_MESSAGE];

        Text text = target.transform.Find("Description_Panel").Find("Text").GetComponent<Text>();
        text.text = textMsg;

        target.transform.Find("Btns_Container").Find("Cancle_Button").gameObject.SetActive(false);
        target.transform.Find("Btns_Container").Find("OK_Button").gameObject.SetActive(true);
        target.SetActive(true);

        popupCloseCallback = callback;

        foreach (DOTweenAnimation dotween in target.transform.parent.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DORewind();
            dotween.DOPlayForward();
        }
    }

    public void OnPopupMessageYN(string textMsg, Callback yesCallback = null, Callback noCallback = null)
    {
        Clear();

        OnBackgroundActive();

        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_MESSAGE];

        Text text = target.transform.Find("Description_Panel").Find("Text").GetComponent<Text>();
        text.text = textMsg;

        target.transform.Find("Btns_Container").Find("Cancle_Button").gameObject.SetActive(true);
        target.transform.Find("Btns_Container").Find("OK_Button").gameObject.SetActive(true);
        target.SetActive(true);

        popupCloseCallback = yesCallback;
        popupCancelCallback = noCallback;

        foreach (DOTweenAnimation dotween in target.transform.parent.GetComponentsInChildren<DOTweenAnimation>())
        {
            dotween.DORewind();
            dotween.DOPlayForward();
        }
    }

    public void OnPopupToast(string textMsg)
    {
        if (ToastFadeCoroutine != null)
            StopCoroutine(ToastFadeCoroutine);

        ToastFadeCoroutine = StartCoroutine(RunToastPopup(textMsg));
    }

    public IEnumerator RunToastPopup(string textMsg)
    {
        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_TOAST];
        target.SetActive(true);

        Image image = target.GetComponent<Image>();
        Text text = target.transform.Find("Toast_Text").GetComponent<Text>();
        text.text = textMsg;

        Color imgcolor = image.color;
        Color txtcolor = text.color;
        float alpha = 0.0f;        
        float panelRatio = 0.705f;
        float time = 0.0f;

        float actionTIme = 0.5f;
        while (time < actionTIme)
        {
            float delta = Time.deltaTime;
            time += delta;

            alpha += (1.0f / actionTIme) * delta;

            imgcolor.a = alpha * panelRatio;
            txtcolor.a = alpha;

            image.color = imgcolor;
            text.color = txtcolor;

            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForSeconds(2.0f);
        
        time = 0.0f;
        while (time < actionTIme)
        {
            float delta = Time.deltaTime;
            time += delta;

            alpha -= (1.0f / actionTIme) * delta;

            imgcolor.a = alpha * panelRatio;
            txtcolor.a = alpha;

            image.color = imgcolor;
            text.color = txtcolor;

            yield return new WaitForEndOfFrame();
        }

        OnToastClose();
    }

    public void OnToastClose()
    {
        GameObject target = popupObject[(int)POPUP_TYPE.POPUP_TOAST];
        target.SetActive(false);
    }

    public void OnPopupClose()
    {
        popupCloseCallback?.Invoke();
        popupCloseCallback = null;
        Clear();        
    }

    public void OnPopupCancel()
    {
        popupCancelCallback?.Invoke();
        popupCancelCallback = null;
        Clear();
    }

    public bool TryCancel()
    {
        if (popupObject[(int)POPUP_TYPE.POPUP_TOAST].activeSelf)
        {
            OnToastClose();
            return true;
        }

        if (popupObject[(int)POPUP_TYPE.POPUP_MESSAGE].activeSelf)
        {
            if(popupObject[(int)POPUP_TYPE.POPUP_MESSAGE].transform.Find("Btns_Container").Find("Cancle_Button").gameObject.activeSelf)
            {
                OnPopupCancel();
                return true;
            }
            if (popupObject[(int)POPUP_TYPE.POPUP_MESSAGE].transform.Find("Btns_Container").Find("OK_Button").gameObject.activeSelf)
            {
                OnPopupClose();
                return true;
            }
        }

        if(popupObject[(int)POPUP_TYPE.POPUP_SERVERWAIT].activeSelf)
        {
            return true;
        }

        return false;
    }
}
