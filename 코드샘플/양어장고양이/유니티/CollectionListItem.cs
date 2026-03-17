using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionListItem : MonoBehaviour
{
    public Text Title;
    public Text Condition;
    public Text Reward;
    public Image FlagImage;
    public Button RewardButton;

    private card_collection curData = null;
    private CardListPopup CardCanvas = null;
    public void SetCollectionListItem(card_collection data, CardListPopup cardCanvas)
    {
        curData = data;
        CardCanvas = cardCanvas;

        Title.text = "";
        Condition.text = "";
        Reward.text = "";
        Title.text = data.GetCollectionTitle();

        object obj;

        if (data.data.TryGetValue("card_list", out obj))
        {
            Condition.text = (string)obj;
        }

        if (data.data.TryGetValue("reward", out obj))
        {
            Reward.text = (string)obj;
        }

        bool bComplete = false;
        
        uint ccid = data.GetCollectionID();
        List<game_data> user_collection_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_COLLECTION);
        if (user_collection_list != null)
        {
            foreach (game_data user_collection in user_collection_list)
            {
                if (user_collection.data.TryGetValue("collection_id", out obj))
                {
                    if (ccid == (uint)obj)
                    {
                        bComplete = true;                        
                    }
                }
            }
        }

        RewardButton.interactable = false;
        if (bComplete)
        {
            FlagImage.color = Color.red;
            return;
        }


        FlagImage.color = Color.white;

        List<game_data> user_card = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_CARD);
        List<user_card> condition_card = data.GetCollectionCondition();
        if (user_card != null && condition_card != null)
        {
            int conditionCount = condition_card.Count;

            foreach (user_card cc in condition_card)
            {
                if (cc.data.TryGetValue("card_id", out obj))
                {
                    uint id = (uint)obj;
                    uint lv = 0;

                    if (cc.data.TryGetValue("card_lv", out obj))
                    {
                        lv = (uint)obj;
                    }

                    bool hasCard = false;
                    foreach (game_data uc in user_card)
                    {
                        if (uc.data.TryGetValue("card_id", out obj))
                        {
                            if (id == (uint)obj)
                            {
                                if (uc.data.TryGetValue("card_lv", out obj))
                                {
                                    if(lv <= (uint)obj)
                                    {
                                        hasCard = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if(hasCard)
                    {
                        conditionCount--;
                    }
                }                
            }

            if(conditionCount <= 0)
            {
                FlagImage.color = Color.green;
                RewardButton.interactable = true;
            }
        }
    }

    public void OnRewardButton()
    {
        WWWForm data = new WWWForm();
        data.AddField("api", "collection");
        data.AddField("id", curData.GetCollectionID().ToString());
        
        NetworkManager.GetInstance().SendApiRequest("collection", 1, data, (response) => {
            uint ccid = curData.GetCollectionID();
            List<game_data> user_collection_list = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_COLLECTION);
            if (user_collection_list == null)
            {
                user_collection_list = GameDataManager.Instance.GetEmptyGameDataWithType(GameDataManager.DATA_TYPE.USER_COLLECTION);
            }

            object obj;
            bool bNeedRefresh = true;
            foreach (game_data user_collection in user_collection_list)
            {
                if (user_collection.data.TryGetValue("collection_id", out obj))
                {
                    if (ccid == (uint)obj)
                    {
                        bNeedRefresh = false;
                    }
                }
            }

            if (bNeedRefresh)
            {
                user_collection newData = new user_collection();
                newData.data.Add("collection_id", ccid);
                user_collection_list.Add(newData);
            }

            CardCanvas.OnCollectionReward(curData);
        });
    }
}
