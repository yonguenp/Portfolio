using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class LandmarkDozer : Landmark
    {
        public List<ProductReward> RewardList { get; private set; } = new List<ProductReward>();

        protected int recent = 0;
        protected bool requested = false;

        public int ExpireTime { get; private set; } = 0;
        public int Boost { get; private set; } = 0;
        public bool Requested { get; private set; } = false;
        public LandmarkDozer()
            : base(eLandmarkType.Dozer)
        {

        }

        public bool Recall(bool force = false)
        {
            if (force)
            {
                requested = true;
                NetworkManager.Send("dozer/state", null, null);
            }
            else if ((TimeManager.GetTime() - recent > 60 || ExpireTime - TimeManager.GetTime() > 0 && ((ExpireTime - TimeManager.GetTime()) % 60) == 0) 
                && !requested 
                && recent != TimeManager.GetTime() 
                && !CAPTCHAPopup.NeedCheck())
            {
                requested = true;
                NetworkManager.Send("dozer/state", null, null, null, false);
            }

            return requested;
        }

        public override void SetData(JToken jsonData)
        {
            base.SetData(jsonData);

            requested = false;
            recent = TimeManager.GetTime();

            var jobject = JObject.FromObject(jsonData);

            if (!SBFunc.IsJTokenCheck(jsonData))
            {
                return;
            }

            if (jobject.ContainsKey("rewards"))
            {
                var array = (JArray)jsonData["rewards"];
                var rewards = SBFunc.ConvertToRewardItemList(RewardList, array);
                RewardList = new List<ProductReward>();
                foreach (var reward in rewards)
                {
                    if (reward.Amount > 0)
                        RewardList.Add(reward);
                }
            }
            if (SBFunc.IsJTokenCheck(jsonData["expire"]) && SBFunc.IsJTokenType(jsonData["expire"], JTokenType.Integer))
            {
                ExpireTime = jsonData["expire"].Value<int>();
            }
            if (SBFunc.IsJTokenCheck(jsonData["boost"]) && SBFunc.IsJTokenType(jsonData["boost"], JTokenType.Integer))
            {
                Boost = jsonData["boost"].Value<int>();
            }

            LandmarkUpdateEvent.Send(Type);
        }

        public void GetReward()
        {
            var currentReward = new List<Asset>(RewardList);
            if (currentReward == null || currentReward.Count <= 0)
            {
                ToastManager.On(100002519);
                return;
            }

            if (User.Instance.CheckInventoryGetItem(currentReward))
            {
                SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                    () => {
                        PopupManager.OpenPopup<InventoryPopup>();
                    },
                    () => {   //나가기
                
                    },
                    () => {  //나가기
                
                    }
                );
                return;
            }

            NetworkManager.SendWithCAPTCHA("dozer/harvest", null, (jsonData) =>
            {
                if (jsonData.ContainsKey("rs"))
                {
                    if (jsonData["rs"].Value<int>() == (int)eApiResCode.INVENTORY_FULL)
                    {
                        SystemPopup.OnSystemPopup(StringData.GetStringByIndex(100000248), StringData.GetStringByIndex(100002077),
                            () =>
                            {
                                PopupManager.OpenPopup<InventoryPopup>();
                            },
                            () =>
                            {   //나가기
                            },
                            () =>
                            {  //나가기
                            }
                        );

                        PopupManager.ForceUpdate<LandMarkPopup>();
                        return;
                    }

                    if (jsonData["rs"].Value<int>() == 502)
                    {
                        ToastManager.On(100000812);
                        return;
                    }
                }

                if (jsonData.ContainsKey("rewards"))
                {
                    RewardList.Clear();

                    SystemRewardPopup.OpenPopup(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["rewards"])));
                    PopupManager.ForceUpdate<LandMarkPopup>();
                }

                Town.Instance.dozer.CheckProductAlarm();
            });
        }
    }
}