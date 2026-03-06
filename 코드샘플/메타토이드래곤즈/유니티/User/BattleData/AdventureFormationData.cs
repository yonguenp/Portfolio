using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class AdventureFormationData
    {
        //팀배치 데이터
        public List<List<int>> TeamFormation { get; private set; } = new List<List<int>>();
        public List<List<int>> TeamPart { get; private set; } = new List<List<int>>();

        public void SetData(JToken jsonData)
        {
            if (jsonData == null) { return; }


            List<int> deckList = new List<int>();
            if (jsonData != null && jsonData["deck"] != null)
            {
                JArray jsonDeck = JArray.Parse(jsonData["deck"].Value<string>());
                if (jsonDeck.Count > 0)
                {
                    for (var j = 0; j < jsonDeck.Count; j++)
                    {
                        var checkIndex = 0;
                        var data = jsonDeck[j].Value<string>();
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
            }
            TeamFormation.Add(deckList);

            List<int> itemList = new List<int>();
            if (jsonData != null && jsonData["items"] != null)
            {
                JToken jsonItem = JArray.Parse(jsonData["items"].Value<string>());
                itemList = jsonItem.ToObject<List<int>>();
            }
            TeamPart.Add(itemList);
        }

        public bool IsEmpty()
        {
            return TeamFormation[0].Count + TeamFormation[1].Count + TeamFormation[2].Count == 0;
        }
        public void SetDeckSize()
        {
            if (TeamFormation.Count < 3)
            {
                for (int i = TeamFormation.Count; i < 3; ++i)
                {
                    TeamFormation.Add(new List<int>());
                }
            }
            if (TeamPart.Count < 3)
            {
                for (int i = TeamPart.Count; i < 3; ++i)
                {
                    TeamPart.Add(new List<int>());
                }
            }
        }

        public void AllClearFormationData()
        {
            //team_formation = [[], [], []];
            TeamFormation.Clear();
        }

        public void ClearFomationData(int tag)
        {
            TeamFormation[tag].Clear();
        }

        public void SetFormationData(int tag, List<int> dragonList)
        {
            TeamFormation[tag] = dragonList;
        }

        public bool Contains(int tag)
        {
            foreach (var line in TeamFormation)
            {
                if (line.Contains(tag))
                    return true;
            }

            return false;
        }
        public List<int> GetFormation(int index)
        {
            if (TeamFormation == null)
                return null;
            if (TeamFormation.Count <= index || index < 0)
                return null;

            return TeamFormation[index];
        }
    }
}