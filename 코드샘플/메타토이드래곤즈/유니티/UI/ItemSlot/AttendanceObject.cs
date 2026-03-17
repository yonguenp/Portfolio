using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public enum HolderCheck
    {
        None = 0,
        Normal = 1,
        Holder = 2
    }
    public class AttendanceObject : MonoBehaviour
    {
        private DailyRewardData Data { get; set; } = null;
        protected bool IsAnim { get; set; } = false;

        [Header("DayInfo")]
        [SerializeField]
        protected Text dayText = null;
        [SerializeField]
        protected Image boxImage = null;
        

        [Space(10f)]
        [Header("ItemInfo")]
        [SerializeField]
        protected Image itemImage = null;
        [SerializeField]
        private Text itemCount = null;
        [Space(10f)]
        [Header("RewardInfo")]
        [SerializeField]
        protected GameObject rewardLayer = null;
        [SerializeField]
        protected Animation anim = null;
        [SerializeField]
        protected GameObject stemp = null;
        [SerializeField]
        SFXPlayer sfxPlayer = null;
    public virtual void SetData(DailyRewardData Data, Sprite boxSprite , bool IsAnim = false)
        {
            this.Data = Data;
            this.IsAnim = IsAnim;
            boxImage.sprite = boxSprite;
            if (this.Data is not null)
                Initialize();
        }

        public virtual void SetData(EventAttendanceData data, EventRewardData _data, Sprite _boxSprite, bool _IsAnim = false) {}

        protected virtual void Initialize()
        {
            InitializeDayInfo();
            InitializeItemInfo();
            InitializeRewardInfo();
        }


        protected virtual void InitializeDayInfo()
        {
            if (dayText is null)
                return;

            dayText.text = StringData.GetStringFormatByIndex(100002509, Data.DAY);

            if (sfxPlayer != null)
            {
                sfxPlayer.isPlayOnEnable = false;
            }
        }
        protected virtual void InitializeItemInfo()
        {
            var postItems = Data.REWARDS;

            if (postItems is null && postItems.Count < 1)
                return;

            var postItem = postItems[0];
            if (postItem is null)
                return;

            InitializeItemIcon(postItem);
            //InitializeItemNo(postItem.Reward);
            InitializeItemCount(postItem);
        }
        protected virtual void InitializeRewardInfo()
        {
            if (rewardLayer is null)
                return;

            if(User.Instance.Attendance.AttendanceDay > Data.DAY)
            {
                SetActive(rewardLayer, true);
                SetActive(stemp, true);
            }
            else
            {
                SetActive(rewardLayer, false);
                if (User.Instance.Attendance.AttendanceDay == Data.DAY)
                    InitializeAnim();
            }
        }
        protected virtual void InitializeAnim()
        {
            SetActive(rewardLayer, true);
            SetActive(stemp, false == IsAnim);
        }
        protected virtual void InitializeItemIcon(Asset data)
        {
            if (itemImage is null)
                return;

            switch(data.GoodType)
            {
                case eGoodType.CHARACTER:
                    CharBaseData charData = CharBaseData.Get(data.ItemNo);
                    if (charData is null)
                        return;

                    itemImage.sprite = charData.GetThumbnail();
                    break;
                case eGoodType.ITEM:
                default:
                    ItemBaseData itemInfo = ItemBaseData.Get(GetItemNo(data));
                    if (itemInfo is null)
                        return;

                    itemImage.sprite = itemInfo.ICON_SPRITE;
                    break;
            }
        }
        //private void InitializeItemNo(Asset data)
        //{
        //    if (itemName is null)
        //        return;

        //    ItemBaseData itemInfo = ItemBaseData.Get(GetItemNo(data));
        //    if (itemInfo is null)
        //        return;

        //    itemName.text = StringData.GetStringByStrKey(itemInfo._NAME);
        //}
        protected void InitializeItemCount(Asset data, int _showCount = 1)
        {
            if (itemCount is null)
                return;

            itemCount.gameObject.SetActive(data.Amount > _showCount);
            itemCount.text = data.Amount.ToString();
        }
        protected virtual void SetActive(GameObject target, bool visible)
        {
            if (target is null)
                return;

            target.SetActive(visible);

        }
        private int GetItemNo(Asset data)
        {
            return SBFunc.GetItemNoByGoodType(data.GoodType, data.ItemNo);
        }
        public void StartAnim()
        {
            if (IsAnim && anim is not null)
                anim.Play();

            if (sfxPlayer != null)
            {
                sfxPlayer.isPlayOnEnable = true;
            }
        }

        public bool AnimPlaying()
        {
            if (anim == null)
                return false;

            return anim.isPlaying;
        }

        public virtual void OnClickShowItemList()
        {
            List<Asset> rewards = Data.REWARDS;
            if(rewards != null && rewards.Count > 0)
            {
                var reward = rewards[0];
                ToolTip.OnToolTip(reward, gameObject);
            }
        }
    }
}