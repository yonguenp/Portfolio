using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork { 
    public class GemRewardObj : MonoBehaviour
    {
        [SerializeField] Slider expSlider = null;
        [SerializeField] Text currentExpText = null;
        [SerializeField] Text gainAbleCountText = null;
        [SerializeField] TimeObject timeObj = null;
        //[SerializeField] GameObject SliderhandleEffect = null;
        [SerializeField] GameObject handleEffectObj = null;
        [SerializeField] Animator getTextAnim = null;
        [SerializeField] GameObject itemObject = null;

        /// <summary> 바깥에서 ItemCount 계산 용도로 쓰임 </summary>
        public int ItemCount { get => DataItemCount; }
        /// <summary> Initialize 세팅 몇 번째 아이템인가 </summary>
        private int Index { get; set; } = -1;
        /// <summary> Initialize 세팅 층 정보 </summary>
        private LandmarkGemDungeonFloor FloorData { get; set; } = null;
        /// <summary> Reward 기준 1개 => 1000000 == Million </summary>
        private float Reward { get => FloorData.GetReward(Index) + ClientReward; }
        /// <summary> Clienttick 계산 용도 </summary>
        private float ClientReward { get; set; } = 0;
        /// <summary> 수령할 수 있는 아이템 수량 </summary>
        private int DataItemCount { get => Mathf.FloorToInt(Reward * SBDefine.CONVERT_MILLION); }

        private int LastestClientReward = 0;
        Coroutine textShowCor = null;
        Tween itemTween = null;

        VoidDelegate itemCountIncreaseCallBack = null;

        public void Init(int index, LandmarkGemDungeonFloor floor, VoidDelegate itemCountIncreaseCB)
        {
            Index = index;

            if (null == floor)
                return;

            FloorData = floor;
            if (FloorData.Rewards.Count <= index || 0 > index)
                return;

            ClientReward = 0;
            if (textShowCor != null)
            {
                StopCoroutine(textShowCor);
                textShowCor = null;
            }
            if (Index < 0 || FloorData == null)
                return;
            getTextAnim.gameObject.SetActive(false);
            handleEffectObj.SetActive(FloorData.State == eGemDungeonState.BATTLE);
            RefreshGage();

            if(itemCountIncreaseCB != null) 
                itemCountIncreaseCallBack = itemCountIncreaseCB;
        }

        private void RefreshGage()
        {
            if (timeObj == null || expSlider == null || currentExpText == null || gainAbleCountText == null)
                return;

            expSlider.maxValue = SBDefine.MILLION * SBDefine.CONVERT_MILLION_PERC;
            if (eGemDungeonState.BATTLE != FloorData.State || FloorData.TotalBattlePoint < FloorData.GetAutoAmount(Index)) // 기준보다 낮은 전투력이라면 오르지도 않는다
            {
                UIRefresh();
                if (eGemDungeonState.END == FloorData.State && FloorData.IsFullReward)
                    currentExpText.color = Color.red;

                ItemTweenClear();
                timeObj.Refresh = null;
                return;
            }
            RefreshClientReward();

            LastestClientReward = DataItemCount;
            currentExpText.color = Color.white;
            timeObj.Refresh = TimeRefresh;
            ItemTweenStart();
        }
        private void TimeRefresh()
        {
            RefreshClientReward();

            if (LastestClientReward < DataItemCount)
            {
                if(textShowCor == null && gameObject.activeInHierarchy)
                {
                    textShowCor = StartCoroutine(TextShow());
                    LastestClientReward = DataItemCount;
                    itemCountIncreaseCallBack?.Invoke();
                }
            }

            /** 갱신 */
            UIRefresh();
            //
        }
        /// <summary> 틱 당 증가 계산 </summary>
        private void RefreshClientReward()
        {
            ClientReward = FloorData.GetClientReward(Index);
        }
        private void OnDisable()
        {
            ItemTweenClear();
            if (textShowCor != null)
            {
                StopCoroutine(textShowCor);
                textShowCor = null;
            }
        }
        private void UIRefresh()
        {
            //int sliderValue = DataItemMod;
            //expSlider.value = sliderValue;
            //SliderhandleEffect.SetActive(6 < sliderValue && sliderValue < 94);
            //currentExpText.text = SBFunc.StrBuilder(sliderValue, StringData.GetStringByStrKey("skill_effect_percent"));
            gainAbleCountText.text = SBFunc.StrBuilder(DataItemCount, StringData.GetStringByStrKey("item_ea"));
        }

        IEnumerator TextShow()
        {
            getTextAnim.gameObject.SetActive(true);
            getTextAnim.Play("DiaGet", 0);
            while (getTextAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                yield return null;
            }
            getTextAnim.gameObject.SetActive(false);

            yield return null;
            textShowCor = null;
        }
        private void ItemTweenStart()
        {
            if (itemTween != null)
            {
                itemTween.Kill();
                itemTween = null;
            }

            if (itemObject != null)
            {
                itemObject.transform.localScale = Vector3.one;
                itemTween = itemObject.transform.DOScale(Vector3.one * 1.15f, 0.5f).SetLoops(-1, LoopType.Yoyo).Play();
            }
        }
        private void ItemTweenClear()
        {
            if (itemObject != null)
                itemObject.transform.localScale = Vector3.one;

            if (itemTween != null)
            {
                itemTween.Kill();
                itemTween = null;
            }
        }
    }
}