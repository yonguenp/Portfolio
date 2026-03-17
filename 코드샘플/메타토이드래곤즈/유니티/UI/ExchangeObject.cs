using Coffee.UIEffects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ExchangeObject : MonoBehaviour
    {
        [SerializeField] GameObject beggingLayer = null;
        [SerializeField] GameObject waitLayer = null;
        [SerializeField] GameObject coverLayer = null;

        [Header("Begging Layer")]
        [SerializeField] ItemFrame[] items = null;
        [SerializeField] ItemFrame[] rewards = null;
        [SerializeField] Button exchangeButton = null;
        [SerializeField] Image dragonImg = null;
        //[SerializeField] Image dragonBackImg = null;
        [SerializeField] Text dragonName = null;

        [SerializeField] Animation effectAnim =  null;

        [Header("Wait Layer")]
        [SerializeField] Text waitTimerText = null;
        [SerializeField] TimeObject timeObj = null;
        [SerializeField] Text remainTime = null;

        [Header("Cover Layer")]
        [SerializeField] Image coverPortrait = null;

        private Exchange data = null;
        private ExchangeLayer parentLayer =null;

        public delegate void OnExchangeCallback(Newtonsoft.Json.Linq.JToken data);


        bool enableExchange = true;
        bool buttonLock { get { return parentLayer.buttonLock; } }

        private void OnEnable()
        {
            //ClearLayer();
            //coverLayer.SetActive(true);
            if (effectAnim != null)
            {
                //effectAnim.SetBool("defaultState", true);
                effectAnim.Stop();
                effectAnim.Play("ExchangeDefault");

                isExChangeInAnim = false;
            }
        }

        public void SetDefaultOff()
        {
            //effectAnim.SetBool("defaultState", false);
        }
        void ClearLayer()
        {
            beggingLayer.SetActive(false);
            waitLayer.SetActive(false);
            coverLayer.SetActive(false);
            timeObj.Refresh = null;

            ButtonLock();
        }

        void ButtonLock()
        {
            if (parentLayer != null) 
                parentLayer.ButtonLock();
        }

        public void SetDiscoverLayer()
        {
            ClearLayer();
            coverLayer.SetActive(true);

            CharBaseData baseData = CharBaseData.Get(data.dragon_no);
            if (baseData != null)
                coverPortrait.sprite = baseData.GetThumbnail();
            else
                coverPortrait.sprite = null;
        }
        public void SetBeggingLayer()
        {
            ClearLayer();
            beggingLayer.SetActive(true);

            CharBaseData baseData = CharBaseData.Get(data.dragon_no);
            if (baseData != null)
            {
                dragonImg.sprite = baseData.GetThumbnail();
                //dragonBackImg.sprite = GetBGIconSpriteByIndex(baseData);
                dragonName.text = string.Format(StringData.GetStringByIndex(100002252), StringData.GetStringByStrKey(baseData._NAME));
            }
            else
            {
                dragonImg.sprite = null;
                dragonName.text = "";
            }

            Dictionary<int, Item> needitems = new Dictionary<int, Item>();
            int rewardGold = 0;
            int rewardExp = 0;

            for (int i = 0; i < 4; i++)
            {
                items[i].gameObject.SetActive(false);
                if (data.NeedItemInfo.Count > i)
                {
                    int groupNo = data.NeedItemInfo[i].GROUP;
                    ExchangeData exchangeData = null;
                    
                    if( parentLayer != null)
                    {
                        exchangeData = parentLayer.Exchange.GetExchangeData(groupNo);
                    }

                    if(exchangeData != null)
                    {
                        int required = data.NeedItemInfo[i].REQUIRED_NUMBER;
                        rewardGold += data.NeedItemInfo[i].REWORD_GOLD;
                        rewardExp += data.NeedItemInfo[i].REWORD_EXP;
                        if (needitems.ContainsKey(exchangeData.NEED_ITEM.KEY))
                            needitems[exchangeData.NEED_ITEM.KEY].AddCount(required);
                        else
                            needitems[exchangeData.NEED_ITEM.KEY] = new Item(exchangeData.NEED_ITEM.KEY, required);
                    }
                }
            }
            int itemIndex = 0;
            foreach( var itemInfo in needitems)
            {
                ItemFrame item = items[itemIndex++];
                item.setFrameRecipeInfo(itemInfo.Key, itemInfo.Value.Amount);
                // item.SetFrameFuncNone();
                item.gameObject.SetActive(true);
            }
            

            enableExchange = true;
            foreach (var need in needitems)
            {
                enableExchange &= User.Instance.GetItemCount(need.Key) >= need.Value.Amount;
            }

            //exchangeButton.interactable = enable;
            exchangeButton.SetButtonSpriteState(enableExchange);

            rewards[0].SetFrameItem(0, rewardGold, (int)eGoodType.GOLD);
            rewards[1].SetFrameItem(0, rewardExp, (int)eGoodType.ACCOUNT_EXP);
            // rewards[0].SetFrameFuncNone();
            // rewards[1].SetFrameFuncNone();
            var UIShiny = rewards[0].GetIconImage().GetComponent<UIShiny>();
            if (UIShiny != null)
            {
                UIShiny.effectPlayer.initialPlayDelay = 0.5f + SBFunc.RandomValue * 10.0f;
                UIShiny.effectPlayer.loopDelay = 0.5f + SBFunc.RandomValue * 10.0f;
            }

            UIShiny = rewards[1].GetIconImage().GetComponent<UIShiny>();
            if (UIShiny != null)
            {
                UIShiny.effectPlayer.initialPlayDelay = 0.5f + SBFunc.RandomValue * 10.0f;
                UIShiny.effectPlayer.loopDelay = 0.5f + SBFunc.RandomValue * 10.0f;
            }
        }

        public void SetWaitLayer()
        {
            ClearLayer();
            waitLayer.SetActive(true);
            if (timeObj.Refresh == null)
            {
                timeObj.Refresh = () =>
                {
                    float remainTime = TimeManager.GetTimeCompare(data.regist_time);
                    waitTimerText.text = SBFunc.TimeString(remainTime);
                    if (remainTime < 0)  //임시코드 서버 구축시 체크
                    {
                        if(parentLayer != null)
                        {
                            if(!parentLayer.Exchange.Requesting)
                                parentLayer.Exchange.Prepare(parentLayer.DataRefresh);
                        }
                    }
                };
            }
        }
        private Sprite GetBGIconSpriteByIndex(CharBaseData dragonInfo)
        {
            if (dragonInfo == null)
                return null;

            return dragonInfo.GetBackGround();
        }

        public void SetData(ExchangeLayer popup, Exchange _data)
        {
            parentLayer = popup;
            data = _data;

            ClearLayer();

            if (data == null)
                return;

            isExChangeInAnim = false;
            LandmarkUpdateEvent.Send(eLandmarkType.EXCHANGE);
            switch (data.State)
            {
                case Exchange.EXCHANGE_STATE.HIDE:
                    SetDiscoverLayer();
                    isExChangeInAnim = true;
                    break;
                case Exchange.EXCHANGE_STATE.BEGGING:
                    SetBeggingLayer();
                    break;
                case Exchange.EXCHANGE_STATE.WAIT:
                    SetWaitLayer();
                    break;
            }
        }

        public bool IsBeggingAble()
        {
            return data.State != Exchange.EXCHANGE_STATE.HIDE;
        }

        public void OnRefresh()
        {
            if(parentLayer != null)
            {
                parentLayer.Exchange.OnRefresh(data.slot_id, parentLayer.DataRefresh);
            }
            
        }

        public void OnExchange()
        {
            if (buttonLock)
                return;

            if (enableExchange == false)
            {
                var needItemList = GetNeedItemList();
                if (needItemList.Count <= 0)
                    return;

                ProductsBuyNowPopup.OpenPopup(needItemList, () => {
                    parentLayer.DataRefresh();
                    parentLayer.ButtonUnlock();
                    OnExchange();//재시도
                },false,(index)=> {
                    parentLayer.DataRefresh();
                });

                return;
            }
            effectAnim.Stop();
            effectAnim.Play("ExchangeOut");
            ButtonLock();
        }

        public void NewRequestCome(JToken response)
        {
            if (response == null)
            {
                OnRefresh();
                return;
            }

            JObject data = (JObject)response;
            List<Asset> rewards = new List<Asset>();
            if (data.ContainsKey("exchange"))
            {
                JArray exchange = (JArray)data["exchange"];
                
                foreach (JObject exData in exchange)
                {
                    if (exData.ContainsKey("reward"))
                    {
                        foreach (var reward in (JArray)exData["reward"])
                        {
                            rewards.Add(new Asset(reward));
                        }
                    }

                    exData.Remove("reward");
                }
            }

            if (rewards.Count > 0)
            {
                SystemRewardPopup.OpenPopup(rewards, () => {
                    parentLayer.Exchange.OnExchangeData(data, () => {
                        parentLayer.DataRefresh();
                        isExChangeInAnim = false;
                        effectAnim.Stop();
                        effectAnim.Play("ExchangeIn");
                        ButtonLock();
                    });
                }, true);
            }
            else
            {
                parentLayer.Exchange.OnExchangeData(data, () => {
                    parentLayer.DataRefresh();
                    isExChangeInAnim = false;
                    effectAnim.Stop();
                    effectAnim.Play("ExchangeIn");
                    ButtonLock();
                });
            }
        }

        public void ExchangeEvent()
        {
            if (parentLayer != null)
            {
                parentLayer.Exchange.OnExchange(data.slot_id, NewRequestCome);
            }

            PlayerPrefs.SetInt("EXCHANGE_" + data.slot_id.ToString(), 1);
        }

        bool isExChangeInAnim = false;
        public void ExChangeInAnimFin()//ExchangeIn animationClip 으로 끝나면 쏨.
        {
            isExChangeInAnim = true;
        }

        public void OnDiscover()
        {
            //if (buttonLock)
            //    return;

            if (!isExChangeInAnim)
                return;

            PlayerPrefs.SetInt("EXCHANGE_" + data.slot_id.ToString(), 0);   
            SetBeggingLayer();
            effectAnim.Stop();
            effectAnim.Play("ExchangeAccept");
        }

        public void OnAccelate()
        {
            //if (buttonLock)
            //    return;

            if(parentLayer != null)
            {
                AccelerationMainPopup.OpenPopup(eAccelerationType.EXCHANGE, data.slot_id, User.Instance.Exchange.EXCHANGE_RENEW_LEAD_TIME, TimeManager.GetTimeStamp(data.regist_time), parentLayer.InitData);
            }
            
        }

        List<Asset> GetNeedItemList()
        {
            List<Asset> ret = new List<Asset>();
            Dictionary<int, Item> needitems = new Dictionary<int, Item>();
            for (int i = 0; i < 4; i++)
            {
                if (data.NeedItemInfo.Count > i)
                {
                    int groupNo = data.NeedItemInfo[i].GROUP;
                    ExchangeData exchangeData = null;

                    if (parentLayer != null)
                        exchangeData = parentLayer.Exchange.GetExchangeData(groupNo);

                    if (exchangeData != null)
                    {
                        int required = data.NeedItemInfo[i].REQUIRED_NUMBER;
                        if (needitems.ContainsKey(exchangeData.NEED_ITEM.KEY))
                            needitems[exchangeData.NEED_ITEM.KEY].AddCount(required);
                        else
                            needitems[exchangeData.NEED_ITEM.KEY] = new Item(exchangeData.NEED_ITEM.KEY, required);
                    }
                }
            }

            foreach (var need in needitems)
            {
                var itemNo = need.Key;
                var amount = need.Value.Amount;

                ret.Add(new Asset(itemNo, amount));
            }

            return ret;
        }
    }

}
