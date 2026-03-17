using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ServerNotifyPopup : Popup
{
    [SerializeField]
    Text Text;

    [SerializeField] RectTransform brt;
    [SerializeField] RectTransform trt;
    [SerializeField] RectTransform container;

    float bgSizeOffset = 45.0f;
    float margin = 300.0f;
    bool isMarquee = false;
    float delayUpdate = 0.1f;

    [ContextMenu("TestNotify")]
    void TestNotify()
    {
        PopupCanvas.Instance.ShowServerNotifyText("안녕하세요 지호규입니다. 안녕하세요 지호규입니다.안녕하세요 지호규입니다.안녕하세요 지호규입니다.");
    }
    [ContextMenu("TestNotify2")]
    void TestNotify2()
    {
        PopupCanvas.Instance.ShowServerNotifyText("Wkks");
    }

    private void OnDisable()
    {
        StopMarquee();
    }
    public void SetServerNotify(string text, float limitTime)
    {
        Text.text = text;
        delayUpdate = 0.1f;
        StopMarquee();

        CancelInvoke("Close");
        Invoke("Close", limitTime);
    }

    void RefreshSize()
    {
        if (brt.sizeDelta.x < trt.sizeDelta.x + bgSizeOffset)
        {
            if (!isMarquee)
            {
                PlayMarquee();
            }
        }
    }

    void StopMarquee()
    {
        isMarquee = false;

        foreach (Transform child in container)
        {
            if (child != trt)
            {
                Destroy(child.gameObject);
            }
        }

        container.DOKill();

        trt.localPosition = Vector3.zero;
        container.localPosition = Vector3.zero;
    }

    public void PlayMarquee()
    {
        isMarquee = true;

        container.sizeDelta = new Vector2(trt.sizeDelta.x + margin + trt.sizeDelta.x, trt.sizeDelta.y);

        Vector3 pos = container.localPosition;
        pos.x = (brt.sizeDelta.x * -0.5f) + (container.sizeDelta.x * 0.5f);
        container.localPosition = pos;
        
        pos.y = 0.0f;
        pos.x = (container.sizeDelta.x * -0.5f) + (trt.sizeDelta.x * 0.5f);
        trt.localPosition = pos;

        pos.x = pos.x + margin + trt.sizeDelta.x;

        var newItem = Instantiate(trt.gameObject);
        newItem.transform.SetParent(container);
        newItem.transform.localPosition = pos;
        newItem.transform.localScale = Vector3.one;

        container.DOLocalMoveX((trt.sizeDelta.x * -0.5f) + (margin * 0.5f), 15.0f).SetDelay(2f).SetLoops(-1, LoopType.Restart);
    }

    // Update is called once per frame
    void Update()
    {
        if (delayUpdate > 0.0f)
        {
            delayUpdate -= Time.deltaTime;
            return;
        }

        RefreshSize();
    }
}
