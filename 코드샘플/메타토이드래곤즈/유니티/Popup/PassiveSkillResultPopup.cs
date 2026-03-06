using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class PassiveSkillResultPopup : Popup<PassiveSkillResultPopupData>
    {

        [Header("skill 1")]
        [SerializeField] GameObject skill1Node;
        [SerializeField] GameObject skill1SkillLayer;
        [SerializeField] GameObject skill1LockLayer;
        [SerializeField] Text skill1Title;
        [SerializeField] Text skill1Text;
        [SerializeField] Text skill1LockText;
        [SerializeField] Image TwingkleImg1;
        [SerializeField] Animation skill1Anim;

        [Header("skill 2")]
        [SerializeField] GameObject skill2Node;
        [SerializeField] GameObject skill2SkillLayer;
        [SerializeField] GameObject skill2LockLayer;
        [SerializeField] Text skill2Title;
        [SerializeField] Text skill2Text;
        [SerializeField] Text skill2LockText;
        [SerializeField] Image TwingkleImg2;
        [SerializeField] Animation skill2Anim;

        [Header("Need")]
        [SerializeField] Image NeedAssetIcon;
        [SerializeField] Text NeedAssetAmountText;
        [SerializeField] Image NeedAssetIcon2;
        [SerializeField] Text NeedAssetAmountText2;

        [Space(10)]
        [SerializeField] Button retryBtn;

        UserDragon dragonData = null;
        int currentSlot = 0;
        int minSkillGetLv = 0;
        float effectTimer = 0.5f;
        bool isNetwork = false;
        bool isEnoughState = false;
        SkillPassiveGroupData passiveData = null;
        VoidDelegate RetryCallBack = null;

        Sequence twingkleSequence = null;
        public override void InitUI()
        {
            dragonData = User.Instance.DragonData.GetDragon(Data.dragonNo);
            passiveData = SkillPassiveGroupData.Get(Data.PassiveKey);
            currentSlot = dragonData.PassiveSkillSlot; // 뚫린 스킬 슬롯

            var maxSkillSlot = CharTranscendenceData.GetMaxSkillSlot((eDragonGrade)dragonData.Grade());
            skill2Node.SetActive(maxSkillSlot > 1);
            SkillGetAnim();
            CheckRetryAbleState();
            isNetwork = false;
        }


        private void OnEnable()
        {
            skill1Title.text = string.Empty;
            skill2Title.text = string.Empty;
        }
        private void OnDisable()
        {
            if (twingkleSequence != null)
                twingkleSequence.Kill();
        }

        void SkillGetAnim()
        {
            skill1Title.text = string.Empty;
            skill2Title.text = string.Empty;
            skill1Text.text = string.Empty;
            skill2Text.text = string.Empty;
            if (currentSlot == 1)
            {
                Invoke(nameof(RefreshSkillInfo), 0.4f);
                skill1Anim.Play();
            }
            else if( currentSlot == 2)
            {
                Invoke(nameof(RefreshSkillInfo), 0.4f);
                skill2Anim.Play();
                skill1Anim.Play();
            }
        }

        //void SkillGetEffect(bool isRetry =false) // 연출
        //{
        //    if (twingkleSequence != null)
        //        twingkleSequence.Kill();
        //    twingkleSequence = DOTween.Sequence();
        //    float fadeTime = isRetry ? effectTimer : 0f;

        //    if (currentSlot == 1)
        //    {
        //        twingkleSequence.Append(TwingkleImg1.DOFade(1f, fadeTime));
        //        if(isRetry==false)
        //            twingkleSequence.AppendInterval(1f); // 팝업 열리는 연출 시간 고려
        //        twingkleSequence.AppendCallback(RefreshSkillInfo);
        //        twingkleSequence.Append(TwingkleImg1.DOFade(0, 1f));
        //    }
        //    else if (currentSlot == 2)
        //    {
        //        twingkleSequence.Append(TwingkleImg1.DOFade(1f, fadeTime));
        //        twingkleSequence.Join(TwingkleImg2.DOFade(1f, fadeTime));
        //        if (isRetry == false)
        //            twingkleSequence.AppendInterval(1f); // 팝업 열리는 연출 시간 고려
        //        twingkleSequence.AppendCallback(RefreshSkillInfo);

        //        twingkleSequence.Append(TwingkleImg1.DOFade(0, effectTimer));
        //        twingkleSequence.Join(TwingkleImg2.DOFade(0, effectTimer));
        //    }

        //}

        void RefreshSkillInfo()
        {
            var transcendenceDatas = CharTranscendenceData.GetByGrade((eDragonGrade)dragonData.Grade());
            var curSkills = dragonData.PassiveSkills; // 현재 스킬들
            SetLockState();
            foreach (var transcendenceData in transcendenceDatas)
            {
                if (transcendenceData.SKILL_SLOT_MAX == 1)
                {
                    if (currentSlot < 1)
                    {
                        skill1LockText.text = StringData.GetStringFormatByStrKey("스킬슬롯잠김", transcendenceData.STEP);
                    }
                    else
                    {
                        skill1SkillLayer.SetActive(true);
                        skill1LockLayer.SetActive(false);
                        skill1Text.text = curSkills.Count < 1 ? StringData.GetStringByStrKey("스킬미획득") : SkillPassiveData.Get(curSkills[0]).STRING;
                        skill1Title.text = curSkills.Count < 1 ? StringData.GetStringByStrKey("스킬1") : SkillPassiveRateData.GetSkillGroupName(curSkills[0]);
                    }
                    minSkillGetLv = transcendenceData.STEP;
                }
                else if (transcendenceData.SKILL_SLOT_MAX == 2)
                {
                    if (currentSlot < 2)
                    {
                        skill2LockText.text = StringData.GetStringFormatByStrKey("스킬슬롯잠김", transcendenceData.STEP);
                    }
                    else
                    {
                        skill2SkillLayer.SetActive(true);
                        skill2LockLayer.SetActive(false);
                        skill2Text.text = curSkills.Count < 2 ? StringData.GetStringByStrKey("스킬미획득") : SkillPassiveData.Get(curSkills[1]).STRING;
                        skill2Title.text = curSkills.Count < 2 ? StringData.GetStringByStrKey("스킬2") : SkillPassiveRateData.GetSkillGroupName(curSkills[1]);
                    }
                }
            }
        }

        void SetLockState()
        {
            skill1SkillLayer.SetActive(false);
            skill2SkillLayer.SetActive(false);
            skill1LockLayer.SetActive(true);
            skill2LockLayer.SetActive(true);
            skill1LockText.text = StringData.GetStringByStrKey("스킬획득불가");
            skill2LockText.text = StringData.GetStringByStrKey("스킬획득불가");
        }

        void CheckRetryAbleState()
        {
            var needItem = passiveData.NeedItem;
            var needGold = passiveData.NeedGold.Amount;
            isEnoughState = User.Instance.GetItemCount(needItem.ItemNo) >= needItem.Amount && User.Instance.GOLD >= needGold;

            NeedAssetIcon.sprite = needItem.BaseData.ICON_SPRITE;
            NeedAssetAmountText.text = SBFunc.CommaFromNumber(needItem.Amount);
            NeedAssetAmountText.color = User.Instance.GetItemCount(needItem.ItemNo) >= needItem.Amount ? Color.white : Color.red;
            NeedAssetIcon2.sprite = ResourceManager.GetResource<Sprite>(eResourcePath.ItemIconPath, "gold");
            NeedAssetAmountText2.text = SBFunc.CommaFromNumber(needGold);
            NeedAssetAmountText2.color = User.Instance.GOLD >= needGold ? Color.white : Color.red;

            retryBtn.SetButtonSpriteState(isEnoughState);
        }


        public void SetRetryCallBack(VoidDelegate cb)
        {
            if(cb != null)
                RetryCallBack = cb;
        }

        public void OnClickRetry()
        {
            // to do .. 재료 체크
            if (isNetwork)
                return;
            isNetwork = true;
            if (isEnoughState == false)
            {
                ToastManager.On(StringData.GetStringByIndex(100002249));
                return;
            }
            WWWForm param = new WWWForm();
            param.AddField("did", Data.dragonNo);
            param.AddField("type", (int)Data.ePassiveRefreshType);
            NetworkManager.Send("dragon/refreshpassive", param, (JObject jsondata) =>
            {
                if (SBFunc.IsJTokenCheck(jsondata["rs"]) && jsondata["rs"].ToObject<int>() == (int)eApiResCode.OK)
                {
                    isNetwork = false;
                    SkillGetAnim();
                    CheckRetryAbleState();
                    RetryCallBack?.Invoke();
                }
            },
            (string dat) =>
            {
                isNetwork = false;
            });
            
        }
    }
}