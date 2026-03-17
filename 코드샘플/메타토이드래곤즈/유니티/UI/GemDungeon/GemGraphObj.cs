using Coffee.UIEffects;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


namespace SandboxNetwork
{
    public class GemGraphObj : MonoBehaviour
    {
        [SerializeField]
        Image rewardPredictImage = null;
        [SerializeField]
        Text rewardPredictText = null;
        [SerializeField]
        Slider rewardGageSlider = null;
        [SerializeField]
        UIShiny iconEffect = null;
        [SerializeField]
        GameObject gageEffect = null;
        [SerializeField] 
        TimeObject timeObj = null;
        [SerializeField]
        Text RewardCount = null;
        public Image RewardImage { get => rewardPredictImage; }
        public Text RewardText { get => rewardPredictText; }
        /// <summary> Reward 기준 1개 => 1000000 == Million </summary>
        private float Reward { get => FloorData.GetReward(Index) + ClientReward; }
        /// <summary> Clienttick 계산 용도 </summary>
        private int ClientReward { get; set; } = 0;
        /// <summary> 수령 하고 남는 짜투리의 수량(표시는 10000 단위기 때문) </summary>
        private int DataItemMod { get => Mathf.FloorToInt(Reward % SBDefine.MILLION); }
        private int Amount { get; set; } = 0;

        /// <summary> Initialize 세팅 몇 번째 아이템인가 </summary>
        private int Index { get; set; } = -1;
        /// <summary> Initialize 세팅 층 정보 </summary>
        private LandmarkGemDungeonFloor FloorData { get; set; } = null;
        public bool IsAnim { get; private set; } = false;
        private Tween textTween = null;


        public void Init(int index, LandmarkGemDungeonFloor floor)
        {
            if (null == floor || index < 0)
                return;
            //if (FloorData.Rewards.Count <= index ) return; // 이 조건문 아무 것도 없는 최초 1회 드래곤 세팅 때는 보상이 0 이라서 이때 동작안해서 뺌
            
            FloorData = floor;
            Index = index;
            Amount = FloorData.GetAutoAmount(Index);
            IsAnim = (0 >= Amount ? 0 : FloorData.TotalBattlePoint / Amount) > 0;
            ClientReward = 0;
            if (IsAnim && eGemDungeonState.BATTLE == FloorData.State)
                InitializeGage();
            else
                SetDisableByFull();
        }
        /// <summary> 틱 당 증가 계산 </summary>
        private void RefreshClientReward()
        {
            ClientReward = FloorData.GetClientReward(Index);


        }
        private void InitializeGage()
        {
            if (rewardGageSlider == null || rewardPredictText == null)
                return;

            var totalValue = Mathf.RoundToInt(LandmarkGemDungeonFloor.RewardItemTick * FloorData.GetAutoTerm(Index) * Mathf.FloorToInt(0 >= Amount ? 0 : FloorData.TotalBattlePoint / Amount) * SBDefine.BASE_FLOAT) * SBDefine.CONVERT_FLOAT;
            rewardPredictText.text = StringData.GetStringFormatByStrKey("시간당획득포인트", string.Format("{0:0.##}", totalValue), GetUIHour());
            rewardPredictText.color = FloorData.IsBuffState ? Color.yellow : Color.white;

            if (gageEffect != null)
                gageEffect.SetActive(true);

            rewardGageSlider.maxValue = SBDefine.MILLION;
            if (timeObj != null)
                timeObj.Refresh = RefreshGage;

            if (FloorData.IsBuffState)
                BoosterTextAnim(RewardText);
        }
        private void RefreshGage()
        {
            RefreshClientReward();
            rewardGageSlider.value = DataItemMod;

            var rewardCount = Mathf.FloorToInt((FloorData.GetReward(Index) + FloorData.GetClientReward(Index)) * SBDefine.CONVERT_MILLION);
            RewardCount.text = rewardCount > 0 ? rewardCount.ToString() : "";
        }

        private void SetDisableByFull() // 수령 아이템 가득 차서 아무것도 못하게 해야 함
        {
            ClearAnim();
            if (gageEffect != null)
                gageEffect.SetActive(false);

            rewardPredictText.color = Color.red;
            rewardPredictText.text = StringData.GetStringFormatByStrKey("시간당획득포인트", 0, GetUIHour());
            rewardGageSlider.maxValue = SBDefine.MILLION;
            rewardGageSlider.value = DataItemMod;
        }
        private int GetUIHour()
        {
            return Mathf.FloorToInt(FloorData.GetAutoTerm(Index) / SBDefine.Hour);
        }
        public void SetEffect(bool isEffect)
        {
            if (iconEffect == null)
                return;

            if (isEffect && IsAnim)
                iconEffect.Play();
            else
                iconEffect.Stop(true);
        }
        private void OnDisable()
        {
            ClearAnim();

            if (timeObj != null)
                timeObj.Refresh = null;
        }
        private void BoosterTextAnim(Text target)
        {
            if (null == target)
                return;

            ClearAnim();
            var sequnce = DOTween.Sequence();
            sequnce.SetDelay(0.125f);
            sequnce.Append(target.DOColor(new Color(1f, 0.6f, 0.016f, 1f), 2f).SetEase(Ease.InQuad));
            sequnce.SetDelay(0.125f);
            sequnce.SetLoops(-1, LoopType.Yoyo);
            textTween = sequnce.Play();
        }
        private void ClearAnim()
        {
            if (textTween != null)
            {
                textTween.Kill();
                textTween = null;
            }
        }
    }
}

