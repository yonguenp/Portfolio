using DG.Tweening;
using SandboxNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBadgeItem : MonoBehaviour
{
    [SerializeField] Image Icon = null;
    [SerializeField] Text NameText = null;
    [SerializeField] Text TimeText = null;
    [SerializeField] TimeObject timeObject = null;
    [SerializeField] GameObject reddotObj = null;
    [SerializeField] GameObject timeLayer = null;
    Action callback = null;
    bool isDonDestroy = false;
    bool showTimeLayer = true;
    bool isNotInTime = false;

    public void InitShopIcon(string icon, string nameString, int remain, bool donDestroy,bool reddot, Action cb)
    {
        CDNManager.SetBanner("store/" + icon, Icon, RefreshIconSize);        
        Init(nameString, remain, donDestroy, reddot, cb);
    }

    public void InitEventIcon(Sprite icon, string nameString, int remain, bool donDestroy, bool reddot, Action cb)
    {
        Icon.sprite = icon;
        RefreshIconSize();
        Init(nameString, remain, donDestroy, reddot, cb);
    }

    private void Init(string nameString, int remain, bool donDestroy, bool reddot, Action cb)
    {
        callback = cb;
        isDonDestroy = donDestroy;
        NameText.text = nameString;
        if (reddotObj != null)
        {
            reddotObj.SetActive(reddot);
            showTimeLayer = !reddot;
        }

        if (timeLayer != null)
        {
            timeLayer.SetActive(showTimeLayer);

            if (remain > 60 * 60 * 24 * 30 * 3) // 3달 이상이면
            {
                timeLayer.SetActive(false);
                isNotInTime = true;
                return;
            }
        }

        SetTimer(remain);
    }
    void RefreshIconSize()
    {
        if (Icon.sprite == null)
            return;

        Icon.transform.DOKill();
        Icon.transform.localScale = Vector3.one;

        float ratio = (float)Icon.sprite.texture.width / Icon.sprite.texture.height;
        var size = (Icon.transform as RectTransform).sizeDelta;
        size.x = size.y * ratio;
        (Icon.transform as RectTransform).sizeDelta = size;
    }

    public void SetTimer(int r)
    {
        int curTime = TimeManager.GetTime();
        int remain = curTime + r;
        timeObject.Refresh = delegate {

            int remainTime = TimeManager.GetTimeCompare(remain);
            TimeText.text = StringData.GetStringFormatByStrKey("남음", SBFunc.TimeCustomString(remainTime, 2));

            if (remainTime <= 0)
            {
                if(isDonDestroy)
                    gameObject.SetActive(false);
                else
                    Destroy(gameObject);
            }
        };
    }

    public void OnClick()
    {
        Icon.transform.DOKill();
        Icon.transform.DOScale(1.3f, 0.15f).SetLoops(2, LoopType.Yoyo);

        if (reddotObj != null)
        {
            reddotObj.SetActive(false);
            timeLayer.SetActive(!isNotInTime);
        }


        callback?.Invoke();
    }
}
