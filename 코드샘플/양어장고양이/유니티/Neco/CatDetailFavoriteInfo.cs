using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatDetailFavoriteInfo : MonoBehaviour
{

    [Header("[Discover Favorite]")]
    public GameObject discoverObjectLayer;

    public Image favoriteIcon;
    public Text favoriteName;

    [Header("[UnKnown Favorite]")]
    public GameObject unknownObjectLayer;

    neco_cat curCataData;

    public void SetCatDetailFavoriteInfo(neco_cat catData)
    {
        if (catData == null) { return; }

        discoverObjectLayer.SetActive(false);
        unknownObjectLayer.SetActive(false);

        curCataData = catData;

        // 캣 인포에서 고양이 오픈 Ani를 보지 않을 경우 정보표시X
        bool isCatInfoOpen = curCataData.GetCatState() >= 3;

        uint itemID = catData.GetLikeObject();

        if (itemID == 0 || isCatInfoOpen == false)
        {
            unknownObjectLayer.SetActive(true);
        }
        else
        {
            discoverObjectLayer.SetActive(true);

            object obj;
            items itemData = items.GetItem(itemID);
            favoriteIcon.sprite = itemData.GetItemIcon();

            favoriteIcon.color = user_items.GetUserItemAmount(itemID) > 0 ? Color.white : Color.black;

            favoriteName.text = itemData.GetItemName();
        }
    }
}
