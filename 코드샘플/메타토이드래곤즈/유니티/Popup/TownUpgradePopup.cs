using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SandboxNetwork
{
    public class TownUpgradePopup : Popup<PopupData>
    {
        [SerializeField] private Text curLevelLabel = null;
        [SerializeField] private Text nextLevelLabel = null;
        [SerializeField] private Text upgradeTimeLabel = null;
        [SerializeField] private GameObject materialContentNode = null;
        [SerializeField] private Text costTextLabel = null;
        [SerializeField] private Button OkButton = null;

        List<ItemFrame> needItemList = new List<ItemFrame>();
        TownExteriorData curExteriorData = null;
        AreaLevelData curAreaLevelData = null;
        bool isSufficientPrice = true;
        VoidDelegate constructCompleteCallback = null;

        private bool isNetworkState = false;


        public override void InitUI()
        {
            SetData();
            isNetworkState = false;
        }

        void SetData()
        {
            curExteriorData = User.Instance.ExteriorData;
            if (curExteriorData == null) { return; }
            curAreaLevelData = AreaLevelData.GetByLevel(curExteriorData.ExteriorLevel);
            if (curAreaLevelData == null) { return; }
            
            RefreshLevelLabel();
            RefreshConstructingTime();
            RefreshNeedItemList();
            RefreshCostLabel();
            RefreshOkButton();
        }

        public void SetConstructCompleteCallback( VoidDelegate callback)
        {
            constructCompleteCallback = callback;
        }

        void RefreshNeedItemList()// 외형 레벨업 필요아이템 업데이트
        {
            if (materialContentNode == null || curAreaLevelData == null) { return; }

            SBFunc.RemoveAllChildrens(materialContentNode.transform);
            needItemList.Clear();

            // 요구 재료가 없는 경우 체크
            if (curAreaLevelData.NEED_ITEM.Count <= 0) { return; }
            if (curAreaLevelData != null)
            {
                foreach (var item in curAreaLevelData.NEED_ITEM)
                {
                    GameObject itemObject = Instantiate(ResourceManager.GetResource<GameObject>(eResourcePath.PrefabClonePath, "itemPrefab"), materialContentNode.transform);
                    itemObject.transform.localScale = Vector3.one;

                    ItemFrame frameComponent = itemObject.GetComponent<ItemFrame>();
                    if (frameComponent != null)
                    {
                        frameComponent.setFrameRecipeInfo(item.ItemNo, item.Amount);
                        needItemList.Add(frameComponent);
                    }
                }
            }
        }

        bool isSufficientNeedItem()
        {
            if (needItemList == null || needItemList.Count <= 0)
                return true;

            int resultCount = 0;
            foreach (ItemFrame item in needItemList)
            {
                if (item != null)
                {
                    resultCount += item.IsSufficientAmount ? 1 : 0;
                }
            }

            return resultCount == curAreaLevelData.NEED_ITEM.Count;
        }

        void RefreshLevelLabel()
        {
            if (curLevelLabel != null)
                curLevelLabel.text = curExteriorData.ExteriorLevel.ToString();
            if (nextLevelLabel != null)
                nextLevelLabel.text = (curExteriorData.ExteriorLevel + 1).ToString();
        }

        void RefreshConstructingTime()
        {
            if (upgradeTimeLabel != null)
                upgradeTimeLabel.text = SBFunc.TimeString(curAreaLevelData.UPGRADE_TIME);
        }

        void RefreshCostLabel()
        {
            if (curAreaLevelData.NEED_GOLD > 0)
            {
                isSufficientPrice = User.Instance.GOLD >= curAreaLevelData.NEED_GOLD;
                costTextLabel.text = curAreaLevelData.NEED_GOLD.ToString();
            }
            
            if (costTextLabel != null)
                costTextLabel.color = isSufficientPrice ? Color.white : Color.red;
        }

        void RefreshOkButton()
        {
            if (OkButton != null)
                OkButton.SetButtonSpriteState(isSufficientPrice && isSufficientNeedItem());
        }
        public void OnClickUpgradeButton()
        {
            if (!isSufficientPrice || !isSufficientNeedItem())//부족할 때의 우선순위 물어보기
            {
                ToastManager.On(100002524);
                return;
            }

            WWWForm paramData = new WWWForm();
            paramData.AddField("tag", 1);

            if (isNetworkState)
            {
                return;
            }
            isNetworkState = true;
            NetworkManager.Send("building/levelup", paramData, (jsonData) =>
            {
                isNetworkState = false;
                if (SBFunc.IsJTokenCheck(jsonData["rs"]))
                {
                    switch (jsonData["rs"].Value<int>())
                    {
                        case (int)eApiResCode.OK:
                            constructCompleteCallback?.Invoke();
                            ClosePopup();

                            // 시스템 메시지 발송
                            if (User.Instance.ExteriorData.ExteriorLevel >= AreaLevelData.GetMaxLevel())
                            {
                                ChatManager.Instance.SendAchieveSystemMessage(eAchieveSystemMessageType.ACHIEVE_TOWN_MAX_LEVEL, User.Instance.UserData.UserNick);
                            }

                            // 레드닷 업데이트
                            UIManager.Instance.MainPopupUI.RequestUpdateConstructionReddot();   // 새로 건설가능한 건물

                            break;
                    }
                }
            }, (string arg) =>
            {
                isNetworkState = false;
            });
        }

        public override void ClosePopup()
        {
            SBFunc.RemoveAllChildrens(materialContentNode.transform);
            base.ClosePopup();
        }
    }
}
