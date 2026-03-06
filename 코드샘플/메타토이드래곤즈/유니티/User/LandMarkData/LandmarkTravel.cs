using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class LandmarkTravel : Landmark
    {
        public int TravelWorld { get; private set; } = -1;
        public eTravelState TravelState { get; private set; } = eTravelState.None;
        public int TravelTime { get; private set; } = -1;
        public List<UserDragon> TravelDragon { get; private set; } = null;
        public LandmarkTravel()
            : base(eLandmarkType.Travel)
        {

        }

        public void Init()
        {
            TravelWorld = -1;
            TravelTime = -1;
            TravelState = eTravelState.Normal;

            if (TravelDragon == null)
                TravelDragon = new List<UserDragon>();
            else
                TravelDragon.Clear();
        }

        public override void SetData(JToken jsonData)
        {
            base.SetData(jsonData);

            if (SBFunc.IsJTokenCheck(jsonData["trip"]) && SBFunc.IsJTokenCheck(jsonData["trip"]["state"]))
                TravelState = (eTravelState)jsonData["trip"]["state"].Value<int>();
            if (SBFunc.IsJTokenCheck(jsonData["state"]))
                TravelState = (eTravelState)jsonData["state"].Value<int>();

            if (SBFunc.IsJTokenCheck(jsonData["trip"]) && SBFunc.IsJTokenCheck(jsonData["trip"]["world"]))
                TravelWorld = jsonData["trip"]["world"].Value<int>();
            if (SBFunc.IsJTokenCheck(jsonData["world"]))
                TravelWorld = jsonData["world"].Value<int>();

            if (TravelState == eTravelState.Complete)
            {
                TravelTime = 0;
            }
            else
            {
                if (SBFunc.IsJTokenCheck(jsonData["trip"]) && SBFunc.IsJTokenCheck(jsonData["trip"]["expire"]))
                    TravelTime = jsonData["trip"]["expire"].Value<int>();

                if (SBFunc.IsJTokenCheck(jsonData["expire"]))
                    TravelTime = jsonData["expire"].Value<int>();
            }

            SetDragon(jsonData);
        }

        private void SetDragon(JToken jsonData)
        {
            if (TravelDragon != null)
            {
                var travelCount = TravelDragon.Count;
                for (var i = 0; i < travelCount; i++)
                {
                    if (TravelDragon[i] != null)
                        TravelDragon[i].RemoveDragonState(eDragonState.Travel);
                }
            }
            TravelDragon.Clear();
            if (SBFunc.IsJTokenCheck(jsonData["trip"]) && SBFunc.IsJTokenCheck(jsonData["trip"]["dragons"]))
            {
                var travelCount = jsonData["trip"]["dragons"].Count();
                for (var i = 0; i < travelCount; i++)
                {
                    var dragonTag = jsonData["trip"]["dragons"][i].Value<int>();
                    UserDragon dragon = User.Instance.DragonData.GetDragon(dragonTag);
                    if (dragon != null)
                    {
                        dragon.SetDragonState(eDragonState.Travel);
                        TravelDragon.Add(dragon);
                    }
                }
            }
            if (SBFunc.IsJTokenCheck(jsonData["dragon"]))
            {
                var jDragon = (JArray)jsonData["dragon"];
                List<int> dragonArray = jDragon.ToObject<List<int>>();
                var dragonArrayCount = dragonArray.Count;

                for (var k = 0; k < dragonArrayCount; k++)
                {
                    var travelDragonTag = dragonArray[k];
                    var travelDragon = User.Instance.DragonData.GetDragon(travelDragonTag);
                    if (travelDragon != null)
                    {
                        travelDragon.SetDragonState(eDragonState.Travel);
                        TravelDragon.Add(travelDragon);
                    }
                }
            }
        }

        public void SetTravelState(eTravelState value)
        {
            TravelState = value;
        }

        public void SetTravelTime(int value)
        {
            TravelTime = value;
        }

        public void GetReward()
        {
            NetworkManager.Send("travel/finish", null, OnFinishResponse);
        }

        void OnFinishResponse(JObject jsonData)
        {
            if (jsonData.ContainsKey("rs"))
            {
                switch ((eApiResCode)jsonData["rs"].Value<int>())
                {
                    case eApiResCode.OK:
                    {
                        SetTravelTime(0);

                        List<Asset> totalRewards = new List<Asset>();

                        if (jsonData.ContainsKey("rewards"))
                        {
                            totalRewards.AddRange(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["rewards"])));

                        }
                        if (jsonData.ContainsKey("exp"))
                        {
                            int accountExp = jsonData["exp"].Value<int>();
                            if (accountExp > 0)
                                totalRewards.Add(new Asset(eGoodType.NONE, 10000003, accountExp));
                        }
                        if (jsonData.ContainsKey("exp_char"))
                        {
                            int dragonExp = jsonData["exp_char"].Value<int>();
                            if (dragonExp > 0)
                                totalRewards.Add(new Asset(eGoodType.NONE, 10000004, dragonExp));
                        }
                        if (jsonData.ContainsKey("bonus"))
                        {
                            totalRewards.AddRange(SBFunc.ConvertSystemRewardDataList(JArray.FromObject(jsonData["bonus"])));
                        }

                        if (totalRewards.Count > 0)
                        {
                            SystemRewardPopup.OpenPopup(totalRewards.ToList());
                        }

                        DragonShowEvent.Send();
                    }
                    break;
                    case eApiResCode.BUILDING_NOT_EXISTS:
                    {// 여행사가 없음.

                    }
                    break;
                    case eApiResCode.INVENTORY_FULL:
                    {// 보상을 받을 인벤토리가 부족함.

                    }
                    break;
                    case eApiResCode.TRAVEL_DRAGON_NOT_EXISTS:
                    {// 존재하지 않는 드래곤.

                    }
                    break;
                    case eApiResCode.TRAVEL_NOT_FINISHED:
                    {// 여행이 아직 끝나지 않음

                    }
                    break;
                    case eApiResCode.TRAVEL_NOT_RUNNING:
                    {// 여행 진행중이 아님.

                    }
                    break;
                    default:
                        break;
                }
            }
        }
    }
}