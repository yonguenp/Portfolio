using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CraftingUI : MonoBehaviour
{
    public delegate void Callback();

    public CraftingWaitPopup CraftingWaitPopup;

    protected recipe curRecipe = null;
    private uint tryCount = 0;
    public abstract void CreftingDone();
    public abstract void Refresh();

    Callback okCallback = null;

    public void OnCrafting(recipe recipe, uint count = 1, Callback _okCallback = null)
    {
        if (recipe == null)
            return;

        okCallback = _okCallback;

        curRecipe = recipe;
        CraftingWaitPopup.SetCraftingInfo(recipe, () => {
            okCallback?.Invoke();
            OnCraftringDone();
        });
        tryCount = count;
    }

    public void OnCraftringDone()
    {
        if (curRecipe == null)
            return;

        string recipeType = string.Empty;
        string curRecipeType = curRecipe.GetRecipeType();
        if (curRecipeType == "FOOD")
        {
            recipeType = "cook";
        }
        else if (curRecipeType == "TOY" || curRecipeType == "T_MATERIAL")
        {
            recipeType = "craft";            
        }


        WWWForm data = new WWWForm();
        data.AddField("api", recipeType);
        data.AddField("op", 1);
        data.AddField("rid", curRecipe.GetRecipeID().ToString());
        data.AddField("rep", tryCount.ToString());

        NetworkManager.GetInstance().SendApiRequest(recipeType, 1, data, (response) => {
            JObject root = JObject.Parse(response);
            JToken apiToken = root["api"];
            if (null == apiToken || apiToken.Type != JTokenType.Array
                || !apiToken.HasValues)
            {
                return;
            }

            JArray apiArr = (JArray)apiToken;
            foreach (JObject row in apiArr)
            {
                string uri = row.GetValue("uri").ToString();
                if (uri == recipeType)
                {
                    JToken resultCode = row["rs"];
                    if (resultCode != null && resultCode.Type == JTokenType.Integer)
                    {
                        int rs = resultCode.Value<int>();
                        if (rs == 0)
                        {
                            Invoke("CraftDataRecived", 0.1f);
                        }
                        else
                        {
                            string msg = rs.ToString();
                            switch (rs)
                            {
                                case 1: msg = LocalizeData.GetText("LOCALIZE_508"); break;
                                case 2: msg = LocalizeData.GetText("LOCALIZE_48"); break;
                            }

                            NecoCanvas.GetPopupCanvas().OnSystemMessagePopupShow(LocalizeData.GetText("LOCALIZE_278"), msg);
                        }
                    }
                }
            }
        });
    }

    public void CraftDataRecived()
    {
        CreftingDone();
    }

    public void ApplyGuideTryCount(int craftTryCount)
    {
        int guideQuestCount = PlayerPrefs.GetInt("GUIDE_QUEST_COUNT", 1);
        guideQuestCount -= (craftTryCount - 1);
        if (guideQuestCount < 0)
            guideQuestCount = 0;

        PlayerPrefs.SetInt("GUIDE_QUEST_COUNT", guideQuestCount);
    }
}
