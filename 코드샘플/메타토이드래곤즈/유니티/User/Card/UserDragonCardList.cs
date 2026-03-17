using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SandboxNetwork
{
    public class UserDragonCardList
    {
        private Dictionary<int, UserDragonCard> cards = null;
        public UserDragonCardList()
        {
            cards = new Dictionary<int, UserDragonCard>();
        }

        public UserDragonCard GetCardTag(int cTag)
        {
            if (cards.ContainsKey(cTag))
                return cards[cTag];

            return null;
        }

        public List<UserDragonCard> GetAllList()
        {
            return cards.Values.ToList();
        }

        public List<UserDragonCard> GetDragonTag(int dTag)
        {
            return cards.Values.ToList().FindAll(element => element.DragonTag == dTag);
        }

        public void Set(JObject jsonData)
        {
            if (jsonData.ContainsKey("api"))
            {
                switch (jsonData.Value<string>("api"))
                {
                    case "dragon_card_new":
                    {
                        if (jsonData.ContainsKey("cards"))
                        {
                            var jArrayObj = (JArray)jsonData["cards"];

                            if (jArrayObj.Type == JTokenType.Array)
                            {
                                int cardsCount = jArrayObj.Count;
                                for (int i = 0; i < cardsCount; i++)
                                {
                                    UserDragonCard card = new UserDragonCard();
                                    if (card.Set(jArrayObj[i]))
                                    {
                                        cards.Add(card.CardTag, card);
                                    }
                                }
                            }
                        }
                    }
                    break;

                    case "dragon_card_del":
                    {
                        if (jsonData.ContainsKey("dcid"))
                        {
                            JArray jObjcids = jsonData.Value<JArray>("dcid");
                            List<int> cids = jObjcids.ToObject<List<int>>();
                            if (cids.Count > 0)
                            {
                                foreach (int cid in cids)
                                {
                                    cards.Remove(cid);
                                }
                            }
                        }
                    }
                    break;
                }
            }
            else if (jsonData.ContainsKey("dragon_cards"))
            {
                JArray jObjcards = (JArray)jsonData["dragon_cards"];
                if (jObjcards.Type == JTokenType.Array)
                {
                    int cardsCount = jObjcards.Count;
                    if (cardsCount <= 0)
                    {
                        return;
                    }

                    for (int i = 0; i < cardsCount; i++)
                    {
                        UserDragonCard card = new UserDragonCard();
                        if (card.Set(jObjcards[i]))
                        {
                            cards.Add(card.CardTag, card);
                        }
                    }
                }
            }            
        }

        public void Clear()
        {
            cards.Clear();
        }
    }
}