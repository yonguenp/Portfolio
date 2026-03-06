using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class BattlePassObject : MonoBehaviour
    {
        [SerializeField]
        GameObject defaultLine = null;
        [SerializeField]
        Image progressImg = null;
        [SerializeField]
        Text progressNumText = null;
        [SerializeField]
        Image nodeImg = null;
        [SerializeField]
        Sprite reachSprite = null;
        [SerializeField]
        Sprite unreachSprite = null;

        [SerializeField]
        ItemFrame normalItem;
        [SerializeField]
        GameObject normalItemEffectObj = null;

        [SerializeField]
        ItemFrame specialItem;
        [SerializeField]
        GameObject specialItemEffectObj = null;


        public int PassLevel { get; private set; } = 0;
        public eBattlePassRewardState NormalRewardState { get; private set; } = eBattlePassRewardState.REWARD_ABLE;
        public eBattlePassRewardState SpecialRewardState { get; private set; } = eBattlePassRewardState.REWARD_ABLE;

        bool isHolder = false;

        public delegate void EachRewardGetCallBack(int lv, int special);

        EachRewardGetCallBack rewardGetCallBack = null;

        PassItemData curPassData = null;
        public void Init(PassItemData passData, int currentLv ,float gaugeValue ,  eBattlePassRewardState normalRewardState, eBattlePassRewardState specialRewardState, bool isLastLine = false, EachRewardGetCallBack callBack = null)
        {
            PassLevel = passData.LEVEL;
            if (PassLevel == 0)
            {
                SetEmptyLine(gaugeValue);
                return;
            }
            
            curPassData = passData;
            nodeImg.sprite = currentLv>=PassLevel ? reachSprite : unreachSprite;
            progressNumText.text = PassLevel.ToString();
            NormalRewardState = normalRewardState;
            SpecialRewardState = specialRewardState;

            isHolder = User.Instance.IS_HOLDER;

            normalItem.SetFrameItem(passData.NormalReward);
            normalItem.setFrameCheck(true);
            normalItemEffectObj.SetActive(false);
            normalItem.SetTooltipShowAble();
            switch (normalRewardState)
            {
                case eBattlePassRewardState.REWARD_ABLE:
                    normalItem.setFrameCheck(false);
                    normalItemEffectObj.SetActive(true);
                    // 서버 리스폰스 받고~
                    //normalItem.setCallback((itemID) =>
                    //{
                    //    GetRewardItemOnce(PassLevel, false, () =>
                    //    {
                    //        normalItemEffectObj.SetActive(false);
                    //        normalItem.SetTooltipShowAble();
                    //        normalItem.setFrameCheck(true);
                    //        normalRewardState = eBattlePassRewardState.REWARDED;
                    //        rewardGetCallBack?.Invoke(PassLevel, 0);
                    //        SystemRewardPopup.OpenPopup(new List<Asset>() { passData.NormalReward });
                    //    });
                    //});
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
            switch (specialRewardState)
            {
                case eBattlePassRewardState.REWARD_ABLE:
                    specialItem.setFrameCheck(false);
                    specialItem.SetFrameItem(isHolder ? passData.HolderReward : passData.SpecialReward);
                    specialItemEffectObj.SetActive(true);
                    // 서버 리스폰스 받고~
                    //specialItem.setCallback((itemID) =>
                    //{
                    //    GetRewardItemOnce(PassLevel, true, () =>
                    //    {
                    //        specialItemEffectObj.SetActive(false);
                    //        specialItem.SetTooltipShowAble();
                    //        specialItem.setFrameCheck(true);
                    //        specialRewardState = isHolder ? eBattlePassRewardState.REWARDED_HOLDER : eBattlePassRewardState.REWARDED;
                    //        rewardGetCallBack?.Invoke(PassLevel, 1);
                    //        SystemRewardPopup.OpenPopup(new List<Asset>() { isHolder ? passData.HolderReward : passData.SpecialReward });
                    //    });
                    //});
                    break;
                case eBattlePassRewardState.REWARD_DISABLE:
                    specialItem.SetFrameItem(isHolder ? passData.HolderReward : passData.SpecialReward);
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
                case eBattlePassRewardState.REWARDED_HOLDER:
                    specialItem.SetFrameItem(passData.HolderReward);
                    break;
            }



            progressImg.fillAmount = gaugeValue;

            if (isLastLine)
            {
                defaultLine.SetActive(false);
                progressImg.gameObject.SetActive(false);
            }
        }

        void SetEmptyLine(float lineGauge)
        {
            var backLine = defaultLine.GetComponent<RectTransform>();
            var frontLine = progressImg.GetComponent<RectTransform>();
            progressImg.fillAmount = lineGauge;
            GetComponent<RectTransform>().sizeDelta = new Vector2(60, 530f);
            backLine.sizeDelta = frontLine.sizeDelta = new Vector2(200, 60);
            //backLine.localPosition = frontLine.localPosition = new Vector2(-60, 0);
            nodeImg.gameObject.SetActive(false);
            normalItem.gameObject.SetActive(false);
            normalItemEffectObj.SetActive(false);
            specialItem.gameObject.SetActive(false);
            specialItemEffectObj.SetActive(false);
        }
        void GetRewardItemOnce(int passIndex, bool isSpecial, VoidDelegate cb = null)
        {
            return;
            List<Asset> AllItems = new List<Asset>();
            var passState = BattlePassManager.Instance.PassRewardStates;
            var spPassState = BattlePassManager.Instance.HolderPassRewardStates;
            if (passState[passIndex] == eBattlePassRewardState.REWARD_ABLE)
                AllItems.Add(curPassData.NormalReward);
            if (passState[passIndex] == eBattlePassRewardState.REWARD_ABLE)
                AllItems.Add(curPassData.HolderReward);
            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                IsFullBagAlert();
                return;
            }
            WWWForm param = new WWWForm();
            param.AddField("level", passIndex);
            param.AddField("type",isSpecial ? 2: 1);
            NetworkManager.Send("pass/???", param, (JObject jsonData) =>
            {
                if (jsonData.ContainsKey("rs") && (int)jsonData["rs"] == (int)eApiResCode.OK)
                {
                    cb?.Invoke();
                }
            });
        }
        void IsFullBagAlert()
        {
            SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077), StringData.GetStringByIndex(100000414), "",
                () => {
                    //메인팝업 열기
                    PopupManager.OpenPopup<InventoryPopup>();
                    PopupManager.ClosePopup<PostListPopup>();
                }, () => { }, () => { });
        }
        public void OnClickPass()
        {

        }





    }



}
