using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISubMenu : MonoBehaviour
{
    [SerializeField]
    Image SubMenuBackground;
    [SerializeField]
    GameObject SubMenu;
    [SerializeField]
    RectTransform TopLine;

    [SerializeField]
    bool OnPlayerInfo;
    [SerializeField]
    bool OnCloseButton;
    public void OnEnable()
    {
        Clear();
    }

    public void Clear()
    {
        SubMenu.SetActive(false);
        if (SubMenuBackground)
        {
            Color color = SubMenuBackground.color;
            color.a = 0.0f;
            SubMenuBackground.color = color;

            SubMenuBackground.gameObject.SetActive(false);
        }
    }

    public void SubMenuToggle()
    {
        float sizeX = (transform as RectTransform).sizeDelta.x;
        if (SubMenu.activeSelf)
        {
            foreach (Transform child in SubMenu.transform)
            {
                child.DOKill();
                child.DOLocalMoveX(sizeX * 1.5f, 0.1f);
            }

            SubMenu.SetActive(false);

            if (SubMenuBackground)
            {
                SubMenuBackground.raycastTarget = false;
                SubMenuBackground.DOKill();
                Color color = SubMenuBackground.color;
                color.a = 0.0f;
                SubMenuBackground.DOColor(color, 0.1f).OnComplete(()=> {
                    SubMenuBackground.gameObject.SetActive(false);
                });
            }
        }
        else
        {
            Vector2 size = (SubMenu.transform as RectTransform).sizeDelta;
            size.y = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta.y - TopLine.sizeDelta.y;
            (SubMenu.transform as RectTransform).sizeDelta = size;

            //(SubBtnMenu.transform as RectTransform).localPosition = new Vector3(0, -TopLine.sizeDelta.y, 0);

            float time = 0.1f;
            SubMenu.SetActive(true);
            if (SubMenuBackground)
            {
                SubMenuBackground.raycastTarget = true;
                (SubMenuBackground.transform as RectTransform).sizeDelta = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
                SubMenuBackground.gameObject.SetActive(true);

                SubMenuBackground.DOKill();
                Color color = SubMenuBackground.color;
                color.a = 0.4f;

                SubMenuBackground.DOColor(color, 0.2f);
            }

            foreach (Transform child in SubMenu.transform)
            {
                child.DOKill();
                Vector2 pos = child.transform.localPosition;
                pos.x = sizeX * 2.0f;
                child.transform.localPosition = pos;
                child.DOLocalMoveX(0, 0.1f);//.SetDelay(time);
                //time += 0.1f;
            }
        }
    }
}
