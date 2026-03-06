using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookUI : MonoBehaviour
{
    public GameObject RecipeListContainer;
    public GameObject RecipeItemCloneObject;
    public ScrollRect RecipeListScrollRect;

    public GameObject RecipeInfoObjectContainer;
    public GameObject RecipeInfoCloneObject;
    public ScrollRect RecipeInfoScrollRect;

    public GameObject MaterialContainer;
    public GameObject MaterialCloneObject;
    public Text MaterialDesc;

    public InputField TryCookCounter;
    public Button MinusCounter;
    public Button PlusCounter;
    public Button MaxCounter;
    public RewardListUI ResultRewardUI;

    public GameObject CookingGaugeObject;
    public Image CookingGauge;
    public Text CookingText;
    public GameObject CookSuccess;
    public GameObject CookFail;

    public GameObject ExitButton;
    public Button CookStartButton;

    private recipe curSelectedRecipeData = null;    
    private uint maxTryCount = 0;

    private void OnEnable()
    {
        curSelectedRecipeData = null;
        SetRecipeListUI();

        CookingGaugeObject.SetActive(false);
        CookingGauge.fillAmount = 0.0f;
        CookingText.text = "";
        CookSuccess.SetActive(false);
        CookFail.SetActive(false);
    }

    public void SetRecipeListUI()
    {
        foreach(Transform child in RecipeListContainer.transform)
        {
            if (child.gameObject != RecipeItemCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        RecipeItemCloneObject.SetActive(true);

        List<game_data> recipeList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
        foreach(game_data recipeData in recipeList)
        {
            recipe recipe = (recipe)recipeData;
            GameObject recipeUI = Instantiate(RecipeItemCloneObject);
            recipeUI.transform.SetParent(RecipeListContainer.transform);
            recipeUI.transform.localScale = RecipeItemCloneObject.transform.localScale;
            recipeUI.transform.localPosition = RecipeItemCloneObject.transform.localPosition;

            RecipeItem recipeComponent = recipeUI.GetComponent<RecipeItem>();
            recipeComponent.SetRecipeData(recipe, this);

            if(curSelectedRecipeData == null)
            {
                if (recipe.GetRecipeCaseCount() > 0)
                {
                    SetRecipeInfoLoad(recipe);
                }
            }
        }

        RecipeItemCloneObject.SetActive(false);
    }

    public void SetRecipeInfoLoad(recipe recipiData)
    {
        curSelectedRecipeData = recipiData;

        foreach (Transform child in RecipeInfoObjectContainer.transform)
        {
            if (child.gameObject != RecipeInfoCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        //RecipeInfoCloneObject.SetActive(true);
        
        //for (int i = 0; i < recipiData.GetRecipeCaseCount(); i++)
        //{
        //    GameObject recipeUI = Instantiate(RecipeInfoCloneObject);
        //    recipeUI.transform.SetParent(RecipeInfoObjectContainer.transform);
        //    recipeUI.transform.localScale = RecipeInfoCloneObject.transform.localScale;
        //    recipeUI.transform.localPosition = RecipeInfoCloneObject.transform.localPosition;
        //    recipeUI.GetComponent<RecipeInfoItem>().SetRecipeInfoData(recipiData.GetNeedItems(i), recipiData.GetOutputItem(i));            
        //}

        //RecipeInfoCloneObject.SetActive(false);


        SetRecipeMaterialItemListUI(curSelectedRecipeData, 0);

        List<game_data> recipeList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
        int index = 0;
        for (int i = 0; i < recipeList.Count; i++)
        {
            if(recipeList[i] == recipiData)
            {
                index = i;
            }
        }

        RecipeInfoCloneObject.SetActive(true);
        RecipeInfoCloneObject.GetComponent<RecipeInfoItem>().SetRecipeInfoData(recipiData.GetNeedItems(0), recipiData.GetOutputItem(0));

        RecipeListScrollRect.DOVerticalNormalizedPos(1.0f - (float)index / (recipeList.Count - 1), 0.1f);
    }

    public void ScrollChangedCallBack(Vector2 value)
    {
        CancelInvoke("RefreshRecipeScrollPivot");
        Invoke("RefreshRecipeScrollPivot", 0.1f);
    }

    public void RefreshRecipeScrollPivot()
    {
        List<game_data> recipeList = GameDataManager.Instance.GetGameData(GameDataManager.DATA_TYPE.RECIPE);
        int index = (int)(Mathf.Round((1.0f - RecipeListScrollRect.verticalNormalizedPosition) * (recipeList.Count - 1)));
        SetRecipeInfoLoad((recipe)recipeList[index]);
    }

    public void SetRecipeInfoUI(KeyValuePair<uint, uint> input)
    {
        int index = 0;
        for(int i = 0; i < curSelectedRecipeData.GetRecipeCaseCount(); i++)
        {
            foreach (KeyValuePair<uint, uint> need in curSelectedRecipeData.GetNeedItems(i))
            {
                if (need.Key == input.Key)
                {
                    index = i;
                    break;
                }
            }
        }

        RecipeInfoCloneObject.SetActive(true);
        RecipeInfoCloneObject.GetComponent<RecipeInfoItem>().SetRecipeInfoData(curSelectedRecipeData.GetNeedItems(index), curSelectedRecipeData.GetOutputItem(index));

        //RecipeInfoScrollRect.DOHorizontalNormalizedPos((float)index / (curSelectedRecipeData.GetRecipeCaseCount() - 1), 0.1f);
    }

    public void InfoScrollChangedCallBack(Vector2 value)
    {
        CancelInvoke("RefreshInfoScrollPivot");
        Invoke("RefreshInfoScrollPivot", 0.1f);
    }

    public void RefreshInfoScrollPivot()
    {
        if (curSelectedRecipeData == null)
            return;

        int index = (int)(Mathf.Round(RecipeInfoScrollRect.horizontalNormalizedPosition * (curSelectedRecipeData.GetRecipeCaseCount() - 1)));
        SetRecipeInfoUI(curSelectedRecipeData.GetNeedItems(index)[0]);
    }

    public void SetRecipeMaterialItemListUI(recipe target, int index)
    {
        MaterialCloneObject.SetActive(true);

        foreach (Transform child in MaterialContainer.transform)
        {
            if (child.gameObject != MaterialCloneObject)
            {
                Destroy(child.gameObject);
            }
        }

        if (target.GetRecipeCaseCount() == 0)
        {
            MaterialCloneObject.SetActive(false);
            return;
        }

        for (int i = 0; i < target.GetRecipeCaseCount(); i++)
        {
            List<KeyValuePair<uint, uint>> itemList = target.GetNeedItems(i);

            foreach (KeyValuePair<uint, uint> input in itemList)
            {
                GameObject userItemUI = Instantiate(MaterialCloneObject);
                userItemUI.transform.SetParent(MaterialContainer.transform);
                userItemUI.transform.localScale = MaterialCloneObject.transform.localScale;
                userItemUI.transform.localPosition = MaterialCloneObject.transform.localPosition;

                RecipeMaterial recipeMaterial = userItemUI.GetComponent<RecipeMaterial>();
                recipeMaterial.SetMarerialData(i, input, target, this);

                recipeMaterial.SetCursor(i == index);
                if(i == index)
                {
                    maxTryCount = recipeMaterial.GetTryMaxCount();
                    SetCookCounterUI(0);
                }
            }
        }

        MaterialCloneObject.SetActive(false);
    }

    public void SetMaterialCursor(KeyValuePair<uint, uint> input, recipe recipeData)
    {
        maxTryCount = 0;

        foreach (Transform child in MaterialContainer.transform)
        {
            RecipeMaterial component = child.GetComponent<RecipeMaterial>();
            bool bCusor = component.GetInputInfo().Key == input.Key;
            if (bCusor)
            {
                maxTryCount = component.GetTryMaxCount();                
            }
        }

        SetRecipeInfoUI(input);


        items item = items.GetItem(input.Key);
        if(item != null)
            MaterialDesc.text = item.GetItemDesc();
        

        int index = 0;
        for (int i = 0; i < curSelectedRecipeData.GetRecipeCaseCount(); i++)
        {
            foreach (KeyValuePair<uint, uint> need in curSelectedRecipeData.GetNeedItems(i))
            {
                if (need.Key == input.Key)
                {
                    index = i;
                    break;
                }
            }
        }

        SetRecipeMaterialItemListUI(recipeData, index);
        SetCookCounterUI(0);
    }

    public void InputChanged()
    {
        TryCookCounter.text = int.Parse(TryCookCounter.text).ToString();
    }
    public void SetCookCounterUI(int typeCase)
    {
        int count = int.Parse(TryCookCounter.text);
        switch(typeCase)
        {
            case 0:
                count = maxTryCount > 0 ? 1 : 0;
                break;
            case 1:
                count -= 1;
                break;
            case 2:
                count += 1;
                break;
            case 3:
                count = (int)maxTryCount;
                break;
        }

        if(count > maxTryCount)
            count = (int)maxTryCount;
        
        if (count < 0)
            count = 0;

        CookStartButton.interactable = count > 0;

        TryCookCounter.text = count.ToString();
    }

    public void OnCookStart()
    {
        if (int.Parse(TryCookCounter.text) <= 0)
        {
            return;
        }

        int index = 0;
        for(int i = 0; i < MaterialContainer.transform.childCount; i++)
        {
            RecipeMaterial material = MaterialContainer.transform.GetChild(i).GetComponent<RecipeMaterial>();
            if(material.isFocusCursor())
            {
                index = material.GetIndex();
                break;
            }
        }

        WWWForm data = new WWWForm();
        data.AddField("api", "cook");
        data.AddField("rid", curSelectedRecipeData.GetRecipeID().ToString());
        data.AddField("type", index);
        data.AddField("rep", TryCookCounter.text);

        NetworkManager.GetInstance().SendApiRequest("cook", 1, data, (response) => {
            OnCookingGaugeAction(0, response);
        });

        CookStartButton.interactable = false;
        MinusCounter.interactable = false;
        PlusCounter.interactable = false;
        MaxCounter.interactable = false;
        ExitButton.SetActive(false);
    }

    public void OnCookDone()
    {
        int index = 0;
        for (int i = 0; i < MaterialContainer.transform.childCount; i++)
        {
            RecipeMaterial material = MaterialContainer.transform.GetChild(i).GetComponent<RecipeMaterial>();
            if (material.isFocusCursor())
            {
                index = material.GetIndex();
                break;
            }
        }

        SetRecipeMaterialItemListUI(curSelectedRecipeData, index);
        CookStartButton.interactable = true;
        MinusCounter.interactable = true;
        PlusCounter.interactable = true;
        MaxCounter.interactable = true;
        ExitButton.SetActive(true);
    }

    public void OnCookingGaugeAction(int count, string response)
    {        
        CookSuccess.SetActive(false);
        CookFail.SetActive(false);
        if(count >= int.Parse(TryCookCounter.text))
        {
            CookingGaugeObject.SetActive(false);
            ResultRewardUI.ShowCookRewardList(response);
            Invoke("OnCookDone", 0.1f);            
            return;
        }

        CookingGaugeObject.SetActive(true);

        CookingGauge.fillAmount = 0.0f;
        CookingText.text = "(" + count.ToString() + "/" + TryCookCounter.text.ToString() + ")";

        CookingGauge.DOKill();

        CookingGauge.DOFillAmount(1.0f, 2.0f).OnComplete(() => {
            CookSuccess.SetActive(true);              
            OnCookingGaugeAction(count + 1, response);
        });
    }
}
