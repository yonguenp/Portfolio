using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class neco_action : game_data
{
    static public clip_event GetRandomAction(uint catID, uint foodID, uint object_id)
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.NECO_ACTION);
        if (necoData == null)
        {
            return null;
        }

        List<clip_event> listClip = new List<clip_event>();
        object obj;
        foreach(neco_action action in necoData)
        {
            if(action.data.TryGetValue("cat_id", out obj))
            {
                if (catID == (uint)obj)
                {
                    if (action.data.TryGetValue("food_id", out obj))
                    {
                        if(foodID == (uint)obj)
                        {
                            if (action.data.TryGetValue("object_id", out obj))
                            {
                                if (object_id == (uint)obj)
                                {
                                    if (action.data.TryGetValue("clip_id", out obj))
                                    {
                                        clip_event clip = clip_event.GetClipEvent((uint)obj);
                                        if (action.data.TryGetValue("probability", out obj))
                                        {
                                            for (int i = 0; i < (uint)obj; i++)
                                            {
                                                listClip.Add(clip);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (listClip.Count == 0)
            return null;
        return listClip[Random.Range(0, listClip.Count)];
    }

    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.NECO_ACTION; }
}

