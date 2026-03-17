using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPortraitCharacter : MonoBehaviour
{
    //[Serializable]
    //public class PortraitSprites
    //{
    //    public string key;
    //    public Sprite target;
    //}

    //[SerializeField]
    //PortraitSprites[] Portraits;

    [SerializeField]
    Image portraitImage;

    public void SetPortrait(int type)
    {
        string key = CharacterGameData.GetUIResourceName(type);
        if (string.IsNullOrEmpty(key))
            return;

        portraitImage.gameObject.SetActive(true);
        portraitImage.enabled = true;


        portraitImage.sprite = CharacterGameData.GetCharacterData(type).sprite_ui_resource;
    }

    public void SetItem(int type)
    {
        Sprite icon = ItemGameData.GetItemIcon(type);
        if (icon == null)
            return;

        portraitImage.gameObject.SetActive(true);
        portraitImage.enabled = true;

        portraitImage.transform.localPosition = Vector3.zero;
        (portraitImage.transform as RectTransform).sizeDelta = (portraitImage.transform.parent as RectTransform).sizeDelta;

        portraitImage.sprite = icon;
    }

    public void SetPortrait(string key)
    {
        //portraitImage.sprite = Resources.Load<Sprite>(key);
        //foreach (PortraitSprites info in Portraits)
        //{
        //    if (info.key == key)
        //    {
        //        portraitImage.sprite = info.target;
        //        return;
        //    }
        //}
    }

    public void SetColor(Color color)
    {
        portraitImage.DOKill();
        portraitImage.color = color;
    }

    public void SetColorWithTween(Color color)
    {
        portraitImage.DOKill();
        Color origin = portraitImage.color;
        portraitImage.color = color;
        portraitImage.DOColor(origin, 0.5f);
    }
    public void Clear()
    {
        portraitImage.sprite = null;
    }
}
