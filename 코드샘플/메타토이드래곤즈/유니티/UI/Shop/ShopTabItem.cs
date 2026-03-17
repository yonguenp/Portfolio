using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ShopTabItem : MonoBehaviour
    {
        [SerializeField]
        Text TitleText = null;
        [SerializeField]
        Button tabBtn = null;

        [SerializeField]
        Image bgImg= null;

        [SerializeField]
        Image IconImg = null;

        //[SerializeField]
        //Image frameImg = null;

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




        //[Header("normal menu")]
        //[SerializeField]
        //Color selectColor;
        //[SerializeField]
        //Color noneSelectColor;
        //[SerializeField]
        //Color selectFrameColor;
        //[SerializeField]
        //Color noneFrameSelectColor;


        //[Header("asset menu")]
        //[SerializeField]
        //Color assetSelectColor;
        //[SerializeField]
        //Color assetNoneSelectColor;
        //[SerializeField]
        //Color assetFrameSelectColor;
        //[SerializeField]
        //Color assetNoneFrameSelectColor;

        //[Header("sold out menu")]
        //[SerializeField]
        //Color selectSoldOutColor;
        //[SerializeField]
        //Color noneSelectSoldOutColor;
        //[SerializeField]
        //Color selectSoldOutFrameColor;
        //[SerializeField]
        //Color noneSelectSoldOutFrameColor;

        [SerializeField]
        GameObject reddot = null;

        public int Index { get; private set; } = -1;
        public int ID { get; private set; } = -1;
        public int sort { private set; get; } = -1;
        public delegate void FuctionInt(int id);

        private eStoreType isAssetStore = eStoreType.SHOP;

        bool isSoldOut = false;
        //private Vector2 defaultSize = new Vector2(420, 175);
        //private Vector2 selectSize = new Vector2(420, 250);

        int tabColorIndex = 0;
        public void Init(ShopMenuData menuData, int index, FuctionInt buttonCallBack, bool reddotState = false)
        {
            TitleText.text = menuData.NAME;
            tabBtn.onClick.RemoveAllListeners();
            Index = index;
            ID = menuData.KEY;
            sort = menuData.SORT;
            isAssetStore = menuData.TYPE;
            isSoldOut =  ShopManager.Instance.IsSoldOutMenu(menuData.KEY);
            tabColorIndex = menuData.TabColor;
            if (menuData.ICON == null)
            {
                IconImg.gameObject.SetActive(false);
            }
            else
            {
                IconImg.gameObject.SetActive(true);
                IconImg.sprite = menuData.ICON;
            }

            SetSelectState(false);
            
            tabBtn.onClick.AddListener(() =>
            {
                buttonCallBack?.Invoke(Index);
                SetSelectState(true);
            });
            reddot.SetActive(reddotState);
        }

        public void SetReddotState (bool state)
        {
            reddot.SetActive(state);
        }
        public void SetSelectState(bool isSelect)
        {
            selectedEffectObj.SetActive(isSelect);



            
            if(isSelect)
            {
                bgImg.sprite = SelectedSprite[tabColorIndex];
                TitleText.color = SelectedColor;
            }
            else
            {
                bgImg.sprite = NormalSprite;
                TitleText.color = NormalColor;
            }


            //if(isSoldOut)
            //{
            //    if (isSelect)
            //    {
            //        tabBtn.GetComponent<Image>().color = selectSoldOutColor;
            //        frameImg.color = selectSoldOutFrameColor;
            //        GetComponent<RectTransform>().localScale = Vector3.one * 1.07f;
            //    }
            //    else
            //    {
            //        GetComponent<RectTransform>().localScale = Vector3.one;
            //        tabBtn.GetComponent<Image>().color = noneSelectSoldOutColor;
            //        frameImg.color = noneSelectSoldOutFrameColor;
            //    }
            //}
            //else
            //{
            //    if (isSelect)
            //    {
            //        tabBtn.GetComponent<Image>().color = isAssetStore == eStoreType.ASSET_STORE ? assetSelectColor : selectColor;
            //        frameImg.color = isAssetStore == eStoreType.ASSET_STORE ? assetFrameSelectColor : selectFrameColor;
            //        GetComponent<RectTransform>().localScale = Vector3.one * 1.07f;
            //    }
            //    else if (isAssetStore == eStoreType.ASSET_STORE)
            //    {
            //        GetComponent<RectTransform>().localScale = Vector3.one;
            //        tabBtn.GetComponent<Image>().color = assetNoneSelectColor;
            //        frameImg.color = assetNoneFrameSelectColor;
            //    }
            //    else
            //    {
            //        GetComponent<RectTransform>().localScale = Vector3.one;
            //        tabBtn.GetComponent<Image>().color = noneSelectColor;
            //        frameImg.color = noneFrameSelectColor;
            //    }
            //}
        }

    }
}
