using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

[Serializable]
public class recipe : game_data
{
    public override GameDataManager.DATA_TYPE GetDataType() { return GameDataManager.DATA_TYPE.RECIPE; }

    static public void ClearLocalizeData()
    {
        List<game_data> necoData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
        if (necoData == null)
        {
            return;
        }

        foreach (recipe data in necoData)
        {
            data.recipeName = "";
            data.recipeDesc = "";
        }
    }

    static public recipe GetRecipe(uint recipeID)
    {
        List<game_data> recipelist = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
        if (recipelist == null)
            return null;

        foreach (recipe data in recipelist)
        {
            if (data.GetRecipeID() == recipeID)
            {
                return data;
            }
        }

        return null;
    }

    static public List<recipe> GetRecipeListByLevel(string type, uint recipeLevel)
    {
        List<game_data> recipeListData = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
        if (recipeListData == null || recipeLevel > 10)
        {
            return null;
        }

        List<recipe> result_list = new List<recipe>();

        object obj;
        foreach (recipe recipeData in recipeListData)
        {
            if (recipeData.data.TryGetValue("recipe_type", out obj))
            {
                if ((string)obj != type) continue;
                
                if (recipeData.data.TryGetValue("need_level", out obj))
                {
                    if ((uint)obj != recipeLevel) continue;
                    
                    result_list.Add(recipeData);
                }
            }
        }

        return result_list;
    }

    [NonSerialized]
    private uint recipeID = 0;
    public uint GetRecipeID()
    {
        if (recipeID == 0)
        {
            object obj;
            if (data.TryGetValue("recipe_id", out obj))
            {
                recipeID = (uint)obj;
            }
        }

        return recipeID;
    }

    [NonSerialized]
    private string recipeName = "";
    public string GetRecipeName()
    {
        if (recipeName == "")
        {
            recipeName = LocalizeData.GetText("recipe:name_kr:" + GetRecipeID().ToString());
        }

        return recipeName;
    }

    [NonSerialized]
    private string recipeDesc = "";
    public string GetRecipeDesc()
    {
        if (recipeDesc == "")
        {
            recipeDesc = LocalizeData.GetText("recipe:recipe_desc_kr:" + GetRecipeID().ToString());
        }

        return recipeDesc;
    }

    [NonSerialized]
    private string recipeIcon = "";
    public string GetRecipeIcon()
    {
        if (recipeIcon == "")
        {
            object obj;
            if (data.TryGetValue("icon_img", out obj))
            {
                recipeIcon = (string)obj;
            }
        }

        return recipeIcon;
    }

    [NonSerialized]
    private string recipeType = "";
    public string GetRecipeType()
    {
        if (recipeType == "")
        {
            object obj;
            if (data.TryGetValue("recipe_type", out obj))
            {
                recipeType = (string)obj;
            }
        }

        return recipeType;
    }

    [NonSerialized]
    private uint recipeLevel = 0;
    public uint GetRecipeLevel()
    {
        if (recipeLevel == 0)
        {
            object obj;
            if (data.TryGetValue("need_level", out obj))
            {
                recipeLevel = (uint)obj;
            }
        }

        return recipeLevel;
    }

    public bool HasRecipeItem()
    {
        List<game_data> user_items = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.USER_ITEMS);
        foreach (game_data data in user_items)
        {
            user_items userItem = (user_items)data;
            uint userItem_id = userItem.GetItemID();
            var outputItem = GetOutputItem(0);

            if (outputItem.Key == userItem_id)
            {
                return true;
            }
        }

        return false;
    }

    [NonSerialized]
    private List<List<KeyValuePair<uint, uint>>> recipeInput = null;

    public int GetRecipeCaseCount()
    {
        if(recipeInput == null)
        {
            GetNeedItems(0);
        }

        if (recipeInput == null)
            return 0;

        return recipeInput.Count;
    }
    public List<KeyValuePair<uint, uint>> GetNeedItems(int index)
    {
        if (recipeInput == null)
        {
            recipeInput = new List<List<KeyValuePair<uint, uint>>>();
            object obj;
            if (data.TryGetValue("value", out obj))
            {
                JArray values = JArray.Parse((string)obj);
                foreach (JArray val in values)
                {
                    List<KeyValuePair<uint, uint>> list = new List<KeyValuePair<uint, uint>>();
                    recipeInput.Add(list);

                    JArray inputs = (JArray)val[0];
                    foreach (JArray input in inputs)
                    {
                        uint itemID = (uint)input[0];
                        uint itemCount = (uint)input[1];

                        list.Add(new KeyValuePair<uint, uint>(itemID, itemCount));
                    }
                }
            }
        }

        if (recipeInput.Count > index)
            return recipeInput[index];
        else
            return new List<KeyValuePair<uint, uint>>();
    }

    [NonSerialized]
    private List<KeyValuePair<uint, uint>> recipeOutput;
    public KeyValuePair<uint, uint> GetOutputItem(int index)
    {
        if (recipeOutput == null)
        {
            recipeOutput = new List<KeyValuePair<uint, uint>>();
            object obj;
            if (data.TryGetValue("value", out obj))
            {
                JArray values = JArray.Parse((string)obj);
                foreach (JArray val in values)
                {
                    JArray output = (JArray)val[1];

                    uint itemID = (uint)output[0];
                    uint itemCount = (uint)output[1];

                    recipeOutput.Add(new KeyValuePair<uint, uint>(itemID, itemCount));
                }
            }
        }

        if (recipeOutput.Count > index)
            return recipeOutput[index];
        else
            return new KeyValuePair<uint, uint>();
    }
}

