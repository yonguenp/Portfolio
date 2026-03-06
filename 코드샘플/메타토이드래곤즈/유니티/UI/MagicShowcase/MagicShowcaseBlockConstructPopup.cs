using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    /// <summary>
    /// 각 상태 (제작 대기 / 제작 중 / 제작 완료) 에 따른 컴포넌트를 찢어서 따로 구현
    /// </summary>
    /// 제작 대기 노드 (defaultNode)- MagicShowcaseBlockConstructDefaultUI
    /// 제작 중 노드 (productingNode)- MagicShowcaseBlockConstructProductingUI
    /// 제작 완료 노드 (resultNode)- MagicShowcaseBlockConstructResultUI
    public struct MagicShowcaseBlockConstructEvent
    {
        public enum MagicShowcaseBlockConstructEventEnum
        {
            InitUI,//초기값 세팅

            StartingProducting,//제작중

            ShowResult,//결과 UI
            
        }

        public MagicShowcaseBlockConstructEventEnum Event;
        static MagicShowcaseBlockConstructEvent e;

        public JObject resultServerData;
        public int recipeKey;
        public int recipeAmount;

        public MagicShowcaseBlockConstructEvent(MagicShowcaseBlockConstructEventEnum _Event, int _recipeKey, int _recipeAmount, JObject _resultServerData)
        {
            Event = _Event;
            recipeKey = _recipeKey;
            recipeAmount = _recipeAmount;
            resultServerData = _resultServerData;
        }
        public static void InitUI()
        {
            e.Event = MagicShowcaseBlockConstructEventEnum.InitUI;
            EventManager.TriggerEvent(e);
        }
        public static void StartingProducting(int _recipeKey, int _recipeAmount)
        {
            e.Event = MagicShowcaseBlockConstructEventEnum.StartingProducting;
            e.recipeKey = _recipeKey;
            e.recipeAmount = _recipeAmount;
            EventManager.TriggerEvent(e);
        }
        public static void ShowResult(JObject _resultServerData)
        {
            e.Event = MagicShowcaseBlockConstructEventEnum.ShowResult;
            e.resultServerData = _resultServerData;
            EventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 마법진열장 - 블록 제작 요청 팝업
    /// </summary>
    /// 
    public class MagicShowcaseBlockConstructPopupData : PopupData
    {
        private RecipeBaseData resultTableData = null;
        public RecipeBaseData ResultTableData { get { return resultTableData; } }
        public Action Callback { get; private set; } = null;
        public MagicShowcaseBlockConstructPopupData(RecipeBaseData _resultData, Action cb = null)
        {
            resultTableData = _resultData;
            Callback = cb;
        }
    }

    public class MagicShowcaseBlockConstructPopup : Popup<MagicShowcaseBlockConstructPopupData>, EventListener<MagicShowcaseBlockConstructEvent>
    {
        [SerializeField] MagicShowcaseBlockConstructDefaultUI defaultUI = null;
        [SerializeField] MagicShowcaseBlockConstructProductingUI productingUI = null;
        [SerializeField] MagicShowcaseBlockConstructResultUI resultUI = null;

        [SerializeField] Text percentLabel = null;
        [SerializeField] Text guideLabel = null;

        private RecipeBaseData recipeData = null;

        #region OpenPopup
        public static MagicShowcaseBlockConstructPopup OpenPopup(RecipeBaseData _resultData, Action cb = null)
        {
            return OpenPopup(new MagicShowcaseBlockConstructPopupData(_resultData, cb));
        }
        public static MagicShowcaseBlockConstructPopup OpenPopup(MagicShowcaseBlockConstructPopupData data)
        {
            if (data == null)
                return null;

            return PopupManager.OpenPopup<MagicShowcaseBlockConstructPopup>(data);
        }
        #endregion
        public void OnEnable()
        {
            EventManager.AddListener(this);
        }
        private void OnDisable()
        {
            EventManager.RemoveListener(this);
        }
        public void OnEvent(MagicShowcaseBlockConstructEvent eventType)
        {
            switch (eventType.Event)
            {
                case MagicShowcaseBlockConstructEvent.MagicShowcaseBlockConstructEventEnum.InitUI:
                {
                    if (productingUI != null)
                        productingUI.SetVisible(false);
                    if (resultUI != null)
                        resultUI.SetVisible(false);

                    if (defaultUI != null)
                    {
                        defaultUI.SetVisible(true);
                        defaultUI.InitUI(recipeData);
                    }
                }
                break;
                case MagicShowcaseBlockConstructEvent.MagicShowcaseBlockConstructEventEnum.StartingProducting:
                {
                    if (defaultUI != null)
                        defaultUI.SetVisible(false);
                    if (resultUI != null)
                        resultUI.SetVisible(false);

                    if (productingUI != null)
                    {
                        productingUI.SetVisible(true);
                        productingUI.InitUI(eventType.recipeKey,eventType.recipeAmount);
                    }
                }
                break;
                case MagicShowcaseBlockConstructEvent.MagicShowcaseBlockConstructEventEnum.ShowResult:
                {
                    if (defaultUI != null)
                        defaultUI.SetVisible(false);
                    if (productingUI != null)
                        productingUI.SetVisible(false);
                    if (resultUI != null)
                    {
                        resultUI.SetVisible(true);
                        resultUI.InitUI(eventType.resultServerData);
                    }
                }
                break;
            }
        }
        public override void InitUI()
        {
            recipeData = Data.ResultTableData;
            if (recipeData == null)
                return;

            SetDefaultUI();
            MagicShowcaseBlockConstructEvent.InitUI();
        }

        void SetDefaultUI()
        {
            if(percentLabel != null)
            {
                var calc_percent = (recipeData.RATE / (float)SBDefine.MILLION * 100).ToString();
                percentLabel.text = SBFunc.StrBuilder(calc_percent, "%");
            }

            if(guideLabel != null)
            {
                var rewardItem = recipeData.REWARD_ITEM_LIST[0];
                var failItem = recipeData.FAIL_ITEM_LIST[0];

                var rewardItemName = rewardItem.BaseData.NAME;
                var failItemName = failItem.BaseData.NAME;
                var colorStrReward = SBFunc.StrBuilder("<color=#03B027>", rewardItemName , "</color>");
                var colorStrFail = SBFunc.StrBuilder("<color=#D41F2C>", failItemName, "</color>");
                guideLabel.text = StringData.GetStringFormatByStrKey("블록제작가이드", colorStrReward, colorStrFail);
            }
        }
    }
}
