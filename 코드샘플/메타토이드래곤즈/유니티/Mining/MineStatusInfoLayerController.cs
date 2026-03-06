using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum eMineStatusTextType
    {
        NONE,
        MINE_LEVEL,
        MINE_AMOUNT,
        MINE_TIME,
        MINE_DURATION,
        MINE_BOOST_ITEM,
    }


    public class MineStatusInfoLayerController : MonoBehaviour
    {
        [SerializeField] Text totalFormulaText = null;//총채굴량 공식
        [SerializeField] Text totalExpectMiningText = null;//총예상채굴량 값

        [SerializeField] List<MineStatusInfoSlot> textList = new List<MineStatusInfoSlot>();


        MineDrillData mineDrillData = null;
        ProductData productData = null;

        bool isVisible = false;

        /// <summary>
        /// 진입 시점 - 1렙 이하의 상태 (레벨이 0이면)면 무조건 진입 못하게 세팅
        /// </summary>
        public void OnClickShowUI()
        {
            var buildingInfo = MiningManager.Instance.MineBuildingInfo;
            if (buildingInfo == null || buildingInfo.Level <= 0)
            {
                ToastManager.On(StringData.GetStringByStrKey("광산토스트7"));
                return;
            }

            if (isVisible)
            {
                HideUI();
                return;
            }

            //화면 열때 mine/state 갱신하고 열기
            MiningManager.Instance.RefreshMiningState(()=> {
                gameObject.SetActive(true);
                isVisible = true;
                SetData();
                SetTotalFomula();
                MiningPopupEvent.OpenStatusPanel();
            });
        }

        public void HideUI()
        {
            isVisible = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 기본 데이터 MiningManager을 통해서 가져와야함.
        /// </summary>
        void SetData()
        {
            var currentBoostItemList = MiningManager.Instance.UserMiningData.GetBoosterItemList();
            var currentItemCount = currentBoostItemList.Count;
            var currentMineLevel = MiningManager.Instance.MineBuildingInfo.Level;
            productData = ProductData.GetProductDataByGroupAndLevel(MiningManager.MINE_BUILDING_GROUP_KEY, currentMineLevel);
            mineDrillData = MineDrillData.GetMineDrillDataByLevel(currentMineLevel);

            if (productData == null || mineDrillData == null)
                return;

            if (textList == null || textList.Count <= 0)
                return;

            int boostItemCheckCount = 0;
            foreach(var textInfo in textList)
            {
                if (textInfo == null)
                    continue;

                var textType = textInfo.textType;
                if (textType == eMineStatusTextType.NONE)
                    continue;

                textInfo.gameObject.SetActive(true);

                switch (textType)
                {
                    case eMineStatusTextType.MINE_LEVEL:
                        textInfo.SetText(StringData.GetStringByStrKey("광산레벨"), StringData.GetStringFormatByStrKey("user_info_lv_02",currentMineLevel));
                        break;
                    case eMineStatusTextType.MINE_AMOUNT:
                        textInfo.SetText(StringData.GetStringByStrKey("기본채굴량"), productData.ProductItem.Amount.ToString());
                        break;
                    case eMineStatusTextType.MINE_TIME:
                        textInfo.SetText(StringData.GetStringByStrKey("채굴시간"), SBFunc.TimeString(productData.PRODUCT_TIME));
                        break;
                    case eMineStatusTextType.MINE_DURATION:
                        textInfo.SetText(StringData.GetStringByStrKey("드릴내구도"), mineDrillData.MINE_DURABILITY.ToString());
                        break;
                    case eMineStatusTextType.MINE_BOOST_ITEM:
                        if(currentItemCount > boostItemCheckCount)
                        {
                            var boostItemInfo = currentBoostItemList[boostItemCheckCount];
                            if(boostItemInfo == null)
                                textInfo.gameObject.SetActive(false);
                            else
                            {
                                string resultString;
                                if (!boostItemInfo.IsPercentType())
                                {
                                    var currentValue = boostItemInfo.BoostTableData.VALUE / SBDefine.Day;//(초 단위 연산)
                                    var currentProductData = MiningManager.Instance.GetProductData();
                                    float productTime = 0;
                                    if (currentProductData != null)
                                        productTime = currentProductData.PRODUCT_TIME;

                                    var resultValue = MathF.Ceiling(currentValue * productTime);
                                    resultString = SBFunc.StrBuilder("+", productTime == 0 ? 0 : resultValue);
                                }
                                else
                                    resultString = boostItemInfo.BoostTableData.VALUE_DESC;

                                textInfo.SetText(boostItemInfo.BaseData.NAME, resultString);
                            }
                            boostItemCheckCount++;
                        }
                        else
                            textInfo.gameObject.SetActive(false);

                        break;
                }
            }
        }

        void SetTotalFomula()
        {
            if (totalExpectMiningText == null || totalFormulaText == null)
                return;


            totalFormulaText.text = StringData.GetStringByStrKey("누적채굴량");
            totalExpectMiningText.text = MiningManager.Instance.UserMiningData.VALUE_DESC;
            return;


            if (productData == null)
                return;

            var preUnit = "(";
            var postUnit = ")";

            var expectTotalAmount = 1000.00f;//매니저쪽에서 값 땡겨오기
            var fomulaStartString = "총 예상 채굴량 : ";
            var fomulaBaseString = string.Format("1.0 x {0}", productData.ProductItem.Amount);//기본 채굴 공식

            bool hasPercent = false;
            bool hasPlus = false;

            string plusString = "";
            string percentString = "";

            var currentBoostItemList = new List<MineBoosterItem>();//현재 적용중인 부스터 아이템 리스트 가져와야함.
            foreach(var item in currentBoostItemList)
            {
                if (item == null)
                    continue;

                bool isPercent = item.IsPercentType();
                if (isPercent)
                {
                    hasPercent = true;
                    percentString = string.Format("x {0}" , item.BoostTableData.VALUE);
                }
                else
                {
                    hasPlus = true;
                    plusString = string.Format("+ {0}", item.BoostTableData.VALUE);
                }
            }

            if (hasPlus)
                fomulaBaseString = preUnit + fomulaBaseString + postUnit;

            fomulaBaseString += plusString;

            if(hasPercent)
                fomulaBaseString = preUnit + fomulaBaseString + postUnit;

            fomulaBaseString += percentString;

            totalFormulaText.text = fomulaStartString + fomulaBaseString;
            totalExpectMiningText.text = expectTotalAmount.ToString("F2");
        }
    }
}

