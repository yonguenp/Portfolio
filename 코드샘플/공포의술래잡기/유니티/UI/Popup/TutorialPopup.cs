using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : Popup
{
    public enum HelpTapType
    {
        RULE,
        CONTROL,
        MAP,
        MATCH,
        CHARACTER
    }

    [Serializable]
    public class HelpTap
    {
        public HelpTapType type;
        public Sprite[] Images;
        public Image menu;
        public Sprite focusMenuSprite;
        public Sprite normalMenuSprite;
    }

    [SerializeField]
    HelpTap[] TapInfo;

    [SerializeField]
    Image curImage;

    [SerializeField]
    Transform dotContainer;
    [SerializeField]
    GameObject dotSample;

    [SerializeField]
    Sprite focusDotSprite;
    [SerializeField]
    Sprite normalDotSprite;
    [SerializeField]
    Button leftButton;
    [SerializeField]
    Button rightButton;

    public Color outColor;
    public Color curColor;
    Sprite[] tutorialImages;
    int curPage = 0;

    public void SetMenu(int index)
    {
        SetMenu((HelpTapType)index);
    }
    public void SetMenu(HelpTapType type)
    {
        foreach(HelpTap tap in TapInfo)
        {
            tap.menu.sprite = tap.normalMenuSprite;
            tap.menu.GetComponentInChildren<Text>().color = outColor;
        }

        HelpTap curTapData = TapInfo[(int)type];
        curTapData.menu.sprite = curTapData.focusMenuSprite;
        curTapData.menu.GetComponentInChildren<Text>().color = curColor;

        tutorialImages = curTapData.Images;

        curPage = 0;

        foreach (Transform dot in dotContainer)
        {
            if (dot == dotSample.transform)
                continue;

            Destroy(dot.gameObject);
        }

        dotSample.SetActive(true);

        for(int i = 0; i < tutorialImages.Length; i++)
        {
            var dot = Instantiate(dotSample);
            dot.transform.SetParent(dotContainer);
            dot.transform.localPosition = Vector3.zero;
            dot.transform.localScale = Vector3.one;
        }

        dotSample.transform.SetAsLastSibling();
        dotSample.SetActive(false);

        RefreshUI();
        Invoke("DotRefresh", 0.05f);
    }

    public override void Close()
    {
        if(Managers.UserData.ShownTutorial == false)
        {
            com.adjust.sdk.AdjustEvent adjustEvent = new com.adjust.sdk.AdjustEvent("mdmeta");
            com.adjust.sdk.Adjust.trackEvent(adjustEvent);
        }

        Managers.UserData.ShownTutorial = true;

        base.Close();
    }

    public void OnPrevPage()
    {
        curPage--;
        RefreshUI();
    }


    public void OnNextPage()
    {
        curPage++;        
        RefreshUI();
    }


    public override void RefreshUI()
    {
        leftButton.interactable = true;
        rightButton.interactable = true;
        
        if (curPage <= 0)
        {
            curPage = 0;
            leftButton.interactable = false;
        }

        if (curPage >= tutorialImages.Length - 1)
        {
            curPage = tutorialImages.Length - 1;
            rightButton.interactable = false;
        }

        DotRefresh();

        curImage.sprite = tutorialImages[curPage];
    }

    void DotRefresh()
    {
        CancelInvoke("DotRefresh");

        for (int i = 0; i < dotContainer.childCount; i++)
        {
            dotContainer.GetChild(i).GetComponent<Image>().sprite = i == curPage ? focusDotSprite : normalDotSprite;

        }
    }
}
