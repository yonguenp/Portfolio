using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class neco_gift : game_data
{
    static public List<items> GetNecoGiftItemList()
    {
        List<items> ret = new List<items>();

        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_GIFT);
        if (necoData == null)
        {
            return ret;
        }

        object obj;
        foreach (neco_gift data in necoData)
        {
            if (data.data.TryGetValue("item_id", out obj))
            {
                items item = items.GetItem((uint)obj);

                if (item == null)
                {
                    continue;
                }

                ret.Add(item);
            }
        }

        return ret;
    }

    static public items GetRandomNecoGift(uint catID, out uint count)
    {
        count = 0; 

        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_GIFT);
        if (necoData == null)
        {
            return null;
        }

        List<items> itemList = new List<items>();
        List<uint> itemRatio = new List<uint>();
        List<uint> itemCount = new List<uint>();

        object obj;
        foreach (neco_gift data in necoData)
        {
            if(data.data.TryGetValue("id", out obj))
            {
                if (catID == (uint)obj)
                {
                    if (data.data.TryGetValue("item_id", out obj))
                    {
                        items item = items.GetItem((uint)obj);
                        if(item != null)
                        {
                            
                        }
                    }
                }
            }
        }

        if (itemList.Count == 0)
            return null;

        uint randomMax = 0;
        foreach(uint ratio in itemRatio)
        {
            randomMax += ratio;
        }


        uint choiceNum = (uint)Random.Range(0, (int)randomMax);
        int selected = 0;
        randomMax = 0;
        foreach (uint ratio in itemRatio)
        {
            randomMax += ratio;
            if (choiceNum < randomMax)
            {
                break;
            }

            selected++;
        }

        count = itemCount[selected];
        return itemList[selected];
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_GIFT; }
}

