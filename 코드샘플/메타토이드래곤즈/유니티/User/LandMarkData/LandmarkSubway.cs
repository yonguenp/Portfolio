using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class LandmarkSubway : Landmark
    {
        public List<LandmarkSubwayPlantData> PlatsData { get; private set; } = null;


        public LandmarkSubway()
            : base(eLandmarkType.SUBWAY)
        {

        }
        public void Init()
        {
            if (PlatsData == null)
                PlatsData = new List<LandmarkSubwayPlantData>();
            else
                PlatsData.Clear();
        }

        public override void SetData(JToken jsonData)
        {
            if (PlatsData == null)
                Init();

            if (PlatsData.Count == 0)
            {
                for (var i = 0; i < 3; i++)
                    PlatsData.Add(new LandmarkSubwayPlantData());
            }

            base.SetData(jsonData);
            if (SBFunc.IsJArray(jsonData["plats"]))
            {
                var array = (JArray)jsonData["plats"];
                for (var i = 0; i < array.Count; i++)
                {
                    var data = (JObject)array[i];
                    var index = data["id"].Value<int>() - 1;

                    PlatsData[index].SetData(data);
                }
            }
        }

        public List<LandmarkSubwayPlantData> GetActivatePlatform()
        {
            List<LandmarkSubwayPlantData> resultList = new();

            if (PlatsData != null)
            {
                for (int i = 0; i < PlatsData.Count; ++i)
                {
                    if (PlatsData[i].State == LandmarkSubwayPlantState.NONE ||
                        PlatsData[i].State == LandmarkSubwayPlantState.CAN_UNLOCK ||
                        PlatsData[i].State == LandmarkSubwayPlantState.LOCKED)
                    {
                        continue;
                    }

                    resultList.Add(PlatsData[i]);
                }
            }

            return resultList;
        }
    }
}