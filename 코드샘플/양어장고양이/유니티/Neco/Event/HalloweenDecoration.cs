using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HalloweenDecoration : MonoBehaviour
{
    public GameObject Sample;
    bool enableAction = false;
    halloween_event curEventData;
    public void Awake()
    {
        OnDecoration();

        foreach (neco_event evt in neco_data.Instance.GetEvents())
        {
            if ((neco_event.EVENT_TYPE)evt.GetEventID() == neco_event.EVENT_TYPE.HALLOWEEN)
                curEventData = (halloween_event)evt;
        }
    }

    public void OnDecoration()
    {
        if(enableAction == false)
        {
            Invoke("OnDecorationAction", 0.1f);
        }
    }

    public void Clear()
    {
        CancelInvoke("Clear");

        enableAction = false;
        Sample.SetActive(false);

        foreach (Transform child in transform)
        {
            if (child.gameObject == Sample)
                continue;

            Destroy(child.gameObject);
        }
    }


    private void OnDecorationAction()
    {
        CancelInvoke("OnDecorationAction");

        Clear();

        NecoPopupCanvas popupCanvas = NecoCanvas.GetPopupCanvas();
        if (popupCanvas == null)
            return;

        if (neco_data.PrologueSeq.배틀패스보상받기 >= neco_data.GetPrologueSeq())
        {
            if(popupCanvas.PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.HALLOWEEN_POPUP].activeSelf == false)
                return;
        }

        if (curEventData == null)
            return;

        if (!curEventData.IsEventTime())
            return;

        if (popupCanvas.PopupObject[(int)NecoPopupCanvas.POPUP_TYPE.HALLOWEEN_POPUP].activeSelf == false)
        {
            if (Random.value <= 0.7f)
                return;
        }

        enableAction = true;
        Sample.SetActive(true);

        RectTransform cavasTransform = (popupCanvas.transform as RectTransform);
        Vector2 canvasSize = cavasTransform.sizeDelta;
        
        float min_x = canvasSize.x * -0.5f;
        float max_x = canvasSize.x * 0.5f;
        float min_y = canvasSize.y * -0.5f;
        float max_y = canvasSize.y * 0.5f;

        Vector2 batSize = (Sample.transform as RectTransform).sizeDelta;

        for(int i = 0; i < Random.Range(3, 6); i++)
        {
            GameObject SampleBat = Instantiate(Sample);
            SampleBat.transform.SetParent(transform);

            float scale = Random.Range(0.5f, 1.5f);
            SampleBat.transform.localScale = Vector2.one * scale;
            SampleBat.transform.localPosition = new Vector2(min_x + (batSize.x * -1.0f * scale), Random.Range(max_y, min_y));

            float goal_x = Random.Range(0.0f, max_x);
            
            SampleBat.transform.DOLocalMoveX(goal_x, Random.Range(1.0f, 1.5f));
            SampleBat.transform.DOLocalMoveY(max_y + (batSize.y * scale), Random.Range(1.0f, 1.5f)).SetEase(Ease.InCubic);
        }

        for (int i = 0; i < Random.Range(3, 6); i++)
        {
            GameObject SampleBat = Instantiate(Sample);
            SampleBat.transform.SetParent(transform);

            float scale = Random.Range(0.5f, 1.5f);
            SampleBat.transform.localScale = Vector2.one * scale;
            SampleBat.transform.localPosition = new Vector2(max_x + (batSize.x * scale), Random.Range(max_y, min_y));

            float goal_x = Random.Range(0.0f, min_x);

            SampleBat.transform.DOLocalMoveX(goal_x, Random.Range(1.0f, 1.5f));
            SampleBat.transform.DOLocalMoveY(max_y + (batSize.y * scale), Random.Range(1.0f, 1.5f)).SetEase(Ease.InCubic);

            SampleBat.transform.localScale = new Vector2(SampleBat.transform.localScale.x * -1.0f, SampleBat.transform.localScale.y);
        }

        Sample.SetActive(false);

        Invoke("Clear", 1.5f);
    }
}
