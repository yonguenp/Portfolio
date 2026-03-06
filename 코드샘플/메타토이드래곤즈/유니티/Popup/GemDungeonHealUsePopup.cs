using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GemDungeonHealUsePopup : Popup<GemDungeonHealPopupData>
    {
        [SerializeField] Image itemIcon;
        [SerializeField] Text itemCountText;
        [SerializeField] Button useBtn;

        int dragonNo = 0;
        int healItemCount = 0;

        int healItemKey = 0;
        public override void InitUI()
        {
            var healItem = ItemBaseData.GetItemListByKind(eItemKind.GEM_FATIGUE_RECOVERY);
            if (healItem != null && healItem.Count > 0)
            {
                itemIcon.sprite = healItem[0].ICON_SPRITE;
            }
            
            healItemCount = 0;
            foreach (var item in healItem)
            {
                healItemKey = item.KEY;
                int itemCount = User.Instance.GetItemCount(healItemKey);
                healItemCount += itemCount;
            }
            itemCountText.text = string.Format("x{0}", healItemCount);
            dragonNo = Data.dragonNo;
            if (dragonNo == 0 || healItemCount == 0)
            {
                useBtn.SetButtonSpriteState(false);
                useBtn.interactable = false;
            }
            else
            {
                useBtn.SetButtonSpriteState(true);
                useBtn.interactable = true;
            }
        }

        public void OnClickUseBtn()
        {
            if(healItemCount == 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("보유아이템없음"));
            }

            if (dragonNo == 0)
                return;
            
            WWWForm param = new WWWForm();
            param.AddField("item_no", healItemKey);
            param.AddField("item_count", 1);
            param.AddField("dragon_tags", string.Format("[{0}]", dragonNo));

            SoundManager.Instance.PlaySFX("sfx_part_reinforcesuccess");
            NetworkManager.Send("gemdungeon/recovery", param, (JObject json) =>
            {
                ToastManager.On(StringData.GetStringByStrKey("스태미나회복완료"));
                PopupManager.GetPopup<GemDungeonPopup>().Refresh();
                ClosePopup();

                foreach(var gd in Town.Instance.Gemdungeon)
                {
                    if (gd.Value != null)
                    {
                        if (gd.Value.DungeonStage != null)
                            gd.Value.DungeonStage.Refresh();
                    }
                }
            });
        }

        public void OnClickMakeBtn()
        {
            int key = 0;
            int.TryParse(RecipeBaseData.GetRecipeData(healItemKey).GetKey(), out key);
            var popup = PopupManager.OpenPopup<ItemMakePerfectPopup>(new ItemMakePopupData(key,StringData.GetStringByStrKey("드링크제작")));
            popup.SetRefreshCallBack(InitUI);
        }
    }
}

