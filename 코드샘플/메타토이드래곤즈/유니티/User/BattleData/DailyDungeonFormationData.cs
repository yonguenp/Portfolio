using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class DailyDungeonFormationData
    {
        //팀배치 데이터
        public List<List<int>> TeamFormation { get; private set; } = new List<List<int>>();
        public List<List<int>> TeamPart { get; private set; } = new List<List<int>>();

        public void SetData(JObject jsonData)//jsonData: any
        {
            if (jsonData == null) { return; }

            TeamFormation.Clear();
            TeamPart.Clear();

            var properties = jsonData.Properties();

            foreach (var obj in properties.Select((value, i) => (value, i)))
            {
                string key = obj.value.Name;
                int index = obj.i;

                List<int> deckList = new List<int>();
                JArray jsonDeck = JArray.Parse(jsonData[key]["deck"].Value<string>());
                if (jsonDeck.Count > 0)
                {
                    for (var i = 0; i < jsonDeck.Count; i++)
                    {
                        var checkIndex = 0;
                        var data = jsonDeck[i].Value<string>();
                        var isInt = int.TryParse(data, out checkIndex);
                        if (isInt)
                        {
                            deckList.Add(checkIndex);
                        }
                        else
                        {
                            deckList.Add(0);
                        }
                    }
                }
                TeamFormation.Add(deckList);

                List<int> itemList = new List<int>();
                JToken jsonItem = JArray.Parse(jsonData[key]["items"].Value<string>());
                itemList = jsonItem.ToObject<List<int>>();

                TeamPart.Add(itemList);
            }
        }

        public void AllClearFormationData()
        {
            TeamFormation.Clear();
        }

        public void ClearFomationData(int tag)
        {
            TeamFormation[tag].Clear();
        }

        public void SetFormationData(int tag, List<int> dragonList)
        {
            if (TeamFormation.Count > tag)
                TeamFormation[tag] = dragonList;
            else
                TeamFormation[0] = dragonList;
        }
        public List<int> GetFormaiotnData(int tag)
        {
            if (0 <= tag && TeamFormation.Count > tag)
                return TeamFormation[tag];

            return null;
        }
    }
}