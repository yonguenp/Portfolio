using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class UIStaminaObject : UIObject, EventListener<UIObjectEvent>
    {
        [SerializeField]
        GameObject needTimeObject = null;
        [SerializeField]
        private Text staminaAmountLabel = null;

        [SerializeField]
        TimeObject staminaTimeObj = null;
        [SerializeField]
        Text energyExpText = null;
        [SerializeField]
        Button staminaBtn = null;
        public delegate void CallBack();
        CallBack timeEndCallback;

        [SerializeField]
        private Animator getTextAnim = null;
        [SerializeField]
        private Text energyAddLabel = null;

        public static int lastestEnergy = 0;
        public int fakeCount = GameConfigTable.GetAdventureStaminaFakeUpdateCount();
        public void setCallBack(CallBack cb)
        {
            if (cb != null)
            {
                timeEndCallback = cb;
            }
        }
        public override void InitUI(eUIType targetType)
        {
            base.InitUI(targetType);
            StopAllCoroutines();
            if (curSceneType > eUIType.None && curUIType.HasFlag(curSceneType))
            {
                lastestEnergy = User.Instance.ENERGY;
                getTextAnim.gameObject.SetActive(false);
                RefreshEnergy();
            }
        }
        public override bool RefreshUI(eUIType targetType)
        {
            if (base.RefreshUI(targetType))
            {
                RefreshEnergy();
            }

            return curSceneType != targetType;
        }
        public void setButtonState(bool isOn)
        {
            if (staminaBtn != null) staminaBtn.SetInteractable(isOn && (curSceneType == eUIType.Adventure));
        }
        public void onClickStamina()
        {
            if (StageManager.Instance.AdventureProgressData.IsClearedStage(1, 1)) {
                var shop = PopupManager.GetPopup<ShopPopup>();
                if (PopupManager.IsPopupOpening(shop))
                {
                    return;
                }

                PopupManager.OpenPopup<ShopPopup>(new MainShopPopupData(14));
            }
            else
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100000422),
                    ()=> { }, null, ()=> { });
            }
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            // lastestEnergy = User.Instance.ENERGY;
            EventManager.RemoveListener(this);
            getTextAnim.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            var energy = User.Instance.ENERGY;
            var distance = energy - lastestEnergy;
            EventManager.AddListener(this);
            if (distance != 0)
            {
                StartCoroutine(TextShow(distance));

                lastestEnergy = energy;
            }
            RefreshEnergy();
        }
        void RefreshEnergy()
        {
            var accountData = AccountData.GetLevel(User.Instance.UserData.Level);
            var energyMax = accountData != null ? accountData.MAX_STAMINA : 0;//TableManager.GetTable<AccountTable>().GetByLevel(User.Instance.UserData.Level).MAX_STAMINA;
            var exp = User.Instance.UserData.Energy_Exp;
            var isNeed = exp > 0;
            if (staminaAmountLabel != null)
            {
                staminaAmountLabel.text = SBFunc.StrBuilder(SBFunc.CommaFromNumber(User.Instance.ENERGY), "/", energyMax);
            }
            if (gameObject.activeInHierarchy)
            {
                var energy = User.Instance.ENERGY;
                var distance = energy - lastestEnergy;
                if (distance != 0)
                {
                    StartCoroutine(TextShow(distance));
                }
            }
            if (timeEndCallback != null)
            {
                timeEndCallback();
            }
            if(needTimeObject != null) { 
                needTimeObject.SetActive(isNeed);
            }
            if (isNeed)
            {
                if (staminaTimeObj.Refresh == null)
                {
                    //ArenaManager.Instance.SetTimeObject(staminaTimeObj);
                    staminaTimeObj.Refresh = () =>
                    {
                        float remain = TimeManager.GetTimeCompare(exp);
                        if(energyExpText != null)
                        {
                            energyExpText.text = SBFunc.TimeString(remain);
                        }
                        
                        if (0 >= remain)
                        {
                            // 업데이터 해제
                            staminaTimeObj.Refresh = null;
                            if (fakeCount <= 0)
                            {
                                fakeCount = GameConfigTable.GetAdventureStaminaFakeUpdateCount();
                                NetworkManager.Send("user/energy", null, (JObject jsonData) =>
                                {
#if UNITY_EDITOR
                                Debug.Log("user/energy server callback is called");
#endif
                                    User.Instance.UserData.UpdateEnergy(jsonData["energy"].Value<int>());
                                    User.Instance.UserData.UpdateEnergyExp(jsonData["energy_tick"].Value<int>());
                                    UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_STAMINA, UIObjectEvent.eUITarget.ALL);
                                });
                            }
                            else
                            {
                                fakeCount--;
                                int val = User.Instance.ENERGY + GameConfigTable.GetAdventureStaminaRechargeAmount();
                                if (energyMax <= val)
                                {
                                    User.Instance.UserData.UpdateEnergy(energyMax);
                                    User.Instance.UserData.UpdateEnergyExp(0);
                                }
                                else
                                { 
                                    User.Instance.UserData.UpdateEnergy(val);
                                    User.Instance.UserData.UpdateEnergyExp(User.Instance.UserData.Energy_Exp + GameConfigTable.GetAdventureStaminaRechargeTime());                                    
                                }
                                UIObjectEvent.Event(UIObjectEvent.eEvent.REFRESH_STAMINA, UIObjectEvent.eUITarget.ALL);
                            }
                        }
                    };
                }
            }
        }

        IEnumerator TextShow(int EnergyDistance)
        {
            if (EnergyDistance == 0)
            {
                getTextAnim.gameObject.SetActive(false);
                yield break;
            }
            getTextAnim.gameObject.SetActive(true);
            getTextAnim.Play("DiaGet", 0);
            energyAddLabel.text = EnergyDistance.ToString("+#;-#");
            while (getTextAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
            getTextAnim.gameObject.SetActive(false);
            lastestEnergy = User.Instance.ENERGY;
            yield return null;
        }

        void EventListener<UIObjectEvent>.OnEvent(UIObjectEvent eventType)
        {

            switch (eventType.e)
            {
                case UIObjectEvent.eEvent.REFRESH_STAMINA:
                    RefreshEnergy();
                    break;
            }
        }
    }
}

