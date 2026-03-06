using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SandboxNetwork
{
    public class ArenaFormationData
    {
        //팀배치 데이터(공격)
        public List<List<int>> TeamFormationATK { get; private set; } = new List<List<int>>();
        //팀장비 데이터(공격)
        public List<List<int>> TeamPartATK { get; private set; } = new List<List<int>>();
        //팀배치 데이터(방어)
        public List<List<int>> TeamFormationDEF { get; private set; } = new List<List<int>>();
        //팀배치 히든 드래곤 데이터(방어)
        public List<int> TeamFormationHidden { get; private set; } = new List<int>();
        //팀장비 데이터(방어)
        public List<List<int>> TeamPartDEF { get; private set; } = new List<List<int>>();
        
        public void SetAtkJsonData(JToken jsonData)//공격 / 방어 세팅 서버에서 어떻게 줄지 물어보기
        {   
            TeamPartATK.Clear();

            List<int> deckList = new List<int>();
            if(jsonData != null && jsonData["deck"] !=null) { 
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
            TeamFormationATK.Add(deckList);

            List<int> itemList = new List<int>();
            if (jsonData != null && jsonData["items"] != null)
            {
                JToken jsonItem = JArray.Parse(jsonData["items"].Value<string>());
                itemList = jsonItem.ToObject<List<int>>();
            }
            TeamPartATK.Add(itemList);
        }

        public void SetAtkDeckSize()
        {
            if(TeamFormationATK.Count < 5)
            {
                for(int i = TeamFormationATK.Count;  i < 5; ++i)
                {
                    TeamFormationATK.Add(new List<int>());
                }
            }
            if(TeamPartATK.Count < 5)
            {
                for (int i = TeamPartATK.Count; i < 5; ++i)
                {
                    TeamPartATK.Add(new List<int>());
                }
            }
        }

        public void SetDefJsonData(JToken jsonData)//공격 / 방어 세팅 서버에서 어떻게 줄지 물어보기
        {
            if (jsonData == null) { return; }

            TeamFormationDEF.Clear();
            TeamPartDEF.Clear();

            TeamFormationDEF.Add(jsonData["team"].ToObject<List<int>>());
            TeamPartDEF.Add(jsonData["items"].ToObject<List<int>>());
            TeamFormationHidden = jsonData["hidden"].ToObject<List<int>>();
        }

        public void AllClearATKFormationData()
        {
            TeamFormationATK.Clear();
        }

        public void ClearATKFomationData(int tag)
        {
            TeamFormationATK[tag].Clear();
        }

        public void SetATKFormationData(int tag, List<int> dragonList)
        {
            TeamFormationATK[tag] = dragonList;
        }

        public void AllClearDEFFormationData()
        {
            TeamFormationDEF.Clear();
        }

        public void ClearDEFFormationData(int tag)
        {
            TeamFormationDEF[tag].Clear();
        }

        public void ClearHiddenFormationData()
        {
            TeamFormationHidden.Clear();
        }

        public void SetDEFFormationData(int tag, List<int> dragonList)
        {
            TeamFormationDEF[tag] = dragonList;
        }

        public void SetDEFHiddenFormationData(List<int> dragonList)
        {
            TeamFormationHidden = dragonList;
        }

        public bool ContainsDEF(int tag)
        {
            foreach (var line in TeamFormationDEF)
            {
                if (line.Contains(tag))
                    return true;
            }

            return false;
        }

        public bool ContainsATK(int tag)
        {
            foreach (var line in TeamFormationATK)
            {
                if (line.Contains(tag))
                    return true;
            }

            return false;
        }

        public bool IsRegistDragonTeam(bool _isDef = true)
        {
            List<List<int>> targetFormation = _isDef ? TeamFormationDEF : TeamFormationATK;

            if (targetFormation == null)
                return false;

            int targetCount = 0;
            var it = targetFormation.GetEnumerator();
            while (it.MoveNext())
            {
                if (it.Current == null)
                    continue;

                for (int i = 0, count = it.Current.Count; i < count; ++i)
                {
                    if (it.Current[i] < 1)
                        continue;

                    targetCount++;
                }
            }

            return targetCount > 0;
        }
        public List<int> GetArenaFormationATK(int index)
        {
            if (TeamFormationATK == null || TeamFormationATK.Count <= index || index < 0)
                return null;

            return TeamFormationATK[index];
        }
        public List<int> GetArenaFormationDEF(int index)
        {
            if (TeamFormationDEF == null || TeamFormationDEF.Count <= index || index < 0)
                return null;

            return TeamFormationDEF[index];
        }
    }
}