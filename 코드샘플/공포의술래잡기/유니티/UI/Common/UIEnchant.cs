using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIEnchant : MonoBehaviour
{
    [SerializeField]
    GameObject[] enchantIcon;
    [SerializeField]
    GameObject[] bg;

    public GameObject[] EnchantIcon { get {return enchantIcon; }}

    Color originBackColor = Color.white;
    public void SetEnchant(int enchant)
    {
        for (int i = 0; i < enchantIcon.Length; i++)
        {
            Clear(i);

            if (i < enchant) 
                Fill(i);
        }
    }

    public void SetNextEnchant(int targetEnchant)
    {
        for (int i = 0; i < enchantIcon.Length; i++)
        {
            Clear(i);

            if (i < targetEnchant - 1)
                Fill(i);
            else if (i < targetEnchant)
                Focus(i);
        }
    }

    public void Fill(int index)
    {
        bg[index].SetActive(true);
        enchantIcon[index].transform.localScale = Vector3.one;
        enchantIcon[index].transform.localRotation = Quaternion.identity;
        enchantIcon[index].SetActive(true);
    }

    public void Focus(int index)
    {
        enchantIcon[index].transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);

        bg[index].SetActive(true);
        enchantIcon[index].SetActive(true);
    }

    public void Clear(int index)
    {
        enchantIcon[index].transform.DOKill();
        enchantIcon[index].transform.localScale = Vector3.one;

        bg[index].SetActive(true);
        enchantIcon[index].SetActive(false);
    }

    public void Show(bool isShow)
    {
        gameObject.SetActive(isShow);
    }
}
