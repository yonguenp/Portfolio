using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class NecoAnimatePopup : MonoBehaviour
{
    public RectTransform Panel;
    Vector2 originPanelSize;

    public RectTransform TopPanel;
    Vector2 originTopPanelSize;

    public RectTransform MidPanel;
    Vector2 originMidPanelSize;

    public RectTransform BotPanel;
    Vector2 originBotPanelSize;

    bool Animated = false;

    [ContextMenu("Animate")]
    protected virtual void OnEnable()
    {
        if (Animated)
        {
            Reset();
        }

        StopCoroutine(Animate());
        StartCoroutine(Animate());
    }

    protected virtual void OnDisable()
    {
        StopCoroutine(Animate());

        if (Animated)
            Reset();
    }

    public void Reset()
    {
        StopCoroutine(Animate());

        Panel.sizeDelta = originPanelSize;
        TopPanel.sizeDelta = originTopPanelSize;
        MidPanel.sizeDelta = originMidPanelSize;
        BotPanel.sizeDelta = originBotPanelSize;

        TopPanel.gameObject.SetActive(true);
        MidPanel.gameObject.SetActive(true);
        BotPanel.gameObject.SetActive(true);

        Animated = false;
    }

    IEnumerator Animate()
    {
        originPanelSize = Panel.sizeDelta;
        originTopPanelSize = TopPanel.sizeDelta;
        originMidPanelSize = MidPanel.sizeDelta;
        originBotPanelSize = BotPanel.sizeDelta;

        Debug.Log("originPanelSize y : " + originPanelSize.y);
        Animated = true;

        Panel.sizeDelta = Vector2.zero;
        TopPanel.sizeDelta = Vector2.zero;
        MidPanel.sizeDelta = Vector2.zero;
        BotPanel.sizeDelta = Vector2.zero;

        TopPanel.gameObject.SetActive(false);
        MidPanel.gameObject.SetActive(false);
        BotPanel.gameObject.SetActive(false);

        float animationTime = 0.1f;
        float curTime = 0.0f;

        TopPanel.gameObject.SetActive(true);
        while (animationTime > curTime)
        {
            curTime += Time.deltaTime;
            float ratio = (curTime / animationTime);

            Panel.sizeDelta = new Vector2(originPanelSize.x, originTopPanelSize.y) * ratio;
            TopPanel.sizeDelta = new Vector2(originTopPanelSize.x, originTopPanelSize.y) * ratio;

            yield return new WaitForEndOfFrame();
        }

        curTime = 0.0f;
        while (animationTime > curTime)
        {
            curTime += Time.deltaTime;
            float ratio = (curTime / animationTime);

            Panel.sizeDelta = new Vector2(originPanelSize.x, originTopPanelSize.y + (originBotPanelSize.y * ratio));
            BotPanel.sizeDelta = new Vector2(originBotPanelSize.x, originBotPanelSize.y * ratio);

            yield return new WaitForEndOfFrame();
        }

        curTime = 0.0f;
        
        float goalHeight = originPanelSize.y;
        float prevHeight = originTopPanelSize.y + originBotPanelSize.y;
        float diff = goalHeight - prevHeight;

        while (animationTime > curTime)
        {
            curTime += Time.deltaTime;
            float ratio = (curTime / animationTime);

            Panel.sizeDelta = new Vector2(originPanelSize.x, prevHeight + (diff * ratio));
            MidPanel.sizeDelta = new Vector2(originMidPanelSize.x, originMidPanelSize.y * ratio);

            yield return new WaitForEndOfFrame();
        }

        MidPanel.gameObject.SetActive(true);
        BotPanel.gameObject.SetActive(true);

        Reset();

        foreach (RectTransform rt in transform.GetComponentsInChildren<RectTransform>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        yield return new WaitForEndOfFrame();

        foreach (RectTransform rt in transform.GetComponentsInChildren<RectTransform>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        OnAnimateDone();
    }

    public abstract void OnAnimateDone();
}
