using SandboxNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GemBlackSmith : MonoBehaviour
{
    [System.Serializable]
    public class Smith {
        [SerializeField] Image[] stuffs;
        [SerializeField] Text[] stuffAmount;
        [SerializeField] Text NeedGold;
        [SerializeField] Button run;
        [SerializeField] GameObject Dim;

        RecipeBaseData Data = null;
        bool hasForge = false;
        public void Refresh(int itemNo, int checkInven, bool opened)
        {
            Data = RecipeBaseData.GetRecipeData(itemNo);
            if (Data == null)
                return;

            hasForge = opened;
            Refresh();
        }

        void Refresh()
        {
            Dim.SetActive(!hasForge);

            var materialData = RecipeMaterialData.GetDataByGroup(int.Parse(Data.GetKey()));
            if (materialData == null)
                return;

            int stuffIndex = 0;
            foreach (var material in materialData)
            {
                switch (material.TYPE)
                {
                    case eGoodType.GOLD:
                        NeedGold.text = SBFunc.CommaFromNumber(material.VALUE);
                        break;
                    case eGoodType.ITEM:
                        if (stuffs.Length > stuffIndex)
                        {
                            stuffs[stuffIndex].sprite = ItemBaseData.Get(material.PARAM).ICON_SPRITE;
                            stuffAmount[stuffIndex].text = SBFunc.CommaFromNumber(material.VALUE);
                            stuffIndex++;
                        }
                        break;
                }
            }
        }

        public void OnMake()
        {
            var popup = PopupManager.OpenPopup<ItemMakePopup>(new ItemMakePopupData(int.Parse(Data.GetKey()), StringData.GetStringByStrKey("제작")));
            popup.SetRefreshCallBack(Refresh);
        }
    } 

    [SerializeField] Smith ChipsetSmith;    
    [SerializeField] Smith GoldGemSmith;

    [SerializeField] GameObject Dim;

    public void Refresh()
    {
        bool nanochipset_smith_opened = false;
        bool goldgem_smith_opened = false;
        var GemDungeonData = LandmarkGemDungeon.Get();
        if (GemDungeonData != null)
        {
            nanochipset_smith_opened = GemDungeonData.NanochipsetSmithOpened;
            goldgem_smith_opened = GemDungeonData.GoldGemSmithOpened;
        }
        ChipsetSmith.Refresh(110000005, 62000004, nanochipset_smith_opened);
        GoldGemSmith.Refresh(110000006, 62000005, goldgem_smith_opened);

        bool SmithOpened = false;
        var data = User.Instance.GetLandmarkData<LandmarkGemDungeon>();
        if(data != null)
        {
            SmithOpened = data.BuildInfo.Level > 1;
        }

        Dim.SetActive(!SmithOpened);
    }

    public void OnOpenSmith()
    {
        var Data = BuildingLevelData.GetDataByGroupAndLevel("gemdungeon", 1);
        if (Data != null && Data.NEED_AREA_LEVEL > User.Instance.TownInfo.AreaLevel)
        {
            ToastManager.On(StringData.GetStringByStrKey("대장간타운레벨제한안내"));
        }

        int tag = -1;

        foreach(var od in BuildingOpenData.GetByBuildingGroup("gemdungeon"))
        {
            tag = od.INSTALL_TAG;
            if (tag > 0)
                break;
        }        

        var popup = PopupManager.OpenPopup<BuildingUpgradePopup>(new BuildingPopupData(tag));
        popup.SetUpgradeCallBack(() => { 
            PopupManager.GetPopup<GemDungeonPopup>().ClosePopup();
            foreach(var gd in Town.Instance.Gemdungeon)
            {
                if(gd.Value != null)
                {
                    gd.Value.ActiveAction();
                }
            }
        });
    }

    public void OnContructChipsetSmith()
    {
        var popup = PopupManager.OpenPopup<ItemMakePerfectPopup>(new ItemMakePopupData(7, StringData.GetStringByStrKey("나노칩셋대장간오픈"), true));
        popup.SetRefreshCallBack(Refresh);
    }

    public void OnContructGoldGemSmith()
    {
        var popup = PopupManager.OpenPopup<ItemMakePerfectPopup>(new ItemMakePopupData(8, StringData.GetStringByStrKey("골드잼대장간오픗"), true));
        popup.SetRefreshCallBack(Refresh);
    }

    public void OnMakeChipset()
    {
        ChipsetSmith.OnMake();
    }

    public void OnMakeGoldGem()
    {
        GoldGemSmith.OnMake();
    }
}
