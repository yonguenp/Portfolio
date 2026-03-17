using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class HolderPassObject : MonoBehaviour
    {
        [SerializeField]
        GameObject defaultLine = null;
        [SerializeField]
        Image progressImg = null;
        [SerializeField]
        GameObject progressImgBackObj = null;
        [SerializeField]
        Image nodeImg = null;
        [SerializeField]
        Sprite reachSprite = null;
        [SerializeField]
        Sprite unreachSprite = null;

        [SerializeField]
        Text progressNumText = null;

        [SerializeField]
        ItemFrame normalItem;
        [SerializeField]
        GameObject normalItemEffectObj = null;

        public int PassLevel { get; private set; } = 0;
        public eBattlePassRewardState NormalRewardState { get; private set; } = eBattlePassRewardState.REWARD_ABLE;
        public delegate void EachRewardGetCallBack(int lv);
        EachRewardGetCallBack rewardGetCallBack = null;

        PassItemData curPassData = null;
        public void Init(PassItemData passData, int currentLv, float gaugeValue ,  eBattlePassRewardState normalRewardState, bool isLastLine =false, EachRewardGetCallBack cb = null)
        {
            PassLevel = passData.LEVEL;
            if (PassLevel == 0)
            {
                progressImg.fillAmount = gaugeValue;
                GetComponent<RectTransform>().sizeDelta = new Vector2(60, 602.92f);
                nodeImg.gameObject.SetActive(false);
                normalItem.gameObject.SetActive(false);
                normalItemEffectObj.SetActive(false);
                return;
            }
            rewardGetCallBack = cb;
            
            curPassData = passData;
            nodeImg.sprite = currentLv >= PassLevel ? reachSprite : unreachSprite;
            progressNumText.text = PassLevel.ToString();
            NormalRewardState = normalRewardState;

            normalItem.SetFrameItem(passData.NormalReward);
            normalItem.SetTooltipShowAble();

            normalItem.setFrameCheck(false);
            normalItemEffectObj.SetActive(false);
            switch (normalRewardState)
            {
                case eBattlePassRewardState.REWARD_ABLE:
                        normalItemEffectObj.SetActive(true);
                        //normalItem.setCallback((itemID) =>
                        //{
                        //    GetRewardItemOnce(PassLevel, true, () =>
                        //    {
                        //        normalItemEffectObj.SetActive(false);
                        //        normalItem.SetTooltipShowAble();
                        //        normalItem.setFrameCheck(true);
                        //        rewardGetCallBack?.Invoke(PassLevel);
                        //        SystemRewardPopup.OpenPopup(new List<Asset>() { passData.NormalReward });
                        //    });

                        //});
                    break;
                case eBattlePassRewardState.REWARDED:
                    normalItem.setFrameCheck(true);
                    break;
                case eBattlePassRewardState.REWARD_DISABLE:
                    break;
            }

            progressImg.fillAmount = gaugeValue;

            if (isLastLine)
            {
                defaultLine.SetActive(false);
                progressImgBackObj.SetActive(false);
            }
        }

        public void OnClickPass()
        {

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
        void GetRewardItemOnce(int passIndex, bool isSpecial, VoidDelegate cb = null)
        {
            return;
            List<Asset> AllItems = new List<Asset>();
            var passState = BattlePassManager.Instance.PassRewardStates;
            var spPassState = BattlePassManager.Instance.HolderPassRewardStates;
            if (passState[passIndex] == eBattlePassRewardState.REWARD_ABLE)
                AllItems.Add(curPassData.NormalReward);
            if (User.Instance.CheckInventoryGetItem(AllItems))
            {
                IsFullBagAlert();
                return;
            }

            Debug.LogError("holder req");
            //holder 패스 제거
            //WWWForm param = new WWWForm();
            //param.AddField("level", passIndex);
            //NetworkManager.Send("pass/holder???", param, (JObject jsonData) =>
            //{
            //    if (jsonData.ContainsKey("rs") && (int)jsonData["rs"] == (int)eApiResCode.OK)
            //    {
            //        cb?.Invoke();
            //    }
            //});
        }




    }



}
