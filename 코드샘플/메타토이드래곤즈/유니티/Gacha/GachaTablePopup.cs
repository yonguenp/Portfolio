using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class GachaTableData : PopupData
    {
        public GachaTableData(int menuID)
        {
            MenuID = menuID;
            menuData = GachaMenuData.Get(MenuID);
        }

        public GachaTableData(List<GachaTypeData> _typeDataList , string titleStr)
        {
            MenuID = 0;
            menuData = null;
            gachaTypeDataList = _typeDataList.ToList();
            customTitleStr = titleStr;
        }

        public int MenuID { get; private set; } = 0;
        public GachaMenuData menuData = null;
        public List<GachaTypeData> gachaTypeDataList = null;
        public string customTitleStr = "";
    }

    public class GachaTablePopup : Popup<GachaTableData>
    {
        #region OpenPopup
        public static GachaTablePopup OpenPopup(int menuID)
        {
            var popup = PopupManager.OpenPopup<GachaTablePopup>(new GachaTableData(menuID));
            if(popup != null)
            {
                popup.SetPetRateBoardBtn(menuID >= 300 && menuID < 400);
            }
            return popup;
        }
        public static GachaTablePopup OpenPopup(int _gachaTypeMenuID, string _titleStr)
        {
            List<GachaTypeData> dataList = new List<GachaTypeData>() { GachaTypeData.Get(_gachaTypeMenuID) };
            return PopupManager.OpenPopup<GachaTablePopup>(new GachaTableData(dataList, _titleStr));
        }
        #endregion

        [SerializeField] ScrollRect scrollRect = null;
        [SerializeField] Text titleText = null;

        [SerializeField] GameObject gachaTableClone = null;
        [SerializeField] GameObject gachaTableSubClone = null;
        [SerializeField] GameObject gachaTableGroupClone = null;

        [SerializeField] GameObject UpArrow = null;
        [SerializeField] GameObject DownArrow = null;

        [SerializeField] GameObject charTableGuideLayer = null;
        [Header("pet rate")]
        [SerializeField] GameObject petRateBoardBtnObj = null;

        List<GachaTypeData> gachaTypeDataList = null;

        List<GachaTableClone> gachaTableCloneList = new();

        float maxRate = 0f;

        #region Initialize
        public override void InitUI()
        {
            InitTitle();
            InitData();
        }

        private void InitTitle()
        {
            if (titleText == null) return;

            var IsEmptyCustomTitle = string.IsNullOrEmpty(Data.customTitleStr);
            var IsNullData = Data.menuData == null;
            var titleName = (!IsNullData && IsEmptyCustomTitle) ? Data.menuData.Name : IsNullData && !IsEmptyCustomTitle ? Data.customTitleStr : "";
            titleText.text = StringData.GetStringFormatByStrKey("뽑기안내_팝업_타이틀", titleName);
        }

        private void InitData()
        {
            if (Data == null || (Data.menuData == null && Data.gachaTypeDataList == null)) return;

            var isAlreadyList = Data.gachaTypeDataList != null;
            if (isAlreadyList)
                gachaTypeDataList = Data.gachaTypeDataList;
            else
                gachaTypeDataList = Data.menuData.typeDatas;

            if (gachaTypeDataList == null || gachaTypeDataList.Count <= 0) return;

            ClearData();

            // 팝업을 구성할 데이터 세팅
            SetGachaTableData();
            
            charTableGuideLayer.SetActive(isAlreadyList ? false : Data.menuData.menu_type != 3);
        }

        void SetGachaTableData()
        {
            gachaTableClone.SetActive(true);
            gachaTableSubClone.SetActive(true);
            
            if (gachaTypeDataList[0] != null)
            {
                var target = gachaTypeDataList[0];
                string optionKey = target.proc_group.ToString();
                switch (target.proc_group)
                {
                    case 104:
                    case 105:
                    case 106:
                    case 107:
                    case 108:
                    case 109:
                        optionKey = "job";
                        break;
                }

                ServerOptionData option_data = ServerOptionData.Get("gacha_" + optionKey);
                foreach (GachaRateData rateData in target.Rate)
                {
                    float weight = 0;
                    if (option_data != null)
                    {
                        string valueKey = rateData.GetServerOptionGachaKey();

                        if (!string.IsNullOrEmpty(valueKey))
                            weight = option_data.GetJsonValueFloat(valueKey, rateData.weight);
                    }
                    else
                    {
                        weight = rateData.weight;
                    }
                    maxRate += weight;

                    // 메인 확률표 생성
                    GameObject newTableClone = Instantiate(gachaTableClone, scrollRect.content);
                    GachaTableClone tableClone = newTableClone.GetComponent<GachaTableClone>();

                    List<GachaTableSubClone> subList = new List<GachaTableSubClone>();
                    // 서브 확률표 생성
                    var resultRateData = GachaRateData.GetGroup(rateData.result_id);
                    resultRateData.Sort((a,b) => { return a.sort.CompareTo(b.sort); });
                    bool subOpen = false;
                    if (resultRateData != null && resultRateData.Count > 0)
                    {
                        for (int i = 0; i < resultRateData.Count; ++i)
                        {
                            switch (resultRateData[i].reward_type)
                            {
                                case "CHAR":
                                case "PET":
                                {
                                    // 세부 항목 확률표 생성
                                    GameObject newSubClone = Instantiate(gachaTableSubClone, scrollRect.content);
                                    var sub = newSubClone.GetComponent<GachaTableSubClone>();
                                    sub.InitSubClone(resultRateData[i]);
                                    subList.Add(sub);
                                }
                                break;
                                case "DICE_GROUP":
                                {
                                    var sub = SetDiceGroup(resultRateData[i].result_id);
                                    subList.AddRange(sub);
                                }
                                break;
                            }
                        }
                    }

                    subOpen = target.Rate.Count == 1;

                    tableClone.InitClone(rateData, subList, this, weight);
                    tableClone.SetSubTable(subOpen);

                    gachaTableCloneList.Add(tableClone);
                }
            }

            gachaTableClone.SetActive(false);
            gachaTableSubClone.SetActive(false);

            if (gachaTableCloneList != null && gachaTableCloneList.Count > 0)
            {
                gachaTableCloneList.ForEach(clone => clone.UpdateMaxRate(maxRate));
            }

            CancelInvoke("CheckScrollSize");
            Invoke("CheckScrollSize", 0.1f);
        }

        List<GachaTableSubClone> SetDiceGroup(int id)
        {
            List<GachaTableSubClone> ret = new List<GachaTableSubClone>();
            List<ItemGroupData> group = ItemGroupData.Get(id);
            List<GachaTableGroupClone> group_obj = new List<GachaTableGroupClone>();
            foreach (ItemGroupData data in group)
            {
                if (data.Reward.GoodType == eGoodType.DICE_GROUP)
                {
                    gachaTableGroupClone.SetActive(true);

                    GameObject newSubClone = Instantiate(gachaTableGroupClone, scrollRect.content);
                    var sub = newSubClone.GetComponent<GachaTableGroupClone>();
                    sub.InitGroupClone(data, this);
                    ret.Add(sub);

                    gachaTableGroupClone.SetActive(false);

                    group_obj.Add(sub);
                }
                else
                {
                    GameObject newSubClone = Instantiate(gachaTableSubClone, scrollRect.content);
                    var sub = newSubClone.GetComponent<GachaTableSubClone>();
                    sub.InitSubClone(data);
                    ret.Add(sub);
                }
            }

            if(group_obj.Count >= 2)
            {
                foreach(var g in group_obj)
                {
                    g.SetSubTable(false);
                }
            }

            return ret;
        }

        public void OnSubTableClear()
        {
            gachaTableCloneList.ForEach(clone => clone.SetSubTable(false));
        }

        public void CheckScrollSize()
        {
            CancelInvoke("CheckScrollSize");
            SetArrow(scrollRect.content.sizeDelta.y > scrollRect.viewport.rect.height);
        }

        void SetArrow(bool enable)
        {
            UpArrow.SetActive(enable);
            DownArrow.SetActive(enable);
        }

        public void OnUpArrow()
        {
            scrollRect.DOVerticalNormalizedPos(1.0f, 0.1f);
        }

        public void OnDownArrow()
        {
            scrollRect.DOVerticalNormalizedPos(0.0f, 0.1f);
        }

        void ClearData()
        {
            maxRate = 0f;

            gachaTableCloneList.Clear();

            SBFunc.RemoveAllChildrens(scrollRect.content);
        }

        private void GachaTableReuse(GameObject itemNode, ITableData itemData)
        {
            //if (itemNode == null)
            //    return;

            //var cloneNode = itemNode.GetComponent<GachaTableClone>();
            //if (cloneNode == null)
            //    return;
            
            //if (itemData is GachaRateData data)
            //{
            //    var characterData = CharBaseData.Get(data.result_id);
            //    if (characterData == null)
            //        return;

            //    var rate = data.weight / maxRate;
            //    cloneNode.SetInfo(StringData.GetStringByStrKey(characterData._NAME), SBFunc.StrBuilder(rate.ToString("F3"), "%"),characterData.GRADE);
            //}
        }
        #endregion

        public void SetPetRateBoardBtn(bool isEnable)
        {
            petRateBoardBtnObj.SetActive(isEnable);
        }
    }
}