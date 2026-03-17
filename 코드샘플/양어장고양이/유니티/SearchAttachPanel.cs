using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchAttachPanel : MonoBehaviour
{
    public GameObject[] SearchCase;
    public NecoVideoCanvas videoCanvas;
    public GameObject AttachTarget;

    public GameObject[] InteractionUIObjects;    
    public Text ScriptTextUI;

    private clip_event curData = null;
    private List<Coroutine> UIObjectCoroutine = new List<Coroutine>();

    private void OnEnable()
    {
        videoCanvas = NecoCanvas.GetVideoCanvas();
    }

    private void OnDisable()
    {
        ClearUI();
    }

    void ClearUI()
    {
        ScriptTextUI.text = "";
        ScriptTextUI.gameObject.SetActive(false);
        ScriptTextUI.DOKill();


        foreach (Coroutine co in UIObjectCoroutine)
        {
            StopCoroutine(co);
        }
        UIObjectCoroutine.Clear();

        foreach (GameObject ui in InteractionUIObjects)
        {
            ui.SetActive(false);
        }

        InteractionUIObjects[(int)ClipUIInfo.UI_TYPE.PROGRESS].transform.GetChild(0).GetComponent<Image>().DOKill();
    }

    public IEnumerator OnUIObjectCoroutine(ClipUIInfo info)
    {
        yield return new WaitForSeconds(info.startTime);

        InteractionUIObjects[(int)info.type].SetActive(true);

        if (info.type != ClipUIInfo.UI_TYPE.PROGRESS)
        {
            RectTransform rt = GetComponent<RectTransform>();
            InteractionUIObjects[(int)info.type].transform.localPosition = (info.position - (Vector2.one * 0.5f)) * rt.rect.size;
        }
        else
        {
            InteractionUIObjects[(int)info.type].transform.GetChild(0).GetComponent<Image>().fillAmount = 1.0f;
            InteractionUIObjects[(int)info.type].transform.GetChild(0).GetComponent<Image>().DOFillAmount(0.0f, info.expireTime);
        }

        yield return new WaitForSeconds(info.expireTime);

        InteractionUIObjects[(int)info.type].SetActive(false);

        yield return null;
    }

    public void OnTouchButton()
    {
        videoCanvas.SendTryInteraction(clip_event.CLIP_EVENT_TYPE.TOUCH);
    }

    public void OnPlayButton()
    {
        videoCanvas.SendTryInteraction(clip_event.CLIP_EVENT_TYPE.PLAY);
    }

    public void OnFeedButton()
    {
        videoCanvas.SendTryInteraction(clip_event.CLIP_EVENT_TYPE.FEED);
    }

    public void OnFishingButton()
    {
        videoCanvas.SendTryInteraction(clip_event.CLIP_EVENT_TYPE.FISH);
    }

    public bool IsParticleInteraction()
    {
        return (curData.GetEventType() == clip_event.CLIP_EVENT_TYPE.PLAY || curData.GetEventType() == clip_event.CLIP_EVENT_TYPE.TOUCH);
    }

    public void OnSearch(clip_event eventData, int index)
    {
        ClearUI();

        curData = eventData;

        gameObject.SetActive(true);
        foreach (Transform child in AttachTarget.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject curObject = Instantiate(SearchCase[index]);
        RectTransform curRT = curObject.GetComponent<RectTransform>();
        Canvas curCanvas = GetComponentInParent<Canvas>();
        RectTransform canvasTransform = curCanvas.GetComponent<RectTransform>();

        curRT.SetParent(AttachTarget.transform);

        curRT.localScale = Vector3.one;
        curRT.localPosition = Vector3.zero;
        float ratio = 1280 / canvasTransform.sizeDelta.y;
        curRT.sizeDelta = curRT.sizeDelta * ratio;

        Animation[] anims = GetComponentsInChildren<Animation>();
        foreach (Animation anim in anims)
        {
            anim.Stop();
            anim.enabled = false;
        }

        Image[] images = curObject.GetComponentsInChildren<Image>();
        foreach(Image image in images)
        {
            image.color = Color.black;
            image.DOColor(Color.white, 0.5f);
        }

        Invoke("SearchAnimationStart", 0.5f);
    }

    public void SearchAnimationStart()
    {
        float maxLength = 0.0f;
        Animation[] anims = GetComponentsInChildren<Animation>();
        foreach (Animation anim in anims)
        {
            anim.enabled = true;
            anim.Play();
            maxLength = Mathf.Max(maxLength, anim.clip.length);
        }

        foreach (Coroutine co in UIObjectCoroutine)
        {
            StopCoroutine(co);
        }
        UIObjectCoroutine.Clear();

        foreach (GameObject ui in InteractionUIObjects)
        {
            ui.SetActive(false);
        }

        ScriptTextUI.text = "";

        ScriptTextUI.DOKill();

        gameObject.SetActive(true);

        List<ClipSciptInfo> scripts = curData.GetScriptList();
        if (scripts != null)
        {
            ScriptTextUI.gameObject.SetActive(true);
            Sequence tseq = DOTween.Sequence();

            foreach (ClipSciptInfo info in scripts)
            {
                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(info.startTime);
                seq.Append(ScriptTextUI.DOText(info.scriptText, 0.0f));
                seq.AppendInterval(info.expireTime);
                seq.Append(ScriptTextUI.DOText("", 0.0f));
                tseq.Join(seq);
            }

            tseq.Restart();
        }

        List<ClipUIInfo> ui_list = curData.GetUIInfoList();
        if (ui_list != null)
        {
            foreach (ClipUIInfo info in ui_list)
            {
                UIObjectCoroutine.Add(StartCoroutine(OnUIObjectCoroutine(info)));
            }
        }

        Invoke("SearchAnimationHide", maxLength);
    }

    public void SearchAnimationHide()
    {
        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.DOColor(Color.black, 0.5f);
        }

        Invoke("SearchAnimationDone", 0.5f);
    }
    public void SearchAnimationDone()
    {
        CancelInvoke();
        videoCanvas.OnNextVideo();
    }

    public void SearchDone()
    {
        CancelInvoke();
        gameObject.SetActive(false);
    }
}
