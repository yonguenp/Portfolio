using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandboxNetwork
{
    public class EventAttendanceObject : AttendanceObject
    {
        private EventAttendanceData Data = null;
        private EventRewardData RewardData = null;
        public override void SetData(EventAttendanceData data, EventRewardData _reward, Sprite _boxSprite, bool _IsAnim = false)
        {
            Data = data;
            RewardData = _reward;
            IsAnim = _IsAnim;
            boxImage.sprite = _boxSprite;
            if (RewardData != null)
                Initialize();
        }

        protected override void InitializeDayInfo()
        {
            if (dayText == null)
                return;

            dayText.text = StringData.GetStringFormatByIndex(100002509, RewardData.SEQ);
        }
        protected override void InitializeItemInfo()
        {
            var postItems = PostRewardData.GetGroup(RewardData.REWARD_ID);

            if (postItems == null || postItems.Count < 1)
                return;

            var postItem = postItems[0];
            if (postItem is null || postItem.Reward is null)
                return;

            InitializeItemIcon(postItem.Reward);
            InitializeItemCount(postItem.Reward);
        }
        protected override void InitializeRewardInfo()
        {
            if (rewardLayer is null)
                return;

            if (Data.AttendanceDay > RewardData.SEQ)
            {
                SetActive(rewardLayer, true);
                SetActive(stemp, true);
            }
            else
            {
                SetActive(rewardLayer, false);
                if (Data.AttendanceDay == RewardData.SEQ)
                    InitializeAnim();
            }
        }

        public override void OnClickShowItemList()
        {
            if (RewardData == null)
                return;

            List<PostRewardData> rewards = PostRewardData.GetGroup(RewardData.REWARD_ID);
            if (rewards != null && rewards.Count > 0)
            {
                var reward = rewards[0].Reward;

                if(reward.GoodType != eGoodType.CHARACTER)
                    ToolTip.OnToolTip(reward, gameObject);
                else
                {
                    var attendancePopup = PopupManager.GetPopup<EventAttendancePopup>();
                    attendancePopup.SetExitCallback(() => { });
                    attendancePopup.ClosePopup();

                    var popup = DragonManagePopup.OpenPopup(0, 1);
                    PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag = true;
                    popup.SetExitCallback(() => {
                        //if (PopupManager.GetPopup<DragonManagePopup>().ForceCloseFlag == false)
                        //    EventAttendancePopup.OpenPopup(null);//끌때 다시 켜기
                    });
                    popup.CurDragonTag = reward.ItemNo;
                    popup.ClearDragonInfoList();

                    popup.DragonInfoList.AddRange(new List<int>() { reward.ItemNo });
                    popup.ForceUpdate();
                }
            }
        }
    }
}
