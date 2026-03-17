using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTabBtn : MonoBehaviour
{
    public enum TabType
    {
        Item,
        Emotion,
        Equip,
        Piece,
    }
    [SerializeField] TabType type;
    [SerializeField] Image tabImage;
    [SerializeField] Text tabText;
    [SerializeField] Toggle toggle;
    [SerializeField] InventoryPanel panel;
    public void TabBtn()
    {
        if (toggle.isOn)
        {
            tabImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_01");
            tabText.color = Color.black;
        }
        else
        {
            tabImage.sprite = Managers.Resource.Load<Sprite>("Texture/UI/clan/clan_tab_02");
            tabText.color = Color.white;
        }


        if (toggle.isOn)
        {
            panel.gameObject.SetActive(true);
            switch (type)
            {
                case TabType.Item:
                    (panel as InventoryPopup).SetFilter(CombineInventoryPopup.GetItemList(type));
                    break;
                case TabType.Piece:
                    (panel as InventoryPopup).SetFilter(CombineInventoryPopup.GetItemList(type));
                    break;
            }

            panel.RefreshUI();
        }
        else
            panel.gameObject.SetActive(false);
        
        panel.transform.SetAsLastSibling();

    }

}
