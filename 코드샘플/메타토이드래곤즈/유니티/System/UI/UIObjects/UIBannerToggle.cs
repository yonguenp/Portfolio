using DG.Tweening;
using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBannerToggle : UIObject, EventListener<UIObjectEvent>
{
    [SerializeField] CanvasGroup group = null;
    [SerializeField] GameObject Show = null;
    [SerializeField] GameObject Hide = null;
    [SerializeField] UIEventBanner Banner = null;
    [SerializeField] UIBadgeObject Badge = null;

    Tween fadeTween = null;
    bool isShow = true;
    private void Awake()
    {
        isShow = true;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        Banner.SetActive(false);
        Badge.SetActive(false);
    }
    public override void Init()
    {
        base.Init();

        Banner.Init();
        Banner.Focus(0); 
        Badge.Init();
    }

    private void Start()
    {
        EventManager.AddListener<UIObjectEvent>(this);
    }
    private void OnDestroy()
    {
        EventManager.RemoveListener<UIObjectEvent>(this);
    }

    public void OnToggle()
    {
        isShow = !isShow;
        Refresh();
    }

    void Refresh()
    {
        Show.SetActive(!isShow);
        Hide.SetActive(isShow);
        Banner.SetActive(isShow);
        Badge.SetActive(isShow);
    }

    public void OnEvent(UIObjectEvent eventType)
    {
        if ((eventType.t & UIObjectEvent.eUITarget.LT) != UIObjectEvent.eUITarget.NONE)
        {
            switch (eventType.e)
            {
                case UIObjectEvent.eEvent.EVENT_SHOW:
                    gameObject.SetActive(true);

                    if (fadeTween != null)
                        fadeTween.Kill();

                    fadeTween = group.DOFade(1.0f, 1.0f - group.alpha).OnComplete(() => { group.alpha = 1.0f; });
                    break;

                case UIObjectEvent.eEvent.EVENT_HIDE:
                    if (fadeTween != null)
                        fadeTween.Kill();
                    fadeTween = group.DOFade(0.0f, group.alpha).OnComplete(()=> {
                        group.alpha = 0.0f;
                        gameObject.SetActive(false); 
                    });
                    break;
            }
        }
    }
}
