using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class LevelPassObject : MonoBehaviour
    {
        [SerializeField]
        Text levelText = null;
        
        [SerializeField]
        ItemFrame normalItem;
        [SerializeField]
        GameObject normalItemEffectObj = null;

        [SerializeField]
        ItemFrame specialItem;
        [SerializeField]
        GameObject specialItemEffectObj = null;

        [SerializeField] Image levelTargetImage = null;
        [SerializeField] Sprite rewardedSprite = null;
        [SerializeField] Sprite unrewardSprite = null;


        public int PassLevel { get; private set; } = 0;
        public eBattlePassRewardState NormalRewardState { get; private set; } = eBattlePassRewardState.REWARD_ABLE;
        public eBattlePassRewardState SpecialRewardState { get; private set; } = eBattlePassRewardState.REWARD_ABLE;

        AccountData data = null;

        public void Init(AccountData passData, eBattlePassRewardState normalRewardState, eBattlePassRewardState specialRewardState)
        {
            if (passData == null)
                return;

            data = passData;

            PassLevel = data.LEVEL;
            if (PassLevel <= 0)
                return;

            NormalRewardState = normalRewardState;
            SpecialRewardState = specialRewardState;

            if (levelText != null)
                levelText.text = PassLevel.ToString();

            normalItem.SetFrameItem(passData.NormalReward);
            normalItem.setFrameCheck(true);
            normalItemEffectObj.SetActive(false);
            normalItem.SetTooltipShowAble();
            switch (NormalRewardState)
            {
                case eBattlePassRewardState.REWARD_ABLE:
                    normalItem.setFrameCheck(false);
                    normalItemEffectObj.SetActive(true);
                    break;
                case eBattlePassRewardState.REWARD_DISABLE:
                    normalItem.setFrameCheck(false);
                    break;
                case eBattlePassRewardState.REWARDED:
                    break;
            }

            specialItem.SetLockIcon(false);
            specialItem.setFrameCheck(true);
            specialItemEffectObj.SetActive(false);
            specialItem.SetTooltipShowAble();
            SetSpecialRewardCallback();
            switch (SpecialRewardState)
            {
                case eBattlePassRewardState.REWARD_ABLE:
                    specialItem.setFrameCheck(false);
                    specialItem.SetFrameItem(passData.SpecialReward);
                    specialItemEffectObj.SetActive(true);
                    break;
                case eBattlePassRewardState.REWARD_DISABLE:
                    specialItem.SetFrameItem(passData.SpecialReward);
                    specialItem.setFrameCheck(false);
                    break;
                case eBattlePassRewardState.LOCK:
                    specialItem.setFrameCheck(false);
                    specialItem.SetFrameItem(passData.SpecialReward);
                    specialItem.SetLockIcon(true);
                    break;
                case eBattlePassRewardState.REWARDED:
                    specialItem.SetFrameItem(passData.SpecialReward);
                    break;
            }

            RefreshLevelButtonSprite();
            ShowDragonPortraitSpine(passData.SpecialReward.ItemNo);
        }
        public void SetSpecialRewardCallback()
        {
            if (data == null || data.SpecialReward == null)
                return;

            if (data.SpecialReward.GoodType != eGoodType.CHARACTER)
                return;

            specialItem.setCallback((itemID) => {

                if (int.TryParse(itemID, out int result))
                {
                    var levelpopup = PopupManager.GetPopup<LevelPassPopup>();
                    levelpopup.SetExitCallback(() => { });

                    levelpopup.ClosePopup();

                    var popup = DragonManagePopup.OpenPopup(0, 1);
                    PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag = true;
                    popup.SetExitCallback(() => {
                        DragonChangedEvent.Refresh();

                        if(PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag == false)
                            LevelPassPopup.OpenPopup();//끌때 다시 켜기
                    });
                    popup.CurDragonTag = result;
                    popup.ClearDragonInfoList();

                    popup.DragonInfoList.AddRange(new List<int>() { result });
                    popup.ForceUpdate();
                }
            });
        }

        void RefreshLevelButtonSprite()
        {
            var isRewardCondition = NormalRewardState == eBattlePassRewardState.REWARDED || NormalRewardState == eBattlePassRewardState.REWARD_ABLE;

            if (levelTargetImage != null)
                levelTargetImage.sprite = isRewardCondition ? rewardedSprite : unrewardSprite;
        }

        void ShowDragonPortraitSpine(int _itemID)
        {
            if (data == null || data.SpecialReward == null)
                return;

            if (data.SpecialReward.GoodType != eGoodType.CHARACTER)
                return;

            specialItem.SetDragonPortraitSpine(_itemID);
        }
    }
}
