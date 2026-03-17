using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class ProductLayerQueueTab : MonoBehaviour
    {
        int index= 0;
        int buildingTag = 0;
        string buildingIndex = "";

        [SerializeField]
        Image buttonImage = null;
        [SerializeField]
        Text tabText = null;

        [SerializeField]
        Sprite selectedStateSprite = null;
        [SerializeField]
        Sprite disableStateSprite = null;

        private TimeObject timeObj = null;

        public void SetData(string paramBuildIndex ,int paramBuildTag ,int paramIndex)
        {
            buildingIndex = paramBuildIndex;
            buildingTag = paramBuildTag;
            index = paramIndex;

            if (tabText != null)
            {
                tabText.text = index.ToString();
            }

            bool bReddot = false;
            var reddot = SBFunc.GetChildrensByName(transform, new string[] { "reddot" });
            if (reddot != null)
                reddot.gameObject.SetActive(bReddot);
            timeObj = gameObject.AddComponent<TimeObject>();
            BuildInfo buildingInfo = User.Instance.GetUserBuildingInfoByTag(buildingTag);

            timeObj.Refresh = () =>
            {
                if ((buildingInfo.State == eBuildingState.CONSTRUCT_FINISHED) ||
                    (buildingInfo.State == eBuildingState.CONSTRUCTING && buildingInfo.ActiveTime <= TimeManager.GetTime()))
                {
                    bReddot = true;
                }
                else
                {
                    if (buildingInfo.State != eBuildingState.NORMAL)
                        return;

                    ProducesBuilding produces = User.Instance.GetProduces(buildingInfo.Tag);

                    if (produces == null || produces.Items.Count == 0)
                        return;

                    if (buildingInfo.Tag < 1100)
                    {
                        produces.Items.ForEach((rElement) =>
                        {
                            if (rElement.ProductionExp == 0 || rElement.ProductionExp <= TimeManager.GetTime())
                            {
                                bReddot = true;
                                return;
                            }
                        });
                    }
                    else
                    {
                        List<ProducesRecipe> itemList = produces.Items;
                        if(itemList != null && itemList.Count > 0)
                        {
                            for(int i = 0; i < itemList.Count; i++)
                            {
                                if (bReddot)
                                    continue;

                                ProducesRecipe recipeData = itemList[i];
                                if(recipeData == null)
                                {
                                    continue;
                                }

                                ProductData itemReceipe = ProductData.GetProductDataByGroupAndKey(buildingIndex, recipeData.RecipeID);
                                if (itemReceipe == null)
                                    continue;

                                if (recipeData.State == eProducesState.Complete ||
                                    recipeData.State == eProducesState.Ing && 
                                    (recipeData.ProductionExp + index * itemReceipe.PRODUCT_TIME) <= TimeManager.GetTime())
                                {
                                    bReddot = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (reddot != null)
                    reddot.gameObject.SetActive(bReddot);
            };

            TimeManager.AddObject(timeObj);
        }

        public void SwitchTabState(bool isSelected)
        {
            if (buttonImage == null) { return; }

            buttonImage.sprite = isSelected ? selectedStateSprite : disableStateSprite;
        }

        void OnDestroy()
        {
            if (timeObj != null)
            {
                timeObj.Refresh = null;
                TimeManager.DelObject(timeObj);
            }
        }
    }
}
