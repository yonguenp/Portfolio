using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUIControl : MonoBehaviour
{
    public Image[] lines;
    public GameObject[] InteractionUIObjects;
    public NecoVideoCanvas videoCanvas;
    public Text ScriptTextUI;

    public Font HoonminFont;
    public Font JapFont;

    private clip_event curData = null;
    private List<Coroutine> UIObjectCoroutine = new List<Coroutine>();

    public Slider sliderHorizontal;
    public Slider sliderVertical;
    public RawImage videoRenderRawImage;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        videoCanvas = NecoCanvas.GetVideoCanvas();
        ScriptTextUI.font = LocalizeData.instance.CurLanguage() == SystemLanguage.Japanese ? JapFont : HoonminFont;

        InitSlider();
        OnSlideChange();
    }

    private void OnDisable()
    {
        sliderHorizontal.gameObject.SetActive(false);
        sliderVertical.gameObject.SetActive(false);

        ScriptTextUI.text = "";
        ScriptTextUI.gameObject.SetActive(false);
        ScriptTextUI.DOKill();

        foreach (Image line in lines)
        {
            Color color = line.color;
            color.a = 0.0f;
            line.color = color;
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

        InteractionUIObjects[(int)ClipUIInfo.UI_TYPE.PROGRESS].transform.GetChild(0).GetComponent<Image>().DOKill();

        curData = null;
    }

    public void SetClipData(clip_event data)
    {
        foreach (Coroutine co in UIObjectCoroutine)
        {
            StopCoroutine(co);
        }
        UIObjectCoroutine.Clear();

        foreach (GameObject ui in InteractionUIObjects)
        {
            ui.SetActive(false);
        }

        curData = data;
        ScriptTextUI.text = "";
        
        ScriptTextUI.DOKill();

        gameObject.SetActive(true);

        List<ClipSciptInfo> scripts = data.GetScriptList();
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

        List<ClipUIInfo> ui_list = data.GetUIInfoList();
        if (ui_list != null)
        {
            foreach (ClipUIInfo info in ui_list)
            {
                UIObjectCoroutine.Add(StartCoroutine(OnUIObjectCoroutine(info)));
            }
        }
        
        foreach (Image line in lines)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(line.DOFade(0.0f, 1.0f));
            seq.Append(line.DOFade(1.0f, 2.0f));
            seq.Restart();
        }
    }

    public IEnumerator OnUIObjectCoroutine(ClipUIInfo info)
    {
        yield return new WaitForSecondsRealtime(info.startTime);

        InteractionUIObjects[(int)info.type].SetActive(true);
        MaskableGraphic[] graphicObjects = InteractionUIObjects[(int)info.type].GetComponentsInChildren<MaskableGraphic>();
        float alpha = 0.0f;

        foreach(MaskableGraphic graphicObject in graphicObjects)
        {
            Color color = graphicObject.color;
            color.a = alpha;
            graphicObject.color = color;
        }

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

        if (info.expireTime > 1.0f)
        {
            while (alpha < 1.0f)
            {
                alpha += 0.1f;

                foreach (MaskableGraphic graphicObject in graphicObjects)
                {
                    Color color = graphicObject.color;
                    color.a = alpha;
                    graphicObject.color = color;
                }

                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForSecondsRealtime(info.expireTime - 1.0f);
        }
        else
        {
            alpha = 1.0f;
            foreach (MaskableGraphic graphicObject in graphicObjects)
            {
                Color color = graphicObject.color;
                color.a = alpha;
                graphicObject.color = color;
            }

            yield return new WaitForSeconds(info.expireTime);
        }

        InteractionUIObjects[(int)info.type].SetActive(false);

        yield return null;
    }

    public void OnVideoFadeOut()
    {
        InteractionUIObjects[(int)ClipUIInfo.UI_TYPE.PROGRESS].transform.GetChild(0).GetComponent<Image>().DOKill();

        foreach (Image line in lines)
        {
            line.DOFade(0.0f, 0.5f);
        }

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
        return curData != null && curData.GetInteractionType() == clip_event.CLIP_INTERACTION_TYPE.TOUCHABLE;
    }

    public void InitSlider()
    {
        //sliderHorizontal.gameObject.SetActive(true);
        //sliderVertical.gameObject.SetActive(true);

        sliderHorizontal.value = 0.5f;
        sliderVertical.value = 1.0f;
        
        OnSlideChange();
    }
    public void OnSlideChange()
    {
        //float vertVal = sliderVertical.value;
        //float horiVal = sliderHorizontal.value;

        float vertVal = 1.0f;
        float horiVal = 0.5f;

        RectTransform canvasRectTransfrom = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        RectTransform curRectTransfrom = videoRenderRawImage.GetComponent<RectTransform>();
        Vector2 sizeCanvas = canvasRectTransfrom.sizeDelta;
        
        float maxHeight = sizeCanvas.y;

        Vector2 curSize = new Vector2(maxHeight, maxHeight);
        curRectTransfrom.sizeDelta = curSize;
        curRectTransfrom.localPosition = Vector3.zero;

        videoCanvas?.ResizedMovieRawImage();
    }
}
