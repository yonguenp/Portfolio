using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class DiceEventTabItem : MonoBehaviour
    {
        [SerializeField]
        Text TitleText = null;

        [SerializeField]
        Image bgImg = null;

        [SerializeField]
        Image IconImg = null;

        [SerializeField]
        GameObject selectedEffectObj = null;


        [Header("Normal State")]
        [SerializeField]
        Sprite NormalSprite = null;
        [SerializeField]
        Color NormalColor = Color.white;

        [Header("Selected State")]
        [SerializeField]
        Sprite[] SelectedSprite = null;
        [SerializeField]
        Color SelectedColor = Color.white;

        [SerializeField]
        GameObject reddot = null;

        public void SetReddotState(bool state)
        {
            reddot.SetActive(state);
        }
        public void SetSelectState(bool isSelect)
        {
            selectedEffectObj.SetActive(isSelect);

            if (isSelect)
            {
                bgImg.sprite = SelectedSprite[0];
                TitleText.color = SelectedColor;
            }
            else
            {
                bgImg.sprite = NormalSprite;
                TitleText.color = NormalColor;
            }
        }
    }
}
