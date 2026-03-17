using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GemDungeonBoostItemClone : MonoBehaviour
    {
        [SerializeField] Image itemImg;
        [SerializeField] Text itemText;
        [SerializeField] Text itemAmountText;
        [SerializeField] Text itemInfoText;
        [SerializeField] Button itemUseBtn;

        public delegate void clickCallBack(int itemId);


        int itemKey = 0;
        public void Init(ItemBaseData itemDat, int boastAmount, clickCallBack cb )
        {

            itemKey = itemDat.KEY;
            itemImg.sprite = itemDat.ICON_SPRITE;
            itemText.text = itemDat.NAME;
            itemAmountText.text = string.Format("x{0}", boastAmount);
            itemInfoText.text = StringData.GetStringFormatByStrKey("부스터사용정보", itemDat.VALUE);
            itemUseBtn.onClick.RemoveAllListeners();
            itemUseBtn.onClick.AddListener(() => { cb(itemKey); });
        }

        public void OnClickItemIcon()
        {
            ToolTip.OnToolTip(new Asset(itemKey) , itemImg.gameObject);
        }

    }

}
