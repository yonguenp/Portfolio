using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class PassiveSkillPopup : Popup<PassiveSkillPopupData>
    {
        [Header("Normal Layer")]
        [SerializeField] Image commonItemIcon;
        [SerializeField] Text commonItemCountText;
        [SerializeField] Button commonSkillGetBtn;
        [SerializeField] Image commonNeedAssetIcon;
        [SerializeField] Text commonNeedAssetAmountText;
        [SerializeField] Image commonNeedAssetIcon2;
        [SerializeField] Text commonNeedAssetAmountText2;

        [Header("Special Layer")]
        [SerializeField] Image uncommonItemIcon;
        [SerializeField] Text uncommonItemCountText;
        [SerializeField] Button uncommonSkillGetBtn;
        [SerializeField] Image uncommonNeedAssetIcon;
        [SerializeField] Text uncommonNeedAssetAmountText;
        [SerializeField] Image uncommonNeedAssetIcon2;
        [SerializeField] Text uncommonNeedAssetAmountText2;

        VoidDelegate skillGetCallBack = null;

        int dragonNo = 0;
        int maxSlotCount = 0;
        eJobType Job = eJobType.NONE;
        SkillPassiveGroupData commonData = null;
        SkillPassiveGroupData uncommonData = null;

        Asset commonNeedItem = null;
        Asset uncommonNeedItem = null;
        int commonNeedGold = 0;
        int uncommonNeedGold = 0;
        bool isEnoughCommonItem = false;
        bool isEnoughUncommonItem = false;
        UserDragon dragon = null;
        public override void InitUI()
        {
            dragonNo = Data.dragonNo;
            maxSlotCount = Data.maxSkillSlot;
            dragon = User.Instance.DragonData.GetDragon(Data.dragonNo);
            Job = CharBaseData.Get(dragonNo).JOB;
            commonData = SkillPassiveGroupData.Get(Job, eSkillPassiveGroupType.COMMON);
            uncommonData = SkillPassiveGroupData.Get(Job, eSkillPassiveGroupType.UNCOMMON);
            if (commonData == null || uncommonData == null)
                return;
            SetItemData();
            RefreshMaterial();
        }

        public void SetSkillGetCallBack(VoidDelegate cb)
        {
            if (cb != null)
                skillGetCallBack = cb;
        }

        #region Button Click
        public void OnClickNormalMaterialMake()
        {
            var data = RecipeBaseData.GetRecipeData(commonNeedItem.ItemNo);
            if (data != null)//제작 팝업 오픈
            {
                int key = 0;
                int.TryParse(data.GetKey(), out key);
                if (key <= 0)
                    return;
                var popup = PopupManager.OpenPopup<ItemMakePopup>(new ItemMakePopupData(key, string.Empty));
                popup.SetRefreshCallBack(() =>
                {
                    RefreshMaterial();
                });
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("스킬재료획득팝업"),
                        () =>
                        {
                            LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.CloudAnimation);
                        },
                        null,
                        () => { }
                    , true, true, true);
            }

        }
        public void OnClickSpecialMaterialMake()
        {
            var data = RecipeBaseData.GetRecipeData(uncommonNeedItem.ItemNo);
            if (data != null)//제작 팝업 오픈
            {
                int key = 0;
                int.TryParse(data.GetKey(), out key);
                if (key <= 0)
                    return;
                var popup = PopupManager.OpenPopup<ItemMakePopup>(new ItemMakePopupData(key, string.Empty));
                popup.SetRefreshCallBack(() =>
                {
                    RefreshMaterial();
                });
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("스킬재료획득팝업"),
                        () =>
                        {
                            LoadingManager.Instance.EffectiveSceneLoad("WorldBossLobby", eSceneEffectType.CloudAnimation);
                        },
                        null,
                        () => { }
                    , true, true, true);
            }
        }

        public void OnClickNormalSkillGet()
        {
            if (isEnoughCommonItem == false)
            {
                ToastManager.On(StringData.GetStringByIndex(100002249));
                return;
            }
            var curPassives = dragon.TranscendenceData.PassiveSkills;
            if (curPassives.Count == 0 || curPassives[0] == 0)
                SendSkillGet(ePassiveRefreshType.COMMON);
            else
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("스킬획득확인팝업"), ()=>  SendSkillGet(ePassiveRefreshType.COMMON),null,null, true, true, true);
        }

        public void OnClickSpecialSkillGet()
        {
            if (isEnoughUncommonItem == false)
            {
                ToastManager.On(StringData.GetStringByIndex(100002249));
                return;
            }
            var curPassives = dragon.TranscendenceData.PassiveSkills;
            if (curPassives.Count == 0 || curPassives[0] == 0)
                SendSkillGet(ePassiveRefreshType.UNCOMMON);
            else
                SystemPopup.OnSystemPopup(StringData.GetStringByStrKey("알림"), StringData.GetStringByStrKey("스킬획득확인팝업"), () => SendSkillGet(ePassiveRefreshType.UNCOMMON), null, null, true, true, true);
        }

        void SendSkillGet(ePassiveRefreshType type)
        {
            WWWForm param = new WWWForm();
            param.AddField("did", dragonNo);
            param.AddField("type", (int)type);
            NetworkManager.Send("dragon/refreshpassive", param, (JObject jsondata) =>
            {
                if (SBFunc.IsJTokenCheck(jsondata["rs"]) && jsondata["rs"].ToObject<int>() == (int)eApiResCode.OK)
                {
                    var popup = PopupManager.OpenPopup<PassiveSkillResultPopup>(new PassiveSkillResultPopupData(dragonNo, type == ePassiveRefreshType.COMMON ? int.Parse(commonData.KEY) : int.Parse(uncommonData.KEY), type));
                    popup.SetRetryCallBack(() =>
                    {
                        RefreshMaterial();
                        skillGetCallBack?.Invoke();
                    });
                    RefreshMaterial();
                    skillGetCallBack?.Invoke();
                }
            });
        }

        public void OnClickNormalGachaInfo()
        {
            if (commonData != null)
            {
                if (maxSlotCount > 0)
                {
                    var group1 = commonData.GROUP_ID_SLOT_1;
                    var group2 = commonData.GROUP_ID_SLOT_2;
                    PopupManager.OpenPopup<PassiveTablePopup>(new PassiveTablePopupData(group1, group2, maxSlotCount));
                }
            }
        }

        public void OnClickSpecialGachaInfo()
        {
            if (uncommonData != null)
            {
                if (maxSlotCount > 0)
                {
                    var group1 = uncommonData.GROUP_ID_SLOT_1;
                    var group2 = uncommonData.GROUP_ID_SLOT_2;
                    PopupManager.OpenPopup<PassiveTablePopup>(new PassiveTablePopupData(group1, group2, maxSlotCount));
                }
            }
        }

        #endregion
        /// <summary>
        /// 현재 내가 가진 아이템 갯수 및 골드 재화에 따라 리프레쉬
        /// </summary>
        public void RefreshMaterial()
        {
            var item1Count = User.Instance.GetItemCount(commonNeedItem.ItemNo);
            var item2Count = User.Instance.GetItemCount(uncommonNeedItem.ItemNo);
            var gold = User.Instance.GOLD;

            commonItemCountText.text = item1Count.ToString();
            commonItemCountText.color = item1Count >= commonNeedItem.Amount ? Color.white : Color.red;
            
            isEnoughCommonItem = item1Count >= commonNeedItem.Amount && gold >= commonNeedGold;
            commonNeedAssetIcon.sprite = commonNeedItem.BaseData.ICON_SPRITE;
            commonNeedAssetAmountText.text = SBFunc.CommaFromNumber(commonNeedItem.Amount);
            commonNeedAssetAmountText.color = item1Count >= commonNeedItem.Amount ? Color.white : Color.red;
            commonNeedAssetIcon2.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
            commonNeedAssetAmountText2.text = SBFunc.CommaFromNumber(commonNeedGold);
            commonNeedAssetAmountText2.color = gold >= commonNeedGold ? Color.white : Color.red;

            uncommonItemCountText.text = item2Count.ToString();
            uncommonItemCountText.color = item2Count >= uncommonNeedItem.Amount ? Color.white : Color.red;

            isEnoughUncommonItem = item2Count >= uncommonNeedItem.Amount && gold >= uncommonNeedGold;
            uncommonNeedAssetIcon.sprite = uncommonNeedItem.BaseData.ICON_SPRITE;
            uncommonNeedAssetAmountText.text = SBFunc.CommaFromNumber(uncommonNeedItem.Amount);
            uncommonNeedAssetAmountText.color = item2Count >= uncommonNeedItem.Amount ? Color.white : Color.red;
            uncommonNeedAssetIcon2.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
            uncommonNeedAssetAmountText2.text = SBFunc.CommaFromNumber(uncommonNeedGold);
            uncommonNeedAssetAmountText2.color = gold >= uncommonNeedGold ? Color.white : Color.red;

            commonSkillGetBtn.SetButtonSpriteState(isEnoughCommonItem);
            uncommonSkillGetBtn.SetButtonSpriteState(isEnoughUncommonItem);
        }

        void SetItemData()
        {
            commonNeedItem = commonData.NeedItem;
            commonNeedGold = commonData.NeedGold.Amount;
            commonNeedAssetAmountText.text = commonNeedGold.ToString();
            commonItemIcon.sprite = commonNeedItem.ICON;

            uncommonNeedItem = uncommonData.NeedItem;
            uncommonNeedGold = uncommonData.NeedGold.Amount;
            uncommonNeedAssetAmountText.text = uncommonNeedGold.ToString();
            uncommonItemIcon.sprite = uncommonNeedItem.ICON;
        }

    }

}