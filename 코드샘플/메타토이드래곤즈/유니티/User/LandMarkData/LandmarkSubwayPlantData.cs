using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class LandmarkSubwayPlantData
    {
        public int ID { get; private set; } = 0;
        public LandmarkSubwayPlantState State { get; private set; } = LandmarkSubwayPlantState.LOCKED;
        public int Expire { get; private set; } = 0;
        public List<List<int>> Slots { get; private set; } = new List<List<int>>();
        public List<JArray> Reward { get; private set; } = new List<JArray>();

        public void SetData(JObject jsonData)
        {
            ID = jsonData["id"].Value<int>();
            State = (LandmarkSubwayPlantState)jsonData["state"].Value<int>();
            Expire = jsonData.ContainsKey("expire") ? jsonData["expire"].Value<int>() : 0;

            if (SBFunc.IsJArray(jsonData["rewards"]))
            {
                var array = (JArray)jsonData["rewards"];
                Reward = array.ToObject<List<JArray>>();
            }
            else
            {
                Reward.Clear();
            }

            if (SBFunc.IsJArray(jsonData["slots"]))
            {
                var array = (JArray)jsonData["slots"];
                Slots = array.ToObject<List<List<int>>>();
            }
            else
            {
                Slots.Clear();
            }
        }
    }
}