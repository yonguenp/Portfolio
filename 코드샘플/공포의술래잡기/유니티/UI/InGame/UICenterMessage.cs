using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICenterMessage : MonoBehaviour
{
    [SerializeField] GameLogNotifyMessage[] centerMessageUI = null;


    List<GameLogNotifyMessage> centerMessage = new List<GameLogNotifyMessage>();


    private void Awake()
    {
        foreach (GameLogNotifyMessage msg in centerMessageUI)
        {
            if (msg != null)
                msg.gameObject.SetActive(false);
        }
    }

    public void ShowDummyMessage(int uid, string id)
    {
        GameLogNotifyMessage cm = CreateCenterMessage(uid, id);
        StartCoroutine(CenterMessageAnimCoroutine(cm));
    }

    public void ShowKillMessage(CharacterObject killer, CharacterObject victim)
    {
        GameLogNotifyMessage cm = CreateCenterKillMessage(killer, victim);
        StartCoroutine(CenterMessageAnimCoroutine(cm));
    }

    private IEnumerator CenterMessageAnimCoroutine(GameLogNotifyMessage cm)
    {
        RectTransform rect = cm.transform as RectTransform;
        rect.localScale = Vector3.zero;

        bool delayShow = true;
        
        while (delayShow)
        {
            delayShow = false;
            if (centerMessage.Count > 0)
            {
                foreach (var msg in centerMessage)
                {
                    if (msg.PassForceShowTime == false)
                    {                        
                        delayShow = true;
                        break;
                    }
                }
            }

            if(delayShow)
                yield return new WaitForEndOfFrame();
        }

        cm.SetTime(1.0f, 0.3f);
        cm.StartCoroutine(cm.FadeOutCoroutine());        

        if (centerMessage.Count > 0)
        {            
            float offsetY = 259.0f;
            int bufferCount = 3;
            if (centerMessage.Count > bufferCount)
            {
                for (int i = 0; i < centerMessage.Count - bufferCount; i++)
                {
                    Destroy(centerMessage[i].gameObject);
                }

                centerMessage.RemoveRange(0, centerMessage.Count - bufferCount);
            }

            for (int i = 0; i < centerMessage.Count; i++)
            {
                RectTransform tmp = centerMessage[i].transform as RectTransform;
                tmp.DOKill();

                float y = 0;
                float scale = 1.0f;
                for (int j = 0; j < centerMessage.Count - i; j++)
                {
                    y += offsetY * scale;
                    scale *= 0.7f;
                }

                tmp.DOLocalMoveY(y, 0.2f);
                tmp.DOScale(scale, 0.2f);
            }
        }

        cm.PassForceShowTime = false;
        centerMessage.Add(cm);
                
        rect.DOKill();
        rect.localScale = Vector3.one * 0.5f;
        rect.DOScale(1.0f, 0.1f);

        yield return new WaitForSeconds(0.3f);
        cm.PassForceShowTime = true;
        yield return new WaitForSeconds(0.7f);

        if (cm == null) yield break;

        centerMessage.Remove(cm);

        rect.DOKill();
        rect.DOScale(0.0f, 0.2f);
    }

    private GameLogNotifyMessage CreateCenterKillMessage(CharacterObject killer, CharacterObject victim)
    {
        var message = Instantiate(centerMessageUI[0]);

        message.transform.SetParent(transform.parent);
        message.SetTime(1.0f, 0.3f);
        (message as KillNotifyMessage).Initialize(killer, victim);
        message.transform.localPosition = Vector3.zero;
        message.transform.localScale = Vector3.one;

        message.SetDestroyCallback((cm) =>
        {
            centerMessage.Remove(cm);
        });

        return message;
    }

    private GameLogNotifyMessage CreateCenterMessage(int uid, string id)
    {
        var message = Instantiate(centerMessageUI[1]);

        message.transform.SetParent(transform.parent);
        message.transform.localPosition = Vector3.zero;
        message.transform.localScale = Vector3.one;
        (message as CenterNotifyMessage).SetData(uid, id);

        message.SetDestroyCallback((cm) =>
        {
            centerMessage.Remove(cm);
        });

        return message;
    }
}
